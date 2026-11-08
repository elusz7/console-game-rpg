using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Runes.Recipes;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class RecipeDao(GameContext context) : IRecipeDao
{
    private readonly GameContext _context = context;
    
    public Recipe? GetRecipeById(int recipeId)
    {
        return _context.Recipes
            .Include(r => r.Rune)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefault(r => r.Id == recipeId);
    }
    
    public List<Recipe> GetCraftableRecipesByPlayerId(int playerId)
    {
        return [
            .. _context.Recipes
                .Include(r => r.Rune)
                .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.Ingredients.Any())
                .Where(r => r.Ingredients.All(ri =>
                    _context.InventoryIngredients.Any(ii =>
                        ii.Inventory.PlayerId == playerId &&
                        ii.IngredientId == ri.IngredientId &&
                        ii.Quantity >= ri.Quantity)))
        ];
    }

    public Recipe? GetRecipeByRuneId(int runeId)
    {
        return _context.Recipes
            .Include(r => r.Rune)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefault(r => r.RuneId == runeId);
    }

    public List<Recipe> GetAllRecipes()
    {
        return [.. _context.Recipes.Include(r => r.Rune)
            .Include(r => r.Ingredients)
            .ThenInclude(ri => ri.Ingredient)];
    }

    public List<Recipe> GetRecipesByIngredientId(int ingredientId)
    {
        return [
            .._context.Recipes
                .Include(r => r.Rune)
                .Include(r => r.Ingredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.Ingredients.Any(i => i.IngredientId == ingredientId))
        ];
    }
}