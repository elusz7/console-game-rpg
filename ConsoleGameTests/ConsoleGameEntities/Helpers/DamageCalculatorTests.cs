using ConsoleGameEntities.Helpers;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGameEntities.Helpers;

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

        var actualDamage = DamageCalculator.CalculateDamage(power, damageType, defense, 0);
        
        Assert.AreEqual(expectedDamage, actualDamage);
    }
    
    [TestMethod]
    public void MagicDamage_UsesResistance()
    {
        var damageType = DamageType.Magical;
        var power = 10;
        var resistance = 5;
        var expectedDamage = 5;

        var actualDamage = DamageCalculator.CalculateDamage(power, damageType, 0, resistance);
        
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

        var actualDamage = DamageCalculator.CalculateDamage(power, damageType, defense, resistance);
        
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

        var actualDamage = DamageCalculator.CalculateDamage(power, damageType, defense, resistance);
        
        Assert.AreEqual(expectedDamage, actualDamage);
    }

    [TestMethod]
    public void Damage_WhenFullyMitigated_ReturnsMinimumOne()
    {
        var damageType = DamageType.Magical;
        var power = 5;
        var resistance = 10;
        var expectedDamage = 1;

        var actualDamage = DamageCalculator.CalculateDamage(power, damageType, 0, resistance);
        
        Assert.AreEqual(expectedDamage, actualDamage);
    }

    [TestMethod]
    public void ZeroDamage_ThrowsArgumentOutOfRangeException()
    {
        var damageType = DamageType.Martial;
        var power = 0;
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => DamageCalculator.CalculateDamage(power, damageType, 0, 0));
    }

    [TestMethod]
    public void NegativeDamage_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => DamageCalculator.CalculateDamage(-10, DamageType.Martial, 0, 0));
    }
    
    [TestMethod]
    public void NullDamageType_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => DamageCalculator.CalculateDamage(10, null, 0, 0));
    }
    
    [TestMethod]
    public void UndefinedDamageType_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => DamageCalculator.CalculateDamage(10, (DamageType)999, 0, 0));
    }
}