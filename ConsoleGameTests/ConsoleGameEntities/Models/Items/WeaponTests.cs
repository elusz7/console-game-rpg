using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Items;

[TestClass]
public class WeaponTests
{
    private Weapon CreateWeapon(decimal value = 100, int requiredLevel = 1, int attackPower = 10, int durability = 10)
    {
        return new Weapon
        {
            Name = "Test Weapon",
            Value = value,
            RequiredLevel = requiredLevel,
            AttackPower = attackPower,
            Durability = durability
        };
    }

    private Inventory CreateInventory(int gold = 1000)
    {
        return new Inventory { Gold = gold };
    }

    [TestMethod]
    public void GetPurificationPrice_ReturnsHalfValue()
    {
        var weapon = CreateWeapon(value: 200);
        Assert.AreEqual(100, weapon.GetPurificationPrice());
    }

    [TestMethod]
    public void GetEnchantmentPrice_ReturnsOneAndHalfValue()
    {
        var weapon = CreateWeapon(value: 200);
        Assert.AreEqual(300, weapon.GetEnchantmentPrice());
    }

    [TestMethod]
    public void Curse_SetsCursedTrue()
    {
        var weapon = CreateWeapon();
        weapon.Curse();
        Assert.IsTrue(weapon.IsCursed());
    }

    [TestMethod]
    public void TargetCursed_SetsCursedFalse()
    {
        var weapon = CreateWeapon();
        weapon.Curse();
        weapon.TargetCursed();
        Assert.IsFalse(weapon.IsCursed());
    }

    [TestMethod]
    public void Equip_SetsEquippedTrue()
    {
        var weapon = CreateWeapon();
        weapon.Equip();
        Assert.IsTrue(weapon.IsEquipped());
    }

    [TestMethod]
    public void Unequip_SetsEquippedFalse()
    {
        var weapon = CreateWeapon();
        weapon.Equip();
        weapon.Unequip();
        Assert.IsFalse(weapon.IsEquipped());
    }

    [TestMethod]
    public void Enchant_ThrowsIfNotInInventory()
    {
        var weapon = CreateWeapon();
        Assert.ThrowsException<ItemEnchantmentException>(() => weapon.Enchant());
    }

    [TestMethod]
    public void Enchant_ThrowsIfNotEnoughGold()
    {
        var weapon = CreateWeapon(value: 100);
        weapon.Inventory = CreateInventory(gold: 10); // Price will be 150
        Assert.ThrowsException<ItemEnchantmentException>(() => weapon.Enchant());
    }

    [TestMethod]
    public void Enchant_IncreasesRequiredLevelAndAttackPowerAndReducesGold()
    {
        var weapon = CreateWeapon(value: 100, requiredLevel: 2, attackPower: 10);
        var inventory = CreateInventory(gold: 1000);
        weapon.Inventory = inventory;

        int oldLevel = weapon.RequiredLevel;
        int oldAttack = weapon.AttackPower;
        decimal oldValue = weapon.Value;
        int oldGold = inventory.Gold;

        try
        {
            weapon.Enchant();
        }
        catch (ItemEnchantmentException ex)
        {
            // If enchantment fails, gold is still deducted and exception is thrown
            StringAssert.Contains(ex.Message, "Enchantment attempt failed.");
            Assert.IsTrue(inventory.Gold < oldGold);
            return;
        }

        Assert.AreEqual(oldLevel + 1, weapon.RequiredLevel);
        Assert.IsTrue(weapon.AttackPower > oldAttack);
        Assert.IsTrue(weapon.Value != oldValue);
        Assert.IsTrue(inventory.Gold < oldGold);
    }

    [TestMethod]
    public void Purify_ThrowsIfNotCursed()
    {
        var weapon = CreateWeapon();
        Assert.ThrowsException<ItemPurificationException>(() => weapon.Purify());
    }

    [TestMethod]
    public void Purify_ThrowsIfNotInInventory()
    {
        var weapon = CreateWeapon();
        weapon.Curse();
        Assert.ThrowsException<ItemPurificationException>(() => weapon.Purify());
    }

    [TestMethod]
    public void Purify_ThrowsIfNotEnoughGold()
    {
        var weapon = CreateWeapon(value: 100);
        weapon.Curse();
        weapon.Inventory = CreateInventory(gold: 10); // Price will be 50
        Assert.ThrowsException<ItemPurificationException>(() => weapon.Purify());
    }

    [TestMethod]
    public void Purify_RemovesCurseAndReducesGold()
    {
        var weapon = CreateWeapon(value: 100);
        weapon.Curse();
        var inventory = CreateInventory(gold: 1000);
        weapon.Inventory = inventory;

        int oldGold = inventory.Gold;
        weapon.Purify();

        Assert.IsFalse(weapon.IsCursed());
        Assert.AreEqual(oldGold - 50, inventory.Gold);
    }

    [TestMethod]
    public void CalculateValue_SetsValueBasedOnStats()
    {
        var weapon = CreateWeapon(requiredLevel: 2, attackPower: 10, durability: 5);
        weapon.CalculateValue();
        Assert.IsTrue(weapon.Value > 0);
    }

    [TestMethod]
    public void CalculateStatsByLevel_SetsDurabilityAndAttackPower()
    {
        var weapon = CreateWeapon(requiredLevel: 2);
        weapon.CalculateStatsByLevel();
        Assert.IsTrue(weapon.Durability >= 3 && weapon.Durability < 9);
        Assert.IsTrue(weapon.AttackPower >= weapon.RequiredLevel * 6);
    }

    [TestMethod]
    public void CalculateLevelByStats_SetsRequiredLevel()
    {
        var weapon = CreateWeapon(attackPower: 95);
        weapon.CalculateLevelByStats();
        Assert.AreEqual(10, weapon.RequiredLevel);
    }
}