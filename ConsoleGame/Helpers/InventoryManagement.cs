using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers;

public class InventoryManagement
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly InventoryDao _inventoryDao;
    private Player _player;

    public InventoryManagement(InputManager inputManager, OutputManager outputManager, ItemDao itemDao, InventoryDao inventoryDao)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _inventoryDao = inventoryDao;
    }
    public void Menu(Player? player = null)
    {
        _outputManager.Clear();

        _player = player;
        if (_player == null)
        {
            List<Player> players = _inventoryDao.GetAllPlayers();

            if (players.Count == 0)
            {
                _outputManager.WriteLine("No players available to manage inventory.", ConsoleColor.Red);
                return;
            }

            _player = _inputManager.PaginateList(players, p => p.Name, "player", "manage inventory of", true);
            
            if (_player == null)
            {
                _outputManager.WriteLine("No player selected. Returning to previous menu.", ConsoleColor.Red);
                return;
            }
        }

        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine($"=== {_player.Name}'s Inventory Management ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Add Item to Inventory"
                + "\n2. Remove Item from Inventory"
                + "\n3. Return to Previous Menu");
            var input = _inputManager.ReadMenuKey(3);

            switch (input)
            {
                case 1:
                    AddItemToInventory();
                    break;
                case 2:
                    RemoveItemFromInventory();
                    break;
                case 3:
                    player = null;
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void AddItemToInventory()
    {
        do
        {
            var equippableItems = _inventoryDao.GetEquippableItems(_player);

            decimal currentCarryingWeight = _inventoryDao.GetInventoryWeight(_player);
            decimal weightAvailable = _player.Inventory.Capacity - currentCarryingWeight;

            _outputManager.WriteLine($"\nCapacity: {currentCarryingWeight} / {_player.Inventory.Capacity}");

            if (!equippableItems.Any())
            {
                _outputManager.WriteLine("\nNo equippable items available.\n");
                break;
            }
            Item itemToAdd = _inputManager.PaginateList(equippableItems, i => i.ToString(), "item", "add to inventory", true);

            if (itemToAdd == null || !ConfirmAction("addition"))
            {
                _outputManager.WriteLine($"\nItem not added to inventory.\n", ConsoleColor.Red);
                continue;
            }

            _player.Inventory.AddItem(itemToAdd);
            _inventoryDao.UpdateInventory(_player.Inventory);
            _outputManager.WriteLine($"\nItem {itemToAdd.Name} added to inventory.\n", ConsoleColor.Green);

        } while (LoopAgain("add"));
        _outputManager.WriteLine();
    }
    private void RemoveItemFromInventory()
    {
        do
        {
            if (_player.Inventory.Items.Count == 0)
            {
                _outputManager.WriteLine("\nNo items in inventory to remove.\n");
                break;
            }

            Item itemToRemove = _inputManager.PaginateList<Item>(_player.Inventory.Items.ToList(), i => i.ToString(), "item", "remove from inventory", true);

            if (itemToRemove == null || !ConfirmAction("removal"))
            {
                _outputManager.WriteLine("\nItem Removal Cancelled.\n");
                break;
            }

            _player.Inventory.RemoveItem(itemToRemove);
            _inventoryDao.UpdateInventory(_player.Inventory);

            _outputManager.WriteLine($"\nItem Successfully Removed From {_player.Name}'s Inventory\n", ConsoleColor.Green);
        } while (LoopAgain("remove"));
        _outputManager.WriteLine();
    }
    private bool ConfirmAction(string action)
    {
        string confirm = _inputManager.ReadString($"\n\nPlease confirm {action} of item (y/n): ", new[] { "y", "n" });
        return confirm == "y";
    }
    private bool LoopAgain(string action)
    {
        string again = _inputManager.ReadString($"Would you like to {action} another? (y/n): ", new[] { "y", "n" }).ToLower();
        return again == "y";
    }
}