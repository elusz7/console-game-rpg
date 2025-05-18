using ConsoleGameEntities.Main.Helpers;
using ConsoleGameEntities.Main.Interfaces;
using ConsoleGameEntities.Main.Interfaces.Attributes;
using ConsoleGameEntities.Main.Models.Monsters.Strategies;

namespace ConsoleGameEntities.Main.Models.Monsters.Strategies;

public class OffensiveStrategy : DefaultStrategy
{
    /*
     *  Use damage skill if can
     *  use buff or healing skill if can
     *  basic attack with a slightly higher power
    */
    public override void ExecuteAttack(IMonster monster, IPlayer target)
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
        monster.AddActionItem($"{monster.Name} attacks for {increasedDamage} damage!");
        target.TakeDamage(increasedDamage, monster.DamageType);
    }
}
