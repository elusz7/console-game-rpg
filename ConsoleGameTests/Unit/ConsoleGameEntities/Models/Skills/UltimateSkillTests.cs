using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Exceptions.Skills;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Skills;

[TestClass]
public class UltimateSkillTests
{
    private UltimateSkill GetSingleEnemyUltimateSkill(int power = 40, int cooldown = 3)
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
    
    private UltimateSkill getMultipleEnemyUltimateSkill(int power = 40, int cooldown = 3)
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
        var archetype = new Archetype
        {
            CurrentResource = resource, MaxResource = resource,
            DefenseBonus = 0, ResistanceBonus = 0
        };
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
            Level = level, DodgeChance = 0
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
            Archetype = archetype,
            DodgeChance = 0
        };
    }

    private Monster GetMonsterTarget(int hp = 100)
    {
        return new Monster
        {
            CurrentHealth = hp,
            MaxHealth = hp,
            DefensePower = 0,
            Resistance = 0, DodgeChance = 0
        };
    }

    private List<ITargetable> GetMonsterTargets(int count, int hp = 100)
    {
        return [.. Enumerable.Range(0, count).Select(_ => GetMonsterTarget(hp))];
    }


    [TestMethod]
    public void InitializeSkill_SetsPowerAndResetsCooldown()
    {
        var skill = GetSingleEnemyUltimateSkill();
        skill.ElapsedTime = 99;

        skill.InitializeSkill(10);

        Assert.AreEqual(0, skill.ElapsedTime);
        Assert.IsTrue(skill.Power > 40); // Scaled
    }

    [TestMethod]
    public void Activate_PlayerSingleTarget_DealsDamageAndConsumesResource()
    {
        var skill = GetSingleEnemyUltimateSkill();
        skill.ElapsedTime = 3;
        var player = GetPlayerCaster(level: 5);
        var target = GetMonsterTarget(100);

        skill.InitializeSkill(player.Level);
        skill.ElapsedTime = skill.Cooldown;
        
        skill.Activate(player, singleEnemy: target);

        if (target.CurrentHealth == target.MaxHealth)
        {
            Assert.IsTrue(target.ActionItems.Values.Any(a => a.Contains("dodged")));
            return;
        }

        Assert.IsTrue(target.CurrentHealth < 100);
        Assert.IsTrue(player.Archetype.CurrentResource < 50);
    }

    [TestMethod]
    public void Activate_MonsterSingleTarget_DealsDamage()
    {
        var skill = GetSingleEnemyUltimateSkill();
        skill.ElapsedTime = 3;
        var monster = GetMonsterCaster(level: 5);
        var target = GetPlayerTarget(100);
        
        skill.InitializeSkill(monster.Level);
        skill.ElapsedTime = skill.Cooldown;

        skill.Activate(monster, singleEnemy: target);

        Assert.IsTrue(target.CurrentHealth < 100);
    }

    [TestMethod]
    public void Activate_PlayerAllEnemies_DealsDamageToAll()
    {
        var skill = getMultipleEnemyUltimateSkill();
        skill.TargetType = TargetType.AllEnemies;
        skill.ElapsedTime = 3;

        var player = GetPlayerCaster(level: 5);
        var enemies = GetMonsterTargets(3, 100);
        
        skill.InitializeSkill(player.Level);
        skill.ElapsedTime = skill.Cooldown;

        skill.Activate(player, multipleEnemies: enemies);

        foreach (var enemy in enemies.Cast<Monster>())
        {
            Assert.IsTrue(enemy.CurrentHealth < 100);
        }
    }

    [TestMethod]
    public void Activate_ThrowsIfSkillNotReady()
    {
        var skill = GetSingleEnemyUltimateSkill();
        skill.ElapsedTime = 1;
        var player = GetPlayerCaster();
        var target = GetMonsterTarget();
        
        skill.InitializeSkill(player.Level);

        Assert.ThrowsException<SkillCooldownException>(() =>
            skill.Activate(player, singleEnemy: target)
        );
    }

    [TestMethod]
    public void Activate_ThrowsIfLevelTooLow()
    {
        var skill = GetSingleEnemyUltimateSkill();
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
        var skill = GetSingleEnemyUltimateSkill();
        skill.ElapsedTime = 3;
        var player = GetPlayerCaster();

        Assert.ThrowsException<InvalidTargetException>(() =>
            skill.Activate(player)
        );
    }

    [TestMethod]
    public void Activate_ThrowsIfInsufficientResource()
    {
        var skill = GetSingleEnemyUltimateSkill();
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
        var skill = getMultipleEnemyUltimateSkill(power: 200); // guaranteed kill
        skill.TargetType = TargetType.AllEnemies;
        skill.ElapsedTime = 3;

        var player = GetPlayerCaster();
        var enemies = GetMonsterTargets(2, hp: 50);
        
        skill.InitializeSkill(player.Level);
        skill.ElapsedTime = skill.Cooldown;

        var results = skill.Activate(player, null, enemies);

        Assert.AreEqual(2, results.AttackResults.Count(ar => ar.TargetDied));
    }
}