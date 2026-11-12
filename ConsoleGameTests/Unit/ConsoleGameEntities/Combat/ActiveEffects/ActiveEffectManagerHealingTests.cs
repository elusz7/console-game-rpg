using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Combat.ActiveEffects;

[TestClass]
public class ActiveEffectManagerHealingTests
{
    private static ActiveEffect CreateElementalStatusEffect(
        ElementalStatusEffectType statusType,
        int power,
        int duration = 3)
    {
        var key = new ElementalStatusEffectKey(statusType);
        var caster = Mock.Of<ITargetable>();
        return new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            power,
            duration,
            sourceCaster: caster);
    }

    private ActiveEffect CreateSupportStatModifierEffect(int power, int duration = 3)
    {
        var skill = new Skill
        {
            Name = "TestSupportSkill",
            Power = power,
            Cooldown = 2,
            ElapsedTime = 4,
            Cost = 0,
            RequiredLevel = 1,
            TargetType = TargetType.Self,
            SkillCategory = SkillCategory.Support
        };
        var key = new SupportSkillEffectKey(skill);
        return new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            power,
            duration,
            sourceSkill: skill);
    }

    private static ActiveEffect CreateDamageOverTimeEffect(int power, int duration = 3)
    {
        var key = new ElementalDamageOverTimeEffectKey(ElementType.Fire);
        var caster = Mock.Of<ITargetable>();
        return new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            power,
            duration,
            sourceCaster: caster);
    }

    [TestMethod]
    public void GetHealingReduction_NoActiveEffects_ReturnsZero()
    {
        var manager = new ActiveEffectManager();

        Assert.AreEqual(0, manager.GetHealingReduction());
    }
    
    [TestMethod]
    public void GetHealingReduction_FrozenEffect_ReturnsPower()
    {
        var manager = new ActiveEffectManager();

        manager.Register(CreateElementalStatusEffect(
            ElementalStatusEffectType.Frozen, power: 10));

        Assert.AreEqual(10, manager.GetHealingReduction());
    }
    
    [TestMethod]
    public void GetHealingReduction_NonFrozenStatuses_ReturnsZero()
    {
        var manager = new ActiveEffectManager();

        manager.Register(CreateElementalStatusEffect(
            ElementalStatusEffectType.Charred, power: 10));
        manager.Register(CreateElementalStatusEffect(
            ElementalStatusEffectType.Shocked, power: 15));
        manager.Register(CreateElementalStatusEffect(
            ElementalStatusEffectType.Snared, power: 20));

        Assert.AreEqual(0, manager.GetHealingReduction());
    }
    
    [TestMethod]
    public void GetHealingReduction_AfterFrozenRemoval_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var frozen = CreateElementalStatusEffect(
            ElementalStatusEffectType.Frozen, power: 10);

        manager.Register(frozen);

        manager.RemoveById(frozen.EffectId);

        Assert.AreEqual(0, manager.GetHealingReduction());
    }
    
    [TestMethod]
    public void GetHealingReduction_AfterStrongerFrozenRefresh_ReturnsUpdatedPower()
    {
        var manager = new ActiveEffectManager();

        manager.Register(CreateElementalStatusEffect(
            ElementalStatusEffectType.Frozen, power: 10));

        manager.Register(CreateElementalStatusEffect(
            ElementalStatusEffectType.Frozen, power: 20));

        Assert.AreEqual(20, manager.GetHealingReduction());
    }
    
    [TestMethod]
    public void GetHealingReduction_NonElementalEffects_ReturnsZero()
    {
        var manager = new ActiveEffectManager();

        manager.Register(CreateSupportStatModifierEffect(power: 10));
        manager.Register(CreateDamageOverTimeEffect(power: 15));

        Assert.AreEqual(0, manager.GetHealingReduction());
    }
}
