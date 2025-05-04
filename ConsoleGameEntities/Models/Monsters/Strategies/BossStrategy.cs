using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BossStrategy : DefaultStrategy
{
    private static Random _rng = new Random(DateTime.Now.Millisecond);

    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var availableSkills = MonsterSkillHelper.GetAvailableBossSkills(monster).ToList();

        if (availableSkills.Count > 0)
        {
            int rand = _rng.Next(availableSkills.Count);
            availableSkills[rand].Activate(monster, target);
            return;
        }

        target.TakeDamage(monster.AttackPower, monster.DamageType);
    }
}
