namespace ConsoleGameEntities.Exceptions.Runes;

public class RuneQuantityException : RuneException
{
    public RuneQuantityException() : base() { }
    public RuneQuantityException(string message) : base(message) { }
}