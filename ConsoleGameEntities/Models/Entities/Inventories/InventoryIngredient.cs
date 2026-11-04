using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGameEntities.Models.Entities.Inventories;

public class InventoryIngredient
{
    public int InventoryId { get; set; }
    public virtual Inventory Inventory { get; set; } = null!;

    public int IngredientId { get; set; }
    public virtual Ingredient Ingredient { get; set; } = null!;

    public int Quantity { get; set; }
}