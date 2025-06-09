using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces;

public interface IArchetype
{
    int Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }

    int HealthBase { get; set; }
    int AttackBonus { get; set; }
    decimal AttackMultiplier { get; set; }
    int MagicBonus { get; set; }
    decimal MagicMultiplier { get; set; }
    int DefenseBonus { get; set; }
    decimal DefenseMultiplier { get; set; }
    int ResistanceBonus { get; set; }
    decimal ResistanceMultiplier { get; set; }
    int Speed { get; set; }
    decimal SpeedMultiplier { get; set; }

    ArchetypeType ArchetypeType { get; set; }

    string ResourceName { get; set; }
    int CurrentResource { get; set; }
    int MaxResource { get; set; }
    decimal ResourceMultiplier { get; set; }
    int RecoveryRate { get; set; }
    int RecoveryGrowth { get; set; }

    ICollection<Skill> Skills { get; set; }

    void UseResource(int cost);
    void RecoverResource();
    int LevelUp(int newLevel);
}

