using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Items;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Models.FloorTests;

[TestClass]
public class MerchantTests
{
    private Mock<IItemFactory> _itemFactoryMock = null!;
    private Mock<IRandomProvider> _randomProviderMock = null!;
    private Floor _floor = null!;

    [TestInitialize]
    public void Setup()
    {
        _itemFactoryMock = new Mock<IItemFactory>();
        _randomProviderMock = new Mock<IRandomProvider>();

        _floor = new Floor(
            _itemFactoryMock.Object,
            _randomProviderMock.Object)
        {
            Level = 5
        };
    }

    private Item CreateItem(string name)
    {
        return new Valuable
        {
            Name = name,
            Description = "Test item",
            Weight = 1,
            RequiredLevel = 1
        };
    }

    private void SetupMerchantItems(params Item[] items)
    {
        _itemFactoryMock
            .Setup(f => f.CreateMerchantItems(It.IsAny<int>()))
            .Returns(items.ToList());
    }

    // Merchant identity

    [TestMethod]
    public void GetMerchantName_MerchantNotMet_ReturnsMysteriousStranger()
    {
        var name = _floor.GetMerchantName();

        Assert.AreEqual("A Mysterious Stranger", name);
    }

    [TestMethod]
    public void GetMerchantName_MerchantMet_ReturnsSenmothis()
    {
        _floor.HasMetMerchant = true;

        var name = _floor.GetMerchantName();

        Assert.AreEqual("Senmothis", name);
    }

    [TestMethod]
    public void GetMerchantName_MerchantStatusChanges_UpdatesReturnedName()
    {
        Assert.AreEqual(
            "A Mysterious Stranger",
            _floor.GetMerchantName());

        _floor.HasMetMerchant = true;

        Assert.AreEqual(
            "Senmothis",
            _floor.GetMerchantName());
    }

    // Inventory generation

    [TestMethod]
    public void UpdateMerchantItems_CallsItemFactoryWithFloorLevel()
    {
        _floor.UpdateMerchantItems();

        _itemFactoryMock.Verify(
            f => f.CreateMerchantItems(5),
            Times.Once);
    }

    [TestMethod]
    public void UpdateMerchantItems_StoresGeneratedItems()
    {
        var item1 = CreateItem("Sword");
        var item2 = CreateItem("Potion");

        SetupMerchantItems(item1, item2);

        _floor.UpdateMerchantItems();

        var merchantItems = _floor.GetMerchantItems();

        Assert.AreEqual(2, merchantItems.Count);
        CollectionAssert.Contains(merchantItems.ToList(), item1);
        CollectionAssert.Contains(merchantItems.ToList(), item2);
    }

    [TestMethod]
    public void UpdateMerchantItems_NoItemsGenerated_StoresEmptyList()
    {
        SetupMerchantItems();

        _floor.UpdateMerchantItems();

        Assert.AreEqual(0, _floor.GetMerchantItems().Count);
    }

    [TestMethod]
    public void UpdateMerchantItems_CalledAgain_ReplacesExistingStock()
    {
        var originalItem = CreateItem("Old Sword");
        var newItem = CreateItem("New Sword");

        _itemFactoryMock
            .SetupSequence(f => f.CreateMerchantItems(It.IsAny<int>()))
            .Returns([originalItem])
            .Returns([newItem]);

        _floor.UpdateMerchantItems();

        Assert.AreEqual(1, _floor.GetMerchantItems().Count);
        Assert.AreSame(originalItem, _floor.GetMerchantItems()[0]);

        _floor.UpdateMerchantItems();

        var merchantItems = _floor.GetMerchantItems();

        Assert.AreEqual(1, merchantItems.Count);
        Assert.AreSame(newItem, merchantItems[0]);
        CollectionAssert.DoesNotContain(
            merchantItems.ToList(),
            originalItem);
    }

    // Inventory retrieval

    [TestMethod]
    public void GetMerchantItems_BeforeUpdate_ReturnsEmptyCollection()
    {
        var merchantItems = _floor.GetMerchantItems();

        Assert.IsNotNull(merchantItems);
        Assert.AreEqual(0, merchantItems.Count);
    }

    [TestMethod]
    public void GetMerchantItems_AfterUpdate_ReturnsGeneratedItems()
    {
        var items = new List<Item>
        {
            CreateItem("Sword"),
            CreateItem("Shield")
        };

        SetupMerchantItems(items.ToArray());

        _floor.UpdateMerchantItems();

        var merchantItems = _floor.GetMerchantItems();

        CollectionAssert.AreEqual(
            items,
            merchantItems.ToList());
    }

    [TestMethod]
    public void GetMerchantItems_ReturnsReadOnlyListInterface()
    {
        SetupMerchantItems(CreateItem("Sword"));

        _floor.UpdateMerchantItems();

        IReadOnlyList<Item> merchantItems = _floor.GetMerchantItems();

        Assert.IsNotNull(merchantItems);
        Assert.AreEqual(1, merchantItems.Count);
    }

