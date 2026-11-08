namespace ConsoleGameEntities.Exceptions.Recipes;

public class RecipeException : Exception
{
    protected RecipeException() : base() { }
    protected RecipeException(string message) : base(message) { }
}