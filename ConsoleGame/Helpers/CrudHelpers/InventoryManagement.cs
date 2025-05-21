using ConsoleGame.GameDao;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Managers.CrudHelpers;

public class InventoryManagement(InputManager inputManager, OutputManager outputManager, InventoryDao inventoryDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly InventoryDao _inventoryDao = inventoryDao;
    private Player? _player;

    public void Menu(Player? player = null)
    {
        _outputManager.Clear();

        if (player == null)
        {
            if (!SelectPlayer())
            {
                _outputManager.WriteLine("No player available or selected. Returning to previous menu.", ConsoleColor.Red);
                return;
            }
        }
        else
        {
            _player = player;
        }

        if (_player == null)
        {
            _outputManager.WriteLine("No player available or selected. Returning to previous menu.", ConsoleColor.Red);
            return;
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
                    _player = null;
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private bool SelectPlayer()
    {
        List<Player> players = _inventoryDao.GetAllPlayers();

        if (players.Count == 0)
        {
            return false;
        }

        var player = 
            _inputManager.Selector(
                players, 
                p => ColorfulToStringHelper.PlayerToString(p), 
                "Select a player to manage their inventory", 
                q => ColorfulToStringHelper.GetArchetypeColor(q.Archetype));

        if (player == null)
        {
            return false;
        }

        _player = player;
        return true;
    }
    private void AddItemToInventory()
    {       
        do
        {
            var equippableItems = _inventoryDao.GetEquippableItems(_player!);

            _outputManager.WriteLine($"\nCapacity: {_player!.Inventory.GetCarryingWeight()} / {_player.Inventory.Capacity}");

            if (equippableItems.Count == 0)
            {
                _outputManager.WriteLine("\nNo equippable items available.\n");
                break;
            }
            Item? itemToAdd =
                _inputManager.Selector(
                    equippableItems,
                    i => ColorfulToStringHelper.ItemToString(i),
                    "Select an item to add to inventory",
                    j => ColorfulToStringHelper.GetItemColor(j)
                    );

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
            var inventory = _player!.Inventory.Items.ToList();
            if (inventory.Count == 0)
            {
                _outputManager.WriteLine("\nNo items in inventory to remove.\n");
                break;
            }

            Item? itemToRemove =
                _inputManager.Selector(
                    inventory,
                    i => ColorfulToStringHelper.ItemToString(i),
                    "Select an item to remove from inventory",
                    j => ColorfulToStringHelper.GetItemColor(j)
                    );

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