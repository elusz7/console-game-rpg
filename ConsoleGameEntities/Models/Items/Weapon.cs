using System.Text;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Items;

public class Weapon : Item
{
    public int AttackPower { get; set; }
    public DamageType DamageType { get; set; }
    
    public Weapon() { }
    public Weapon(string name, decimal value, string description, int durability, decimal weight, int attackPower)
    {
        Name = name;
        Value = value;
        Description = description;
        Durability = durability;
        Weight = weight;
        AttackPower = attackPower;
    }
    
    public override int Use()
    {
        if (Durability > 0)
        {
            Durability--;
            return AttackPower;
        }
        else
        {
            return -1;
        }
    }
    public override string ToString()
    {
        var sb = new StringBuilder(base.ToString());

        sb.Insert(0, "[Weapon] ");

        sb.Append($", Attack Power: {AttackPower}");

        if (Inventory != null)
        {
            sb.Append($", Held by: {Inventory.Player.Name}");
        }

        return sb.ToString();
    }
}
