namespace ConsoleGameEntities.Exceptions.Recipes;

public class RecipeNotFoundException : RecipeException
{
    public RecipeNotFoundException() : base() { }
    public RecipeNotFoundException(string message) : base(message) { }
}