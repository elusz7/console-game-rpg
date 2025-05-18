namespace ConsoleGameEntities.Main.Exceptions;

public class ItemNotFoundException : InventoryException
{
    public ItemNotFoundException() : base() { }
    public ItemNotFoundException(string message) : base(message) { }
}
