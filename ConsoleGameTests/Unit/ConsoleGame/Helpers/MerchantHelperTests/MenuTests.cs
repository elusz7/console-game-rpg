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
public class MenuTests
{
    private Mock<IInputManager> _input = null!;
    private Mock<IOutputManager> _output = null!;
    private Mock<IRandomProvider> _random = null!;
    private Mock<IItemFactory> _itemFactory = null!;
    private MerchantHelper _merchant = null!;
    private Floor _floor = null!;
    private Player _player = null!;
    private List<string> _messages = null!;
    private List<int> _menuSizes = null!;

    [TestInitialize]
    public void Setup()
    {
        _input = new Mock<IInputManager>();
        _output = new Mock<IOutputManager>();
        _random = new Mock<IRandomProvider>();
        _itemFactory = new Mock<IItemFactory>();
        _messages = [];
        _menuSizes = [];

        _output.Setup(o => o.WriteLine(It.IsAny<string>(), It.IsAny<ConsoleColor>()))
            .Callback<string, ConsoleColor>((message, _) => _messages.Add(message));

        // Exit is always the final menu option, regardless of available services.
        _input.Setup(i => i.ReadInt(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns((string _, int? max, bool _) =>
            {
                _menuSizes.Add(max ?? 0);
                return max ?? 1;
            });

        _player = new Player
        {
            Id = 17,
            Name = "Test Hero",
            Level = 5,
            Archetype = new Archetype { ArchetypeType = ArchetypeType.Martial },
            Inventory = new Inventory { Id = 9, Gold = 123, Capacity = 100m }
        };
        _player.Inventory.Player = _player;

        _floor = new Floor(_itemFactory.Object, _random.Object)
        {
            Level = 5,
            HasMetMerchant = true
        };

        _merchant = new MerchantHelper(
            _input.Object,
            _output.Object,
            _random.Object,
            Mock.Of<IRecipeDao>(),
            Mock.Of<IRuneDao>(),
            Mock.Of<IIngredientDao>());
    }

    private void Meet() => _merchant.Meeting(_floor, _player);

    private bool HasMessage(string text) =>
        _messages.Any(m => m.Contains(text, StringComparison.OrdinalIgnoreCase));

    private void AssertOptionAbsent(string label) =>
        Assert.IsFalse(_messages.Any(m => m.Contains($". {label}", StringComparison.Ordinal)),
            $"The menu should not contain '{label}'.");

    private void AssertOptionPresent(string label) =>
        Assert.IsTrue(_messages.Any(m => m.Contains($". {label}", StringComparison.Ordinal)),
            $"The menu should contain '{label}'.");

    private Weapon AddWeapon(int requiredLevel = 5, bool cursed = false)
    {
        var weapon = new Weapon
        {
            Name = "Test Sword",
            RequiredLevel = requiredLevel,
            Weight = 1m,
            Value = 10m,
            AttackPower = 5
        };
        if (cursed) weapon.Curse();
        _player.Inventory.AddItem(weapon);
        return weapon;
    }

    private Armor AddArmor(int requiredLevel = 5)
    {
        var armor = new Armor
        {
            Name = "Test Armor",
            RequiredLevel = requiredLevel,
            Weight = 1m,
            Value = 10m,
            DefensePower = 5,
            Resistance = 3
        };
        _player.Inventory.AddItem(armor);
        return armor;
    }

    [TestMethod]
    public void Meeting_ReturnVisit_DisplaysMerchantMenu()
    {
        Meet();

        AssertOptionPresent("Buy Item");
        Assert.AreEqual(1, _menuSizes.Count);
        Assert.IsTrue(_floor.HasMetMerchant);
    }

    [TestMethod]
    public void Meeting_ReturnVisit_DisplaysAvailableGold()
    {
        Meet();

        Assert.IsTrue(HasMessage("Available Gold: 123"));
    }

    [TestMethod]
    public void Meeting_ReturnVisit_AlwaysOffersBuySellAndExit()
    {
        Meet();

        AssertOptionPresent("Buy Item");
        AssertOptionPresent("Sell Item");
        AssertOptionPresent("Exit");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_NoCursedItems_HidesPurifyOption()
    {
        AddWeapon();
        Meet();

        AssertOptionAbsent("Purify Item");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_CursedItem_ShowsPurifyOption()
    {
        AddWeapon(cursed: true);
        Meet();

        AssertOptionPresent("Purify Item");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_NoReforgableItems_HidesReforgeOption()
    {
        AddWeapon();
        Meet();

        AssertOptionAbsent("Reforge Armor");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_ReforgableItem_ShowsReforgeOption()
    {
        AddArmor();
        Meet();

        AssertOptionPresent("Reforge Armor");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_NoEnchantableItems_HidesEnchantOption()
    {
        Meet();

        AssertOptionAbsent("Enchant Item");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_LowerLevelEnchantableItem_ShowsEnchantOption()
    {
        AddWeapon(requiredLevel: 4);
        Meet();

        AssertOptionPresent("Enchant Item");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_ItemAtPlayerLevel_HidesEnchantOption()
    {
        AddWeapon(requiredLevel: 5);
        Meet();

        AssertOptionAbsent("Enchant Item");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_Exit_EndsMeeting()
    {
        Meet();

        Assert.IsTrue(HasMessage("A pleasure as always, Test Hero"));
        Assert.AreEqual(1, _menuSizes.Count, "Exit should end the menu loop immediately.");
    }

    [TestMethod]
    public void Meeting_ReturnVisit_AfterService_ReturnsToMenu()
    {
        // Select Buy, where empty stock returns immediately, then select Exit.
        var calls = 0;
        _input.Setup(i => i.ReadInt(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns((string _, int? max, bool _) =>
            {
                _menuSizes.Add(max ?? 0);
                return calls++ == 0 ? 1 : max ?? 1;
            });

        Meet();

        Assert.AreEqual(2, _menuSizes.Count, "The menu should appear again after Buy returns.");
        Assert.IsTrue(HasMessage("stock is currently empty"));
        Assert.IsTrue(HasMessage("A pleasure as always"));
    }

    [TestMethod]
    public void Meeting_ReturnVisit_MenuOptionsAreRenumberedCorrectly()
    {
        // With armor only, the menu is Buy=1, Sell=2, Reforge=3, Exit=4.
        // The selected service must be Reforge, not Purify or Enchant.
        AddArmor();
        var calls = 0;
        _input.Setup(i => i.ReadInt(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns((string _, int? max, bool _) =>
            {
                _menuSizes.Add(max ?? 0);
                return calls++ == 0 ? 3 : max ?? 1;
            });
        _input.Setup(i => i.SelectItem(It.IsAny<string>(), It.IsAny<List<Item>>(), It.IsAny<string?>()))
            .Returns((Item?)null);

        Meet();

        Assert.AreEqual(2, _menuSizes.Count);
        Assert.AreEqual(4, _menuSizes[0]);
        AssertOptionPresent("Reforge Armor");
        _input.Verify(i => i.SelectItem(
            It.Is<string>(s => s.Contains("reforge", StringComparison.OrdinalIgnoreCase)),
            It.IsAny<List<Item>>(),
            "reforge"), Times.Once);
        _input.Verify(i => i.SelectItem(
            It.Is<string>(s => s.Contains("purify", StringComparison.OrdinalIgnoreCase)),
            It.IsAny<List<Item>>(),
            "purify"), Times.Never);
    }
}
