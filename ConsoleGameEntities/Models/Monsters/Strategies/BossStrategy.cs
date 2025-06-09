using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BossStrategy : DefaultStrategy
{
    private readonly static Random _rng = Random.Shared;
    private readonly IMonsterSkillSelector _skillSelector;

    public BossStrategy(IMonsterSkillSelector skillSelector)
    {
        _skillSelector = skillSelector;
    }

    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var strongestBossSkill = _skillSelector.GetStrongestBossSkill(monster);

        if (strongestBossSkill != null)
        {
            strongestBossSkill.Activate(monster, target);
            return;
        }

        var supportSkills = _skillSelector.GetSupportSkills(monster);

        if (supportSkills.Count > 0)
        {
            var randomSkill = supportSkills[_rng.Next(supportSkills.Count)];
            randomSkill.Activate(monster, target);
            MakeAttack(monster, target);
            return;
        }

        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);

        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
            return;
        }

        MakeAttack(monster, target);
    }

    private static void MakeAttack(IMonster monster, IPlayer target)
    {
        monster.Logger.Log($"{monster.Name} attacks for {monster.Combat.GetStat((Monster)monster, StatType.Attack)} damage!");
        target.TakeDamage(monster.Combat.GetStat((Monster)monster, StatType.Attack), monster.DamageType);
    }
}
