using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Items.Weapons;

[TestClass]
public class WeaponEquippedRuneTests
{
    private static Weapon CreateWeapon(EquippedRune? equippedRune = null)
    {
        return new Weapon
        {
            Name = "Test Weapon",
            AttackPower = 10,
            EquippedRune = equippedRune
        };
    }

    private static EquippedRune CreateEquippedRune(
        ElementType element = ElementType.Fire,
        RarityLevel rarity = RarityLevel.Rare,
        int power = 4,
        int tier = 2,
        int duration = 3)
    {
        var rune = new WeaponRune
        {
            Name = $"Test {element} Rune",
            Element = element,
            Rarity = rarity,
            Power = power,
            Duration = duration
        };

        var inventoryRune = new InventoryRune
        {
            InventoryId = 1,
            Rune = rune,
            Tier = tier,
            Quantity = 1
        };

        return new EquippedRune
        {
            Id = 1,
            Tier = tier,
            InventoryRune = inventoryRune
        };
    }
    
    [TestMethod]
    public void GetEquippedRuneElementalPower_NoRuneEquipped_ReturnsZero()
    {
        var weapon = CreateWeapon();

        Assert.AreEqual(0, weapon.GetEquippedRuneElementalPower());
    }

    [TestMethod]
    public void GetEquippedRuneElementalPower_WithRune_ReturnsExpectedPower()
    {
        var equippedRune = CreateEquippedRune(power: 4, tier: 2);
        var weapon = CreateWeapon(equippedRune);

        Assert.AreEqual(8, weapon.GetEquippedRuneElementalPower());
    }
    
    [TestMethod]
    public void ApplyEquippedRuneStatusEffect_NoRuneEquipped_ReturnsFalse()
    {
        var weapon = CreateWeapon();

        Assert.IsFalse(weapon.ApplyEquippedRuneStatusEffect());
    }
    
    [TestMethod]
    public void GetEquippedRuneElementalStatusEffect_NoRuneEquipped_ThrowsEquipmentException()
    {
        var weapon = CreateWeapon();

        Assert.ThrowsException<EquipmentException>(
            () => weapon.GetEquippedRuneElementalStatusEffect());
    }

    [TestMethod]
    [DataRow(ElementType.Fire, ElementalStatusEffectType.Charred)]
    [DataRow(ElementType.Lightning, ElementalStatusEffectType.Shocked)]
    [DataRow(ElementType.Ice, ElementalStatusEffectType.Frozen)]
    [DataRow(ElementType.Nature, ElementalStatusEffectType.Snared)]
    [DataRow(ElementType.Radiance, ElementalStatusEffectType.Blinded)]
    [DataRow(ElementType.Abyssal, ElementalStatusEffectType.Corrupted)]
    public void GetEquippedRuneElementalStatusEffect_WithRune_ReturnsExpectedStatus(
        ElementType element,
        ElementalStatusEffectType expectedStatus)
    {
        var weapon = CreateWeapon(CreateEquippedRune(element));

        Assert.AreEqual(expectedStatus, weapon.GetEquippedRuneElementalStatusEffect());
    }
    
    [TestMethod]
    public void GetEquippedRuneStatusEffectPower_NoRuneEquipped_ThrowsEquipmentException()
    {
        var weapon = CreateWeapon();

        Assert.ThrowsException<EquipmentException>(
            () => weapon.GetEquippedRuneStatusEffectPower());
    }

    [TestMethod]
    [DataRow(RarityLevel.Common, 1, 1)]
    [DataRow(RarityLevel.Uncommon, 1, 2)]
    [DataRow(RarityLevel.Rare, 2, 4)]
    [DataRow(RarityLevel.Epic, 3, 6)]
    [DataRow(RarityLevel.Legendary, 2, 6)]
    [DataRow(RarityLevel.Mythic, 3, 8)]
    public void GetEquippedRuneStatusEffectPower_WithRune_ReturnsExpectedPower(
        RarityLevel rarity,
        int tier,
        int expectedPower)
    {
        var weapon = CreateWeapon(CreateEquippedRune(rarity: rarity, tier: tier));

        Assert.AreEqual(expectedPower, weapon.GetEquippedRuneStatusEffectPower());
    }
    
    [TestMethod]
    public void GetEquippedRune_NoRuneEquipped_ThrowsEquipmentException()
    {
        var weapon = CreateWeapon();

        Assert.ThrowsException<EquipmentException>(
            () => weapon.GetEquippedRune());
    }

    [TestMethod]
    public void GetEquippedRune_WithRune_ReturnsEquippedWeaponRune()
    {
        var equippedRune = CreateEquippedRune();
        var weapon = CreateWeapon(equippedRune);

        var result = weapon.GetEquippedRune();

        Assert.AreSame(equippedRune.InventoryRune.Rune, result);
    }
    
    [TestMethod]
    public void GetEquippedRuneDoT_NoRuneEquipped_ThrowsEquipmentException()
    {
        var weapon = CreateWeapon();

        Assert.ThrowsException<EquipmentException>(
            () => weapon.GetEquippedRuneDoT());
    }

    [TestMethod]
    [DataRow(2, 1, 1)]
    [DataRow(2, 2, 2)]
    [DataRow(3, 2, 3)]
    [DataRow(4, 2, 4)]
    [DataRow(10, 3, 15)]
    public void GetEquippedRuneDoT_WithRune_ReturnsExpectedDamage(
        int power,
        int tier,
        int expectedDamage)
    {
        var weapon = CreateWeapon(CreateEquippedRune(power: power, tier: tier));

        var result = weapon.GetEquippedRuneDoT();

        Assert.AreEqual(expectedDamage, result.dotDamage);
    }

    [TestMethod]
    [DataRow(ElementType.Fire, ElementDamageType.Burning)]
    [DataRow(ElementType.Lightning, ElementDamageType.Static)]
    [DataRow(ElementType.Ice, ElementDamageType.Frostbite)]
    [DataRow(ElementType.Nature, ElementDamageType.Poisoned)]
    [DataRow(ElementType.Radiance, ElementDamageType.Weakened)]
    [DataRow(ElementType.Abyssal, ElementDamageType.Despair)]
    public void GetEquippedRuneDoT_WithRune_ReturnsExpectedDamageType(ElementType element, ElementDamageType expectedDamageType)
    {
        var weapon = CreateWeapon(CreateEquippedRune(element));

        var result = weapon.GetEquippedRuneDoT();

        Assert.AreEqual(expectedDamageType, result.dotDamageType);
    }
    
    
}