
using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Items;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Factories.ItemFactoryTests;

[TestClass]
public class GetMerchantGiftsTests
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
    
    //parameter validation
    [TestMethod]
    public void GetMerchantGifts_ZeroLevel_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _factory.GetMerchantGifts(ArchetypeType.Martial, 0));
    }
    
    [TestMethod]
    public void GetMerchantGifts_NegativeLevel_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _factory.GetMerchantGifts(ArchetypeType.Magical, -5));
    }
    
    [TestMethod]
    public void GetMerchantGifts_InvalidArchetypeType_ThrowsArgumentOutOfRangeExceptionException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _factory.GetMerchantGifts((ArchetypeType)999, 5));
    }

    // Martial archetype

    [TestMethod]
    public void GetMerchantGifts_MartialArchetype_ReturnsMartialWeapon()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Staff", DamageType.Magical));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(1, gifts.Count);
        Assert.IsInstanceOfType<Weapon>(gifts[0]);
        Assert.AreEqual(
            DamageType.Martial,
            ((Weapon)gifts[0]).DamageType);
    }

    [TestMethod]
    public void GetMerchantGifts_MartialArchetype_DoesNotReturnMagicalWeapon()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Staff", DamageType.Magical));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.IsFalse(gifts.OfType<Weapon>()
            .Any(w => w.DamageType == DamageType.Magical));
    }

    // Magical archetype

    [TestMethod]
    public void GetMerchantGifts_MagicalArchetype_ReturnsMagicalWeapon()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Staff", DamageType.Magical));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Magical, 1);

        Assert.AreEqual(1, gifts.Count);
        Assert.IsInstanceOfType<Weapon>(gifts[0]);
        Assert.AreEqual(
            DamageType.Magical,
            ((Weapon)gifts[0]).DamageType);
    }

    [TestMethod]
    public void GetMerchantGifts_MagicalArchetype_DoesNotReturnMartialWeapon()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateWeapon("Staff", DamageType.Magical));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Magical, 1);

        Assert.IsFalse(gifts.OfType<Weapon>()
            .Any(w => w.DamageType == DamageType.Martial));
    }

    // Missing categories

    [TestMethod]
    public void GetMerchantGifts_NoAvailableItems_ReturnsEmptyList()
    {
        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(0, gifts.Count);
    }

    [TestMethod]
    public void GetMerchantGifts_NoMatchingWeapon_ReturnsOtherAvailableGifts()
    {
        SetupAvailableItems(
            CreateWeapon("Staff", DamageType.Magical),
            CreateArmor("Chestplate"),
            CreateConsumable("Health Potion"));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(2, gifts.Count);
        Assert.AreEqual(0, gifts.OfType<Weapon>().Count());
        Assert.AreEqual(1, gifts.OfType<Armor>().Count());
        Assert.AreEqual(1, gifts.OfType<Consumable>().Count());
    }

    [TestMethod]
    public void GetMerchantGifts_NoArmorAvailable_ReturnsOtherAvailableGifts()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateConsumable("Health Potion"));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(2, gifts.Count);
        Assert.AreEqual(1, gifts.OfType<Weapon>().Count());
        Assert.AreEqual(0, gifts.OfType<Armor>().Count());
        Assert.AreEqual(1, gifts.OfType<Consumable>().Count());
    }

    [TestMethod]
    public void GetMerchantGifts_NoConsumablesAvailable_ReturnsOtherAvailableGifts()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"),
            CreateArmor("Chestplate"));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(2, gifts.Count);
        Assert.AreEqual(1, gifts.OfType<Weapon>().Count());
        Assert.AreEqual(1, gifts.OfType<Armor>().Count());
        Assert.AreEqual(0, gifts.OfType<Consumable>().Count());
    }

    [TestMethod]
    public void GetMerchantGifts_OnlyMatchingWeaponAvailable_ReturnsOneGift()
    {
        SetupAvailableItems(
            CreateWeapon("Sword"));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(1, gifts.Count);
        Assert.IsInstanceOfType<Weapon>(gifts[0]);
    }

    [TestMethod]
    public void GetMerchantGifts_OnlyArmorAvailable_ReturnsOneGift()
    {
        SetupAvailableItems(CreateArmor("Chestplate"));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(1, gifts.Count);
        Assert.IsInstanceOfType<Armor>(gifts[0]);
    }

    [TestMethod]
    public void GetMerchantGifts_OnlyConsumablesAvailable_ReturnsOneGift()
    {
        SetupAvailableItems(CreateConsumable("Health Potion"));

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(1, gifts.Count);
        Assert.IsInstanceOfType<Consumable>(gifts[0]);
    }

    // Item creation

    [TestMethod]
    public void GetMerchantGifts_ReturnsCreatedItemsWithGeneratedStatsAndValue()
    {
        var weaponTemplate = CreateWeapon("Sword");
        var armorTemplate = CreateArmor("Chestplate");
        var consumableTemplate = CreateConsumable("Health Potion");

        SetupAvailableItems(
            weaponTemplate,
            armorTemplate,
            consumableTemplate);

        var gifts = _factory.GetMerchantGifts(ArchetypeType.Martial, 1);

        Assert.AreEqual(3, gifts.Count);

        var weapon = gifts.OfType<Weapon>().Single();
        var armor = gifts.OfType<Armor>().Single();
        var consumable = gifts.OfType<Consumable>().Single();

        Assert.AreNotSame(weaponTemplate, weapon);
        Assert.AreNotSame(armorTemplate, armor);
        Assert.AreNotSame(consumableTemplate, consumable);

        Assert.IsTrue(gifts.All(i => i.Value > 0));
        Assert.IsTrue(gifts.All(i => i.Durability > 0));

        Assert.AreEqual(weaponTemplate.Name, weapon.Name);
        Assert.AreEqual(armorTemplate.Name, armor.Name);
        Assert.AreEqual(consumableTemplate.Name, consumable.Name);

        Assert.AreEqual(
            weaponTemplate.RequiredLevel,
            weapon.RequiredLevel);

        Assert.AreEqual(
            armorTemplate.RequiredLevel,
            armor.RequiredLevel);

        Assert.AreEqual(
            consumableTemplate.RequiredLevel,
            consumable.RequiredLevel);
    }

    [TestMethod]
    public void GetMerchantGifts_RequestsItemsAvailableAtRequestedLevel()
    {
        const int expectedLevel = 5;

        _factory.GetMerchantGifts(ArchetypeType.Martial, expectedLevel);

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
