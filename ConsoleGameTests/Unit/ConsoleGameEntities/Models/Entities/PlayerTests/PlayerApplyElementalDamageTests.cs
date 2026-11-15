using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Runes;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Entities.PlayerTests;

[TestClass]
public class PlayerApplyElementalDamageTests
{
    private static Player CreatePlayer()
    {
        var player = new Player
        {
            CurrentHealth = 100,
            MaxHealth = 100,
            DodgeChance = 0
        };

        var archetype = new Archetype
        {
            Name = "Test",
            ArchetypeType = ArchetypeType.Martial,
            AttackMultiplier = 1.5m,
            MagicMultiplier = 1.2m,
            DefenseMultiplier = 1.0m,
            ResistanceMultiplier = 0.5m,
            SpeedMultiplier = 2.0m,
            ResourceMultiplier = 3.0m,
            ResourceName = "Energy",
            RecoveryRate = 2,
            RecoveryGrowth = 1,
            HealthBase = 10,
            MaxResource = 10,
            CurrentResource = 10
        };

        var inventory = new Inventory
        {
            Id = 1,
            Gold = 100,
            Capacity = 36.2M,
            Items = []
        };

        player.Archetype = archetype;
        player.Inventory = inventory;
        inventory.Player = player;

        return player;
    }

    private static Armor CreateArmor()
    {
        return new Armor
        {
            Name = "Test Armor",
            Value = 10.5M,
            RequiredLevel = 1,
            Durability = 3,
            DefensePower = 10,
            Resistance = 10,
            ArmorType = ArmorType.Head
        };
    }
    
