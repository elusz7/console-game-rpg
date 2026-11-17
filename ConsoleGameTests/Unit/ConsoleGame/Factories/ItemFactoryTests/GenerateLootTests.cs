using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Items;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Factories.ItemFactoryTests;

[TestClass]
public class ItemFactoryGenerateLootTests
{
    private Mock<IItemDao> _itemDaoMock = null!;
    private Mock<IRandomProvider> _randomProviderMock = null!;
    private ItemFactory _factory = null!;
    
    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        _itemDaoMock = new Mock<IItemDao>();
        _randomProviderMock = new Mock<IRandomProvider>();

        _factory = new ItemFactory(
            _itemDaoMock.Object,
            _randomProviderMock.Object);

        _randomProviderMock
            .Setup(r => r.NextDouble())
            .Returns(0.5);
        
        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>()))
            .Returns((int max) => 0);
        
        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => min);

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(It.IsAny<int>()))
            .Returns([]);
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

    [TestMethod]
    public void GenerateLoot_ZeroMonsters_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => _factory.GenerateLoot(1, 0, false));
    }
    
    [TestMethod]
    public void GenerateLoot_NoAvailableItems_ReturnsEmptyList()
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(It.IsAny<int>()))
            .Returns([]);

        var loot = _factory.GenerateLoot(1, 1, false);

        Assert.AreEqual(0, loot.Count);
    }

    [TestMethod]
    public void GenerateLoot_NoEligibleItems_ReturnsEmptyList()
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(It.IsAny<int>()))
            .Returns([CreateValuable("Expensive Valuable", 1000)]);

        var loot = _factory.GenerateLoot(1, 1, false);

        Assert.AreEqual(0, loot.Count);
    }

    [TestMethod]
    public void GenerateLoot_ExcludesItemsAboveTrueValueThreshold()
    {
        var eligibleItem = CreateValuable("Eligible Valuable");
        var ineligibleItem = CreateValuable("Ineligible Valuable", 1000);

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(It.IsAny<int>()))
            .Returns([eligibleItem, ineligibleItem]);

        var loot = _factory.GenerateLoot(1, 1, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item.Name == eligibleItem.Name));
    }

    [TestMethod]
    public void GenerateLoot_IncludesItemsBelowTrueValueThreshold()
    {
        var eligibleItem = CreateValuable("Eligible Valuable");

        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(It.IsAny<int>()))
            .Returns([eligibleItem]);

        var loot = _factory.GenerateLoot(1, 1, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item.Name == eligibleItem.Name));
    }

    [TestMethod]
    public void GenerateLoot_RequestsItemsAvailableAtRequestedLevel()
    {
        const int expectedLevel = 5;

        _factory.GenerateLoot(expectedLevel, 1, false);

        _itemDaoMock.Verify(
            d => d.GetItemsAvailableAtLevel(expectedLevel),
            Times.Once);
    }
    
    [TestMethod]
    public void GenerateLoot_ReturnsLootWithinMonsterLimit()
    {
        const int monsterCount = 10;

        var loot = _factory.GenerateLoot(1, monsterCount, false);

        Assert.IsTrue(loot.Count <= monsterCount);
    }

    [TestMethod]
    public void GenerateLoot_GeneratesLootWhenEligibleItemsExist()
    {
        SetupAvailableItems(CreateValuable("Test Valuable"));

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 0);
    }

    [TestMethod]
    public void GenerateLoot_OneMonster_ReturnsAtMostOneItem()
    {
        var loot = _factory.GenerateLoot(1, 1, false);

        Assert.IsTrue(loot.Count <= 1);
    }

    [TestMethod]
    public void GenerateLoot_OnlyValuablesAvailable_GeneratesValuables()
    {
        SetupAvailableItems(CreateValuable("Test Valuable"));

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item is Valuable));
    }

    [TestMethod]
    public void GenerateLoot_OnlyWeaponsAvailable_GeneratesWeapons()
    {
        SetupAvailableItems(CreateWeapon("Test Weapon"));

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item is Weapon));
    }

    [TestMethod]
    public void GenerateLoot_OnlyArmorAvailable_GeneratesArmor()
    {
        SetupAvailableItems(CreateArmor("Test Armor"));

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item is Armor));
    }

    [TestMethod]
    public void GenerateLoot_OnlyConsumablesAvailable_GeneratesConsumables()
    {
        SetupAvailableItems(CreateConsumable("Test Consumable"));

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item is Consumable));
    }

    [TestMethod]
    public void GenerateLoot_WeaponAndArmorAvailable_ReturnsOnlyAvailableCategories()
    {
        SetupAvailableItems(
            CreateWeapon("Test Weapon"),
            CreateArmor("Test Armor"));

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item is Weapon or Armor));
    }

    private void SetupAvailableItems(params Item[] items)
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(It.IsAny<int>()))
            .Returns(items.ToList());
    }

    private Valuable CreateValuable(string name, int requiredLevel = 1)
    {
        return new Valuable(_randomProviderMock.Object)
        {
            Name = name,
            Description = "Test valuable",
            Weight = 1,
            RequiredLevel = requiredLevel
        };
    }

    private Weapon CreateWeapon(string name)
    {
        return new Weapon(_randomProviderMock.Object)
        {
            Name = name,
            Description = "Test weapon",
            Weight = 1,
            RequiredLevel = 1,
            DamageType = DamageType.Martial
        };
    }

    private Armor CreateArmor(string name)
    {
        return new Armor(_randomProviderMock.Object)
        {
            Name = name,
            Description = "Test armor",
            Weight = 1,
            RequiredLevel = 1,
            ArmorType = ArmorType.Chest
        };
    }

    private Consumable CreateConsumable(string name)
    {
        return new Consumable(_randomProviderMock.Object)
        {
            Name = name,
            Description = "Test consumable",
            Weight = 1,
            RequiredLevel = 1,
            ConsumableType = ConsumableType.Health
        };
    }

    [TestMethod]
    public void GenerateLoot_CampaignMode_IncreasesLootBudget()
    {
        SetupAvailableItems(CreateValuable("Test Valuable"));

        var normalLoot = _factory.GenerateLoot(1, 20, false);
        var campaignLoot = _factory.GenerateLoot(1, 20, true);

        Assert.IsTrue(campaignLoot.Count > normalLoot.Count);
    }
    
    [TestMethod]
    public void GenerateLoot_UsesInjectedRandomProviderForLootVariance()
    {
        SetupAvailableItems(CreateValuable("Test Valuable"));

        double budgetVariance = 0.0;
        int randomCallCount = 0;

        _randomProviderMock
            .Setup(r => r.NextDouble())
            .Returns(() =>
            {
                return randomCallCount++ == 0
                    ? budgetVariance
                    : 0.5;
            });

        var lowVarianceLoot = _factory.GenerateLoot(1, 20, false);

        budgetVariance = 0.999;
        randomCallCount = 0;

        var highVarianceLoot = _factory.GenerateLoot(1, 20, false);

        Assert.IsTrue(
            highVarianceLoot.Count > lowVarianceLoot.Count,
            $"Low variance: {lowVarianceLoot.Count}, High variance: {highVarianceLoot.Count}");
    }
    
    [TestMethod]
    public void GenerateLoot_UsesInjectedRandomProviderForCategorySelection()
    {
        SetupAvailableItems(
            CreateValuable("Test Valuable"),
            CreateWeapon("Test Weapon"));

        _randomProviderMock
            .Setup(r => r.Next(80))
            .Returns(65);

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item is Weapon));
    }
    
    [TestMethod]
    public void GenerateLoot_UsesInjectedRandomProviderForItemSelection()
    {
        SetupAvailableItems(
            CreateValuable("First Valuable"),
            CreateValuable("Second Valuable"));

        _randomProviderMock
            .Setup(r => r.Next(2))
            .Returns(1);

        var loot = _factory.GenerateLoot(1, 20, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(item => item.Name == "Second Valuable"));
    }
    
    
    [TestMethod]
    public void GenerateLoot_LowValueLootWithValuable_ReplacesLowestValuableWithJackpot()
    {
        SetupAvailableItems(
            CreateValuable("Low Valuable"),
            CreateValuable("Jackpot Valuable", 5));

        // Select the low-value valuable during initial loot generation.
        _randomProviderMock
            .Setup(r => r.Next(2))
            .Returns(0);

        var loot = _factory.GenerateLoot(10, 2, false);

        foreach (var item in loot)
        {
            TestContext.WriteLine(
                $"{item.Name}: Value={item.Value}, Level={item.RequiredLevel}");
        }

        Assert.AreEqual(2, loot.Count);
        Assert.AreEqual(
            2,
            loot.Count,
            $"Generated loot: {string.Join(", ", loot.Select(i => $"{i.Name}: {i.Value}"))}");
        Assert.AreEqual(1, loot.Count(i => i.Name == "Low Valuable"));
        Assert.AreEqual(1, loot.Count(i => i.Name == "Jackpot Valuable"));
    }

    [TestMethod]
    public void GenerateLoot_LowValueLootWithoutSuitableJackpot_RetainsOriginalValuable()
    {
        SetupAvailableItems(CreateValuable("Original Valuable"));

        var loot = _factory.GenerateLoot(1, 2, false);

        Assert.AreEqual(2, loot.Count);
        Assert.IsTrue(loot.All(i => i.Name == "Original Valuable"));
    }

    [TestMethod]
    public void GenerateLoot_LowValueLootWithoutValuables_DoesNotReplaceItems()
    {
        SetupAvailableItems(CreateWeapon("Test Weapon"));

        var loot = _factory.GenerateLoot(10, 2, false);

        Assert.AreEqual(2, loot.Count);
        Assert.IsTrue(loot.All(i => i is Weapon));
        Assert.IsTrue(loot.All(i => i.Name == "Test Weapon"));
    }

    [TestMethod]
    public void GenerateLoot_LootAboveJackpotThreshold_DoesNotReplaceValuable()
    {
        SetupAvailableItems(
            CreateValuable("Original Valuable"),
            CreateValuable("Jackpot Valuable", 5));

        // Select the original valuable during initial loot generation.
        _randomProviderMock
            .Setup(r => r.Next(2))
            .Returns(0);

        var loot = _factory.GenerateLoot(1, 1, false);

        Assert.AreEqual(1, loot.Count);
        Assert.AreEqual("Original Valuable", loot[0].Name);
    }
    
    [TestMethod]
    public void GenerateLoot_NonCursableItems_AreNeverCursed()
    {
        SetupAvailableItems(CreateValuable("Test Valuable"));

        _randomProviderMock
            .Setup(r => r.NextDouble())
            .Returns(0.0);

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.All(i => i is not ICursable));
    }

    [TestMethod]
    public void GenerateLoot_CursesAtMostTwoItems()
    {
        SetupAvailableItems(CreateWeapon("Test Weapon"));

        _randomProviderMock
            .Setup(r => r.NextDouble())
            .Returns(0.0);

        var loot = _factory.GenerateLoot(1, 10, false);

        Assert.IsTrue(loot.Count > 2);
        Assert.AreEqual(
            2,
            loot.OfType<ICursable>().Count(i => i.IsCursed()));
    }

    
    [TestMethod]
    public void GenerateLoot_NormalMode_UsesNormalCurseChance()
    {
        SetupAvailableItems(CreateWeapon("Test Weapon"));

        _randomProviderMock
            .Setup(r => r.NextDouble())
            .Returns(0.03);

        var loot = _factory.GenerateLoot(10, 10, false);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.OfType<ICursable>().All(i => !i.IsCursed()));
    }

    [TestMethod]
    public void GenerateLoot_CampaignMode_UsesCampaignCurseChance()
    {
        SetupAvailableItems(CreateWeapon("Test Weapon"));

        _randomProviderMock
            .Setup(r => r.NextDouble())
            .Returns(0.03);

        var loot = _factory.GenerateLoot(10, 10, true);

        Assert.IsTrue(loot.Count > 0);
        Assert.IsTrue(loot.OfType<ICursable>().Any(i => i.IsCursed()));
    }
}