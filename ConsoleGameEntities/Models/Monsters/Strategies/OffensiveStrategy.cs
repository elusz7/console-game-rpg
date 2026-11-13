using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class OffensiveStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;

    /*
     *  Use damage skill if able
     *  use buff or healing skill if able
     *  basic attack with a slightly higher power
     */
    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);
        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
            return;
        }

        var buffSkill = _skillSelector.GetBuffSkill(monster);
        if (buffSkill != null)
        {
            buffSkill.Activate(monster);
            monster.MakeBasicAttack(target, 1.2);
            return;
        }

        var healingThreshold = monster.MaxHealth * 0.67;

        if (monster.CurrentHealth < healingThreshold)
        {
            var healthLost = monster.MaxHealth - monster.CurrentHealth;
            _skillSelector.GetHealingSkill(monster, healthLost)?.Activate(monster);
        }

        monster.MakeBasicAttack(target, 1.2);
    }
}