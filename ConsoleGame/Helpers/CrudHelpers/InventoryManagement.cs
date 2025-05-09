using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers.CrudHelpers;

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

        if (player == null)
        {
            List<Player> players = _inventoryDao.GetAllPlayers();

            if (players.Count == 0)
            {
                _outputManager.WriteLine("No players available to manage inventory.", ConsoleColor.Red);
                return;
            }

            player = _inputManager.PaginateList(players, "player", "manage inventory of", true, false);
            
            if (player == null)
            {
                _outputManager.WriteLine("No player selected. Returning to previous menu.", ConsoleColor.Red);
                return;
            }
        }

        _player = player;

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

            _outputManager.WriteLine($"\nCapacity: {_player.Inventory.GetCarryingWeight()} / {_player.Inventory.Capacity}");

            if (equippableItems.Count == 0)
            {
                _outputManager.WriteLine("\nNo equippable items available.\n");
                break;
            }
            Item? itemToAdd = _inputManager.PaginateList(equippableItems, "item", "add to inventory", true, false);

            if (itemToAdd == null)
            {
                _outputManager.WriteLine($"\nNo item selected to add to inventory.\n", ConsoleColor.Red);
                continue;
            }

            _outputManager.WriteLine($"You have selected [{itemToAdd.Name}]", ConsoleColor.Green);

            if( !_inputManager.ConfirmAction("addition"))
            {
                _outputManager.WriteLine($"\nAction cancelled. {itemToAdd.Name} not added to inventory.\n", ConsoleColor.Red);
            }

            _player.Inventory.AddItem(itemToAdd);
            _inventoryDao.UpdateInventory(_player.Inventory);
            _outputManager.WriteLine($"\nItem {itemToAdd.Name} added to inventory.\n", ConsoleColor.Green);

        } while (_inputManager.LoopAgain("add"));
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

            Item? itemToRemove = _inputManager.PaginateList(_player.Inventory.Items.ToList(), "item", "remove from inventory", true);

            if (itemToRemove == null || !_inputManager.ConfirmAction("removal"))
            {
                _outputManager.WriteLine("\nItem Removal Cancelled.\n");
                break;
            }

            _player.Inventory.RemoveItem(itemToRemove);
            _inventoryDao.UpdateInventory(_player.Inventory);

            _outputManager.WriteLine($"\nItem Successfully Removed From {_player.Name}'s Inventory\n", ConsoleColor.Green);
        } while (_inputManager.LoopAgain("remove"));
        _outputManager.WriteLine();
    }
}