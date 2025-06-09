using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class OffensiveStrategy : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector;

    public OffensiveStrategy(IMonsterSkillSelector skillSelector)
    {
        _skillSelector = skillSelector;
    }

    /*
     *  Use damage skill if can
     *  use buff or healing skill if can
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
            MakeAttack(monster, target);
            return;
        }

        var healingThreshold = monster.MaxHealth * 0.67;

        if (monster.CurrentHealth < healingThreshold)
        {
            var healthLost = monster.MaxHealth - monster.CurrentHealth;
            _skillSelector.GetHealingSkill(monster, healthLost)?.Activate(monster);
        }

        MakeAttack(monster, target);
    }

    private static void MakeAttack(IMonster monster, IPlayer target)
    {
        var increasedDamage = (int)Math.Ceiling(monster.Combat.GetStat((Monster)monster, StatType.Attack) * 1.2);
        monster.Logger.Log($"{monster.Name} attacks for {increasedDamage} damage!");
        target.TakeDamage(increasedDamage, monster.DamageType);
    }
}
