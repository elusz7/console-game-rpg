using System.Linq;
using System.Threading;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class DefaultStrategy : IMonsterStrategy
{
    /*
     * DefaultMonsterStrategy
     * heal if at less than a 1/3 health, buff if between 1/2 health and maximum, else debuff
     * use attack skill if one's available otherwise basic attack
    */
    public virtual void ExecuteAttack(IMonster monster, IPlayer target)
    {
        bool usedSupportSkill = false;

        if (monster.CurrentHealth < monster.MaxHealth / 3)
        {
            var healingSkill = MonsterSkillHelper.GetHealingSkill(monster);
            if (healingSkill != null)
            {
                healingSkill.Activate(monster);
                usedSupportSkill = true;
            }
        }
        else if (monster.CurrentHealth < monster.MaxHealth
                && monster.CurrentHealth > monster.MaxHealth / 2)
        {
            var buffSkill = MonsterSkillHelper.GetBuffSkill(monster);
            if (buffSkill != null)
            {
                buffSkill.Activate(monster);
                usedSupportSkill = true;
            }
        }
        else if (monster.CurrentHealth > monster.MaxHealth / 4)
        {
            var debuffSkill = MonsterSkillHelper.GetDebuffSkill(monster);
            if (debuffSkill != null)
            {
                debuffSkill.Activate(monster, target);
                usedSupportSkill = true;
            }
        }

        if (!usedSupportSkill)
        {
            var damageSkill = MonsterSkillHelper.GetHighestDamageSkill(monster);

            if (damageSkill != null)
            {
                damageSkill.Activate(monster, target);
                return;
            }
        }

        monster.AddActionItem($"{monster.Name} attacks for {monster.AttackPower} damage!");
        target.TakeDamage(monster.AttackPower, monster.DamageType);
    }
}

