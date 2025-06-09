using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Helpers.Gameplay;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;
public class Monster : IMonster
{
    private static readonly Random _rng = Random.Shared;

    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    [NotMapped]
    public int CurrentHealth { get; set; }
    [NotMapped]
    public EffectManager Effects { get; } = new();
    [NotMapped]
    public MonsterCombatBehavior Combat { get; } = new();
    [NotMapped]
    public ActionLogger Logger { get; } = new();
    [NotMapped]
    public bool Looted { get; set; } = false; //indicates if the monster has been looted
    public int MaxHealth { get; set; }
    public ThreatLevel ThreatLevel { get; set; }
    public int AggressionLevel { get; set; }
    [NotMapped]
    public virtual double DodgeChance { get; set; } = 0.01;
    public string MonsterType { get; set; }
    public virtual List<Skill> Skills { get; set; } = new List<Skill>();
    public int? RoomId { get; set; }
    public virtual Room? Room { get; set; }
    public int? ItemId { get; set; }
    public virtual Item? Treasure { get; set; }
    public int DefensePower { get; set; }
    public int AttackPower { get; set; } //base damage for all types
    public DamageType DamageType { get; set; }
    public int Resistance { get; set; }
    public ElementType? AttackElement { get; set; }
    public int? ElementalPower { get; set; }
    public ElementType? Vulnerability { get; set; }
    [NotMapped]
    public virtual IMonsterStrategy Strategy { get; set; }
    [NotMapped]
    private readonly Dictionary<DamageType, int> DamageRecord = new()
    {
        { DamageType.Martial, 0 },
        { DamageType.Magical, 0 }
    };
    public virtual bool MoreMartialDamageTaken() => DamageRecord[DamageType.Martial] > DamageRecord[DamageType.Magical];

    public virtual void Attack(IPlayer target)
    {
        Combat.Attack(this, target, Strategy);
    }
    public static IMonsterStrategy GetStrategyFor(MonsterBehaviorType behavior, IMonsterSkillSelector skillSelector)
    {
        return behavior switch
        {
            MonsterBehaviorType.Berserker => new BerserkerStrategy(skillSelector),
            MonsterBehaviorType.Cautious => new CautiousStrategy(skillSelector),
            MonsterBehaviorType.Defensive => new DefensiveStrategy(skillSelector),
            MonsterBehaviorType.Offensive => new OffensiveStrategy(skillSelector),
            MonsterBehaviorType.Cunning => new CunningStrategy(skillSelector),
            _ => new DefaultStrategy(skillSelector)
        };
    }
    public virtual void SetLevel(int newLevel)
    {
        if (Level != newLevel)
        {
            Level = newLevel;

            int attackMin, attackMax;
            int defenseMin, defenseMax;
            int resistMin, resistMax;
            int healthMin, healthMax;
            int speedMin, speedMax;

            switch (ThreatLevel)
            {
                case ThreatLevel.Medium:
                    attackMin = newLevel * 3; attackMax = newLevel * 6;
                    defenseMin = newLevel * 2; defenseMax = newLevel * 4;
                    resistMin = newLevel * 2; resistMax = newLevel * 4;
                    healthMin = newLevel * 8; healthMax = newLevel * 12;
                    speedMin = 2; speedMax = 4;
                    break;
                case ThreatLevel.High:
                    attackMin = newLevel * 5; attackMax = newLevel * 8;
                    defenseMin = newLevel * 3; defenseMax = newLevel * 6;
                    resistMin = newLevel * 3; resistMax = newLevel * 6;
                    healthMin = newLevel * 12; healthMax = newLevel * 18;
                    speedMin = 3; speedMax = 6;
                    break;
                case ThreatLevel.Elite:
                    attackMin = newLevel * 6; attackMax = newLevel * 10;
                    defenseMin = newLevel * 4; defenseMax = newLevel * 7;
                    resistMin = newLevel * 4; resistMax = newLevel * 7;
                    healthMin = newLevel * 18; healthMax = newLevel * 25;
                    speedMin = 4; speedMax = 7;
                    break;
                case ThreatLevel.Boss:
                    attackMin = newLevel * 8; attackMax = newLevel * 12;
                    defenseMin = newLevel * 5; defenseMax = newLevel * 9;
                    resistMin = newLevel * 5; resistMax = newLevel * 9;
                    healthMin = newLevel * 25; healthMax = newLevel * 35;
                    speedMin = 5; speedMax = 9;
                    break;
                default:
                    attackMin = newLevel * 2; attackMax = newLevel * 4;
                    defenseMin = newLevel * 1; defenseMax = newLevel * 2;
                    resistMin = newLevel * 1; resistMax = newLevel * 2;
                    healthMin = newLevel * 5; healthMax = newLevel * 8;
                    speedMin = 1; speedMax = 3;
                    break;
            }

            AttackPower = _rng.Next(attackMin, attackMax + 1);
            DefensePower = _rng.Next(defenseMin, defenseMax + 1);
            Resistance = _rng.Next(resistMin, resistMax + 1);
            AggressionLevel = _rng.Next(speedMin, speedMax + 1);
            MaxHealth = _rng.Next(healthMin, healthMax + 1);
        }

        //set these regardless of level being different
        CurrentHealth = MaxHealth;
        DodgeChance = 0.002 * newLevel;
    }

    public virtual void TakeDamage(int damage, DamageType? damageType)
    {
        Combat.TakeDamage(this, damage, damageType);
    }
    public virtual void TakeDamage(int damage, ElementType element)
    {
        Combat.TakeElementalDamage(this, damage, element);
    }
    public virtual void Heal(int regainedHealth)
    {
        Combat.Heal(this, regainedHealth);
    }

    public virtual void ModifyStat(StatType stat, int duration, int amount)
    {
        if (stat == StatType.Health)
            Heal(amount);
        else
        {
            Effects.AddEffect(StatusRecordType.Skill, (int)stat, duration, amount);

            Logger.Log($"{Name}'s {stat} is changed by {amount}!");
        }
    }
    public Item? Loot()
    {
        if (Treasure == null)
            return null;

        Item loot = Treasure;

        loot.MonsterId = null; // Remove the item from the monster
        loot.Monster = null;
        ItemId = null;
        Treasure = null;

        return loot;
    }

    public void SetLoot(Item item)
    {
        item.MonsterId = Id;
        item.Monster = this;
        ItemId = item.Id;
        Treasure = item;
    }

    public void RuneAttack(WeaponRune rune)
    {
        Combat.ApplyRuneEffect(this, rune);
    }
    public virtual void ElapseTime()
    {
        Effects.ElapseTime(
            onExpire: e =>
            {
                switch (e.Source)
                {
                    case StatusRecordType.Skill:
                        Logger.Log($"An effect affecting {(StatType)e.Type} has ended!");
                        break;
                    case StatusRecordType.ElementalStatus:
                        Logger.Log($"An elemental status {(ElementalStatusEffectType)e.Type} has ended!");
                        break;
                    default:
                        Logger.Log($"A {(ElementDamageType)e.Type} effect has ended!");
                        break;
                }
            },
            onTick: e =>
            {
                if (e.Source == StatusRecordType.ElementalDamage)
                {
                    TakeDamage(e.Power, (ElementType)e.Type);
                }
            }
        );

        foreach (var skill in Skills)
        {
            skill.UpdateElapsedTime();
        }
    }
}
