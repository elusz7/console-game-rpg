using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Models.Combat;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Helpers;

[TestClass]
public class DamageCalculatorTests
{
    [TestMethod]
    public void MartialDamage_UsesDefense()
    {
        var damageType = DamageType.Martial;
        var power = 10;
        var defense = 5;
        var expectedDamage = 5;

        var actualDamage = DamageCalculator.CalculateStandardDamage(power, damageType, defense, 0);
        
        Assert.AreEqual(expectedDamage, actualDamage);
    }
    
    [TestMethod]
    public void MagicDamage_UsesResistance()
    {
        var damageType = DamageType.Magical;
        var power = 10;
        var resistance = 5;
        var expectedDamage = 5;

        var actualDamage = DamageCalculator.CalculateStandardDamage(power, damageType, 0, resistance);
        
        Assert.AreEqual(expectedDamage, actualDamage);
    }

    [TestMethod]
    public void HybridDamage_DefenseHigher_UsesHalfDefense()
    {
        var damageType = DamageType.Hybrid;
        var power = 10;
        var defense = 10;
        var resistance = 5;
        var expectedDamage = 5;

        var actualDamage = DamageCalculator.CalculateStandardDamage(power, damageType, defense, resistance);
        
        Assert.AreEqual(expectedDamage, actualDamage);
    }

    [TestMethod]
    public void HybridDamage_ResistanceHigher_UsesHalfResistance()
    {
        var damageType = DamageType.Hybrid;
        var power = 10;
        var defense = 5;
        var resistance = 10;
        var expectedDamage = 5;

        var actualDamage = DamageCalculator.CalculateStandardDamage(power, damageType, defense, resistance);
        
        Assert.AreEqual(expectedDamage, actualDamage);
    }

    [TestMethod]
    public void Damage_WhenFullyMitigated_ReturnsMinimumOne()
    {
        var damageType = DamageType.Magical;
        var power = 5;
        var resistance = 10;
        var expectedDamage = 1;

        var actualDamage = DamageCalculator.CalculateStandardDamage(power, damageType, 0, resistance);
        
        Assert.AreEqual(expectedDamage, actualDamage);
    }

