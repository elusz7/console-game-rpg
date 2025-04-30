using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleGameEntities.Models.Skills;

public class UltimateSkill : Skill
{
    // "Cooldown" here means the charge-up time before the ultimate can be used again.
    // ElapsedTime must meet or exceed Cooldown.
    [NotMapped]
    public bool IsReady => ElapsedTime >= Cooldown;

    public override bool Activate(ITargetable? caster, List<ITargetable>? targets)
    {
        if (!IsReady || targets == null || targets.Count == 0)
            return false;

        if (caster is Player player)
        {
            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { return false; }

            foreach (var target in targets)
                target.TakeDamage(Power, ESkillType);
            
            ElapsedTime = 0;
            return true;
        }
        else if (caster is Monster attacker)
        {
            targets[0].TakeDamage(Power, ESkillType);
            ElapsedTime = 0;
            return true;
        }

        return false;
    }
}
