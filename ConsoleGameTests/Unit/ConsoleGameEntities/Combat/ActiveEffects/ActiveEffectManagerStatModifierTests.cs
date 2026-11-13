using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Combat.ActiveEffects;

[TestClass]
public class ActiveEffectManagerStatModifierTests
{
    // ---------------------------------------------------------------------------
    // Test setup and helpers
    // ---------------------------------------------------------------------------

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
        var skill = new SupportSkill
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

    // ---------------------------------------------------------------------------
    // Default and unsupported stat tests
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetStatModifier_NoActiveEffects_ReturnsZero()
    {
        var manager = new ActiveEffectManager();

        var modifier = manager.GetStatModifier(StatType.Defense);

        Assert.AreEqual(0, modifier);
    }

    [TestMethod]
    public void GetStatModifier_UnsupportedStat_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        manager.Register(effect);

        var attackModifier = manager.GetStatModifier(StatType.Attack);
        var magicModifier = manager.GetStatModifier(StatType.Magic);

        Assert.AreEqual(0, attackModifier);
        Assert.AreEqual(0, magicModifier);
    }

    [TestMethod]
    public void GetStatModifier_Health_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        manager.Register(effect);

        var modifier = manager.GetStatModifier(StatType.Health);

        Assert.AreEqual(0, modifier);
    }

    [TestMethod]
    public void GetStatModifier_NoMatchingStatus_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        manager.Register(effect);

        var resistanceModifier = manager.GetStatModifier(StatType.Resistance);
        var speedModifier = manager.GetStatModifier(StatType.Speed);

        Assert.AreEqual(0, resistanceModifier);
        Assert.AreEqual(0, speedModifier);
    }

    // ---------------------------------------------------------------------------
    // Individual status mapping tests
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetStatModifier_Charred_ReducesDefense()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        manager.Register(effect);

        var modifier = manager.GetStatModifier(StatType.Defense);

        Assert.AreEqual(-5, modifier);
    }

    [TestMethod]
    public void GetStatModifier_Shocked_ReducesResistance()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Shocked, 4);
        manager.Register(effect);

        var modifier = manager.GetStatModifier(StatType.Resistance);

        Assert.AreEqual(-4, modifier);
    }

    [TestMethod]
    public void GetStatModifier_Snared_ReducesSpeed()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Snared, 3);
        manager.Register(effect);

        var modifier = manager.GetStatModifier(StatType.Speed);

        Assert.AreEqual(-3, modifier);
    }

    // ---------------------------------------------------------------------------
    // Status isolation tests
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetStatModifier_Charred_DoesNotAffectOtherStats()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        manager.Register(effect);

        var resistanceModifier = manager.GetStatModifier(StatType.Resistance);
        var speedModifier = manager.GetStatModifier(StatType.Speed);

        Assert.AreEqual(0, resistanceModifier);
        Assert.AreEqual(0, speedModifier);
    }

    [TestMethod]
    public void GetStatModifier_Shocked_DoesNotAffectOtherStats()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Shocked, 5);
        manager.Register(effect);

        var defenseModifier = manager.GetStatModifier(StatType.Defense);
        var speedModifier = manager.GetStatModifier(StatType.Speed);

        Assert.AreEqual(0, defenseModifier);
        Assert.AreEqual(0, speedModifier);
    }

    [TestMethod]
    public void GetStatModifier_Snared_DoesNotAffectOtherStats()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Snared, 5);
        manager.Register(effect);

        var defenseModifier = manager.GetStatModifier(StatType.Defense);
        var resistanceModifier = manager.GetStatModifier(StatType.Resistance);

        Assert.AreEqual(0, defenseModifier);
        Assert.AreEqual(0, resistanceModifier);
    }

    // ---------------------------------------------------------------------------
    // Aggregation and coexistence tests
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetStatModifier_DifferentStatuses_OnlyIncludesMatchingStat()
    {
        var manager = new ActiveEffectManager();
        var charred = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        var shocked = CreateElementalStatusEffect(ElementalStatusEffectType.Shocked, 4);
        var snared = CreateElementalStatusEffect(ElementalStatusEffectType.Snared, 3);
        manager.Register(charred);
        manager.Register(shocked);
        manager.Register(snared);

        var defenseModifier = manager.GetStatModifier(StatType.Defense);
        var resistanceModifier = manager.GetStatModifier(StatType.Resistance);
        var speedModifier = manager.GetStatModifier(StatType.Speed);

        Assert.AreEqual(-5, defenseModifier);
        Assert.AreEqual(-4, resistanceModifier);
        Assert.AreEqual(-3, speedModifier);
    }

    [TestMethod]
    public void GetStatModifier_UnrelatedEffectTypes_AreIgnored()
    {
        var manager = new ActiveEffectManager();
        var supportEffect = CreateSupportStatModifierEffect(10);
        var dotEffect = CreateDamageOverTimeEffect(8);
        manager.Register(supportEffect);
        manager.Register(dotEffect);

        var defenseModifier = manager.GetStatModifier(StatType.Defense);
        var resistanceModifier = manager.GetStatModifier(StatType.Resistance);
        var speedModifier = manager.GetStatModifier(StatType.Speed);

        Assert.AreEqual(0, defenseModifier);
        Assert.AreEqual(0, resistanceModifier);
        Assert.AreEqual(0, speedModifier);
    }

    // ---------------------------------------------------------------------------
    // Removal and dynamic recalculation tests
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetStatModifier_AfterRemovingEffect_UpdatesModifier()
    {
        var manager = new ActiveEffectManager();
        var charred = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        var result = manager.Register(charred);
        var effectId = result.Effect.EffectId;

        Assert.AreEqual(-5, manager.GetStatModifier(StatType.Defense));

        manager.RemoveById(effectId);

        Assert.AreEqual(0, manager.GetStatModifier(StatType.Defense));
    }

    [TestMethod]
    public void GetStatModifier_AfterRemovingOneOfMultipleEffects_UpdatesModifier()
    {
        var manager = new ActiveEffectManager();
        var charred = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        var shocked = CreateElementalStatusEffect(ElementalStatusEffectType.Shocked, 4);
        var charredResult = manager.Register(charred);
        manager.Register(shocked);
        var charredId = charredResult.Effect.EffectId;

        manager.RemoveById(charredId);

        var defenseModifier = manager.GetStatModifier(StatType.Defense);
        var resistanceModifier = manager.GetStatModifier(StatType.Resistance);

        Assert.AreEqual(0, defenseModifier);
        Assert.AreEqual(-4, resistanceModifier);
    }

    [TestMethod]
    public void GetStatModifier_AfterClearAll_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var charred = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        var shocked = CreateElementalStatusEffect(ElementalStatusEffectType.Shocked, 4);
        var snared = CreateElementalStatusEffect(ElementalStatusEffectType.Snared, 3);
        manager.Register(charred);
        manager.Register(shocked);
        manager.Register(snared);

        manager.ClearAll();

        var defenseModifier = manager.GetStatModifier(StatType.Defense);
        var resistanceModifier = manager.GetStatModifier(StatType.Resistance);
        var speedModifier = manager.GetStatModifier(StatType.Speed);

        Assert.AreEqual(0, defenseModifier);
        Assert.AreEqual(0, resistanceModifier);
        Assert.AreEqual(0, speedModifier);
    }
    
    //stacking rule preserved
    [TestMethod]
    public void GetStatModifier_AfterStrongerStatusRefresh_UsesUpdatedPower()
    {
        var manager = new ActiveEffectManager();

        var original = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 3);
        manager.Register(original);

        var stronger = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 7);
        manager.Register(stronger);

        var modifier = manager.GetStatModifier(StatType.Defense);

        Assert.AreEqual(-7, modifier);
    }
}
