namespace ConsoleGameEntities.Exceptions.Runes;

public class RuneException : Exception
{
    protected RuneException() : base() { }
    protected RuneException(string message) : base(message) { }
}