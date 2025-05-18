using ConsoleGameEntities.Main.Models.Monsters;

namespace ConsoleGameEntities.Main.Models.Items;

public class Valuable : Item
{
    public override void Use()
    {
        throw new InvalidOperationException(); //A valuable cannot be used
    }

    public override decimal GetSellPrice() => Value; // values sell for full price

    public override string ToString()
    {
        return Description;
    }
}
