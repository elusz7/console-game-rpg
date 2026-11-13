namespace ConsoleGameEntities.Exceptions;

public class AttackException : Exception
{
    public AttackException() : base() { }

    public AttackException(string message) : base(message) { }
}