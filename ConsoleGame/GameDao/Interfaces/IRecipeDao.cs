using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IRecipeDao
{
    Recipe? GetRecipeById(int recipeId);
    List<Recipe> GetCraftableRecipesByPlayerId(int playerId);
    Recipe? GetRecipeByRuneId(int runeId);
    List<Recipe> GetAllRecipes();
    List<Recipe> GetRecipesByIngredientId(int ingredientId);
}