using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces;

public interface IMonster : ITargetable
{
    int Id { get; set; }
    string Name { get; set; }

    int CurrentHealth { get; set; }
    int MaxHealth { get; set; }
    int Level { get; set; }
    int AggressionLevel { get; set; } //same as speed for player
    string MonsterType { get; set; }
    int DefensePower { get; set; } //defense power of monster
    int AttackPower { get; set; } //attack power of monster
    int Resistance { get; set; } //resistance to damage
    DamageType DamageType { get; set; }

    ICollection<Skill>? Skills { get; set; }

    void Attack(ITargetable target);
}