using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace ConsoleGameEntities.Models.Entities;

public class ModelEnums
{
    public enum ItemType
    {
        Weapon = 0,
        Armor = 1
    }

    public enum TargetType
    {
        Self,            // Buffs, heals yourself
        SingleEnemy,     // Single-target attacks
        AllEnemies,      // AoE attacks
        RandomEnemy      // Randomly strike one enemy (good for "chaotic" abilities)
    }

    public enum ArchetypeType
    {
        Martial = 0,
        Magical = 1
    }

    public enum StatType
    {
        Health = 0,
        Attack = 1,
        Magic = 2,
        Defense = 3,
        Resistance = 4,
        Speed = 5
    }

    public enum SupportEffectType
    {
        Boost,
        Reduce
    }

    public enum DamageType
    {
        Generic = 0,
        Martial = 1,
        Magical = 2,
        Hybrid = 3
    }

    public enum SkillCategory
    {
        Basic = 0,
        Support = 1,
        Ultimate = 2
    }

    public enum MonsterBehaviorType
    {
        Default = 0,
        Berserker = 1,
        Offensive = 2,
        Defensive = 3,
        Cautious = 4,
        Cunning = 5,
        Boss = 6
    }

    public enum ThreatLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Elite = 3,
        Boss = 4
    }

    public enum ArmorType
    {
        Helmet = 0,
        Chest = 1,
        Arms = 2,
        Legs = 3
    }

    public enum ConsumableType
    {
        Health = 0,
        Durability = 1,
        Resource = 2
    }
}
