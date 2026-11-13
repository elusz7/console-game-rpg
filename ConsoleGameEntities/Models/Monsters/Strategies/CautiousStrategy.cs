using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class CautiousStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    
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
            monster.MakeBasicAttack(target, 0.8);
            return;
        }

        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);
        if (damageSkill != null)
        {
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
                monster.MakeBasicAttack(target, 0.8);
                return;
            }
        }

        _skillSelector.GetBuffSkill(monster)?.Activate(monster);

        monster.MakeBasicAttack(target, 0.8);
    }
}