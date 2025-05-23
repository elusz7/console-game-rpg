using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class DefensiveStrategy : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector;

    public DefensiveStrategy(IMonsterSkillSelector skillSelector)
    {
        _skillSelector = skillSelector;
    }

    /*
     * 1. Try To Heal
     * 2. If not heal, try to use an damage skill or debuff skill
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
                MakeAttack(monster, target);
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

        MakeAttack(monster, target);
    }

    private static void MakeAttack(IMonster monster, IPlayer target)
    {
        var decreasedDamage = (int)Math.Round(monster.GetStat(StatType.Attack) * .8);
        monster.AddActionItem($"{monster.Name} attacks for {decreasedDamage} damage!");
        target.TakeDamage(decreasedDamage, monster.DamageType);
    }
}

