namespace ConsoleGameEntities.Exceptions;

public class OverweightException : Exception
{
    public OverweightException() : base() { }
    public OverweightException(string message) : base(message) { }
}
