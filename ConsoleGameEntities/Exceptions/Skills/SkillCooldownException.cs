namespace ConsoleGameEntities.Exceptions.Skills;

public class SkillCooldownException : Exception
{
    public SkillCooldownException() : base()
    {
    }

    public SkillCooldownException(string message) : base(message)
    {
    }
}