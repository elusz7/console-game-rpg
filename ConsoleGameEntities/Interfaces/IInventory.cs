using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGameEntities.Models.Entities;

public interface IInventory
{
    int Id { get; set; }
    int Gold { get; set; }
    decimal Capacity { get; set; }
    Player Player { get; set; }
    List<Item> Items { get; set; }
    public Dictionary<Ingredient, int> Ingredients { get; set; }
    Dictionary<Rune, int> Runes { get; set; }
    public void Buy(Item item);
    public void Sell(Item item);
    void AddItem(Item item);
    void RemoveItem(Item item);

    void AddIngredient(Ingredient ingredient);
    void RemoveIngredient(Ingredient ingredient, int quantity);
    void AddRune(Rune rune);
    void RemoveRune(Rune rune, int quantity);
    void DestoyRune(Rune rune);
    List<(Rune rune, int quantity)> GetFuseableRunes();
    List<Rune> GetDestroyableRunes();
    public bool ContainsConsumables();
    public decimal GetCarryingWeight();
}