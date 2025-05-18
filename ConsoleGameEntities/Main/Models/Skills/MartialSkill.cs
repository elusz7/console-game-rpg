using ConsoleGameEntities.Main.Interfaces.Attributes;
using ConsoleGameEntities.Main.Models.Entities;
using static ConsoleGameEntities.Main.Models.Entities.ModelEnums;
using ConsoleGameEntities.Main.Models.Monsters;
using ConsoleGameEntities.Main.Exceptions;

namespace ConsoleGameEntities.Main.Models.Skills;

public class MartialSkill : Skill
{
    public override void Activate(ITargetable? caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown) 
            throw new SkillCooldownException();

        if (singleEnemy == null && (multipleEnemies == null || multipleEnemies.Count == 0))
            throw new InvalidTargetException("No suitable target(s) provided.");

        var monsterDeath = false;

        if (caster is Player player)
        {
            if (player.Level < RequiredLevel)
                throw new InvalidSkillLevelException("Your level is too low to use this skill.");

            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { throw new SkillResourceException($"You don't have enough {player.Archetype.ResourceName}."); }

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

            Reset();
            if (monsterDeath)
                throw new MonsterDeathException();
        }
        else if (caster is Monster monster)
        {
            if (monster.Level < RequiredLevel)
                throw new InvalidSkillLevelException("This monster's level is too low to use this skill.");

            monster.AddActionItem(this);

            singleEnemy.TakeDamage(Power, DamageType);
            Reset();
        }
    }
}
