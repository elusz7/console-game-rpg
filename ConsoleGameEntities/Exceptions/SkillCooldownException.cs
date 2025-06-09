namespace ConsoleGameEntities.Exceptions;

public class SkillCooldownException : Exception
{
    public SkillCooldownException() : base() { }
    public SkillCooldownException(string message) : base(message) { }
}
