using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Runes;

public abstract class Rune : IRune
{
    public int Id { get; set; }
    public string RuneType { get; set; }
    public string Name { get; set; }
    public int Power { get; set; }
    public ElementType Element { get; set; }
    public RarityLevel Rarity { get; set; }
    public virtual Recipe? Recipe { get; set; }
}