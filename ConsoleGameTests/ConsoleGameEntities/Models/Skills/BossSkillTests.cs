using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Skills;

[TestClass]
public class BossSkillTests
{
    private static BossSkill GetBasicBossSkill(int power = 20, int cooldown = 2)
    {
        return new BossSkill
        {
            Name = "Inferno Slam",
            Power = power,
            Cooldown = cooldown,
            DamageType = DamageType.Magical
        };
    }

    private static Player GetTargetablePlayer(int health = 100)
    {
        return new Player
        {
            CurrentHealth = health,
            MaxHealth = health,
            Level = 1,
            Archetype = new Archetype() { ResistanceBonus = 0, DefenseBonus = 0 }
        };
    }

    private Monster GetMonsterCaster(int level = 5)
    {
        return new BossMonster
        {
            Level = level
        };
    }

    [TestMethod]
    public void InitializeSkill_SetsPhaseAndResetsCooldown()
    {
        var skill = GetBasicBossSkill();
        skill.ElapsedTime = 99;

        skill.InitializeSkill(1);

        Assert.AreEqual(1, skill.Phase);
        Assert.AreEqual(0, skill.ElapsedTime);
    }

    [TestMethod]
    public void Activate_ThrowsIfNotReady()
    {
        var skill = GetBasicBossSkill();
        skill.ElapsedTime = 1; // Not enough for cooldown of 2

        var monster = GetMonsterCaster();
        var target = GetTargetablePlayer();

        Assert.ThrowsException<SkillCooldownException>(() =>
        {
            skill.Activate(monster, target);
        });
    }

    [TestMethod]
    public void Activate_DealsBaseDamage_Phase1()
    {
        var skill = GetBasicBossSkill(power: 30);
        skill.ElapsedTime = 2;
        skill.Phase = 1;

        var monster = GetMonsterCaster();
        var target = GetTargetablePlayer(health: 100);

        skill.Activate(monster, target);

        Assert.AreEqual(70, target.CurrentHealth); // 30 damage
    }

    [TestMethod]
    public void Activate_DealsBonusDamage_Phase2()
    {
        var skill = GetBasicBossSkill(power: 30);
        skill.ElapsedTime = 2;
        skill.Phase = 2;

        var monster = GetMonsterCaster();
        var target = GetTargetablePlayer(health: 100);

        skill.Activate(monster, target);

        Assert.AreEqual(55, target.CurrentHealth); // 30 * 1.5 = 45 damage
    }

    [TestMethod]
    public void Activate_DealsBonusDamage_Phase3()
    {
        var skill = GetBasicBossSkill(power: 30);
        skill.ElapsedTime = 2;
        skill.Phase = 3;

        var monster = GetMonsterCaster();
        var target = GetTargetablePlayer(health: 100);

        skill.Activate(monster, target);

        Assert.AreEqual(40, target.CurrentHealth); // 30 * 2.0 = 60 damage
    }

    [TestMethod]
    public void Activate_AddsSkillToActionLog()
    {
        var skill = GetBasicBossSkill();
        skill.ElapsedTime = 2;

        var monster = GetMonsterCaster();
        var target = GetTargetablePlayer();

        skill.Activate(monster, target);

        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains($"uses {skill.Name}")));
    }
}
