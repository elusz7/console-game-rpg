namespace ConsoleGameEntities.Exceptions;

public class InvalidTargetException : Exception
{
    public InvalidTargetException() : base() { }
    public InvalidTargetException(string message) : base(message) { }
}
