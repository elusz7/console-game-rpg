using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BerserkerStrategy : DefaultStrategy
{
    /*
     *  Use a damage skill if available; otherwise, strike once with 1.5× base attack
    */
    public override void ExecuteAttack(IMonster monster, ITargetable target)
    {
        var damageSkill = MonsterSkillHelper.GetHighestDamageSkill(monster);

        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
        }
        else
        {
            int boostedDamage = (int)Math.Ceiling(monster.AttackPower * 1.5);
            target.TakeDamage(boostedDamage, monster.DamageType);
        }
    }
}

