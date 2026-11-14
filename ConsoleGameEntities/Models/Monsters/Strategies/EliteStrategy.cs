using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class EliteStrategy(IMonsterSkillSelector skillSelector, MonsterBehaviorType behavior) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    private readonly IMonsterStrategy _strategy = Monster.GetStrategyFor(behavior, skillSelector);

   public override AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target)
    {
        var ultimateSkill = _skillSelector.GetUltimateSkill(monster);
        if (ultimateSkill is { IsReady: true })
        {
            var skillResult = ultimateSkill.Activate(monster, target);
            
            if (skillResult.AttackResults.Count != 1)
                throw new InvalidOperationException(
                    "Monster damage skills must produce exactly one attack result."); //this shouldn't ever throw because of all the target validation
            
            var attackAction = skillResult.AttackResults[0];

            return attackAction with { SkillResult = skillResult };
        }

        return _strategy.ExecuteAttack(monster, target);
    }
}