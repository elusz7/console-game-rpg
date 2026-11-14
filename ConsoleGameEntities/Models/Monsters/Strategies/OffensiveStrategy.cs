using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class OffensiveStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;

    /*
     *  Use damage skill if able
     *  use buff or healing skill if able
     *  basic attack with a slightly higher power
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

        var buffSkill = _skillSelector.GetBuffSkill(monster);
        if (buffSkill != null)
        {
            var skillResult = buffSkill.Activate(monster);
            return monster.MakeBasicAttack(target, 1.2) with {SkillResult = skillResult};
        }

        var healingThreshold = monster.MaxHealth * 0.67;

        if (monster.CurrentHealth < healingThreshold)
        {
            var healthLost = monster.MaxHealth - monster.CurrentHealth;
            var skillResult = _skillSelector.GetHealingSkill(monster, healthLost)?.Activate(monster);
            
            if (skillResult != null)
                return monster.MakeBasicAttack(target, 1.2) with {SkillResult = skillResult};
            
        }

        return monster.MakeBasicAttack(target, 1.2);
    }
}