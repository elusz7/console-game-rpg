using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces.Attributes;

public interface IMonsterSkillSelector
{
    Skill? GetHighestDamageSkill(IMonster monster);
    BossSkill? GetStrongestBossSkill(IMonster monster);
    List<SupportSkill> GetSupportSkills(IMonster monster);
    SupportSkill? GetDebuffSkill(IMonster monster, StatType? affectedStat = null);
    SupportSkill? GetBuffSkill(IMonster monster, StatType? affectedStat = null);
    SupportSkill? GetHealingSkill(IMonster monster, int desiredHealingPower);
    UltimateSkill? GetUltimateSkill(IMonster monster);
}
