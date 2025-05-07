using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Interfaces;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Helpers;

public static class MonsterSkillHelper
{
    public static IEnumerable<Skill> GetAvailableSkills(IMonster monster)
    {
        return monster.Skills?
            .Where(s => s.RequiredLevel <= monster.Level && !s.IsOnCooldown)
            ?? Enumerable.Empty<Skill>();
    }

    private static IEnumerable<SupportSkill> GetAvailableSupportSkills(IMonster monster)
    {
        return monster.Skills?.OfType<SupportSkill>()
            .Where(s => s.RequiredLevel <= monster.Level && !s.IsOnCooldown)
            ?? Enumerable.Empty<SupportSkill>();
    }

    public static Skill? GetHighestDamageSkill(IMonster monster)
    {
        return GetAvailableSkills(monster)?
            .Where(s => s.SkillCategory == SkillCategory.Basic)
            .OrderByDescending(s => s.Power)
            .FirstOrDefault();
    }

    public static List<Skill> GetAvailableDamageSkills(IMonster monster)
    {
        return GetAvailableSkills(monster)?
            .Where(s => s.SkillCategory == SkillCategory.Basic)
            .OrderByDescending(s => s.Power)
            .ToList() ?? new List<Skill>();
    }

    public static Skill? GetHealingSkill(IMonster monster)
    {
        return GetAvailableSupportSkills(monster)?
            .Where(s => s.StatAffected == StatType.Health)
            .OrderByDescending(s => s.Power)
            .FirstOrDefault();
    }

    public static Skill? GetClosestHealingSkill(IMonster monster, int desiredHealingPower)
    {
        return GetAvailableSupportSkills(monster)?
            .Where(s => s.StatAffected == StatType.Health)
            .OrderBy(skill => Math.Abs(skill.Power - desiredHealingPower))
            .FirstOrDefault();
    }

    public static Skill? GetBuffSkill(IMonster monster, StatType? affectedStat = null)
    {
        return GetAvailableSupportSkills(monster)?
            .Where(s => s.StatAffected != StatType.Health)
            .Where(s => affectedStat == null || s.StatAffected == affectedStat)
            .Where(s => s.TargetType == TargetType.Self)
            .OrderByDescending(s => s.Power)
            .FirstOrDefault();
    }

    public static Skill? GetDebuffSkill(IMonster monster, StatType? affectedStat = null)
    {
        return GetAvailableSupportSkills(monster)?
            .Where(s => affectedStat == null || s.StatAffected == affectedStat)
            .Where(s => s.TargetType == TargetType.SingleEnemy)
            .OrderByDescending(s => s.Power)
            .FirstOrDefault();
    }

    public static Skill? GetUltimate(IMonster monster)
    {
        return monster.Skills?.OfType<UltimateSkill>()
            .Where(s => s.IsReady).FirstOrDefault();
    }
    
    public static List<T> FilterByType<T>(IMonster monster) where T : Skill
    {
        return monster.Skills?
            .Where(s => s.RequiredLevel <= monster.Level && !s.IsOnCooldown)
            .OfType<T>()
            .ToList() ?? new List<T>();
    }
}