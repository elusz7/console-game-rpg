using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.GameDao;

public class RecipeDao(GameContext context) : IRecipeDao
{
    private readonly GameContext _context = context;

    public List<Recipe> GetCraftableRecipes(Dictionary<Ingredient, int> availableIngredients)
    {
        var allRecipes = _context.Recipes.ToList();

        var craftableRecipes = allRecipes
            .Where(recipe => recipe.Ingredients.All(recipeIngredient => HasSufficientIngredient(recipeIngredient, availableIngredients)))
            .ToList();

        return craftableRecipes;
    }
    private static bool HasSufficientIngredient(RecipeIngredient recipeIngredient, Dictionary<Ingredient, int> availableIngredients)
    {
        return availableIngredients.TryGetValue(recipeIngredient.Ingredient, out var availableQty) &&
               availableQty >= recipeIngredient.Quantity;
    }
    public List<Recipe> GetAllRecipes()
    {
        return [.. _context.Recipes];
    }
    public List<Recipe> FindRecipesByName(string name)
    {
        return [.. _context.Recipes.ToList().Where(recipe => recipe.Name.Contains(name, StringComparison.OrdinalIgnoreCase))];
    }
    public List<Recipe> FindRecipesByElement(string element)
    {
        return [.. _context.Recipes.ToList().Where(recipe => recipe.Rune.Element.ToString().Equals(element, StringComparison.OrdinalIgnoreCase))];
    }
    public List<Recipe> FindRecipesByRarity(string rarity)
    {
        return [.. _context.Recipes.ToList().Where(recipe => recipe.Rune.Rarity.ToString().Equals(rarity, StringComparison.OrdinalIgnoreCase))];
    }
}
