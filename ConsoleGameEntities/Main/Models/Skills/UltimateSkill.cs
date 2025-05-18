using ConsoleGameEntities.Main.Models.Monsters;
using ConsoleGameEntities.Main.Interfaces.Attributes;
using ConsoleGameEntities.Main.Models.Entities;
using static ConsoleGameEntities.Main.Models.Entities.ModelEnums;
using ConsoleGameEntities.Main.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleGameEntities.Main.Models.Skills;

public class UltimateSkill : Skill
{
    // "Cooldown" here means the charge-up time before the ultimate can be used again.
    // ElapsedTime must meet or exceed Cooldown.
    [NotMapped]
    public bool IsReady => ElapsedTime >= Cooldown;
    private int previousScaledLevel = 0;


    public override void Activate(ITargetable? caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (!IsReady) throw new SkillCooldownException();

        if (singleEnemy == null && (multipleEnemies == null || multipleEnemies.Count == 0))
            throw new InvalidTargetException("No suitable target(s) provided.");

        bool monsterDeath = false;

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
                    singleEnemy.TakeDamage(Power, DamageType);
                    break;

                case TargetType.AllEnemies:
                    foreach (var tar in multipleEnemies)
                    {
                        try
                        {
                            tar.TakeDamage(Power, DamageType);
                        }
                        catch (MonsterDeathException) { monsterDeath = true; }
                    }
                    break;
            }
        }
        else if (caster is Monster monster)
        {
            if (monster.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            ScalePowerWithLevel(monster.Level);

            monster.AddActionItem(this);

            singleEnemy.TakeDamage(Power, DamageType);
        }

        Reset();

        if (monsterDeath)
        {
            throw new MonsterDeathException();
        }
    }

    public void ScalePowerWithLevel(int level)
    {
        if (level != previousScaledLevel)
        {
            Power = Power * (int)(1 + (Math.Pow((level - 3), 1.2) / 5));

            Cooldown += level % 2;

            previousScaledLevel = level;
        }
    }
}
