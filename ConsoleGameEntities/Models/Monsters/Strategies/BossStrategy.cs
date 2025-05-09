using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BossStrategy : DefaultStrategy
{
    private static Random _rng = new Random(Guid.NewGuid().GetHashCode());

    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var availableSkills = MonsterSkillHelper.GetAvailableSkills(monster).ToList();

        if (availableSkills.Count > 0)
        {
            var bossSkill = availableSkills
                .OfType<BossSkill>()
                .OrderByDescending(s => s.Power)
                .FirstOrDefault();

            if (bossSkill != null)
            {
                bossSkill.Activate(monster, target);
                return;
            }
            else
            {
                int rand = _rng.Next(availableSkills.Count);
                availableSkills[rand].Activate(monster, target);
                return;
            }
        }

        target.TakeDamage(monster.AttackPower, monster.DamageType);
    }
}
