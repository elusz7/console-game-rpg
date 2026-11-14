using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;

namespace ConsoleGameEntities.Models.Monsters;

public class Monster : IMonster
{
    [NotMapped] public ActiveEffectManager ActiveEffectManager { get; } = new();
    private static readonly Random Random = Random.Shared;
    [NotMapped] private static readonly DamageResolver DamageResolver = new();
    [NotMapped] public Dictionary<long, string> ActionItems { get; } = new();
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    [NotMapped] public int CurrentHealth { get; set; }

    public int MaxHealth { get; set; }
    public ThreatLevel ThreatLevel { get; set; }
    public int AggressionLevel { get; set; }
    [NotMapped] public virtual double DodgeChance { get; set; } = 0.01;
    public string MonsterType { get; set; }
    public bool IsCore {get; set;}
    
    public ElementType? AttackElement { get; set; }
    public int? ElementalPower { get; set; }
    public ElementType? Vulnerability { get; set; }
    public ElementType? Resilience  { get; set; }

    [NotMapped] public bool TreasureLooted { get; set; }
    [NotMapped] public bool IngredientsLooted { get; set; }
    
    public virtual List<Skill> Skills { get; set; } = [];
    public int? RoomId { get; set; }
    public virtual Room? Room { get; set; }
    public virtual Item? Treasure { get; set; }
    public int DefensePower { get; set; }
    public int AttackPower { get; set; } //base damage for all types
    public DamageType DamageType { get; set; }
    public int Resistance { get; set; }
    [NotMapped] public virtual IMonsterStrategy Strategy { get; set; }

    [NotMapped] private Dictionary<DamageType, int> DamageRecord = new()
    {
        { DamageType.Martial, 0 },
        { DamageType.Magical, 0 }
    };

    public virtual bool MoreMartialDamageTaken() => DamageRecord[DamageType.Martial] > DamageRecord[DamageType.Magical];

    public virtual AttackResolutionResult Attack(IPlayer target)
    {
        return Strategy.ExecuteAttack(this, target);
    }
    
    public AttackResolutionResult MakeBasicAttack(IPlayer target, double multiplier = 1.0)
    {
        var damage = GetEffectiveDamage(multiplier);

        if (damage.StandardDamage > 0)
        {
            AddActionItem(
                $"{Name} attacks for {damage.StandardDamage} {DamageType} damage!");
        }

        if (damage.ElementalDamage > 0)
        {
            AddActionItem(
                $"{Name} attacks for {damage.ElementalDamage} {AttackElement} damage!");
        }

        var attack = new AttackDamage(
            damage.StandardDamage,
            DamageType,
            damage.ElementalDamage,
            damage.ElementalDamage > 0 ? AttackElement : null);

        var result = target.TakeDamage(attack);

        if (result is { IsDodged: false, FinalElementalDamage: > 0 })
        {
            if (ApplyElementalStatusEffect())
            {
                //apply status effect
            }

            var dotPower = Math.Max(1, (ElementalPower!.Value + 3) / 4);
            
            var elementalDoTEffect = new ActiveEffect(ActiveEffectType.ElementalDamageOverTime,
                new ElementalDamageOverTimeEffectKey(AttackElement!.Value), dotPower, 3, sourceCaster: this);

            var dotResult = target.ActiveEffectManager.Register(elementalDoTEffect);
            if (dotResult.Registration == RegistrationStatus.Registered)
            {
                AddActionItem("Primordial energy lingers after the attack.");
            }
            else if (dotResult.Registration == RegistrationStatus.Strengthened)
            {
                AddActionItem("The primordial energy clinging to you surges with power");
            }
        }


        return result;
    }
    
    private (int StandardDamage, int ElementalDamage) GetEffectiveDamage(double multiplier = 1.0)
    {
        var standardDamage = (int)Math.Round(
            GetStat(StatType.Attack) * multiplier,
            MidpointRounding.AwayFromZero);

        var corruptedModifier = ActiveEffectManager.GetOutgoingDamageDecrease();

        var effectiveStandardDamage = Math.Max(0, standardDamage - corruptedModifier);
        var effectiveElementalDamage = Math.Max(0, (ElementalPower ?? 0) - corruptedModifier);

        if (effectiveStandardDamage == 0 && effectiveElementalDamage == 0)
        {
            throw new AttackException($"{Name}'s attack has been completely suppressed!");
        }
        
        return (effectiveStandardDamage, effectiveElementalDamage);
    }

