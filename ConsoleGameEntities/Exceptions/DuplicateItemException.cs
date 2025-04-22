namespace ConsoleGameEntities.Exceptions;

public class DuplicateItemException : Exception
{
    public DuplicateItemException() :base() { }
    public DuplicateItemException(string message) : base(message) { }
}
