using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Mono.Cecil;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Skills;

[TestClass]
public class UltimateSkillTests
{
    private UltimateSkill GetBasicUltimateSkill(int power = 40, int cooldown = 3)
    {
        return new UltimateSkill
        {
            Name = "Divine Cataclysm",
            Power = power,
            Cooldown = cooldown,
            DamageType = DamageType.Hybrid,
            TargetType = TargetType.SingleEnemy,
            RequiredLevel = 3,
            Cost = 10
        };
    }

    private Player GetPlayerCaster(int level = 5, int resource = 50)
    {
        var archetype = new Archetype { CurrentResource = resource, MaxResource = resource,
                                            DefenseBonus = 0, ResistanceBonus = 0};
        return new Player
        {
            Level = level,
            Archetype = archetype
        };
    }

    private Monster GetMonsterCaster(int level = 5)
    {
        return new Monster
        {
            Level = level
        };
    }

    private Player GetPlayerTarget(int hp = 100)
    {
        var archetype = new Archetype
        {
            DefenseBonus = 0,
            ResistanceBonus = 0
        };

        return new Player
        {
            CurrentHealth = hp,
            MaxHealth = hp,
            Archetype = archetype
        };
    }

    private Monster GetMonsterTarget(int hp = 100)
    {
        return new Monster
        {
            CurrentHealth = hp,
            MaxHealth = hp,
            DefensePower = 0,
            Resistance = 0
        };
    }

    private List<ITargetable> GetMonsterTargets(int count, int hp = 100)
    {
        return [.. Enumerable.Range(0, count).Select(_ => GetMonsterTarget(hp)).Cast<ITargetable>()];
    }


    [TestMethod]
    public void InitializeSkill_SetsPowerAndResetsCooldown()
    {
        var skill = GetBasicUltimateSkill();
        skill.ElapsedTime = 99;

        skill.InitializeSkill(10);

        Assert.AreEqual(0, skill.ElapsedTime);
        Assert.IsTrue(skill.Power > 40); // Scaled
    }

    [TestMethod]
    public void Activate_PlayerSingleTarget_DealsDamageAndConsumesResource()
    {
        var skill = GetBasicUltimateSkill();
        skill.ElapsedTime = 3;
        var player = GetPlayerCaster(level: 5);
        var target = GetMonsterTarget(100);

        skill.Activate(player, singleEnemy: target);

        Assert.IsTrue(target.CurrentHealth < 100);
        Assert.IsTrue(player.Archetype is Archetype a && a.CurrentResource < 50);
    }

    [TestMethod]
    public void Activate_MonsterSingleTarget_DealsDamage()
    {
        var skill = GetBasicUltimateSkill();
        skill.ElapsedTime = 3;
        var monster = GetMonsterCaster(level: 5);
        var target = GetPlayerTarget(100);

        skill.Activate(monster, singleEnemy: target);

        Assert.IsTrue(target.CurrentHealth < 100);
    }

    [TestMethod]
    public void Activate_PlayerAllEnemies_DealsDamageToAll()
    {
        var skill = GetBasicUltimateSkill();
        skill.TargetType = TargetType.AllEnemies;
        skill.ElapsedTime = 3;

        var player = GetPlayerCaster(level: 5);
        var enemies = GetMonsterTargets(3, 100);

        skill.Activate(player, multipleEnemies: enemies);

        foreach (var enemy in enemies.Cast<Monster>())
        {
            Assert.IsTrue(enemy.CurrentHealth < 100);
        }
    }

    [TestMethod]
    public void Activate_ThrowsIfSkillNotReady()
    {
        var skill = GetBasicUltimateSkill();
        skill.ElapsedTime = 1;
        var player = GetPlayerCaster();
        var target = GetMonsterTarget();

        Assert.ThrowsException<SkillCooldownException>(() =>
            skill.Activate(player, singleEnemy: target)
        );
    }

    [TestMethod]
    public void Activate_ThrowsIfLevelTooLow()
    {
        var skill = GetBasicUltimateSkill();
        skill.ElapsedTime = 3;

        var player = GetPlayerCaster(level: 2); // Below RequiredLevel
        var target = GetMonsterTarget();

        Assert.ThrowsException<InvalidSkillLevelException>(() =>
            skill.Activate(player, singleEnemy: target)
        );
    }

    [TestMethod]
    public void Activate_ThrowsIfNoTargets()
    {
        var skill = GetBasicUltimateSkill();
        skill.ElapsedTime = 3;
        var player = GetPlayerCaster();

        Assert.ThrowsException<InvalidTargetException>(() =>
            skill.Activate(player)
        );
    }

    [TestMethod]
    public void Activate_ThrowsIfInsufficientResource()
    {
        var skill = GetBasicUltimateSkill();
        skill.ElapsedTime = 3;
        var player = GetPlayerCaster(resource: 0);
        var target = GetMonsterTarget();

        Assert.ThrowsException<SkillResourceException>(() =>
            skill.Activate(player, singleEnemy: target)
        );
    }

    [TestMethod]
    public void Activate_AllEnemies_ThrowsMonsterDeathIfAnyDie()
    {
        var skill = GetBasicUltimateSkill(power: 200); // guaranteed kill
        skill.TargetType = TargetType.AllEnemies;
        skill.ElapsedTime = 3;

        var player = GetPlayerCaster();
        var enemies = GetMonsterTargets(2, hp: 50);

        Assert.ThrowsException<MonsterDeathException>(() =>
            skill.Activate(player, multipleEnemies: enemies)
        );
    }
}
