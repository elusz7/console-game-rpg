using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameEntities.Models.Skills;

public class MagicSkill : Skill
{
    public override bool Activate(ITargetable? caster, List<ITargetable>? targets = null)
    {
        if (ElapsedTime < Cooldown || targets == null || targets.Count == 0)
            return false;

        if (caster is Player player)
        {
            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { return false; }

            switch (TargetType)
            {
                case TargetType.SingleEnemy:
                case TargetType.RandomEnemy:
                    if (targets[0] is Monster singleTarget)
                        singleTarget.TakeDamage(Power, ESkillType);
                    break;

                case TargetType.AllEnemies:
                    foreach (var target in targets)
                        target.TakeDamage(Power, ESkillType);
                    break;
            }

            ElapsedTime = 0;
            return true;
        }
        else if (caster is Monster)
        {
            targets[0].TakeDamage(Power, ESkillType);
            ElapsedTime = 0;
            return true;
        }

        return false;
    }
}
