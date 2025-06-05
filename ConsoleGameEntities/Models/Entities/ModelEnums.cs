using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace ConsoleGameEntities.Models.Entities;

public class ModelEnums
{
    public enum ItemType
    {
        Weapon = 0,
        Armor = 1,
        Valuable = 2,
        Consumable = 3
    }

    public enum TargetType
    {
        Self = 0,            // Buffs, heals yourself
        SingleEnemy = 1,     // Single-target attacks, debuffs
        AllEnemies = 2     // AoE attacks, mass debuff
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

    public static class SkillKeys
    {
        public const string CATEGORY = "SkillCategory";
        public const string TARGET = "TargetType";
        public const string POWER = "Power";
        public const string LEVEL = "Level";
        public const string COST = "Cost";
        public const string COOLDOWN = "Cooldown";
        public const string DAMAGE = "DamageType";
        public const string STAT = "StatEffected";
        public const string EFFECT = "SupportEffect";
        public const string DURATION = "Duration";
    }

    public enum SkillType
    {
        MartialSkill = 0,
        MagicSkill = 1,
        SupportSkill = 2,
        UltimateSkill = 3,
        BossSkill = 4
    }

    public enum SupportEffectType
    {
        Boost = 0,
        Reduce = 1
    }

    public enum DamageType
    {
        Martial = 0,
        Magical = 1,
        Hybrid = 2
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
        Head = 0,
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

    public enum ElementType
    {
        Fire = 0,
        Lightning = 1,
        Ice = 2,
        Nature = 3,
        Radiance = 4,
        Abyssal = 5
    }

    public enum RarityLevel
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5
    }

    public enum ElementalStatusEffectType
    {
        Charred = 0,
        Shocked = 1,
        Frozen = 2,
        Snared = 3,
        Blinded = 4,
        Corrupted = 5
    }
}
