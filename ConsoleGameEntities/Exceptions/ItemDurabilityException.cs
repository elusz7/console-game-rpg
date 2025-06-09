namespace ConsoleGameEntities.Exceptions;

public class ItemDurabilityException : InventoryException
{
    public ItemDurabilityException() : base() { }
    public ItemDurabilityException(string message) : base(message) { }
}
