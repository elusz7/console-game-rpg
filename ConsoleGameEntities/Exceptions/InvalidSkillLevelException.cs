namespace ConsoleGameEntities.Exceptions;

public class InvalidSkillLevelException : Exception
{
    public InvalidSkillLevelException() : base() { }
    public InvalidSkillLevelException(string message) : base(message) { }
}
