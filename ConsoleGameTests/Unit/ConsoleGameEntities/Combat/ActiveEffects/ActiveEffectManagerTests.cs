using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Skills;
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

        Assert.AreSame(effect1, result1.Effect);
        Assert.AreSame(effect2, result2.Effect);
        
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
            sourceRune: rune);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;

        var newEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            key,
            15,
            5);

        var updateResult = manager.Register(newEffect);

        Assert.AreEqual(RegistrationStatus.Strengthened, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreSame(caster, existingEffect.SourceCaster);
        Assert.AreSame(rune, existingEffect.SourceRune);
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
        var caster = Mock.Of<ITargetable>();

        var existingEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            10,
            3,
            sourceCaster: caster,
            sourceRune: rune);

        var result = manager.Register(existingEffect);
        Assert.AreEqual(RegistrationStatus.Registered, result.Registration);
        var originalId = existingEffect.EffectId;

        var incomingCaster = Mock.Of<ITargetable>();
        var incomingRune = Mock.Of<IRune>();

        var strongerEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            20,
            5,
            sourceCaster: incomingCaster,
            sourceRune: incomingRune);

        var updateResult = manager.Register(strongerEffect);

        Assert.AreSame(existingEffect, updateResult.Effect);
        Assert.AreEqual(RegistrationStatus.Strengthened, updateResult.Registration);
        Assert.AreEqual(originalId, existingEffect.EffectId);
        Assert.AreSame(caster, existingEffect.SourceCaster);
        Assert.AreSame(rune, existingEffect.SourceRune);
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

        Assert.AreEqual(RegistrationStatus.RejectedWeaker, updateResult.Registration);
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

        Assert.AreSame(existingEffect, updateResult.Effect);
        Assert.AreSame(skill, existingEffect.SourceSkill);
        Assert.AreEqual(RegistrationStatus.RejectedDuplicate, updateResult.Registration);
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

        Assert.AreEqual(RegistrationStatus.Strengthened, updateResult.Registration);
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
        Assert.AreEqual(
            new SupportSkillEffectKey(skillA),
            new SupportSkillEffectKey(skillA));
        Assert.AreEqual(RegistrationStatus.Registered, result1.Registration);
        Assert.AreEqual(RegistrationStatus.Registered, result2.Registration);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
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
            sourceRune: rune);

        var dotEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            20,
            4,
            sourceRune: rune);

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
        Assert.IsTrue(removed.HasRemovedEffects);
        Assert.AreEqual(1, removed.Count);
        Assert.AreSame(statusEffect, removed.RemovedEffect);
        Assert.AreSame(statusEffect, removed.RemovedEffects[0]);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.IsNull(manager.GetById(2L));

        // RemoveById with non-existent id
        var notRemoved = manager.RemoveById(999L);
        Assert.IsFalse(notRemoved.HasRemovedEffects);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.AreEqual(0, notRemoved.Count);
        Assert.IsNull(notRemoved.RemovedEffect);
        Assert.AreEqual(0, notRemoved.RemovedEffects.Count);

        // RemoveByIdentityKey
        var fireKey = new ElementalDamageOverTimeEffectKey(ElementType.Fire);
        var removedByKey = manager.RemoveByIdentityKey(fireKey);
        Assert.IsTrue(removedByKey.HasRemovedEffects);
        Assert.AreEqual(1, removedByKey.Count);
        Assert.AreSame(dotEffect, removedByKey.RemovedEffect);
        Assert.AreSame(dotEffect, removedByKey.RemovedEffects[0]);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.IsNull(manager.GetByIdentityKey(fireKey));
        

        // RemoveByIdentityKey with non-existent key
        var notRemovedByKey = manager.RemoveByIdentityKey(
            new ElementalDamageOverTimeEffectKey(ElementType.Ice));
        Assert.IsFalse(notRemovedByKey.HasRemovedEffects);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.AreEqual(0, notRemovedByKey.Count);
        Assert.IsNull(notRemovedByKey.RemovedEffect);
        Assert.AreEqual(0, notRemovedByKey.RemovedEffects.Count);
        
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
    
    //I promise its immutable
    [TestMethod]
    public void ActiveEffects_ReturnsReadOnlyCollection()
    {
        var manager = new ActiveEffectManager();

        var skill = GetTestSkill();
        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            3,
            sourceSkill: skill);

        manager.Register(effect);

        var activeEffects = manager.ActiveEffects;

        Assert.AreEqual(1, activeEffects.Count);

        var mutableView = activeEffects as IList<ActiveEffect>;

        Assert.IsNotNull(mutableView);

        Assert.ThrowsException<NotSupportedException>(
            () => mutableView.Add(effect));

        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }
    
    
    // ---------------------------------------------------------------------------
    // T. ClearBySource(ITargetable)
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ClearBySource_NullCaster_Throws()
    {
        var manager = new ActiveEffectManager();

        Assert.ThrowsException<ArgumentNullException>(
            () => manager.ClearBySource((ITargetable)null!));

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_NoMatchingEffects_ReturnsEmptyResult()
    {
        var manager = new ActiveEffectManager();

        var skill = GetTestSkill();
        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        manager.Register(effect);

        var otherCaster = Mock.Of<ITargetable>();
        var result = manager.ClearBySource(otherCaster);

        Assert.IsFalse(result.HasRemovedEffects);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_RemovesSingleMatchingEffect()
    {
        var manager = new ActiveEffectManager();
        var caster = Mock.Of<ITargetable>();
        var rune = Mock.Of<IRune>();

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceCaster: caster,
            sourceRune: rune);

        manager.Register(effect);

        var result = manager.ClearBySource(caster);

        Assert.IsTrue(result.HasRemovedEffects);
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(effect, result.RemovedEffect);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_RemovesMultipleMatchingEffects()
    {
        var manager = new ActiveEffectManager();
        var casterA = Mock.Of<ITargetable>();
        var casterB = Mock.Of<ITargetable>();
        var runeA = Mock.Of<IRune>();
        var runeB = Mock.Of<IRune>();

        var effectA1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceCaster: casterA,
            sourceRune: runeA);

        var effectA2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceCaster: casterA,
            sourceRune: runeA);

        var effectA3 = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            20,
            4,
            sourceCaster: casterA,
            sourceRune: runeA);

        var effectB1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Lightning),
            12,
            6,
            sourceCaster: casterB,
            sourceRune: runeB);

        var effectB2 = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked),
            18,
            2,
            sourceCaster: casterB,
            sourceRune: runeB);

        manager.Register(effectA1);
        manager.Register(effectA2);
        manager.Register(effectA3);
        manager.Register(effectB1);
        manager.Register(effectB2);

        var result = manager.ClearBySource(casterA);

        Assert.IsTrue(result.HasRemovedEffects);
        Assert.AreEqual(3, result.Count);
        Assert.IsNull(result.RemovedEffect);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA1);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA2);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA3);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        CollectionAssert.Contains(manager.ActiveEffects.ToList(), effectB1);
        CollectionAssert.Contains(manager.ActiveEffects.ToList(), effectB2);
        Assert.IsNull(manager.GetById(effectA1.EffectId));
        Assert.IsNull(manager.GetById(effectA2.EffectId));
        Assert.IsNull(manager.GetById(effectA3.EffectId));
    }

    [TestMethod]
    public void ClearBySource_PreservesEffectsFromOtherCasters()
    {
        var manager = new ActiveEffectManager();
        var casterA = Mock.Of<ITargetable>();
        var casterB = Mock.Of<ITargetable>();
        var rune = Mock.Of<IRune>();

        var effectA = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceCaster: casterA,
            sourceRune: rune);

        var effectB = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceCaster: casterB,
            sourceRune: rune);

        manager.Register(effectA);
        manager.Register(effectB);

        var result = manager.ClearBySource(casterA);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.AreSame(effectB, manager.ActiveEffects[0]);
        Assert.IsFalse(result.RemovedEffects.Contains(effectB));
    }

    [TestMethod]
    public void ClearBySource_UsesReferenceEquality()
    {
        var manager = new ActiveEffectManager();
        var casterA = Mock.Of<ITargetable>();
        var casterB = Mock.Of<ITargetable>();
        var rune = Mock.Of<IRune>();

        Assert.IsFalse(ReferenceEquals(casterA, casterB));

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceCaster: casterA,
            sourceRune: rune);

        manager.Register(effect);

        var result = manager.ClearBySource(casterB);

        Assert.IsFalse(result.HasRemovedEffects);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_ReturnsRemovedEffectInstances()
    {
        var manager = new ActiveEffectManager();
        var caster = Mock.Of<ITargetable>();
        var rune = Mock.Of<IRune>();

        var effect1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceCaster: caster,
            sourceRune: rune);

        var effect2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceCaster: caster,
            sourceRune: rune);

        manager.Register(effect1);
        manager.Register(effect2);

        var result = manager.ClearBySource(caster);

        Assert.AreSame(effect1, result.RemovedEffects[0]);
        Assert.AreSame(effect2, result.RemovedEffects[1]);
        Assert.AreEqual(2, result.Count);
        Assert.AreSame(effect1, result.RemovedEffects[0]);
        Assert.AreSame(effect2, result.RemovedEffects[1]);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // U. ClearBySource(ISkill)
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ClearBySource_Skill_NullSource_Throws()
    {
        var manager = new ActiveEffectManager();

        Assert.ThrowsException<ArgumentNullException>(
            () => manager.ClearBySource((ISkill)null!));

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_Skill_NoMatchingEffects_ReturnsEmptyResult()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        manager.Register(effect);

        var otherSkill = GetTestSkill2();
        var result = manager.ClearBySource(otherSkill);

        Assert.IsFalse(result.HasRemovedEffects);
        Assert.AreEqual(0, result.Count);
        Assert.IsNull(result.RemovedEffect);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_Skill_RemovesSingleMatchingEffect()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        manager.Register(effect);

        var result = manager.ClearBySource(skill);

        Assert.IsTrue(result.HasRemovedEffects);
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(effect, result.RemovedEffect);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_Skill_RemovesMultipleMatchingEffects()
    {
        var manager = new ActiveEffectManager();
        var skillA = GetTestSkill();
        var skillB = GetTestSkill2();

        // Use elemental effects with different identity keys but the same SourceSkill
        var effectA1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceSkill: skillA);

        var effectA2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceSkill: skillA);

        var effectA3 = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            20,
            4,
            sourceSkill: skillA);

        var effectB1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Lightning),
            12,
            6,
            sourceSkill: skillB);

        var effectB2 = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked),
            18,
            2,
            sourceSkill: skillB);

        manager.Register(effectA1);
        manager.Register(effectA2);
        manager.Register(effectA3);
        manager.Register(effectB1);
        manager.Register(effectB2);

        var result = manager.ClearBySource(skillA);

        Assert.IsTrue(result.HasRemovedEffects);
        Assert.AreEqual(3, result.Count);
        Assert.IsNull(result.RemovedEffect);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA1);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA2);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA3);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        CollectionAssert.Contains(manager.ActiveEffects.ToList(), effectB1);
        CollectionAssert.Contains(manager.ActiveEffects.ToList(), effectB2);
        Assert.IsNull(manager.GetById(effectA1.EffectId));
        Assert.IsNull(manager.GetById(effectA2.EffectId));
        Assert.IsNull(manager.GetById(effectA3.EffectId));
    }

    [TestMethod]
    public void ClearBySource_Skill_PreservesEffectsFromOtherSkills()
    {
        var manager = new ActiveEffectManager();
        var skillA = GetTestSkill();
        var skillB = GetTestSkill2();

        var effectA = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skillA),
            10,
            5,
            sourceSkill: skillA);

        var effectB = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skillB),
            15,
            3,
            sourceSkill: skillB);

        manager.Register(effectA);
        manager.Register(effectB);

        var result = manager.ClearBySource(skillA);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.AreSame(effectB, manager.ActiveEffects[0]);
        Assert.IsFalse(result.RemovedEffects.Contains(effectB));
    }

    [TestMethod]
    public void ClearBySource_Skill_UsesReferenceEquality()
    {
        var manager = new ActiveEffectManager();

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

        Assert.IsFalse(ReferenceEquals(skillA, skillB));

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skillA),
            10,
            5,
            sourceSkill: skillA);

        manager.Register(effect);

        var result = manager.ClearBySource(skillB);

        Assert.IsFalse(result.HasRemovedEffects);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_Skill_ReturnsRemovedEffectInstances()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();

        var effect1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceSkill: skill);

        var effect2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceSkill: skill);

        manager.Register(effect1);
        manager.Register(effect2);

        var result = manager.ClearBySource(skill);

        Assert.AreSame(effect1, result.RemovedEffects[0]);
        Assert.AreSame(effect2, result.RemovedEffects[1]);
        Assert.AreEqual(2, result.Count);
        Assert.AreSame(effect1, result.RemovedEffects[0]);
        Assert.AreSame(effect2, result.RemovedEffects[1]);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // V. ClearBySource(IRune)
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ClearBySource_Rune_NullSource_Throws()
    {
        var manager = new ActiveEffectManager();

        Assert.ThrowsException<ArgumentNullException>(
            () => manager.ClearBySource((IRune)null!));

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_Rune_NoMatchingEffects_ReturnsEmptyResult()
    {
        var manager = new ActiveEffectManager();
        var rune = Mock.Of<IRune>();

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceRune: rune);

        manager.Register(effect);

        var otherRune = Mock.Of<IRune>();
        var result = manager.ClearBySource(otherRune);

        Assert.IsFalse(result.HasRemovedEffects);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_Rune_RemovesSingleMatchingEffect()
    {
        var manager = new ActiveEffectManager();
        var rune = Mock.Of<IRune>();

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceRune: rune);

        manager.Register(effect);

        var result = manager.ClearBySource(rune);

        Assert.IsTrue(result.HasRemovedEffects);
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(effect, result.RemovedEffect);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_Rune_RemovesMultipleMatchingEffects()
    {
        var manager = new ActiveEffectManager();
        var runeA = Mock.Of<IRune>();
        var runeB = Mock.Of<IRune>();

        var effectA1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceRune: runeA);

        var effectA2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceRune: runeA);

        var effectA3 = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            20,
            4,
            sourceRune: runeA);

        var effectB1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Lightning),
            12,
            6,
            sourceRune: runeB);

        var effectB2 = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked),
            18,
            2,
            sourceRune: runeB);

        manager.Register(effectA1);
        manager.Register(effectA2);
        manager.Register(effectA3);
        manager.Register(effectB1);
        manager.Register(effectB2);

        var result = manager.ClearBySource(runeA);

        Assert.IsTrue(result.HasRemovedEffects);
        Assert.AreEqual(3, result.Count);
        Assert.IsNull(result.RemovedEffect);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA1);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA2);
        CollectionAssert.Contains(result.RemovedEffects.ToList(), effectA3);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        CollectionAssert.Contains(manager.ActiveEffects.ToList(), effectB1);
        CollectionAssert.Contains(manager.ActiveEffects.ToList(), effectB2);
        Assert.IsNull(manager.GetById(effectA1.EffectId));
        Assert.IsNull(manager.GetById(effectA2.EffectId));
        Assert.IsNull(manager.GetById(effectA3.EffectId));
    }

    [TestMethod]
    public void ClearBySource_Rune_PreservesEffectsFromOtherRunes()
    {
        var manager = new ActiveEffectManager();
        var runeA = Mock.Of<IRune>();
        var runeB = Mock.Of<IRune>();

        var effectA = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceRune: runeA);

        var effectB = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceRune: runeB);

        manager.Register(effectA);
        manager.Register(effectB);

        var result = manager.ClearBySource(runeA);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.AreSame(effectB, manager.ActiveEffects[0]);
        Assert.IsFalse(result.RemovedEffects.Contains(effectB));
    }

    [TestMethod]
    public void ClearBySource_Rune_UsesReferenceEquality()
    {
        var manager = new ActiveEffectManager();
        var runeA = Mock.Of<IRune>();
        var runeB = Mock.Of<IRune>();

        Assert.IsFalse(ReferenceEquals(runeA, runeB));

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceRune: runeA);

        manager.Register(effect);

        var result = manager.ClearBySource(runeB);

        Assert.IsFalse(result.HasRemovedEffects);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearBySource_Rune_ReturnsRemovedEffectInstances()
    {
        var manager = new ActiveEffectManager();
        var rune = Mock.Of<IRune>();

        var effect1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceRune: rune);

        var effect2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceRune: rune);

        manager.Register(effect1);
        manager.Register(effect2);

        var result = manager.ClearBySource(rune);

        Assert.AreSame(effect1, result.RemovedEffects[0]);
        Assert.AreSame(effect2, result.RemovedEffects[1]);
        Assert.AreEqual(2, result.Count);
        Assert.AreSame(effect1, result.RemovedEffects[0]);
        Assert.AreSame(effect2, result.RemovedEffects[1]);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // W. ClearAll
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ClearAll_EmptyManager_ReturnsEmptyResult()
    {
        var manager = new ActiveEffectManager();

        var result = manager.ClearAll();

        Assert.IsFalse(result.HasRemovedEffects);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearAll_RemovesSingleEffect()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        manager.Register(effect);

        var result = manager.ClearAll();

        Assert.IsTrue(result.HasRemovedEffects);
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(effect, result.RemovedEffect);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearAll_RemovesMultipleEffects()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();
        var rune = Mock.Of<IRune>();

        var effect1 = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        var effect2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            15,
            3,
            sourceRune: rune);

        var effect3 = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            20,
            4,
            sourceRune: rune);

        manager.Register(effect1);
        manager.Register(effect2);
        manager.Register(effect3);

        var result = manager.ClearAll();

        Assert.AreEqual(3, result.Count);
        Assert.IsNull(result.RemovedEffect);
        Assert.AreEqual(3, result.RemovedEffects.Count);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearAll_RemovesEffectsRegardlessOfType()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();
        var rune = Mock.Of<IRune>();

        var supportEffect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        var statusEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            15,
            3,
            sourceRune: rune);

        var dotEffect = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            20,
            4,
            sourceRune: rune);

        manager.Register(supportEffect);
        manager.Register(statusEffect);
        manager.Register(dotEffect);

        var result = manager.ClearAll();

        Assert.AreEqual(3, result.Count);
        Assert.IsNull(result.RemovedEffect);
        Assert.AreEqual(3, result.RemovedEffects.Count);

        CollectionAssert.AreEquivalent(
            new[] { supportEffect, statusEffect, dotEffect },
            result.RemovedEffects.ToArray());

        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    [TestMethod]
    public void ClearAll_ReturnsAllOriginalEffectInstances()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();
        var rune = Mock.Of<IRune>();

        var effect1 = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        var effect2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            15,
            3,
            sourceRune: rune);

        manager.Register(effect1);
        manager.Register(effect2);

        var result = manager.ClearAll();

        Assert.AreSame(effect1, result.RemovedEffects[0]);
        Assert.AreSame(effect2, result.RemovedEffects[1]);
    }

    [TestMethod]
    public void ClearAll_DoesNotResetEffectIdSequence()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();

        var effect1 = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        var rune = Mock.Of<IRune>();
        var effect2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            15,
            3,
            sourceRune: rune);

        manager.Register(effect1);
        manager.Register(effect2);

        var clearResult = manager.ClearAll();
        Assert.AreEqual(2, clearResult.Count);

        var skill2 = GetTestSkill2();
        var effect3 = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill2),
            20,
            4,
            sourceSkill: skill2);

        manager.Register(effect3);

        Assert.AreEqual(3L, effect3.EffectId);
    }

    [TestMethod]
    public void ClearAll_CanBeCalledRepeatedly()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        manager.Register(effect);

        var result1 = manager.ClearAll();
        Assert.AreEqual(1, result1.Count);

        var result2 = manager.ClearAll();
        Assert.IsFalse(result2.HasRemovedEffects);
        Assert.AreEqual(0, result2.Count);
    }

    // ---------------------------------------------------------------------------
    // X. Removal result contract
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void RemovalResult_OneEffect_SetsRemovedEffect()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            10,
            5,
            sourceSkill: skill);

        manager.Register(effect);

        var result = manager.RemoveById(effect.EffectId);

        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.HasRemovedEffects);
        Assert.AreSame(effect, result.RemovedEffect);
    }

    [TestMethod]
    public void RemovalResult_MultipleEffects_HasNullRemovedEffect()
    {
        var manager = new ActiveEffectManager();
        var skill = GetTestSkill();

        var effect1 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Fire),
            10,
            5,
            sourceSkill: skill);

        var effect2 = new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            15,
            3,
            sourceSkill: skill);

        manager.Register(effect1);
        manager.Register(effect2);

        var result = manager.ClearBySource(skill);

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.HasRemovedEffects);
        Assert.IsNull(result.RemovedEffect);
    }

    [TestMethod]
    public void RemovalResult_EmptyCollection_HasNoRemovedEffects()
    {
        var manager = new ActiveEffectManager();

        var result = manager.RemoveById(999L);

        Assert.IsFalse(result.HasRemovedEffects);
        Assert.AreEqual(0, result.Count);
        Assert.IsNull(result.RemovedEffect);
    }
    
    [TestMethod]
    public void ClearBySource_Caster_DoesNotMatchSharedRune()
    {
        var manager = new ActiveEffectManager();

        var casterA = new Mock<ITargetable>().Object;
        var casterB = new Mock<ITargetable>().Object;
        var sharedRune = new Mock<IRune>().Object;

        var effectA = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            power: 10,
            duration: 3,
            sourceCaster: casterA,
            sourceRune: sharedRune);

        var effectB = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Frozen),
            power: 10,
            duration: 3,
            sourceCaster: casterB,
            sourceRune: sharedRune);

        manager.Register(effectA);
        manager.Register(effectB);

        var result = manager.ClearBySource(casterA);

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(effectA, result.RemovedEffect);
        Assert.AreSame(effectB, manager.ActiveEffects.Single());
    }
    
    [TestMethod]
    public void ClearBySource_Rune_DoesNotMatchSharedCaster()
    {
        var manager = new ActiveEffectManager();

        var sharedCaster = new Mock<ITargetable>().Object;
        var runeA = new Mock<IRune>().Object;
        var runeB = new Mock<IRune>().Object;

        var effectA = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked),
            power: 10,
            duration: 3,
            sourceCaster: sharedCaster,
            sourceRune: runeA);

        var effectB = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 10,
            duration: 3,
            sourceCaster: sharedCaster,
            sourceRune: runeB);

        manager.Register(effectA);
        manager.Register(effectB);

        var result = manager.ClearBySource(runeA);

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(effectA, result.RemovedEffect);
        Assert.AreSame(effectB, manager.ActiveEffects.Single());
    }
}
