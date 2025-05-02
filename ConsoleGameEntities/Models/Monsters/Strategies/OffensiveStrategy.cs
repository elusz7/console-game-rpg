using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters.Strategies;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class OffensiveStrategy : DefaultStrategy
{
    /*
     *  Use damage skill if can
     *  use buff or healing skill if can
     *  basic attack with a slightly higher power
    */
    public override void ExecuteAttack(IMonster monster, ITargetable target)
    {
        var damageSkill = MonsterSkillHelper.GetHighestDamageSkill(monster);
        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
            return;
        }

        var buffSkill = MonsterSkillHelper.GetBuffSkill(monster);
        if (buffSkill != null)
            buffSkill.Activate(monster);
        else if (monster.CurrentHealth < (monster.MaxHealth * 2.0 / 3.0))
        {
            MonsterSkillHelper.GetHealingSkill(monster)?.Activate(monster);
        }

        var increasedDamage = (int)Math.Ceiling(monster.AttackPower * 1.2);
        target.TakeDamage(increasedDamage, monster.DamageType);
    }
}
