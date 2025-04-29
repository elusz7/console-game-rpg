using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Abilities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Entities;

public class Archetype : IArchetype
{
    private static readonly Random _rng = new();

    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int HealthBase { get; set; } // health gained on level up
    public int AttackBonus { get; set; } //bonus damage to weapons
    public int MagicBonus { get; set; } //bonus damage to skills (spells)
    public int DefenseBonus { get; set; } //a bonus to reducing incoming damage
    public int Speed { get; set; } //used for establishing turn order

    public ArchetypeType ArchetypeType { get; set; } //martial or magic

    public virtual ICollection<Skill>? Skills { get; set; } = new List<Skill>();

    public string ResourceName { get; set; }
    public int CurrentResource { get; set; }
    public int MaxResource { get; set; }
    public int RecoveryRate { get; set; }
    public decimal AttackMultiplier { get; set; }
    public decimal MagicMultiplier { get; set; }
    public decimal DefenseMultiplier { get; set; }
    public decimal SpeedMultiplier { get; set; }
    public decimal ResourceMultiplier { get; set; }
    public int RecoveryGrowth { get; set; }

    public void UseResource(int cost)
    {
        if (CurrentResource >= cost)
        {
            CurrentResource -= cost;
        }
        else
        {
            throw new InvalidOperationException("Not enough resource.");
        }
    }
    public void RecoverResource()
    {
        CurrentResource = Math.Min(CurrentResource + RecoveryRate, MaxResource);
    }
    public virtual int LevelUp(int newLevel)
    {
        if (ArchetypeType == ArchetypeType.Martial)
            AttackBonus += (int)Math.Floor(newLevel * AttackMultiplier);
        if (ArchetypeType == ArchetypeType.Magical)
            MagicBonus += (int)Math.Floor(newLevel * MagicMultiplier);
        DefenseBonus += (int)Math.Floor(newLevel * DefenseMultiplier);
        Speed += (int)Math.Floor(newLevel * SpeedMultiplier);

        MaxResource += (int)Math.Floor(newLevel * ResourceMultiplier);
        RecoveryRate += RecoveryGrowth;

        return _rng.Next(1, HealthBase + 1);
    }
}
