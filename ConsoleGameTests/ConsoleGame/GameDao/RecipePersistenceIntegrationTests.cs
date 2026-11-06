using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGameTests.ConsoleGame.GameDao;

[TestClass]
public class RecipePersistenceIntegrationTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private RecipeDao _recipeDao = null!;

    [TestInitialize]
    public void Initialize()
    {
        // Create and open an isolated in-memory SQLite database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Configure the real GameContext to use SQLite.
        var options = new DbContextOptionsBuilder<GameContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new GameContext(options);

        // Create the database schema.
        _context.Database.EnsureCreated();

        // Use the real DAO
        _recipeDao = new RecipeDao(_context);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void GetRecipeById_ReturnsNull_WhenNotFound()
    {
        var recipe =  _recipeDao.GetRecipeById(1);
        
        Assert.IsNull(recipe);
    }

    [TestMethod]
    public void GetRecipeById_ReturnsRecipe_WhenFound()
    {
        var ingredient1 = CreateIngredient("Test", ComponentType.MonsterPart);
        var ingredient2 = CreateIngredient("Sample", ComponentType.Essence);
        var rune = CreateRune("Wisp", 3, ElementType.Fire, RarityLevel.Common, TestRuneType.Armor);

        var recipe = GetRecipe("No Thanks", ingredient1, ingredient2, rune);
        
        var resultRecipe = _recipeDao.GetRecipeById(recipe.Id);
        Assert.IsNotNull(resultRecipe);
        
        AssertRecipeMatches(ingredient1, ingredient2, rune, recipe, resultRecipe);
    }
    
    [TestMethod]
    public void GetRecipeByRuneId_ReturnsNull_WhenNotFound()
    {
        var recipe =  _recipeDao.GetRecipeByRuneId(1);
        
        Assert.IsNull(recipe);
    }

    [TestMethod]
    public void GetRecipeByRuneId_ReturnsRecipe_WhenFound()
    {
        var ingredient1 = CreateIngredient("Test", ComponentType.MonsterPart);
        var ingredient2 = CreateIngredient("Sample", ComponentType.Core);
        var rune = CreateRune("Wisp", 3, ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);
        
        var recipe = GetRecipe("Try This", ingredient1, ingredient2, rune);
        
        var resultRecipe = _recipeDao.GetRecipeByRuneId(rune.Id);
        Assert.IsNotNull(resultRecipe);
        
        AssertRecipeMatches(ingredient1, ingredient2, rune, recipe, resultRecipe);
    }

    [TestMethod]
    public void GetAllRecipes_ReturnsEmptyList_WhenNoRecipesExist()
    {
        var recipes = _recipeDao.GetAllRecipes();
        Assert.IsNotNull(recipes);
        Assert.AreEqual(0, recipes.Count);
    }

    [TestMethod]
    public void GetAllRecipes_ReturnsAllRecipes_WithRelatedEntities()
    {
        var ingredient1 = CreateIngredient("Test", ComponentType.Essence);
        var ingredient2 = CreateIngredient("Sample", ComponentType.MonsterPart);
        var rune1 = CreateRune("Wisp", 3, ElementType.Fire, RarityLevel.Common, TestRuneType.Armor);
        var recipe1 = GetRecipe("Test1", ingredient1, ingredient2, rune1);
        
        var ingredient3 = CreateIngredient("Test", ComponentType.MonsterPart);
        var ingredient4 = CreateIngredient("Sample", ComponentType.Core);
        var rune2 = CreateRune("Stormy", 4, ElementType.Lightning, RarityLevel.Uncommon, TestRuneType.Weapon);
        var recipe2 = GetRecipe("Test2", ingredient3, ingredient4, rune2);

        var recipes = _recipeDao.GetAllRecipes();
        Assert.IsNotNull(recipes);
        Assert.AreEqual(2, recipes.Count);

        var resultRecipe1 = recipes.Single(r => r.Id == recipe1.Id);
        var resultRecipe2 = recipes.Single(r => r.Id == recipe2.Id);
        
        AssertRecipeMatches(ingredient1, ingredient2, rune1, recipe1, resultRecipe1);
        AssertRecipeMatches(ingredient3, ingredient4, rune2, recipe2, resultRecipe2);
    }
    
    [TestMethod]
    public void GetRecipesByIngredientId_ReturnsEmptyList_WhenNoRecipesMatch()
    {
        var ingredient = CreateIngredient("Unused", ComponentType.MonsterPart);
        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        var recipes = _recipeDao.GetRecipesByIngredientId(ingredient.Id);

        Assert.IsNotNull(recipes);
        Assert.AreEqual(0, recipes.Count);
    }

    [TestMethod]
    public void GetRecipesByIngredientId_ReturnsMatchingRecipes_WithRelatedEntities()
    {
        var ingredient1 = CreateIngredient("Test", ComponentType.MonsterPart);
        var ingredient2 = CreateIngredient("Sample", ComponentType.Essence);
        var rune1 = CreateRune("Wisp", 3, ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);
        var recipe1 = GetRecipe("Test1", ingredient1, ingredient2, rune1);
        
        var ingredient3 = CreateIngredient("Test", ComponentType.Core);
        var rune2 = CreateRune("Stormy", 4, ElementType.Lightning, RarityLevel.Uncommon, TestRuneType.Weapon);
        var recipe2 = GetRecipe("Test2", ingredient2, ingredient3, rune2);
        
        var ingredient4 = CreateIngredient("Sample", ComponentType.MonsterPart);
        var rune3 = CreateRune("Windy", 5, ElementType.Ice, RarityLevel.Epic, TestRuneType.Armor);
        var recipe3 = GetRecipe("Test2", ingredient3, ingredient4, rune3);

        var recipes = _recipeDao.GetRecipesByIngredientId(ingredient2.Id);
        Assert.IsNotNull(recipes);
        Assert.AreEqual(2, recipes.Count);
        Assert.IsFalse(recipes.Any(r => r.Id == recipe3.Id));
        
        var resultRecipe1 = recipes.Single(r => r.Id == recipe1.Id);
        var resultRecipe2 = recipes.Single(r => r.Id == recipe2.Id);
        
        AssertRecipeMatches(ingredient1, ingredient2, rune1, recipe1, resultRecipe1);
        AssertRecipeMatches(ingredient2, ingredient3, rune2, recipe2, resultRecipe2);
    }

    private Recipe GetRecipe(string name, Ingredient ingredient1, Ingredient ingredient2, Rune rune)
    {
        if (ingredient1.Id != 0)
            _context.Ingredients.Attach(ingredient1);

        if (ingredient2.Id != 0 && ingredient2.Id != ingredient1.Id)
            _context.Ingredients.Attach(ingredient2);
        
        var recipe = new Recipe
        {
            Name = name,
            Rune = rune,
            Ingredients =
            [
                new RecipeIngredient
                {
                    Ingredient = ingredient1,
                    Quantity = 5
                },
                new RecipeIngredient
                {
                    Ingredient = ingredient2,
                    Quantity = 10
                }
            ]
        };

        _context.Recipes.Add(recipe);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        return recipe;
    }

    private static void AssertRecipeMatches(Ingredient ingredient1, Ingredient ingredient2, 
        Rune rune, Recipe originalRecipe, Recipe resultRecipe)
    {
        Assert.AreEqual(originalRecipe.Name, resultRecipe.Name);

        var resultRune = resultRecipe.Rune;
        Assert.IsNotNull(resultRune);
        Assert.AreEqual(rune.Id, resultRune.Id);
        Assert.AreEqual(rune.Name, resultRune.Name);
        Assert.AreEqual(rune.Power, resultRune.Power);
        Assert.AreEqual(rune.Element, resultRune.Element);
        Assert.AreEqual(rune.Rarity, resultRune.Rarity);
        
        if (rune is WeaponRune wr1 && resultRune is WeaponRune wr2)
            Assert.AreEqual(wr1.Duration, wr2.Duration);
        
        var resultIngredients = resultRecipe.Ingredients;
        Assert.AreEqual(2, resultIngredients.Count);
        
        var resultIngredient1 = resultIngredients
            .Single(ri => ri.IngredientId == ingredient1.Id);

        var resultIngredient2 = resultIngredients
            .Single(ri => ri.IngredientId == ingredient2.Id);
        
        Assert.AreEqual(ingredient1.Name, resultIngredient1.Ingredient.Name);
        Assert.AreEqual(ingredient1.ComponentType, resultIngredient1.Ingredient.ComponentType);
        Assert.AreEqual(5, resultIngredient1.Quantity);

        Assert.AreEqual(ingredient2.Name, resultIngredient2.Ingredient.Name);
        Assert.AreEqual(ingredient2.ComponentType, resultIngredient2.Ingredient.ComponentType);
        Assert.AreEqual(10, resultIngredient2.Quantity);
    }
}