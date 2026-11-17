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
public class BuySellTests
{
    private Mock<IInputManager> _input = null!;
    private Mock<IOutputManager> _output = null!;
    private Mock<IItemFactory> _itemFactory = null!;
    private MerchantHelper _merchant = null!;
    private Floor _floor = null!;
    private Player _player = null!;
    private List<string> _messages = null!;
    private List<Item> _stock = null!;
    private int _menuCalls;

    [TestInitialize]
    public void Setup()
    {
        _input = new Mock<IInputManager>();
        _output = new Mock<IOutputManager>();
        _itemFactory = new Mock<IItemFactory>();
        var random = new Mock<IRandomProvider>();
        _messages = [];
        _stock = [];
        _menuCalls = 0;

        _output.Setup(o => o.WriteLine(It.IsAny<string>(), It.IsAny<ConsoleColor>()))
            .Callback<string, ConsoleColor>((message, _) => _messages.Add(message));

        _player = new Player
        {
            Id = 17,
            Name = "Test Hero",
            Level = 5,
            Archetype = new Archetype { ArchetypeType = ArchetypeType.Martial },
            Inventory = new Inventory { Id = 9, Gold = 100, Capacity = 100m }
        };
        _player.Inventory.Player = _player;

        _itemFactory.Setup(f => f.CreateMerchantItems(It.IsAny<int>()))
            .Returns(() => _stock.ToList());
        _floor = new Floor(_itemFactory.Object, random.Object)
        {
            Level = 5,
            HasMetMerchant = true
        };
        _floor.UpdateMerchantItems();

        _merchant = new MerchantHelper(
            _input.Object, _output.Object, random.Object,
            Mock.Of<IRecipeDao>(), Mock.Of<IRuneDao>(), Mock.Of<IIngredientDao>());
    }

    private static Weapon MakeWeapon(string name = "Test Sword", decimal value = 20m, decimal weight = 1m) =>
        new() { Name = name, Value = value, Weight = weight, RequiredLevel = 5, AttackPower = 5 };

