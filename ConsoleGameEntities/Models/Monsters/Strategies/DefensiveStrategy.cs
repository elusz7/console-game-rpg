using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class DefensiveStrategy : DefaultStrategy
{
    /*
     * 1. Try To Heal
     * 2. If not heal, try to use an damage skill or debuff skill
     * 3. slightly weaker basic attack as last resort or followup
     */
    public override void ExecuteAttack(IMonster monster, ITargetable target)
    {
        bool healingUsed = false;

        // 1. Prioritize healing if below max HP
        if (monster.CurrentHealth < monster.MaxHealth)
        {
            var healingSkill = MonsterSkillHelper.GetHealingSkill(monster);

            if (healingSkill != null)
            {
                healingSkill.Activate(monster);
                healingUsed = true;
            }
        }
        
        if (!healingUsed)
        {
            // 2. Try to use the strongest damage skill
            var damageSkill = MonsterSkillHelper.GetHighestDamageSkill(monster);
            if (damageSkill != null)
            {
                damageSkill.Activate(monster, target);
                return;
            }

            // 3. If no damage skill, try debuffing the player
            var debuffSkill = MonsterSkillHelper.GetDebuffSkill(monster);

            debuffSkill?.Activate(monster, target);
        }

        // 4. As a fallback or follow up to healing/debuff, do a basic attack
        var decreasedDamage = (int)Math.Ceiling(monster.AttackPower * .8);
        target.TakeDamage(decreasedDamage, monster.DamageType);
    }
}

