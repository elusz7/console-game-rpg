using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Combat.ActiveEffects;

[TestClass]
public class ActiveEffectManagerDoTTickTests
{
    private static ActiveEffect CreateDamageOverTimeEffect(
        ElementType element,
        int power,
        int duration,
        ITargetable? sourceCaster = null,
        IRune? sourceRune = null)
    {
        var key = new ElementalDamageOverTimeEffectKey(element);
        return new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            key,
            power,
            duration,
            sourceCaster: sourceCaster,
            sourceRune: sourceRune);
    }

    // ---------------------------------------------------------------------------
    // ProcessDoTTicks — duration and expiration
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ProcessDoTTicks_DurationThree_ProcessesThreeTicksAndExpires()
    {
        var manager = new ActiveEffectManager();
        var target = new Mock<ITargetable>();
        target.Setup(t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()))
            .Returns(new DoTEffectResult(5, 5, ElementType.Fire, false));

        var effect = CreateDamageOverTimeEffect(ElementType.Fire, 5, 3);
        manager.Register(effect);

        // Tick 1
        var result1 = manager.ProcessDoTTicks(target.Object);
        Assert.AreEqual(1, result1.EffectResults.Count);
        Assert.AreEqual(1, result1.ProcessedEffects.Count);
        Assert.AreEqual(0, result1.ExpiredEffects.Count);
        Assert.AreEqual(2, effect.RemainingDuration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);

        // Tick 2
        var result2 = manager.ProcessDoTTicks(target.Object);
        Assert.AreEqual(1, result2.EffectResults.Count);
        Assert.AreEqual(1, result2.ProcessedEffects.Count);
        Assert.AreEqual(0, result2.ExpiredEffects.Count);
        Assert.AreEqual(1, effect.RemainingDuration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);

        // Tick 3 — expires
        var result3 = manager.ProcessDoTTicks(target.Object);
        Assert.AreEqual(1, result3.EffectResults.Count);
        Assert.AreEqual(1, result3.ProcessedEffects.Count);
        Assert.AreEqual(1, result3.ExpiredEffects.Count);
        Assert.AreSame(effect, result3.ExpiredEffects[0]);
        Assert.AreEqual(0, effect.RemainingDuration);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // ProcessDoTTicks — newly registered effects are not ticked
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ProcessDoTTicks_DoesNotTickNewlyRegisteredEffect()
    {
        var manager = new ActiveEffectManager();
        var target = new Mock<ITargetable>();

        var effect1 = CreateDamageOverTimeEffect(ElementType.Fire, 5, 3);
        var effect2 = CreateDamageOverTimeEffect(ElementType.Ice, 8, 3);

        target.Setup(t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()))
            .Callback<int, ElementType>((_, _) => manager.Register(effect2))
            .Returns((int damage, ElementType element) =>
                new DoTEffectResult(damage, damage, element, false));

        manager.Register(effect1);

        var result = manager.ProcessDoTTicks(target.Object);

        Assert.AreEqual(1, result.ProcessedEffects.Count);
        Assert.AreSame(effect1, result.ProcessedEffects[0]);
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.AreEqual(3, effect2.RemainingDuration);
        target.Verify(t => t.ApplyElementalDamage(5, ElementType.Fire), Times.Once);
    }

    // ---------------------------------------------------------------------------
    // TickTurnBased — does not decrement DoT effects
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TickTurnBased_DoesNotDecrementDoT()
    {
        var manager = new ActiveEffectManager();

        var dotEffect = CreateDamageOverTimeEffect(ElementType.Fire, 5, 3);
        manager.Register(dotEffect);

        var result = manager.TickTurnBased();

        Assert.AreEqual(0, result.ProcessedEffects.Count);
        Assert.AreEqual(0, result.ExpiredEffects.Count);
        Assert.AreEqual(3, dotEffect.RemainingDuration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // ProcessDoTTicks — decrements duration after processed tick
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ProcessDoTTicks_DecrementsDurationAfterProcessedTick()
    {
        var manager = new ActiveEffectManager();
        var target = new Mock<ITargetable>();
        target.Setup(t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()))
            .Returns(new DoTEffectResult(10, 10, ElementType.Fire, false));

        var effect = CreateDamageOverTimeEffect(ElementType.Fire, 10, 5);
        manager.Register(effect);
        Assert.AreEqual(5, effect.RemainingDuration);

        var result = manager.ProcessDoTTicks(target.Object);

        Assert.AreEqual(1, result.ProcessedEffects.Count);
        Assert.AreEqual(4, effect.RemainingDuration);
        Assert.AreEqual(0, result.ExpiredEffects.Count);
    }

    // ---------------------------------------------------------------------------
    // ProcessDoTTicks — zero damage still consumes duration
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ProcessDoTTicks_ZeroDamageStillConsumesDuration()
    {
        var manager = new ActiveEffectManager();
        var target = new Mock<ITargetable>();
        target.Setup(t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()))
            .Returns(new DoTEffectResult(0, 0, ElementType.Fire, false));

        var effect = CreateDamageOverTimeEffect(ElementType.Fire, 0, 2);
        manager.Register(effect);

        var result1 = manager.ProcessDoTTicks(target.Object);
        Assert.AreEqual(1, result1.EffectResults.Count);
        Assert.AreEqual(0, result1.EffectResults[0].InitialDamage);
        Assert.AreEqual(1, effect.RemainingDuration);

        var result2 = manager.ProcessDoTTicks(target.Object);
        Assert.AreEqual(1, result2.ExpiredEffects.Count);
        Assert.AreEqual(0, effect.RemainingDuration);
        Assert.AreEqual(0, manager.ActiveEffects.Count);
        
        target.Verify(
            t => t.ApplyElementalDamage(0, ElementType.Fire),
            Times.Exactly(2));
    }

    // ---------------------------------------------------------------------------
    // ProcessDoTTicks — target dies stops processing subsequent effects
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ProcessDoTTicks_TargetDies_StopsProcessingSubsequentEffects()
    {
        var manager = new ActiveEffectManager();
        var target = new Mock<ITargetable>();

        target.SetupSequence(t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()))
            .Returns(new DoTEffectResult(5, 5, ElementType.Fire, false))
            .Returns(new DoTEffectResult(8, 8, ElementType.Ice, true));

        var fireEffect = CreateDamageOverTimeEffect(ElementType.Fire, 5, 5);
        var iceEffect = CreateDamageOverTimeEffect(ElementType.Ice, 8, 5);
        var natureEffect = CreateDamageOverTimeEffect(ElementType.Nature, 3, 4);

        manager.Register(fireEffect);
        manager.Register(iceEffect);
        manager.Register(natureEffect);

        var result = manager.ProcessDoTTicks(target.Object);

        // Only the first two effects are processed; the second kills the target.
        Assert.AreEqual(2, result.EffectResults.Count);
        Assert.AreEqual(2, result.ProcessedEffects.Count);
        Assert.AreSame(fireEffect, result.ProcessedEffects[0]);
        Assert.AreSame(iceEffect, result.ProcessedEffects[1]);
        Assert.IsTrue(result.EffectResults[1].TargetDied);

        // Both processed effects consume one duration tick.
        Assert.AreEqual(4, fireEffect.RemainingDuration);
        Assert.AreEqual(4, iceEffect.RemainingDuration);

        // The third effect is untouched.
        Assert.AreEqual(4, natureEffect.RemainingDuration);
        Assert.AreEqual(3, manager.ActiveEffects.Count);

        target.Verify(
            t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()),
            Times.Exactly(2));
    }

    // ---------------------------------------------------------------------------
    // ProcessDoTTicks — uses current (strengthened) power
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ProcessDoTTicks_UsesStrengthenedEffectPower()
    {
        var manager = new ActiveEffectManager();
        var target = new Mock<ITargetable>();
        var capturedDamage = 0;
        var capturedElement = ElementType.Fire;
        target.Setup(t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()))
            .Returns((int damage, ElementType element) =>
            {
                capturedDamage = damage;
                capturedElement = element;
                return new DoTEffectResult(damage, damage, element, false);
            });

        var sourceCaster = Mock.Of<ITargetable>();
        var effect = CreateDamageOverTimeEffect(ElementType.Fire, 10, 5, sourceCaster: sourceCaster);
        manager.Register(effect);

        // Strengthen the effect to a higher power
        var strongerEffect = CreateDamageOverTimeEffect(ElementType.Fire, 20, 5, sourceCaster: sourceCaster);
        var updateResult = manager.Register(strongerEffect);
        Assert.AreEqual(RegistrationStatus.Strengthened, updateResult.Registration);

        var result = manager.ProcessDoTTicks(target.Object);

        // The tick should use the current (strengthened) power of 20
        Assert.AreEqual(20, capturedDamage);
        Assert.AreEqual(ElementType.Fire, capturedElement);
        Assert.AreEqual(1, result.EffectResults.Count);
        Assert.AreEqual(20, result.EffectResults[0].InitialDamage);
    }

    // ---------------------------------------------------------------------------
    // ProcessDoTTicks — Blinded status does not increase DoT damage
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ProcessDoTTicks_BlindedDoesNotIncreaseDamage()
    {
        var manager = new ActiveEffectManager();
        var target = new Mock<ITargetable>();
        var capturedDamage = 0;
        target.Setup(t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()))
            .Returns((int damage, ElementType element) =>
            {
                capturedDamage = damage;
                return new DoTEffectResult(damage, damage, element, false);
            });

        var effect = CreateDamageOverTimeEffect(ElementType.Fire, 10, 3);
        manager.Register(effect);

        // Add a Blinded status effect to the same manager
        var blindedKey = new ElementalStatusEffectKey(ElementalStatusEffectType.Blinded);
        var blindedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            blindedKey,
            5,
            3);
        manager.Register(blindedEffect);

        var result = manager.ProcessDoTTicks(target.Object);

        // ProcessDoTTicks passes effect.Power directly; Blinded does not increase it
        Assert.AreEqual(10, capturedDamage);
        Assert.AreEqual(1, result.EffectResults.Count);
        
        Assert.AreEqual(2, manager.ActiveEffects.Count);
        Assert.AreEqual(3, blindedEffect.RemainingDuration);
        Assert.IsTrue(manager.ActiveEffects.Contains(blindedEffect));
    }

    // ---------------------------------------------------------------------------
    // ProcessDoTTicks — stronger refresh preserves original source
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ProcessDoTTicks_StrongerRefresh_PreservesOriginalSource()
    {
        var manager = new ActiveEffectManager();
        var target = new Mock<ITargetable>();
        target.Setup(t => t.ApplyElementalDamage(It.IsAny<int>(), It.IsAny<ElementType>()))
            .Returns(new DoTEffectResult(20, 20, ElementType.Fire, false));

        var originalCaster = Mock.Of<ITargetable>();
        var originalRune = Mock.Of<IRune>();

        var effect = CreateDamageOverTimeEffect(
            ElementType.Fire, 10, 3,
            sourceCaster: originalCaster,
            sourceRune: originalRune);
        manager.Register(effect);

        // Apply a stronger refresh from a different source
        var newCaster = Mock.Of<ITargetable>();
        var newRune = Mock.Of<IRune>();
        var strongerEffect = CreateDamageOverTimeEffect(
            ElementType.Fire, 20, 5,
            sourceCaster: newCaster,
            sourceRune: newRune);
        var updateResult = manager.Register(strongerEffect);
        Assert.AreEqual(RegistrationStatus.Strengthened, updateResult.Registration);

        // The existing effect should still reference the original source
        Assert.AreSame(originalCaster, effect.SourceCaster);
        Assert.AreSame(originalRune, effect.SourceRune);
        Assert.AreEqual(20, effect.Power);
        Assert.AreEqual(5, effect.RemainingDuration);

        var result = manager.ProcessDoTTicks(target.Object);
        Assert.AreEqual(1, result.ProcessedEffects.Count);
        Assert.AreSame(effect, result.ProcessedEffects[0]);
    }
}
