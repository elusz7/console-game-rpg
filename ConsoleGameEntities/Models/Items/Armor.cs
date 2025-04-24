using System.Text;

namespace ConsoleGameEntities.Models.Items;

public class Armor : Item
{
    public int DefensePower { get; set; }

    public Armor() { }  
    public Armor(string name, decimal value, string description, int durability, decimal weight, int defensePower)
    {
        Name = name;
        Value = value;
        Description = description;
        Durability = durability;
        Weight = weight;
        DefensePower = defensePower;
        ItemType = "Armor";
    }

    public override int Use()
    {
        // Implement the Use logic for Armor  
        return 0;
    }

    public override string ToString()
    {
        var sb = new StringBuilder(base.ToString());

        sb.Insert(0, "[Armor] ");

        sb.Append($", Defense Power: {DefensePower}");

        if (Inventory != null)
        {
            sb.Append($", Held by: {Inventory.Player.Name}");
        }

        return sb.ToString();
    }
}
