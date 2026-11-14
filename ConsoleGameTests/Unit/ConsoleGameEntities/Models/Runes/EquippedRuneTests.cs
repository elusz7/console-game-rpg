

using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Runes;


[TestClass]
public class EquippedRuneTests
{
    private static EquippedRune CreateEquippedRune(RarityLevel rarity)
    {
        var rune = new WeaponRune
        {
            Name = "Test Rune",
            Element = ElementType.Fire,
            Power = 2,
            Rarity = rarity,
            Duration = 3
        };

        var inventoryRune = new InventoryRune
        {
            InventoryId = 1,
            Rune = rune,
            Tier = 1,
            Quantity = 1
        };

        return new EquippedRune
        {
            Id = 1,
            Tier = 1,
            InventoryRune = inventoryRune
        };
    }
    
    [TestMethod]
    public void ApplyStatusEffect_CommonRarity_SucceedsBelowProbabilityThreshold()
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Common);

        var result = equippedRune.ApplyStatusEffect(0.0099);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ApplyStatusEffect_CommonRarity_FailsAtProbabilityThreshold()
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Common);

        var result = equippedRune.ApplyStatusEffect(0.01);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ApplyStatusEffect_MythicRarity_SucceedsBelowProbabilityThreshold()
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Mythic);

        var result = equippedRune.ApplyStatusEffect(0.3499);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ApplyStatusEffect_MythicRarity_FailsAtProbabilityThreshold()
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Mythic);

        var result = equippedRune.ApplyStatusEffect(0.35);

        Assert.IsFalse(result);
    }

    [TestMethod]
    [DataRow(RarityLevel.Common, 0.01)]
    [DataRow(RarityLevel.Uncommon, 0.03)]
    [DataRow(RarityLevel.Rare, 0.08)]
    [DataRow(RarityLevel.Epic, 0.12)]
    [DataRow(RarityLevel.Legendary, 0.20)]
    [DataRow(RarityLevel.Mythic, 0.35)]
    public void ApplyStatusEffect_EachRarity_UsesExpectedProbability(
        RarityLevel rarity,
        double threshold)
    {
        var equippedRune = CreateEquippedRune(rarity);

        Assert.IsTrue(equippedRune.ApplyStatusEffect(threshold - 0.0001));
        Assert.IsFalse(equippedRune.ApplyStatusEffect(threshold));
    }

    [TestMethod]
    [DataRow(-0.01)]
    [DataRow(1.0)]
    [DataRow(1.5)]
    public void ApplyStatusEffect_InvalidRoll_ThrowsArgumentOutOfRangeException(
        double roll)
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Common);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => equippedRune.ApplyStatusEffect(roll));
    }
    
    [TestMethod]
    [DataRow(RarityLevel.Common, 1, 1)]
    [DataRow(RarityLevel.Common, 3, 3)]
    [DataRow(RarityLevel.Uncommon, 1, 2)]
    [DataRow(RarityLevel.Rare, 2, 4)]
    [DataRow(RarityLevel.Epic, 3, 6)]
    [DataRow(RarityLevel.Legendary, 2, 6)]
    [DataRow(RarityLevel.Mythic, 3, 8)]
    public void GetStatusEffectPower_UsesRarityAndTier(
        RarityLevel rarity,
        int tier,
        int expectedPower)
    {
        var equippedRune = CreateEquippedRune(rarity);
        equippedRune.Tier = tier;

        Assert.AreEqual(expectedPower, equippedRune.GetStatusEffectPower());
    }
    
    [TestMethod]
    [DataRow(2, 1, 1)]
    [DataRow(2, 2, 2)]
    [DataRow(2, 3, 3)]
    [DataRow(3, 1, 2)]
    [DataRow(3, 2, 3)]
    [DataRow(3, 3, 5)]
    [DataRow(10, 3, 15)]
    public void GetDoTDamage_UsesHalfElementalPowerRoundedUp(
        int runePower,
        int tier,
        int expectedDamage)
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Common);
        equippedRune.InventoryRune.Rune.Power = runePower;
        equippedRune.Tier = tier;

        var result = equippedRune.GetDoTDamage();

        Assert.AreEqual(expectedDamage, result.dotDamage);
    }
    
    [TestMethod]
    public void GetDoTDamage_NeverReturnsLessThanOne()
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Common);
        equippedRune.InventoryRune.Rune.Power = 0;

        var result = equippedRune.GetDoTDamage();

        Assert.AreEqual(1, result.dotDamage);
    }
    
    [TestMethod]
    [DataRow(ElementType.Fire, ElementDamageType.Burning)]
    [DataRow(ElementType.Lightning, ElementDamageType.Static)]
    [DataRow(ElementType.Ice, ElementDamageType.Frostbite)]
    [DataRow(ElementType.Nature, ElementDamageType.Poisoned)]
    [DataRow(ElementType.Radiance, ElementDamageType.Weakened)]
    [DataRow(ElementType.Abyssal, ElementDamageType.Despair)]
    public void GetDoTDamage_UsesExpectedDamageType(
        ElementType element,
        ElementDamageType expectedType)
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Common);
        ((WeaponRune)equippedRune.InventoryRune.Rune).Element = element;

        var result = equippedRune.GetDoTDamage();

        Assert.AreEqual(expectedType, result.dotDamageType);
    }
    
    [TestMethod]
    [DataRow(ElementType.Fire, ElementalStatusEffectType.Charred)]
    [DataRow(ElementType.Lightning, ElementalStatusEffectType.Shocked)]
    [DataRow(ElementType.Ice, ElementalStatusEffectType.Frozen)]
    [DataRow(ElementType.Nature, ElementalStatusEffectType.Snared)]
    [DataRow(ElementType.Radiance, ElementalStatusEffectType.Blinded)]
    [DataRow(ElementType.Abyssal, ElementalStatusEffectType.Corrupted)]
    public void GetElementalStatusEffect_ReturnsExpectedStatus(
        ElementType element,
        ElementalStatusEffectType expectedStatus)
    {
        var equippedRune = CreateEquippedRune(RarityLevel.Common);
        ((WeaponRune)equippedRune.InventoryRune.Rune).Element = element;

        Assert.AreEqual(expectedStatus, equippedRune.GetElementalStatusEffect());
    }
}