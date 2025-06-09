namespace ConsoleGameEntities.Exceptions;

public class RecipeException : InventoryException
{
    public RecipeException() : base() { }
    public RecipeException(string message) : base(message) { }
}