    private bool ApplyElementalStatusEffect()
    {
        var chance = ThreatLevel switch
        {
            ThreatLevel.Low => 0.0, //currently isn't assigned an element in the factory 
            ThreatLevel.Medium => 5.0,
            ThreatLevel.High => 10.0,
            ThreatLevel.Elite => 20.0,
            ThreatLevel.Boss => 30.0,
            _ => throw new AttackException("Invalid Threat Level for monster")
        };

        return Random.NextDouble() < chance / 100.0;
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
                    attackMin = newLevel * 3;
                    attackMax = newLevel * 6;
                    defenseMin = newLevel * 2;
                    defenseMax = newLevel * 4;
                    resistMin = newLevel * 2;
                    resistMax = newLevel * 4;
                    healthMin = newLevel * 8;
                    healthMax = newLevel * 12;
                    speedMin = 2;
                    speedMax = 4;
                    break;
                case ThreatLevel.High:
                    attackMin = newLevel * 5;
                    attackMax = newLevel * 8;
                    defenseMin = newLevel * 3;
                    defenseMax = newLevel * 6;
                    resistMin = newLevel * 3;
                    resistMax = newLevel * 6;
                    healthMin = newLevel * 12;
                    healthMax = newLevel * 18;
                    speedMin = 3;
                    speedMax = 6;
                    break;
                case ThreatLevel.Elite:
                    attackMin = newLevel * 6;
                    attackMax = newLevel * 10;
                    defenseMin = newLevel * 4;
                    defenseMax = newLevel * 7;
                    resistMin = newLevel * 4;
                    resistMax = newLevel * 7;
                    healthMin = newLevel * 18;
                    healthMax = newLevel * 25;
                    speedMin = 4;
                    speedMax = 7;
                    break;
                case ThreatLevel.Boss:
                    attackMin = newLevel * 8;
                    attackMax = newLevel * 12;
                    defenseMin = newLevel * 5;
                    defenseMax = newLevel * 9;
                    resistMin = newLevel * 5;
                    resistMax = newLevel * 9;
                    healthMin = newLevel * 25;
                    healthMax = newLevel * 35;
                    speedMin = 5;
                    speedMax = 9;
                    break;
                default:
                    attackMin = newLevel * 2;
                    attackMax = newLevel * 4;
                    defenseMin = newLevel * 1;
                    defenseMax = newLevel * 2;
                    resistMin = newLevel * 1;
                    resistMax = newLevel * 2;
                    healthMin = newLevel * 5;
                    healthMax = newLevel * 8;
                    speedMin = 1;
                    speedMax = 3;
                    break;
            }

