using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Items;
using System.Text;

namespace ConsoleGameEntities.Models.Monsters;
public class Monster : IMonster
{    
    private static readonly Random _rng = Random.Shared;
    [NotMapped]
    public Dictionary<long, string> ActionItems { get; } = new();
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    [NotMapped]
    public int CurrentHealth { get; set; }
    [NotMapped]
    private readonly Dictionary<StatType, int> ActiveEffects = new()
    {
        { StatType.Defense, 0 },
        { StatType.Resistance, 0 },
        { StatType.Speed, 0 },
        { StatType.Attack, 0 },
        { StatType.Magic, 0 },
    };
    public int MaxHealth { get; set; }
    public ThreatLevel ThreatLevel { get; set; }
    public int AggressionLevel { get; set; }
    private double DodgeChance { get; set; } = 0.01;
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
    [NotMapped]
    public virtual IMonsterStrategy Strategy { get; set; }
    [NotMapped]
    private Dictionary<DamageType, int> DamageRecord = new()
    {
        { DamageType.Martial, 0 },
        { DamageType.Magical, 0 }
    };
    public virtual bool MoreMartialDamageTaken() => DamageRecord[DamageType.Martial] > DamageRecord[DamageType.Magical];

    public virtual void Attack(IPlayer target)
    {
        Strategy.ExecuteAttack(this, target);
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
        Level = newLevel;
        DodgeChance = 0.002 * newLevel;

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
        CurrentHealth = MaxHealth;
    }

    public virtual void TakeDamage(int damage, DamageType? damageType)
    {
        var chance = Math.Min(33, DodgeChance + (GetStat(StatType.Speed) * 0.01)); //cap of 33% dodge chance
        bool dodged = _rng.Next(0, 100) < chance;

        if (dodged)
        {
            AddActionItem($"{Name} dodged the attack and took no damage!");
            return;
        }

        int damageTaken = 0;
        switch (damageType)
        {
            case DamageType.Martial:
                damageTaken = Math.Max(1, damage - GetStat(StatType.Defense));
                DamageRecord[DamageType.Martial] += 1;
                break;
            case DamageType.Magical:
                damageTaken = Math.Max(1, damage - GetStat(StatType.Resistance));
                DamageRecord[DamageType.Magical] += 1;
                break;
            case DamageType.Hybrid:
                damageTaken = Math.Max(1, damage - (Math.Max(GetStat(StatType.Defense), GetStat(StatType.Resistance)) / 2));
                break;
        }

        AddActionItem($"{Name} takes {damageTaken} damage!");
        CurrentHealth -= damageTaken;

        if (CurrentHealth <= 0)
        {
            AddActionItem($"{Name} is now dead.");
            throw new MonsterDeathException();
        }
    }
    public virtual void Heal(int regainedHealth)
    {
        if (CurrentHealth + regainedHealth > MaxHealth)
        {
            AddActionItem($"{Name} heals for {MaxHealth - CurrentHealth} health!");
            CurrentHealth = MaxHealth;
        }
        else
        {
            AddActionItem($"{Name} heals for {regainedHealth} health!");
            CurrentHealth += regainedHealth;
        }
    }
    public virtual int GetStat(StatType stat)
    {
        return stat switch
        {
            StatType.Defense => Math.Max(0, DefensePower + ActiveEffects[StatType.Defense]),
            StatType.Speed => Math.Max(0, AggressionLevel + ActiveEffects[StatType.Speed]),
            StatType.Attack => Math.Max(0, AttackPower + ActiveEffects[StatType.Attack]),
            StatType.Resistance => Math.Max(0, Resistance + ActiveEffects[StatType.Resistance]),
            StatType.Health => CurrentHealth,
            StatType.Magic => Math.Max(0, AttackPower + ActiveEffects[StatType.Magic]),
            _ => throw new StatTypeException("Monster Get Stat")
        };
    }
    public virtual void ModifyStat(StatType stat, int amount)
    {
        if (stat == StatType.Health)
            Heal(amount);
        else if (ActiveEffects.ContainsKey(stat))
        {
            AddActionItem($"{Name}'s {stat} is changed by {amount}!");
            ActiveEffects[stat] += amount;
        }
        else
            throw new StatTypeException("Invalid stat to modify");
    }

    public void ClearActionItems()
    {
        ActionItems.Clear();
    }

    public void AddActionItem(string action)
    {
        long key = DateTime.Now.Ticks;
        while (ActionItems.ContainsKey(key)) key++;

        ActionItems.Add(key, action);
    }
    public void AddActionItem(Skill skill)
    {
        long key = DateTime.Now.Ticks;
        while (ActionItems.ContainsKey(key)) key++;

        ActionItems.Add(key, $"{Name} uses {skill.Name}!");
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
}
