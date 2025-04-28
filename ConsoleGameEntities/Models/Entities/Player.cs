using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Exceptions;
using System.Text;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces;

namespace ConsoleGameEntities.Models.Entities;

public class Player : ITargetable, IPlayer
{
    public int Experience { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public int Health { get; set; }
    public virtual Inventory Inventory { get; set; }
    public int? RoomId { get; set; }
    public virtual Room? CurrentRoom { get; set; }
    public void Attack(ITargetable target)
    {
        if (CanAttack(target))
        {
            Weapon? weapon = SelectAttackWeapon();

            if (weapon == null)
            {
                Console.WriteLine("No weapon available. Attack failed.");
                return;
            }
            else
            {
                int damage = weapon.Use();

                if (damage !=  -1)
                {
                    OutputDamage(target.Name, weapon.Name, damage);

                    target.TakeDamage(damage);

                    if (weapon.Durability <= 0)
                    {
                        Console.WriteLine($"{weapon.Name} is broken and cannot be used anymore!");
                    }
                    
                    GainExperience(target);
                }
                else
                {
                    ColorDisplay(weapon.Name, ConsoleColor.Blue);
                    Console.WriteLine(" is broken and cannot be used!");
                }
            }
        }
    }
    private static bool CanAttack(ITargetable target)
    {
        if (target is Monster monster)
            if (monster.Health <= 0)
            {
                throw new InvalidAttackTargetException($"Cannot attack {monster.Name}. It is already dead.");
            }
        return true;
    }
    public void TakeDamage(int damage)
    {
        Armor? armor = ReduceDamage();

        if (armor != null)
        {
            int absorbed = armor.Use();
            
            damage -= absorbed;

            Console.WriteLine($"{armor.Name} absorbs {absorbed} damage!");

            if (armor.Durability <= 0)
            {
                Console.WriteLine($"{armor.Name} is broken and cannot be used anymore!");
            }
        }

        if (damage <= 0)
        {
            Console.WriteLine($"{Name} takes no damage!");
        }
        else
        {
            Health -= damage;
            Console.WriteLine($"{Name} takes {damage} damage!");
        }
    }
    private Weapon? SelectAttackWeapon()
    {
        var validWeapons = Inventory.Items
            .Select((item, index) => new { Item = item, Index = index })
            .Where(entry => entry.Item is Weapon weapon && weapon.Durability > 0)
            .ToList();

        if (!validWeapons.Any())
        {
            return null;
        }

        if (validWeapons.Count == 1)
        {
            return (Weapon)validWeapons[0].Item;
        }

        for (int i = 0; i < validWeapons.Count; i++)
        {
            var weapon = validWeapons[i].Item;
            Console.WriteLine($"{i + 1}. {weapon.Name} (Durability: {weapon.Durability})");
        }

        while (true)
        {
            Console.Write("Which weapon do you want to use? (Enter the number) ");
            bool isInt = int.TryParse(Console.ReadLine(), out var weaponIndex);

            if (weaponIndex < 1 || weaponIndex > validWeapons.Count || !isInt)
            {
                Console.WriteLine("Invalid choice. Please try again.");
            }
            else
            {
                return (Weapon)validWeapons[weaponIndex - 1].Item;
            }
        }
    }
    private Armor? ReduceDamage()
    {
        Armor? activeArmor = null;

        foreach (Item item in Inventory.Items)
        {
            if (item is Armor armor && armor.Durability > 0)
            {
                if (activeArmor == null || armor.DefensePower > activeArmor.DefensePower)
                {
                    activeArmor = armor;
                }
            }
        }

        return activeArmor;
    }
    private void OutputDamage(string targetName, string weaponName, int damage)
    {
        ColorDisplay(Name, ConsoleColor.Green);
        Console.Write(" attacks ");

        ColorDisplay(targetName, ConsoleColor.Yellow);
        Console.Write(" with ");

        ColorDisplay(weaponName, ConsoleColor.Blue);
        Console.Write(" for ");

        ColorDisplay(damage.ToString(), ConsoleColor.Red);
        Console.WriteLine(" damage!");
    }
    private void GainExperience(ITargetable target)
    {
        if (target is Monster monster && monster.Health <= 0)
        {
            Experience += monster.AggressionLevel;
            Console.WriteLine($"{Name} gained {monster.AggressionLevel} experience!");
        }
    }
    private static void ColorDisplay(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append("Name: ");
        builder.Append(Name);

        builder.Append(", Health: ");
        builder.Append(Health);

        builder.Append(", Experience: ");
        builder.Append(Experience);

        builder.Append(", Capacity: ");
        builder.Append(Inventory?.Capacity.ToString() ?? "N/A");

        builder.Append(", Gold: ");
        builder.Append(Inventory?.Gold.ToString() ?? "N/A");

        builder.Append("\n\tInventory: ");
        builder.Append(Inventory?.Items != null && Inventory.Items.Any()
            ? string.Join(", ", Inventory.Items.Select(i => i.Name))
            : "Empty");

        return builder.ToString();
    }
}
