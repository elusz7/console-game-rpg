using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces.ItemAttributes;

public interface IEquippable
{
    void Equip();
    void Unequip();
    bool IsEquipped();
    
    EquippedRune? EquippedRune { get; set; }
    int UseEquippedRune();
}