using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.MerchantHelperTests;

[TestClass]
public class FirstMeetingTests
{
    private Mock<IInputManager> _input = null!;
    private Mock<IOutputManager> _output = null!;
    private Mock<IRandomProvider> _random = null!;
    private Mock<IRecipeDao> _recipes = null!;
    private Mock<IRuneDao> _runes = null!;
    private Mock<IIngredientDao> _ingredients = null!;
    private Mock<IItemFactory> _items = null!;
    private MerchantHelper _helper = null!;
    private Floor _floor = null!;
    private Player _player = null!;

    [TestInitialize]
    public void Setup()
    {
        _input = new Mock<IInputManager>();
        _output = new Mock<IOutputManager>();
        _random = new Mock<IRandomProvider>();
        _recipes = new Mock<IRecipeDao>();
        _runes = new Mock<IRuneDao>();
        _ingredients = new Mock<IIngredientDao>();
        _items = new Mock<IItemFactory>();

        _random.Setup(r => r.Next(It.IsAny<int>())).Returns(0);
        _recipes.Setup(d => d.GetAllRecipes()).Returns(new List<Recipe>());
        _items.Setup(f => f.GetMerchantGifts(It.IsAny<ArchetypeType>(), It.IsAny<int>()))
            .Returns(new List<Item>());
        _input.Setup(i => i.ReadInt(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns(3); // Buy, Sell, Exit: only these options with empty inventory.

        _player = new Player
        {
            Id = 42,
            Name = "Test Hero",
            Level = 5,
            Archetype = new Archetype { ArchetypeType = ArchetypeType.Martial },
            Inventory = new Inventory { Id = 10, Capacity = 100, Gold = 100 }
        };
        _player.Inventory.Player = _player;

        _floor = new Floor(_items.Object, _random.Object) { Level = 5 };
        _helper = new MerchantHelper(_input.Object, _output.Object, _random.Object,
            _recipes.Object, _runes.Object, _ingredients.Object);
    }

    private void Meet() => _helper.Meeting(_floor, _player);

    private void SetRecipes(params Recipe[] recipes) =>
        _recipes.Setup(d => d.GetAllRecipes()).Returns(recipes.ToList());

    private void SetGifts(params Item[] items) =>
        _items.Setup(f => f.GetMerchantGifts(It.IsAny<ArchetypeType>(), It.IsAny<int>()))
            .Returns(items.ToList());

    private void SetRandomIndices(params int[] indices)
    {
        var queue = new Queue<int>(indices);
        _random.Setup(r => r.Next(It.IsAny<int>()))
            .Returns((int max) => queue.Count > 0 ? queue.Dequeue() % max : 0);
    }

    private static Ingredient Ingredient(int id) => new()
    {
        Id = id,
        Name = $"Ingredient {id}"
    };

    private static Recipe MakeRecipe(int id, RarityLevel rarity = RarityLevel.Common,
        params (Ingredient ingredient, int quantity)[] requirements)
    {
        var rune = new WeaponRune
        {
            Id = 100 + id,
            Name = $"Rune {id}",
            Rarity = rarity
        };
        var recipe = new Recipe
        {
            Id = id,
            Name = $"Recipe {id}",
            RuneId = rune.Id,
            Rune = rune,
            Ingredients = []
        };
        rune.Recipe = recipe;
        recipe.Ingredients = requirements.Select((x, index) => new RecipeIngredient
        {
            Id = id * 100 + index,
            RecipeId = id,
            Recipe = recipe,
            IngredientId = x.ingredient.Id,
            Ingredient = x.ingredient,
            Quantity = x.quantity
        }).ToList();
        return recipe;
    }

    private void VerifyOutputContains(string expected) =>
        _output.Verify(o => o.WriteLine(
            It.Is<string>(s => s.Contains(expected)), It.IsAny<ConsoleColor>()),
            Times.AtLeastOnce());

    // FIRST MEETING AND REGULAR GIFTS

    [TestMethod]
    public void Meeting_FirstVisit_MarksMerchantAsMet()
    {
        Meet();
        Assert.IsTrue(_floor.HasMetMerchant);
    }

    [TestMethod]
    public void Meeting_FirstVisit_DisplaysIntroduction()
    {
        Meet();
        VerifyOutputContains("You approach the stranger");
        VerifyOutputContains(_player.Name);
        VerifyOutputContains("Senmothis");
    }

    [TestMethod]
    public void Meeting_FirstVisit_GivesArchetypeSpecificItems()
    {
        var weapon = new Weapon { Name = "Starter Sword", Weight = 1 };
        var armor = new Armor { Name = "Starter Armor", Weight = 2 };
        var potion = new Consumable { Name = "Starter Potion" };
        SetGifts(weapon, armor, potion);

        Meet();

        CollectionAssert.AreEquivalent(new Item[] { weapon, armor, potion },
            _player.Inventory.Items);
        _items.Verify(f => f.GetMerchantGifts(ArchetypeType.Martial, 5), Times.Once());
    }

    [TestMethod]
    public void Meeting_FirstVisit_InventoryFull_HandlesGiftFailure()
    {
        _player.Inventory.Capacity = 0;
        var heavy = new Weapon { Name = "Too Heavy", Weight = 10 };
        var valuable = new Valuable { Name = "Light Gift" };
        SetGifts(heavy, valuable);

        Meet();

        Assert.IsFalse(_player.Inventory.Items.Contains(heavy));
        Assert.IsTrue(_player.Inventory.Items.Contains(valuable));
        Assert.IsTrue(_floor.HasMetMerchant);
    }

    [TestMethod]
    public void Meeting_FirstVisit_NoRegularGifts_StillGivesRuneStarterPack()
    {
        SetRecipes(MakeRecipe(1, RarityLevel.Common, (Ingredient(1), 2)));

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(101, 42, 1, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(1, 42, 1), Times.Once());
    }

    [TestMethod]
    public void Meeting_FirstVisit_GivesRuneStarterPackOnlyOnce()
    {
        SetRecipes(MakeRecipe(1, RarityLevel.Common, (Ingredient(1), 1)));

        Meet();
        // On return, the standard menu offers Buy, Sell, Exit.
        _helper.Meeting(_floor, _player);

        _runes.Verify(d => d.AddRuneToPlayer(It.IsAny<int>(), 42, 1, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(It.IsAny<int>(), 42, 1), Times.Once());
        _recipes.Verify(d => d.GetAllRecipes(), Times.Once());
    }

    [TestMethod]
    public void Meeting_FirstVisit_DisplaysFarewell()
    {
        Meet();
        VerifyOutputContains("Come back soon");
        VerifyOutputContains("smiles and waves goodbye");
    }

    // RUNE STARTER PACK

    [TestMethod]
    public void Meeting_FirstVisit_GiftsOneCommonRune()
    {
        SetRecipes(MakeRecipe(1), MakeRecipe(2), MakeRecipe(3));

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(101, 42, 1, 1), Times.Once());
        _runes.Verify(d => d.AddRuneToPlayer(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Once());
    }

    [TestMethod]
    public void Meeting_FirstVisit_GiftedRuneIsTierOne()
    {
        SetRecipes(MakeRecipe(1));

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(It.IsAny<int>(), 42, 1, 1), Times.Once());
    }

    [TestMethod]
    public void Meeting_FirstVisit_SelectsRuneUsingRandomProvider()
    {
        SetRecipes(MakeRecipe(1), MakeRecipe(2), MakeRecipe(3));
        SetRandomIndices(2, 0, 0, 0);

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(103, 42, 1, 1), Times.Once());
        _random.Verify(r => r.Next(3), Times.AtLeastOnce());
    }

    [TestMethod]
    public void Meeting_FirstVisit_GiftsIngredientsFromThreeRecipes()
    {
        SetRecipes(
            MakeRecipe(1, RarityLevel.Common, (Ingredient(1), 1)),
            MakeRecipe(2, RarityLevel.Common, (Ingredient(2), 1)),
            MakeRecipe(3, RarityLevel.Common, (Ingredient(3), 1)),
            MakeRecipe(4, RarityLevel.Common, (Ingredient(4), 1)));
        SetRandomIndices(0, 0, 0, 0); // rune 1; recipes 1, 2, 3

        Meet();

        foreach (var id in new[] { 1, 2, 3 })
            _ingredients.Verify(d => d.AddIngredientToPlayer(id, 42, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(4, 42, 1), Times.Never());
    }

    [TestMethod]
    public void Meeting_FirstVisit_SelectsRecipesWithoutReplacement()
    {
        SetRecipes(
            MakeRecipe(1, RarityLevel.Common, (Ingredient(1), 1)),
            MakeRecipe(2, RarityLevel.Common, (Ingredient(2), 1)),
            MakeRecipe(3, RarityLevel.Common, (Ingredient(3), 1)),
            MakeRecipe(4, RarityLevel.Common, (Ingredient(4), 1)));
        SetRandomIndices(0, 0, 0, 0);

        Meet();

        foreach (var id in new[] { 1, 2, 3 })
            _ingredients.Verify(d => d.AddIngredientToPlayer(id, 42, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(It.IsAny<int>(), 42, 1),
            Times.Exactly(3));
    }

    [TestMethod]
    public void Meeting_FirstVisit_SharedIngredients_AwardsOneCopyEach()
    {
        var shared = Ingredient(1);
        SetRecipes(
            MakeRecipe(1, RarityLevel.Common, (shared, 1), (Ingredient(2), 1)),
            MakeRecipe(2, RarityLevel.Common, (shared, 1), (Ingredient(3), 1)),
            MakeRecipe(3, RarityLevel.Common, (shared, 1)));

        Meet();

        foreach (var id in new[] { 1, 2, 3 })
            _ingredients.Verify(d => d.AddIngredientToPlayer(id, 42, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(It.IsAny<int>(), 42, 1),
            Times.Exactly(3));
    }

    [TestMethod]
    public void Meeting_FirstVisit_RecipeRequiresMultipleIngredients_AwardsOneCopy()
    {
        SetRecipes(MakeRecipe(1, RarityLevel.Common, (Ingredient(1), 9)));

        Meet();

        _ingredients.Verify(d => d.AddIngredientToPlayer(1, 42, 1), Times.Once());
    }

    [TestMethod]
    public void Meeting_FirstVisit_ExcludesNonCommonRecipes()
    {
        SetRecipes(
            MakeRecipe(1, RarityLevel.Common, (Ingredient(1), 1)),
            MakeRecipe(2, RarityLevel.Uncommon, (Ingredient(2), 1)),
            MakeRecipe(3, RarityLevel.Rare, (Ingredient(3), 1)));

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(101, 42, 1, 1), Times.Once());
        _runes.Verify(d => d.AddRuneToPlayer(102, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        _runes.Verify(d => d.AddRuneToPlayer(103, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        _ingredients.Verify(d => d.AddIngredientToPlayer(1, 42, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(2, 42, 1), Times.Never());
        _ingredients.Verify(d => d.AddIngredientToPlayer(3, 42, 1), Times.Never());
    }

    [TestMethod]
    public void Meeting_FirstVisit_GiftedRuneRecipeMayBeSelected()
    {
        SetRecipes(
            MakeRecipe(1, RarityLevel.Common, (Ingredient(1), 1)),
            MakeRecipe(2, RarityLevel.Common, (Ingredient(2), 1)),
            MakeRecipe(3, RarityLevel.Common, (Ingredient(3), 1)),
            MakeRecipe(4, RarityLevel.Common, (Ingredient(4), 1)));
        SetRandomIndices(0, 0, 0, 0);

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(101, 42, 1, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(1, 42, 1), Times.Once());
    }

    [TestMethod]
    public void Meeting_FirstVisit_FewerThanThreeCommonRecipes_UsesAvailableRecipes()
    {
        SetRecipes(
            MakeRecipe(1, RarityLevel.Common, (Ingredient(1), 1)),
            MakeRecipe(2, RarityLevel.Common, (Ingredient(2), 1)));

        Meet();

        _ingredients.Verify(d => d.AddIngredientToPlayer(1, 42, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(2, 42, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(It.IsAny<int>(), 42, 1),
            Times.Exactly(2));
    }

    [TestMethod]
    public void Meeting_FirstVisit_NoCommonRecipes_SkipsRuneStarterPack()
    {
        SetRecipes(MakeRecipe(1, RarityLevel.Uncommon), MakeRecipe(2, RarityLevel.Rare));

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        _ingredients.Verify(d => d.AddIngredientToPlayer(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<int>()), Times.Never());
    }

    [TestMethod]
    public void Meeting_FirstVisit_NoRecipes_SkipsRuneStarterPack()
    {
        Meet();

        _recipes.Verify(d => d.GetAllRecipes(), Times.Once());
        _runes.Verify(d => d.AddRuneToPlayer(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        _ingredients.Verify(d => d.AddIngredientToPlayer(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<int>()), Times.Never());
    }

    [TestMethod]
    public void Meeting_FirstVisit_RecipesWithoutIngredients_GiftsRuneOnly()
    {
        SetRecipes(MakeRecipe(1), MakeRecipe(2));

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(It.IsAny<int>(), 42, 1, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<int>()), Times.Never());
    }

    [TestMethod]
    public void Meeting_FirstVisit_GiftsToCorrectPlayer()
    {
        _player.Id = 987;
        SetRecipes(MakeRecipe(1, RarityLevel.Common, (Ingredient(5), 1)));

        Meet();

        _runes.Verify(d => d.AddRuneToPlayer(101, 987, 1, 1), Times.Once());
        _ingredients.Verify(d => d.AddIngredientToPlayer(5, 987, 1), Times.Once());
    }

    [TestMethod]
    public void Meeting_FirstVisit_DisplaysRuneAndIngredientGiftMessages()
    {
        SetRecipes(MakeRecipe(1, RarityLevel.Common, (Ingredient(5), 1)));

        Meet();

        VerifyOutputContains("Rune 1 (Tier 1) has been gifted to you!");
        VerifyOutputContains("Ingredient 5 has been gifted to you!");
    }
}
