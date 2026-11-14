using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BossStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    private static readonly Random Random = Random.Shared;

    public override AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target)
    {
        var strongestBossSkill = _skillSelector.GetStrongestBossSkill(monster);

        if (strongestBossSkill != null)
        {
            var skillResult = strongestBossSkill.Activate(monster, target);
            
            if (skillResult.AttackResults.Count != 1)
                throw new InvalidOperationException(
                    "Monster damage skills must produce exactly one attack result."); //this shouldn't ever throw because of all the target validation
            
            var attackAction = skillResult.AttackResults[0];

            return attackAction with { SkillResult = skillResult };
        }

        var supportSkills = _skillSelector.GetSupportSkills(monster);

        if (supportSkills.Count > 0)
        {
            var randomSkill = supportSkills[Random.Next(supportSkills.Count)];
            var skillResult = randomSkill.Activate(monster, target);
            return monster.MakeBasicAttack(target) with {SkillResult = skillResult };
        }

        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);

        if (damageSkill != null)
        {
            var skillResult = damageSkill.Activate(monster, target);
            
            if (skillResult.AttackResults.Count != 1)
                throw new InvalidOperationException(
                    "Monster damage skills must produce exactly one attack result."); //this shouldn't ever throw because of all the target validation
            
            var attackAction = skillResult.AttackResults[0];

            return attackAction with { SkillResult = skillResult };
        }

        return monster.MakeBasicAttack(target);
    }
}