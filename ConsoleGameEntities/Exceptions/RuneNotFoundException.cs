namespace ConsoleGameEntities.Exceptions;

public class RuneNotFoundException : InventoryException
{
    public RuneNotFoundException() : base() { }
    public RuneNotFoundException(string message) : base(message) { }
}
