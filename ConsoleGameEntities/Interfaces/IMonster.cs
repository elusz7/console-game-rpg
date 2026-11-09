using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces;

public interface IMonster : ITargetable
{
    int Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    int Level { get; set; }
    int CurrentHealth { get; set; }
    int MaxHealth { get; set; }
    string MonsterType { get; set; }
    bool IsCore {get; set;}

    int AggressionLevel { get; set; } //same as speed for player
    int DefensePower { get; set; } //defense power of monster
    int AttackPower { get; set; } //attack power of monster
    int Resistance { get; set; } //resistance to damage
    
    ElementType? AttackElement { get; set; } //what element they attack with/ingredient drop
    int? ElementalPower { get; set; } //how much elemental damage they deal
    ElementType? Vulnerability { get; set; } //what element they take more damage from

    DamageType DamageType { get; set; }
    ThreatLevel ThreatLevel { get; set; }

    List<Skill> Skills { get; set; }
    Item? Treasure { get; set; }
    Room? Room { get; set; }

    void Attack(IPlayer target);
    Item? Loot();
    void SetLoot(Item item);

    bool MoreMartialDamageTaken();
}