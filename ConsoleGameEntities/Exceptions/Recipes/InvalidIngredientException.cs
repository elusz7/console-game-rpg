namespace ConsoleGameEntities.Exceptions.Recipes;

public class InvalidIngredientException : RecipeException
{
    public InvalidIngredientException() : base() { }
    public InvalidIngredientException(string message) : base(message) { }
}