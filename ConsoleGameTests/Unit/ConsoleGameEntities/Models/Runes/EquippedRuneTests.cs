

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
}