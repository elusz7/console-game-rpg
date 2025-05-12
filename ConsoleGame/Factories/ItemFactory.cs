using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class ItemFactory(ItemDao itemDao)
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
    private readonly ItemDao _itemDao = itemDao;

    public (List<Item>, int) GenerateLoot(int level, int numMonsters, bool campaign)
    {
        var lootMaxValue = CalculateLootValue(level, numMonsters, campaign);
        var availableItems = _itemDao.GetItemsByMaxLevel(level).ToList();

        var loot = GatherTribute(lootMaxValue, availableItems, numMonsters);

        if (loot.Sum(i => i.Value) < lootMaxValue * 0.75M && loot.Count == numMonsters)
        {
            TryRemoveLowestValue(loot, [.. availableItems.OfType<Valuable>()], lootMaxValue);
        }

        var numberOfCursedItems = ALittlePoisonNeverHurtNoOne(loot.Count, campaign);

        return (loot, numberOfCursedItems);
    }
    public static Item CreateItem(Item itemBase)
    {
        return itemBase switch
        {
            Weapon weapon => new Weapon
            {
                Name = weapon.Name,
                Value = weapon.Value,
                Description = weapon.Description,
                Durability = weapon.Durability,
                Weight = weapon.Weight,
                RequiredLevel = weapon.RequiredLevel,
                ItemType = weapon.ItemType,
                AttackPower = weapon.AttackPower,
                DamageType = weapon.DamageType
            },
            Armor armor => new Armor
            {
                Name = armor.Name,
                Value = armor.Value,
                Description = armor.Description,
                Durability = armor.Durability,
                Weight = armor.Weight,
                RequiredLevel = armor.RequiredLevel,
                ItemType = armor.ItemType,
                DefensePower = armor.DefensePower,
                Resistance = armor.Resistance,
                ArmorType = armor.ArmorType
            },
            Consumable consumable => new Consumable
            {
                Name = consumable.Name,
                Value = consumable.Value,
                Description = consumable.Description,
                Durability = consumable.Durability,
                Weight = consumable.Weight,
                RequiredLevel = consumable.RequiredLevel,
                ItemType = consumable.ItemType,
                Power = consumable.Power,
                ConsumableType = consumable.ConsumableType
            },
            _ => new Item
            {
                Name = itemBase.Name,
                Value = itemBase.Value,
                Description = itemBase.Description,
                Durability = itemBase.Durability,
                Weight = itemBase.Weight,
                RequiredLevel = itemBase.RequiredLevel,
                ItemType = itemBase.ItemType
            },
        };
    }
    private static decimal CalculateLootValue(int level, int numMonsters, bool campaign)
    {
        decimal baseValue;

        if (level < 5)
            baseValue = 2M * level + (numMonsters * 0.1M);
        else if (level < 10)
            baseValue = 2.5M * level + (numMonsters * 0.2M);
        else
            baseValue = 3M * level + (numMonsters * 0.3M);

        // Add variance
        decimal variance = (decimal)_rng.NextDouble() * 0.2M + 0.9M; // 90% to 110%
        baseValue *= variance;

        // Campaign bonus
        if (campaign)
            baseValue *= 1.2M;

        return Math.Max(baseValue, numMonsters); // Avoid zero or near-zero
    }
    public static void TryRemoveLowestValue(List<Item> loot, List<Valuable> valuables, decimal lootMaxValue)
    {
        var itemToRemove = loot.OfType<Valuable>().OrderBy(i => i.Value).FirstOrDefault();

        if (itemToRemove != null)
        {
            loot.Remove(itemToRemove);
            var newValue = loot.Sum(i => i.Value);

            //Try to find a random valuable in the ideal range
            var jackpot = valuables
                .Where(v => v.Value > newValue && v.Value <= (lootMaxValue - newValue))
                .OrderBy(_ => _rng.Next())
                .FirstOrDefault();

            // if no item found in ideal range, find the best one
            jackpot ??= valuables
                    .Where(v => v.Value <= (lootMaxValue - newValue))
                    .OrderByDescending(v => v.Value)
                    .FirstOrDefault();

            if (jackpot != null)
            {
                loot.Add(jackpot); // Add the jackpot item
            }
            else
            {
                loot.Add(itemToRemove); // No better item found — restore the original
            }
        }
    }
    public static List<Item> GatherTribute(decimal lootMaxValue, List<Item> availableItems, int numMonsters)
    {
        var loot = new List<Item>();
        
        decimal currentValue = 0.0M;

        var valuables = availableItems.OfType<Valuable>().ToList();
        var weapons = availableItems.OfType<Weapon>().ToList();
        var armors = availableItems.OfType<Armor>().ToList();
        var consumables = availableItems.OfType<Consumable>().ToList();

        while (currentValue < lootMaxValue && loot.Count < numMonsters)
        {
            double roll = _rng.NextDouble();

            Item itemBase;
            if (roll < 0.65 && valuables.Count > 0) // 65%
            {
                itemBase = valuables[_rng.Next(valuables.Count)];
            }
            else if (roll < 0.75 && weapons.Count > 0) // 15%
            {
                itemBase = weapons[_rng.Next(weapons.Count)];
            }
            else if (roll < 0.90 && armors.Count > 0) // 15%
            {
                itemBase = armors[_rng.Next(armors.Count)];
            }
            else if (consumables.Count > 0) // 5%
            {
                itemBase = consumables[_rng.Next(consumables.Count)];
            }
            else
            {
                // fallback if one of the lists is empty
                itemBase = availableItems[_rng.Next(availableItems.Count)];
            }

            var item = CreateItem(itemBase);

            var itemTrueValue = CalculateItemTrueValue(item);

            loot.Add(item);
            currentValue += itemTrueValue;
        }
        return loot;
    }
    public static int ALittlePoisonNeverHurtNoOne(int lootCount, bool campaign)
    {
        double curseChance = campaign ? 0.05 : 0.01;

        return Math.Max(1, (int)Math.Floor(lootCount * curseChance));
    }
    private static decimal CalculateItemTrueValue(Item item)
    {
        decimal value = item.Value;
        if (item is Weapon w)
            value += (w.AttackPower * 0.5M) + (w.Durability * 0.2M) + (w.RequiredLevel * 0.5M);
        else if (item is Armor a)
            value += (a.DefensePower * 0.3M) + (a.Resistance * 0.3M) + (a.Durability * 0.2M) + (a.RequiredLevel * 0.5M);
        else if (item is Consumable c)
            value += (c.Power * 0.5M) + (c.Durability * 0.2M) + (c.RequiredLevel * 0.5M);
        return value;
    }

    public List<Item> CreateMerchantItems(int level)
    {
        var merchantItems = new List<Item>();
        var availableItems = _itemDao.GetItemsByMaxLevel(level).ToList();

        var consumables = availableItems.OfType<Consumable>().ToList();
        var weapons = availableItems.OfType<Weapon>().ToList();
        var armors = availableItems.OfType<Armor>().ToList();

        if (consumables.Count > 0)
        {
            var consumable = consumables[_rng.Next(consumables.Count)];
            merchantItems.Add(CreateItem(consumable));
        }

        int weaponsToAdd = Math.Min(4, weapons.Count);
        for (int i = 0; i < weaponsToAdd; i++)
        {
            var weapon = weapons[_rng.Next(weapons.Count)];
            merchantItems.Add(CreateItem(weapon));
            weapons.Remove(weapon);
        }

        int armorsToAdd = Math.Min(4, armors.Count);
        for (int i = 0; i < armorsToAdd; i++)
        {
            var armor = armors[_rng.Next(armors.Count)];
            merchantItems.Add(CreateItem(armor));
            armors.Remove(armor);
        }

        return merchantItems;
    }
}
