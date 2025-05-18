using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Main.Helpers;
using ConsoleGameEntities.Main.Interfaces;
using ConsoleGameEntities.Main.Interfaces.Attributes;

namespace ConsoleGameEntities.Main.Models.Monsters.Strategies;

public class CautiousStrategy : DefaultStrategy
{
    /*
        CautiousMonsterStrategy:
        debuff > damage > heal > buff
    */
    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var debuffUsed = false;

        var debuffSkill = MonsterSkillHelper.GetDebuffSkill(monster);
        if (debuffSkill != null)
        {
            debuffUsed = true;
            debuffSkill.Activate(monster, target);
        }

        if (!debuffUsed)
        {
            var damageSkill = MonsterSkillHelper.GetHighestDamageSkill(monster);
            if (damageSkill != null) {
                damageSkill.Activate(monster, target);
                return;
            }
            
            var healingSkill = MonsterSkillHelper.GetHealingSkill(monster);
            if (healingSkill != null)
            {
                healingSkill.Activate(monster);
            }
            else
                MonsterSkillHelper.GetBuffSkill(monster)?.Activate(monster);
        }

        var decreasedDamage = (int)Math.Ceiling(monster.AttackPower * .8);
        monster.AddActionItem($"{monster.Name} attacks for {decreasedDamage} damage!");
        target.TakeDamage(decreasedDamage, monster.DamageType);
    }
}
