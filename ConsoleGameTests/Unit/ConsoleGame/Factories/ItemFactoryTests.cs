
using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Items;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Factories;

[TestClass]
public class ItemFactoryTests
{
    private Mock<IItemDao> _itemDaoMock = null!;
    private ItemFactory _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        _itemDaoMock = new Mock<IItemDao>();
        _factory = new ItemFactory(_itemDaoMock.Object);
    }

    [TestMethod]
    public void GenerateLoot_ReturnsLootWithinMonsterLimit()
    {
        var items = CreateAvailableItems();
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(items);

        var loot = _factory.GenerateLoot(1, 2, false);

        Assert.IsNotNull(loot);
        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.Count <= 2);

        // Generated loot should consist of new item instances.
        foreach (var item in loot)
            Assert.IsFalse(items.Contains(item));
    }

    [TestMethod]
    public void GenerateLoot_NoEligibleItems_ReturnsEmptyList()
    {
        var items = CreateAvailableItems();

        foreach (var item in items)
            item.Value = 1000;

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(items);

        var loot = _factory.GenerateLoot(1, 2, false);

        Assert.IsNotNull(loot);
        Assert.AreEqual(0, loot.Count);
    }

    [TestMethod]
    public void CreateMerchantItems_ReturnsAvailableMerchantCategoriesWithoutValuables()
    {
        var items = CreateAvailableItems();

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(items);

        var merchantItems = _factory.CreateMerchantItems(1);

        Assert.IsNotNull(merchantItems);
        Assert.IsTrue(merchantItems.Count > 0);
        Assert.IsTrue(merchantItems.Count <= 7);

        Assert.IsTrue(merchantItems.OfType<Consumable>().Count() <= 1);
        Assert.IsTrue(merchantItems.OfType<Weapon>().Count() <= 3);
        Assert.IsTrue(merchantItems.OfType<Armor>().Count() <= 3);

        Assert.IsFalse(merchantItems.Any(i => i is Valuable));
    }

    [TestMethod]
    public void CreateMerchantItems_NoAvailableItems_ReturnsEmptyList()
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns([]);

        var merchantItems = _factory.CreateMerchantItems(1);

        Assert.IsNotNull(merchantItems);
        Assert.AreEqual(0, merchantItems.Count);
    }

    [TestMethod]
    public void GetMerchantGifts_MartialArchetype_ReturnsMatchingGifts()
    {
        var items = CreateAvailableItems();

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(items);

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.IsNotNull(gifts);
        Assert.AreEqual(3, gifts.Count);

        var weapon = gifts.OfType<Weapon>().Single();
        Assert.AreEqual(DamageType.Martial, weapon.DamageType);

        Assert.AreEqual(1, gifts.OfType<Armor>().Count());
        Assert.AreEqual(1, gifts.OfType<Consumable>().Count());
        Assert.IsFalse(gifts.Any(i => i is Valuable));
    }

    [TestMethod]
    public void GetMerchantGifts_MagicalArchetype_ReturnsMatchingGifts()
    {
        var items = CreateAvailableItems();

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(items);

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Magical, 1);

        Assert.IsNotNull(gifts);
        Assert.AreEqual(3, gifts.Count);

        var weapon = gifts.OfType<Weapon>().Single();
        Assert.AreEqual(DamageType.Magical, weapon.DamageType);

        Assert.AreEqual(1, gifts.OfType<Armor>().Count());
        Assert.AreEqual(1, gifts.OfType<Consumable>().Count());
        Assert.IsFalse(gifts.Any(i => i is Valuable));
    }

    [TestMethod]
    public void GetMerchantGifts_NoAvailableItems_ReturnsEmptyList()
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns([]);

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.IsNotNull(gifts);
        Assert.AreEqual(0, gifts.Count);
    }

    private static List<Item> CreateAvailableItems()
    {
        return
        [
            new Valuable
            {
                Name = "Gold Coin",
                Value = 1,
                RequiredLevel = 1
            },
            new Weapon
            {
                Name = "Sword",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 5,
                Durability = 10
            },
            new Weapon
            {
                Name = "Staff",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Magical,
                AttackPower = 7,
                Durability = 8
            },
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest,
                DefensePower = 3,
                Resistance = 2,
                Durability = 5
            },
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health,
                Power = 2,
                Durability = 1
            }
        ];
    }
    
    
    [TestMethod]
    public void GenerateLoot_ZeroMonsters_ReturnsEmptyList()
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns([CreateWeaponTemplate()]);

        var loot = _factory.GenerateLoot(1, 0, false);

        Assert.IsNotNull(loot);
        Assert.AreEqual(0, loot.Count);
    }

    [TestMethod]
    public void GenerateLoot_OneMonster_ReturnsAtMostOneItem()
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns([CreateWeaponTemplate()]);

        var loot = _factory.GenerateLoot(1, 1, false);

        Assert.IsNotNull(loot);
        Assert.AreEqual(1, loot.Count);
    }

    [TestMethod]
    public void GenerateLoot_CampaignMode_GeneratesLoot()
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns([CreateWeaponTemplate()]);

        var loot = _factory.GenerateLoot(1, 2, true);

        Assert.IsNotNull(loot);
        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.Count <= 2);
        Assert.IsTrue(loot.All(item => item is Weapon));
    }

    [TestMethod]
    public void GenerateLoot_OnlyValuablesAvailable_UsesValuables()
    {
        AssertLootUsesOnlyCategory<Valuable>(
        [
            new Valuable
            {
                Name = "Gold Coin",
                Value = 1,
                RequiredLevel = 1
            }
        ]);
    }

    [TestMethod]
    public void GenerateLoot_OnlyWeaponsAvailable_UsesWeapons()
    {
        AssertLootUsesOnlyCategory<Weapon>(
        [
            CreateWeaponTemplate()
        ]);
    }

    [TestMethod]
    public void GenerateLoot_OnlyArmorAvailable_UsesArmor()
    {
        AssertLootUsesOnlyCategory<Armor>(
        [
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest
            }
        ]);
    }

    [TestMethod]
    public void GenerateLoot_OnlyConsumablesAvailable_UsesConsumables()
    {
        AssertLootUsesOnlyCategory<Consumable>(
        [
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health
            }
        ]);
    }

    [TestMethod]
    public void GenerateLoot_MultipleCategoriesUnavailable_UsesAvailableCategories()
    {
        AssertLootUsesOnlyAvailableCategories(
        [
            CreateWeaponTemplate(),
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health
            }
        ]);
    }

    [TestMethod]
    public void GenerateLoot_UsesItemsAvailableAtRequestedLevel()
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(7))
            .Returns([CreateWeaponTemplate()]);

        var loot = _factory.GenerateLoot(7, 1, false);

        _itemDaoMock.Verify(
            d => d.GetItemsAvailableAtLevel(7),
            Times.Once);
        
        Assert.IsNotNull(loot);
        Assert.AreEqual(1, loot.Count);
    }

    [TestMethod]
    public void GenerateLoot_ReturnsCreatedItemsWithGeneratedStatsAndValue()
    {
        var template = CreateWeaponTemplate();

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns([template]);

        var loot = _factory.GenerateLoot(1, 1, false);

        Assert.IsNotNull(loot);
        Assert.IsTrue(loot.Count > 0);

        var weapon = loot.OfType<Weapon>().First();

        Assert.AreNotSame(template, weapon);
        Assert.AreEqual(template.Name, weapon.Name);
        Assert.AreEqual(1, weapon.RequiredLevel);

        Assert.IsTrue(weapon.AttackPower >= 6);
        Assert.IsTrue(weapon.AttackPower <= 8);

        Assert.IsTrue(weapon.Durability >= 3);
        Assert.IsTrue(weapon.Durability <= 8);

        Assert.IsTrue(weapon.Value > 0);
    }

    private void AssertLootUsesOnlyCategory<T>(List<Item> availableItems)
        where T : Item
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var loot = _factory.GenerateLoot(1, 2, false);

        Assert.IsNotNull(loot);
        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item is T));
    }

    private void AssertLootUsesOnlyAvailableCategories(List<Item> availableItems)
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var loot = _factory.GenerateLoot(1, 2, false);

        Assert.IsNotNull(loot);
        Assert.IsTrue(loot.Count > 0);

        foreach (var item in loot)
        {
            Assert.IsTrue(
                availableItems.Any(template => template.GetType() == item.GetType()),
                $"Unexpected loot category: {item.GetType().Name}");
        }
    }

    private static Weapon CreateWeaponTemplate()
    {
        return new Weapon
        {
            Name = "Sword",
            Value = 1,
            RequiredLevel = 1,
            DamageType = DamageType.Martial,
            AttackPower = 5,
            Durability = 5
        };
    }
    
    
    [TestMethod]
    public void CreateMerchantItems_OnlyAvailableCategories_ReturnsThoseCategories()
    {
        var availableItems = new List<Item>
        {
            CreateWeaponTemplate(),
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health,
                Power = 2,
                Durability = 3
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var merchantItems = _factory.CreateMerchantItems(1);

        Assert.IsNotNull(merchantItems);
        Assert.IsTrue(merchantItems.Count > 0);

        Assert.IsTrue(merchantItems.All(item =>
            item is Weapon || item is Consumable));

        Assert.IsFalse(merchantItems.Any(item => item is Armor));
        Assert.IsFalse(merchantItems.Any(item => item is Valuable));

        Assert.AreEqual(1, merchantItems.OfType<Consumable>().Count());
        Assert.AreEqual(1, merchantItems.OfType<Weapon>().Count());
    }

    [TestMethod]
    public void CreateMerchantItems_DoesNotReturnDuplicateTemplates()
    {
        var availableItems = new List<Item>
        {
            CreateWeaponTemplate(),
            new Weapon
            {
                Name = "Staff",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Magical,
                AttackPower = 7,
                Durability = 5
            },
            new Weapon
            {
                Name = "Axe",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 8,
                Durability = 5
            },
            new Weapon
            {
                Name = "Dagger",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 4,
                Durability = 5
            },
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest,
                DefensePower = 3,
                Resistance = 2,
                Durability = 5
            },
            new Armor
            {
                Name = "Helmet",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Head,
                DefensePower = 2,
                Resistance = 1,
                Durability = 5
            },
            new Armor
            {
                Name = "Boots",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Legs,
                DefensePower = 1,
                Resistance = 2,
                Durability = 5
            },
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health,
                Power = 2,
                Durability = 3
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var merchantItems = _factory.CreateMerchantItems(1);

        Assert.IsNotNull(merchantItems);

        Assert.AreEqual(
            merchantItems.Count,
            merchantItems.Select(item => item.Name).Distinct().Count(),
            "Merchant stock should not contain duplicate templates.");
    }

    [TestMethod]
    public void CreateMerchantItems_SelectsMartialAndMagicalWeaponsWhenAvailable()
    {
        var availableItems = new List<Item>
        {
            CreateWeaponTemplate(),
            new Weapon
            {
                Name = "Staff",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Magical,
                AttackPower = 7,
                Durability = 5
            },
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest,
                DefensePower = 3,
                Resistance = 2,
                Durability = 5
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var merchantItems = _factory.CreateMerchantItems(1);

        var weapons = merchantItems.OfType<Weapon>().ToList();

        Assert.AreEqual(2, weapons.Count);
        Assert.IsTrue(weapons.Any(w => w.DamageType == DamageType.Martial));
        Assert.IsTrue(weapons.Any(w => w.DamageType == DamageType.Magical));
    }

    [TestMethod]
    public void CreateMerchantItems_RespectsMaximumCategoryCounts()
    {
        var availableItems = new List<Item>
        {
            new Weapon
            {
                Name = "Sword",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 5,
                Durability = 5
            },
            new Weapon
            {
                Name = "Staff",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Magical,
                AttackPower = 7,
                Durability = 5
            },
            new Weapon
            {
                Name = "Axe",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 8,
                Durability = 5
            },
            new Weapon
            {
                Name = "Dagger",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 4,
                Durability = 5
            },
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest,
                DefensePower = 3,
                Resistance = 2,
                Durability = 5
            },
            new Armor
            {
                Name = "Helmet",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Head,
                DefensePower = 2,
                Resistance = 1,
                Durability = 5
            },
            new Armor
            {
                Name = "Boots",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Legs,
                DefensePower = 1,
                Resistance = 2,
                Durability = 5
            },
            new Armor
            {
                Name = "Gloves",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Arms,
                DefensePower = 1,
                Resistance = 1,
                Durability = 5
            },
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health,
                Power = 2,
                Durability = 3
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var merchantItems = _factory.CreateMerchantItems(1);

        Assert.IsTrue(merchantItems.OfType<Weapon>().Count() <= 3);
        Assert.IsTrue(merchantItems.OfType<Armor>().Count() <= 3);
        Assert.IsTrue(merchantItems.OfType<Consumable>().Count() <= 1);
        Assert.IsFalse(merchantItems.Any(item => item is Valuable));
    }

    
    [TestMethod]
    public void GetMerchantGifts_NoMatchingWeapon_ReturnsAvailableOtherGifts()
    {
        var availableItems = new List<Item>
        {
            new Weapon
            {
                Name = "Staff",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Magical,
                AttackPower = 7,
                Durability = 5
            },
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest,
                DefensePower = 3,
                Resistance = 2,
                Durability = 5
            },
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health,
                Power = 2,
                Durability = 3
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.IsNotNull(gifts);
        Assert.AreEqual(2, gifts.Count);

        Assert.IsFalse(gifts.Any(item => item is Weapon));
        Assert.AreEqual(1, gifts.OfType<Armor>().Count());
        Assert.AreEqual(1, gifts.OfType<Consumable>().Count());
    }

    [TestMethod]
    public void GetMerchantGifts_NoArmor_ReturnsRemainingGifts()
    {
        var availableItems = new List<Item>
        {
            new Weapon
            {
                Name = "Sword",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 5,
                Durability = 5
            },
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health,
                Power = 2,
                Durability = 3
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.IsNotNull(gifts);
        Assert.AreEqual(2, gifts.Count);

        Assert.AreEqual(1, gifts.OfType<Weapon>().Count());
        Assert.AreEqual(1, gifts.OfType<Consumable>().Count());
        Assert.IsFalse(gifts.Any(item => item is Armor));
    }

    [TestMethod]
    public void GetMerchantGifts_NoConsumable_ReturnsRemainingGifts()
    {
        var availableItems = new List<Item>
        {
            new Weapon
            {
                Name = "Sword",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 5,
                Durability = 5
            },
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest,
                DefensePower = 3,
                Resistance = 2,
                Durability = 5
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.IsNotNull(gifts);
        Assert.AreEqual(2, gifts.Count);

        Assert.AreEqual(1, gifts.OfType<Weapon>().Count());
        Assert.AreEqual(1, gifts.OfType<Armor>().Count());
        Assert.IsFalse(gifts.Any(item => item is Consumable));
    }

    
    [TestMethod]
    public void GenerateLoot_NeverCursesMoreThanTwoItems()
    {
        var availableItems = new List<Item>
        {
            CreateWeaponTemplate(),
            new Weapon
            {
                Name = "Axe",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Martial,
                AttackPower = 7,
                Durability = 5
            },
            new Weapon
            {
                Name = "Staff",
                Value = 1,
                RequiredLevel = 1,
                DamageType = DamageType.Magical,
                AttackPower = 8,
                Durability = 5
            },
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest,
                DefensePower = 3,
                Resistance = 2,
                Durability = 5
            },
            new Armor
            {
                Name = "Helmet",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Head,
                DefensePower = 2,
                Resistance = 1,
                Durability = 5
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        // Repeat generation to exercise the curse application.
        for (int i = 0; i < 100; i++)
        {
            var loot = _factory.GenerateLoot(1, 5, true);

            var cursedItems = loot
                .OfType<ICursable>()
                .Count(item => item.IsCursed());

            Assert.IsTrue(
                cursedItems <= 2,
                $"Expected at most 2 cursed items, but found {cursedItems}.");
        }
    }

    [TestMethod]
    public void GenerateLoot_OnlyCursesEligibleItems()
    {
        var availableItems = new List<Item>
        {
            CreateWeaponTemplate(),
            new Armor
            {
                Name = "Shield",
                Value = 1,
                RequiredLevel = 1,
                ArmorType = ArmorType.Chest,
                DefensePower = 3,
                Resistance = 2,
                Durability = 5
            },
            new Consumable
            {
                Name = "Potion",
                Value = 1,
                RequiredLevel = 1,
                ConsumableType = ConsumableType.Health,
                Power = 2,
                Durability = 3
            },
            new Valuable
            {
                Name = "Gold Coin",
                Value = 1,
                RequiredLevel = 1
            }
        };

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(1))
            .Returns(availableItems);

        for (int i = 0; i < 100; i++)
        {
            var loot = _factory.GenerateLoot(1, 5, true);

            var cursedItems = loot
                .OfType<ICursable>()
                .Where(item => item.IsCursed())
                .ToList();

            Assert.IsTrue(
                cursedItems.Count <= 2,
                "No more than two items should be cursed.");

            Assert.IsTrue(
                cursedItems.All(item => item is Weapon or Armor),
                "Only weapons and armor should be cursed.");
        }
    }
    
    [TestMethod]
    public void GenerateLoot_ZeroLevel_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => _factory.GenerateLoot(0, 1, false));
    }

    [TestMethod]
    public void GenerateLoot_NegativeLevel_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => _factory.GenerateLoot(-1, 1, false));
    }

    [TestMethod]
    public void GenerateLoot_NegativeMonsterCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => _factory.GenerateLoot(1, -1, false));
    }
    
    
}