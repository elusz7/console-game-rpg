using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Combat.ActiveEffects;

[TestClass]
public class ActiveEffectManagerDamageModifierTests
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

    // ---------------------------------------------------------------------------
    // GetIncomingDamageIncrease() — Blinded
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetIncomingDamageIncrease_NoActiveEffects_ReturnsZero()
    {
        var manager = new ActiveEffectManager();

        var increase = manager.GetIncomingDamageIncrease();

        Assert.AreEqual(0, increase);
    }

    [TestMethod]
    public void GetIncomingDamageIncrease_BlindedActive_ReturnsPower()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Blinded, 5);
        manager.Register(effect);

        var increase = manager.GetIncomingDamageIncrease();

        Assert.AreEqual(5, increase);
    }

    [TestMethod]
    public void GetIncomingDamageIncrease_NoBlindedEffect_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var charred = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        var shocked = CreateElementalStatusEffect(ElementalStatusEffectType.Shocked, 4);
        var frozen = CreateElementalStatusEffect(ElementalStatusEffectType.Frozen, 3);
        manager.Register(charred);
        manager.Register(shocked);
        manager.Register(frozen);

        var increase = manager.GetIncomingDamageIncrease();

        Assert.AreEqual(0, increase);
    }

    [TestMethod]
    public void GetIncomingDamageIncrease_CorruptedEffect_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Corrupted, 5);
        manager.Register(effect);

        var increase = manager.GetIncomingDamageIncrease();

        Assert.AreEqual(0, increase);
    }

    [TestMethod]
    public void GetIncomingDamageIncrease_OtherEffectTypes_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var supportEffect = CreateSupportStatModifierEffect(10);
        var dotEffect = CreateDamageOverTimeEffect(8);
        manager.Register(supportEffect);
        manager.Register(dotEffect);

        var increase = manager.GetIncomingDamageIncrease();

        Assert.AreEqual(0, increase);
    }

    [TestMethod]
    public void GetIncomingDamageIncrease_AfterBlindedRemoval_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Blinded, 5);
        var result = manager.Register(effect);
        var effectId = result.Effect.EffectId;

        Assert.AreEqual(5, manager.GetIncomingDamageIncrease());

        manager.RemoveById(effectId);

        Assert.AreEqual(0, manager.GetIncomingDamageIncrease());
    }

    [TestMethod]
    public void GetIncomingDamageIncrease_AfterClearAll_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var blinded = CreateElementalStatusEffect(ElementalStatusEffectType.Blinded, 5);
        var charred = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 3);
        manager.Register(blinded);
        manager.Register(charred);

        manager.ClearAll();

        Assert.AreEqual(0, manager.GetIncomingDamageIncrease());
    }

    [TestMethod]
    public void GetIncomingDamageIncrease_AfterStrongerBlindedRefresh_ReturnsUpdatedPower()
    {
        var manager = new ActiveEffectManager();

        var original = CreateElementalStatusEffect(ElementalStatusEffectType.Blinded, 3);
        manager.Register(original);

        var stronger = CreateElementalStatusEffect(ElementalStatusEffectType.Blinded, 7);
        manager.Register(stronger);

        var increase = manager.GetIncomingDamageIncrease();

        Assert.AreEqual(7, increase);
    }

    [TestMethod]
    public void GetIncomingDamageIncrease_AfterWeakerBlindedRegistration_ReturnsExistingPower()
    {
        var manager = new ActiveEffectManager();

        var stronger = CreateElementalStatusEffect(ElementalStatusEffectType.Blinded, 7);
        manager.Register(stronger);

        var weaker = CreateElementalStatusEffect(ElementalStatusEffectType.Blinded, 3);
        manager.Register(weaker);

        var increase = manager.GetIncomingDamageIncrease();

        Assert.AreEqual(7, increase);
    }

    // ---------------------------------------------------------------------------
    // GetOutgoingDamageDecrease() — Corrupted
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetOutgoingDamageDecrease_NoActiveEffects_ReturnsZero()
    {
        var manager = new ActiveEffectManager();

        var decrease = manager.GetOutgoingDamageDecrease();

        Assert.AreEqual(0, decrease);
    }

    [TestMethod]
    public void GetOutgoingDamageDecrease_CorruptedActive_ReturnsPower()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Corrupted, 5);
        manager.Register(effect);

        var decrease = manager.GetOutgoingDamageDecrease();

        Assert.AreEqual(5, decrease);
    }

    [TestMethod]
    public void GetOutgoingDamageDecrease_NoCorruptedEffect_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var charred = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 5);
        var shocked = CreateElementalStatusEffect(ElementalStatusEffectType.Shocked, 4);
        var frozen = CreateElementalStatusEffect(ElementalStatusEffectType.Frozen, 3);
        manager.Register(charred);
        manager.Register(shocked);
        manager.Register(frozen);

        var decrease = manager.GetOutgoingDamageDecrease();

        Assert.AreEqual(0, decrease);
    }

    [TestMethod]
    public void GetOutgoingDamageDecrease_BlindedEffect_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Blinded, 5);
        manager.Register(effect);

        var decrease = manager.GetOutgoingDamageDecrease();

        Assert.AreEqual(0, decrease);
    }

    [TestMethod]
    public void GetOutgoingDamageDecrease_OtherEffectTypes_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var supportEffect = CreateSupportStatModifierEffect(10);
        var dotEffect = CreateDamageOverTimeEffect(8);
        manager.Register(supportEffect);
        manager.Register(dotEffect);

        var decrease = manager.GetOutgoingDamageDecrease();

        Assert.AreEqual(0, decrease);
    }

    [TestMethod]
    public void GetOutgoingDamageDecrease_AfterCorruptedRemoval_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var effect = CreateElementalStatusEffect(ElementalStatusEffectType.Corrupted, 5);
        var result = manager.Register(effect);
        var effectId = result.Effect.EffectId;

        Assert.AreEqual(5, manager.GetOutgoingDamageDecrease());

        manager.RemoveById(effectId);

        Assert.AreEqual(0, manager.GetOutgoingDamageDecrease());
    }

    [TestMethod]
    public void GetOutgoingDamageDecrease_AfterClearAll_ReturnsZero()
    {
        var manager = new ActiveEffectManager();
        var corrupted = CreateElementalStatusEffect(ElementalStatusEffectType.Corrupted, 5);
        var charred = CreateElementalStatusEffect(ElementalStatusEffectType.Charred, 3);
        manager.Register(corrupted);
        manager.Register(charred);

        manager.ClearAll();

        Assert.AreEqual(0, manager.GetOutgoingDamageDecrease());
    }

    [TestMethod]
    public void GetOutgoingDamageDecrease_AfterStrongerCorruptedRefresh_ReturnsUpdatedPower()
    {
        var manager = new ActiveEffectManager();

        var original = CreateElementalStatusEffect(ElementalStatusEffectType.Corrupted, 3);
        manager.Register(original);

        var stronger = CreateElementalStatusEffect(ElementalStatusEffectType.Corrupted, 7);
        manager.Register(stronger);

        var decrease = manager.GetOutgoingDamageDecrease();

        Assert.AreEqual(7, decrease);
    }

    [TestMethod]
    public void GetOutgoingDamageDecrease_AfterWeakerCorruptedRegistration_ReturnsExistingPower()
    {
        var manager = new ActiveEffectManager();

        var stronger = CreateElementalStatusEffect(ElementalStatusEffectType.Corrupted, 7);
        manager.Register(stronger);

        var weaker = CreateElementalStatusEffect(ElementalStatusEffectType.Corrupted, 3);
        manager.Register(weaker);

        var decrease = manager.GetOutgoingDamageDecrease();

        Assert.AreEqual(7, decrease);
    }
}
