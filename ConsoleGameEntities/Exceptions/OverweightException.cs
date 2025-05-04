namespace ConsoleGameEntities.Exceptions;

public class OverweightException : InventoryException
{
    public OverweightException() : base() { }
    public OverweightException(string message) : base(message) { }
}