    private static Armor CreateArmorWithRune(ElementType element, int tier, int power)
    {
        var armorRune = new ArmorRune
        {
            Id = 1,
            Name = $"Rune of {element}",
            Element = element,
            Power = power,
            Rarity = RarityLevel.Common
        };

        var inventoryRune = new InventoryRune
        {
            InventoryId = 1,
            Rune = armorRune,
            Tier = tier,
            Quantity = 1
        };

        var equippedRune = new EquippedRune
        {
            Id = 1,
            Tier = tier,
            InventoryRune = inventoryRune
        };

        inventoryRune.EquippedRunes.Add(equippedRune);

        var armor = CreateArmor();
        armor.EquippedRune = equippedRune;

        return armor;
    }
        
    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — basic damage without resilience
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_NoResilience_DealsFullDamage()
    {
        var player = CreatePlayer();

        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, player.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.AreEqual(ElementType.Fire, result.Element);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_NoResilience_ReturnsCorrectElementType()
    {
        var player = CreatePlayer();

        var result = player.ApplyElementalDamage(5, ElementType.Ice);

        Assert.AreEqual(ElementType.Ice, result.Element);
    }

    [TestMethod]
    public void ApplyElementalDamage_ZeroDamage_DealsNoDamage()
    {
        var player = CreatePlayer();

        var result = player.ApplyElementalDamage(0, ElementType.Fire);

        Assert.AreEqual(100, player.CurrentHealth);
        Assert.AreEqual(0, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — resilience reduction
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_WithMatchingRune_ResilienceReducesDamage()
    {
        var player = CreatePlayer();

        var armor = CreateArmorWithRune(ElementType.Fire, tier: 2, power: 5);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        // Resilience = tier * power = 2 * 5 = 10
        var result = player.ApplyElementalDamage(15, ElementType.Fire);

        Assert.AreEqual(95, player.CurrentHealth);
        Assert.AreEqual(15, result.InitialDamage);
        Assert.AreEqual(5, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResilienceEqualsDamage_DealsZeroDamage()
    {
        var player = CreatePlayer();

        var armor = CreateArmorWithRune(ElementType.Fire, tier: 2, power: 5);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        // Resilience = 2 * 5 = 10, damage = 10
        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(100, player.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResilienceExceedsDamage_DealsZeroDamage()
    {
        var player = CreatePlayer();

        var armor = CreateArmorWithRune(ElementType.Fire, tier: 3, power: 10);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        // Resilience = 3 * 10 = 30, damage = 10
        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(100, player.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_MismatchedElementRune_NoResilienceApplied()
    {
        var player = CreatePlayer();

        var armor = CreateArmorWithRune(ElementType.Ice, tier: 2, power: 5);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        // Ice rune does not provide resilience against Fire
        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, player.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_MultipleArmorPieces_SumsResilience()
    {
        var player = CreatePlayer();

        var headArmor = CreateArmorWithRune(ElementType.Fire, tier: 2, power: 5);
        headArmor.ArmorType = ArmorType.Head;
        headArmor.RequiredLevel = player.Level;
        player.Inventory.AddItem(headArmor);
        player.Equip(headArmor);

        var chestArmor = CreateArmorWithRune(ElementType.Fire, tier: 1, power: 3);
        chestArmor.ArmorType = ArmorType.Chest;
        chestArmor.RequiredLevel = player.Level;
        player.Inventory.AddItem(chestArmor);
        player.Equip(chestArmor);

        // Resilience = (2*5) + (1*3) = 13
        var result = player.ApplyElementalDamage(20, ElementType.Fire);

        Assert.AreEqual(93, player.CurrentHealth);
        Assert.AreEqual(20, result.InitialDamage);
        Assert.AreEqual(7, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_MultipleArmorPieces_MixedElements_SumsMatchingOnly()
    {
        var player = CreatePlayer();

        var headArmor = CreateArmorWithRune(ElementType.Fire, tier: 2, power: 5);
        headArmor.ArmorType = ArmorType.Head;
        headArmor.RequiredLevel = player.Level;
        player.Inventory.AddItem(headArmor);
        player.Equip(headArmor);

        var chestArmor = CreateArmorWithRune(ElementType.Ice, tier: 2, power: 5);
        chestArmor.ArmorType = ArmorType.Chest;
        chestArmor.RequiredLevel = player.Level;
        player.Inventory.AddItem(chestArmor);
        player.Equip(chestArmor);

        // Only Fire rune matches: resilience = 2*5 = 10
        var result = player.ApplyElementalDamage(15, ElementType.Fire);

        Assert.AreEqual(95, player.CurrentHealth);
        Assert.AreEqual(15, result.InitialDamage);
        Assert.AreEqual(5, result.FinalDamage);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — death cases
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_DamageEqualsHealth_TargetDies()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 10;

        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(0, player.CurrentHealth);
        Assert.IsTrue(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_DamageExceedsHealth_TargetDies()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 10;

        var result = player.ApplyElementalDamage(20, ElementType.Fire);

        Assert.AreEqual(-10, player.CurrentHealth);
        Assert.IsTrue(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResiliencePreventsDeath()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 5;

        var armor = CreateArmorWithRune(ElementType.Fire, tier: 2, power: 10);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        // Resilience = 20, damage = 15, actual damage = 0
        var result = player.ApplyElementalDamage(15, ElementType.Fire);

        Assert.AreEqual(5, player.CurrentHealth);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResilienceReducesButStillKills()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 5;

        var armor = CreateArmorWithRune(ElementType.Fire, tier: 1, power: 3);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        // Resilience = 3, damage = 10, actual damage = 7
        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(-2, player.CurrentHealth);
        Assert.AreEqual(7, result.FinalDamage);
        Assert.IsTrue(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — all elements
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_AllElements_ReturnsCorrectElement()
    {
        var player = CreatePlayer();
        var elements = new[]
        {
            ElementType.Fire,
            ElementType.Lightning,
            ElementType.Ice,
            ElementType.Nature,
            ElementType.Radiance,
            ElementType.Abyssal
        };

        foreach (var element in elements)
        {
            var result = player.ApplyElementalDamage(1, element);
            Assert.AreEqual(element, result.Element, $"Element {element} not returned correctly.");
        }
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — action item
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_AddsActionItemWithElementTypeName()
    {
        var player = CreatePlayer();

        player.ApplyElementalDamage(10, ElementType.Fire);

        var actionItem = player.ActionItems.Values.Last();
        Assert.IsTrue(actionItem.Contains("Burning"), $"Expected action item to contain 'Burning', but was: {actionItem}");
        Assert.IsTrue(actionItem.Contains("drains your vitality"), $"Expected action item to contain 'drains your vitality', but was: {actionItem}");
    }

    [TestMethod]
    public void ApplyElementalDamage_ActionItemIncludesCurrentHealth()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 50;

        player.ApplyElementalDamage(10, ElementType.Fire);

        var actionItem = player.ActionItems.Values.Last();
        Assert.IsTrue(actionItem.Contains("[40 / 100]"), $"Expected action item to contain '[40 / 100]', but was: {actionItem}");
    }

    [TestMethod]
    public void ApplyElementalDamage_DifferentElements_HaveCorrectActionItemNames()
    {
        var player = CreatePlayer();

        var tests = new (ElementType Element, string ExpectedName)[]
        {
            (ElementType.Fire, "Burning"),
            (ElementType.Lightning, "Static"),
            (ElementType.Ice, "Frostbite"),
            (ElementType.Nature, "Poisoned"),
            (ElementType.Radiance, "Weakened"),
            (ElementType.Abyssal, "Despair")
        };

        foreach (var (element, expectedName) in tests)
        {
            player.ClearActionItems();
            player.ApplyElementalDamage(1, element);
            var actionItem = player.ActionItems.Values.Last();
            Assert.IsTrue(actionItem.Contains(expectedName),
                $"Expected action item for {element} to contain '{expectedName}', but was: {actionItem}");
        }
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — return value contract
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_ReturnValue_PreservesInitialDamage()
    {
        var player = CreatePlayer();

        var armor = CreateArmorWithRune(ElementType.Fire, tier: 1, power: 3);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(7, result.FinalDamage);
        Assert.AreEqual(ElementType.Fire, result.Element);
    }

    [TestMethod]
    public void ApplyElementalDamage_NoEquippedArmor_ReturnsZeroResilience()
    {
        var player = CreatePlayer();

        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
    }

    [TestMethod]
    public void ApplyElementalDamage_EquippedArmorWithoutRune_ReturnsZeroResilience()
    {
        var player = CreatePlayer();

        var armor = CreateArmor();
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
    }
    
    [TestMethod]
    public void ApplyElementalDamage_DoesNotTriggerDodge()
    {
        var player = CreatePlayer();
        player.DodgeChance = 100;

        var result = player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, player.CurrentHealth);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_DoesNotReduceArmorDurability()
    {
        var player = CreatePlayer();

        var armor = CreateArmorWithRune(ElementType.Fire, tier: 1, power: 3);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        var initialDurability = armor.Durability;

        player.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(initialDurability, armor.Durability);
    }
    
    [TestMethod]
    public void ApplyElementalDamage_CompleteResilience_ReportsNoDamage()
    {
        var player = CreatePlayer();

        var armor = CreateArmorWithRune(ElementType.Fire, tier: 2, power: 5);
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        player.ApplyElementalDamage(10, ElementType.Fire);

        var actionItem = player.ActionItems.Values.Last();

        Assert.IsFalse(actionItem.Contains("drains your vitality"));
    }
}