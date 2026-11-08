using ConsoleGameEntities.Helpers;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Helpers;

[TestClass]
public class DamageResolverTests
{
    // Provides predictable random values for dodge tests.
    private class FixedRandom(double value) : Random
    {
        public override double NextDouble() => value;
    }

    [TestMethod]
    public void MartialDamage_Success()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));

        var result = resolver.ResolveDamage(
            20, DamageType.Martial, 5, 10, 10);

        Assert.AreEqual(15, result.FinalDamage);
        Assert.IsFalse(result.IsDodged);
        Assert.AreEqual(DamageType.Martial, result.DamageType);
    }

    [TestMethod]
    public void MagicalDamage_Success()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));

        var result = resolver.ResolveDamage(
            20, DamageType.Magical, 5, 7, 10);

        Assert.AreEqual(13, result.FinalDamage);
        Assert.IsFalse(result.IsDodged);
        Assert.AreEqual(DamageType.Magical, result.DamageType);
    }

    [TestMethod]
    public void HybridDamage_Success()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));

        var result = resolver.ResolveDamage(
            20, DamageType.Hybrid, 8, 4, 10);

        Assert.AreEqual(16, result.FinalDamage);
        Assert.IsFalse(result.IsDodged);
        Assert.AreEqual(DamageType.Hybrid, result.DamageType);
    }

    [TestMethod]
    public void DodgeSuccess_ZeroDamage()
    {
        var resolver = new DamageResolver(new FixedRandom(0.0));

        var result = resolver.ResolveDamage(
            20, DamageType.Martial, 5, 10, 20);

        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsTrue(result.IsDodged);
        Assert.IsFalse(result.IsCritical);
    }

    [TestMethod]
    public void CriticalHit_False()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));

        var result = resolver.ResolveDamage(
            20, DamageType.Martial, 5, 10, 10);

        Assert.IsFalse(result.IsCritical);
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void InvalidDamage_Throws(int damage)
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => resolver.ResolveDamage(
                damage, DamageType.Martial, 5, 10, 10));
    }

    [TestMethod]
    public void NullDamageType_Throws()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => resolver.ResolveDamage(
                20, null, 5, 10, 10));
    }

    [TestMethod]
    public void UndefinedDamageType_Throws()
    {
        var resolver = new DamageResolver(new FixedRandom(0.99));

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => resolver.ResolveDamage(
                20, (DamageType)999, 5, 10, 10));
    }
}