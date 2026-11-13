namespace ConsoleGameEntities.Exceptions.Skills;

public class InvalidSkillConfigurationException : Exception
{
    public InvalidSkillConfigurationException() : base() { }
    public InvalidSkillConfigurationException(string message) : base(message) { }
}