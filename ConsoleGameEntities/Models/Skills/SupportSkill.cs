using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Exceptions.Skills;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public class SupportSkill : Skill
{
    public int Duration { get; set; } // Duration MUST be less than cooldown
    public StatType StatAffected { get; set; }
    public SupportEffectType SupportEffect { get; set; }
    [NotMapped] protected override bool AllowsAllyTargets => true;

    // Track how many turns each target has left under the effect
    [NotMapped]
    private readonly HashSet<ITargetable> _targets = [];

    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException();

        ValidateCasterLevel(caster);
        ValidateSupportTargets(caster, singleEnemy, multipleEnemies);

        ExecuteSkill(caster, singleEnemy, multipleEnemies);
    }

    private void ExecuteSkill(ITargetable self, ITargetable? singleEnemy, List<ITargetable>? multipleEnemies)
    {
        if (self is Player player)
            ConsumePlayerResources(player);

        var appliedCount = 0;

        switch (TargetType)
        {
            case TargetType.Self:
                appliedCount = TryApplyEffect(self, self) ? 1 : 0;
                break;

            case TargetType.SingleEnemy:
                appliedCount = TryApplyEffect(self, singleEnemy!) ? 1 : 0;
                break;

            case TargetType.AllEnemies:
                foreach (var target in multipleEnemies!)
                {
                    if (TryApplyEffect(self, target))
                        appliedCount++;
                }
                break;
        }

        if (appliedCount == 0 && self is Player p)
        {
            RefundPlayerResources(p);
            throw new InvalidTargetException("Support");
        }

        self.AddActionItem(this);
        ElapsedTime = 0;
    }

    public override void UpdateElapsedTime()
    {
        ElapsedTime++;
    }

    public override void Reset()
    {
        foreach (var target in _targets)
            ClearEffectFromTarget(target);

        _targets.Clear();
        ElapsedTime = 0;
    }

    private bool TryApplyEffect(ITargetable caster, ITargetable target)
    {
        if (StatAffected == StatType.Health)
        {
            target.Heal(Power);
            return true;
        }
        
        var identityKey = new SupportSkillEffectKey(this);

        if (target.ActiveEffectManager.GetByIdentityKey(identityKey) != null)
            return false;

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            identityKey,
            power: Power,
            duration: Duration,
            sourceCaster: caster,
            sourceSkill: this);

        var result = target.ActiveEffectManager.Register(effect);

        if (result.Registration == RegistrationStatus.RejectedDuplicate)
            return false;

        _targets.Add(target);
        return true;
    }

    private void ClearEffectFromTarget(ITargetable target)
    {
        var result = target.ActiveEffectManager.ClearBySource(this);
        if (result.HasRemovedEffects)
            target.AddActionItem($"{Name} effect removed");
    }

    public override SupportSkill Clone()
    {
        var clone = new SupportSkill
        {
            Duration = Duration,
            StatAffected = StatAffected,
            SupportEffect = SupportEffect
        };

        CopyTo(clone);

        return clone;
    }

    private void ValidateSupportTargets(ITargetable caster, ITargetable? singleEnemy, List<ITargetable>? multipleEnemies)
    {
        ValidateTargets(caster, singleEnemy, multipleEnemies);

        if (TargetType == TargetType.Self && SupportEffect != SupportEffectType.Boost)
            throw new InvalidSkillConfigurationException("Support skills targeting self must boost stats.");
        
        if (caster is Player && TargetType != TargetType.Self && SupportEffect != SupportEffectType.Reduce)
            throw new InvalidSkillConfigurationException("Player support skills targeting enemies must reduce enemy stats.");
        
        if (TargetType == TargetType.SingleEnemy)
        {
            if (caster is Player && singleEnemy is not Monster _)
                throw new InvalidTargetException("Player support skills must target monsters.");

            if (caster is Monster && singleEnemy is not (Player or Monster _))
                throw new InvalidTargetException("Monster support skills must target players or monsters.");
        }
        
        if (caster is Monster && TargetType == TargetType.SingleEnemy && singleEnemy is Player && SupportEffect != SupportEffectType.Reduce)
            throw new InvalidSkillConfigurationException("Monster support skills must reduce player stats");
        
        if (caster is Monster && TargetType == TargetType.SingleEnemy && singleEnemy is Monster && SupportEffect != SupportEffectType.Boost)
            throw new InvalidSkillConfigurationException("Monster support skills must boost another monster's stats");
        
        if (caster is Monster && TargetType == TargetType.AllEnemies && SupportEffect != SupportEffectType.Boost)
            throw new InvalidSkillConfigurationException("Monster multi-target support skills must boost other monsters' stats.");
        
        if (TargetType == TargetType.AllEnemies &&
            multipleEnemies!.Any(me => me is not Monster _))
            throw new InvalidSkillConfigurationException(
                "Support skills targeting multiple enemies must target only monsters.");
    }
}