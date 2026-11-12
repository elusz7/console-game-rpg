using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public class ActiveEffectManager
{
    private long _nextEffectId = 1;
    private readonly List<ActiveEffect> _activeEffects = [];

    public IReadOnlyList<ActiveEffect> ActiveEffects =>
        _activeEffects.AsReadOnly();

    public ActiveEffectRegisterResult Register(ActiveEffect effect)
    {
        ValidateEffect(effect);

        var existingEffect = GetByIdentityKey(effect.EffectIdentityKey);

        if (existingEffect != null)
        {
            if (ReferenceEquals(existingEffect, effect))
                return new ActiveEffectRegisterResult(existingEffect, RegistrationStatus.Retained);

            var status = UpdateEffect(existingEffect, effect);
            return new ActiveEffectRegisterResult(existingEffect, status);
        }

        var newStatus = AddNewEffect(effect);
        return new ActiveEffectRegisterResult(effect, newStatus);
    }

    public ActiveEffect? GetById(long effectId)
    {
        return ActiveEffects.FirstOrDefault(ae => ae.EffectId == effectId);
    }

    public ActiveEffect? GetByIdentityKey(ActiveEffectIdentityKey identityKey)
    {
        ArgumentNullException.ThrowIfNull(identityKey);
        
        return ActiveEffects.FirstOrDefault(ae => ae.EffectIdentityKey == identityKey);
    }

    public List<ActiveEffect> GetByType(ActiveEffectType type)
    {
        return [.. ActiveEffects.Where(ae => ae.EffectType == type)];
    }

    public List<ActiveEffect> GetBySourceSkill(ISkill source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return [.. _activeEffects.Where(
            ae => ReferenceEquals(ae.SourceSkill, source))];
    }

    public ActiveEffectRemovalResult RemoveById(long effectId)
    {
        var effect = GetById(effectId);

        if (effect == null)
            return new ActiveEffectRemovalResult([]);

        _activeEffects.Remove(effect);

        return new ActiveEffectRemovalResult([effect]);
    }

    public ActiveEffectRemovalResult RemoveByIdentityKey(
        ActiveEffectIdentityKey identityKey)
    {
        var effect = GetByIdentityKey(identityKey);

        if (effect == null)
            return new ActiveEffectRemovalResult([]);

        _activeEffects.Remove(effect);

        return new ActiveEffectRemovalResult([effect]);
    }
    
    private static void ValidateEffect(ActiveEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);

        if (effect.Duration <= 0)
            throw new ArgumentOutOfRangeException(nameof(effect));

        if (effect.EffectIdentityKey is SupportSkillEffectKey supportKey &&
            !ReferenceEquals(supportKey.SourceSkill, effect.SourceSkill))
        {
            throw new ArgumentException(
                "The support skill identity must match the effect source skill.",
                nameof(effect));
        }
    }

    private RegistrationStatus UpdateEffect(
        ActiveEffect existingEffect,
        ActiveEffect newEffect)
    {
        switch (existingEffect.EffectType)
        {
            case ActiveEffectType.ElementalStatus:
                return UpdateElementalStatus(existingEffect, newEffect);

            case ActiveEffectType.ElementalDamageOverTime:
                return UpdateElementalDamageOverTime(existingEffect, newEffect);

            case ActiveEffectType.SupportStatModifier:
                return RegistrationStatus.RejectedDuplicate;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(existingEffect), "Effect Type is Invalid.");
        }
    }

    private RegistrationStatus AddNewEffect(ActiveEffect newEffect)
    {
        if (newEffect.EffectId != 0)
            throw new InvalidOperationException(
                "An effect that has already been registered cannot be registered again.");
        
        var nextId = checked(_nextEffectId + 1);

        newEffect.AssignEffectId(_nextEffectId);
        _activeEffects.Add(newEffect);

        _nextEffectId = nextId;

        return RegistrationStatus.Registered;
    }

    private RegistrationStatus UpdateElementalStatus(ActiveEffect existingEffect, ActiveEffect newEffect)
    {
        /*
         *  Category	        Stronger	                Equal	            Weaker
            Elemental status	Refresh Power and Duration	Refresh Duration	Retain existing
         */
        if (newEffect.Power > existingEffect.Power)
        {
            existingEffect.Refresh(newEffect.Power, newEffect.Duration);
            return RegistrationStatus.Strengthened;
        }

        if (newEffect.Power == existingEffect.Power)
        {
            existingEffect.Refresh(existingEffect.Power, newEffect.Duration);
            return RegistrationStatus.Refreshed;
        }

        return RegistrationStatus.Retained;
    }
    
    private RegistrationStatus UpdateElementalDamageOverTime(ActiveEffect existingEffect, ActiveEffect newEffect)
    {
        /*
         *  Category	    Stronger	                Equal	            Weaker
            Elemental DOT	Refresh Power and Duration	Refresh Duration	Rejected
         */
        if (newEffect.Power > existingEffect.Power)
        {
            existingEffect.Refresh(newEffect.Power, newEffect.Duration);
            return RegistrationStatus.Strengthened;
        }

        if (newEffect.Power == existingEffect.Power)
        {
            existingEffect.Refresh(existingEffect.Power, newEffect.Duration);
            return RegistrationStatus.Refreshed;
        }

        return RegistrationStatus.RejectedWeaker;
    }

    public ActiveEffectRemovalResult ClearBySource(ITargetable sourceCaster)
    {
        ArgumentNullException.ThrowIfNull(sourceCaster);

        var effectsToRemove = _activeEffects.Where(ae => ReferenceEquals(ae.SourceCaster, sourceCaster)).ToList();

        foreach (var effect in effectsToRemove)
            _activeEffects.Remove(effect);
        
        return new ActiveEffectRemovalResult(effectsToRemove);
    }

    public ActiveEffectRemovalResult ClearBySource(ISkill sourceSkill)
    {
        ArgumentNullException.ThrowIfNull(sourceSkill);

        var effectsToRemove = _activeEffects.Where(ae => ReferenceEquals(ae.SourceSkill, sourceSkill)).ToList();

        foreach (var effect in effectsToRemove)
            _activeEffects.Remove(effect);
        
        return new ActiveEffectRemovalResult(effectsToRemove);
    }

    public ActiveEffectRemovalResult ClearBySource(IRune sourceInstance)
    {
        ArgumentNullException.ThrowIfNull(sourceInstance);

        var effectsToRemove = _activeEffects.Where(ae => ReferenceEquals(ae.SourceRune, sourceInstance)).ToList();

        foreach (var effect in effectsToRemove)
            _activeEffects.Remove(effect);
        
        return new ActiveEffectRemovalResult(effectsToRemove);
    }

    public ActiveEffectRemovalResult ClearAll()
    {
        var effectsToRemove = _activeEffects.ToList();

        _activeEffects.Clear();

        return new ActiveEffectRemovalResult(effectsToRemove);
    }
}