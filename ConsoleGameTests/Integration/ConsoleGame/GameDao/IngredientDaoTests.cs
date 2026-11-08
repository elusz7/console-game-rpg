using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Integration.ConsoleGame.GameDao;

[TestClass]
public class IngredientDaoTests
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
        var ingredient1 = CreateIngredient("Test Ingredient 1", ComponentType.Essence);
        var ingredient2 = CreateIngredient("Test Ingredient 2", ComponentType.Core);

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

        var retrievedIngredient = ingredients[0].Ingredient;

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
        var ingredient = CreateIngredient("Test Ingredient", ComponentType.MonsterPart);

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
        var ingredient1 = CreateIngredient("Sample", ComponentType.MonsterPart);
        var ingredient2 = CreateIngredient("Test", ComponentType.Core);

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
        var ingredient = CreateIngredient("Test Ingredient", ComponentType.MonsterPart);

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
        var ingredient = CreateIngredient("Test Ingredient 1", ComponentType.MonsterPart);

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
        var ingredient = CreateIngredient("Sample", ComponentType.MonsterPart);
        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        Assert.ThrowsException<InventoryException>(() => _ingredientDao.AddIngredientToPlayer(ingredient.Id, 99));
    }

    [TestMethod]
    public void AddIngredientToPlayer_ThrowsInvalidOperationException_WhenIngredientDefinitionDoesNotExist()
    {
        var player = CreateAndSavePlayer("Test Player");
        _context.ChangeTracker.Clear();

        Assert.ThrowsException<InvalidOperationException>(() => _ingredientDao.AddIngredientToPlayer(999, player.Id));
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

        _context.Players.Add(player);
        _context.SaveChanges();

        return player;
    }

    [TestMethod]
    public void GetEssenceByRarity_WhenMatchingEssenceExists_ReturnsEssence()
    {
        // Arrange
        var commonEssence = CreateIngredient(
            "Fading Essence", ComponentType.Essence);

        commonEssence.EssenceRarity = RarityLevel.Common;

        var rareEssence = CreateIngredient(
            "Potent Essence", ComponentType.Essence);

        rareEssence.EssenceRarity = RarityLevel.Rare;

        _context.Ingredients.AddRange(commonEssence, rareEssence);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        // Act
        var result = _ingredientDao.GetEssenceByRarity(RarityLevel.Rare);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(rareEssence.Id, result.Id);
        Assert.AreEqual("Potent Essence", result.Name);
        Assert.AreEqual(ComponentType.Essence, result.ComponentType);
        Assert.AreEqual(RarityLevel.Rare, result.EssenceRarity);
    }

    [TestMethod]
    public void GetEssenceByRarity_WhenRarityDoesNotExist_ReturnsNull()
    {
        // Arrange
        var commonEssence = CreateIngredient(
            "Fading Essence", ComponentType.Essence);

        commonEssence.EssenceRarity = RarityLevel.Common;

        _context.Ingredients.Add(commonEssence);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        // Act
        var result = _ingredientDao.GetEssenceByRarity(RarityLevel.Mythic);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetEssenceByRarity_WhenOnlyNonEssenceIngredientHasRarity_ReturnsNull()
    {
        // Arrange
        var core = CreateIngredient(
            "Fire Core", ComponentType.Core);

        core.EssenceRarity = RarityLevel.Rare;

        _context.Ingredients.Add(core);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        // Act
        var result = _ingredientDao.GetEssenceByRarity(RarityLevel.Rare);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void UpdatePlayerIngredientQuantity_ZeroQuantity_RemovesOwnershipRecord()
    {
        // Arrange
        var player = CreateAndSavePlayer("IngredientUpdateTestPlayer");
        var ingredient = CreateIngredient(
            "Test Ingredient", ComponentType.MonsterPart);

        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        _ingredientDao.AddIngredientToPlayer(ingredient.Id, player.Id, 5);

        // Act
        _ingredientDao.UpdatePlayerIngredientQuantity(
            ingredient.Id, player.Id, 0);

        // Assert
        _context.ChangeTracker.Clear();

        var inventoryIngredient = _context.InventoryIngredients
            .SingleOrDefault(ii =>
                ii.Inventory.PlayerId == player.Id &&
                ii.IngredientId == ingredient.Id);

        Assert.IsNull(inventoryIngredient);
    }

    [TestMethod]
    public void UpdatePlayerIngredientQuantity_IngredientNotOwned_ThrowsInventoryException()
    {
        // Arrange
        var player = CreateAndSavePlayer("IngredientUpdateTestPlayer");
        var ingredient = CreateIngredient(
            "Test Ingredient", ComponentType.MonsterPart);

        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        // Act & Assert
        Assert.ThrowsException<InventoryException>(() => _ingredientDao.UpdatePlayerIngredientQuantity(
            ingredient.Id, player.Id, 5));
    }

    [TestMethod]
    public void UpdatePlayerIngredientQuantity_NegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ingredientDao.UpdatePlayerIngredientQuantity(
            1, 1, -5));
    }

    [TestMethod]
    public void GetTransmutableEssencesByPlayerId_ReturnsEmptyList_WhenPlayerHasNoIngredients()
    {
        var player = CreateAndSavePlayer("Test Player");

        var result = _ingredientDao.GetTransmutableEssencesByPlayerId(player.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTransmutableEssencesByPlayerId_ReturnsEssence_WhenPlayerHasEnoughQuantity()
    {
        var player = CreateAndSavePlayer("Test Player");

        var essence = CreateIngredient(
            "Common Essence", ComponentType.Essence, RarityLevel.Common);
        var nextEssence = CreateIngredient(
            "Uncommon Essence", ComponentType.Essence, RarityLevel.Uncommon);

        _context.Ingredients.AddRange(essence, nextEssence);
        _context.SaveChanges();

        _ingredientDao.AddIngredientToPlayer(essence.Id, player.Id, 5);

        var result = _ingredientDao.GetTransmutableEssencesByPlayerId(player.Id);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(essence.Id, result.Single().IngredientId);
        Assert.AreEqual(5, result.Single().Quantity);
    }

    [TestMethod]
    public void GetTransmutableEssencesByPlayerId_ExcludesEssence_WhenQuantityIsInsufficient()
    {
        var player = CreateAndSavePlayer("Test Player");

        var essence = CreateIngredient(
            "Common Essence", ComponentType.Essence, RarityLevel.Common);
        var nextEssence = CreateIngredient(
            "Uncommon Essence", ComponentType.Essence, RarityLevel.Uncommon);

        _context.Ingredients.AddRange(essence, nextEssence);
        _context.SaveChanges();

        _ingredientDao.AddIngredientToPlayer(essence.Id, player.Id, 4);

        var result = _ingredientDao.GetTransmutableEssencesByPlayerId(player.Id);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTransmutableEssencesByPlayerId_ExcludesNonEssenceIngredients()
    {
        var player = CreateAndSavePlayer("Test Player");

        var ingredient = CreateIngredient("Monster Part", ComponentType.MonsterPart);

        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        _ingredientDao.AddIngredientToPlayer(ingredient.Id, player.Id, 10);

        var result = _ingredientDao.GetTransmutableEssencesByPlayerId(player.Id);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTransmutableEssencesByPlayerId_ExcludesEssenceWithNullRarity()
    {
        var player = CreateAndSavePlayer("Test Player");

        var essence = CreateIngredient("Unknown Essence", ComponentType.Essence);

        _context.Ingredients.Add(essence);
        _context.SaveChanges();

        _ingredientDao.AddIngredientToPlayer(essence.Id, player.Id, 10);

        var result = _ingredientDao.GetTransmutableEssencesByPlayerId(player.Id);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTransmutableEssencesByPlayerId_ExcludesMythicEssence()
    {
        var player = CreateAndSavePlayer("Test Player");

        var essence = CreateIngredient(
            "Mythic Essence", ComponentType.Essence, RarityLevel.Mythic);

        _context.Ingredients.Add(essence);
        _context.SaveChanges();

        _ingredientDao.AddIngredientToPlayer(essence.Id, player.Id, 10);

        var result = _ingredientDao.GetTransmutableEssencesByPlayerId(player.Id);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTransmutableEssencesByPlayerId_ExcludesEssence_WhenNextRarityDoesNotExist()
    {
        var player = CreateAndSavePlayer("Test Player");

        var essence = CreateIngredient(
            "Common Essence", ComponentType.Essence, RarityLevel.Common);

        _context.Ingredients.Add(essence);
        _context.SaveChanges();

        _ingredientDao.AddIngredientToPlayer(essence.Id, player.Id, 10);

        var result = _ingredientDao.GetTransmutableEssencesByPlayerId(player.Id);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTransmutableEssencesByPlayerId_ReturnsOnlyPlayersEssences()
    {
        var player = CreateAndSavePlayer("Test Player");
        var otherPlayer = CreateAndSavePlayer("Other Player");

        var essence = CreateIngredient(
            "Common Essence", ComponentType.Essence, RarityLevel.Common);
        var nextEssence = CreateIngredient(
            "Uncommon Essence", ComponentType.Essence, RarityLevel.Uncommon);

        _context.Ingredients.AddRange(essence, nextEssence);
        _context.SaveChanges();

        _ingredientDao.AddIngredientToPlayer(essence.Id, player.Id, 10);
        _ingredientDao.AddIngredientToPlayer(essence.Id, otherPlayer.Id, 10);

        var result = _ingredientDao.GetTransmutableEssencesByPlayerId(player.Id);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(essence.Id, result.Single().IngredientId);
    }
}