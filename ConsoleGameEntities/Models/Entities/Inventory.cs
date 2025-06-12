using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Entities;

public class Inventory : IInventory
{
    public int Id { get; set; }
    public int Gold { get; set; }
    public decimal Capacity { get; set; }
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; }
    public virtual List<Item> Items { get; set; } = new();
    [NotMapped]
    public Dictionary<Rune, int> Runes { get; set; } = new();
    [NotMapped]
    public Dictionary<Ingredient, int> Ingredients { get; set; } = new();

    public virtual void AddItem(Item item)
    {
        if (item is Consumable)
        {
            AddConsumable(item);
            return;
        }

        if (item is Valuable)
        {
            Items.Add(item);
            item.InventoryId = Id;
            item.Inventory = this;
            return;
        }

        decimal wiggleRoom = Capacity - GetCarryingWeight();

        if (Items.Contains(item))
        {
            throw new DuplicateItemException($"You already have {item.Name} in your inventory.");
        }
        else if (item.Weight <= wiggleRoom)
        {
            Items.Add(item);
            item.InventoryId = Id;
            item.Inventory = this;
        }
        else if (item.Weight > wiggleRoom)
        {
            throw new OverweightException($"You cannot add {item.Name} to your inventory. Will exceed carrying capacity.");
        }
        else
        {
            throw new InventoryException($"An error has occured trying to add {item.Name} to your inventory.");
        }
    }
    public virtual void RemoveItem(Item item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
        }
        else
        {
            throw new ItemNotFoundException($"You do not have {item.Name} in your inventory.");
        }
    }
    public virtual void AddIngredient(Ingredient ingredient)
    {
        AddIngredient(ingredient, 1);
    }
    public virtual void AddIngredients(Dictionary<Ingredient, int> ingredients)
    {
        foreach (var kvp in ingredients)
        {
            AddIngredient(kvp.Key, kvp.Value);
        }
    }
    private void AddIngredient(Ingredient ingredient, int quantity)
    {
        if (Ingredients.ContainsKey(ingredient))
        {
            Ingredients[ingredient] += quantity;
        }
        else
        {
            Ingredients.Add(ingredient, quantity);
        }
    }
    public virtual void RemoveIngredient(Ingredient ingredient, int quantity)
    {
        if (Ingredients.ContainsKey(ingredient) && Ingredients[ingredient] >= quantity)
        {
            Ingredients[ingredient] -= quantity;
            if (Ingredients[ingredient] <= 0)
            {
                Ingredients.Remove(ingredient);
            }
        }
        else
        {
            throw new IngredientNotFoundException($"You do not have enough {ingredient.Name} in your inventory.");
        }
    }
    public void AddRune(Rune rune)
    {
        if (Runes.ContainsKey(rune))
        {
            Runes[rune]++;
        }
        else
        {
            Runes.Add(rune, 1);
        }
    }
    public void RemoveRune(Rune rune, int quantity)
    {
        if (Runes.ContainsKey(rune) && Runes[rune] >= quantity)
        {
            Runes[rune] -= quantity;
            if (Runes[rune] <= 0)
            {
                Runes.Remove(rune);
            }
        }
        else
        {
            throw new RuneNotFoundException($"You do not have {rune.Name} in your inventory.");
        }
    }
    public List<(Rune rune, int quantity)> GetFuseableRunes()
    {
        return Runes
            .Where(kvp => kvp.Value >= 4)
            .Where(kvp => kvp.Key.Rarity != RarityLevel.Mythic && kvp.Key.Tier != 3) // Exclude mythic runes already at tier 3
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }
    public List<Rune> GetDestroyableRunes()
    {
        // Step 1: Get equipped runes
        var equipment = Player?.Equipment;
        var equippedRunes = new List<(Rune rune, int qty)>();
        if (equipment != null)
        {
            equippedRunes = equipment
                .Select<IEquippable, Rune?>(e =>
                {
                    if (e is Weapon weapon) return weapon.Rune;
                    if (e is Armor armor) return armor.Rune;
                    return null;
                })
                .Where(r => r != null)
                .Select(r => r!) // assert non-null
                .GroupBy(r => new { r.Element, r.Tier, r.Rarity, r.RuneType })
                .Select(g => (rune: g.First(), qty: g.Count()))
                .ToList();
        }

        // Step 2: If nothing is equipped, all runes can be destroyed
        if (equippedRunes.Count == 0)
        {
            return Runes.Select(kvp => kvp.Key).ToList();
        }

        // Step 3: Filter out runes that are fully equipped (i.e., no spares left)
        var destroyableRunes = Runes
            .Where(kvp =>
            {
                var matchingEquipped = equippedRunes.FirstOrDefault(e =>
                    e.rune.Element == kvp.Key.Element &&
                    e.rune.Rarity == kvp.Key.Rarity &&
                    e.rune.Tier == kvp.Key.Tier &&
                    e.rune.RuneType == kvp.Key.RuneType);

                return matchingEquipped == default || kvp.Value > matchingEquipped.qty;
            })
            .Select(kvp => kvp.Key)
            .ToList();

        return destroyableRunes;
    }
    public List<Ingredient> GetTransmutableEssence()
    {
        return Ingredients.Where(kvp => kvp.Key.ComponentType == ComponentType.Essence && kvp.Value > 5)
            .Where(kvp => kvp.Key.Id != 6) // Exclude mythic essences
            .Select(kvp => kvp.Key)
            .ToList();
    }
    public void TransmuteEssence(Ingredient oldEssence, Ingredient newEssence)
    {
        if (!Ingredients.ContainsKey(oldEssence) || Ingredients[oldEssence] < 6)
            throw new IngredientNotFoundException($"You do not have enough {oldEssence.Name} to transmute.");

        RemoveIngredient(oldEssence, 5); // remove 5 of the essence
        AddIngredient(newEssence, 1); // add 1 of the new essence

    }

    public void DestroyRune(Rune rune)
    {
        if (!Runes.ContainsKey(rune))
            throw new RuneNotFoundException($"You do not have {rune.Name} in your inventory.");

        RemoveRune(rune, 1);

        var ingredientsRecovered = rune.DestroyRune();
        foreach (var (ingredient, quantity) in ingredientsRecovered)
        {
            AddIngredient(ingredient, quantity);
        }
    }

    private void AddConsumable(Item item)
    {
        var cap = Math.Max(3m, Capacity * 0.10m);
        int consumableCap = (int)Math.Floor(cap);


        if (item is Consumable consumable)
        {
            var currentCount = Items.OfType<Consumable>()
                .Count(c => c.ConsumableType == consumable.ConsumableType);

            if (currentCount < consumableCap)
            {
                Items.Add(consumable);
                item.InventoryId = Id;
                item.Inventory = this;
            }
            else
            {
                throw new InventoryException($"You cannot carry more than {consumableCap} {consumable.ConsumableType} consumables.");
            }
        }
    }
    public decimal GetCarryingWeight()
    {
        return Items
            .Where(i => i is Weapon || i is Armor)
            .Sum(i => i.Weight);
    }
    public bool ContainsConsumables()
    {
        return Items
            .OfType<Consumable>()
            .Any();
    }
    public void Sell(Item item)
    {
        if (item is IEquippable e && e.IsEquipped())
            throw new InvalidOperationException("Cannot sell an equipped item.");

        Gold += (int)Math.Round(item.GetSellPrice());
        RemoveItem(item);
    }
    public void Buy(Item item)
    {
        var price = (int)Math.Round(item.GetBuyPrice());

        if (Gold < price)
            throw new ItemPurchaseException($"You are short by {price - Gold} gold.");

        try
        {
            AddItem(item);
        }
        catch (InventoryException ex)
        {
            throw new ItemPurchaseException(ex.Message);
        }

        Gold -= price;
    }
}
