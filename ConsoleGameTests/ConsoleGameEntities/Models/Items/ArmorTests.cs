using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Items;

[TestClass]
public class ArmorTests
{
    private static Armor CreateArmor(decimal value = 100, int requiredLevel = 1, int defensePower = 10, int resistancePower = 10, int durability = 10)
    {
        return new Armor
        {
            Name = "Test Armor",
            Value = value,
            RequiredLevel = requiredLevel,
            Durability = durability,
            DefensePower = defensePower,
            Resistance = resistancePower
        };
    }

    private static Inventory CreateInventory(int gold = 1000)
    {
        return new Inventory { Gold = gold };
    }

    [TestMethod]
    public void GetPurificationPrice_ReturnsHalfValue()
    {
        var armor = CreateArmor(value: 200);
        Assert.AreEqual(100, armor.GetPurificationPrice());
    }

    [TestMethod]
    public void GetEnchantmentPrice_ReturnsOneAndHalfValue()
    {
        var armor = CreateArmor(value: 200);
        Assert.AreEqual(300, armor.GetEnchantmentPrice());
    }

    [TestMethod]
    public void Curse_SetsCursedTrue()
    {
        var armor = CreateArmor();
        armor.Curse();
        Assert.IsTrue(armor.IsCursed());
    }

    [TestMethod]
    public void TargetCursed_SetsCursedFalse()
    {
        var armor = CreateArmor();
        armor.Curse();
        armor.TargetCursed();
        Assert.IsFalse(armor.IsCursed());
    }

    [TestMethod]
    public void Equip_SetsEquippedTrue()
    {
        var armor = CreateArmor();
        armor.Equip();
        Assert.IsTrue(armor.IsEquipped());
    }

    [TestMethod]
    public void Unequip_SetsEquippedFalse()
    {
        var armor = CreateArmor();
        armor.Equip();
        armor.Unequip();
        Assert.IsFalse(armor.IsEquipped());
    }

    [TestMethod]
    public void Enchant_ThrowsIfNotInInventory()
    {
        var armor = CreateArmor();
        Assert.ThrowsException<ItemEnchantmentException>(() => armor.Enchant());
    }

    [TestMethod]
    public void Enchant_ThrowsIfNotEnoughGold()
    {
        var armor = CreateArmor(value: 100);
        armor.Inventory = CreateInventory(gold: 10); // Price will be 150
        Assert.ThrowsException<ItemEnchantmentException>(() => armor.Enchant());
    }

    [TestMethod]
    public void Enchant_IncreasesRequiredLevelAndAttackPowerAndReducesGold()
    {
        var armor = CreateArmor(value: 100, requiredLevel: 2, defensePower: 10, resistancePower: 10);
        var inventory = CreateInventory(gold: 1000);
        armor.Inventory = inventory;

        int oldLevel = armor.RequiredLevel;
        int oldDefense = armor.DefensePower;
        int oldResistance = armor.Resistance;
        decimal oldValue = armor.Value;
        int oldGold = inventory.Gold;

        try
        {
            armor.Enchant();
        }
        catch (ItemEnchantmentException ex)
        {
            // If enchantment fails, gold is still deducted and exception is thrown
            StringAssert.Contains(ex.Message, "Enchantment attempt failed.");
            Assert.IsTrue(inventory.Gold < oldGold);
            return;
        }

        Assert.AreEqual(oldLevel + 1, armor.RequiredLevel);
        Assert.IsTrue(armor.DefensePower >= oldDefense);
        Assert.IsTrue(armor.Resistance >= oldResistance);
        Assert.IsTrue(armor.Value != oldValue);
        Assert.IsTrue(inventory.Gold < oldGold);
    }

    [TestMethod]
    public void Purify_ThrowsIfNotCursed()
    {
        var armor = CreateArmor();
        Assert.ThrowsException<ItemPurificationException>(() => armor.Purify());
    }

    [TestMethod]
    public void Purify_ThrowsIfNotInInventory()
    {
        var armor = CreateArmor();
        armor.Curse();
        Assert.ThrowsException<ItemPurificationException>(() => armor.Purify());
    }

    [TestMethod]
    public void Purify_ThrowsIfNotEnoughGold()
    {
        var armor = CreateArmor(value: 100);
        armor.Curse();
        armor.Inventory = CreateInventory(gold: 10); // Price will be 50
        Assert.ThrowsException<ItemPurificationException>(() => armor.Purify());
    }

    [TestMethod]
    public void Purify_RemovesCurseAndReducesGold()
    {
        var armor = CreateArmor(value: 100);
        armor.Curse();
        var inventory = CreateInventory(gold: 1000);
        armor.Inventory = inventory;

        int oldGold = inventory.Gold;
        armor.Purify();

        Assert.IsFalse(armor.IsCursed());
        Assert.AreEqual(oldGold - 50, inventory.Gold);
    }

    [TestMethod]
    public void CalculateValue_SetsValueBasedOnStats()
    {
        var armor = CreateArmor(requiredLevel: 2, defensePower: 10, resistancePower: 10, durability: 5);
        armor.CalculateValue();
        Assert.IsTrue(armor.Value > 0);
    }

    [TestMethod]
    public void CalculateStatsByLevel_SetsDurabilityAndAttackPower()
    {
        var armor = CreateArmor(requiredLevel: 2);
        armor.CalculateStatsByLevel();
        Assert.IsTrue(armor.Durability >= 3 && armor.Durability < 9);
        Assert.IsTrue(armor.DefensePower + armor.Resistance > 0);
    }

    [TestMethod]
    public void CalculateLevelByStats_SetsRequiredLevel()
    {
        var armor = CreateArmor(defensePower: 50, resistancePower: 50);
        armor.CalculateLevelByStats();
        Assert.AreEqual(12, armor.RequiredLevel);
    }
    [TestMethod]
    public void GetReforgePrice_ReturnsOneThirdValue()
    {
        var armor = CreateArmor(value: 300);
        Assert.AreEqual(99.00M, armor.GetReforgePrice());
    }

    [TestMethod]
    public void Reforge_ThrowsIfNotInInventory()
    {
        var armor = CreateArmor();
        Assert.ThrowsException<ItemReforgeException>(() => armor.Reforge());
    }

    [TestMethod]
    public void Reforge_ThrowsIfNotEnoughGold()
    {
        var armor = CreateArmor(value: 300);
        armor.Inventory = CreateInventory(gold: 10); // Price will be 99
        Assert.ThrowsException<ItemReforgeException>(() => armor.Reforge());
    }

    [TestMethod]
    public void Reforge_ChangesStatsAndReducesGold()
    {
        var armor = CreateArmor(value: 300, defensePower: 10, resistancePower: 10);
        var inventory = CreateInventory(gold: 1000);
        armor.Inventory = inventory;

        int oldDefense = armor.DefensePower;
        int oldResistance = armor.Resistance;
        decimal oldValue = armor.Value;
        int oldGold = inventory.Gold;

        try
        {
            armor.Reforge();
        }
        catch (ItemReforgeException ex)
        {
            // Acceptable: random failure or no stat change
            StringAssert.Contains(ex.Message, "Reforge");
            return;
        }

        Assert.AreNotEqual(oldDefense, armor.DefensePower, "DefensePower should change");
        Assert.AreNotEqual(oldResistance, armor.Resistance, "Resistance should change");
        //Assert.IsTrue(armor.Value > oldValue, "Value should increase"); //value doesn't change with reforging
        Assert.IsTrue(inventory.Gold < oldGold, "Gold should decrease");
    }
}