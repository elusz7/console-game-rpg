using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGameTests.ConsoleGame.GameDao;

[TestClass]
public class IngredientPersistenceIntegrationTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private IngredientDao _ingredientDao = null!;

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
        _ingredientDao = new IngredientDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void GetIngredientsByPlayerId_ReturnsEmptyList_WhenPlayerHasNoIngredients()
    {
        var player = CreateAndSavePlayer("Test Player");

        var ingredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);

        Assert.AreEqual(0, ingredients.Count);
    }

    [TestMethod]
    public void GetIngredientsByPlayerId_ReturnsOnlyPlayersIngredients()
    {
        var player = CreateAndSavePlayer("Test Player");
        var otherPlayer = CreateAndSavePlayer("Other Player");
        var ingredient1 = CreateIngredient("Test Ingredient 1");
        var ingredient2 = CreateIngredient("Test Ingredient 2");

        _context.Ingredients.AddRange(ingredient1, ingredient2);
        _context.SaveChanges();

        _context.InventoryIngredients.AddRange(
            new InventoryIngredient()
            {
                InventoryId = player.Inventory.Id,
                IngredientId = ingredient1.Id,
                Quantity = 1
            },
            new InventoryIngredient()
            {
                InventoryId = otherPlayer.Inventory.Id,
                IngredientId = ingredient2.Id,
                Quantity = 1
            });
        _context.SaveChanges();

        // Clear tracking so the DAO query reads from SQLite.
        _context.ChangeTracker.Clear();

        var ingredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);

        Assert.IsNotNull(ingredients);
        Assert.AreEqual(1, ingredients.Count);

        var retrievedIngredient = ingredients.First().Ingredient;

        Assert.AreEqual(ingredient1.Id, retrievedIngredient.Id);
        Assert.AreEqual(ingredient1.Name, retrievedIngredient.Name);
        Assert.AreEqual(ingredient1.ComponentType, retrievedIngredient.ComponentType);
    }

    [TestMethod]
    public void GetIngredientDefinitionById_ReturnsNull_WhenNotFound()
    {
        var ingredient = _ingredientDao.GetIngredientDefinitionById(999);

        Assert.IsNull(ingredient);
    }

    [TestMethod]
    public void GetIngredientDefinitionById_ReturnsIngredient_WhenFound()
    {
        var ingredient = CreateIngredient("Test Ingredient");

        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        var retrievedIngredient =
            _ingredientDao.GetIngredientDefinitionById(ingredient.Id);

        Assert.IsNotNull(retrievedIngredient);
        Assert.AreEqual(ingredient.Id, retrievedIngredient.Id);
        Assert.AreEqual(ingredient.Name, retrievedIngredient.Name);
        Assert.AreEqual(ingredient.ComponentType, retrievedIngredient.ComponentType);
    }

    [TestMethod]
    public void GetAllIngredients_ReturnsAllDefinitions()
    {
        var ingredient1 = CreateIngredient("Sample");
        var ingredient2 = CreateIngredient("Test");

        _context.Ingredients.AddRange(ingredient1, ingredient2);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        var ingredients = _ingredientDao.GetAllIngredients();
        Assert.IsNotNull(ingredients);
        Assert.AreEqual(2, ingredients.Count);

        var retIngredient1 = ingredients.Single(i => i.Name.Equals("Sample"));
        var retIngredient2 = ingredients.Single(i => i.Name.Equals("Test"));

        Assert.AreEqual(ingredient1.Id, retIngredient1.Id);
        Assert.AreEqual(ingredient1.ComponentType, retIngredient1.ComponentType);
        Assert.AreEqual(ingredient2.Id, retIngredient2.Id);
        Assert.AreEqual(ingredient2.ComponentType, retIngredient2.ComponentType);
    }

    [TestMethod]
    public void AddIngredientToPlayer_ThrowsArgumentOutOfRangeException_WhenQuantityIsZero()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            _ingredientDao.AddIngredientToPlayer(1, 1, 0));
    }

    [TestMethod]
    public void AddIngredientToPlayer_ThrowsArgumentOutOfRangeException_WhenQuantityIsNegative()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            _ingredientDao.AddIngredientToPlayer(1, 1, -5));
    }

    [TestMethod]
    public void AddIngredientToPlayer_AddsIngredient_WhenPlayerDoesNotOwnIt()
    {
        var player = CreateAndSavePlayer("Test Player");
        var ingredient = CreateIngredient("Test Ingredient");

        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        _ingredientDao.AddIngredientToPlayer(ingredient.Id, player.Id, 5);
        _context.ChangeTracker.Clear();

        var inventoryIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);

        Assert.IsNotNull(inventoryIngredients);
        Assert.AreEqual(1, inventoryIngredients.Count);

        Assert.AreEqual(ingredient.Id, inventoryIngredients[0].IngredientId);
        Assert.AreEqual(5, inventoryIngredients[0].Quantity);
    }

    [TestMethod]
    public void AddIngredientToPlayer_IncrementsQuantity_WhenPlayerAlreadyOwnsIngredient()
    {
        var player = CreateAndSavePlayer("Test Player");
        var ingredient = CreateIngredient("Test Ingredient 1");

        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        _context.InventoryIngredients.AddRange(
            new InventoryIngredient()
            {
                InventoryId = player.Inventory.Id,
                IngredientId = ingredient.Id,
                Quantity = 1
            });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        _ingredientDao.AddIngredientToPlayer(ingredient.Id, player.Id);
        _context.ChangeTracker.Clear();

        var inventoryIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);

        Assert.IsNotNull(inventoryIngredients);
        Assert.AreEqual(1, inventoryIngredients.Count);

        Assert.AreEqual(ingredient.Id, inventoryIngredients[0].IngredientId);
        Assert.AreEqual(2, inventoryIngredients[0].Quantity);
    }

    [TestMethod]
    public void AddIngredientToPlayer_ThrowsInventoryException_WhenPlayerDoesNotExist()
    {
        var ingredient = CreateIngredient("Sample");
        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        Assert.ThrowsException<InventoryException>(
            () => _ingredientDao.AddIngredientToPlayer(ingredient.Id, 99));
    }

    [TestMethod]
    public void AddIngredientToPlayer_ThrowsInvalidOperationException_WhenIngredientDefinitionDoesNotExist()
    {
        var player =  CreateAndSavePlayer("Test Player");
        _context.ChangeTracker.Clear();

        Assert.ThrowsException<InvalidOperationException>(
            () => _ingredientDao.AddIngredientToPlayer(999, player.Id) );
    }
    
    private Player CreateAndSavePlayer(string name)
    {
        var archetype = new Archetype
        {
            Name = "Warrior",
            Description = "A strong melee fighter.",
            HealthBase = 10,
            ArchetypeType = ArchetypeType.Martial,
            ResourceName = "Stamina",
            MaxResource = 20,
            RecoveryRate = 5,
            RecoveryGrowth = 1,
            AttackMultiplier = 1.0M,
            MagicMultiplier = 0.5M,
            DefenseMultiplier = 1.0M,
            ResistanceMultiplier = 0.5M,
            SpeedMultiplier = 0.5M,
            ResourceMultiplier = 1.0M
        };

        var player = new Player
        {
            Name = name,
            Archetype = archetype
        };

        player.Initialize();

        _context.Players!.Add(player);
        _context.SaveChanges();

        return player;
    }
    
    private static Ingredient CreateIngredient(string name)
    {
        return new Ingredient()
        {
            Name = name,
            ComponentType = ComponentType.Essence
        };
    }
}