            AttackPower = Random.Next(attackMin, attackMax + 1);
            DefensePower = Random.Next(defenseMin, defenseMax + 1);
            Resistance = Random.Next(resistMin, resistMax + 1);
            AggressionLevel = Random.Next(speedMin, speedMax + 1);
            MaxHealth = Random.Next(healthMin, healthMax + 1);
        }

        //set these regardless of level being different
        CurrentHealth = MaxHealth;
        DodgeChance = 0.002 * newLevel;
    }

    public virtual AttackResolutionResult TakeDamage(AttackDamage attack)
    {
        var modifiers = new MonsterDamageModifiers(
            GetStat(StatType.Defense),
            GetStat(StatType.Resistance),
            Resilience,
            Vulnerability
        );
        
        var blindedBonus = CalculateBlindedBonus(attack.StandardDamage, attack.ElementalDamage);

        var updatedAttack = new AttackDamage(attack.StandardDamage + blindedBonus.StandardBonus, attack.DamageType,
            attack.ElementalDamage + blindedBonus.ElementalBonus, attack.Element);
        
        var result = DamageResolver.ResolveMonsterDamage(
            updatedAttack,
            modifiers,
            DodgeChance + (GetStat(StatType.Speed) * 0.01));

        if (result.IsDodged)
        {
            AddActionItem($"{Name} dodged the attack and took no damage!");
            return result;
        }

        // Preserve damage tracking for successful Martial and Magical hits.
        if (result.StandardDamageType == DamageType.Martial)
            DamageRecord[DamageType.Martial] += 1;
        else if (result.StandardDamageType == DamageType.Magical)
            DamageRecord[DamageType.Magical] += 1;

        AddActionItem($"{Name} takes {result.FinalStandardDamage} damage!");

        if (result.FinalElementalDamage != 0)
        {
            AddActionItem($"{Name} takes {result.FinalElementalDamage} {result.Element} damage!");
        }

        if (result.IncomingElementalDamage != 0 && result.FinalElementalDamage == 0)
        {
            AddActionItem($"{Name} shrugs off the {result.Element} damage!");
        }

        CurrentHealth -= (result.FinalStandardDamage + result.FinalElementalDamage);
        if (CurrentHealth <= 0)
        {
            AddActionItem($"{Name} is now dead.");
            return result with {TargetDied = true};
        }

        return result;
    }
    
    private (int StandardBonus, int ElementalBonus) CalculateBlindedBonus(int standardDamage, int elementalDamage)
    {
        var blindedBonus = ActiveEffectManager.GetIncomingDamageIncrease();

        var standardBlindedBonus = 0;
        var elementalBlindedBonus = 0;

        var hasStandardDamage = standardDamage > 0;
        var hasElementalDamage = elementalDamage > 0;

        if (hasStandardDamage && hasElementalDamage)
        {
            standardBlindedBonus = (blindedBonus + 1) / 2;
            elementalBlindedBonus = blindedBonus / 2;
        }
        else if (hasStandardDamage)
        {
            standardBlindedBonus = blindedBonus;
        }
        else if (hasElementalDamage)
        {
            elementalBlindedBonus = blindedBonus;
        }

        return (standardBlindedBonus, elementalBlindedBonus);
    }

    public virtual void Heal(int regainedHealth)
    {
        if (regainedHealth < 0)
            throw new ArgumentOutOfRangeException(
                nameof(regainedHealth),
                "Healing amount cannot be negative.");

        var frozenReduction = ActiveEffectManager.GetHealingReduction();
        var actualHealing = Math.Max(0, regainedHealth - frozenReduction);

        var healedAmount = Math.Min(actualHealing, MaxHealth - CurrentHealth);

        CurrentHealth += healedAmount;

        AddActionItem($"{Name} heals for {healedAmount} health!");
    }

    public virtual int GetStat(StatType stat)
    {
        var damageBoost = DamageType switch
        {
            DamageType.Martial => ActiveEffectManager.GetStatModifier(StatType.Attack),
            DamageType.Magical => ActiveEffectManager.GetStatModifier(StatType.Magic),
            DamageType.Hybrid =>
                (int)Math.Round((ActiveEffectManager.GetStatModifier(StatType.Attack) + ActiveEffectManager.GetStatModifier(StatType.Magic)) / 2.0),
            _ => throw new InvalidDataException("Damage Type not properly mapped to a enum")
        };

        return stat switch
        {
            StatType.Defense => Math.Max(0, DefensePower + ActiveEffectManager.GetStatModifier(StatType.Defense)),
            StatType.Speed => Math.Max(0, AggressionLevel + ActiveEffectManager.GetStatModifier(StatType.Speed)),
            StatType.Attack or StatType.Magic => Math.Max(0, AttackPower + damageBoost),
            StatType.Resistance => Math.Max(0, Resistance + ActiveEffectManager.GetStatModifier(StatType.Resistance)),
            StatType.Health => CurrentHealth,
            _ => throw new StatTypeException("Monster Get Stat")
        };
    }

    public void ClearActionItems()
    {
        ActionItems.Clear();
    }

    public void AddActionItem(string action)
    {
        if (CurrentHealth <= 0) return;

        long key = DateTime.Now.Ticks;
        while (ActionItems.ContainsKey(key)) key++;

        ActionItems.Add(key, action);
    }

    public void AddActionItem(Skill skill)
    {
        if (CurrentHealth <= 0) return;

        long key = DateTime.Now.Ticks;
        while (ActionItems.ContainsKey(key)) key++;

        ActionItems.Add(key, $"{Name} uses {skill.Name}!");
    }

    public Item? Loot()
    {
        if (Treasure == null)
            return null;

        var loot = Treasure;

        loot.MonsterId = null; // Remove the item from the monster
        loot.Monster = null;
        Treasure = null;

        return loot;
    }

    public void SetLoot(Item item)
    {
        item.MonsterId = Id;
        item.Monster = this;
        Treasure = item;
    }
}