namespace ConsoleGameEntities.Exceptions.Recipes;

public class IngredientQuantityException : IngredientException
{
    public IngredientQuantityException() : base() { }
    public IngredientQuantityException(string message) : base(message) { }
}