using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class DefaultStrategy : IMonsterStrategy
{
    private readonly IMonsterSkillSelector _skillSelector;

    public DefaultStrategy() { }
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
                MakeAttack(monster, target);
                return;
            }
        }
        else if (monster.CurrentHealth > buffThreshold)
        {
            var buffSkill = _skillSelector.GetBuffSkill(monster);
            if (buffSkill != null)
            {
                buffSkill.Activate(monster);
                MakeAttack(monster, target);
                return;
            }
        }
        else
        {
            var debuffSkill = _skillSelector.GetDebuffSkill(monster);
            if (debuffSkill != null)
            {
                debuffSkill.Activate(monster, target);
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

        MakeAttack(monster, target);
    }

    private static void MakeAttack(IMonster monster, IPlayer target)
    {
        monster.AddActionItem($"{monster.Name} attacks for {monster.GetStat(StatType.Attack)} damage!");
        target.TakeDamage(monster.GetStat(StatType.Attack), monster.DamageType);
    }
}

