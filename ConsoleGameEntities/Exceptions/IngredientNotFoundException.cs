namespace ConsoleGameEntities.Exceptions;

public class IngredientNotFoundException : InventoryException
{
    public IngredientNotFoundException() : base() { }
    public IngredientNotFoundException(string message) : base(message) { }
}
