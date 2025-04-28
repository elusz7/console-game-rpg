using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers;

public class ItemManagement
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly ItemDao _itemDao;

    public ItemManagement(InputManager inputManager, OutputManager outputManager, ItemDao itemDao)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _itemDao = itemDao;
    }

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

            ItemType itemType = GetItemType();
            Item item = CreateItem(itemType);

            _itemDao.AddItem(item);

            _outputManager.WriteLine($"\nItem {item.Name} successfully created.\n", ConsoleColor.Green);

        } while (LoopAgain("add"));
        _outputManager.WriteLine();
    }
    private ItemType GetItemType()
    {
        _outputManager.WriteLine("\nSelect item type:");
        var itemTypes = Enum.GetValues<ItemType>();
        for (int i = 0; i < itemTypes.Length; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {itemTypes[i]}");
        }

        int index = _inputManager.ReadInt("Enter the number corresponding to the item type: ", itemTypes.Length) - 1;

        return itemTypes[index];
    }
    private Item CreateItem(ItemType itemType)
    {
        string name = _inputManager.ReadString("\nEnter item name: ");
        string description = _inputManager.ReadString("Enter item description: ");
        decimal value = _inputManager.ReadDecimal("Enter item value: ");
        int durability = _inputManager.ReadInt("Enter item durability: ");
        decimal weight = _inputManager.ReadDecimal("Enter item weight: ");

        return itemType switch
        {
            ItemType.Weapon => new Weapon
            {
                Name = name,
                Value = value,
                Description = description,
                Durability = durability,
                Weight = weight,
                AttackPower = _inputManager.ReadInt("Enter item attack power: ")
            },
            ItemType.Armor => new Armor
            {
                Name = name,
                Value = value,
                Description = description,
                Durability = durability,
                Weight = weight,
                DefensePower = _inputManager.ReadInt("Enter item defense power: ")
            },
            _ => throw new ArgumentException("Invalid item type")
        };
    }
    private void EditItem()
    {
        List<Item> items = _itemDao.GetAllItems();

        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nNo items available to edit.\n", ConsoleColor.Red);
            return;
        }

        Item item = _inputManager.PaginateList(items, i => i.Name, "item", "edit", true);

        if (item == null)
        {
            _outputManager.WriteLine($"\nItem Editing Cancelled.\n", ConsoleColor.Red);
            return;
        }

        string[] validResponses = item switch
        {
            Weapon => new[] { "Name", "Description", "Value", "Durability", "Weight", "Attack power" },
            Armor => new[] { "Name", "Description", "Value", "Durability", "Weight", "Defense power" }
        };

        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine($"Editing item {item.Name}.", ConsoleColor.Cyan);
            DisplayItemDetails(item);
            _outputManager.WriteLine();
            for (int i = 0; i < validResponses.Length; i++)
            {
                _outputManager.WriteLine($"{i + 1}. {validResponses[i]}");
            }
            _outputManager.WriteLine($"{validResponses.Length + 1}. Exit");

            int index = _inputManager.ReadInt("\nSelect property to edit by number: ", validResponses.Length + 1) - 1;

            if (index == validResponses.Length)
            {
                _itemDao.UpdateItem(item);
                _outputManager.WriteLine($"\nItem {item.Name} successfully updated.\n", ConsoleColor.Green);
                return;
            }

            UpdateItemProperty(item, validResponses[index]);
        }
    }
    private void DeleteItem()
    {
        do
        {
            var items = _itemDao.GetAllItems();

            if (items.Count == 0)
            {
                _outputManager.WriteLine("\nNo items available to delete.\n", ConsoleColor.Red);
                break;
            } 

            Item itemToDelete = _inputManager.PaginateList(items, i => i.Name, "item", "delete", true);

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

        } while (LoopAgain("delete"));
        _outputManager.WriteLine();
    }
    private void DisplayItemDetails(Item item)
    {
        if (item is Weapon weapon)
            _outputManager.WriteLine(weapon.ToString(), ConsoleColor.Magenta);
        else if (item is Armor armor)
            _outputManager.WriteLine(armor.ToString(), ConsoleColor.DarkYellow);
    }
    private void UpdateItemProperty(Item item, string property)
    {
        Action updateAction = property switch
        {
            "Name" => () => item.Name = _inputManager.ReadString("\nEnter new name: "),
            "Description" => () => item.Description = _inputManager.ReadString("\nEnter new description: "),
            "Value" => () => item.Value = _inputManager.ReadDecimal("\nEnter new value: "),
            "Durability" => () => item.Durability = _inputManager.ReadInt("\nEnter new durability: "),
            "Weight" => () => item.Weight = _inputManager.ReadDecimal("\nEnter new weight: "),
            "Attack power" => () => ((Weapon)item).AttackPower = _inputManager.ReadInt("\nEnter new attack power: "),
            "Defense power" => () => ((Armor)item).DefensePower = _inputManager.ReadInt("\nEnter new defense power: "),
            _ => throw new ArgumentException("Invalid property")
        };
        updateAction();
    }
    private bool LoopAgain(string action)
    {
        string again = _inputManager.ReadString($"Would you like to {action} another? (y/n): ", new[] { "y", "n" }).ToLower();
        return again == "y";
    }
}
