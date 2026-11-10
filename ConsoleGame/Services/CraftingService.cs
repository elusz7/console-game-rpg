using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Services.Interfaces;
using ConsoleGameEntities.Exceptions.Recipes;
using ConsoleGameEntities.Exceptions.Runes;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGame.Models.Results.Crafting.CraftingResults;

namespace ConsoleGame.Services;

public class CraftingService(
    ITransactionManager transactionManager,
    IRuneDao runeDao,
    IIngredientDao ingredientDao,
    IRecipeDao recipeDao) : ICraftingService
{
    private readonly ITransactionManager _transactionManager = transactionManager;
    private readonly IRuneDao _runeDao = runeDao;
    private readonly IIngredientDao _ingredientDao = ingredientDao;
    private readonly IRecipeDao _recipeDao = recipeDao;
    
    public CraftRuneResult CraftRune(int playerId, int recipeId)
    {
        CraftRuneResult? result = null;
        
        _transactionManager.ExecuteInTransaction(() =>
        {
            var recipe = _recipeDao.GetRecipeById(recipeId);

            if (recipe == null)
                throw new RecipeNotFoundException("A recipe was not found in the database.");
            
            ValidateRecipe(recipe);

            var ingredients = _ingredientDao.GetIngredientsByPlayerId(playerId);

            List<IngredientChange> ingredientsConsumed = [];
            
            foreach (var neededIngredient in recipe.Ingredients)
            {
                var playerIngredient = GetPlayerIngredient(
                    ingredients, neededIngredient.IngredientId, neededIngredient.Quantity);

                ConsumeIngredient(playerId, playerIngredient.IngredientId, playerIngredient.Quantity, neededIngredient.Quantity);
                ingredientsConsumed.Add(new IngredientChange(neededIngredient.Ingredient, neededIngredient.Quantity));
            }

            _runeDao.AddRuneToPlayer(recipe.RuneId, playerId, 1);

            result = new CraftRuneResult(recipe.Rune, ingredientsConsumed);
        });
        
        return result!;
    }

    public FuseRuneResult FuseRune(int playerId, int runeId, int tier)
    {
        FuseRuneResult? result = null;

        _transactionManager.ExecuteInTransaction(() =>
        {
            var playerRune = GetPlayerRune(playerId, runeId, tier, 3);

            if (playerRune.Rune.Rarity == RarityLevel.Mythic && tier == 3)
                throw new RuneFusionException("This rune cannot be fused at its current tier or rarity.");

            var (expectedRune, expectedTier) = GetFusionResult(playerRune, tier);

            ConsumeRunes(playerId, runeId, tier, playerRune.Quantity);

            _runeDao.AddRuneToPlayer(expectedRune.Id, playerId, expectedTier);

            result = new FuseRuneResult(
                expectedRune, expectedTier,
                playerRune.Rune, playerRune.Tier);
        });

        return result!;
    }
    
    public DestroyRuneResult DestroyRune(int playerId, int runeId, int tier)
    {
        DestroyRuneResult? result = null;
        
        _transactionManager.ExecuteInTransaction(() =>
        {
            var playerRune = GetPlayerRune(playerId, runeId, tier, 1);

            var recipe = _recipeDao.GetRecipeByRuneId(runeId);
            if (recipe == null)
                throw new RecipeNotFoundException($"A recipe was not found for {playerRune.Rune.Name}");
            
            ValidateRecipe(recipe);
            
            var returnedIngredients = ReturnRecipeIngredients(playerId, tier, recipe.Ingredients);

            var remainder = playerRune.Quantity - 1;

            if (remainder > 0)
                _runeDao.UpdatePlayerRuneQuantity(runeId, playerId, tier, remainder);
            else
                _runeDao.RemoveRuneFromPlayer(runeId, playerId, tier);

            result = new DestroyRuneResult(playerRune.Rune, playerRune.Tier, returnedIngredients);
        });

        return result!;
    }

    public TransmuteEssenceResult TransmuteEssence(int playerId, int essenceId)
    {
        TransmuteEssenceResult? result = null;
        
        _transactionManager.ExecuteInTransaction(() =>
        {
            var playerEssence = GetEssenceForTransmutation(playerId, essenceId);

            var nextEssence = GetNextEssence(
                playerEssence.Ingredient.EssenceRarity!.Value);

            ConsumeIngredient(playerId, essenceId, playerEssence.Quantity, 5);

            _ingredientDao.AddIngredientToPlayer(nextEssence.Id, playerId);

            result = new TransmuteEssenceResult(playerEssence.Ingredient, nextEssence);
        });

        return result!;
    }
    
    private static InventoryIngredient GetPlayerIngredient(List<InventoryIngredient> ingredients, int ingredientId, int requiredQuantity)
    {
        var playerIngredient = ingredients.SingleOrDefault(
            i => i.IngredientId == ingredientId);

        if (playerIngredient == null)
            throw new IngredientNotFoundException(
                "The required ingredient was not found in your inventory.");

        return playerIngredient.Quantity < requiredQuantity ? 
            throw new IngredientQuantityException(
                "You don't have enough ingredients.") : playerIngredient;
    }
    
    private void ConsumeIngredient(int playerId, int ingredientId, int currentQuantity, int quantityToConsume)
    {
        var remainder = currentQuantity - quantityToConsume;

        if (remainder > 0)
            _ingredientDao.UpdatePlayerIngredientQuantity(
                ingredientId, playerId, remainder);
        else
            _ingredientDao.RemoveIngredientFromPlayer(ingredientId, playerId);
    }
    
    private (Rune Rune, int Tier) GetFusionResult(InventoryRune playerRune, int tier)
    {
        var expectedTier = tier == 3 ? 1 : tier + 1;

        var expectedRarity = tier == 3
            ? playerRune.Rune.Rarity + 1
            : playerRune.Rune.Rarity;

        var expectedRune = _runeDao.FindRune(
            playerRune.Rune.Element,
            expectedRarity,
            playerRune.Rune.RuneType);

        return expectedRune == null ? 
            throw new RuneNotFoundException(
                "The resulting rune could not be found. Please contact support.") : 
            (expectedRune, expectedTier);
    }
    
    private void ConsumeRunes(int playerId, int runeId, int tier, int currentQuantity)
    {
        var remainder = currentQuantity - 3;

        if (remainder > 0)
            _runeDao.UpdatePlayerRuneQuantity(runeId, playerId, tier, remainder);
        else
            _runeDao.RemoveRuneFromPlayer(runeId, playerId, tier);
    }
    
    private List<IngredientChange> ReturnRecipeIngredients(int playerId, int tier, List<RecipeIngredient> ingredients)
    {
        List<IngredientChange> returnedIngredients = [];
        
        foreach (var ingredient in ingredients)
        {
            var returnedQuantity = Math.Max(1, ingredient.Quantity / 2) + (tier - 1);

            _ingredientDao.AddIngredientToPlayer(
                ingredient.IngredientId,
                playerId,
                returnedQuantity);
            returnedIngredients.Add(new IngredientChange(ingredient.Ingredient, returnedQuantity));
        }
        
        return returnedIngredients;
    }
    
    private InventoryIngredient GetEssenceForTransmutation(int playerId, int essenceId)
    {
        var playerEssence = _ingredientDao.GetIngredientsByPlayerId(playerId)
            .SingleOrDefault(ii => ii.IngredientId == essenceId);

        if (playerEssence == null)
            throw new IngredientNotFoundException(
                "The required ingredient was not found in your inventory.");

        if (playerEssence.Ingredient.ComponentType != ComponentType.Essence)
            throw new InvalidIngredientException("You cannot transmute non-essence components.");

        if (playerEssence.Quantity < 5)
            throw new IngredientQuantityException(
                "You don't have enough ingredients.");

        ValidateEssenceRarity(playerEssence.Ingredient.EssenceRarity);

        return playerEssence;
    }
    
    private static void ValidateEssenceRarity(RarityLevel? rarity)
    {
        switch (rarity)
        {
            case RarityLevel.Common:
            case RarityLevel.Uncommon:
            case RarityLevel.Rare:
            case RarityLevel.Epic:
            case RarityLevel.Legendary:
                return;

            default:
                throw new InvalidIngredientException(
                    "This essence cannot be transmuted.");
        }
    }
    
    private Ingredient GetNextEssence(RarityLevel currentRarity)
    {
        var nextRarity = currentRarity + 1;

        var nextEssence = _ingredientDao.GetEssenceByRarity(nextRarity);

        if (nextEssence == null)
            throw new IngredientNotFoundException(
                "The required ingredient was not found in the database.");

        return nextEssence;
    }
    
    private InventoryRune GetPlayerRune(int playerId, int runeId, int tier, int requiredQuantity)
    {
        if (tier is < 1 or > 3)
            throw new RuneFusionException(
                "You must select a valid rune tier.");

        var playerRune = _runeDao.GetRunesByPlayerId(playerId)
            .SingleOrDefault(r => r.RuneId == runeId && r.Tier == tier);

        if (playerRune == null)
            throw new RuneNotFoundException("You don't have this rune in your inventory.");

        var availableQuantity =
            playerRune.Quantity - playerRune.EquippedRunes.Count;

        return availableQuantity < requiredQuantity ? throw new RuneQuantityException(
            "You don't have enough unequipped copies of this rune.") : playerRune;
    }
    
    private static void ValidateRecipe(Recipe recipe)
    {
        if (recipe.Rune == null)
            throw new InvalidRecipeException("No rune attached to this recipe.");

        if (recipe.Rune.Id != recipe.RuneId)
            throw new InvalidRecipeException("The rune configuration for this recipe is invalid.");

        if (recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            throw new InvalidRecipeException("No ingredients are attached to this recipe.");

        var ingredientIds = new HashSet<int>();

        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient == null || ingredient.Ingredient == null)
                throw new InvalidRecipeException(
                    "An ingredient was not attached properly to this recipe.");

            if (ingredient.Quantity <= 0)
                throw new InvalidRecipeException(
                    $"Invalid quantity assigned to recipe for {ingredient.Ingredient.Name}.");

            if (ingredient.Ingredient.Id != ingredient.IngredientId)
                throw new InvalidRecipeException(
                    "An ingredient's configuration is invalid.");

            if (!ingredientIds.Add(ingredient.IngredientId))
                throw new InvalidRecipeException(
                    "A recipe cannot contain duplicate ingredients.");
        }
    }
}