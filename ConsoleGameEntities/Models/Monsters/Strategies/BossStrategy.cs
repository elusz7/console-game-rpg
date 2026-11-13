using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BossStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    private static readonly Random Random = Random.Shared;

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
            var randomSkill = supportSkills[Random.Next(supportSkills.Count)];
            randomSkill.Activate(monster, target);
            monster.MakeBasicAttack(target);
            return;
        }

        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);

        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
            return;
        }

        monster.MakeBasicAttack(target);
    }
}