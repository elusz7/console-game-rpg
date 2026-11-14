using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class MonsterBasicAttackTests
{
    private class TestMonster(
        bool statusEffectSucceeds,
        ElementType element = ElementType.Fire,
        int elementalPower = 12)
        : DummyMonster(element, elementalPower)
    {
        protected override bool ApplyStatusEffect()
        {
            return statusEffectSucceeds;
        }
    }
    
    // ── Standard damage only ──

    [TestMethod]
    public void MakeBasicAttack_StandardDamageOnly()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster();

        monster.MakeBasicAttack(player);

        Assert.AreEqual(50, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    // ── Elemental damage only ──

    [TestMethod]
    public void MakeBasicAttack_ElementalDamageOnly()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            AttackPower = 0
        };

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    // ── Combined damage ──

    [TestMethod]
    public void MakeBasicAttack_CombinedDamage()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(50, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    // ── Multiplier affects standard damage only ──

    [TestMethod]
    public void MakeBasicAttack_MultiplierAffectsStandardDamageOnly()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        monster.MakeBasicAttack(player, multiplier: 1.5);

        var expectedStandard = (int)Math.Round(monster.AttackPower * 1.5, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedStandard, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    // ── Corrupted reduces both components ──

    [TestMethod]
    public void MakeBasicAttack_CorruptedReducesBothComponents()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 10,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(50 - 10, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    // ── Corrupted reduces either component to zero ──

    [TestMethod]
    public void MakeBasicAttack_CorruptedReducesStandardToZero_ElementalSurvives()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 60)
        {
            AttackPower = 50
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 50,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.DamageTaken);
        Assert.AreEqual(10, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    [TestMethod]
    public void MakeBasicAttack_CorruptedReducesElementalToZero_StandardSurvives()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 5,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(50 - 5, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    // ── Corrupted reduces both to zero and throws ──

    [TestMethod]
    public void MakeBasicAttack_CorruptedReducesBothToZero_ThrowsAttackException()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster();

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 50,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        Assert.ThrowsException<AttackException>(() => monster.MakeBasicAttack(player));
    }

    // ── Damage components never become negative ──

    [TestMethod]
    public void MakeBasicAttack_DamageComponentsNeverBecomeNegative()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 3)
        {
            AttackPower = 10
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 100,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        Assert.ThrowsException<AttackException>(() => monster.MakeBasicAttack(player));

        Assert.AreEqual(0, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
    }

    // ── Element is null when elemental damage is zero ──

    [TestMethod]
    public void MakeBasicAttack_ElementIsNullWhenElementalDamageIsZero()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 5,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    [TestMethod]
    public void MakeBasicAttack_ElementIsNullWithoutElementalProperties()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster();

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    // ── Correct action messages for each component ──

    [TestMethod]
    public void MakeBasicAttack_ActionMessages_ForStandardDamageOnly()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster();

        monster.MakeBasicAttack(player);

        var actionValues = monster.ActionItems.Values.ToList();
        Assert.IsTrue(actionValues.Any(a => a.Contains("50") && a.Contains(monster.DamageType.ToString())));
        Assert.IsFalse(actionValues.Any(a => a.Contains("Fire")));
    }

    [TestMethod]
    public void MakeBasicAttack_ActionMessages_ForBothComponents()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        monster.MakeBasicAttack(player);

        var actionValues = monster.ActionItems.Values.ToList();
        Assert.IsTrue(actionValues.Any(a => a.Contains("50") && a.Contains(monster.DamageType.ToString())));
        Assert.IsTrue(actionValues.Any(a => a.Contains("5") && a.Contains("Fire")));
    }

    [TestMethod]
    public void MakeBasicAttack_ActionMessages_NoElementalMessage_WhenElementalDamageIsZero()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 5,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        var actionValues = monster.ActionItems.Values.ToList();
        Assert.IsTrue(actionValues.Any(a => a.Contains("45") && a.Contains(monster.DamageType.ToString())));
        Assert.IsFalse(actionValues.Any(a => a.Contains("Fire")));
    }

    // ── Midpoint rounding behavior ──

    [TestMethod]
    public void MakeBasicAttack_MidpointRoundingBehavior()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            AttackPower = 33
        };

        monster.MakeBasicAttack(player, multiplier: 1.5);

        var expected = (int)Math.Round(33 * 1.5, MidpointRounding.AwayFromZero);
        Assert.AreEqual(50, expected);
        Assert.AreEqual(expected, player.DamageTaken);
    }
    
    // ── Status Effect Probability

    [DataTestMethod]
    [DataRow(ThreatLevel.Low, 0.0)]
    [DataRow(ThreatLevel.Medium, 5.0)]
    [DataRow(ThreatLevel.High, 10.0)]
    [DataRow(ThreatLevel.Elite, 20.0)]
    [DataRow(ThreatLevel.Boss, 30.0)]
    public void ApplyElementalStatusEffect_ThreatLevel_UsesExpectedProbability(
        ThreatLevel threatLevel,
        double expectedChance)
    {
        var monster = new DummyMonster
        {
            ThreatLevel = threatLevel
        };

        var threshold = expectedChance / 100.0;

        if (expectedChance > 0)
        {
            Assert.IsTrue(
                monster.ApplyElementalStatusEffect(threshold - 0.001),
                $"{threatLevel} should succeed immediately below its threshold.");
        }

        Assert.IsFalse(
            monster.ApplyElementalStatusEffect(threshold),
            $"{threatLevel} should fail at its probability threshold.");
    }
    
    [TestMethod]
    public void ApplyElementalStatusEffect_InvalidRoll_ThrowsArgumentOutOfRangeException()
    {
        var monster = new DummyMonster
        {
            ThreatLevel = ThreatLevel.Medium
        };

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => monster.ApplyElementalStatusEffect(-0.01));

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => monster.ApplyElementalStatusEffect(1.0));
    }

    [TestMethod]
    public void ApplyElementalStatusEffect_InvalidThreatLevel_ThrowsAttackException()
    {
        var monster = new DummyMonster
        {
            ThreatLevel = (ThreatLevel)999
        };

        Assert.ThrowsException<AttackException>(
            () => monster.ApplyElementalStatusEffect(0.0));
    }
    
    // ── StatusEffectHits
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_StatusEffectUsesExpectedPower()
    {
        var testCases = new[]
        {
            (ThreatLevel.Medium, 3),
            (ThreatLevel.High, 4),
            (ThreatLevel.Elite, 5),
            (ThreatLevel.Boss, 7)
        };

        foreach (var (threatLevel, expectedPower) in testCases)
        {
            var player = new DummyPlayer();
            var monster = new TestMonster(true)
            {
                ThreatLevel = threatLevel
            };

            monster.MakeBasicAttack(player);

            var statusEffect = player.ActiveEffectManager
                .GetByType(ActiveEffectType.ElementalStatus)
                .Single();

            Assert.AreEqual(expectedPower, statusEffect.Power,
                $"{threatLevel} should produce status power {expectedPower}.");
        }
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_StatusEffectUsesExpectedDuration()
    {
        var testCases = new[]
        {
            (ThreatLevel.Medium, 2),
            (ThreatLevel.High, 3),
            (ThreatLevel.Elite, 4),
            (ThreatLevel.Boss, 6)
        };

        foreach (var (threatLevel, expectedDuration) in testCases)
        {
            var player = new DummyPlayer();
            var monster = new TestMonster(true)
            {
                ThreatLevel = threatLevel
            };

            monster.MakeBasicAttack(player);

            var statusEffect = player.ActiveEffectManager
                .GetByType(ActiveEffectType.ElementalStatus)
                .Single();

            Assert.AreEqual(expectedDuration, statusEffect.Duration,
                $"{threatLevel} should produce duration {expectedDuration}.");
        }
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_StatusEffectUsesAttackElement()
    {
        var testCases = new[]
        {
            (ElementType.Fire, ElementalStatusEffectType.Charred),
            (ElementType.Lightning, ElementalStatusEffectType.Shocked),
            (ElementType.Ice, ElementalStatusEffectType.Frozen),
            (ElementType.Nature, ElementalStatusEffectType.Snared),
            (ElementType.Radiance, ElementalStatusEffectType.Blinded),
            (ElementType.Abyssal, ElementalStatusEffectType.Corrupted)
        };

        foreach (var (element, expectedStatus) in testCases)
        {
            var player = new DummyPlayer();
            var monster = new TestMonster(true, element)
            {
                ThreatLevel = ThreatLevel.Medium
            };

            monster.MakeBasicAttack(player);

            var statusEffect = player.ActiveEffectManager
                .GetByType(ActiveEffectType.ElementalStatus)
                .Single();

            var identityKey = (ElementalStatusEffectKey)statusEffect.EffectIdentityKey;

            Assert.AreEqual(expectedStatus, identityKey.StatusType,
                $"{element} should inflict {expectedStatus}.");
        }
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_StatusChanceSucceeds_RegistersStatusEffect()
    {
        var player = new DummyPlayer();
        var monster = new TestMonster(true)
        {
            ThreatLevel = ThreatLevel.Medium
        };

        monster.MakeBasicAttack(player);

        Assert.AreEqual(
            1,
            player.ActiveEffectManager.GetByType(ActiveEffectType.ElementalStatus).Count);

        Assert.AreEqual(
            1,
            player.ActiveEffectManager.GetByType(ActiveEffectType.ElementalDamageOverTime).Count);
    }
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_StatusChanceFails_StillRegistersDamageOverTime()
    {
        var player = new DummyPlayer();
        var monster = new TestMonster(false)
        {
            ThreatLevel = ThreatLevel.Boss
        };

        monster.MakeBasicAttack(player);

        Assert.AreEqual(
            0,
            player.ActiveEffectManager.GetByType(ActiveEffectType.ElementalStatus).Count);

        Assert.AreEqual(
            1,
            player.ActiveEffectManager.GetByType(ActiveEffectType.ElementalDamageOverTime).Count);
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_StatusEffectSetsSourceCaster()
    {
        var player = new DummyPlayer();
        var monster = new TestMonster(true)
        {
            ThreatLevel = ThreatLevel.Medium
        };

        monster.MakeBasicAttack(player);

        var statusEffect = player.ActiveEffectManager
            .GetByType(ActiveEffectType.ElementalStatus)
            .Single();

        Assert.AreSame(monster, statusEffect.SourceCaster);
    }
    
    private class FixedResultPlayer(AttackResolutionResult result) : DummyPlayer
    {
        public override AttackResolutionResult TakeDamage(AttackDamage attack)
        {
            return result;
        }
    }
    
    [TestMethod]
    public void MakeBasicAttack_DodgedElementalAttack_DoesNotRegisterEffects()
    {
        var result = new AttackResolutionResult(
            0, 0, DamageType.Martial,
            12, 0, ElementType.Fire,
            true, false);

        var player = new FixedResultPlayer(result);
        var monster = new TestMonster(true);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.ActiveEffectManager.ActiveEffects.Count);
    }
    
    [TestMethod]
    public void MakeBasicAttack_NegatedElementalDamage_DoesNotRegisterEffects()
    {
        var result = new AttackResolutionResult(
            0, 0, DamageType.Martial,
            12, 0, ElementType.Fire,
            false, false);

        var player = new FixedResultPlayer(result);
        var monster = new TestMonster(true);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.ActiveEffectManager.ActiveEffects.Count);
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_DoTEffectSetsSourceCaster()
    {
        var player = new DummyPlayer();
        var monster = new TestMonster(false)
        {
            ThreatLevel = ThreatLevel.Medium
        };

        monster.MakeBasicAttack(player);

        var dotEffect = player.ActiveEffectManager
            .GetByType(ActiveEffectType.ElementalDamageOverTime)
            .Single();

        Assert.AreSame(monster, dotEffect.SourceCaster);
        Assert.IsNull(dotEffect.SourceRune);
    }
}
