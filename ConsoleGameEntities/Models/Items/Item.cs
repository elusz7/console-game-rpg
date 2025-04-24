using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Characters;

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

        sb.Append($"Name: {Name}, ");
        sb.Append($"Description: {Description}, ");
        sb.Append($"Value: {Value}, ");
        sb.Append($"Weight: {Weight} , ");
        sb.Append($"Durability: {Durability}");
        
        return sb.ToString();
    }
}
