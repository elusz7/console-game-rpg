using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class EliteStrategy(IMonsterSkillSelector skillSelector, MonsterBehaviorType behavior) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    private readonly IMonsterStrategy _strategy = Monster.GetStrategyFor(behavior, skillSelector);

   public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var ultimateSkill = _skillSelector.GetUltimateSkill(monster);
        if (ultimateSkill is { IsReady: true })
        {
            ultimateSkill.Activate(monster, target);
            return;
        }

        _strategy.ExecuteAttack(monster, target);
    }
}