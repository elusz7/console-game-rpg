using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class CautiousStrategy : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector;

    public CautiousStrategy(IMonsterSkillSelector skillSelector)
    {
        _skillSelector = skillSelector;
    }

    /*
        CautiousMonsterStrategy:
        debuff > damage > heal > buff
    */
    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var debuffSkill = _skillSelector.GetDebuffSkill(monster);
        if (debuffSkill != null)
        {
            debuffSkill.Activate(monster, target);
            MakeAttack(monster, target);
            return;
        }

        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);
        if (damageSkill != null) {
            damageSkill.Activate(monster, target);
            return;
        }

        var healthLost = monster.MaxHealth - monster.CurrentHealth;
        var healingThreshold = monster.MaxHealth * 0.75;
        
        if (monster.CurrentHealth <= healingThreshold)
        {
            var healingSkill = _skillSelector.GetHealingSkill(monster, healthLost);
            if (healingSkill != null)
            {
                healingSkill.Activate(monster);
                MakeAttack(monster, target);
                return;
            }            
        }

        _skillSelector.GetBuffSkill(monster)?.Activate(monster);

        MakeAttack(monster, target);        
    }

    private static void MakeAttack(IMonster monster, IPlayer target)
    {
        var decreasedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);
        monster.AddActionItem($"{monster.Name} attacks for {decreasedDamage} damage!");
        target.TakeDamage(decreasedDamage, monster.DamageType);
    }
}
