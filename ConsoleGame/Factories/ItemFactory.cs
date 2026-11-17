using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class ItemFactory(IItemDao itemDao, IRandomProvider randomProvider) : IItemFactory
{
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly IItemDao _itemDao = itemDao;

    public List<Item> GenerateLoot(int level, int numMonsters, bool campaign)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(level);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numMonsters);
        
        var lootMaxValue = CalculateLootValue(level, numMonsters, campaign);

        var availableItems = _itemDao.GetItemsAvailableAtLevel(level)
            .Select(CreateItem)
            .Where(i => CalculateItemTrueValue(i) < lootMaxValue / 2.0M)
            .ToList();

        if (availableItems.Count == 0)
            return [];
        
        var loot = GatherTribute(lootMaxValue, availableItems, numMonsters);

        if (loot.Sum(CalculateItemTrueValue) < lootMaxValue * 0.75M && loot.Count == numMonsters)
        {
            TryReplaceLowestValueValuableWithJackpot(loot, [.. availableItems.OfType<Valuable>()], lootMaxValue);
        }

        ApplyRandomCurses(loot, campaign);
        
        //LootDiagnosticLoop();

        return loot;
    }

    public void LootDiagnosticLoop()
    {
        for (int i = 1; i < 21; i++)
        {
            var testNumMonsters = i * 7;
            var targetValue = CalculateLootValue(i, testNumMonsters, false);

            var testItems = _itemDao
                .GetItemsAvailableAtLevel(i)
                .Where(item => CalculateItemTrueValue(CreateItem(item)) < targetValue / 2.0M)
                .ToList();

            var tempLoot = GatherTribute(
                targetValue,
                testItems,
                testNumMonsters);

            if (tempLoot.Sum(CalculateItemTrueValue) < targetValue * 0.75M &&
                tempLoot.Count == testNumMonsters)
            {
                TryReplaceLowestValueValuableWithJackpot(
                    tempLoot,
                    [.. testItems.OfType<Valuable>()],
                    targetValue);
            }

            ApplyRandomCurses(tempLoot, false);

            var actualValue = tempLoot.Sum(item => item.Value);
            var actualTrueValue = tempLoot.Sum(CalculateItemTrueValue);

            var valuables = tempLoot.OfType<Valuable>().Count();
            var weapons = tempLoot.OfType<Weapon>().Count();
            var armor = tempLoot.OfType<Armor>().Count();
            var consumables = tempLoot.OfType<Consumable>().Count();
            var cursed = tempLoot.OfType<ICursable>().Count(item => item.IsCursed());

            System.Diagnostics.Debug.WriteLine(
                $"Level {i} ({testNumMonsters} monsters) | " +
                $"Target: {targetValue:F2} | " +
                $"Actual: {actualValue:F2} | " +
                $"True Value: {actualTrueValue:F2} | " +
                $"Loot: {tempLoot.Count} " +
                $"(Valuables: {valuables}, Weapons: {weapons}, " +
                $"Armor: {armor}, Consumables: {consumables}) | " +
                $"Cursed: {cursed}");
        }
    }

    private Item CreateItem(Item itemBase)
    {
        Item newItem = itemBase switch
        {
            Weapon weapon => SetBaseProperties(new Weapon(_randomProvider)
            {
                DamageType = weapon.DamageType
            }, weapon),

            Armor armor => SetBaseProperties(new Armor(_randomProvider)
            {
                ArmorType = armor.ArmorType
            }, armor),

            Consumable consumable => SetBaseProperties(new Consumable(_randomProvider)
            {
                ConsumableType = consumable.ConsumableType
            }, consumable),

            Valuable valuable => SetBaseProperties(new Valuable(_randomProvider), valuable),

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

    private decimal CalculateLootValue(int level, int numMonsters, bool campaign)
    {
        decimal baseGold = 0.75M * level * level + 14.25M;

        decimal monsterBonus = numMonsters * (3M + (level * 0.3M)); // More monsters, more gold

        decimal total = baseGold + monsterBonus;

        // Variance
        decimal variance = (decimal)_randomProvider.NextDouble() * 0.2M + 0.9M;
        total *= variance;

        // Campaign bonus
        if (campaign)
            total *= 1.2M;

        return Math.Max(total, numMonsters);
    }

    private void TryReplaceLowestValueValuableWithJackpot(List<Item> loot, List<Valuable> valuables, decimal lootMaxValue)
    {
        var itemToRemove = loot.OfType<Valuable>().OrderBy(i => i.Value).FirstOrDefault();

        if (itemToRemove != null)
        {
            loot.Remove(itemToRemove);
            var newValue = loot.Sum(CalculateItemTrueValue);

            //Try to find a random valuable in the ideal range
            var jackpot = valuables
                .Where(v => CalculateItemTrueValue(v) > CalculateItemTrueValue(itemToRemove) &&
                            CalculateItemTrueValue(v) <= lootMaxValue - newValue)
                .OrderBy(_ => _randomProvider.Next())
                .FirstOrDefault();

            jackpot ??= valuables
                .Where(v => CalculateItemTrueValue(v) > CalculateItemTrueValue(itemToRemove) &&
                            CalculateItemTrueValue(v) <= lootMaxValue - newValue)
                .OrderByDescending(CalculateItemTrueValue)
                .FirstOrDefault();

            loot.Add(jackpot ?? itemToRemove); // If no better item found — restore the original
        }
    }

    private List<Item> GatherTribute(decimal lootMaxValue, List<Item> availableItems, int numMonsters)
    {
        if (availableItems.Count == 0)
            return [];
        
        var loot = new List<Item>();

        decimal currentValue = 0.0M;

        var valuables = availableItems.OfType<Valuable>().ToList();
        var weapons = availableItems.OfType<Weapon>().ToList();
        var armors = availableItems.OfType<Armor>().ToList();
        var consumables = availableItems.OfType<Consumable>().ToList();

        while (currentValue < lootMaxValue && loot.Count < numMonsters)
        {
            var itemBase = SelectLootItem(valuables, weapons, armors, consumables);

            var item = CreateItem(itemBase);

            var itemTrueValue = CalculateItemTrueValue(item);

            loot.Add(item);
            currentValue += itemTrueValue;
        }

        return loot;
    }

    private Item SelectLootItem(List<Valuable> valuables, List<Weapon> weapons, List<Armor> armors, List<Consumable> consumables)
    {
        const int valuableWeight = 65;
        const int weaponWeight = 15;
        const int armorWeight = 15;
        const int consumableWeight = 5;

        int totalWeight =
            (valuables.Count > 0 ? valuableWeight : 0) +
            (weapons.Count > 0 ? weaponWeight : 0) +
            (armors.Count > 0 ? armorWeight : 0) +
            (consumables.Count > 0 ? consumableWeight : 0);

        int roll = _randomProvider.Next(totalWeight);

        if (valuables.Count > 0)
        {
            if (roll < valuableWeight)
                return valuables[_randomProvider.Next(valuables.Count)];

            roll -= valuableWeight;
        }

        if (weapons.Count > 0)
        {
            if (roll < weaponWeight)
                return weapons[_randomProvider.Next(weapons.Count)];

            roll -= weaponWeight;
        }

        if (armors.Count > 0 && roll < armorWeight)
        {
            return armors[_randomProvider.Next(armors.Count)];
        }

        if (consumables.Count > 0)
            return consumables[_randomProvider.Next(consumables.Count)];

        throw new InvalidOperationException("No loot categories available");
    }
    
    private void ApplyRandomCurses(List<Item> loot, bool campaign)
    {
        const double normalCurseChance = 0.02;
        const double campaignCurseChance = 0.05;
        const int maxCursedItems = 2;

        double curseChance = campaign
            ? campaignCurseChance
            : normalCurseChance;

        var eligibleItems = loot.OfType<ICursable>().ToList();

        // Randomize which equipment gets considered first.
        for (int i = eligibleItems.Count - 1; i > 0; i--)
        {
            int j = _randomProvider.Next(i + 1);
            (eligibleItems[i], eligibleItems[j]) =
                (eligibleItems[j], eligibleItems[i]);
        }

        int cursedCount = 0;

        foreach (var item in eligibleItems)
        {
            if (cursedCount >= maxCursedItems)
                break;

            if (_randomProvider.NextDouble() < curseChance)
            {
                item.Curse();
                cursedCount++;
            }
        }
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(level);
        
        var merchantItems = new List<Item>();
        var availableItems = _itemDao.GetItemsAvailableAtLevel(level).ToList();

        var consumables = availableItems.OfType<Consumable>().ToList();
        var weapons = availableItems.OfType<Weapon>().ToList();
        var armors = availableItems.OfType<Armor>().ToList();

        if (consumables.Count > 0)
        {
            var consumable = consumables[_randomProvider.Next(consumables.Count)];
            merchantItems.Add(CreateItem(consumable));
        }

        var selectedWeapons = new List<Weapon>();

        // Add one Martial weapon if available
        var martial = weapons
            .Where(w => w.DamageType == DamageType.Martial)
            .ToList();

        if (martial.Count > 0)
        {
            var weapon = martial[_randomProvider.Next(martial.Count)];
            selectedWeapons.Add(weapon);
            weapons.Remove(weapon);
        }

        // Add one Magical weapon if available
        var magical = weapons
            .Where(w => w.DamageType == DamageType.Magical)
            .ToList();

        if (magical.Count > 0 && selectedWeapons.Count < 3)
        {
            var weapon = magical[_randomProvider.Next(magical.Count)];
            selectedWeapons.Add(weapon);
            weapons.Remove(weapon);
        }

        // Fill remaining slots randomly
        while (selectedWeapons.Count < 3 && weapons.Count > 0)
        {
            var weapon = weapons[_randomProvider.Next(weapons.Count)];
            selectedWeapons.Add(weapon);
            weapons.Remove(weapon);
        }

        merchantItems.AddRange(selectedWeapons.Select(CreateItem));

        int armorsToAdd = Math.Min(3, armors.Count);
        for (int i = 0; i < armorsToAdd; i++)
        {
            var armor = armors[_randomProvider.Next(armors.Count)];
            merchantItems.Add(CreateItem(armor));
            armors.Remove(armor);
        }

        return merchantItems;
    }

    public List<Item> GetMerchantGifts(ArchetypeType type, int level)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(level);

        var weaponType = type switch
        {
            ArchetypeType.Martial => DamageType.Martial,
            ArchetypeType.Magical => DamageType.Magical,
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Archtetype type must be Martial or Magical")
        };
        
        var availableItems = _itemDao.GetItemsAvailableAtLevel(level).ToList();

        var weapons = availableItems
            .OfType<Weapon>()
            .Where(w => w.DamageType == weaponType)
            .ToList();

        var armors = availableItems.OfType<Armor>().ToList();
        var consumables = availableItems.OfType<Consumable>().ToList();

        var gifts = new List<Item>();

        if (weapons.Count > 0)
            gifts.Add(CreateItem(weapons[_randomProvider.Next(weapons.Count)]));

        if (armors.Count > 0)
            gifts.Add(CreateItem(armors[_randomProvider.Next(armors.Count)]));

        if (consumables.Count > 0)
            gifts.Add(CreateItem(consumables[_randomProvider.Next(consumables.Count)]));

        return gifts;
    }
}