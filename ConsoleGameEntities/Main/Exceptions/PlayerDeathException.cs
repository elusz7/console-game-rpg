namespace ConsoleGameEntities.Main.Exceptions;

public class PlayerDeathException : Exception
{
    public PlayerDeathException() :base() { }
    public PlayerDeathException(string message) : base(message) { }
}
