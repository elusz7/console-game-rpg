using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameEntities.Models.Items;

public class Valuable : Item
{
    public override void Use()
    {
        throw new InvalidOperationException(); //A valuable cannot be used
    }

    public override string ToString()
    {
        return Description;
    }
}
