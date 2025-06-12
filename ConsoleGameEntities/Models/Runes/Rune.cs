using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Runes;

public abstract class Rune : IRune
{
    public int Id { get; set; }
    public string RuneType { get; set; }
    public string Name { get; set; }
    public int Power { get; set; }
    public int Tier { get; set; }
    public ElementType Element { get; set; }
    public RarityLevel Rarity { get; set; }

    public int? RecipeId { get; set; }
    public virtual Recipe? Recipe { get; set; }

    public virtual Dictionary<Ingredient, int> DestroyRune()
    {
        if (Recipe == null)
        {
            return new Dictionary<Ingredient, int>(); // No recipe, no ingredients to return
        }

        var ingredients = Recipe.Ingredients.ToDictionary(ingredient => ingredient.Ingredient, ingredient => ingredient.Quantity);
        foreach (var kvp in ingredients)
        {
            var key = kvp.Key;
            ingredients[key] = ingredients[key] / 3; // Reduce quantity by 1/3
        }

        return ingredients;
    }
    public override bool Equals(object? obj)
    {
        if (obj is not Rune)
        {
            return false;
        }

        if (obj is Rune otherRune)
        {
            return RuneType == otherRune.RuneType &&
                     Tier == otherRune.Tier &&
                     Element == otherRune.Element &&
                     Rarity == otherRune.Rarity;
        }

        return false;
    }
}
