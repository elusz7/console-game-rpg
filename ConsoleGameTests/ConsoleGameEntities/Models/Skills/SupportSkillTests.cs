using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Skills;

[TestClass]
public class SupportSkillTests
{
    private SupportSkill GetBasicSupportSkill()
    {
        return new SupportSkill
        {
            Name = "Inspire",
            Power = 10,
            Duration = 2,
            Cooldown = 3,
            ElapsedTime = 6,
            Cost = 0,
            RequiredLevel = 1,
            StatAffected = StatType.Attack,
            SupportEffect = (int)SupportEffectType.Boost,
            TargetType = TargetType.Self
        };
    }

    [TestMethod]
    public void Activate_SelfBoostsStat()
    {
        var skill = GetBasicSupportSkill();
        var player = new Player { Level = 2, Archetype = new Archetype { AttackBonus = 0 } };
        int original = player.GetStat(StatType.Attack);

        skill.Activate(player);

        Assert.AreEqual(original + skill.Power, player.GetStat(StatType.Attack));
        Assert.IsTrue(skill.TargetsAffected.ContainsKey(player));
    }

    [TestMethod]
    public void Activate_FailsIfOnCooldown()
    {
        var skill = GetBasicSupportSkill();

        var player = new Player { Level = 2, Archetype = new Archetype { AttackBonus = 0 } };
        skill.Activate(player); // Use it once
        Assert.ThrowsException<SkillCooldownException>(() => skill.Activate(player));
    }

    [TestMethod]
    public void UpdateElapsedTime_RemovesExpiredEffect()
    {
        var skill = GetBasicSupportSkill();
        var player = new Player { Level = 2, Archetype = new Archetype { AttackBonus = 0 } };
        skill.Activate(player);

        // Simulate turns passing
        skill.UpdateElapsedTime(); // now 1 turn left
        skill.UpdateElapsedTime(); // now 0, should expire

        Assert.IsFalse(skill.TargetsAffected.ContainsKey(player));
    }

    [TestMethod]
    public void Activate_DoesNotStackOnSameTarget()
    {
        var skill = GetBasicSupportSkill();
        var player = new Player { Level = 2, Archetype = new Archetype { AttackBonus = 0 } };

        skill.Activate(player);
        skill.ElapsedTime = 6;
        Assert.ThrowsException<InvalidTargetException>(() => skill.Activate(player));
    }

    [TestMethod]
    public void Reset_RevertsAllEffects()
    {
        var skill = GetBasicSupportSkill();
        var player = new Player { Level = 2, Archetype = new Archetype { AttackBonus = 0 } };
        skill.Activate(player);
        int boosted = player.GetStat(StatType.Attack);

        skill.Reset();

        Assert.AreEqual(boosted - skill.Power, player.GetStat(StatType.Attack));
        Assert.IsFalse(skill.TargetsAffected.ContainsKey(player));
    }
}
