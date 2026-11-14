using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class DefensiveStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    
    /*
     * 1. Try To Heal
     * 2. If not heal, try to use a damage skill or debuff skill
     * 3. slightly weaker basic attack as last resort or followup
     */
    public override AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target)
    {
        // 1. Prioritize healing if below max HP
        if (monster.CurrentHealth < monster.MaxHealth)
        {
            var healthLost = monster.MaxHealth - monster.CurrentHealth;
            var healingSkill = _skillSelector.GetHealingSkill(monster, healthLost);

            if (healingSkill != null)
            {
                var skillResult = healingSkill.Activate(monster);
                return monster.MakeBasicAttack(target, 0.8) with {SkillResult = skillResult};
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

        // 3. If no damage skill, try debuffing the player
        var debuffResult = _skillSelector.GetDebuffSkill(monster)?.Activate(monster, target);

        if (debuffResult != null)
            return monster.MakeBasicAttack(target, 0.8) with {SkillResult = debuffResult};
        
        return monster.MakeBasicAttack(target, 0.8);
    }
}