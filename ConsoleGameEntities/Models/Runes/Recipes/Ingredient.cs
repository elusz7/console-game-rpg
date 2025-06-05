using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Runes.Recipes;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IngredientType IngredientType { get; set; }
}
