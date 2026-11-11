using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Combat;

public record PlayerDamageModifiers(
    int Defense,
    int Resistance,
    int ArmorResilience);