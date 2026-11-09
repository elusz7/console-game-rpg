namespace ConsoleGameEntities.Interfaces.Attributes;

public interface ILootable
{
    bool TreasureLooted { get; set; }
    bool IngredientsLooted { get; set; }
}