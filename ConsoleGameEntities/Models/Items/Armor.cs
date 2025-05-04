using System.Text;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Items;

public class Armor : Item
{
    public int DefensePower { get; set; }
    public int Resistance { get; set; }
    public ArmorType ArmorType { get; set; }
    public override int Use()
    {
        return 0;
    }

    public override string ToString()
    {
        var sb = new StringBuilder(base.ToString());

        sb.Insert(0, "[Armor] ");

        sb.Append($", Defense Power: {DefensePower}, Resistance: {Resistance}");

        if (Inventory != null)
        {
            sb.Append($", Held by: {Inventory.Player.Name}");
        }

        return sb.ToString();
    }
}