    // Inventory removal

    [TestMethod]
    public void RemoveMerchantItem_ExistingItem_RemovesItem()
    {
        var item = CreateItem("Sword");

        SetupMerchantItems(item);
        _floor.UpdateMerchantItems();

        _floor.RemoveMerchantItem(item);

        Assert.AreEqual(0, _floor.GetMerchantItems().Count);
    }

    [TestMethod]
    public void RemoveMerchantItem_NonExistingItem_DoesNotModifyInventory()
    {
        var existingItem = CreateItem("Sword");
        var unknownItem = CreateItem("Shield");

        SetupMerchantItems(existingItem);
        _floor.UpdateMerchantItems();

        _floor.RemoveMerchantItem(unknownItem);

        Assert.AreEqual(1, _floor.GetMerchantItems().Count);
        Assert.AreSame(existingItem, _floor.GetMerchantItems()[0]);
    }

    [TestMethod]
    public void RemoveMerchantItem_OnlyItem_LeavesEmptyInventory()
    {
        var item = CreateItem("Sword");

        SetupMerchantItems(item);
        _floor.UpdateMerchantItems();

        _floor.RemoveMerchantItem(item);

        Assert.IsNotNull(_floor.GetMerchantItems());
        Assert.AreEqual(0, _floor.GetMerchantItems().Count);
    }

    [TestMethod]
    public void RemoveMerchantItem_OneOfMultipleItems_PreservesOtherItems()
    {
        var item1 = CreateItem("Sword");
        var item2 = CreateItem("Shield");
        var item3 = CreateItem("Potion");

        SetupMerchantItems(item1, item2, item3);
        _floor.UpdateMerchantItems();

        _floor.RemoveMerchantItem(item2);

        var merchantItems = _floor.GetMerchantItems();

        Assert.AreEqual(2, merchantItems.Count);
        CollectionAssert.Contains(merchantItems.ToList(), item1);
        CollectionAssert.Contains(merchantItems.ToList(), item3);
        CollectionAssert.DoesNotContain(merchantItems.ToList(), item2);
    }

    // Merchant gifts

    [TestMethod]
    public void GetMerchantGifts_CallsItemFactoryWithCorrectArchetype()
    {
        _itemFactoryMock
            .Setup(f => f.GetMerchantGifts(
                ArchetypeType.Magical,
                It.IsAny<int>()))
            .Returns([]);

        _floor.GetMerchantGifts(ArchetypeType.Magical, 5);

        _itemFactoryMock.Verify(
            f => f.GetMerchantGifts(
                ArchetypeType.Magical,
                It.IsAny<int>()),
            Times.Once);
    }

    [TestMethod]
    public void GetMerchantGifts_CallsItemFactoryWithCorrectLevel()
    {
        _itemFactoryMock
            .Setup(f => f.GetMerchantGifts(
                It.IsAny<ArchetypeType>(),
                10))
            .Returns([]);

        _floor.GetMerchantGifts(ArchetypeType.Martial, 10);

        _itemFactoryMock.Verify(
            f => f.GetMerchantGifts(
                It.IsAny<ArchetypeType>(),
                10),
            Times.Once);
    }

    [TestMethod]
    public void GetMerchantGifts_ReturnsFactoryGeneratedGifts()
    {
        var gifts = new List<Item>
        {
            CreateItem("Gift Sword"),
            CreateItem("Gift Potion")
        };

        _itemFactoryMock
            .Setup(f => f.GetMerchantGifts(
                ArchetypeType.Martial,
                5))
            .Returns(gifts);

        var result = _floor.GetMerchantGifts(
            ArchetypeType.Martial,
            5);

        Assert.AreSame(gifts, result);
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetMerchantGifts_NoGiftsAvailable_ReturnsEmptyList()
    {
        _itemFactoryMock
            .Setup(f => f.GetMerchantGifts(
                ArchetypeType.Martial,
                5))
            .Returns([]);

        var gifts = _floor.GetMerchantGifts(
            ArchetypeType.Martial,
            5);

        Assert.IsNotNull(gifts);
        Assert.AreEqual(0, gifts.Count);
    }

    [TestMethod]
    public void GetMerchantGifts_DoesNotModifyMerchantInventory()
    {
        var merchantItem = CreateItem("Merchant Sword");
        var giftItem = CreateItem("Gift Sword");

        SetupMerchantItems(merchantItem);

        _itemFactoryMock
            .Setup(f => f.GetMerchantGifts(
                ArchetypeType.Martial,
                5))
            .Returns([giftItem]);

        _floor.UpdateMerchantItems();

        _floor.GetMerchantGifts(ArchetypeType.Martial, 5);

        var merchantItems = _floor.GetMerchantItems();

        Assert.AreEqual(1, merchantItems.Count);
        Assert.AreSame(merchantItem, merchantItems[0]);
    }
}