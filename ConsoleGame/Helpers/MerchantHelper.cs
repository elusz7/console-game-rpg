using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using static ConsoleGame.Helpers.AdventureEnums;

namespace ConsoleGame.Helpers;

public class MerchantHelper(InputManager inputManager, OutputManager outputManager)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;

    private Floor _floor;
    private Player _player;

    public void Meeting(Floor floor, Player player)
    {
        _floor = floor;
        _player = player;

        if (floor.HasMetMerchant)
        {
            StandardMerchantMeeting();
        }
        else
        {
            FirstMerchantMeeting();
        }
    }

    public void FirstMerchantMeeting()
    {
        _outputManager.WriteLine("\nYou approach the merchant and she greets you with a smile.");
        _floor.HasMetMerchant = true;
        _outputManager.WriteLine($"Hello, {_player.Name}. I am {_floor.GetMerchantName()}.");
        var choice = _inputManager.ReadString("I have some items for sale. Would you like to see them (y/n)? ", ["y", "n"]).ToLower();

        if (choice.Equals("y"))
        {
            BuyItems();
        }
        else
        {
            _outputManager.WriteLine("Okay, maybe next time.", ConsoleColor.Cyan);
        }

        _outputManager.WriteLine("\n\"I also have some other services you might be interested in. Come back soon.\"");
        _outputManager.WriteLine($"\t{_floor.GetMerchantName()} smiles and waves goodbye as you leave.\n");
    }
    private void StandardMerchantMeeting()
    {
        while (true)
        {
            var options = GetMenuOptions();

            if (options.Count == 0)
            {
                _outputManager.WriteLine("There are no services available right now.", ConsoleColor.Yellow);
                return;
            }

            _outputManager.WriteLine($"\nAvailable Gold: {_player.Inventory.Gold}\n", ConsoleColor.DarkYellow);

            var index = 1;
            foreach (var option in options)
            {
                _outputManager.WriteLine($"{index}. {option.Value}");
                index++;
            }

            var choice = options.ElementAt(_inputManager.ReadInt("Select an option: ", options.Count) - 1);

            switch (choice.Value)
            {
                case MerchantOptions.Purify:
                    PurifyItems();
                    break;
                case MerchantOptions.Reforge:
                    ReforgeArmor();
                    break;
                case MerchantOptions.Enchant:
                    EnchantItems();
                    break;
                case MerchantOptions.Buy:
                    BuyItems();
                    break;
                case MerchantOptions.Sell:
                    SellItems();
                    break;
                case MerchantOptions.Exit:
                    _outputManager.WriteLine($"\nA pleasure as always, {_player.Name}\n");
                    return;
            }
        }
    }
    private void BuyItems()
    {
        var items = _floor.GetMerchantItems();

        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nMy stock is currently empty. Come back later.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var item = SelectItem("Select an item to purchase", 1.25M, items);

        if (item == null)
        {
            _outputManager.WriteLine("You didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                _player.Buy(item);
                _floor.RemoveMerchantItem(item);
                _outputManager.WriteLine($"\n{item.Name} has been purchased!", ConsoleColor.Green);
            }
            catch (ItemPurchaseException ex)
            {
                _outputManager.WriteLine("\nIt seems there was an issue with your purchase.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private void SellItems()
    {
        var items = _player.Inventory.Items.ToList();
        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no items to sell.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var item = SelectItem("Select an item to sell", 0.75M, items);
       
        if (item == null)
        {
            _outputManager.WriteLine("\nYou didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                _player.Sell(item);
                _outputManager.WriteLine($"\n{item.Name} has been sold!", ConsoleColor.Green);
            }
            catch (InvalidOperationException ex)
            {
                _outputManager.WriteLine("\nIt seems there was an issue with your sale.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private void PurifyItems()
    {
        var items = _player.Inventory.Items
            .Where(i => i.IsCursed)
            .ToList();

        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no cursed items to purify.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var choice = SelectItem("Select an item to purify", 0.5M, items);

        if (choice == null)
        {
            _outputManager.WriteLine("\nYou didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                choice.Purify();
                _outputManager.WriteLine($"\n{choice.Name} has been purified!", ConsoleColor.Green);
            }
            catch (ItemPurificationException ex)
            {
                _outputManager.WriteLine("\nIt seems there was an issue with your purification.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private void ReforgeArmor()
    {
        var items = _player.Inventory.Items.Where(i => i is Armor).ToList();
        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no armor to reforge.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var choice = SelectItem("Select an item to reforge.", 0.33M, items);

        if (choice == null)
        {
            _outputManager.WriteLine("\nYou didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                _outputManager.WriteLine($"\nCURRENT STATS {ColorfulToStringHelper.GetItemStats(choice)} |", ConsoleColor.Cyan);

                choice.Reforge();
                _outputManager.WriteLine($"\n{choice.Name} has been reforged!");
                _outputManager.WriteLine($"NEW STATS {ColorfulToStringHelper.GetItemStats(choice)} |", ConsoleColor.Cyan);
            }
            catch (ItemReforgeException ex)
            {
                _outputManager.WriteLine("\nIt seems there was an issue with reforging.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private void EnchantItems()
    {
        var items = _player.Inventory.Items
            .Where(i => (i is Armor || i is Weapon) && i.RequiredLevel < _player.Level)
            .ToList();

        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no items to enchant.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var choice = SelectItem("Select an item to enchant", 1.5M, items);

        if (choice == null)
        {
            _outputManager.WriteLine("\nYou didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                _outputManager.WriteLine($"\nCURRENT STATS | LVL: {choice.RequiredLevel}{ColorfulToStringHelper.GetItemStats(choice)} |", ConsoleColor.Cyan);
                choice.Enchant();
                _outputManager.WriteLine($"\n{choice.Name} has been enchanted!", ConsoleColor.Green);
                _outputManager.WriteLine($"NEW STATS | LVL: {choice.RequiredLevel}{ColorfulToStringHelper.GetItemStats(choice)} |", ConsoleColor.Cyan);
            }
            catch (ItemEnchantmentException ex)
            {
                _outputManager.WriteLine("\nIt seems there was an issue with the enchantment process.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private Item? SelectItem(string prompt, decimal valueMultiplier, List<Item> items)
    {
        return _inputManager.SelectFromList(
            items,
            i => ColorfulToStringHelper.ItemStatsString(i, valueMultiplier),
            prompt,
            i => ColorfulToStringHelper.GetItemColor(i)
        );
    }
    private Dictionary<string, MerchantOptions> GetMenuOptions()
    {
        var options = new Dictionary<string, MerchantOptions>();

        AddMenuOption(options, "Buy Item", MerchantOptions.Buy);
        AddMenuOption(options, "Sell Item", MerchantOptions.Sell);

        if (_player.Inventory.Items.Where(i => i.IsCursed).Any())
        {
            AddMenuOption(options, "Purify Item", MerchantOptions.Purify);
        }

        if (_player.Inventory.Items.OfType<Armor>().Any())
        {
            AddMenuOption(options, "Reforge Armor", MerchantOptions.Reforge);
        }

        //add option to raise weapon/armor level if player has lower level items
        if (_player.Inventory.Items
            .Where(i => (i is Weapon || i is Armor)
            && i.RequiredLevel < _player.Level).Any())
        {
            AddMenuOption(options, "Enchant Item", MerchantOptions.Enchant);
        }

        AddMenuOption(options, "Exit", MerchantOptions.Exit);

        return options;
    }
    private static void AddMenuOption(Dictionary<string, MerchantOptions> options, string label, MerchantOptions option)
    {
        options.TryAdd(label, option);
    }
}
