using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces;

public interface IRune
{
    int Id { get; set; }
    string Name { get; set; }
    int Power { get; set; }
    int Tier { get; set; }
    ElementType Element { get; set; }
    RarityLevel Rarity { get; set; }

    Dictionary<Ingredient, int> DestroyRune();
}
