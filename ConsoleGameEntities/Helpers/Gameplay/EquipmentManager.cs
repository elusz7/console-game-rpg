using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Helpers.Gameplay;

public class EquipmentManager
{
    private readonly List<IEquippable> _equippedItems = new();

    public bool Equip(Player player, Item item)
    {
        if (item is not IEquippable equippable)
            throw new EquipmentException($"Cannot equip {item.Name}");

        if (item.Inventory?.Player != player)
            throw new ItemNotFoundException("This item is not in your inventory.");

        if (item.RequiredLevel > player.Level)
            throw new EquipmentException("Your level is too low to equip this item.");

        if (item.Durability < 1)
            throw new EquipmentException("This item is broken and cannot be equipped.");

        // Weapon logic
        if (item is Weapon newWeapon)
        {
            if ((newWeapon.DamageType == DamageType.Martial && player.Archetype.ArchetypeType == ArchetypeType.Magical)
                || (newWeapon.DamageType == DamageType.Magical && player.Archetype.ArchetypeType == ArchetypeType.Martial))
                throw new EquipmentException($"You cannot equip weapons that deal {newWeapon.DamageType} damage.");

            var currentWeapon = _equippedItems.OfType<Weapon>().FirstOrDefault();
            if (currentWeapon != null)
            {
                currentWeapon.Unequip();
                _equippedItems.Remove(currentWeapon);
                player.Logger.Log($"You unequipped {currentWeapon.Name}.");
            }

            newWeapon.Equip();
            _equippedItems.Add(newWeapon);
            player.Logger.Log($"You equipped {newWeapon.Name}.");
        }
        // Armor logic
        else if (item is Armor newArmor)
        {
            var existing = _equippedItems
                .OfType<Armor>()
                .FirstOrDefault(a => a.ArmorType == newArmor.ArmorType);

            if (existing != null)
            {
                existing.Unequip();
                _equippedItems.Remove(existing);
                player.Logger.Log($"You unequipped {existing.Name}.");
            }

            newArmor.Equip();
            _equippedItems.Add(newArmor);
            player.Logger.Log($"You equipped {newArmor.Name}.");
        }

        // Cursed effect
        if (item is ICursable c && c.IsCursed())
            EnactCursedEffect(player, c);

        return true;
    }

    public bool Unequip(Player player, Item item)
    {
        if (item is IEquippable equippable && _equippedItems.Contains(equippable))
        {
            equippable.Unequip();
            _equippedItems.Remove(equippable);
            player.Logger.Log($"You unequipped {item.Name}.");
            return true;
        }

        return false;
    }

    private void EnactCursedEffect(Player player, ICursable cursedItem)
    {
        var deduction = Math.Max(1, (int)Math.Floor(player.MaxHealth * 0.1));
        player.MaxHealth -= deduction;
        player.CurrentHealth -= deduction;

        player.Logger.Log($"You have been cursed! Your max health has been lowered by {deduction}!");
        cursedItem.TargetCursed();
    }

    public IReadOnlyCollection<IEquippable> GetEquippedItems() => _equippedItems.AsReadOnly();
}
