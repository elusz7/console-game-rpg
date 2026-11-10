namespace ConsoleGameEntities.Exceptions.Recipes;

public class IngredientNotFoundException : IngredientException
{
    public IngredientNotFoundException() : base() { }
    public IngredientNotFoundException(string message) : base(message) { }
}