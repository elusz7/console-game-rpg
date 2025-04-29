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
    public int Level { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public int Health { get; set; }
    public int ArchetypeId { get; set; }
    public virtual Archetype Archetype { get; set; }
    public virtual Inventory Inventory { get; set; }
    public int? RoomId { get; set; }
    public virtual Room? CurrentRoom { get; set; }
    
    public void Attack(ITargetable target) { }
    public void TakeDamage(int damage) { }
    public void LevelUp()
    {
        Level++;
        Health += Archetype.LevelUp(Level);
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
