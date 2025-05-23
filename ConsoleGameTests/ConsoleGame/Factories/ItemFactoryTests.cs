using ConsoleGame.Factories;
using ConsoleGame.GameDao;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Models.Items;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGame.Factories;

[TestClass]
public class ItemFactoryTests
{
    private Mock<IItemDao> _itemDaoMock;
    private ItemFactory _factory;

    [TestInitialize]
    public void Setup()
    {
        _itemDaoMock = new Mock<IItemDao>();
        _factory = new ItemFactory(_itemDaoMock.Object);
    }

    [TestMethod]
    public void GenerateLoot_ReturnsLootAndCurseCount()
    {
        var items = new List<Item>
        {
            new Valuable { Name = "Gold Coin", Value = 10, RequiredLevel = 1 },
            new Weapon { Name = "Sword", Value = 20, RequiredLevel = 1, DamageType = DamageType.Martial, AttackPower = 5, Durability = 10 },
            new Armor { Name = "Shield", Value = 15, RequiredLevel = 1, ArmorType = ArmorType.Chest, DefensePower = 3, Resistance = 2, Durability = 5 },
            new Consumable { Name = "Potion", Value = 5, RequiredLevel = 1, ConsumableType = ConsumableType.Health, Power = 2, Durability = 1 }
        };
        _itemDaoMock.Setup(d => d.GetItemsByMaxLevel(It.IsAny<int>())).Returns(items);

        var (loot, curseCount) = _factory.GenerateLoot(1, 2, false);

        Assert.IsNotNull(loot);
        Assert.IsTrue(curseCount >= 0);
    }

    [TestMethod]
    public void CreateMerchantItems_ReturnsMerchantItems()
    {
        var items = new List<Item>
        {
            new Consumable { Name = "Potion", Value = 5, RequiredLevel = 1, ConsumableType = ConsumableType.Health, Power = 2, Durability = 1 },
            new Weapon { Name = "Sword", Value = 20, RequiredLevel = 1, DamageType = DamageType.Martial, AttackPower = 5, Durability = 10 },
            new Weapon { Name = "Staff", Value = 25, RequiredLevel = 1, DamageType = DamageType.Magical, AttackPower = 7, Durability = 8 },
            new Armor { Name = "Shield", Value = 15, RequiredLevel = 1, ArmorType = ArmorType.Chest, DefensePower = 3, Resistance = 2, Durability = 5 }
        };
        _itemDaoMock.Setup(d => d.GetItemsByMaxLevel(It.IsAny<int>())).Returns(items);

        var merchantItems = _factory.CreateMerchantItems(1);

        Assert.IsNotNull(merchantItems);
        Assert.IsTrue(merchantItems.Count > 0);
    }

    [TestMethod]
    public void GetMerchantGifts_ReturnsGifts()
    {
        var items = new List<Item>
        {
            new Weapon { Name = "Sword", Value = 20, RequiredLevel = 1, DamageType = DamageType.Martial, AttackPower = 5, Durability = 10 },
            new Armor { Name = "Shield", Value = 15, RequiredLevel = 1, ArmorType = ArmorType.Chest, DefensePower = 3, Resistance = 2, Durability = 5 },
            new Consumable { Name = "Potion", Value = 5, RequiredLevel = 1, ConsumableType = ConsumableType.Health, Power = 2, Durability = 1 }
        };
        _itemDaoMock.Setup(d => d.GetItemsByMaxLevel(It.IsAny<int>())).Returns(items);

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.IsNotNull(gifts);
        Assert.IsTrue(gifts.Count > 0);
    }
}