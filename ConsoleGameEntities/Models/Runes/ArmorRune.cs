namespace ConsoleGameEntities.Models.Runes;

public class ArmorRune : Rune
{
    public int Use()
    {
        return Power * Tier;
    }
}
