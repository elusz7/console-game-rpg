namespace ConsoleGameEntities.Exceptions;

public class DuplicateItemException : InventoryException
{
    public DuplicateItemException() :base() { }
    public DuplicateItemException(string message) : base(message) { }
}
