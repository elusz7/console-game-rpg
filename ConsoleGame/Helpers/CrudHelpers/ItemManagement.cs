using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers.CrudHelpers;

public class ItemManagement(IInputManager inputManager, IOutputManager outputManager, IItemDao itemDao)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IItemDao _itemDao = itemDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Item Management Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Add Item"
                + "\n2. Edit Item"
                + "\n3. Remove Item"
                + "\n4. Return to Inventory Main Menu");
            
            var input = _inputManager.ReadMenuKey(4);

            switch (input)
            {
                case 1:
                    AddItem();
                    break;
                case 2:
                    EditItem();
                    break;
                case 3:
                    DeleteItem();
                    break;
                case 4:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void AddItem()
    {
        do
        {
            _outputManager.WriteLine();

            ItemType itemType = _inputManager.GetEnumChoice<ItemType>("Select item type to create");

            var choice = _inputManager.ReadInt("\n1. Generate Stats Automatically By Level\n2. Manually Enter Stats\n3. Cancel\n\tChoose an option: ", 3);

            Item item;
            switch (choice)
            {
                case 1:
                    item = CreateItemByLevel(itemType);
                    break;
                case 2:
                    item = CreateItemByStats(itemType);
                    break;
                case 3:
                    _outputManager.WriteLine($"\nItem Creation Cancelled.\n", ConsoleColor.Red);
                    return;
                default:
                    return;
            }

            _itemDao.AddItem(item);

            _outputManager.WriteLine($"\nItem {item.Name} successfully created.\n", ConsoleColor.Green);

        } while (_inputManager.LoopAgain("add"));
        _outputManager.WriteLine();
    }
    private Item CreateItemByLevel(ItemType itemType)
    {
        string name = _inputManager.ReadString("\nEnter item name: ");
        string description = _inputManager.ReadString("Enter item description: ");
        int durability = _inputManager.ReadInt("Enter item durability: ");
        decimal weight = _inputManager.ReadDecimal("Enter item weight: ");
        int level = _inputManager.ReadInt("Enter item required level: ");

        var item = itemType switch
        {
            ItemType.Weapon => new Weapon
            {
                Name = name,
                Description = description,
                Durability = durability,
                Weight = weight,
                RequiredLevel = level,
                DamageType = _inputManager.GetEnumChoice<DamageType>("\nSelect item damage type")
            } as Item,
            ItemType.Armor => new Armor
            {
                Name = name,
                Description = description,
                Durability = durability,
                Weight = weight,
                RequiredLevel = level,
                ArmorType = _inputManager.GetEnumChoice<ArmorType>("\nSelect item armor type")
            } as Item,
            ItemType.Valuable => new Valuable
            {
                Name = name,
                Description = description,
                Durability = durability,
                Weight = weight,
                RequiredLevel = level
            } as Item,
            ItemType.Consumable => new Consumable
            {
                Name = name,
                Description = description,
                Durability = durability,
                Weight = weight,
                RequiredLevel = level,
                ConsumableType = _inputManager.GetEnumChoice<ConsumableType>("\nSelect item consumable type")
            } as Item,
            _ => throw new ArgumentException("Invalid item type")
        };

        item.CalculateStatsByLevel();
        item.CalculateValue();
        
        _outputManager.WriteLine($"\nBased on the level provided, stats have been automatically generated.", ConsoleColor.Green);
        _outputManager.WriteLine(ColorfulToStringHelper.ItemToString(item), ColorfulToStringHelper.GetItemColor(item));

        return item;
    }
    private Item CreateItemByStats(ItemType itemType)
    {
        string name = _inputManager.ReadString("\nEnter item name: ");
        string description = _inputManager.ReadString("Enter item description: ");
        decimal weight = _inputManager.ReadDecimal("Enter item weight: ");

        var item = itemType switch
        {
            ItemType.Weapon => new Weapon
            {
                Name = name,
                Description = description,
                Weight = weight,
                AttackPower = _inputManager.ReadInt("Enter item attack power: "),
                DamageType = _inputManager.GetEnumChoice<DamageType>("Select item damage type")
            } as Item,
            ItemType.Armor => new Armor
            {
                Name = name,
                Description = description,
                Weight = weight,
                DefensePower = _inputManager.ReadInt("Enter item defense power: "),
                Resistance = _inputManager.ReadInt("Enter item resistance: "),
                ArmorType = _inputManager.GetEnumChoice<ArmorType>("Select item armor type")
            } as Item,
            ItemType.Valuable => new Valuable
            {
                Name = name,
                Description = description,
                Weight = weight,
                Value = _inputManager.ReadDecimal("Enter item value: ")
            } as Item,
            ItemType.Consumable => new Consumable
            {
                Name = name,
                Description = description,
                Weight = weight,
                Power = _inputManager.ReadInt("Enter item power: "),
                ConsumableType = _inputManager.GetEnumChoice<ConsumableType>("Select item consumable type")
            } as Item,
            _ => throw new ArgumentException("Invalid item type")
        };

        if (item is not Valuable)
            item.CalculateValue();

        item.CalculateLevelByStats();

        _outputManager.WriteLine($"\nBased on the stats provided, a level has been automatically generated.", ConsoleColor.Green);
        _outputManager.WriteLine(ColorfulToStringHelper.ItemToString(item), ColorfulToStringHelper.GetItemColor(item));

        return item;
    }
    private void EditItem()
    {
        var items = _itemDao.GetAllNonCoreItems();

        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nNo items available to edit.\n", ConsoleColor.Red);
            return;
        }

        Item? item = 
            _inputManager.Selector(
                items,
                i => ColorfulToStringHelper.ItemStatsString(i),
                "Select item to edit",
                ColorfulToStringHelper.GetItemColor);

        if (item == null)
        {
            _outputManager.WriteLine($"\nItem Editing Cancelled.\n", ConsoleColor.Red);
            return;
        }

        var propertyActions = new Dictionary<string, Action>
        {
            { "Name", () => item.Name = _inputManager.ReadString("\nEnter new name: ") },
            { "Description", () => item.Description = _inputManager.ReadString("\nEnter new description: ") },
            { "Level", () => item.RequiredLevel = _inputManager.ReadInt("\nEnter new level: ") },
            { "Value", () => item.Value = _inputManager.ReadDecimal("\nEnter new value: ") },
            { "Durability", () => item.Durability = _inputManager.ReadInt("\nEnter new durability: ") },
            { "Weight", () => item.Weight = _inputManager.ReadDecimal("\nEnter new weight: ") }
        };

        if (item is Armor a)
        {
            propertyActions["Armor type"] = () => a.ArmorType = _inputManager.GetEnumChoice<ArmorType>("Select new armor type");
            propertyActions["Defense power"] = () => a.DefensePower = _inputManager.ReadInt("\nEnter new defense power: ");
            propertyActions["Resistance"] = () => a.Resistance = _inputManager.ReadInt("\nEnter new resistance: ");
        }

        if (item is Weapon w)
        {
            propertyActions["Damage type"] = () => w.DamageType = _inputManager.GetEnumChoice<DamageType>("Select new damage type");
            propertyActions["Attack power"] = () => w.AttackPower = _inputManager.ReadInt("\nEnter new attack power: ");
        }

        if (item is Consumable c)
            propertyActions["power"] = () => c.Power = _inputManager.ReadInt("\nEnter new power: ");

        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine($"Editing item {item.Name}.", ConsoleColor.Cyan);
            _outputManager.WriteLine(ColorfulToStringHelper.ItemToString(item), ColorfulToStringHelper.GetItemColor(item));
            _outputManager.WriteLine();

            int option = _inputManager.DisplayEditMenu([.. propertyActions.Keys]);

            if (option == propertyActions.Count + 1)
            {
                _itemDao.UpdateItem(item);
                _outputManager.WriteLine($"\nExiting. Any changes made have been successfully applied to {item.Name}\n", ConsoleColor.Green);
                return;
            }

            string selectedProperty = propertyActions.Keys.ElementAt(option - 1);
            propertyActions[selectedProperty].Invoke();
        }
    }
    private void DeleteItem()
    {
        do
        {
            var items = _itemDao.GetAllNonCoreItems();

            if (items.Count == 0)
            {
                _outputManager.WriteLine("\nNo items available to delete.\n", ConsoleColor.Red);
                break;
            } 

            Item? itemToDelete =
                _inputManager.Selector(
                    items,
                    i => ColorfulToStringHelper.ItemStatsString(i),
                    "Select item to delete",
                    ColorfulToStringHelper.GetItemColor);

            if (itemToDelete == null)
            {
                _outputManager.WriteLine($"\nItem Deletion Cancelled: No Items found or action cancelled.\n", ConsoleColor.Red);
                break;
            }

            string confirm = _inputManager.ReadString($"\nPlease confirm deletion of {itemToDelete.Name} (y/n): ", ["y", "n"]).ToLower();

            if (confirm == "n")
            {
                _outputManager.WriteLine($"\nDeletion cancelled. Item [{itemToDelete.Name}] has not been deleted.", ConsoleColor.Red);
                break;
            }

            _itemDao.DeleteItem(itemToDelete);

            _outputManager.WriteLine("\nItem has been deleted successfully!\n", ConsoleColor.Green);

        } while (_inputManager.LoopAgain("delete"));
        _outputManager.WriteLine();
    }
}
