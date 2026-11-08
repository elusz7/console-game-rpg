using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Services;
using ConsoleGameEntities.Exceptions.Recipes;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes.Recipes;
using ConsoleGameEntities.Exceptions.Runes;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Services;

[TestClass]
public class CraftingServiceTests
{
    private readonly Mock<ITransactionManager> _transactionManagerMock = new();
    private readonly Mock<IRuneDao> _runeDaoMock = new();
    private readonly Mock<IIngredientDao> _ingredientDaoMock = new();
    private readonly Mock<IRecipeDao> _recipeDaoMock = new();

    private readonly CraftingService _service;

    private const int PlayerId = 1;
    private const int RecipeId = 10;
    private const int RuneId = 20;
    private const int ExpectedRuneId = 21;
    private const int IngredientId = 30;
    private const int ExpectedIngredientId = 40;

    public CraftingServiceTests()
    {
        ConfigureTransactionManager();

        _service = CreateService();
    }

    // --------------------------------------------------
    // CraftRune tests
    // --------------------------------------------------
    [TestMethod]
    public void CraftRune_WhenRecipeDoesNotExist_ThrowsRecipeNotFoundException()
    {
        // Arrange
        _recipeDaoMock
            .Setup(r => r.GetRecipeById(RecipeId))
            .Returns((Recipe?)null);

        // Act & Assert
        Assert.ThrowsException<RecipeNotFoundException>(() => _service.CraftRune(PlayerId, RecipeId));

        _ingredientDaoMock.Verify(
            i => i.GetIngredientsByPlayerId(It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void CraftRune_WhenPlayerDoesNotOwnRequiredIngredient_ThrowsIngredientNotFoundException()
    {
        // Arrange
        var recipe = CreateRecipe(3);

        _recipeDaoMock
            .Setup(r => r.GetRecipeById(RecipeId))
            .Returns(recipe);

        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([]);

        // Act & Assert
        Assert.ThrowsException<IngredientNotFoundException>(() => _service.CraftRune(PlayerId, RecipeId));

        _ingredientDaoMock.Verify(
            i => i.UpdatePlayerIngredientQuantity(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(
                It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void CraftRune_WhenIngredientQuantityIsInsufficient_ThrowsIngredientQuantityException()
    {
        // Arrange
        const int requiredQuantity = 3;
        const int availableQuantity = 2;

        var recipe = CreateRecipe(requiredQuantity);

        _recipeDaoMock
            .Setup(r => r.GetRecipeById(RecipeId))
            .Returns(recipe);

        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([
                CreatePlayerIngredient(availableQuantity)
            ]);

        // Act & Assert
        Assert.ThrowsException<IngredientQuantityException>(() => _service.CraftRune(PlayerId, RecipeId));

        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(
                It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _ingredientDaoMock.Verify(
            i => i.UpdatePlayerIngredientQuantity(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void CraftRune_WhenIngredientQuantityMatchesRequirement_RemovesIngredientFromInventory()
    {
        // Arrange
        const int requiredQuantity = 3;

        var recipe = CreateRecipe(requiredQuantity);

        _recipeDaoMock
            .Setup(r => r.GetRecipeById(RecipeId))
            .Returns(recipe);

        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([
                CreatePlayerIngredient(requiredQuantity)
            ]);

        // Act
        _service.CraftRune(PlayerId, RecipeId);

        // Assert
        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(IngredientId, PlayerId),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.UpdatePlayerIngredientQuantity(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(RuneId, PlayerId, 1, 1),
            Times.Once);
    }

    [TestMethod]
    public void CraftRune_WhenIngredientQuantityExceedsRequirement_UpdatesRemainingQuantity()
    {
        // Arrange
        const int requiredQuantity = 3;
        const int availableQuantity = 5;
        const int expectedRemainder = 2;

        var recipe = CreateRecipe(requiredQuantity);

        _recipeDaoMock
            .Setup(r => r.GetRecipeById(RecipeId))
            .Returns(recipe);

        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([
                CreatePlayerIngredient(availableQuantity)
            ]);

        // Act
        _service.CraftRune(PlayerId, RecipeId);

        // Assert
        _ingredientDaoMock.Verify(
            i => i.UpdatePlayerIngredientQuantity(
                IngredientId, PlayerId, expectedRemainder),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(
                It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void CraftRune_WhenMultipleIngredientsAreRequired_ConsumesAllIngredients()
    {
        // Arrange
        const int secondIngredientId = 31;

        var recipe = CreateRecipe(
            2,
            new RecipeIngredient
            {
                IngredientId = secondIngredientId,
                Quantity = 4
            });

        _recipeDaoMock
            .Setup(r => r.GetRecipeById(RecipeId))
            .Returns(recipe);

        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([
                CreatePlayerIngredient(2),
                CreatePlayerIngredient(4, secondIngredientId)
            ]);

        // Act
        _service.CraftRune(PlayerId, RecipeId);

        // Assert
        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(IngredientId, PlayerId),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(secondIngredientId, PlayerId),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.UpdatePlayerIngredientQuantity(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void CraftRune_WhenCraftingSucceeds_AddsRuneAtTierOne()
    {
        // Arrange
        var recipe = CreateRecipe(3);

        _recipeDaoMock
            .Setup(r => r.GetRecipeById(RecipeId))
            .Returns(recipe);

        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([
                CreatePlayerIngredient(3)
            ]);

        // Act
        _service.CraftRune(PlayerId, RecipeId);

        // Assert
        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(RuneId, PlayerId, 1),
            Times.Once);
    }

    // --------------------------------------------------
    // FuseRune validation tests
    // --------------------------------------------------

    [TestMethod]
    public void FuseRune_WhenTierIsBelowOne_ThrowsRuneFusionException()
    {
        // Act & Assert
        Assert.ThrowsException<RuneFusionException>(() => _service.FuseRune(PlayerId, RuneId, 0));

        _runeDaoMock.Verify(
            r => r.GetRunesByPlayerId(It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void FuseRune_WhenTierIsAboveThree_ThrowsRuneFusionException()
    {
        // Act & Assert
        Assert.ThrowsException<RuneFusionException>(() => _service.FuseRune(PlayerId, RuneId, 4));

        _runeDaoMock.Verify(
            r => r.GetRunesByPlayerId(It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void FuseRune_WhenPlayerDoesNotOwnRune_ThrowsRuneNotFoundException()
    {
        // Arrange
        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([]);

        // Act & Assert
        Assert.ThrowsException<RuneNotFoundException>(() => _service.FuseRune(PlayerId, RuneId, 1));

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void FuseRune_WhenInsufficientUnequippedRunes_ThrowsRuneQuantityException()
    {
        // Arrange
        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([
                CreatePlayerRune(quantity: 2)
            ]);

        // Act & Assert
        Assert.ThrowsException<RuneQuantityException>(() => _service.FuseRune(PlayerId, RuneId, 1));

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.UpdatePlayerRuneQuantity(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void FuseRune_WhenAllRunesAreEquipped_ThrowsRuneQuantityException()
    {
        // Arrange
        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([
                CreatePlayerRune(quantity: 3, equippedCount: 3)
            ]);

        // Act & Assert
        Assert.ThrowsException<RuneQuantityException>(() => _service.FuseRune(PlayerId, RuneId, 1));

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void FuseRune_WhenMythicRuneIsTierThree_ThrowsRuneFusionException()
    {
        // Arrange
        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([
                CreatePlayerRune(
                    quantity: 3,
                    rarity: RarityLevel.Mythic,
                    tier: 3)
            ]);

        // Act & Assert
        Assert.ThrowsException<RuneFusionException>(() => _service.FuseRune(PlayerId, RuneId, 3));

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void FuseRune_WhenResultingRuneDoesNotExist_ThrowsRuneNotFoundException()
    {
        // Arrange
        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([
                CreatePlayerRune(quantity: 3)
            ]);

        _runeDaoMock
            .Setup(r => r.FindRune(
                ElementType.Fire,
                RarityLevel.Common,
                "WeaponRune"))
            .Returns((Rune?)null);

        // Act & Assert
        Assert.ThrowsException<RuneNotFoundException>(() => _service.FuseRune(PlayerId, RuneId, 1));

        // Original runes must not be consumed.
        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(RuneId, PlayerId, 1),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.UpdatePlayerRuneQuantity(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    // --------------------------------------------------
    // FuseRune successful fusion tests
    // --------------------------------------------------
    [TestMethod]
    public void FuseRune_WhenTierOneRuneIsFused_ProducesTierTwoRune()
    {
        // Arrange
        SetupSuccessfulFusion(tier: 1, rarity: RarityLevel.Common);

        // Act
        _service.FuseRune(PlayerId, RuneId, 1);

        // Assert
        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(ExpectedRuneId, PlayerId, 2),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(RuneId, PlayerId, 1),
            Times.Once);
    }

    [TestMethod]
    public void FuseRune_WhenTierTwoRuneIsFused_ProducesTierThreeRune()
    {
        // Arrange
        SetupSuccessfulFusion(tier: 2, rarity: RarityLevel.Common);

        // Act
        _service.FuseRune(PlayerId, RuneId, 2);

        // Assert
        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(ExpectedRuneId, PlayerId, 3),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(RuneId, PlayerId, 2),
            Times.Once);
    }

    [TestMethod]
    public void FuseRune_WhenTierThreeRuneIsFused_ProducesNextRarityTierOneRune()
    {
        // Arrange
        SetupSuccessfulFusion(tier: 3, rarity: RarityLevel.Rare);

        // Act
        _service.FuseRune(PlayerId, RuneId, 3);

        // Assert
        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(ExpectedRuneId, PlayerId, 1),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.FindRune(
                ElementType.Fire, RarityLevel.Epic, "WeaponRune"),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(RuneId, PlayerId, 3),
            Times.Once);
    }

    [TestMethod]
    public void FuseRune_WhenExactlyThreeAvailableRunesExist_RemovesInventoryRecord()
    {
        // Arrange
        SetupSuccessfulFusion(quantity: 3);

        // Act
        _service.FuseRune(PlayerId, RuneId, 1);

        // Assert
        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(RuneId, PlayerId, 1),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.UpdatePlayerRuneQuantity(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void FuseRune_WhenMoreThanThreeRunesExist_UpdatesRemainingQuantity()
    {
        // Arrange
        const int originalQuantity = 5;
        const int expectedRemainder = 2;

        SetupSuccessfulFusion(quantity: originalQuantity);

        // Act
        _service.FuseRune(PlayerId, RuneId, 1);

        // Assert
        _runeDaoMock.Verify(
            r => r.UpdatePlayerRuneQuantity(
                RuneId, PlayerId, 1, expectedRemainder),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void FuseRune_WhenSomeRunesAreEquipped_PreservesEquippedRunes()
    {
        // Arrange
        const int originalQuantity = 4;
        const int equippedCount = 1;
        const int expectedRemainder = 1;

        SetupSuccessfulFusion(
            quantity: originalQuantity,
            equippedCount: equippedCount);

        // Act
        _service.FuseRune(PlayerId, RuneId, 1);

        // Assert
        _runeDaoMock.Verify(
            r => r.UpdatePlayerRuneQuantity(
                RuneId, PlayerId, 1, expectedRemainder),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(ExpectedRuneId, PlayerId, 2),
            Times.Once);
    }

    [DataTestMethod]
    [DataRow(false)] // WeaponRune
    [DataRow(true)] // ArmorRune
    public void FuseRune_WhenFusionSucceeds_PreservesRuneType(bool isArmor)
    {
        // Arrange
        var expectedRune = SetupSuccessfulFusion(isArmor: isArmor);
        var expectedRuneType = isArmor ? "ArmorRune" : "WeaponRune";

        // Act
        _service.FuseRune(PlayerId, RuneId, 1);

        // Assert
        _runeDaoMock.Verify(
            r => r.FindRune(
                ElementType.Fire, RarityLevel.Common, expectedRuneType),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.AddRuneToPlayer(expectedRune.Id, PlayerId, 2),
            Times.Once);
    }

    // --------------------------------------------------
// DestroyRune validation tests
// --------------------------------------------------

    [TestMethod]
    public void DestroyRune_WhenTierIsBelowOne_ThrowsRuneFusionException()
    {
        // Act & Assert
        Assert.ThrowsException<RuneFusionException>(() => _service.DestroyRune(PlayerId, RuneId, 0));
    }

    [TestMethod]
    public void DestroyRune_WhenTierIsAboveThree_ThrowsRuneFusionException()
    {
        // Act & Assert
        Assert.ThrowsException<RuneFusionException>(() => _service.DestroyRune(PlayerId, RuneId, 4));
    }

    [TestMethod]
    public void DestroyRune_WhenPlayerDoesNotOwnRune_ThrowsRuneNotFoundException()
    {
        // Arrange
        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([]);

        // Act & Assert
        Assert.ThrowsException<RuneNotFoundException>(() => _service.DestroyRune(PlayerId, RuneId, 1));

        _recipeDaoMock.Verify(
            r => r.GetRecipeByRuneId(RuneId),
            Times.Never);
    }

    [TestMethod]
    public void DestroyRune_WhenAllRunesAreEquipped_ThrowsRuneQuantityException()
    {
        // Arrange
        var playerRune = CreatePlayerRune(
            quantity: 2,
            equippedCount: 2);

        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([playerRune]);

        // Act & Assert
        Assert.ThrowsException<RuneQuantityException>(() => _service.DestroyRune(PlayerId, RuneId, 1));

        _recipeDaoMock.Verify(
            r => r.GetRecipeByRuneId(RuneId),
            Times.Never);
    }

    [TestMethod]
    public void DestroyRune_WhenRecipeDoesNotExist_ThrowsRecipeNotFoundException()
    {
        // Arrange
        SetupOwnedRuneForDestruction();

        _recipeDaoMock
            .Setup(r => r.GetRecipeByRuneId(RuneId))
            .Returns((Recipe)null!);

        // Act & Assert
        Assert.ThrowsException<RecipeNotFoundException>(() => _service.DestroyRune(PlayerId, RuneId, 1));

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(
                It.IsAny<int>(),
                PlayerId,
                It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void DestroyRune_WhenRecipeHasNoIngredients_ThrowsInvalidRecipeException()
    {
        // Arrange
        SetupOwnedRuneForDestruction();

        var recipe = new Recipe
        {
            Id = RecipeId,
            RuneId = RuneId,
            Name = "Test Rune Recipe",
            Ingredients = []
        };

        _recipeDaoMock
            .Setup(r => r.GetRecipeByRuneId(RuneId))
            .Returns(recipe);

        // Act & Assert
        Assert.ThrowsException<InvalidRecipeException>(() => _service.DestroyRune(PlayerId, RuneId, 1));

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(
                It.IsAny<int>(),
                PlayerId,
                It.IsAny<int>()),
            Times.Never);
    }

// --------------------------------------------------
// DestroyRune ingredient refund tests
// --------------------------------------------------

    [TestMethod]
    public void DestroyRune_WhenTierOneRuneIsDestroyed_ReturnsCorrectIngredientQuantities()
    {
        // Arrange
        SetupRuneAndRecipeForDestruction(tier: 1);

        // Act
        _service.DestroyRune(PlayerId, RuneId, 1);

        // Assert
        VerifyIngredientRefund(31, 1);
        VerifyIngredientRefund(32, 1);
        VerifyIngredientRefund(33, 2);
    }

    [TestMethod]
    public void DestroyRune_WhenTierTwoRuneIsDestroyed_IncreasesIngredientRefunds()
    {
        // Arrange
        SetupRuneAndRecipeForDestruction(tier: 2);

        // Act
        _service.DestroyRune(PlayerId, RuneId, 2);

        // Assert
        VerifyIngredientRefund(31, 2);
        VerifyIngredientRefund(32, 2);
        VerifyIngredientRefund(33, 3);
    }

    [TestMethod]
    public void DestroyRune_WhenTierThreeRuneIsDestroyed_IncreasesIngredientRefunds()
    {
        // Arrange
        SetupRuneAndRecipeForDestruction(tier: 3);

        // Act
        _service.DestroyRune(PlayerId, RuneId, 3);

        // Assert
        VerifyIngredientRefund(31, 3);
        VerifyIngredientRefund(32, 3);
        VerifyIngredientRefund(33, 4);
    }

    [TestMethod]
    public void DestroyRune_WhenMultipleCopiesExist_ConsumesOnlyOneRune()
    {
        // Arrange
        SetupRuneAndRecipeForDestruction(quantity: 3);

        // Act
        _service.DestroyRune(PlayerId, RuneId, 1);

        // Assert
        _runeDaoMock.Verify(
            r => r.UpdatePlayerRuneQuantity(RuneId, PlayerId, 1, 2),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void DestroyRune_WhenOnlyOneUnequippedCopyExists_RemovesInventoryRecord()
    {
        // Arrange
        SetupRuneAndRecipeForDestruction(quantity: 1);

        // Act
        _service.DestroyRune(PlayerId, RuneId, 1);

        // Assert
        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(RuneId, PlayerId, 1),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.UpdatePlayerRuneQuantity(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void DestroyRune_WhenSomeCopiesAreEquipped_PreservesEquippedRunes()
    {
        // Arrange
        SetupRuneAndRecipeForDestruction(
            quantity: 3,
            equippedCount: 1);

        // Act
        _service.DestroyRune(PlayerId, RuneId, 1);

        // Assert
        _runeDaoMock.Verify(
            r => r.UpdatePlayerRuneQuantity(RuneId, PlayerId, 1, 2),
            Times.Once);

        _runeDaoMock.Verify(
            r => r.RemoveRuneFromPlayer(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public void DestroyRune_WhenDestructionSucceeds_ReturnsAllRecipeIngredients()
    {
        // Arrange
        SetupRuneAndRecipeForDestruction();

        // Act
        _service.DestroyRune(PlayerId, RuneId, 1);

        // Assert
        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(31, PlayerId, 1),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(32, PlayerId, 1),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(33, PlayerId, 2),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(
                It.IsAny<int>(),
                PlayerId,
                It.IsAny<int>()),
            Times.Exactly(3));
    }


    // --------------------------------------------------
// TransmuteEssence validation tests
// --------------------------------------------------

    [TestMethod]
    public void TransmuteEssence_WhenPlayerDoesNotOwnEssence_ThrowsIngredientNotFoundException()
    {
        // Arrange
        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([]);

        // Act & Assert
        Assert.ThrowsException<IngredientNotFoundException>(() => _service.TransmuteEssence(PlayerId, IngredientId));

        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(
                IngredientId,
                PlayerId),
            Times.Never);
    }

    [TestMethod]
    public void TransmuteEssence_WhenIngredientIsNotEssence_ThrowsInvalidIngredientException()
    {
        // Arrange
        SetupPlayerIngredientForTransmutation(
            componentType: ComponentType.Core);

        // Act & Assert
        Assert.ThrowsException<InvalidIngredientException>(() => _service.TransmuteEssence(PlayerId, IngredientId));
    }

    [TestMethod]
    public void TransmuteEssence_WhenEssenceHasNoRarity_ThrowsInvalidIngredientException()
    {
        // Arrange
        SetupPlayerIngredientForTransmutation(
            essenceRarity: null);

        // Act & Assert
        Assert.ThrowsException<InvalidIngredientException>(() => _service.TransmuteEssence(PlayerId, IngredientId));
    }

    [TestMethod]
    public void TransmuteEssence_WhenEssenceIsMythic_ThrowsInvalidIngredientException()
    {
        // Arrange
        SetupPlayerIngredientForTransmutation(
            essenceRarity: RarityLevel.Mythic);

        // Act & Assert
        Assert.ThrowsException<InvalidIngredientException>(() => _service.TransmuteEssence(PlayerId, IngredientId));
    }

    [TestMethod]
    public void TransmuteEssence_WhenQuantityIsBelowFive_ThrowsIngredientQuantityException()
    {
        // Arrange
        SetupPlayerIngredientForTransmutation(quantity: 4);

        // Act & Assert
        Assert.ThrowsException<IngredientQuantityException>(() => _service.TransmuteEssence(PlayerId, IngredientId));

        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(
                IngredientId,
                PlayerId),
            Times.Never);
    }

    [TestMethod]
    public void TransmuteEssence_WhenQuantityIsExactlyFive_RemovesOriginalEssence()
    {
        // Arrange
        SetupPlayerIngredientForTransmutation(quantity: 5);

        var nextEssence = new Ingredient
        {
            Id = ExpectedIngredientId,
            Name = "Uncommon Essence",
            ComponentType = ComponentType.Essence,
            EssenceRarity = RarityLevel.Uncommon
        };

        _ingredientDaoMock
            .Setup(i => i.GetEssenceByRarity(RarityLevel.Uncommon))
            .Returns(nextEssence);

        // Act
        _service.TransmuteEssence(PlayerId, IngredientId);

        // Assert
        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(IngredientId, PlayerId),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.UpdatePlayerIngredientQuantity(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(nextEssence.Id, PlayerId, 1),
            Times.Once);
    }

// --------------------------------------------------
// TransmuteEssence success and progression tests
// --------------------------------------------------

    [TestMethod]
    public void TransmuteEssence_WhenQuantityExceedsFive_UpdatesRemainingQuantity()
    {
        // Arrange
        SetupPlayerIngredientForTransmutation(
            quantity: 7,
            essenceRarity: RarityLevel.Common);

        var nextEssence = CreateEssence(40, RarityLevel.Uncommon);

        _ingredientDaoMock
            .Setup(i => i.GetEssenceByRarity(RarityLevel.Uncommon))
            .Returns(nextEssence);

        // Act
        _service.TransmuteEssence(PlayerId, IngredientId);

        // Assert
        _ingredientDaoMock.Verify(
            i => i.UpdatePlayerIngredientQuantity(IngredientId, PlayerId, 2),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(IngredientId, PlayerId),
            Times.Never);

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(nextEssence.Id, PlayerId, 1),
            Times.Once);
    }

    [TestMethod]
    public void TransmuteEssence_WhenNextEssenceDoesNotExist_ThrowsIngredientNotFoundException()
    {
        // Arrange
        SetupPlayerIngredientForTransmutation(
            quantity: 5,
            essenceRarity: RarityLevel.Common);

        _ingredientDaoMock
            .Setup(i => i.GetEssenceByRarity(RarityLevel.Uncommon))
            .Returns((Ingredient)null!);

        // Act & Assert
        Assert.ThrowsException<IngredientNotFoundException>(() => _service.TransmuteEssence(PlayerId, IngredientId));

        // No inventory changes should occur.
        _ingredientDaoMock.Verify(
            i => i.RemoveIngredientFromPlayer(IngredientId, PlayerId),
            Times.Never);

        _ingredientDaoMock.Verify(
            i => i.UpdatePlayerIngredientQuantity(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(
                It.IsAny<int>(),
                PlayerId,
                It.IsAny<int>()),
            Times.Never);
    }

    [DataTestMethod]
    [DataRow(RarityLevel.Common, RarityLevel.Uncommon)]
    [DataRow(RarityLevel.Uncommon, RarityLevel.Rare)]
    [DataRow(RarityLevel.Rare, RarityLevel.Epic)]
    [DataRow(RarityLevel.Epic, RarityLevel.Legendary)]
    [DataRow(RarityLevel.Legendary, RarityLevel.Mythic)]
    public void TransmuteEssence_WhenEssenceIsTransmuted_AddsNextRarityEssence(
        RarityLevel currentRarity,
        RarityLevel expectedRarity)
    {
        // Arrange
        SetupPlayerIngredientForTransmutation(
            quantity: 5,
            essenceRarity: currentRarity);

        var nextEssence = CreateEssence(40, expectedRarity);

        _ingredientDaoMock
            .Setup(i => i.GetEssenceByRarity(expectedRarity))
            .Returns(nextEssence);

        // Act
        _service.TransmuteEssence(PlayerId, IngredientId);

        // Assert
        _ingredientDaoMock.Verify(
            i => i.GetEssenceByRarity(expectedRarity),
            Times.Once);

        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(nextEssence.Id, PlayerId, 1),
            Times.Once);
    }

    [TestMethod]
    public void CraftRune_WhenOperationSucceeds_ExecutesWithinTransaction()
    {
        // Arrange
        const int requiredQuantity = 3;

        var recipe = CreateRecipe(requiredQuantity);

        _recipeDaoMock
            .Setup(r => r.GetRecipeById(RecipeId))
            .Returns(recipe);

        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([
                CreatePlayerIngredient(requiredQuantity)
            ]);

        // Act
        _service.CraftRune(PlayerId, RecipeId);

        // Assert
        _transactionManagerMock.Verify(
            t => t.ExecuteInTransaction(It.IsAny<Action>()),
            Times.Once);
    }

    [TestMethod]
    public void FuseRune_WhenOperationSucceeds_ExecutesWithinTransaction()
    {
        // Arrange
        SetupSuccessfulFusion(quantity: 3);

        // Act
        _service.FuseRune(PlayerId, RuneId, 1);

        // Assert
        _transactionManagerMock.Verify(
            t => t.ExecuteInTransaction(It.IsAny<Action>()),
            Times.Once);
    }

    [TestMethod]
    public void DestroyRune_WhenOperationSucceeds_ExecutesWithinTransaction()
    {
        // Arrange
        SetupRuneAndRecipeForDestruction();

        // Act
        _service.DestroyRune(PlayerId, RuneId, 1);

        // Assert
        _transactionManagerMock.Verify(
            t => t.ExecuteInTransaction(It.IsAny<Action>()),
            Times.Once);
    }

    [TestMethod]
    public void TransmuteEssence_WhenOperationSucceeds_ExecutesWithinTransaction()
    {
        // Arrange
        SetupPlayerIngredientForTransmutation();

        var nextEssence = CreateEssence(40, RarityLevel.Uncommon);

        _ingredientDaoMock
            .Setup(d => d.GetEssenceByRarity(RarityLevel.Uncommon))
            .Returns(nextEssence);

        // Act
        _service.TransmuteEssence(PlayerId, IngredientId);

        // Assert
        _transactionManagerMock.Verify(
            t => t.ExecuteInTransaction(It.IsAny<Action>()),
            Times.Once);
    }

    // --------------------------------------------------
    // Test Helpers
    // --------------------------------------------------
    private CraftingService CreateService()
    {
        return new CraftingService(
            _transactionManagerMock.Object,
            _runeDaoMock.Object,
            _ingredientDaoMock.Object,
            _recipeDaoMock.Object);
    }

    private void ConfigureTransactionManager()
    {
        _transactionManagerMock
            .Setup(t => t.ExecuteInTransaction(It.IsAny<Action>()))
            .Callback<Action>(action => action());
    }

    private static Recipe CreateRecipe(int requiredQuantity, params RecipeIngredient[] additionalIngredients)
    {
        return new Recipe
        {
            Id = RecipeId,
            RuneId = RuneId,
            Name = "Test Rune Recipe",
            Ingredients =
            [
                new RecipeIngredient
                {
                    IngredientId = IngredientId,
                    Quantity = requiredQuantity
                },
                .. additionalIngredients
            ]
        };
    }

    private static InventoryIngredient CreatePlayerIngredient(int quantity, int ingredientId = IngredientId)
    {
        return new InventoryIngredient
        {
            IngredientId = ingredientId,
            Quantity = quantity
        };
    }

    private static InventoryRune CreatePlayerRune(int quantity = 3, int equippedCount = 0, int tier = 1,
        RarityLevel rarity = RarityLevel.Common, bool isArmor = false)
    {
        Rune rune = isArmor
            ? new ArmorRune()
            : new WeaponRune();

        rune.Id = RuneId;
        rune.Name = "Test Rune";
        rune.RuneType = isArmor ? "ArmorRune" : "WeaponRune";
        rune.Element = ElementType.Fire;
        rune.Rarity = rarity;
        rune.Power = 10;

        return new InventoryRune
        {
            RuneId = RuneId,
            Rune = rune,
            Tier = tier,
            Quantity = quantity,
            EquippedRunes =
            [
                .. Enumerable.Range(0, equippedCount)
                    .Select(_ => new EquippedRune())
            ]
        };
    }

    private static Rune CreateRuneDefinition(int runeId, RarityLevel rarity, bool isArmor = false)
    {
        Rune rune = isArmor
            ? new ArmorRune()
            : new WeaponRune();

        rune.Id = runeId;
        rune.Name = "Fused Test Rune";
        rune.RuneType = isArmor ? "ArmorRune" : "WeaponRune";
        rune.Element = ElementType.Fire;
        rune.Rarity = rarity;
        rune.Power = 20;

        return rune;
    }

    private Rune SetupSuccessfulFusion(int tier = 1, RarityLevel rarity = RarityLevel.Common, int quantity = 3,
        int equippedCount = 0, bool isArmor = false)
    {
        var playerRune = CreatePlayerRune(
            quantity, equippedCount, tier, rarity, isArmor);

        var expectedRarity = tier == 3 ? rarity + 1 : rarity;
        var runeType = isArmor ? "ArmorRune" : "WeaponRune";

        var expectedRune = CreateRuneDefinition(ExpectedRuneId, expectedRarity, isArmor);

        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([playerRune]);

        _runeDaoMock
            .Setup(r => r.FindRune(ElementType.Fire, expectedRarity, runeType))
            .Returns(expectedRune);

        return expectedRune;
    }

    private void SetupOwnedRuneForDestruction(int quantity = 1, int equippedCount = 0, int tier = 1)
    {
        var playerRune = CreatePlayerRune(
            quantity: quantity,
            equippedCount: equippedCount,
            tier: tier);

        _runeDaoMock
            .Setup(r => r.GetRunesByPlayerId(PlayerId))
            .Returns([playerRune]);
    }

    private void SetupRuneAndRecipeForDestruction(int quantity = 1, int equippedCount = 0, int tier = 1)
    {
        SetupOwnedRuneForDestruction(
            quantity,
            equippedCount,
            tier);

        var recipe = new Recipe
        {
            Id = RecipeId,
            RuneId = RuneId,
            Name = "Test Rune Recipe",
            Ingredients =
            [
                new RecipeIngredient
                {
                    IngredientId = 31,
                    Quantity = 1
                },
                new RecipeIngredient
                {
                    IngredientId = 32,
                    Quantity = 3
                },
                new RecipeIngredient
                {
                    IngredientId = 33,
                    Quantity = 5
                }
            ]
        };

        _recipeDaoMock
            .Setup(r => r.GetRecipeByRuneId(RuneId))
            .Returns(recipe);
    }

    private void VerifyIngredientRefund(int ingredientId, int quantity)
    {
        _ingredientDaoMock.Verify(
            i => i.AddIngredientToPlayer(
                ingredientId,
                PlayerId,
                quantity),
            Times.Once);
    }

    private void SetupPlayerIngredientForTransmutation(int quantity = 5,
        ComponentType componentType = ComponentType.Essence, RarityLevel? essenceRarity = RarityLevel.Common)
    {
        var playerIngredient = new InventoryIngredient
        {
            IngredientId = IngredientId,
            Quantity = quantity,
            Ingredient = new Ingredient
            {
                Id = IngredientId,
                Name = "Test Essence",
                ComponentType = componentType,
                EssenceRarity = essenceRarity
            }
        };

        _ingredientDaoMock
            .Setup(i => i.GetIngredientsByPlayerId(PlayerId))
            .Returns([playerIngredient]);
    }

    private static Ingredient CreateEssence(int ingredientId, RarityLevel rarity)
    {
        return new Ingredient
        {
            Id = ingredientId,
            Name = $"{rarity} Essence",
            ComponentType = ComponentType.Essence,
            EssenceRarity = rarity
        };
    }
}