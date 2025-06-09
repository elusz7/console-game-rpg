using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class BerserkerStrategy : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector;

    public BerserkerStrategy(IMonsterSkillSelector skillSelector)
    {
        _skillSelector = skillSelector;
    }

    /*
     *  Use a damage skill if available; otherwise, strike once with 1.5× base attack
    */
    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {
        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);

        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
            return;
        }

        int boostedDamage = (int)Math.Ceiling(monster.Combat.GetStat((Monster)monster, StatType.Attack) * 1.5);
        monster.Logger.Log($"{monster.Name} attacks for {boostedDamage} damage!");
        target.TakeDamage(boostedDamage, monster.DamageType);

    }
}

