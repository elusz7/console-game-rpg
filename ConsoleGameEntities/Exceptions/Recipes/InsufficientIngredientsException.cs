namespace ConsoleGameEntities.Exceptions.Recipes;

public class InsufficientIngredientsException : RecipeException
{
    public InsufficientIngredientsException() : base() { }
    public InsufficientIngredientsException(string message) : base(message) { }
}