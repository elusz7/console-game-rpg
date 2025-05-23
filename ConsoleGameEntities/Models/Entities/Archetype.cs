using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Entities;

public class Archetype : IArchetype
{
    private static readonly Random _rng = Random.Shared;

    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int HealthBase { get; set; } // health gained on level up
    public int AttackBonus { get; set; } //bonus damage to weapons
    public int MagicBonus { get; set; } //bonus damage to skills (spells)
    public int DefenseBonus { get; set; } //a bonus to reducing incoming melee damage
    public int ResistanceBonus { get; set; } //a bonus to reducing incoming magic damage
    public int Speed { get; set; } //used for establishing turn order

    public ArchetypeType ArchetypeType { get; set; } //martial or magic

    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();

    public string ResourceName { get; set; }
    [NotMapped]
    public int CurrentResource { get; set; }
    public int MaxResource { get; set; }
    public int RecoveryRate { get; set; }
    public decimal AttackMultiplier { get; set; }
    public decimal MagicMultiplier { get; set; }
    public decimal DefenseMultiplier { get; set; }
    public decimal ResistanceMultiplier { get; set; }
    public decimal SpeedMultiplier { get; set; }
    public decimal ResourceMultiplier { get; set; }
    public int RecoveryGrowth { get; set; }

    public virtual void UseResource(int cost)
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
    public virtual void RecoverResource()
    {
        CurrentResource = Math.Min(CurrentResource + RecoveryRate, MaxResource);
    }
    public virtual void RecoverResource(int recoveryPower)
    {
        var recoverableAmount = MaxResource - CurrentResource;
        var recoveredResource = Math.Min(recoveryPower, recoverableAmount);

        CurrentResource += recoveredResource;
    }
    public virtual int LevelUp(int newLevel)
    {
        if (ArchetypeType == ArchetypeType.Martial)
            AttackBonus += (int)Math.Floor(newLevel * AttackMultiplier);
        else if (ArchetypeType == ArchetypeType.Magical)
            MagicBonus += (int)Math.Floor(newLevel * MagicMultiplier);

        DefenseBonus += (int)Math.Floor(newLevel * DefenseMultiplier);
        ResistanceBonus += (int)Math.Floor(newLevel * ResistanceMultiplier);
        Speed += (int)Math.Floor(newLevel * SpeedMultiplier);

        MaxResource += (int)Math.Floor(newLevel * ResourceMultiplier);
        CurrentResource = MaxResource;

        RecoveryRate += RecoveryGrowth;
        RecoveryRate = Math.Min((int)Math.Floor(MaxResource / 3.0), RecoveryRate); // cap to 1/3 of MaxResource

        return _rng.Next(2, HealthBase + 1); // random health increase
    }
}
