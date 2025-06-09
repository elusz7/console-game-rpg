namespace ConsoleGameEntities.Interfaces.ItemAttributes;

public interface IEquippable
{
    void Equip();
    void Unequip();
    bool IsEquipped();
}