    private void Stock(Item item)
    {
        _stock.Add(item);
        _floor.UpdateMerchantItems();
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

    [TestMethod]
    public void Meeting_Buy_NoMerchantStock_DisplaysMessage()
    {
        ChooseService(1);
        Meet();
        Assert.IsTrue(HasMessage("stock is currently empty"));
        _input.Verify(i => i.SelectItem(It.IsAny<string>(), It.IsAny<List<Item>>(), It.IsAny<string?>()), Times.Never);
    }

    [TestMethod]
    public void Meeting_Buy_Cancelled_DoesNotPurchaseItem()
    {
        var item = MakeWeapon();
        Stock(item);
        ChooseService(1);
        Select(null);
        Meet();
        Assert.AreEqual(100, _player.Inventory.Gold);
        Assert.IsFalse(_player.Inventory.Items.Contains(item));
        Assert.IsTrue(_floor.GetMerchantItems().Contains(item));
    }

    [TestMethod]
    public void Meeting_Buy_ValidPurchase_AddsItemToInventory()
    {
        var item = MakeWeapon();
        Stock(item);
        ChooseService(1);
        Select(item);
        Meet();
        CollectionAssert.Contains(_player.Inventory.Items, item);
        Assert.AreSame(_player.Inventory, item.Inventory);
        Assert.IsTrue(HasMessage("has been purchased"));
    }

    [TestMethod]
    public void Meeting_Buy_ValidPurchase_DeductsGold()
    {
        var item = MakeWeapon(value: 23.6m);
        Stock(item);
        ChooseService(1);
        Select(item);
        Meet();
        Assert.AreEqual(100 - (int)Math.Round(item.GetBuyPrice()), _player.Inventory.Gold);
    }

    [TestMethod]
    public void Meeting_Buy_ValidPurchase_RemovesItemFromMerchantStock()
    {
        var item = MakeWeapon();
        Stock(item);
        ChooseService(1);
        Select(item);
        Meet();
        Assert.IsFalse(_floor.GetMerchantItems().Contains(item));
    }

    [TestMethod]
    public void Meeting_Buy_InsufficientGold_DisplaysError()
    {
        var item = MakeWeapon(value: 150m);
        Stock(item);
        ChooseService(1);
        Select(item);
        Meet();
        Assert.IsTrue(HasMessage("issue with your purchase"));
        Assert.IsTrue(HasMessage("short by"));
    }

    [TestMethod]
    public void Meeting_Buy_FailedPurchase_RetainsMerchantStock()
    {
        var item = MakeWeapon(value: 150m);
        Stock(item);
        ChooseService(1);
        Select(item);
        Meet();
        Assert.IsTrue(_floor.GetMerchantItems().Contains(item));
        Assert.IsFalse(_player.Inventory.Items.Contains(item));
        Assert.AreEqual(100, _player.Inventory.Gold);
    }

    [TestMethod]
    public void Meeting_Buy_InventoryFull_DoesNotCompletePurchase()
    {
        var item = MakeWeapon(weight: 10m);
        Stock(item);
        _player.Inventory.Capacity = 0m;
        ChooseService(1);
        Select(item);
        Meet();
        Assert.IsTrue(HasMessage("issue with your purchase"));
        Assert.AreEqual(100, _player.Inventory.Gold);
        Assert.IsFalse(_player.Inventory.Items.Contains(item));
        Assert.IsTrue(_floor.GetMerchantItems().Contains(item));
    }

    [TestMethod]
    public void Meeting_Sell_EmptyInventory_DisplaysMessage()
    {
        ChooseService(2);
        Meet();
        Assert.IsTrue(HasMessage("no items to sell"));
    }

    [TestMethod]
    public void Meeting_Sell_Cancelled_DoesNotSellItem()
    {
        var item = MakeWeapon();
        _player.Inventory.AddItem(item);
        ChooseService(2);
        Select(null);
        Meet();
        Assert.AreEqual(100, _player.Inventory.Gold);
        Assert.IsTrue(_player.Inventory.Items.Contains(item));
    }

    [TestMethod]
    public void Meeting_Sell_ValidSale_RemovesItemFromInventory()
    {
        var item = MakeWeapon();
        _player.Inventory.AddItem(item);
        ChooseService(2);
        Select(item);
        Meet();
        Assert.IsFalse(_player.Inventory.Items.Contains(item));
        Assert.IsNull(item.Inventory);
        Assert.IsTrue(HasMessage("has been sold"));
    }

    [TestMethod]
    public void Meeting_Sell_ValidSale_AddsGold()
    {
        var item = MakeWeapon(value: 22m);
        _player.Inventory.AddItem(item);
        ChooseService(2);
        Select(item);
        Meet();
        Assert.AreEqual(100 + (int)Math.Round(item.GetSellPrice()), _player.Inventory.Gold);
    }

    [TestMethod]
    public void Meeting_Sell_InvalidSale_DisplaysError()
    {
        var owned = MakeWeapon("Owned Sword");
        var unowned = MakeWeapon("Unowned Sword");
        _player.Inventory.AddItem(owned);
        ChooseService(2);
        Select(unowned);
        Meet();
        Assert.IsTrue(HasMessage("issue with your sale"));
        Assert.IsTrue(HasMessage("do not have"));
    }

    [TestMethod]
    public void Meeting_Sell_FailedSale_PreservesItemAndGold()
    {
        var owned = MakeWeapon("Owned Sword");
        var unowned = MakeWeapon("Unowned Sword");
        _player.Inventory.AddItem(owned);
        ChooseService(2);
        Select(unowned);
        Meet();
        Assert.AreEqual(100, _player.Inventory.Gold);
        CollectionAssert.AreEqual(new List<Item> { owned }, _player.Inventory.Items);
        Assert.AreSame(_player.Inventory, owned.Inventory);
    }
}
