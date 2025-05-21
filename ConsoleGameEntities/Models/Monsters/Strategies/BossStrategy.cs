using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BossStrategy : DefaultStrategy
{
    private readonly static Random _rng = Random.Shared;
    private readonly IMonsterSkillSelector _skillSelector;

    public BossStrategy(IMonsterSkillSelector skillSelector)
    {
        _skillSelector = skillSelector;
    }

    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var strongestBossSkill = _skillSelector.GetStrongestBossSkill(monster);

        if (strongestBossSkill != null)
        {
            strongestBossSkill.Activate(monster, target);
            return;
        }

        var supportSkills = _skillSelector.GetSupportSkills(monster);

        if (supportSkills.Count > 0)
        {
            var randomSkill = supportSkills[_rng.Next(supportSkills.Count)];
            randomSkill.Activate(monster, target);
            MakeAttack(monster, target);
            return;
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
        monster.AddActionItem($"{monster.Name} attacks for {monster.AttackPower} damage!");
        target.TakeDamage(monster.AttackPower, monster.DamageType);
    }
}
