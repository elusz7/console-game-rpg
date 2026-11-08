namespace ConsoleGameEntities.Exceptions.Recipes;

public class IngredientQuantityException : RecipeException
{
    public IngredientQuantityException() : base() { }
    public IngredientQuantityException(string message) : base(message) { }
}