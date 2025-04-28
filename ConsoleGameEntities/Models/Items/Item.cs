using System.Text;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameEntities.Models.Items;

public abstract class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public string Description { get; set; }
    public int Durability { get; set; }
    public decimal Weight { get; set; }

    public string ItemType { get; set; }

    public int? InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public abstract int Use();

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"{Name}: ");
        sb.Append($"{Description}");
        sb.Append($"\n\tValue: {Value}, ");
        sb.Append($"Weight: {Weight} , ");
        sb.Append($"Durability: {Durability}");
        
        return sb.ToString();
    }
}
