using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces;

public interface ISkill
{
    int Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    int Cost { get; set; }
    int Power { get; set; }
    int ElapsedTime { get; set; }
    int Cooldown { get; set; }
    TargetType TargetType { get; set; }
    SkillType SkillType { get; set; }
    void Activate();
    void UpdateElapsedTime();
}