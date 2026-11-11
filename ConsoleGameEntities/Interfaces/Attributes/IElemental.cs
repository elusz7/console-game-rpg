
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces.Attributes;

public interface IElemental
{
    ElementType? AttackElement { get; set; } //what element they attack with/ingredient drop
    int? ElementalPower { get; set; } //how much elemental damage they deal
    ElementType? Vulnerability { get; set; } //what element they take more damage from
    ElementType? Resilience { get; set; } //what element they take less damage from
}