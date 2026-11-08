namespace ConsoleGameEntities.Exceptions.Runes;

public class RuneNotFoundException : RuneException
{
    public RuneNotFoundException() : base() { }
    public RuneNotFoundException(string message) : base(message) { }
}