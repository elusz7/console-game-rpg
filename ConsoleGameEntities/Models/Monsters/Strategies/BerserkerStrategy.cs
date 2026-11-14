using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BerserkerStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;

    /*
     *  Use a damage skill if available; otherwise, strike once with 1.5× base attack
     */
    public override AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target)
    {
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

        return monster.MakeBasicAttack(target, 1.5);
    }
}