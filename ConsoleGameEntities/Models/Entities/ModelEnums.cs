using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace ConsoleGameEntities.Models.Entities;

public class ModelEnums
{
    public enum ItemType
    {
        Weapon = 0,
        Armor = 1
    }
    public enum SkillTypeEnum
    {
        Generic = 0,
        Martial = 1,
        Magic = 2,
        Support = 3,
        Ultimate = 4
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
}
