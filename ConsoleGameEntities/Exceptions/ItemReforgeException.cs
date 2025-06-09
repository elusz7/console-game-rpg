namespace ConsoleGameEntities.Exceptions;

public class ItemReforgeException : InventoryException
{
    public ItemReforgeException() : base() { }
    public ItemReforgeException(string message) : base(message) { }
}
