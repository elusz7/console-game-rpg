using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IRecipeDao
{
    Recipe? GetRecipeById(int recipeId);
    Recipe? GetRecipeByRuneId(int runeId);
    List<Recipe> GetAllRecipes();
    List<Recipe> GetRecipesByIngredientId(int ingredientId);
}