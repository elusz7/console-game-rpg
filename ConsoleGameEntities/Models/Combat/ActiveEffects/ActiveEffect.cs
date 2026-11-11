using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public class ActiveEffect
{
    public long EffectId { get; private set; }

    public ActiveEffectType EffectType { get; }

    public ActiveEffectIdentityKey EffectIdentityKey { get; }

    public int Power { get; private set; }

    public int Duration { get; private set; }

    public int RemainingDuration { get; private set; }

    public ITargetable? SourceCaster { get; }

    public ISkill? SourceSkill { get; }

    public IRune? SourceInstance { get; }

    public ActiveEffect(
        ActiveEffectType effectType,
        ActiveEffectIdentityKey effectIdentityKey,
        int power,
        int duration,
        ITargetable? sourceCaster = null,
        ISkill? sourceSkill = null,
        IRune? sourceInstance = null)
    {
        ArgumentNullException.ThrowIfNull(effectIdentityKey);
        
        if (power < 0)
            throw new ArgumentOutOfRangeException(nameof(power));

        if (duration < 0)
            throw new ArgumentOutOfRangeException(nameof(duration));

        ValidateIdentity(effectType, effectIdentityKey);

        EffectType = effectType;
        EffectIdentityKey = effectIdentityKey;
        Power = power;
        Duration = duration;
        RemainingDuration = duration;

        SourceCaster = sourceCaster;
        SourceSkill = sourceSkill;
        SourceInstance = sourceInstance;
    }

    internal void AssignEffectId(long effectId)
    {
        if (effectId <= 0)
            throw new ArgumentOutOfRangeException(nameof(effectId));

        if (EffectId != 0)
            throw new InvalidOperationException(
                "An effect ID cannot be reassigned.");

        EffectId = effectId;
    }

    internal void Refresh(int power, int duration)
    {
        if (power < 0)
            throw new ArgumentOutOfRangeException(nameof(power));

        if (duration <= 0)
            throw new ArgumentOutOfRangeException(nameof(duration));

        Power = power;
        Duration = duration;
        RemainingDuration = duration;
    }

    internal void DecrementRemainingDuration()
    {
        if (RemainingDuration <= 0)
            throw new InvalidOperationException(
                "An expired effect cannot have its duration decremented.");

        RemainingDuration--;
    }

    private static void ValidateIdentity(
        ActiveEffectType effectType,
        ActiveEffectIdentityKey identityKey)
    {
        var isValid = (effectType, identityKey) switch
        {
            (ActiveEffectType.SupportStatModifier,
                SupportSkillEffectKey) => true,

            (ActiveEffectType.ElementalStatus,
                ElementalStatusEffectKey) => true,

            (ActiveEffectType.ElementalDamageOverTime,
                ElementalDamageOverTimeEffectKey) => true,

            _ => false
        };

        if (!isValid)
            throw new ArgumentException(
                "The identity key does not match the effect type.",
                nameof(identityKey));
    }
}