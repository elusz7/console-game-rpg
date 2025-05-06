using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Exceptions;

namespace ConsoleGameEntities.Models.Skills;

public class MagicSkill : Skill
{
    public override void Activate(ITargetable? caster, ITargetable? target = null, List<ITargetable>? targets = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException();

        if (target == null || targets == null || targets.Count == 0)
            throw new InvalidTargetException("MagicSkill");

        if (caster is Player player)
        {
            if (player.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { throw new SkillResourceException(); }

            switch (TargetType)
            {
                case TargetType.SingleEnemy:
                        target.TakeDamage(Power, DamageType);
                    break;

                case TargetType.AllEnemies:
                    foreach (var tar in targets)
                        tar.TakeDamage(Power, DamageType);
                    break;
            }

            ElapsedTime = 0;
        }
        else if (caster is Monster monster)
        {
            if (monster.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            target.TakeDamage(Power, DamageType);
            ElapsedTime = 0;
        }
    }
}
