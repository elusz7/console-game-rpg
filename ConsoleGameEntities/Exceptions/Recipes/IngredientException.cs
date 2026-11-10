namespace ConsoleGameEntities.Exceptions.Recipes;

public class IngredientException : Exception
{
    protected IngredientException() : base() { }
    protected IngredientException(string message) : base(message) { }
}