using ConsoleGame.GameDao;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers.CrudHelpers;

public class ItemManagement(InputManager inputManager, OutputManager outputManager, ItemDao itemDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly ItemDao _itemDao = itemDao;

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
            Item item = CreateItem(itemType);

            _itemDao.AddItem(item);

            _outputManager.WriteLine($"\nItem {item.Name} successfully created.\n", ConsoleColor.Green);

        } while (_inputManager.LoopAgain("add"));
        _outputManager.WriteLine();
    }
    private Item CreateItem(ItemType itemType)
    {
        string name = _inputManager.ReadString("\nEnter item name: ");
        string description = _inputManager.ReadString("Enter item description: ");
        decimal value = _inputManager.ReadDecimal("Enter item value: ");
        int durability = _inputManager.ReadInt("Enter item durability: ");
        decimal weight = _inputManager.ReadDecimal("Enter item weight: ");
        int level = _inputManager.ReadInt("Enter item required level: ");

        return itemType switch
        {
            ItemType.Weapon => new Weapon
            {
                Name = name,
                Value = value,
                Description = description,
                Durability = durability,
                Weight = weight,
                RequiredLevel = level,
                AttackPower = _inputManager.ReadInt("Enter item attack power: "),
                DamageType = _inputManager.GetEnumChoice<DamageType>("Select item damage type")
            },
            ItemType.Armor => new Armor
            {
                Name = name,
                Value = value,
                Description = description,
                Durability = durability,
                Weight = weight,
                RequiredLevel = level,
                DefensePower = _inputManager.ReadInt("Enter item defense power: "),
                Resistance = _inputManager.ReadInt("Enter item resistance: "),
                ArmorType = _inputManager.GetEnumChoice<ArmorType>("Select item armor type")
            },
            ItemType.Valuable => new Valuable
            {
                Name = name,
                Value = value,
                Description = description,
                Durability = durability,
                Weight = weight,
                RequiredLevel = level
            },
            ItemType.Consumable => new Consumable
            {
                Name = name,
                Value = value,
                Description = description,
                Durability = durability,
                Weight = weight,
                RequiredLevel = level,
                Power = _inputManager.ReadInt("Enter item power: "),
                ConsumableType = _inputManager.GetEnumChoice<ConsumableType>("Select item consumable type")
            },
            _ => throw new ArgumentException("Invalid item type")
        };
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
