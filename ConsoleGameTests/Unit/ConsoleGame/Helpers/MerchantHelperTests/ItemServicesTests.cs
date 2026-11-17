using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.MerchantHelperTests;

[TestClass]
public class ItemServicesTests
{
    private Mock<IInputManager> _input = null!;
    private Mock<IOutputManager> _output = null!;
    private Mock<IRandomProvider> _random = null!;
    private MerchantHelper _merchant = null!;
    private Floor _floor = null!;
    private Player _player = null!;
    private List<string> _messages = null!;
    private int _menuCalls;

    [TestInitialize]
    public void Setup()
    {
        _input = new Mock<IInputManager>();
        _output = new Mock<IOutputManager>();
        _random = new Mock<IRandomProvider>();
        _messages = [];

        _output.Setup(o => o.WriteLine(It.IsAny<string>(), It.IsAny<ConsoleColor>()))
            .Callback<string, ConsoleColor>((message, _) => _messages.Add(message));

        // Item methods use their own injected random provider. Avoid random failures
        // and make reforging/enchantment stat changes reproducible.
        _random.Setup(r => r.NextDouble()).Returns(0.99);
        _random.Setup(r => r.Next(It.IsAny<int>())).Returns(0);
        _random.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int _) => min);

        _player = new Player
        {
            Id = 17,
            Name = "Test Hero",
            Level = 5,
            Archetype = new Archetype { ArchetypeType = ArchetypeType.Martial },
            Inventory = new Inventory { Id = 9, Gold = 100, Capacity = 100m }
        };
        _player.Inventory.Player = _player;

        _floor = new Floor(Mock.Of<IItemFactory>(), _random.Object)
        {
            Level = 5,
            HasMetMerchant = true
        };

        _merchant = new MerchantHelper(
            _input.Object, _output.Object, _random.Object,
            Mock.Of<IRecipeDao>(), Mock.Of<IRuneDao>(), Mock.Of<IIngredientDao>());

        // Exit immediately unless a test explicitly selects a service.
        _input.Setup(i => i.ReadInt(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns((string _, int? max, bool _) => max ?? 1);
    }

    private Weapon AddWeapon(int level = 4, bool cursed = false)
    {
        var weapon = new Weapon(_random.Object)
        {
            Name = "Test Sword", RequiredLevel = level, Weight = 1m,
            Value = 20m, AttackPower = 10
        };
        if (cursed) weapon.Curse();
        _player.Inventory.AddItem(weapon);
        return weapon;
    }

    private Armor AddArmor(int level = 4, bool cursed = false)
    {
        var armor = new Armor(_random.Object)
        {
            Name = "Test Armor", RequiredLevel = level, Weight = 1m,
            Value = 20m, DefensePower = 5, Resistance = 3
        };
        if (cursed) armor.Curse();
        _player.Inventory.AddItem(armor);
        return armor;
    }

    private void ChooseService(int option)
    {
        _menuCalls = 0;
        _input.Setup(i => i.ReadInt(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns((string _, int? max, bool _) => ++_menuCalls == 1 ? option : max ?? 1);
    }

    private void Select(Item? item) =>
        _input.Setup(i => i.SelectItem(It.IsAny<string>(), It.IsAny<List<Item>>(), It.IsAny<string?>()))
            .Returns(item);

    private void Meet() => _merchant.Meeting(_floor, _player);
    private bool HasMessage(string fragment) =>
        _messages.Any(m => m.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    private bool HasOption(string label) =>
        _messages.Any(m => m.Contains($". {label}", StringComparison.Ordinal));

    // Menu order: Buy=1, Sell=2, then Purify, Reforge, Enchant (when eligible), Exit.

    [TestMethod]
    public void Meeting_Purify_NoCursedItems_HidesService()
    {
        AddWeapon();
        Meet();
        Assert.IsFalse(HasOption("Purify Item"));
    }

    [TestMethod]
    public void Meeting_Purify_Cancelled_DoesNotModifyItem()
    {
        var weapon = AddWeapon(cursed: true);
        ChooseService(3);
        Select(null);
        Meet();
        Assert.IsTrue(weapon.IsCursed());
        Assert.AreEqual(100m, _player.Inventory.Gold);
    }

    [TestMethod]
    public void Meeting_Purify_CursedItem_RemovesCurse()
    {
        var weapon = AddWeapon(cursed: true);
        ChooseService(3);
        Select(weapon);
        Meet();
        Assert.IsFalse(weapon.IsCursed());
        Assert.AreEqual(90m, _player.Inventory.Gold); // floor(20 * 0.5)
        Assert.IsTrue(HasMessage("Test Sword has been purified"));
    }

    [TestMethod]
    public void Meeting_Purify_Failure_DisplaysError()
    {
        var weapon = AddWeapon(cursed: true);
        _player.Inventory.Gold = 0;
        ChooseService(3);
        Select(weapon);
        Meet();
        Assert.IsTrue(weapon.IsCursed());
        Assert.IsTrue(HasMessage("issue with your purification"));
        Assert.IsTrue(HasMessage("short by"));
    }

    [TestMethod]
    public void Meeting_Reforge_NoEligibleArmor_HidesService()
    {
        AddWeapon();
        Meet();
        Assert.IsFalse(HasOption("Reforge Armor"));
    }

    [TestMethod]
    public void Meeting_Reforge_Cancelled_DoesNotModifyArmor()
    {
        var armor = AddArmor();
        ChooseService(3);
        Select(null);
        Meet();
        Assert.AreEqual(5, armor.DefensePower);
        Assert.AreEqual(3, armor.Resistance);
        Assert.AreEqual(100m, _player.Inventory.Gold);
    }

    [TestMethod]
    public void Meeting_Reforge_ValidArmor_UpdatesStats()
    {
        var armor = AddArmor();
        ChooseService(3);
        Select(armor);
        Meet();
        Assert.AreEqual(0, armor.DefensePower); // Next(totalPower) returns 0
        Assert.AreEqual(8, armor.Resistance);
        Assert.AreEqual(94m, _player.Inventory.Gold); // floor(20 * 0.33)
    }

    [TestMethod]
    public void Meeting_Reforge_Success_DisplaysOldAndNewStats()
    {
        var armor = AddArmor();
        ChooseService(3);
        Select(armor);
        Meet();
        Assert.IsTrue(HasMessage("UPDATED [Test Armor]"));
        Assert.IsTrue(HasMessage("DEF: 5 --> 0"));
        Assert.IsTrue(HasMessage("RES: 3 --> 8"));
    }

    [TestMethod]
    public void Meeting_Reforge_Failure_DisplaysError()
    {
        var armor = AddArmor();
        _player.Inventory.Gold = 0;
        ChooseService(3);
        Select(armor);
        Meet();
        Assert.AreEqual(5, armor.DefensePower);
        Assert.AreEqual(3, armor.Resistance);
        Assert.IsTrue(HasMessage("issue with reforging"));
        Assert.IsTrue(HasMessage("short by"));
    }

    [TestMethod]
    public void Meeting_Enchant_NoEligibleItems_HidesService()
    {
        AddWeapon(level: 5);
        AddArmor(level: 5);
        Meet();
        Assert.IsFalse(HasOption("Enchant Item"));
    }

    [TestMethod]
    public void Meeting_Enchant_Cancelled_DoesNotModifyItem()
    {
        var weapon = AddWeapon(level: 4);
        ChooseService(3);
        Select(null);
        Meet();
        Assert.AreEqual(4, weapon.RequiredLevel);
        Assert.AreEqual(10, weapon.AttackPower);
        Assert.AreEqual(100m, _player.Inventory.Gold);
    }

    [TestMethod]
    public void Meeting_Enchant_Weapon_IncreasesRequiredLevelAndAttack()
    {
        var weapon = AddWeapon(level: 4);
        ChooseService(3);
        Select(weapon);
        Meet();
        Assert.AreEqual(5, weapon.RequiredLevel);
        Assert.IsTrue(weapon.AttackPower > 10);
        Assert.AreEqual(70m, _player.Inventory.Gold); // floor(20 * 1.5)
    }

    [TestMethod]
    public void Meeting_Enchant_Armor_IncreasesRequiredLevelAndStats()
    {
        var armor = AddArmor(level: 4);
        // Reforge is option 3, Enchant is option 4.
        ChooseService(4);
        Select(armor);
        Meet();
        Assert.AreEqual(5, armor.RequiredLevel);
        Assert.IsTrue(armor.DefensePower > 5);
        Assert.IsTrue(armor.Resistance > 3);
        Assert.AreEqual(70m, _player.Inventory.Gold);
    }

    [TestMethod]
    public void Meeting_Enchant_Success_DisplaysOldAndNewStats()
    {
        var weapon = AddWeapon(level: 4);
        ChooseService(3);
        Select(weapon);
        Meet();
        Assert.IsTrue(HasMessage("Test Sword | LVL: 4 --> 5"));
        Assert.IsTrue(HasMessage("ATK: 10 -->"));
    }

    [TestMethod]
    public void Meeting_Enchant_Failure_DisplaysError()
    {
        var weapon = AddWeapon(level: 4);
        _player.Inventory.Gold = 0;
        ChooseService(3);
        Select(weapon);
        Meet();
        Assert.AreEqual(4, weapon.RequiredLevel);
        Assert.AreEqual(10, weapon.AttackPower);
        Assert.IsTrue(HasMessage("issue with the enchantment process"));
        Assert.IsTrue(HasMessage("short by"));
    }
}
