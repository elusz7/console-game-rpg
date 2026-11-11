using ConsoleGame.Services;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil.Cil;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Combat.ActiveEffects;

[TestClass]
public class ActiveEffectManagerTests
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

    private ISkill GetTestSkill2()
    {
        return new Skill
        {
            Name = "TestSkill2",
            Power = 15,
            Cooldown = 2,
            ElapsedTime = 4,
            Cost = 0,
            RequiredLevel = 1,
            TargetType = TargetType.Self,
            SkillCategory = SkillCategory.Basic
        };
    }

    // ---------------------------------------------------------------------------
    // A. New effect registration
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_NewEffect_AssignsUniqueIncreasingId()
    {
        var manager = new ActiveEffectManager();

        var skill1 = GetTestSkill();
        var skill2 = GetTestSkill2();

        var effect1 = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill1),
            10,
            3,
            sourceSkill: skill1);

        var effect2 = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill2),
            15,
            5,
            sourceSkill: skill2);

        var result1 = manager.Register(effect1);
        var result2 = manager.Register(effect2);

        Assert.AreEqual(RegistrationStatus.Registered, result1.Registration);
        Assert.AreEqual(RegistrationStatus.Registered, result2.Registration);

        Assert.AreEqual(1L, effect1.EffectId);
        Assert.AreEqual(2L, effect2.EffectId);

        Assert.AreEqual(2, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // B. Duplicate status handling
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_DuplicateStatus_PreservesExistingEffectIdAndSource()
    {
        var manager = new ActiveEffectManager();

        var key = new ElementalStatusEffectKey(ElementalStatusEffectType.Charred);
        var caster = Mock.Of<ITargetable>();
        var rune = Mock.Of<IRune>();

        var existingEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            10,
            3,
            sourceCaster: caster,
            sourceInstance: rune);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;

        var newEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            15,
            5);

        var updateResult = manager.Register(newEffect);

        Assert.AreEqual(RegistrationStatus.Refreshed, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreSame(caster, existingEffect.SourceCaster);
        Assert.AreSame(rune, existingEffect.SourceInstance);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // C. DoT update - stronger
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_StrongerDoT_PreservesExistingEffectIdAndSource()
    {
        var manager = new ActiveEffectManager();

        var key = new ElementalDamageOverTimeEffectKey(ElementType.Fire);
        var rune = Mock.Of<IRune>();

        var existingEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            10,
            3,
            sourceInstance: rune);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;

        var strongerEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            20,
            5);

        var updateResult = manager.Register(strongerEffect);

        Assert.AreEqual(RegistrationStatus.Refreshed, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreSame(rune, existingEffect.SourceInstance);
        Assert.AreEqual(20, existingEffect.Power);
        Assert.AreEqual(5, existingEffect.Duration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.AreEqual(5, existingEffect.RemainingDuration);
    }

    // ---------------------------------------------------------------------------
    // D. Weaker status - retained
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_WeakerStatus_RetainsExistingEffectUnchanged()
    {
        var manager = new ActiveEffectManager();

        var key = new ElementalStatusEffectKey(ElementalStatusEffectType.Charred);

        var existingEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            20,
            5);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;
        var originalPower = existingEffect.Power;
        var originalDuration = existingEffect.Duration;
        var originalRemainingDuration = existingEffect.RemainingDuration;

        var weakerEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            10,
            3);

        var updateResult = manager.Register(weakerEffect);

        Assert.AreEqual(RegistrationStatus.Retained, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreEqual(originalPower, existingEffect.Power);
        Assert.AreEqual(originalDuration, existingEffect.Duration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.AreEqual(originalRemainingDuration, existingEffect.RemainingDuration);
    }

    // ---------------------------------------------------------------------------
    // E. Weaker DoT - rejected
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_WeakerDoT_RejectsWithoutChangingExistingEffect()
    {
        var manager = new ActiveEffectManager();

        var key = new ElementalDamageOverTimeEffectKey(ElementType.Fire);

        var existingEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            20,
            5);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;
        var originalPower = existingEffect.Power;
        var originalDuration = existingEffect.Duration;
        var originalRemainingDuration = existingEffect.RemainingDuration;

        var weakerEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            10,
            3);

        var updateResult = manager.Register(weakerEffect);

        Assert.AreEqual(RegistrationStatus.Rejected, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreEqual(originalPower, existingEffect.Power);
        Assert.AreEqual(originalDuration, existingEffect.Duration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.AreEqual(originalRemainingDuration, existingEffect.RemainingDuration);
    }

    // ---------------------------------------------------------------------------
    // F. Duplicate support skill - rejected
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_DuplicateSupportSkill_RejectsWithoutChangingExistingEffect()
    {
        var manager = new ActiveEffectManager();

        var skill = GetTestSkill();
        var key = new SupportSkillEffectKey(skill);

        var existingEffect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            15,
            5,
            sourceSkill: skill);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;
        var originalPower = existingEffect.Power;
        var originalDuration = existingEffect.Duration;

        var duplicateEffect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            25,
            10,
            sourceSkill: skill);

        var updateResult = manager.Register(duplicateEffect);

        Assert.AreSame(skill, existingEffect.SourceSkill);
        Assert.AreEqual(RegistrationStatus.Rejected, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreEqual(originalPower, existingEffect.Power);
        Assert.AreEqual(originalDuration, existingEffect.Duration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // G. Same instance registered twice
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_SameInstanceTwice_DoesNotChangeExistingEffect()
    {
        var manager = new ActiveEffectManager();

        var skill = GetTestSkill();
        var key = new SupportSkillEffectKey(skill);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            10,
            3,
            sourceSkill: skill);

        var result1 = manager.Register(effect);
        Assert.AreEqual(RegistrationStatus.Registered, result1.Registration);
        var originalId = effect.EffectId;
        var originalPower = effect.Power;
        var originalDuration = effect.Duration;

        var result2 = manager.Register(effect);

        Assert.AreEqual(RegistrationStatus.Retained, result2.Registration);
        Assert.AreEqual(originalId, effect.EffectId);
        Assert.AreEqual(originalPower, effect.Power);
        Assert.AreEqual(originalDuration, effect.Duration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // H. Invalid duration throws
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_InvalidDuration_Throws()
    {
        var manager = new ActiveEffectManager();

        var skill = GetTestSkill();
        var key = new SupportSkillEffectKey(skill);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            10,
            0,
            sourceSkill: skill);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => manager.Register(effect));

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // I. Null effect throws
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_NullEffect_Throws()
    {
        var manager = new ActiveEffectManager();

        Assert.ThrowsException<ArgumentNullException>(
            () => manager.Register(null!));

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // J. Already registered effect with different manager throws
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_AlreadyRegisteredEffectWithDifferentManager_Throws()
    {
        var manager1 = new ActiveEffectManager();
        var manager2 = new ActiveEffectManager();

        var skill = GetTestSkill();
        var key = new SupportSkillEffectKey(skill);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            key,
            10,
            3,
            sourceSkill: skill);

        var result1 = manager1.Register(effect);
        Assert.AreEqual(RegistrationStatus.Registered, result1.Registration);
        Assert.IsTrue(effect.EffectId > 0);

        Assert.ThrowsException<InvalidOperationException>(
            () => manager2.Register(effect));

        Assert.AreEqual(0, manager2.ActiveEffects.Count);
        Assert.AreEqual(1, manager1.ActiveEffects.Count);
        Assert.AreSame(effect, manager1.ActiveEffects[0]);
        Assert.AreEqual(1L, effect.EffectId);
    }

    // ---------------------------------------------------------------------------
    // K. Equal status reapplication: Refresh Duration, retain Power
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_EqualStatus_Reapplication_RefreshesDuration_RetainsPower()
    {
        var manager = new ActiveEffectManager();

        var key = new ElementalStatusEffectKey(ElementalStatusEffectType.Charred);

        var existingEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            10,
            5);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;

        // Decrement to simulate time passing
        existingEffect.DecrementRemainingDuration();
        existingEffect.DecrementRemainingDuration();
        Assert.AreEqual(3, existingEffect.RemainingDuration);

        // Reapply with equal power but different duration
        var reapplyEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            10,
            8);

        var updateResult = manager.Register(reapplyEffect);

        Assert.AreEqual(RegistrationStatus.Refreshed, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreEqual(10, existingEffect.Power);
        Assert.AreEqual(8, existingEffect.Duration);
        Assert.AreEqual(8, existingEffect.RemainingDuration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // L. Equal DoT reapplication: Refresh Duration, retain Power
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_EqualDoT_Reapplication_RefreshesDuration_RetainsPower()
    {
        var manager = new ActiveEffectManager();

        var key = new ElementalDamageOverTimeEffectKey(ElementType.Fire);

        var existingEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            10,
            5);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;

        // Decrement to simulate time passing
        existingEffect.DecrementRemainingDuration();
        existingEffect.DecrementRemainingDuration();
        Assert.AreEqual(3, existingEffect.RemainingDuration);

        // Reapply with equal power but different duration
        var reapplyEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            10,
            8);

        var updateResult = manager.Register(reapplyEffect);

        Assert.AreEqual(RegistrationStatus.Refreshed, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreEqual(10, existingEffect.Power);
        Assert.AreEqual(8, existingEffect.Duration);
        Assert.AreEqual(8, existingEffect.RemainingDuration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // M. Stronger status reapplication: Refresh Power and Duration
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_StrongerStatus_Reapplication_RefreshesPowerAndDuration()
    {
        var manager = new ActiveEffectManager();

        var key = new ElementalStatusEffectKey(ElementalStatusEffectType.Charred);

        var existingEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            10,
            5);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;

        // Reapply with stronger power and different duration
        var strongerEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            20,
            8);

        var updateResult = manager.Register(strongerEffect);

        Assert.AreEqual(RegistrationStatus.Refreshed, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreEqual(20, existingEffect.Power);
        Assert.AreEqual(8, existingEffect.Duration);
        Assert.AreEqual(8, existingEffect.RemainingDuration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // N. Different status types: Both register
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_DifferentStatusTypes_BothRegister()
    {
        var manager = new ActiveEffectManager();

        var charredKey = new ElementalStatusEffectKey(ElementalStatusEffectType.Charred);
        var shockedKey = new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked);

        var charredEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            charredKey,
            10,
            5);

        var shockedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            shockedKey,
            15,
            3);

        var result1 = manager.Register(charredEffect);
        var result2 = manager.Register(shockedEffect);

        Assert.AreEqual(RegistrationStatus.Registered, result1.Registration);
        Assert.AreEqual(RegistrationStatus.Registered, result2.Registration);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.AreEqual(1L, charredEffect.EffectId);
        Assert.AreEqual(2L, shockedEffect.EffectId);
    }

    // ---------------------------------------------------------------------------
    // O. Different DoT elements: Both register
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_DifferentDoTElements_BothRegister()
    {
        var manager = new ActiveEffectManager();

        var fireKey = new ElementalDamageOverTimeEffectKey(ElementType.Fire);
        var iceKey = new ElementalDamageOverTimeEffectKey(ElementType.Ice);

        var fireEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            fireKey,
            10,
            5);

        var iceEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            iceKey,
            15,
            3);

        var result1 = manager.Register(fireEffect);
        var result2 = manager.Register(iceEffect);

        Assert.AreEqual(RegistrationStatus.Registered, result1.Registration);
        Assert.AreEqual(RegistrationStatus.Registered, result2.Registration);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.AreEqual(1L, fireEffect.EffectId);
        Assert.AreEqual(2L, iceEffect.EffectId);
    }

    // ---------------------------------------------------------------------------
    // P. Different support skill instances: Both register
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_DifferentSupportSkillInstances_BothRegister()
    {
        var manager = new ActiveEffectManager();

        var skill1 = GetTestSkill();
        var skill2 = GetTestSkill2();

        var effect1 = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill1),
            10,
            5,
            sourceSkill: skill1);

        var effect2 = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill2),
            15,
            3,
            sourceSkill: skill2);

        var result1 = manager.Register(effect1);
        var result2 = manager.Register(effect2);

        Assert.AreEqual(RegistrationStatus.Registered, result1.Registration);
        Assert.AreEqual(RegistrationStatus.Registered, result2.Registration);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.AreEqual(1L, effect1.EffectId);
        Assert.AreEqual(2L, effect2.EffectId);
    }

    // ---------------------------------------------------------------------------
    // Q. Support skill identity uses reference equality
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Register_SupportSkillIdentity_UsesReferenceEquality()
    {
        var manager = new ActiveEffectManager();

        // Two distinct skill instances with identical properties
        var skillA = new Skill
        {
            Name = "IdenticalSkill",
            Power = 10,
            Cooldown = 3,
            ElapsedTime = 6,
            Cost = 0,
            RequiredLevel = 1,
            TargetType = TargetType.Self,
            SkillCategory = SkillCategory.Basic
        };

        var skillB = new Skill
        {
            Name = "IdenticalSkill",
            Power = 10,
            Cooldown = 3,
            ElapsedTime = 6,
            Cost = 0,
            RequiredLevel = 1,
            TargetType = TargetType.Self,
            SkillCategory = SkillCategory.Basic
        };

        // Distinct instances should not match by reference
        Assert.IsFalse(ReferenceEquals(skillA, skillB));

        var effectA = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skillA),
            10,
            5,
            sourceSkill: skillA);

        var effectB = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skillB),
            20,
            8,
            sourceSkill: skillB);

        var result1 = manager.Register(effectA);
        var result2 = manager.Register(effectB);

        // Both should register since they're distinct references
        Assert.AreEqual(RegistrationStatus.Registered, result1.Registration);
        Assert.AreEqual(RegistrationStatus.Registered, result2.Registration);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.AreNotEqual(
            new SupportSkillEffectKey(skillA),
            new SupportSkillEffectKey(skillB));
    }

    // ---------------------------------------------------------------------------
    // R. Lookup and removal methods
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void LookupAndRemoval_Methods_ReturnCorrectEffects()
    {
        var manager = new ActiveEffectManager();

        var skill1 = GetTestSkill();
        var skill2 = GetTestSkill2();
        var rune = Mock.Of<IRune>();

        var supportEffect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill1),
            10,
            5,
            sourceSkill: skill1);

        var statusEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            15,
            3,
            sourceInstance: rune);

        var dotEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            20,
            4,
            sourceInstance: rune);

        manager.Register(supportEffect);
        manager.Register(statusEffect);
        manager.Register(dotEffect);

        Assert.AreEqual(3, manager.ActiveEffects.Count);

        // GetById
        var foundById = manager.GetById(2L);
        Assert.IsNotNull(foundById);
        Assert.AreSame(statusEffect, foundById);

        var notFoundById = manager.GetById(999L);
        Assert.IsNull(notFoundById);

        // GetByIdentityKey
        var charredKey = new ElementalStatusEffectKey(ElementalStatusEffectType.Charred);
        var foundByKey = manager.GetByIdentityKey(charredKey);
        Assert.IsNotNull(foundByKey);
        Assert.AreSame(statusEffect, foundByKey);

        var notFoundByKey = manager.GetByIdentityKey(
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked));
        Assert.IsNull(notFoundByKey);

        // GetByType
        var statusEffects = manager.GetByType(ActiveEffectType.ElementalStatus);
        Assert.AreEqual(1, statusEffects.Count);
        Assert.AreSame(statusEffect, statusEffects[0]);

        var dotEffects = manager.GetByType(ActiveEffectType.ElementalDamageOverTime);
        Assert.AreEqual(1, dotEffects.Count);
        Assert.AreSame(dotEffect, dotEffects[0]);

        var supportEffects = manager.GetByType(ActiveEffectType.SupportStatModifier);
        Assert.AreEqual(1, supportEffects.Count);
        Assert.AreSame(supportEffect, supportEffects[0]);

        // GetBySourceSkill
        var bySkill1 = manager.GetBySourceSkill(skill1);
        Assert.AreEqual(1, bySkill1.Count);
        Assert.AreSame(supportEffect, bySkill1[0]);

        var bySkill2 = manager.GetBySourceSkill(skill2);
        Assert.AreEqual(0, bySkill2.Count);

        // RemoveById
        var removed = manager.RemoveById(2L);
        Assert.IsTrue(removed);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.IsNull(manager.GetById(2L));

        // RemoveById with non-existent id
        var notRemoved = manager.RemoveById(999L);
        Assert.IsFalse(notRemoved);
        Assert.AreEqual(2, manager.ActiveEffects.Count);

        // RemoveByIdentityKey
        var fireKey = new ElementalDamageOverTimeEffectKey(ElementType.Fire);
        var removedByKey = manager.RemoveByIdentityKey(fireKey);
        Assert.IsTrue(removedByKey);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.IsNull(manager.GetByIdentityKey(fireKey));

        // RemoveByIdentityKey with non-existent key
        var notRemovedByKey = manager.RemoveByIdentityKey(
            new ElementalDamageOverTimeEffectKey(ElementType.Ice));
        Assert.IsFalse(notRemovedByKey);
        Assert.AreEqual(1, manager.ActiveEffects.Count);

        // Verify remaining effect
        Assert.AreEqual(1L, supportEffect.EffectId);
        Assert.AreSame(supportEffect, manager.ActiveEffects[0]);
    }
    
    // ---------------------------------------------------------------------------
    // S. Argument validation
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetByIdentityKey_NullKey_Throws()
    {
        var manager = new ActiveEffectManager();

        Assert.ThrowsException<ArgumentNullException>(
            () => manager.GetByIdentityKey(null!));

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void GetBySourceSkill_NullSource_Throws()
    {
        var manager = new ActiveEffectManager();

        Assert.ThrowsException<ArgumentNullException>(
            () => manager.GetBySourceSkill(null!));

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void MismatchedIdentityKeyAndEffectType_Throws()
    {
        var manager = new ActiveEffectManager();

        Assert.ThrowsException<ArgumentException>(
            () => new ActiveEffect(
                ActiveEffectType.ElementalStatus,
                new ElementalDamageOverTimeEffectKey(ElementType.Fire),
                10,
                3));

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }
}
