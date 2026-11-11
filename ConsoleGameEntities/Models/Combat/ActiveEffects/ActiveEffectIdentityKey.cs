using ConsoleGameEntities.Interfaces;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public abstract record ActiveEffectIdentityKey;

public sealed record SupportSkillEffectKey : ActiveEffectIdentityKey
{
    public ISkill SourceSkill { get; }

    public SupportSkillEffectKey(ISkill sourceSkill)
    {
        ArgumentNullException.ThrowIfNull(sourceSkill);
        SourceSkill = sourceSkill;
    }

    public bool Equals(SupportSkillEffectKey? other)
    {
        return other is not null &&
               ReferenceEquals(SourceSkill, other.SourceSkill);
    }

    public override int GetHashCode()
    {
        return System.Runtime.CompilerServices.RuntimeHelpers
            .GetHashCode(SourceSkill);
    }
}

public sealed record ElementalStatusEffectKey(
    ElementalStatusEffectType StatusType)
    : ActiveEffectIdentityKey;

public sealed record ElementalDamageOverTimeEffectKey(
    ElementType Element)
    : ActiveEffectIdentityKey;
    
    