    [TestMethod]
    public void ZeroDamage_ThrowsArgumentOutOfRangeException()
    {
        var attack = new AttackDamage(0, DamageType.Magical, 0, null);
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => DamageCalculator.ValidateInputs(attack));
    }

    [TestMethod]
    public void NegativeDamage_ThrowsArgumentOutOfRangeException()
    {
        var attack = new AttackDamage(-10, DamageType.Magical, 0, null);
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => DamageCalculator.ValidateInputs(attack));
    }
    
    [TestMethod]
    public void UndefinedDamageType_ThrowsArgumentOutOfRangeException()
    {
        var attack = new AttackDamage(10, (DamageType)999, 0, null);
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => DamageCalculator.ValidateInputs(attack));
    }

    // ── Monster Primordial Damage: Neutral (1.0×) ──
    [DataTestMethod]
    [DataRow(ElementType.Fire, ElementType.Ice, ElementType.Nature)]
    [DataRow(ElementType.Lightning, ElementType.Ice, ElementType.Nature)]
    [DataRow(ElementType.Ice, ElementType.Fire, ElementType.Nature)]
    [DataRow(ElementType.Nature, ElementType.Fire, ElementType.Ice)]
    [DataRow(ElementType.Radiance, ElementType.Fire, ElementType.Ice)]
    [DataRow(ElementType.Abyssal, ElementType.Fire, ElementType.Ice)]
    public void MonsterPrimordial_Neutral_ReturnsBaseDamage(
        ElementType incoming,
        ElementType resilience,
        ElementType vulnerability)
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(
            10, incoming, resilience, vulnerability);

        Assert.AreEqual(10, actual);
    }

    // ── Monster Primordial Damage: Resilience (0.5×) ──
    [DataTestMethod]
    [DataRow(ElementType.Fire, ElementType.Ice)]
    [DataRow(ElementType.Lightning, ElementType.Ice)]
    [DataRow(ElementType.Ice, ElementType.Fire)]
    [DataRow(ElementType.Nature, ElementType.Fire)]
    [DataRow(ElementType.Radiance, ElementType.Fire)]
    [DataRow(ElementType.Abyssal, ElementType.Fire)]
    public void MonsterPrimordial_Resilient_AppliesHalfMultiplier(
        ElementType incoming,
        ElementType vulnerability)
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(
            10, incoming, incoming, vulnerability);

        Assert.AreEqual(5, actual);
    }

    // ── Monster Primordial Damage: Vulnerability (1.5×) ──
    [DataTestMethod]
    [DataRow(ElementType.Fire, ElementType.Ice)]
    [DataRow(ElementType.Lightning, ElementType.Ice)]
    [DataRow(ElementType.Ice, ElementType.Fire)]
    [DataRow(ElementType.Nature, ElementType.Fire)]
    [DataRow(ElementType.Radiance, ElementType.Fire)]
    [DataRow(ElementType.Abyssal, ElementType.Fire)]
    public void MonsterPrimordial_Vulnerable_AppliesOnePointFiveMultiplier(
        ElementType incoming,
        ElementType resilience)
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(
            10, incoming, resilience, incoming);

        Assert.AreEqual(15, actual);
    }

    // ── Monster Primordial Damage: Integer Truncation ──

    [TestMethod]
    public void MonsterPrimordial_Resilience_TruncatesDown()
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(7, ElementType.Fire, ElementType.Fire, ElementType.Ice);
        Assert.AreEqual(3, actual);
    }

    [TestMethod]
    public void MonsterPrimordial_Vulnerability_TruncatesDown()
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(7, ElementType.Fire, ElementType.Ice, ElementType.Fire);
        Assert.AreEqual(10, actual);
    }

    // ── Monster Primordial Damage: Zero Damage Edge Cases ──

    [TestMethod]
    public void MonsterPrimordial_ZeroIncomingDamage_ReturnsZero()
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(0, ElementType.Fire, ElementType.Fire, ElementType.Ice);
        Assert.AreEqual(0, actual);
    }

    [TestMethod]
    public void MonsterPrimordial_ResilienceOneDamage_ReturnsZero()
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(1, ElementType.Fire, ElementType.Fire, ElementType.Ice);
        Assert.AreEqual(0, actual);
    }

    // ── Monster Primordial Damage: Null Resilience/Vulnerability ──

    [TestMethod]
    public void MonsterPrimordial_NullResilienceAndVulnerability_ReturnsBaseDamage()
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(10, ElementType.Fire, null, null);
        Assert.AreEqual(10, actual);
    }

    [TestMethod]
    public void MonsterPrimordial_NullResilience_AppliesVulnerability()
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(10, ElementType.Fire, null, ElementType.Fire);
        Assert.AreEqual(15, actual);
    }

    [TestMethod]
    public void MonsterPrimordial_NullVulnerability_AppliesResilience()
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(10, ElementType.Fire, ElementType.Fire, null);
        Assert.AreEqual(5, actual);
    }

    // ── Monster Primordial Damage: Resilience takes priority over Vulnerability ──

    [TestMethod]
    public void MonsterPrimordial_ResilienceMatchesIncoming_TakesPriorityOverVulnerability()
    {
        var actual = DamageCalculator.CalculateMonsterPrimordialDamage(10, ElementType.Fire, ElementType.Fire, ElementType.Ice);
        Assert.AreEqual(5, actual);
    }

    // ── Player Primordial Damage: No Mitigation ──

    [TestMethod]
    public void PlayerPrimordial_ZeroResilience_ReturnsBaseDamage()
    {
        var actual = DamageCalculator.CalculatePlayerPrimordialDamage(10, 0);
        Assert.AreEqual(10, actual);
    }

    // ── Player Primordial Damage: Partial Mitigation ──

    [TestMethod]
    public void PlayerPrimordial_PartialMitigation_SubtractsResilience()
    {
        var actual = DamageCalculator.CalculatePlayerPrimordialDamage(10, 4);
        Assert.AreEqual(6, actual);
    }

    // ── Player Primordial Damage: Full Mitigation ──

    [TestMethod]
    public void PlayerPrimordial_FullMitigation_ReturnsZero()
    {
        var actual = DamageCalculator.CalculatePlayerPrimordialDamage(10, 10);
        Assert.AreEqual(0, actual);
    }

    [TestMethod]
    public void PlayerPrimordial_ExcessMitigation_ReturnsZero()
    {
        var actual = DamageCalculator.CalculatePlayerPrimordialDamage(10, 15);
        Assert.AreEqual(0, actual);
    }

    // ── Player Primordial Damage: Zero Incoming ──

    [TestMethod]
    public void PlayerPrimordial_ZeroIncomingDamage_ReturnsZero()
    {
        var actual = DamageCalculator.CalculatePlayerPrimordialDamage(0, 5);
        Assert.AreEqual(0, actual);
    }
}