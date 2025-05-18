namespace ConsoleGameEntities.Main.Exceptions;

public class ItemPurchaseException : Exception
{
    public ItemPurchaseException() :base() { }
    public ItemPurchaseException(string message) : base(message) { }
}
