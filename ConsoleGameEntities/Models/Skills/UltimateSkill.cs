using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleGameEntities.Models.Skills;

public class UltimateSkill : Skill
{
    // "Cooldown" here means the charge-up time before the ultimate can be used again.
    // ElapsedTime must meet or exceed Cooldown.
    [NotMapped]
    public bool IsReady => ElapsedTime >= Cooldown;
    private int BasePower = 0;


    public override void Activate(ITargetable? caster, ITargetable? target = null, List<ITargetable>? targets = null)
    {
        if (!IsReady) throw new SkillCooldownException();

        if (target == null || targets == null || targets.Count == 0) throw new InvalidTargetException("Ultimate");

        if (caster is Player player)
        {
            if (player.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            ScalePowerWithLevel(player.Level);

            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { throw new SkillResourceException("Ultimate"); }

            player.AddActionItem(this);

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

            ScalePowerWithLevel(monster.Level);

            monster.AddActionItem(this);

            target.TakeDamage(Power, DamageType);
            ElapsedTime = 0;
        }
    }

    private void ScalePowerWithLevel(int level)
    {
        if (BasePower == 0)
            BasePower = Power;

        Power = Power * (int)(1 + (Math.Pow((level - 3), 1.2) / 5));

        switch (level)
        {
            case 5: Cooldown = 7; break;
            case 7: Cooldown = 8; break;
            case 9: Cooldown = 9; break;
            case 10: Cooldown = 10; break;
        }
    }
}
