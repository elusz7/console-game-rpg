using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class ItemFactory(IItemDao itemDao) : IItemFactory
{
    private static readonly Random _rng = Random.Shared;
    private readonly IItemDao _itemDao = itemDao;

    public (List<Item>, int) GenerateLoot(int level, int numMonsters, bool campaign)
    {
        var lootMaxValue = CalculateLootValue(level, numMonsters, campaign);

        var availableItems = _itemDao.GetItemsByMaxLevel(level)
            .Where(i => i.Value < (lootMaxValue / 2.0M)).ToList();

        var loot = GatherTribute(lootMaxValue, availableItems, numMonsters);
        foreach (var item in loot)
        {
            var itemTrueValue = CalculateItemTrueValue(item);
        }

        if (loot.Sum(i => i.Value) < lootMaxValue * 0.75M && loot.Count == numMonsters)
        {
            TryRemoveLowestValue(loot, [.. availableItems.OfType<Valuable>()], lootMaxValue);
        }

        var numberOfCursedItems = ALittlePoisonNeverHurtNoOne(loot.Count, campaign, loot);

        return (loot, numberOfCursedItems);
    }
    private static Item CreateItem(Item itemBase)
    {
        Item newItem = itemBase switch
        {
            Weapon weapon => SetBaseProperties(new Weapon
            {
                DamageType = weapon.DamageType
            }, weapon),

            Armor armor => SetBaseProperties(new Armor
            {
                ArmorType = armor.ArmorType
            }, armor),

            Consumable consumable => SetBaseProperties(new Consumable
            {
                ConsumableType = consumable.ConsumableType
            }, consumable),

            Valuable valuable => SetBaseProperties(new Valuable(), valuable),

            _ => throw new ArgumentException("Unknown item type", nameof(itemBase))
        };

        newItem.CalculateStatsByLevel();
        newItem.CalculateValue();

        return newItem;
    }
    private static T SetBaseProperties<T>(T item, Item itemBase) where T : Item
    {
        item.Name = itemBase.Name;
        item.Description = itemBase.Description;
        item.Weight = itemBase.Weight;
        item.RequiredLevel = itemBase.RequiredLevel;
        item.ItemType = itemBase.ItemType;
        return item;
    }
    private static decimal CalculateLootValue(int level, int numMonsters, bool campaign)
    {
        decimal baseGold = 0.75M * level * level + 14.25M; 

        decimal monsterBonus = numMonsters * (3M + (level * 0.3M)); // More monsters, more gold

        decimal total = baseGold + monsterBonus;

        // Variance
        decimal variance = (decimal)_rng.NextDouble() * 0.2M + 0.9M;
        total *= variance;

        // Campaign bonus
        if (campaign)
            total *= 1.2M;

        return Math.Max(total, numMonsters); 
    }
    private static void TryRemoveLowestValue(List<Item> loot, List<Valuable> valuables, decimal lootMaxValue)
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
    private static List<Item> GatherTribute(decimal lootMaxValue, List<Item> availableItems, int numMonsters)
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
    private static int ALittlePoisonNeverHurtNoOne(int lootCount, bool campaign, List<Item> loot)
    {
        var curseChance = campaign ? 0.05 : 0.01;

        var estimatedCurseCount = Math.Max(1, (int)Math.Floor(lootCount * curseChance));
        var eligibleItemCount = loot.Count(i => i is Armor || i is Weapon);

        return Math.Min(estimatedCurseCount, eligibleItemCount);
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

        // Ensure at least one Martial and one Magical weapon
        var martialWeapons = weapons.Where(w => w.DamageType == DamageType.Martial).ToList();
        var magicalWeapons = weapons.Where(w => w.DamageType == DamageType.Magical).ToList();

        int weaponsToAdd = Math.Min(3, weapons.Count);

        // Add one Martial weapon if available
        if (martialWeapons.Count > 0)
        {
            var martial = martialWeapons[_rng.Next(martialWeapons.Count)];
            merchantItems.Add(CreateItem(martial));
            weapons.Remove(martial);
            weaponsToAdd--;
        }

        // Add one Magical weapon if available
        if (weaponsToAdd > 0 && magicalWeapons.Count > 0)
        {
            var magical = magicalWeapons[_rng.Next(magicalWeapons.Count)];
            merchantItems.Add(CreateItem(magical));
            weapons.Remove(magical);
            weaponsToAdd--;
        }

        // Fill the rest with random weapons
        for (int i = 0; i < weaponsToAdd; i++)
        {
            if (weapons.Count == 0) break;

            var weapon = weapons[_rng.Next(weapons.Count)];
            merchantItems.Add(CreateItem(weapon));
            weapons.Remove(weapon);
        }

        int armorsToAdd = Math.Min(3, armors.Count);
        for (int i = 0; i < armorsToAdd; i++)
        {
            var armor = armors[_rng.Next(armors.Count)];
            merchantItems.Add(CreateItem(armor));
            armors.Remove(armor);
        }

        return merchantItems;
    }
    public List<Item> GetMerchantGifts(ArchetypeType type, int level)
    {
        var availableItems = _itemDao.GetItemsByMaxLevel(level).ToList();

        var weapons = availableItems
            .OfType<Weapon>()
            .Where(w => w.DamageType.ToString() == type.ToString())
            .ToList();

        var armors = availableItems.OfType<Armor>().ToList();
        var consumables = availableItems.OfType<Consumable>().ToList();

        var gifts = new List<Item>();

        if (weapons.Count > 0)
            gifts.Add(CreateItem(weapons[_rng.Next(weapons.Count)]));

        if (armors.Count > 0)
            gifts.Add(CreateItem(armors[_rng.Next(armors.Count)]));

        if (consumables.Count > 0)
            gifts.Add(CreateItem(consumables[_rng.Next(consumables.Count)]));

        return gifts;
    }
}
