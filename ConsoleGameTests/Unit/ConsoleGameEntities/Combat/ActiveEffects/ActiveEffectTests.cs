using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Combat.ActiveEffects;

[TestClass]
public class ActiveEffectTests
{
    private ISkill GetTestSkill()
    {
        return new Skill
        {
            Name = "TestSkill",
            Power = 10,
            Cooldown = 3,
            ElapsedTime = 6,
            Cost = 0,
            RequiredLevel = 1,
            TargetType = TargetType.Self,
            SkillCategory = SkillCategory.Basic
        };
    }

    // ---------------------------------------------------------------------------
    // A. Construction and initial state
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Constructor_SetsEffectTypeAndIdentityKey()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.AreEqual(ActiveEffectType.SupportStatModifier, effect.EffectType);
        Assert.AreEqual(key, effect.EffectIdentityKey);
    }

    [TestMethod]
    public void Constructor_SetsPower()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 25, 3);

        Assert.AreEqual(25, effect.Power);
    }

    [TestMethod]
    public void Constructor_SetsDuration()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 5);

        Assert.AreEqual(5, effect.Duration);
    }

    [TestMethod]
    public void Constructor_InitializesRemainingDurationToDuration()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 4);

        Assert.AreEqual(4, effect.RemainingDuration);
    }

    [TestMethod]
    public void Constructor_AllowsNullSourceReferences()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.IsNull(effect.SourceCaster);
        Assert.IsNull(effect.SourceSkill);
        Assert.IsNull(effect.SourceInstance);
    }

    [TestMethod]
    public void Constructor_PreservesProvidedSourceReferences()
    {
        var caster = Mock.Of<ITargetable>();
        var skill = GetTestSkill();
        var rune = Mock.Of<IRune>();
        var key = new SupportSkillEffectKey(skill);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            10,
            3,
            sourceCaster: caster,
            sourceSkill: skill,
            sourceInstance: rune);

        Assert.AreSame(caster, effect.SourceCaster);
        Assert.AreSame(skill, effect.SourceSkill);
        Assert.AreSame(rune, effect.SourceInstance);
    }
    
    [TestMethod]
    public void Constructor_RejectsInvalidEffectType()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());

        Assert.ThrowsException<ArgumentException>(
            () => new ActiveEffect((ActiveEffectType)999, key, 10, 3));
    }

    // ---------------------------------------------------------------------------
    // B. Duration validation
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Constructor_RejectsNegativeDuration()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, -1));
    }

    [TestMethod]
    public void Constructor_RejectsNegativePower()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new ActiveEffect(ActiveEffectType.SupportStatModifier, key, -1, 3));
    }

    [TestMethod]
    public void Constructor_AllowsZeroRemainingDuration()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 0);

        Assert.AreEqual(0, effect.RemainingDuration);
    }

    [TestMethod]
    public void Constructor_AllowsZeroDurationBeforeRegistration()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 0);

        Assert.AreEqual(0, effect.Duration);
        Assert.AreEqual(0, effect.RemainingDuration);
    }

    [TestMethod]
    public void Constructor_RejectsNullIdentityKey()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => new ActiveEffect(
                ActiveEffectType.SupportStatModifier,
                null!,
                10,
                3));
    }

    // ---------------------------------------------------------------------------
    // C. Effect identity
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void EffectId_IsInitiallyUnassigned()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.AreEqual(0L, effect.EffectId);
    }

    [TestMethod]
    public void EffectId_CannotBeChangedAfterAssignment()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        effect.AssignEffectId(42);
        Assert.ThrowsException<InvalidOperationException>(
            () => effect.AssignEffectId(99));
    }
    
    [TestMethod]
    public void EffectId_InvalidAssignmentDoesNotPreventValidAssignment()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => effect.AssignEffectId(-1));

        effect.AssignEffectId(42);

        Assert.AreEqual(42L, effect.EffectId);
    }

    [TestMethod]
    public void EffectId_RejectsNonPositiveValue()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => effect.AssignEffectId(0));
    }

    // ---------------------------------------------------------------------------
    // D. Identity-key compatibility
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void SupportStatModifier_RequiresSupportSkillEffectKey()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.AreEqual(ActiveEffectType.SupportStatModifier, effect.EffectType);
        Assert.IsInstanceOfType(effect.EffectIdentityKey, typeof(SupportSkillEffectKey));
    }
    
    [TestMethod]
    public void SupportSkillEffectKey_RejectsNullSourceSkill()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => new SupportSkillEffectKey(null!));
    }

    [TestMethod]
    public void ElementalStatus_RequiresElementalStatusEffectKey()
    {
        var key = new ElementalStatusEffectKey(ElementalStatusEffectType.Charred);
        var effect = new ActiveEffect(ActiveEffectType.ElementalStatus, key, 10, 3);

        Assert.AreEqual(ActiveEffectType.ElementalStatus, effect.EffectType);
        Assert.IsInstanceOfType(effect.EffectIdentityKey, typeof(ElementalStatusEffectKey));
    }

    [TestMethod]
    public void ElementalDamageOverTime_RequiresElementalDamageOverTimeEffectKey()
    {
        var key = new ElementalDamageOverTimeEffectKey(ElementType.Fire);
        var effect = new ActiveEffect(ActiveEffectType.ElementalDamageOverTime, key, 10, 3);

        Assert.AreEqual(ActiveEffectType.ElementalDamageOverTime, effect.EffectType);
        Assert.IsInstanceOfType(effect.EffectIdentityKey, typeof(ElementalDamageOverTimeEffectKey));
    }
    
    [TestMethod]
    public void ElementalDamageOverTime_DifferentElementsHaveDifferentIdentityKeys()
    {
        var fireKey = new ElementalDamageOverTimeEffectKey(ElementType.Fire);
        var iceKey = new ElementalDamageOverTimeEffectKey(ElementType.Ice);

        var fire = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime, fireKey, 10, 3);

        var ice = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime, iceKey, 10, 3);

        Assert.AreNotEqual(
            fire.EffectIdentityKey,
            ice.EffectIdentityKey);
    }
    
    [TestMethod]
    public void ElementalStatus_DifferentStatusesHaveDifferentIdentityKeys()
    {
        var charredKey = new ElementalStatusEffectKey(
            ElementalStatusEffectType.Charred);

        var shockedKey = new ElementalStatusEffectKey(
            ElementalStatusEffectType.Shocked);

        var charred = new ActiveEffect(
            ActiveEffectType.ElementalStatus, charredKey, 10, 3);

        var shocked = new ActiveEffect(
            ActiveEffectType.ElementalStatus, shockedKey, 10, 3);

        Assert.AreNotEqual(
            charred.EffectIdentityKey,
            shocked.EffectIdentityKey);
    }

    [TestMethod]
    public void Constructor_RejectsMismatchedEffectTypeAndIdentityKey()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());

        Assert.ThrowsException<ArgumentException>(
            () => new ActiveEffect(ActiveEffectType.ElementalStatus, key, 10, 3));
    }

    // ---------------------------------------------------------------------------
    // E. Source ownership
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void SourceCaster_PreservesOriginalReference()
    {
        var caster = Mock.Of<ITargetable>();
        var key = new SupportSkillEffectKey(GetTestSkill());

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            10,
            3,
            sourceCaster: caster);

        Assert.AreSame(caster, effect.SourceCaster);
    }

    [TestMethod]
    public void SourceSkill_PreservesOriginalReference()
    {
        var skill = GetTestSkill();
        var key = new SupportSkillEffectKey(skill);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            10,
            3,
            sourceSkill: skill);

        Assert.AreSame(skill, effect.SourceSkill);
    }

    [TestMethod]
    public void SourceInstance_PreservesOriginalReference()
    {
        var rune = Mock.Of<IRune>();
        var key = new SupportSkillEffectKey(GetTestSkill());

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            10,
            3,
            sourceInstance: rune);

        Assert.AreSame(rune, effect.SourceInstance);
    }

    [TestMethod]
    public void SourceReferences_AreNotRequired()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.IsNull(effect.SourceCaster);
        Assert.IsNull(effect.SourceSkill);
        Assert.IsNull(effect.SourceInstance);
    }

    // ---------------------------------------------------------------------------
    // F. Controlled mutation
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Power_CanBeUpdatedForApprovedRefresh()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        effect.Refresh(20, 5);

        Assert.AreEqual(20, effect.Power);
    }

    [TestMethod]
    public void Duration_CanBeUpdatedForApprovedRefresh()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        effect.Refresh(10, 7);

        Assert.AreEqual(7, effect.Duration);
    }

    [TestMethod]
    public void RemainingDuration_CanBeAdvancedByManager()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        effect.DecrementRemainingDuration();

        Assert.AreEqual(2, effect.RemainingDuration);
    }

    [TestMethod]
    public void SourceOwnership_RemainsUnchangedAfterMutation()
    {
        var caster = Mock.Of<ITargetable>();
        var skill = GetTestSkill();
        var key = new SupportSkillEffectKey(skill);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            10,
            3,
            sourceCaster: caster,
            sourceSkill: skill);

        effect.Refresh(20, 5);

        Assert.AreSame(caster, effect.SourceCaster);
        Assert.AreSame(skill, effect.SourceSkill);
    }

    [TestMethod]
    public void EffectIdentity_RemainsUnchangedAfterMutation()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        effect.AssignEffectId(42);
        effect.Refresh(20, 5);

        Assert.AreEqual(42L, effect.EffectId);
        Assert.AreEqual(key, effect.EffectIdentityKey);
    }
    
    [TestMethod]
    public void Refresh_PreservesOriginalSourceInstance()
    {
        var rune = Mock.Of<IRune>();
        var key = new ElementalDamageOverTimeEffectKey(ElementType.Fire);

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            10,
            3,
            sourceInstance: rune);

        effect.Refresh(20, 5);

        Assert.AreSame(rune, effect.SourceInstance);
    }

    [TestMethod]
    public void Refresh_ResetsRemainingDurationToNewDuration()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        effect.DecrementRemainingDuration();
        effect.DecrementRemainingDuration();
        effect.Refresh(15, 4);

        Assert.AreEqual(4, effect.RemainingDuration);
    }

    [TestMethod]
    public void Refresh_RejectsNegativePower()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => effect.Refresh(-1, 3));
    }

    [TestMethod]
    public void Refresh_RejectsZeroDuration()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => effect.Refresh(10, 0));
    }

    [TestMethod]
    public void Refresh_RejectsNegativeDuration()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => effect.Refresh(10, -1));
    }
    
    [TestMethod]
    public void Refresh_InvalidValuesLeaveExistingEffectUnchanged()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier, key, 10, 3);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => effect.Refresh(-1, 5));

        Assert.AreEqual(10, effect.Power);
        Assert.AreEqual(3, effect.Duration);
        Assert.AreEqual(3, effect.RemainingDuration);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => effect.Refresh(20, 0));

        Assert.AreEqual(10, effect.Power);
        Assert.AreEqual(3, effect.Duration);
        Assert.AreEqual(3, effect.RemainingDuration);
    }

    [TestMethod]
    public void DecrementRemainingDuration_ThrowsWhenExpired()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 1);

        effect.DecrementRemainingDuration(); // RemainingDuration becomes 0

        Assert.ThrowsException<InvalidOperationException>(
            () => effect.DecrementRemainingDuration());
    }

    [TestMethod]
    public void DecrementRemainingDuration_MultipleTimes()
    {
        var key = new SupportSkillEffectKey(GetTestSkill());
        var effect = new ActiveEffect(ActiveEffectType.SupportStatModifier, key, 10, 3);

        effect.DecrementRemainingDuration();
        Assert.AreEqual(2, effect.RemainingDuration);

        effect.DecrementRemainingDuration();
        Assert.AreEqual(1, effect.RemainingDuration);

        effect.DecrementRemainingDuration();
        Assert.AreEqual(0, effect.RemainingDuration);
    }
}
