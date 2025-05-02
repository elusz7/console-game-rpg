using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;
public class Monster : IMonster
{
    private static Random _rng = new Random(DateTime.Now.Millisecond);
    public int Id { get; set; }
    public string Name { get; set; }
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
    public int AggressionLevel { get; set; }
    public int DesiredHitsToKillPlayer { get; set; } = 3;
    public int DesiredHitsToBeKilledByPlayer { get; set; } = 3;
    public string MonsterType { get; set; }
    public ICollection<Skill>? Skills { get; set; } = new List<Skill>();
    public int? RoomId { get; set; }
    public Room? Room { get; set; }
    public int DefensePower { get; set; }
    public int AttackPower { get; set; } //base damage for all types
    public DamageType DamageType { get; set; }
    public int Resistance { get; set; }
    [NotMapped]
    public IMonsterStrategy Strategy { get; set; }
    [NotMapped]
    private Dictionary<DamageType, int> DamageRecord = new Dictionary<DamageType, int>
    {
        { DamageType.Martial, 0 },
        { DamageType.Magical, 0 }
    };
    [NotMapped]
    public bool MoreMartialDamageTaken => DamageRecord[DamageType.Martial] > DamageRecord[DamageType.Magical];

    public virtual void Attack(ITargetable target)
    {
        Strategy.ExecuteAttack(this, target);
    }
    public static IMonsterStrategy GetStrategyFor(MonsterBehaviorType behavior)
    {
        return behavior switch
        {
            MonsterBehaviorType.Berserker => new BerserkerStrategy(),
            MonsterBehaviorType.Cautious => new CautiousStrategy(),
            MonsterBehaviorType.Defensive => new DefensiveStrategy(),
            MonsterBehaviorType.Offensive => new OffensiveStrategy(),
            MonsterBehaviorType.Cunning => new CunningStrategy(),
            _ => new DefaultStrategy()
        };
    }
    public virtual void LevelUp(int newLevel) 
    {
        List<double> variableIncrease = new List<double>() { 0.3, 0.4, 0.5, 0.6, 0.7};

        //attackpower
        int index = _rng.Next(variableIncrease.Count);
        AttackPower += (int)Math.Floor(variableIncrease[index] * AttackPower);
        variableIncrease.Remove(index);
        //defensepower
        index = _rng.Next(variableIncrease.Count);
        DefensePower += (int)Math.Floor(variableIncrease[index] * DefensePower);
        variableIncrease.Remove(index);
        //resistance
        index = _rng.Next(variableIncrease.Count);
        Resistance += (int)Math.Floor(variableIncrease[index] * Resistance);
        variableIncrease.Remove(index);
        //aggression level (speed)
        index = _rng.Next(variableIncrease.Count);
        AggressionLevel += (int)Math.Floor(variableIncrease[index] * AggressionLevel);
        variableIncrease.Remove(index);
        //health
        MaxHealth += (int)Math.Floor(variableIncrease[0] * MaxHealth);
    }
    public virtual void TakeDamage(int damage, DamageType damageType)
    {
        int damageTaken = 0;
        switch (damageType)
        {
            case DamageType.Generic:
                damageTaken = damage;
                break;
            case DamageType.Martial:
                damageTaken = damage - GetStat(StatType.Defense);
                DamageRecord[DamageType.Martial] += 1;
                break;
            case DamageType.Magical:
                damageTaken = damage - GetStat(StatType.Resistance);
                DamageRecord[DamageType.Magical] += 1;
                break;
            case DamageType.Hybrid:
                damageTaken = damage - (Math.Max(GetStat(StatType.Defense), GetStat(StatType.Resistance)) / 2);
                break;
        }

        if (damageTaken < 1)
            throw new HealthReductionException("Monster");

        CurrentHealth -= damageTaken;

        if (CurrentHealth <= 0)
            throw new MonsterDeathException();
    }
    public void AdjustStartingStat(IArchetype archetype)
    {
        // Defensive power doubles here to represent armor mitigation
        int effectivePlayerDefense = archetype.DefenseBonus * 2;

        // Desired damage per hit = target health / desired number of hits
        int desiredDamage = (int)Math.Floor((archetype.HealthBase * 1.5) / DesiredHitsToKillPlayer);

        // Compute current monster damage
        int currentDamage = Math.Max(GetStat(StatType.Attack) - effectivePlayerDefense, 1);

        // Adjust attack stat so damage matches desired value
        int requiredAttack = desiredDamage + effectivePlayerDefense;

        int attackAdjustment = requiredAttack - currentDamage;

        // Apply the change as a temporary effect
        ActiveEffects[StatType.Attack] = attackAdjustment;

        // Assume archetype base attack and defense of monster cancel out somewhat
        int playerAverageDamage = (int)Math.Floor(((archetype.ArchetypeType == ArchetypeType.Martial ? archetype.AttackBonus : archetype.MagicBonus) * 1.5)) - this.DefensePower;
        playerAverageDamage = Math.Max(playerAverageDamage, 1); // ensure damage isn't 0 or negative

        int desiredMonsterHealth = playerAverageDamage * DesiredHitsToBeKilledByPlayer;
        MaxHealth = desiredMonsterHealth;
        CurrentHealth = MaxHealth;
    }
    public virtual void Heal(int regainedHealth)
    {
        if (CurrentHealth + regainedHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        else
            CurrentHealth += regainedHealth;
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
            ActiveEffects[stat] += amount;
        else
            throw new StatTypeException("Invalid stat to modify");
    }
}
