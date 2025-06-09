using ConsoleGameEntities.Exceptions;
namespace ConsoleGameEntities.Models.Runes.Recipes;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int RuneId { get; set; }
    public virtual Rune Rune { get; set; }

    public virtual List<RecipeIngredient> Ingredients { get; set; }

    public Rune CraftRune(Dictionary<Ingredient, int> availableIngredients)
    {
        if (Ingredients == null || Ingredients.Count == 0)
        {
            throw new RecipeException("Recipe has no ingredients defined.");
        }
        foreach (var recipeIngredient in Ingredients)
        {
            if (!availableIngredients.TryGetValue(recipeIngredient.Ingredient, out var quantity) || quantity < recipeIngredient.Quantity)
            {
                throw new RecipeException($"Not enough {recipeIngredient.Ingredient.Name} to craft {Name}.");
            }
        }
        // If we reach here, remove used ingredients from availableIngredients
        foreach (var recipeIngredient in Ingredients)
        {
            availableIngredients[recipeIngredient.Ingredient] -= recipeIngredient.Quantity;
            if (availableIngredients[recipeIngredient.Ingredient] <= 0)
            {
                availableIngredients.Remove(recipeIngredient.Ingredient);
            }
        }

        return Rune; // Return the crafted rune
    }
}
