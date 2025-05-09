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
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
    [NotMapped]
    public Dictionary<int, string> ActionItems { get; } = new();
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
    [NotMapped]
    public int DesiredHitsToKillPlayer { get; set; }
    [NotMapped]
    public int DesiredHitsToBeKilledByPlayer { get; set; }
    private double DodgeChance { get; set; } = 0.01;
    public string MonsterType { get; set; }
    public virtual ICollection<Skill>? Skills { get; set; } = new List<Skill>();
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
    private Dictionary<DamageType, int> DamageRecord = new Dictionary<DamageType, int>
    {
        { DamageType.Martial, 0 },
        { DamageType.Magical, 0 }
    };
    [NotMapped]
    public bool MoreMartialDamageTaken => DamageRecord[DamageType.Martial] > DamageRecord[DamageType.Magical];

    public virtual void Attack(IPlayer target)
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
        if (newLevel <= Level) return;

        ClearActionItems();

        double baseAttack = AttackPower;
        double baseDefense = DefensePower;
        double baseResistance = Resistance;
        double baseAggression = AggressionLevel;
        double baseHealth = MaxHealth;

        for (int i = Level; i < newLevel; i++)
        {
            Level++;
            List<double> variableIncrease = new List<double>() { 0.3, 0.4, 0.5, 0.6, 0.7 };

            int index = _rng.Next(variableIncrease.Count);
            AttackPower += (int)(baseAttack * variableIncrease[index]);
            variableIncrease.RemoveAt(index);

            index = _rng.Next(variableIncrease.Count);
            DefensePower += (int)(baseDefense * variableIncrease[index]);
            variableIncrease.RemoveAt(index);

            index = _rng.Next(variableIncrease.Count);
            Resistance += (int)(baseResistance * variableIncrease[index]);
            variableIncrease.RemoveAt(index);

            index = _rng.Next(variableIncrease.Count);
            AggressionLevel += (int)(baseAggression * variableIncrease[index]);
            variableIncrease.RemoveAt(index);

            MaxHealth += (int)(baseHealth * variableIncrease[0]);
            DodgeChance += 0.002;
        }
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

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"({MonsterType}) {Name} : {Description}");
        sb.Append($"\n\tLevel: {Level}, Health: {MaxHealth}, DamageType: {DamageType}, ThreatLevel: {ThreatLevel}");
        sb.Append($"\n\tAttack: {AttackPower}, Defense: {DefensePower}, Resistance: {Resistance}, Speed: {AggressionLevel}");

        return sb.ToString();
    }

    public void ClearActionItems()
    {
        ActionItems.Clear();
    }

    public void AddActionItem(string action)
    {
        ActionItems.Add(DateTime.Now.Millisecond, action);
    }
    public void AddActionItem(ISkill skill)
    {
        ActionItems.Add(DateTime.Now.Millisecond, $"{Name} uses {skill.Name}!");
    }

    public Item Loot()
    {
        if (Treasure == null)
            throw new InvalidOperationException($"{Name} has no treasure to loot.");
        Item loot = Treasure;

        loot.MonsterId = null; // Remove the item from the monster
        loot.Monster = null; 
        ItemId = null;
        Treasure = null; 

        return loot;
    }
}
