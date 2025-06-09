using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IRecipeDao
{
    List<Recipe> GetCraftableRecipes(Dictionary<Ingredient, int> ingredients);
    List<Recipe> GetAllRecipes();
    List<Recipe> FindRecipesByName(string name);
    List<Recipe> FindRecipesByElement(string element);
    List<Recipe> FindRecipesByRarity(string rarity);
}
