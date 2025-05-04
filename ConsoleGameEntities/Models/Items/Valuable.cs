using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameEntities.Models.Items;

public class Valuable : Item
{
    public override int Use()
    {
        throw new NotImplementedException(); //A valuable cannot be used
    }

    public override string ToString()
    {
        return Description;
    }
}
