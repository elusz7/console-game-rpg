namespace ConsoleGameEntities.Exceptions;

public class InventoryException : Exception
{
    public InventoryException() : base() { }
    public InventoryException(string message) : base(message) { }
}
