namespace ConsoleGameEntities.Exceptions;

public class ItemPurchaseException : Exception
{
    public ItemPurchaseException() :base() { }
    public ItemPurchaseException(string message) : base(message) { }
}
