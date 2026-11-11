namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public enum ActiveEffectType
{
    SupportStatModifier,
    ElementalStatus,
    ElementalDamageOverTime
}

public enum RegistrationStatus
{
    Registered,
    Refreshed,
    Strengthened,
    Retained,
    RejectedWeaker,
    RejectedDuplicate
}