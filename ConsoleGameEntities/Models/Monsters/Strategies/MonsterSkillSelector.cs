using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class MonsterSkillSelector : IMonsterSkillSelector
{
    public static List<Skill> GetAvailableSkills(IMonster monster)
    {
        return monster.Skills
            .Where(s => s.RequiredLevel <= monster.Level)
            .Where(s => s is not UltimateSkill && s is not BossSkill && !s.IsOnCooldown)
            .ToList() ?? new List<Skill>();
    }
    public virtual Skill? GetHighestDamageSkill(IMonster monster)
    {
        return GetAvailableSkills(monster)
            .Where(s => s.SkillCategory == SkillCategory.Basic)
            .Where(s => s.TargetType != TargetType.AllEnemies)
            .OrderByDescending(s => s.Power)
            .FirstOrDefault();
    }
    public virtual BossSkill? GetStrongestBossSkill(IMonster monster)
    {
        return monster.Skills.OfType<BossSkill>()
            .Where(s => s.IsReady)
            .OrderByDescending(s => s.Power).FirstOrDefault();
    }

    public virtual List<SupportSkill> GetSupportSkills(IMonster monster)
    {
        return GetAvailableSkills(monster).OfType<SupportSkill>().ToList() 
            ?? new List<SupportSkill>();
    }

    public virtual SupportSkill? GetDebuffSkill(IMonster monster, StatType? affectedStat = null)
    {
        return GetSupportSkills(monster)?
            .Where(s => affectedStat == null || s.StatAffected == affectedStat)
            .Where(s => s.TargetType == TargetType.SingleEnemy)
            .OrderByDescending(s => s.Power)
            .FirstOrDefault();
    }
    public virtual SupportSkill? GetHealingSkill(IMonster monster, int desiredHealingPower)
    {
        return GetSupportSkills(monster)
            .Where(s => s.StatAffected == StatType.Health)
            .OrderBy(skill => Math.Abs(skill.Power - desiredHealingPower))
            .FirstOrDefault();
    }
    public virtual SupportSkill? GetBuffSkill(IMonster monster, StatType? affectedStat = null)
    {
        return GetSupportSkills(monster)
            .Where(s => affectedStat == null || s.StatAffected == affectedStat)
            .Where(s => s.TargetType == TargetType.Self)
            .OrderByDescending(s => s.Power).FirstOrDefault();
    }
    public virtual UltimateSkill? GetUltimateSkill(IMonster monster)
    {
        return monster.Skills.OfType<UltimateSkill>()
            .Where(s => s.IsReady).FirstOrDefault();
    }
}
