using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Services;
using ConsoleGame.Services.Interfaces;
using ConsoleGameEntities.Exceptions.Recipes;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Exceptions.Runes;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Integration.ConsoleGame.Services;

[TestClass]
public class CraftingServiceTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private IIngredientDao _ingredientDao = null!;
    private IRecipeDao _recipeDao = null!;
    private IRuneDao _runeDao = null!;
    private ITransactionManager _transactionManager = null!;
    private ICraftingService _craftingService = null!;
    
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

        // Use the real classes
        _ingredientDao = new IngredientDao(_context);
        _recipeDao = new RecipeDao(_context);
        _runeDao = new RuneDao(_context);
        _transactionManager = new TransactionManager(_context);

        _craftingService = new CraftingService(
            _transactionManager,
            _runeDao,
            _ingredientDao,
            _recipeDao);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }
    
    private InventoryIngredient? GetPlayerIngredient(int playerId, int ingredientId)
    {
        _context.ChangeTracker.Clear();
        return _context.InventoryIngredients
            .SingleOrDefault(i =>
                i.Inventory.PlayerId == playerId &&
                i.IngredientId == ingredientId);
    }
    
    private InventoryRune? GetPlayerRune(int playerId, int runeId, int tier)
    {
        _context.ChangeTracker.Clear();

        return _context.InventoryRunes
            .Include(r => r.Rune)
            .Include(r => r.Inventory)
            .Include(r => r.EquippedRunes)
            .SingleOrDefault(r =>
                r.Inventory.PlayerId == playerId &&
                r.RuneId == runeId &&
                r.Tier == tier);
    }
    
    [TestMethod]
    public void CraftRune_WhenRecipeExists_ConsumesIngredientsAndAwardsRune()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        var rune = CreateRune(
            "Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);
        var ingredient = CreateIngredient("Fire Shard", ComponentType.MonsterPart);

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = ingredient,
                Quantity = 5
            }
        ];

        var recipe = CreateRecipe(
            "Fire Rune Recipe",
            rune,
            [
                CreateRecipeIngredient(ingredient, 3)
            ]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);

        _context.SaveChanges();

        // Act
        _craftingService.CraftRune(player.Id, recipe.Id);

        // Assert
        var remainingIngredient = GetPlayerIngredient(player.Id, ingredient.Id);
        var awardedRune = GetPlayerRune(player.Id, rune.Id, 1);

        Assert.IsNotNull(remainingIngredient);
        Assert.AreEqual(2, remainingIngredient.Quantity);

        Assert.IsNotNull(awardedRune);
        Assert.AreEqual(1, awardedRune.Quantity);
        Assert.AreEqual(1, awardedRune.Tier);
    }
    
    [TestMethod]
    public void CraftRune_WhenIngredientQuantityExceedsRequirement_UpdatesRemainder()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        var ingredient = CreateIngredient("Fire Shard", ComponentType.MonsterPart);

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = ingredient,
                Quantity = 8
            }
        ];

        var recipe = CreateRecipe("Fire Rune Recipe", rune,
            [
                CreateRecipeIngredient(ingredient, 3)
            ]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        // Act
        _craftingService.CraftRune(player.Id, recipe.Id);

        // Assert
        var remainingIngredient = GetPlayerIngredient(player.Id, ingredient.Id);

        Assert.IsNotNull(remainingIngredient);
        Assert.AreEqual(5, remainingIngredient.Quantity);
    }
    
    [TestMethod]
    public void CraftRune_WhenIngredientQuantityMatchesRequirement_RemovesIngredient()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        var ingredient = CreateIngredient("Fire Shard", ComponentType.MonsterPart);

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = ingredient,
                Quantity = 3
            }
        ];

        var recipe = CreateRecipe("Fire Rune Recipe", rune,
            [
                CreateRecipeIngredient(ingredient, 3)
            ]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        // Act
        _craftingService.CraftRune(player.Id, recipe.Id);

        // Assert
        var remainingIngredient = GetPlayerIngredient(player.Id, ingredient.Id);

        Assert.IsNull(remainingIngredient);
    }
    
    [TestMethod]
    public void CraftRune_WhenRecipeHasMultipleIngredients_ConsumesAllIngredients()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        var fireShard = CreateIngredient("Fire Shard", ComponentType.MonsterPart);
        var monsterCore = CreateIngredient("Monster Core", ComponentType.Core);

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = fireShard,
                Quantity = 5
            },
            new InventoryIngredient
            {
                Ingredient = monsterCore,
                Quantity = 4
            }
        ];

        var recipe = CreateRecipe("Fire Rune Recipe", rune,
            [
                CreateRecipeIngredient(fireShard, 3),
                CreateRecipeIngredient(monsterCore, 2)
            ]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.AddRange(fireShard, monsterCore);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        // Act
        _craftingService.CraftRune(player.Id, recipe.Id);

        // Assert
        var remainingFireShard = GetPlayerIngredient(player.Id, fireShard.Id);
        var remainingMonsterCore = GetPlayerIngredient(player.Id, monsterCore.Id);

        Assert.IsNotNull(remainingFireShard);
        Assert.AreEqual(2, remainingFireShard.Quantity);

        Assert.IsNotNull(remainingMonsterCore);
        Assert.AreEqual(2, remainingMonsterCore.Quantity);

        var awardedRune = GetPlayerRune(player.Id, rune.Id, 1);
        Assert.IsNotNull(awardedRune);
        Assert.AreEqual(1, awardedRune.Quantity);
    }
    
    [TestMethod]
    public void CraftRune_WhenPlayerLacksIngredient_ThrowsExceptionAndDoesNotModifyInventory()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        var fireShard = CreateIngredient("Fire Shard", ComponentType.MonsterPart);
        var monsterCore = CreateIngredient("Monster Core", ComponentType.Core);

        // Player owns a different ingredient, but not the required Fire Shard.
        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = monsterCore,
                Quantity = 5
            }
        ];

        var recipe = CreateRecipe("Fire Rune Recipe", rune,
        [
            CreateRecipeIngredient(fireShard, 3)
        ]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.AddRange(fireShard, monsterCore);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        // Act
        Assert.ThrowsException<IngredientNotFoundException>(
            () => _craftingService.CraftRune(player.Id, recipe.Id));

        // Assert
        var remainingMonsterCore = GetPlayerIngredient(player.Id, monsterCore.Id);
        var missingFireShard = GetPlayerIngredient(player.Id, fireShard.Id);
        var awardedRune = GetPlayerRune(player.Id, rune.Id, 1);

        Assert.IsNotNull(remainingMonsterCore);
        Assert.AreEqual(5, remainingMonsterCore.Quantity);

        Assert.IsNull(missingFireShard);
        Assert.IsNull(awardedRune);
    }
    
    [TestMethod]
    public void CraftRune_WhenRecipeDoesNotExist_ThrowsExceptionAndDoesNotModifyInventory()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var fireShard = CreateIngredient("Fire Shard", ComponentType.MonsterPart);

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = fireShard,
                Quantity = 5
            }
        ];

        _context.Players.Add(player);
        _context.Ingredients.Add(fireShard);
        _context.SaveChanges();

        // Act
        Assert.ThrowsException<RecipeNotFoundException>(
            () => _craftingService.CraftRune(player.Id, 999));

        // Assert
        var remainingFireShard = GetPlayerIngredient(player.Id, fireShard.Id);
        var awardedRunes = _context.InventoryRunes
            .Where(r => r.Inventory.PlayerId == player.Id)
            .ToList();

        Assert.IsNotNull(remainingFireShard);
        Assert.AreEqual(5, remainingFireShard.Quantity);

        Assert.AreEqual(0, awardedRunes.Count);
    }
    
    [TestMethod]
    public void FuseRune_WhenTierIsOne_FusesIntoTierTwo()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var tierOneRune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.Add(tierOneRune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = tierOneRune,
                Tier = 1,
                Quantity = 3
            }
        ];

        _context.SaveChanges();

        // Act
        _craftingService.FuseRune(player.Id, tierOneRune.Id, 1);

        // Assert
        var remainingTierOne = GetPlayerRune(player.Id, tierOneRune.Id, 1);
        var awardedTierTwo = GetPlayerRune(player.Id, tierOneRune.Id, 2);

        Assert.IsNull(remainingTierOne);

        Assert.IsNotNull(awardedTierTwo);
        Assert.AreEqual(1, awardedTierTwo.Quantity);
        Assert.AreEqual(2, awardedTierTwo.Tier);
        Assert.AreEqual(RarityLevel.Common, awardedTierTwo.Rune.Rarity);
        Assert.IsInstanceOfType<WeaponRune>(awardedTierTwo.Rune);
    }
    
    [TestMethod]
    public void FuseRune_WhenTierIsTwo_FusesIntoTierThree()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var tierTwoRune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.Add(tierTwoRune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = tierTwoRune,
                Tier = 2,
                Quantity = 3
            }
        ];

        _context.SaveChanges();

        // Act
        _craftingService.FuseRune(player.Id, tierTwoRune.Id, 2);

        // Assert
        var remainingTierTwo = GetPlayerRune(player.Id, tierTwoRune.Id, 2);
        var awardedTierThree = GetPlayerRune(player.Id, tierTwoRune.Id, 3);

        Assert.IsNull(remainingTierTwo);

        Assert.IsNotNull(awardedTierThree);
        Assert.AreEqual(1, awardedTierThree.Quantity);
        Assert.AreEqual(3, awardedTierThree.Tier);
        Assert.AreEqual(RarityLevel.Common, awardedTierThree.Rune.Rarity);
        Assert.IsInstanceOfType<WeaponRune>(awardedTierThree.Rune);
    }
    
    [TestMethod]
    public void FuseRune_WhenTierIsThree_FusesIntoNextRarityTierOne()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var tierThreeRune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        var nextRarityRune = CreateRune("Uncommon Fire Rune", 15,
            ElementType.Fire, RarityLevel.Uncommon, TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.AddRange(tierThreeRune, nextRarityRune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = tierThreeRune,
                Tier = 3,
                Quantity = 3
            }
        ];

        _context.SaveChanges();

        // Act
        _craftingService.FuseRune(player.Id, tierThreeRune.Id, 3);

        // Assert
        var remainingTierThree = GetPlayerRune(player.Id, tierThreeRune.Id, 3);
        var awardedTierOne = GetPlayerRune(player.Id, nextRarityRune.Id, 1);

        Assert.IsNull(remainingTierThree);

        Assert.IsNotNull(awardedTierOne);
        Assert.AreEqual(1, awardedTierOne.Quantity);
        Assert.AreEqual(1, awardedTierOne.Tier);
        Assert.AreEqual(RarityLevel.Uncommon, awardedTierOne.Rune.Rarity);
        Assert.AreEqual(ElementType.Fire, awardedTierOne.Rune.Element);
        Assert.IsInstanceOfType<WeaponRune>(awardedTierOne.Rune);
    }
    
    [TestMethod]
    public void FuseRune_WhenMoreThanThreeRunesExist_PreservesRemainder()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
    
        var tierOneRune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);
    
        _context.Players.Add(player);
        _context.Runes.Add(tierOneRune);
        _context.SaveChanges();
    
        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = tierOneRune,
                Tier = 1,
                Quantity = 5
            }
        ];
    
        _context.SaveChanges();
    
        // Act
        _craftingService.FuseRune(player.Id, tierOneRune.Id, 1);
    
        // Assert
        var remainingTierOne = GetPlayerRune(player.Id, tierOneRune.Id, 1);
        var awardedTierTwo = GetPlayerRune(player.Id, tierOneRune.Id, 2);
    
        Assert.IsNotNull(remainingTierOne);
        Assert.AreEqual(2, remainingTierOne.Quantity);
    
        Assert.IsNotNull(awardedTierTwo);
        Assert.AreEqual(1, awardedTierTwo.Quantity);
    }
    
    [TestMethod]
    public void FuseRune_WhenSomeRunesAreEquipped_PreservesEquippedRunes()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = rune,
                Tier = 1,
                Quantity = 4
            }
        ];

        _context.SaveChanges();

        var playerRune = inventory.Runes.Single();

        _context.EquippedRunes.Add(new EquippedRune
        {
            InventoryRune = playerRune
        });

        _context.SaveChanges();

        // Act
        _craftingService.FuseRune(player.Id, rune.Id, 1);

        // Assert
        var remainingTierOne = GetPlayerRune(player.Id, rune.Id, 1);
        var awardedTierTwo = GetPlayerRune(player.Id, rune.Id, 2);

        Assert.IsNotNull(remainingTierOne);
        Assert.AreEqual(1, remainingTierOne.Quantity);
        Assert.AreEqual(1, remainingTierOne.EquippedRunes.Count);

        Assert.IsNotNull(awardedTierTwo);
        Assert.AreEqual(1, awardedTierTwo.Quantity);
    }
    
    [TestMethod]
    public void FuseRune_WhenAllRunesAreEquipped_DoesNotModifyInventory()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = rune,
                Tier = 1,
                Quantity = 1
            }
        ];

        _context.SaveChanges();

        var playerRune = inventory.Runes.Single();

        _context.EquippedRunes.Add(new EquippedRune
        {
            InventoryRune = playerRune
        });

        _context.SaveChanges();

        // Act
        Assert.ThrowsException<RuneQuantityException>(
            () => _craftingService.FuseRune(player.Id, rune.Id, 1));

        // Assert
        var remainingTierOne = GetPlayerRune(player.Id, rune.Id, 1);
        var awardedTierTwo = GetPlayerRune(player.Id, rune.Id, 2);

        Assert.IsNotNull(remainingTierOne);
        Assert.AreEqual(1, remainingTierOne.Quantity);
        Assert.AreEqual(1, remainingTierOne.EquippedRunes.Count);

        Assert.IsNull(awardedTierTwo);
    }
    
    [TestMethod]
    public void FuseRune_WhenResultingRuneDoesNotExist_DoesNotConsumeRunes()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune("Fire Rune", 10,
            ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = rune,
                Tier = 3,
                Quantity = 3
            }
        ];

        _context.SaveChanges();

        // Act
        Assert.ThrowsException<RuneNotFoundException>(
            () => _craftingService.FuseRune(player.Id, rune.Id, 3));

        // Assert
        var remainingTierThree = GetPlayerRune(player.Id, rune.Id, 3);

        Assert.IsNotNull(remainingTierThree);
        Assert.AreEqual(3, remainingTierThree.Quantity);

        var awardedRunes = _context.InventoryRunes
            .Where(r => r.Inventory.PlayerId == player.Id)
            .ToList();

        Assert.AreEqual(1, awardedRunes.Count);
    }
    
    [TestMethod]
    public void FuseRune_WhenAwardingResultFails_RollsBackRuneConsumption()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Fire Rune",
            10,
            ElementType.Fire,
            RarityLevel.Common,
            TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = rune,
                Tier = 1,
                Quantity = 3
            }
        ];

        _context.SaveChanges();

        _context.Database.ExecuteSqlRaw($"""
                                         CREATE TRIGGER FailTierTwoRuneAward
                                         BEFORE INSERT ON InventoryRunes
                                         WHEN NEW.RuneId = {rune.Id} AND NEW.Tier = 2
                                         BEGIN
                                             SELECT RAISE(ABORT, 'Simulated rune award failure');
                                         END;
                                         """);

        // Act
        Assert.ThrowsException<DbUpdateException>(() =>
            _craftingService.FuseRune(player.Id, rune.Id, 1));

        // Assert
        var remainingTierOne = GetPlayerRune(player.Id, rune.Id, 1);
        var awardedTierTwo = GetPlayerRune(player.Id, rune.Id, 2);

        Assert.IsNotNull(remainingTierOne);
        Assert.AreEqual(3, remainingTierOne.Quantity);

        Assert.IsNull(awardedTierTwo);
    }
    
    [TestMethod]
    public void DestroyRune_WhenRuneIsDestroyed_ReturnsRecipeIngredientsAndConsumesRune()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredient = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [CreateRecipeIngredient(ingredient, 4)]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 1 }
        ];
        _context.SaveChanges();

        _craftingService.DestroyRune(player.Id, rune.Id, 1);

        Assert.IsNull(GetPlayerRune(player.Id, rune.Id, 1));

        var refundedIngredient = GetPlayerIngredient(player.Id, ingredient.Id);
        Assert.IsNotNull(refundedIngredient);
        Assert.AreEqual(2, refundedIngredient.Quantity);
    }
    
    [TestMethod]
    public void DestroyRune_WhenRuneHasMultipleCopies_ConsumesOnlyOne()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredient = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [CreateRecipeIngredient(ingredient, 4)]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 3 }
        ];
        _context.SaveChanges();

        _craftingService.DestroyRune(player.Id, rune.Id, 1);

        var remainingRune = GetPlayerRune(player.Id, rune.Id, 1);
        Assert.IsNotNull(remainingRune);
        Assert.AreEqual(2, remainingRune.Quantity);
    }
    
    [TestMethod]
    public void DestroyRune_WhenOnlyOneUnequippedCopyExists_RemovesRuneInventoryRecord()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredient = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [CreateRecipeIngredient(ingredient, 4)]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 1 }
        ];
        _context.SaveChanges();

        _craftingService.DestroyRune(player.Id, rune.Id, 1);

        Assert.IsNull(GetPlayerRune(player.Id, rune.Id, 1));
    }
    
    [TestMethod]
    public void DestroyRune_WhenSomeCopiesAreEquipped_PreservesEquippedCopies()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredient = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [CreateRecipeIngredient(ingredient, 4)]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 4 }
        ];
        _context.SaveChanges();

        var playerRune = inventory.Runes.Single();

        _context.EquippedRunes.Add(new EquippedRune
        {
            InventoryRune = playerRune
        });
        _context.SaveChanges();

        _craftingService.DestroyRune(player.Id, rune.Id, 1);

        var remainingRune = GetPlayerRune(player.Id, rune.Id, 1);

        Assert.IsNotNull(remainingRune);
        Assert.AreEqual(3, remainingRune.Quantity);
        Assert.AreEqual(1, remainingRune.EquippedRunes.Count);
    }
    
    [TestMethod]
    public void DestroyRune_WhenRecipeHasMultipleIngredients_ReturnsAllIngredients()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredientOne = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var ingredientTwo = CreateIngredient("Monster Scale", ComponentType.MonsterPart);
        var ingredientThree = CreateIngredient("Monster Fang", ComponentType.MonsterPart);

        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [
                CreateRecipeIngredient(ingredientOne, 4),
                CreateRecipeIngredient(ingredientTwo, 6),
                CreateRecipeIngredient(ingredientThree, 3)
            ]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.AddRange(ingredientOne, ingredientTwo, ingredientThree);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 1 }
        ];
        _context.SaveChanges();

        _craftingService.DestroyRune(player.Id, rune.Id, 1);

        Assert.IsNull(GetPlayerRune(player.Id, rune.Id, 1));

        Assert.AreEqual(2, GetPlayerIngredient(player.Id, ingredientOne.Id)!.Quantity);
        Assert.AreEqual(3, GetPlayerIngredient(player.Id, ingredientTwo.Id)!.Quantity);
        Assert.AreEqual(1, GetPlayerIngredient(player.Id, ingredientThree.Id)!.Quantity);
    }
    
    [TestMethod]
    public void DestroyRune_WhenTierIsOne_ReturnsBaseRefundAmounts()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredient = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [CreateRecipeIngredient(ingredient, 6)]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 1 }
        ];
        _context.SaveChanges();

        _craftingService.DestroyRune(player.Id, rune.Id, 1);

        Assert.AreEqual(3, GetPlayerIngredient(player.Id, ingredient.Id)!.Quantity);
    }
    
    [TestMethod]
    public void DestroyRune_WhenTierIsTwo_ReturnsIncreasedRefundAmounts()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredient = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [CreateRecipeIngredient(ingredient, 6)]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 2, Quantity = 1 }
        ];
        _context.SaveChanges();

        _craftingService.DestroyRune(player.Id, rune.Id, 2);

        Assert.AreEqual(4, GetPlayerIngredient(player.Id, ingredient.Id)!.Quantity);
    }
    
    [TestMethod]
    public void DestroyRune_WhenTierIsThree_ReturnsIncreasedRefundAmounts()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredient = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [CreateRecipeIngredient(ingredient, 6)]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 3, Quantity = 1 }
        ];
        _context.SaveChanges();

        _craftingService.DestroyRune(player.Id, rune.Id, 3);

        Assert.AreEqual(5, GetPlayerIngredient(player.Id, ingredient.Id)!.Quantity);
    }
    
    [TestMethod]
    public void DestroyRune_WhenRecipeDoesNotExist_DoesNotModifyInventory()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 1 }
        ];
        _context.SaveChanges();

        Assert.ThrowsException<RecipeNotFoundException>(
            () => _craftingService.DestroyRune(player.Id, rune.Id, 1));

        var remainingRune = GetPlayerRune(player.Id, rune.Id, 1);

        Assert.IsNotNull(remainingRune);
        Assert.AreEqual(1, remainingRune.Quantity);
        Assert.AreEqual(0, _context.InventoryIngredients.Count());
    }
    
    [TestMethod]
    public void DestroyRune_WhenRecipeHasNoIngredients_DoesNotModifyInventory()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var recipe = CreateRecipe("Test Recipe", rune, []);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 1 }
        ];
        _context.SaveChanges();

        Assert.ThrowsException<InvalidRecipeException>(
            () => _craftingService.DestroyRune(player.Id, rune.Id, 1));

        var remainingRune = GetPlayerRune(player.Id, rune.Id, 1);

        Assert.IsNotNull(remainingRune);
        Assert.AreEqual(1, remainingRune.Quantity);
        Assert.AreEqual(0, _context.InventoryIngredients.Count());
    }
    
    [TestMethod]
    public void DestroyRune_WhenLaterIngredientRefundFails_RollsBackEarlierRefunds()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
    
        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire,
            RarityLevel.Common, TestRuneType.Weapon);
    
        var ingredientOne = CreateIngredient(
            "Monster Claw", ComponentType.MonsterPart);
    
        var ingredientTwo = CreateIngredient(
            "Monster Scale", ComponentType.MonsterPart);
    
        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [
                CreateRecipeIngredient(ingredientOne, 4),
                CreateRecipeIngredient(ingredientTwo, 6)
            ]);
    
        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.AddRange(ingredientOne, ingredientTwo);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();
    
        inventory.Runes =
        [
            new InventoryRune
            {
                Rune = rune,
                Tier = 1,
                Quantity = 1
            }
        ];
    
        _context.SaveChanges();
    
        // Reject the second ingredient refund.
        _context.Database.ExecuteSqlRaw($"""
            CREATE TRIGGER FailSecondIngredientRefund
            BEFORE INSERT ON InventoryIngredients
            WHEN NEW.IngredientId = {ingredientTwo.Id}
            BEGIN
                SELECT RAISE(ABORT, 'Simulated ingredient refund failure');
            END;
            """);
    
        // Act
        Assert.ThrowsException<DbUpdateException>(() =>
            _craftingService.DestroyRune(player.Id, rune.Id, 1));
    
        // Assert
        var remainingRune = GetPlayerRune(player.Id, rune.Id, 1);
    
        var refundedIngredientOne = GetPlayerIngredient(
            player.Id, ingredientOne.Id);
    
        var refundedIngredientTwo = GetPlayerIngredient(
            player.Id, ingredientTwo.Id);
    
        // The rune must be restored.
        Assert.IsNotNull(remainingRune);
        Assert.AreEqual(1, remainingRune.Quantity);
    
        // Neither ingredient refund should persist.
        Assert.IsNull(refundedIngredientOne);
        Assert.IsNull(refundedIngredientTwo);
    }
    
    [TestMethod]
    public void TransmuteEssence_WhenEssenceIsTransmuted_ConsumesFiveAndAwardsNextRarity()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var commonEssence = CreateIngredient("Common Essence", ComponentType.Essence);
        commonEssence.EssenceRarity = RarityLevel.Common;

        var uncommonEssence = CreateIngredient("Uncommon Essence", ComponentType.Essence);
        uncommonEssence.EssenceRarity = RarityLevel.Uncommon;

        _context.Players.Add(player);
        _context.Ingredients.AddRange(commonEssence, uncommonEssence);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = commonEssence,
                Quantity = 5
            }
        ];
        _context.SaveChanges();

        _craftingService.TransmuteEssence(player.Id, commonEssence.Id);

        Assert.IsNull(GetPlayerIngredient(player.Id, commonEssence.Id));

        var awardedEssence = GetPlayerIngredient(player.Id, uncommonEssence.Id);
        Assert.IsNotNull(awardedEssence);
        Assert.AreEqual(1, awardedEssence.Quantity);
    }
    
    [TestMethod]
    public void TransmuteEssence_WhenMoreThanFiveEssencesExist_PreservesRemainder()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var commonEssence = CreateIngredient("Common Essence", ComponentType.Essence);
        commonEssence.EssenceRarity = RarityLevel.Common;

        var uncommonEssence = CreateIngredient("Uncommon Essence", ComponentType.Essence);
        uncommonEssence.EssenceRarity = RarityLevel.Uncommon;

        _context.Players.Add(player);
        _context.Ingredients.AddRange(commonEssence, uncommonEssence);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = commonEssence,
                Quantity = 8
            }
        ];
        _context.SaveChanges();

        _craftingService.TransmuteEssence(player.Id, commonEssence.Id);

        var remainingEssence = GetPlayerIngredient(player.Id, commonEssence.Id);
        Assert.IsNotNull(remainingEssence);
        Assert.AreEqual(3, remainingEssence.Quantity);

        var awardedEssence = GetPlayerIngredient(player.Id, uncommonEssence.Id);
        Assert.IsNotNull(awardedEssence);
        Assert.AreEqual(1, awardedEssence.Quantity);
    }
    
    [TestMethod]
    public void TransmuteEssence_WhenTransmutingCommonEssence_AwardsUncommonEssence()
    {
        AssertEssenceTransmutationAwardsNextRarity(
            RarityLevel.Common, RarityLevel.Uncommon);
    }

    [TestMethod]
    public void TransmuteEssence_WhenTransmutingUncommonEssence_AwardsRareEssence()
    {
        AssertEssenceTransmutationAwardsNextRarity(
            RarityLevel.Uncommon, RarityLevel.Rare);
    }

    [TestMethod]
    public void TransmuteEssence_WhenTransmutingRareEssence_AwardsEpicEssence()
    {
        AssertEssenceTransmutationAwardsNextRarity(
            RarityLevel.Rare, RarityLevel.Epic);
    }

    [TestMethod]
    public void TransmuteEssence_WhenTransmutingEpicEssence_AwardsLegendaryEssence()
    {
        AssertEssenceTransmutationAwardsNextRarity(
            RarityLevel.Epic, RarityLevel.Legendary);
    }

    [TestMethod]
    public void TransmuteEssence_WhenTransmutingLegendaryEssence_AwardsMythicEssence()
    {
        AssertEssenceTransmutationAwardsNextRarity(
            RarityLevel.Legendary, RarityLevel.Mythic);
    }
    
    private void AssertEssenceTransmutationAwardsNextRarity(
        RarityLevel currentRarity,
        RarityLevel nextRarity)
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var currentEssence = CreateIngredient(
            $"{currentRarity} Essence", ComponentType.Essence);
        currentEssence.EssenceRarity = currentRarity;

        var nextEssence = CreateIngredient(
            $"{nextRarity} Essence", ComponentType.Essence);
        nextEssence.EssenceRarity = nextRarity;

        _context.Players.Add(player);
        _context.Ingredients.AddRange(currentEssence, nextEssence);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = currentEssence,
                Quantity = 5
            }
        ];
        _context.SaveChanges();

        _craftingService.TransmuteEssence(player.Id, currentEssence.Id);

        Assert.IsNull(GetPlayerIngredient(player.Id, currentEssence.Id));

        var awardedEssence = GetPlayerIngredient(player.Id, nextEssence.Id);
        Assert.IsNotNull(awardedEssence);
        Assert.AreEqual(1, awardedEssence.Quantity);
    }
    
    [TestMethod]
    public void TransmuteEssence_WhenNextRarityEssenceDoesNotExist_DoesNotConsumeOriginalEssence()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var commonEssence = CreateIngredient("Common Essence", ComponentType.Essence);
        commonEssence.EssenceRarity = RarityLevel.Common;

        _context.Players.Add(player);
        _context.Ingredients.Add(commonEssence);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = commonEssence,
                Quantity = 5
            }
        ];
        _context.SaveChanges();

        Assert.ThrowsException<IngredientNotFoundException>(() =>
            _craftingService.TransmuteEssence(player.Id, commonEssence.Id));

        var remainingEssence = GetPlayerIngredient(player.Id, commonEssence.Id);
        Assert.IsNotNull(remainingEssence);
        Assert.AreEqual(5, remainingEssence.Quantity);
    }
    
    [TestMethod]
    public void TransmuteEssence_WhenPlayerDoesNotOwnEnoughEssence_DoesNotModifyInventory()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var commonEssence = CreateIngredient("Common Essence", ComponentType.Essence);
        commonEssence.EssenceRarity = RarityLevel.Common;

        var uncommonEssence = CreateIngredient("Uncommon Essence", ComponentType.Essence);
        uncommonEssence.EssenceRarity = RarityLevel.Uncommon;

        _context.Players.Add(player);
        _context.Ingredients.AddRange(commonEssence, uncommonEssence);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = commonEssence,
                Quantity = 4
            }
        ];
        _context.SaveChanges();

        Assert.ThrowsException<IngredientQuantityException>(() =>
            _craftingService.TransmuteEssence(player.Id, commonEssence.Id));

        var remainingEssence = GetPlayerIngredient(player.Id, commonEssence.Id);
        Assert.IsNotNull(remainingEssence);
        Assert.AreEqual(4, remainingEssence.Quantity);

        Assert.IsNull(GetPlayerIngredient(player.Id, uncommonEssence.Id));
    }
    
    [TestMethod]
    public void TransmuteEssence_WhenAwardingNextEssenceFails_RollsBackConsumption()
    {
        // Arrange
        var archetype = CreateArchetype(
            "Test Archetype", ArchetypeType.Martial);

        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var commonEssence = CreateIngredient(
            "Common Essence", ComponentType.Essence);

        commonEssence.EssenceRarity = RarityLevel.Common;

        var uncommonEssence = CreateIngredient(
            "Uncommon Essence", ComponentType.Essence);

        uncommonEssence.EssenceRarity = RarityLevel.Uncommon;

        _context.Players.Add(player);
        _context.Ingredients.AddRange(commonEssence, uncommonEssence);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = commonEssence,
                Quantity = 5
            }
        ];

        _context.SaveChanges();

        // Reject the Uncommon Essence award after Common Essence consumption.
        _context.Database.ExecuteSqlRaw($"""
                                         CREATE TRIGGER FailUncommonEssenceAward
                                         BEFORE INSERT ON InventoryIngredients
                                         WHEN NEW.IngredientId = {uncommonEssence.Id}
                                         BEGIN
                                             SELECT RAISE(ABORT, 'Simulated essence award failure');
                                         END;
                                         """);

        // Act
        Assert.ThrowsException<DbUpdateException>(() =>
            _craftingService.TransmuteEssence(
                player.Id, commonEssence.Id));

        // Assert
        var remainingCommonEssence = GetPlayerIngredient(
            player.Id, commonEssence.Id);

        var awardedUncommonEssence = GetPlayerIngredient(
            player.Id, uncommonEssence.Id);

        // Original essence must be restored.
        Assert.IsNotNull(remainingCommonEssence);
        Assert.AreEqual(5, remainingCommonEssence.Quantity);

        // Failed award must not persist.
        Assert.IsNull(awardedUncommonEssence);
    }
    
    [TestMethod]
    public void CraftRune_WhenLaterIngredientIsMissing_RollsBackEarlierIngredientConsumption()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredientOne = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var ingredientTwo = CreateIngredient("Monster Scale", ComponentType.MonsterPart);

        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [
                CreateRecipeIngredient(ingredientOne, 2),
                CreateRecipeIngredient(ingredientTwo, 1)
            ]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.AddRange(ingredientOne, ingredientTwo);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = ingredientOne,
                Quantity = 2
            }
        ];
        _context.SaveChanges();

        Assert.ThrowsException<IngredientNotFoundException>(() =>
            _craftingService.CraftRune(player.Id, recipe.Id));

        var remainingIngredient = GetPlayerIngredient(player.Id, ingredientOne.Id);
        Assert.IsNotNull(remainingIngredient);
        Assert.AreEqual(2, remainingIngredient.Quantity);

        Assert.IsNull(GetPlayerIngredient(player.Id, ingredientTwo.Id));
        Assert.IsNull(GetPlayerRune(player.Id, rune.Id, 1));
    }
    
    [TestMethod]
    public void CraftRune_WhenLaterIngredientQuantityIsInsufficient_RollsBackEarlierIngredientConsumption()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredientOne = CreateIngredient("Monster Claw", ComponentType.MonsterPart);
        var ingredientTwo = CreateIngredient("Monster Scale", ComponentType.MonsterPart);

        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [
                CreateRecipeIngredient(ingredientOne, 2),
                CreateRecipeIngredient(ingredientTwo, 3)
            ]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.AddRange(ingredientOne, ingredientTwo);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = ingredientOne,
                Quantity = 5
            },
            new InventoryIngredient
            {
                Ingredient = ingredientTwo,
                Quantity = 2
            }
        ];
        _context.SaveChanges();

        // Act
        Assert.ThrowsException<IngredientQuantityException>(() =>
            _craftingService.CraftRune(player.Id, recipe.Id));

        // Assert
        var remainingIngredientOne =
            GetPlayerIngredient(player.Id, ingredientOne.Id);

        var remainingIngredientTwo =
            GetPlayerIngredient(player.Id, ingredientTwo.Id);

        var awardedRune = GetPlayerRune(player.Id, rune.Id, 1);

        // Earlier ingredient consumption must be rolled back.
        Assert.IsNotNull(remainingIngredientOne);
        Assert.AreEqual(5, remainingIngredientOne.Quantity);

        // The insufficient ingredient must also remain unchanged.
        Assert.IsNotNull(remainingIngredientTwo);
        Assert.AreEqual(2, remainingIngredientTwo.Quantity);

        // No rune should be awarded.
        Assert.IsNull(awardedRune);
    }

    
    [TestMethod]
    public void FuseRune_WhenOperationFails_DoesNotPersistPartialChanges()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Legendary,
            TestRuneType.Weapon);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 3, Quantity = 3 }
        ];
        _context.SaveChanges();

        Assert.ThrowsException<RuneNotFoundException>(() =>
            _craftingService.FuseRune(player.Id, rune.Id, 3));

        var remainingRune = GetPlayerRune(player.Id, rune.Id, 3);
        Assert.IsNotNull(remainingRune);
        Assert.AreEqual(3, remainingRune.Quantity);
    }
    
    [TestMethod]
    public void DestroyRune_WhenOperationFails_DoesNotPersistPartialChanges()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var recipe = CreateRecipe("Test Recipe", rune, []);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Runes =
        [
            new InventoryRune { Rune = rune, Tier = 1, Quantity = 2 }
        ];
        _context.SaveChanges();

        Assert.ThrowsException<InvalidRecipeException>(() =>
            _craftingService.DestroyRune(player.Id, rune.Id, 1));

        var remainingRune = GetPlayerRune(player.Id, rune.Id, 1);
        Assert.IsNotNull(remainingRune);
        Assert.AreEqual(2, remainingRune.Quantity);

        Assert.AreEqual(0, _context.InventoryIngredients.Count());
    }
    
    [TestMethod]
    public void TransmuteEssence_WhenOperationFails_DoesNotPersistPartialChanges()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var commonEssence = CreateIngredient("Common Essence", ComponentType.Essence);
        commonEssence.EssenceRarity = RarityLevel.Common;

        _context.Players.Add(player);
        _context.Ingredients.Add(commonEssence);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = commonEssence,
                Quantity = 5
            }
        ];
        _context.SaveChanges();

        Assert.ThrowsException<IngredientNotFoundException>(() =>
            _craftingService.TransmuteEssence(player.Id, commonEssence.Id));

        var remainingEssence = GetPlayerIngredient(player.Id, commonEssence.Id);
        Assert.IsNotNull(remainingEssence);
        Assert.AreEqual(5, remainingEssence.Quantity);

        Assert.AreEqual(1, _context.InventoryIngredients.Count());
    }
    
    [TestMethod]
    public void CraftingService_WhenOperationSucceeds_PersistsChangesToDatabase()
    {
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var rune = CreateRune(
            "Test Rune", 10, ElementType.Fire, RarityLevel.Common,
            TestRuneType.Weapon);

        var ingredient = CreateIngredient("Monster Claw", ComponentType.MonsterPart);

        var recipe = CreateRecipe(
            "Test Recipe",
            rune,
            [CreateRecipeIngredient(ingredient, 2)]);

        _context.Players.Add(player);
        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        inventory.Ingredients =
        [
            new InventoryIngredient
            {
                Ingredient = ingredient,
                Quantity = 5
            }
        ];
        _context.SaveChanges();

        _craftingService.CraftRune(player.Id, recipe.Id);

        // Clear tracked entities and reload from the database.
        _context.ChangeTracker.Clear();

        var persistedIngredient = GetPlayerIngredient(player.Id, ingredient.Id);
        Assert.IsNotNull(persistedIngredient);
        Assert.AreEqual(3, persistedIngredient.Quantity);

        var persistedRune = GetPlayerRune(player.Id, rune.Id, 1);
        Assert.IsNotNull(persistedRune);
        Assert.AreEqual(1, persistedRune.Quantity);
    }
}