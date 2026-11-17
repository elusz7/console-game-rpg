
using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Items;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Factories.ItemFactoryTests;

[TestClass]
public class CreateMerchantItemsTests
{
    private Mock<IItemDao> _itemDaoMock = null!;
    private Mock<IRandomProvider> _randomProviderMock = null!;
    private ItemFactory _factory = null!;

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
            .Returns((int _) => 0);

        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int _) => min);

        SetupAvailableItems();
    }
    //validate level
    
    [TestMethod]
    public void CreateMerchantItems_ZeroLevel_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _factory.CreateMerchantItems(0));
    }
    
    [TestMethod]
    public void CreateMerchantItems_NegativeLevel_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _factory.CreateMerchantItems(0));
    }

    // Available item categories

    [TestMethod]
    public void CreateMerchantItems_NoAvailableItems_ReturnsEmptyList()
    {
        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(0, items.Count);
    }

    [TestMethod]
    public void CreateMerchantItems_OnlyConsumablesAvailable_ReturnsOneConsumable()
    {
        SetupAvailableItems(
            CreateConsumable("Health Potion"),
            CreateConsumable("Mana Potion"));

        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(1, items.Count);
        Assert.IsInstanceOfType<Consumable>(items[0]);
    }

    [TestMethod]
    public void CreateMerchantItems_OnlyWeaponsAvailable_ReturnsUpToThreeWeapons()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Axe"),
            CreateWeapon("Dagger"),
            CreateWeapon("Spear"));

        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(3, items.Count);
        Assert.IsTrue(items.All(i => i is Weapon));
    }

    [TestMethod]
    public void CreateMerchantItems_OnlyArmorAvailable_ReturnsUpToThreeArmorPieces()
    {
        SetupAvailableItems(
            CreateArmor("Armor 1"),
            CreateArmor("Armor 2"),
            CreateArmor("Armor 3"),
            CreateArmor("Armor 4"));

        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(3, items.Count);
        Assert.IsTrue(items.All(i => i is Armor));
    }

    [TestMethod]
    public void CreateMerchantItems_AllCategoriesAvailable_ReturnsMerchantStock()
    {
        SetupAvailableItems(
            CreateConsumable("Health Potion"),
            CreateWeapon("Sword"),
            CreateWeapon("Staff", DamageType.Magical),
            CreateWeapon("Dagger"),
            CreateArmor("Helmet"),
            CreateArmor("Chestplate"),
            CreateArmor("Boots"));

        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(7, items.Count);
        Assert.AreEqual(1, items.OfType<Consumable>().Count());
        Assert.AreEqual(3, items.OfType<Weapon>().Count());
        Assert.AreEqual(3, items.OfType<Armor>().Count());
    }

    // Weapon selection

    [TestMethod]
    public void CreateMerchantItems_IncludesMartialWeaponWhenAvailable()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Staff", DamageType.Magical));

        var items = _factory.CreateMerchantItems(1);

        Assert.IsTrue(items.OfType<Weapon>()
            .Any(w => w.DamageType == DamageType.Martial));
    }

    [TestMethod]
    public void CreateMerchantItems_IncludesMagicalWeaponWhenAvailable()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Staff", DamageType.Magical));

        var items = _factory.CreateMerchantItems(1);

        Assert.IsTrue(items.OfType<Weapon>()
            .Any(w => w.DamageType == DamageType.Magical));
    }

    [TestMethod]
    public void CreateMerchantItems_SelectsUpToThreeWeapons()
    {
        SetupAvailableItems(
            CreateWeapon("Weapon 1"),
            CreateWeapon("Weapon 2"),
            CreateWeapon("Weapon 3"),
            CreateWeapon("Weapon 4"),
            CreateWeapon("Weapon 5"));

        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(3, items.OfType<Weapon>().Count());
    }

    [TestMethod]
    public void CreateMerchantItems_DoesNotDuplicateWeapons()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Axe"),
            CreateWeapon("Dagger"),
            CreateWeapon("Spear"));

        var weapons = _factory.CreateMerchantItems(1)
            .OfType<Weapon>()
            .ToList();

        Assert.AreEqual(
            weapons.Count,
            weapons.Select(w => w.Name).Distinct().Count());
    }

    [TestMethod]
    public void CreateMerchantItems_FillsRemainingWeaponSlotsWhenPossible()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Axe"),
            CreateWeapon("Dagger"));

        var weapons = _factory.CreateMerchantItems(1)
            .OfType<Weapon>()
            .ToList();

        Assert.AreEqual(3, weapons.Count);
    }

    // Armor and consumables

    [TestMethod]
    public void CreateMerchantItems_SelectsUpToThreeArmorPieces()
    {
        SetupAvailableItems(
            CreateArmor("Armor 1"),
            CreateArmor("Armor 2"),
            CreateArmor("Armor 3"),
            CreateArmor("Armor 4"),
            CreateArmor("Armor 5"));

        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(3, items.OfType<Armor>().Count());
    }

    [TestMethod]
    public void CreateMerchantItems_DoesNotDuplicateArmor()
    {
        SetupAvailableItems(
            CreateArmor("Helmet"),
            CreateArmor("Chestplate"),
            CreateArmor("Boots"),
            CreateArmor("Gloves"));

        var armor = _factory.CreateMerchantItems(1)
            .OfType<Armor>()
            .ToList();

        Assert.AreEqual(
            armor.Count,
            armor.Select(a => a.Name).Distinct().Count());
    }

    [TestMethod]
    public void CreateMerchantItems_SelectsAtMostOneConsumable()
    {
        SetupAvailableItems(
            CreateConsumable("Potion 1"),
            CreateConsumable("Potion 2"),
            CreateConsumable("Potion 3"));

        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(1, items.OfType<Consumable>().Count());
    }

    // Item creation

    [TestMethod]
    public void CreateMerchantItems_ReturnsCreatedItemsWithGeneratedStatsAndValue()
    {
        var template = CreateWeapon("Test Weapon");

        SetupAvailableItems(template);

        var items = _factory.CreateMerchantItems(1);

        Assert.AreEqual(1, items.Count);

        var weapon = (Weapon)items[0];

        Assert.AreNotSame(template, weapon);
        Assert.IsTrue(weapon.Value > 0);
        Assert.IsTrue(weapon.Durability > 0);
        Assert.IsTrue(weapon.AttackPower > 0);
        Assert.AreEqual(template.Name, weapon.Name);
        Assert.AreEqual(template.RequiredLevel, weapon.RequiredLevel);
    }

    [TestMethod]
    public void CreateMerchantItems_RequestsItemsAvailableAtRequestedLevel()
    {
        const int expectedLevel = 5;

        _factory.CreateMerchantItems(expectedLevel);

        _itemDaoMock.Verify(
            d => d.GetItemsAvailableAtLevel(expectedLevel),
            Times.Once);
    }

    // Test helpers

    private void SetupAvailableItems(params Item[] items)
    {
        _itemDaoMock
            .Setup(d => d.GetItemsAvailableAtLevel(It.IsAny<int>()))
            .Returns(items.ToList());
    }

    private Weapon CreateWeapon(
        string name,
        DamageType damageType = DamageType.Martial)
    {
        return new Weapon(_randomProviderMock.Object)
        {
            Name = name,
            Description = "Test weapon",
            Weight = 1,
            RequiredLevel = 1,
            DamageType = damageType
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
}
