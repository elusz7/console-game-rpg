using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Services.Interfaces;
using ConsoleGameEntities.Exceptions.Recipes;
using ConsoleGameEntities.Exceptions.Runes;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;


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
    
    public void CraftRune(int playerId, int recipeId)
    {
        _transactionManager.ExecuteInTransaction(() =>
        {
            var recipe = _recipeDao.GetRecipeById(recipeId);

            if (recipe == null)
                throw new RecipeNotFoundException();
            
            ValidateRecipe(recipe);

            var ingredients = _ingredientDao.GetIngredientsByPlayerId(playerId);
            
            foreach (var neededIngredient in recipe.Ingredients)
            {
                var playerIngredient = GetPlayerIngredient(
                    ingredients, neededIngredient.IngredientId, neededIngredient.Quantity);

                ConsumeIngredient(playerId, playerIngredient.IngredientId, playerIngredient.Quantity, neededIngredient.Quantity);
            }

            _runeDao.AddRuneToPlayer(recipe.RuneId, playerId, 1);
        });
    }

    public void FuseRune(int playerId, int runeId, int tier)
    {
        _transactionManager.ExecuteInTransaction(() =>
        {
            var playerRune = GetPlayerRune(playerId, runeId, tier, 3);

            if (playerRune.Rune.Rarity == RarityLevel.Mythic && tier == 3)
                throw new RuneFusionException();

            var (expectedRune, expectedTier) = GetFusionResult(playerRune, tier);

            ConsumeRunes(playerId, runeId, tier, playerRune.Quantity);

            _runeDao.AddRuneToPlayer(expectedRune.Id, playerId, expectedTier);
        });
    }

    public void DestroyRune(int playerId, int runeId, int tier)
    {
        _transactionManager.ExecuteInTransaction(() =>
        {
            var playerRune = GetPlayerRune(playerId, runeId, tier, 1);

            var recipe = _recipeDao.GetRecipeByRuneId(runeId);
            if (recipe == null)
                throw new RecipeNotFoundException();

            var ingredients = recipe.Ingredients;
            if (ingredients.Count == 0)
                throw new InvalidRecipeException();

            ReturnRecipeIngredients(playerId, tier, ingredients);

            var remainder = playerRune.Quantity - 1;

            if (remainder > 0)
                _runeDao.UpdatePlayerRuneQuantity(runeId, playerId, tier, remainder);
            else
                _runeDao.RemoveRuneFromPlayer(runeId, playerId, tier);
        });
    }

    public void TransmuteEssence(int playerId, int essenceId)
    {
        _transactionManager.ExecuteInTransaction(() =>
        {
            var playerEssence = GetEssenceForTransmutation(playerId, essenceId);

            var nextEssence = GetNextEssence(
                playerEssence.Ingredient.EssenceRarity!.Value);

            ConsumeIngredient(playerId, essenceId, playerEssence.Quantity, 5);

            _ingredientDao.AddIngredientToPlayer(nextEssence.Id, playerId);
        });
    }
    
    private static InventoryIngredient GetPlayerIngredient(List<InventoryIngredient> ingredients, int ingredientId, int requiredQuantity)
    {
        var playerIngredient = ingredients.SingleOrDefault(
            i => i.IngredientId == ingredientId);

        if (playerIngredient == null)
            throw new IngredientNotFoundException();

        return playerIngredient.Quantity < requiredQuantity ? 
            throw new IngredientQuantityException() : playerIngredient;
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
            throw new RuneNotFoundException("Expected rune not in database") : 
            (expectedRune, expectedTier);
    }
    
    private void ConsumeRunes(int playerId, int runeId, int tier, int quantity)
    {
        var remainder = quantity - 3;

        if (remainder > 0)
            _runeDao.UpdatePlayerRuneQuantity(runeId, playerId, tier, remainder);
        else
            _runeDao.RemoveRuneFromPlayer(runeId, playerId, tier);
    }
    
    private void ReturnRecipeIngredients(int playerId, int tier, List<RecipeIngredient> ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            var returnedQuantity = Math.Max(1, ingredient.Quantity / 2) + (tier - 1);

            _ingredientDao.AddIngredientToPlayer(
                ingredient.IngredientId,
                playerId,
                returnedQuantity);
        }
    }
    
    private InventoryIngredient GetEssenceForTransmutation(int playerId, int essenceId)
    {
        var playerEssence = _ingredientDao.GetIngredientsByPlayerId(playerId)
            .SingleOrDefault(ii => ii.IngredientId == essenceId);

        if (playerEssence == null)
            throw new IngredientNotFoundException("Not in player inventory");

        if (playerEssence.Quantity < 5)
            throw new IngredientQuantityException();

        if (playerEssence.Ingredient.ComponentType != ComponentType.Essence)
            throw new InvalidIngredientException();

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
                break;

            case null:
            case RarityLevel.Mythic:
                throw new InvalidIngredientException();

            default:
                throw new ArgumentOutOfRangeException(nameof(rarity));
        }
    }
    
    private Ingredient GetNextEssence(RarityLevel currentRarity)
    {
        var nextRarity = currentRarity + 1;

        var nextEssence = _ingredientDao.GetEssenceByRarity(nextRarity);

        if (nextEssence == null)
            throw new IngredientNotFoundException();

        return nextEssence;
    }
    
    private InventoryRune GetPlayerRune(int playerId, int runeId, int tier, int requiredQuantity)
    {
        if (tier is < 1 or > 3)
            throw new RuneFusionException();

        var playerRune = _runeDao.GetRunesByPlayerId(playerId)
            .SingleOrDefault(r => r.RuneId == runeId && r.Tier == tier);

        if (playerRune == null)
            throw new RuneNotFoundException("Not in player inventory");

        var availableQuantity =
            playerRune.Quantity - playerRune.EquippedRunes.Count;

        return availableQuantity < requiredQuantity ? throw new RuneQuantityException() : playerRune;
    }
    
    private static void ValidateRecipe(Recipe recipe)
    {
        if (recipe.Rune == null)
            throw new InvalidRecipeException();

        if (recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            throw new InvalidRecipeException();

        var ingredientIds = new HashSet<int>();

        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient == null)
                throw new InvalidRecipeException();

            if (ingredient.Quantity <= 0)
                throw new InvalidRecipeException();

            if (ingredient.Ingredient == null)
                throw new InvalidRecipeException();

            if (ingredient.Ingredient.Id != ingredient.IngredientId)
                throw new InvalidRecipeException();

            if (!ingredientIds.Add(ingredient.IngredientId))
                throw new InvalidRecipeException();
        }
    }
}