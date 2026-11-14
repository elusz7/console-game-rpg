using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class CautiousStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    
    /*
        CautiousMonsterStrategy:
        debuff > damage > heal > buff
    */
    public override AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target)
    {
        var debuffSkill = _skillSelector.GetDebuffSkill(monster);
        if (debuffSkill != null)
        {
            var skillResult = debuffSkill.Activate(monster, target);
            return monster.MakeBasicAttack(target, 0.8) with {SkillResult = skillResult};
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

        var healthLost = monster.MaxHealth - monster.CurrentHealth;
        var healingThreshold = monster.MaxHealth * 0.75;

        if (monster.CurrentHealth <= healingThreshold)
        {
            var healingSkill = _skillSelector.GetHealingSkill(monster, healthLost);
            if (healingSkill != null)
            {
                var skillResult = healingSkill.Activate(monster);
                return monster.MakeBasicAttack(target, 0.8) with {SkillResult = skillResult};
            }
        }

        var buffResult = _skillSelector.GetBuffSkill(monster)?.Activate(monster);

        if (buffResult != null)
            return monster.MakeBasicAttack(target, 0.8) with {SkillResult = buffResult};
        
        return monster.MakeBasicAttack(target, 0.8);
    }
}