using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;

namespace ConsoleGameEntities.Models.Monsters;

public class EliteMonster : Monster
{
    public override void Attack(IPlayer target)
    {
        var ultimate = MonsterSkillHelper.GetUltimate(this);
        if (ultimate != null)
        {
            ultimate.Activate(this, target);
            return;
        }
        Strategy.ExecuteAttack(this, target);
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
