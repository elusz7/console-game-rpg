namespace ConsoleGameEntities.Exceptions.Recipes;

public class IngredientNotFoundException : RecipeException
{
    public IngredientNotFoundException() : base() { }
    public IngredientNotFoundException(string message) : base(message) { }
}