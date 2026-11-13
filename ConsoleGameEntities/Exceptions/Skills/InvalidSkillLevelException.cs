namespace ConsoleGameEntities.Exceptions.Skills;

public class InvalidSkillLevelException : Exception
{
    public InvalidSkillLevelException() : base()
    {
    }

    public InvalidSkillLevelException(string message) : base(message)
    {
    }
}