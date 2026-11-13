using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

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
    public virtual void ExecuteAttack(IMonster monster, IPlayer target)
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
                healingSkill.Activate(monster);
                monster.MakeBasicAttack(target);
                return;
            }
        }
        else if (monster.CurrentHealth > buffThreshold)
        {
            var buffSkill = _skillSelector.GetBuffSkill(monster);
            if (buffSkill != null)
            {
                buffSkill.Activate(monster);
                monster.MakeBasicAttack(target);
                return;
            }
        }
        else
        {
            var debuffSkill = _skillSelector.GetDebuffSkill(monster);
            if (debuffSkill != null)
            {
                debuffSkill.Activate(monster, target);
                monster.MakeBasicAttack(target);
                return;
            }
        }

        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);
        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
            return;
        }

        monster.MakeBasicAttack(target);
    }
}