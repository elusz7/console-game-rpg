using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class EliteStrategy : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector;
    private readonly IMonsterStrategy _strategy;

    public EliteStrategy(IMonsterSkillSelector skillSelector, MonsterBehaviorType behavior)
    {
        _skillSelector = skillSelector;
        _strategy = Monster.GetStrategyFor(behavior, skillSelector);
    }

    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var ultimateSkill = _skillSelector.GetUltimateSkill(monster);
        if (ultimateSkill != null)
        {
            ultimateSkill.Activate(monster, target);
            return;
        }

        _strategy.ExecuteAttack(monster, target);
    }
}
