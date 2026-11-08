namespace ConsoleGameEntities.Exceptions.Recipes;

public class InvalidRecipeException : RecipeException
{
    public InvalidRecipeException() : base() { }
    public InvalidRecipeException(string message) : base(message) { }
}