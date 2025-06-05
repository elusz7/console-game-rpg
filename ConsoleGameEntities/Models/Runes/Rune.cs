using ConsoleGameEntities.Interfaces;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Runes;

public abstract class Rune : IRune
{
    public int Id { get; set; }
    public string RuneType { get; set; }
    public string Name { get; set; }
    public int Power { get; set; }
    public int Tier { get; set; }
    public ElementType Element { get; set; }
    public RarityLevel Rarity { get; set; }
}
