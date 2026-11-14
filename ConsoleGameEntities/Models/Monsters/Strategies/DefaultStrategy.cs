using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class DefaultStrategy : IMonsterStrategy
{
    private readonly IMonsterSkillSelector _skillSelector;

    public DefaultStrategy()
    {
    }

    public DefaultStrategy(IMonsterSkillSelector skillSelector)
    {
        _skillSelector = skillSelector;
    }

    /*
     * DefaultMonsterStrategy
     * heal if at less than a 1/3 health, buff if between 1/2 health and maximum, else debuff
     * use attack skill if one's available otherwise basic attack
     */
    public virtual AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target)
    {
        if (_skillSelector == null) throw new InvalidOperationException("Skill selector is null!");

        var healthLost = monster.MaxHealth - monster.CurrentHealth;
        var healingThreshold = monster.MaxHealth * 0.33;
        var buffThreshold = monster.MaxHealth * 0.5;

        if (monster.CurrentHealth < healingThreshold)
        {
            var healingSkill = _skillSelector.GetHealingSkill(monster, healthLost);
            if (healingSkill != null)
            {
                var skillResult = healingSkill.Activate(monster);
                return monster.MakeBasicAttack(target) with {SkillResult = skillResult};
            }
        }
        else if (monster.CurrentHealth > buffThreshold)
        {
            var buffSkill = _skillSelector.GetBuffSkill(monster);
            if (buffSkill != null)
            {
                var skillResult = buffSkill.Activate(monster);
                return monster.MakeBasicAttack(target) with {SkillResult = skillResult};
            }
        }
        else
        {
            var debuffSkill = _skillSelector.GetDebuffSkill(monster);
            if (debuffSkill != null)
            {
                var skillResult = debuffSkill.Activate(monster, target);
                return monster.MakeBasicAttack(target) with {SkillResult = skillResult};
            }
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