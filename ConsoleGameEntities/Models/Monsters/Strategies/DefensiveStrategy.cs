using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class DefensiveStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    
    /*
     * 1. Try To Heal
     * 2. If not heal, try to use a damage skill or debuff skill
     * 3. slightly weaker basic attack as last resort or followup
     */
    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        // 1. Prioritize healing if below max HP
        if (monster.CurrentHealth < monster.MaxHealth)
        {
            var healthLost = monster.MaxHealth - monster.CurrentHealth;
            var healingSkill = _skillSelector.GetHealingSkill(monster, healthLost);

            if (healingSkill != null)
            {
                healingSkill.Activate(monster);
                monster.MakeBasicAttack(target, 0.8);
                return;
            }
        }

        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);
        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
            return;
        }

        // 3. If no damage skill, try debuffing the player
        _skillSelector.GetDebuffSkill(monster)?.Activate(monster, target);

        monster.MakeBasicAttack(target, 0.8);
    }
}