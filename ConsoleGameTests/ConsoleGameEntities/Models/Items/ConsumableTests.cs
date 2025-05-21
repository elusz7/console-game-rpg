using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Items;

[TestClass]
public class ConsumableTests
{
    public class MockPlayer : Player
    {
        public int HealedAmount { get; private set; }

        public MockPlayer()
        {
            MaxHealth = 50;
            CurrentHealth = 50;
            Archetype = new Archetype { CurrentResource = 10 };
        }

        public override void Heal(int amount)
        {
            HealedAmount += amount;
            CurrentHealth += amount; // simulate increase for test
        }

        public override void RecoverResource(int x)
        {
            Archetype.CurrentResource += x;
        }

        public List<string> Actions { get; } = new();
        public override void AddActionItem(string message) => Actions.Add(message);
    }

    public class MockInventory : Inventory
    {
        public override void RemoveItem(Item item) => Items.Remove(item);
    }

    public class MockItem : Item
    {
        public int RecoveredDurability { get; private set; }

        public override void RecoverDurability(int amount)
        {
            RecoveredDurability += amount;
            Durability += amount; // to make assertion pass
        }
    }


    [TestMethod]
    [ExpectedException(typeof(InvalidTargetException))]
    public void Use_ThrowsInvalidTargetException()
    {
        var consumable = new Consumable();
        consumable.Use();
    }

    [TestMethod]
    public void UseOn_Item_WithDurability_RepairsAndConsumes()
    {
        var player = new MockPlayer();
        var inventory = new MockInventory { Player = player };
        var item = new MockItem { Name = "Sword", Inventory = inventory };
        var consumable = new Consumable
        {
            Name = "Repair Kit",
            Power = 5,
            Durability = 1,
            ConsumableType = ConsumableType.Durability,
            Inventory = inventory
        };

        inventory.Items.Add(consumable);

        consumable.UseOn(item);

        Assert.AreEqual(5, item.Durability);
        CollectionAssert.Contains(player.Actions, "Sword has been repaired by 5 durability!");
        CollectionAssert.Contains(player.Actions, "Repair Kit has been used up.");
        CollectionAssert.DoesNotContain(inventory.Items, consumable);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidTargetException))]
    public void UseOn_Item_WithWrongType_Throws()
    {
        var item = new MockItem();
        var consumable = new Consumable
        {
            Name = "Potion",
            Power = 10,
            ConsumableType = ConsumableType.Health
        };

        consumable.UseOn(item);
    }

    [TestMethod]
    public void UseOn_Player_HealsAndConsumes()
    {
        var player = new MockPlayer();
        var inventory = new MockInventory { Player = player };
        var consumable = new Consumable
        {
            Name = "Health Potion",
            Power = 20,
            Durability = 1,
            ConsumableType = ConsumableType.Health,
            Inventory = inventory
        };

        inventory.Items.Add(consumable);
        consumable.UseOn(player);

        Assert.AreEqual(70, player.CurrentHealth);
        CollectionAssert.Contains(player.Actions, "Health Potion has been used up.");
        CollectionAssert.DoesNotContain(inventory.Items, consumable);
    }

    [TestMethod]
    public void UseOn_Player_ResourceRecovery_Works()
    {
        var player = new MockPlayer();
        var inventory = new MockInventory { Player = player };
        var consumable = new Consumable
        {
            Name = "Mana Flask",
            Power = 15,
            Durability = 1,
            ConsumableType = ConsumableType.Resource,
            Inventory = inventory
        };

        inventory.Items.Add(consumable);
        consumable.UseOn(player);

        Assert.AreEqual(25, player.Archetype.CurrentResource);
        CollectionAssert.Contains(player.Actions, "Mana Flask has been used up.");
        CollectionAssert.DoesNotContain(inventory.Items, consumable);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidTargetException))]
    public void UseOn_Player_WithWrongType_Throws()
    {
        var player = new MockPlayer();
        var consumable = new Consumable
        {
            Name = "Wrench",
            Power = 5,
            ConsumableType = ConsumableType.Durability
        };

        consumable.UseOn(player);
    }

    [TestMethod]
    public void CalculateValue_ProducesExpectedRange()
    {
        var consumable = new Consumable
        {
            RequiredLevel = 5,
            Durability = 3,
            Power = 10
        };

        consumable.CalculateValue();

        Assert.IsTrue(consumable.Value >= 28 && consumable.Value <= 37); // Approximate range
    }

    [TestMethod]
    public void CalculateStatsByLevel_SetsDurabilityAndPower()
    {
        var consumable = new Consumable
        {
            RequiredLevel = 5,
            ConsumableType = ConsumableType.Resource
        };

        consumable.CalculateStatsByLevel();

        Assert.AreEqual(3, consumable.Durability);
        Assert.IsTrue(consumable.Power >= 10 && consumable.Power <= 20);
    }

    [DataTestMethod]
    [DataRow(ConsumableType.Health, 40, 10)]
    [DataRow(ConsumableType.Durability, 15, 10)]
    [DataRow(ConsumableType.Resource, 30, 10)]
    public void CalculateLevelByStats_Works(ConsumableType type, int power, int expectedLevel)
    {
        var consumable = new Consumable
        {
            Power = power,
            ConsumableType = type
        };

        consumable.CalculateLevelByStats();

        Assert.AreEqual(expectedLevel, consumable.RequiredLevel);
    }
}
