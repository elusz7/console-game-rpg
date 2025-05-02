using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces;

public interface ISkill
{
    //basic info
    int Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    

    int RequiredLevel { get; set; }
    int Cost { get; set; }
    int Power { get; set; }
    int ElapsedTime { get; set; }
    int Cooldown { get; set; }
    
    TargetType TargetType { get; set; }
    SkillCategory SkillCategory { get; set; }
    DamageType DamageType { get; set; }

    //relationship info
    Archetype? Archetype { get; set; }
    Monster? Monster { get; set; }

    void Activate(ITargetable self, ITargetable? target = null, List<ITargetable>? targets = null);
    void UpdateElapsedTime();
    void Reset();
}