using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace ConsoleGameEntities.Models.Entities;

public class ModelEnums
{
    public enum ItemType
    {
        Weapon = 0,
        Armor = 1
    }
    public enum SkillType
    {
        Martial,
        Magic,
        Support,
        Utility
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
}
