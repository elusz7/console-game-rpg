
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Models.Combat;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Helpers;

[TestClass]
public class DamageResolverTests
{
    // Provides predictable random values for dodge tests.
    private class FixedRandom(double value) : Random
    {
        public int CallCount { get; private set; }

        public override double NextDouble()
        {
            CallCount++;
            return value;
        }
    }

    // ── ResolveMonsterDamage: Standard Damage Only ──

    [TestMethod]
    public void ResolveMonsterDamage_Martial_NoDefense_ReturnsFullDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Martial, 0, null);
        var modifiers = new MonsterDamageModifiers(0, 0, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(20, result.FinalStandardDamage);
        Assert.AreEqual(0, result.FinalElementalDamage);
        Assert.IsFalse(result.IsDodged);
        Assert.AreEqual(DamageType.Martial, result.StandardDamageType);
    }

    [TestMethod]
    public void ResolveMonsterDamage_Martial_WithDefense_ReducesDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Martial, 0, null);
        var modifiers = new MonsterDamageModifiers(10, 0, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(10, result.FinalStandardDamage);
        Assert.IsFalse(result.IsDodged);
    }

    [TestMethod]
    public void ResolveMonsterDamage_Magical_WithResistance_ReducesDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Magical, 0, null);
        var modifiers = new MonsterDamageModifiers(0, 10, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(10, result.FinalStandardDamage);
        Assert.IsFalse(result.IsDodged);
    }

    [TestMethod]
    public void ResolveMonsterDamage_Hybrid_WithDefenseAndResistance_ReducesDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Hybrid, 0, null);
        var modifiers = new MonsterDamageModifiers(8, 4, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(16, result.FinalStandardDamage);
        Assert.IsFalse(result.IsDodged);
    }

    // ── ResolveMonsterDamage: Elemental Damage ──

    [DataTestMethod]
    [DataRow(ElementType.Fire, ElementType.Ice, ElementType.Nature)]
    [DataRow(ElementType.Lightning, ElementType.Ice, ElementType.Nature)]
    [DataRow(ElementType.Ice, ElementType.Fire, ElementType.Nature)]
    [DataRow(ElementType.Nature, ElementType.Fire, ElementType.Ice)]
    [DataRow(ElementType.Radiance, ElementType.Fire, ElementType.Ice)]
    [DataRow(ElementType.Abyssal, ElementType.Fire, ElementType.Ice)]
    public void ResolveMonsterDamage_Elemental_Neutral_ReturnsBaseDamage(
        ElementType attackElement,
        ElementType resilience,
        ElementType vulnerability)
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 10, attackElement);
        var modifiers = new MonsterDamageModifiers(
            0, 0, resilience, vulnerability);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(10, result.FinalStandardDamage);
        Assert.AreEqual(10, result.FinalElementalDamage);
        Assert.IsFalse(result.IsDodged);
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire, ElementType.Ice)]
    [DataRow(ElementType.Lightning, ElementType.Fire)]
    [DataRow(ElementType.Ice, ElementType.Nature)]
    [DataRow(ElementType.Nature, ElementType.Fire)]
    [DataRow(ElementType.Radiance, ElementType.Fire)]
    [DataRow(ElementType.Abyssal, ElementType.Fire)]
    public void ResolveMonsterDamage_Elemental_Resilient_AppliesHalfMultiplier(
        ElementType element,
        ElementType vulnerability)
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 10, element);
        var modifiers = new MonsterDamageModifiers(
            0, 0, element, vulnerability);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(5, result.FinalElementalDamage);
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire, ElementType.Ice)]
    [DataRow(ElementType.Lightning, ElementType.Fire)]
    [DataRow(ElementType.Ice, ElementType.Nature)]
    [DataRow(ElementType.Nature, ElementType.Fire)]
    [DataRow(ElementType.Radiance, ElementType.Fire)]
    [DataRow(ElementType.Abyssal, ElementType.Fire)]
    public void ResolveMonsterDamage_Elemental_Vulnerable_AppliesOnePointFiveMultiplier(
        ElementType element,
        ElementType resilience)
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 10, element);
        var modifiers = new MonsterDamageModifiers(
            0, 0, resilience, element);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(15, result.FinalElementalDamage);
    }

    // ── ResolveMonsterDamage: Combined Standard + Elemental ──

    [TestMethod]
    public void ResolveMonsterDamage_Combined_BothDamagesResolved()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(
            5, 0, ElementType.Ice, ElementType.Fire);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(15, result.FinalStandardDamage);
        Assert.AreEqual(15, result.FinalElementalDamage);
        Assert.IsFalse(result.IsDodged);
    }

    [TestMethod]
    public void ResolveMonsterDamage_Combined_Dodged_BothZero()
    {
        var resolver = new DamageResolver(new FixedRandom(0.0));
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(
            5, 0, ElementType.Ice, ElementType.Fire);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 33);

        Assert.AreEqual(0, result.FinalStandardDamage);
        Assert.AreEqual(0, result.FinalElementalDamage);
        Assert.IsTrue(result.IsDodged);
    }

    // ── ResolveMonsterDamage: Dodge ──

    [TestMethod]
    public void ResolveMonsterDamage_Dodged_ZeroStandardAndElementalDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.0));
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(0, 0, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 33);

        Assert.AreEqual(0, result.FinalStandardDamage);
        Assert.AreEqual(0, result.FinalElementalDamage);
        Assert.IsTrue(result.IsDodged);
    }

    [TestMethod]
    public void ResolveMonsterDamage_Dodged_IncomingDamagePreserved()
    {
        var resolver = new DamageResolver(new FixedRandom(0.0));
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(0, 0, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 33);

        Assert.AreEqual(20, result.IncomingStandardDamage);
        Assert.AreEqual(10, result.IncomingElementalDamage);
        Assert.AreEqual(ElementType.Fire, result.Element);
        Assert.IsTrue(result.IsDodged);
    }

    [TestMethod]
    public void ResolveMonsterDamage_DodgeChance_Zero_NeverDodges()
    {
        var resolver = new DamageResolver(new FixedRandom(0.0));
        var attack = new AttackDamage(20, DamageType.Martial, 0, null);
        var modifiers = new MonsterDamageModifiers(0, 0, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(20, result.FinalStandardDamage);
        Assert.IsFalse(result.IsDodged);
    }

    [TestMethod]
    public void ResolveMonsterDamage_DodgeChance_ClampedTo33()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Martial, 0, null);
        var modifiers = new MonsterDamageModifiers(0, 0, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 100);

        Assert.AreEqual(20, result.FinalStandardDamage);
        Assert.IsFalse(result.IsDodged);
    }

    [TestMethod]
    public void ResolveMonsterDamage_Combined_UsesSingleDodgeRoll()
    {
        var rng = new FixedRandom(0.99);
        var resolver = new DamageResolver(rng);
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(
            5, 0, ElementType.Ice, ElementType.Fire);

        resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(1, rng.CallCount);
    }

    // ── ResolveMonsterDamage: Elemental Truncation ──

    [TestMethod]
    public void ResolveMonsterDamage_Elemental_Resilience_TruncatesDown()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 7, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(
            0, 0, ElementType.Fire, ElementType.Ice);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(3, result.FinalElementalDamage);
    }

    [TestMethod]
    public void ResolveMonsterDamage_Elemental_Vulnerability_TruncatesDown()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 7, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(
            0, 0, ElementType.Ice, ElementType.Fire);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(10, result.FinalElementalDamage);
    }

    // ── ResolveMonsterDamage: Zero Elemental Damage ──

    [TestMethod]
    public void ResolveMonsterDamage_ZeroElemental_SkipsPrimordialCalculation()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Martial, 0, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(
            0, 0, ElementType.Fire, ElementType.Ice);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(20, result.FinalStandardDamage);
        Assert.AreEqual(0, result.FinalElementalDamage);
    }

    // ── ResolveMonsterDamage: Null Resilience/Vulnerability ──

    [TestMethod]
    public void ResolveMonsterDamage_Elemental_NullResilienceAndVulnerability_Neutral()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new MonsterDamageModifiers(0, 0, null, null);

        var result = resolver.ResolveMonsterDamage(attack, modifiers, 0);

        Assert.AreEqual(10, result.FinalElementalDamage);
    }

    // ── ResolvePlayerDamage: Standard Damage Only ──

    [TestMethod]
    public void ResolvePlayerDamage_Martial_NoDefense_ReturnsFullDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Martial, 0, null);
        var modifiers = new PlayerDamageModifiers(0, 0, 0);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(20, result.FinalStandardDamage);
        Assert.AreEqual(0, result.FinalElementalDamage);
        Assert.IsFalse(result.IsDodged);
    }

    [TestMethod]
    public void ResolvePlayerDamage_Magical_WithResistance_ReducesDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Magical, 0, null);
        var modifiers = new PlayerDamageModifiers(0, 10, 0);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(10, result.FinalStandardDamage);
        Assert.IsFalse(result.IsDodged);
    }

    // ── ResolvePlayerDamage: Elemental Damage (Armor Rune Mitigation) ──

    [TestMethod]
    public void ResolvePlayerDamage_Elemental_NoMitigation_ReturnsBaseDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(0, 0, 0);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(10, result.FinalStandardDamage);
        Assert.AreEqual(10, result.FinalElementalDamage);
    }

    [TestMethod]
    public void ResolvePlayerDamage_Elemental_PartialMitigation_SubtractsArmorResilience()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(0, 0, 4);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(6, result.FinalElementalDamage);
    }

    [TestMethod]
    public void ResolvePlayerDamage_Elemental_FullMitigation_ReturnsZero()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(0, 0, 10);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(0, result.FinalElementalDamage);
    }

    [TestMethod]
    public void ResolvePlayerDamage_Elemental_ExcessMitigation_ReturnsZero()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(10, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(0, 0, 15);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(0, result.FinalElementalDamage);
    }

    // ── ResolvePlayerDamage: Combined Standard + Elemental ──

    [TestMethod]
    public void ResolvePlayerDamage_Combined_BothDamagesResolved()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(5, 0, 3);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(15, result.FinalStandardDamage);
        Assert.AreEqual(7, result.FinalElementalDamage);
        Assert.IsFalse(result.IsDodged);
    }

    [TestMethod]
    public void ResolvePlayerDamage_Combined_Dodged_BothZero()
    {
        var resolver = new DamageResolver(new FixedRandom(0.0));
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(5, 0, 3);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 33);

        Assert.AreEqual(0, result.FinalStandardDamage);
        Assert.AreEqual(0, result.FinalElementalDamage);
        Assert.IsTrue(result.IsDodged);
    }

    // ── ResolvePlayerDamage: Dodge ──

    [TestMethod]
    public void ResolvePlayerDamage_Dodged_ZeroStandardAndElementalDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.0));
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(0, 0, 0);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 33);

        Assert.AreEqual(0, result.FinalStandardDamage);
        Assert.AreEqual(0, result.FinalElementalDamage);
        Assert.IsTrue(result.IsDodged);
    }

    [TestMethod]
    public void ResolvePlayerDamage_Dodged_IncomingDamagePreserved()
    {
        var resolver = new DamageResolver(new FixedRandom(0.0));
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(0, 0, 0);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 33);

        Assert.AreEqual(20, result.IncomingStandardDamage);
        Assert.AreEqual(10, result.IncomingElementalDamage);
        Assert.AreEqual(ElementType.Fire, result.Element);
        Assert.IsTrue(result.IsDodged);
    }

    [TestMethod]
    public void ResolvePlayerDamage_Combined_UsesSingleDodgeRoll()
    {
        var rng = new FixedRandom(0.99);
        var resolver = new DamageResolver(rng);
        var attack = new AttackDamage(20, DamageType.Martial, 10, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(5, 0, 3);

        resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(1, rng.CallCount);
    }

    // ── ResolvePlayerDamage: Zero Elemental Damage ──

    [TestMethod]
    public void ResolvePlayerDamage_ZeroElemental_SkipsPrimordialCalculation()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, DamageType.Martial, 0, ElementType.Fire);
        var modifiers = new PlayerDamageModifiers(0, 0, 5);

        var result = resolver.ResolvePlayerDamage(attack, modifiers, 0);

        Assert.AreEqual(20, result.FinalStandardDamage);
        Assert.AreEqual(0, result.FinalElementalDamage);
    }

    // ── ResolveMonsterDamage: Validation ──

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void ResolveMonsterDamage_InvalidPhysicalDamage_Throws(int damage)
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(damage, DamageType.Martial, 0, null);
        var modifiers = new MonsterDamageModifiers(0, 0, null, null);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => resolver.ResolveMonsterDamage(attack, modifiers, 0));
    }

    [TestMethod]
    public void ResolveMonsterDamage_UndefinedDamageType_Throws()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(20, (DamageType)999, 0, null);
        var modifiers = new MonsterDamageModifiers(0, 0, null, null);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => resolver.ResolveMonsterDamage(attack, modifiers, 0));
    }

    // ── ResolvePlayerDamage: Validation ──

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void ResolvePlayerDamage_InvalidPhysicalDamage_Throws(int damage)
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));
        var attack = new AttackDamage(damage, DamageType.Martial, 0, null);
        var modifiers = new PlayerDamageModifiers(0, 0, 0);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => resolver.ResolvePlayerDamage(attack, modifiers, 0));
    }
}
