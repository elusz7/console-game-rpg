using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Exceptions.Skills;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameEntities.Models.Skills;

public class BossSkill : DamageSkill
{
    [NotMapped]
    public int Phase { get; set; }

    [NotMapped]
    public bool IsReady => ElapsedTime >= Cooldown;

    public override void InitializeSkill(int level)
    {
        Phase = 1;
        Reset();
    }

    public override SkillResolutionResult Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (!IsReady)
            throw new SkillCooldownException("This skill is still on cooldown.");

        ValidateCasterLevel(caster);
        ValidateBossTargets(caster, singleEnemy, multipleEnemies);

        var multiplier = Phase switch
        {
            1 => 1.0,
            2 => 1.5,
            3 => 2.0,
            _ => 1.0
        };

        var damage = (int)(Power * multiplier);
        var attack = new AttackDamage(damage, DamageType, 0, null);

        // Start cooldown once activation is committed.
        Reset();

        caster.AddActionItem(this);
        var result = singleEnemy!.TakeDamage(attack);

        return new SkillResolutionResult(this, [result]);
    }

    public override BossSkill Clone()
    {
        var clone = new BossSkill
        {
            DamageType = DamageType,
            Phase = Phase
        };

        CopyTo(clone);

        return clone;
    }

    private void ValidateBossTargets(ITargetable caster, ITargetable? singleEnemy, List<ITargetable>? multipleEnemies)
    {
        if (caster is not BossMonster)
            throw new InvalidSkillConfigurationException(
                "Only boss monsters can use boss skills.");

        ValidateDamageTargets(caster, singleEnemy, multipleEnemies);

        if (singleEnemy is not Player)
            throw new InvalidTargetException(
                "Boss skills must target a player.");
    }
}
