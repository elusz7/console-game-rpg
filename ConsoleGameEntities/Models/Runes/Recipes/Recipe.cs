namespace ConsoleGameEntities.Models.Runes.Recipes;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int RuneId { get; set; }
    public virtual Rune Rune { get; set; }

    public virtual List<RecipeIngredient> Ingredients { get; set; }
}
