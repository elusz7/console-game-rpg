namespace ConsoleGameEntities.Exceptions;

public class HealthReductionException : Exception
{
    public HealthReductionException() : base() { }
    public HealthReductionException(string message) : base(message) { }
}
