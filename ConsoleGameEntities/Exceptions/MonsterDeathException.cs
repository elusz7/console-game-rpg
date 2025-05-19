namespace ConsoleGameEntities.Exceptions;

public class MonsterDeathException : Exception
{
    public MonsterDeathException() :base() { }
    public MonsterDeathException(string message) : base(message) { }
}
