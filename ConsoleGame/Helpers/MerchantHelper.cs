using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        _outputManager.WriteLine("You approach the merchant and she greets you with a smile.", ConsoleColor.Cyan);
        _floor.HasMetMerchant = true;
        _outputManager.WriteLine($"Hello, {_player.Name}. I am {_floor.GetMerchantName()}.", ConsoleColor.Cyan);
        _outputManager.WriteLine("I have some items for sale. Would you like to see them?", ConsoleColor.Cyan);
        var choice = _inputManager.ConfirmAction("See items for sale");

        if (choice)
        {
            BuyItems();
        }
        else
        {
            _outputManager.WriteLine("Okay, maybe next time.", ConsoleColor.Cyan);
        }

        _outputManager.WriteLine("\"I also have some other services you might be interested in. Come back soon.\"");
        _outputManager.WriteLine($"{_floor.GetMerchantName()} smiles and waves goodbye as you leave.", ConsoleColor.Cyan);
    }
    private void StandardMerchantMeeting()
    {
        var options = GetMenuOptions();

        if (options.Count == 0)
        {
            _outputManager.WriteLine("There are no services available right now.", ConsoleColor.Yellow);
            return;
        }

        var index = 1;
        foreach (var option in options)
        {
            _outputManager.WriteLine($"{index}. {option.Value}", ConsoleColor.Cyan);
            index++;
        }

        var choice = options.ElementAt(_inputManager.ReadInt("Select an option", options.Count) - 1);

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
                _outputManager.WriteLine($"A pleasure as always, {_player.Name}", ConsoleColor.Cyan);
                break;
        }
    }

    private void BuyItems()
    {
        var items = new List<Item>();//_floor.GetMerchantItems();

        if (items.Count == 0)
        {
            _outputManager.WriteLine("My stock is currently empty. Come back later.");
            return;
        }

        _outputManager.WriteLine("Here are the items I have for sale:", ConsoleColor.Cyan);

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
                _outputManager.WriteLine($"{item.Name} has been purchased!", ConsoleColor.Green);
            }
            catch (ItemPurchaseException ex)
            {
                _outputManager.WriteLine("It seems there was an issue with your purchase.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private void SellItems()
    {
        var items = _player.Inventory.Items.ToList();
        if (items.Count == 0)
        {
            _outputManager.WriteLine("You have no items to sell.");
            return;
        }
        
        _outputManager.WriteLine("Here are the items you can sell:", ConsoleColor.Cyan);
        
        var item = SelectItem("Select an item to sell", 0.75M, items);
       
        if (item == null)
        {
            _outputManager.WriteLine("You didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                _player.Sell(item);
                _outputManager.WriteLine($"{item.Name} has been sold!", ConsoleColor.Green);
            }
            catch (InvalidOperationException ex)
            {
                _outputManager.WriteLine("It seems there was an issue with your sale.", ConsoleColor.Red);
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
            _outputManager.WriteLine("You have no cursed items to purify.");
            return;
        }

        var choice = SelectItem("Select an item to purify", 0.5M, items);

        if (choice == null)
        {
            _outputManager.WriteLine("You didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                choice.Purify();
                _outputManager.WriteLine($"{choice.Name} has been purified!", ConsoleColor.Green);
            }
            catch (ItemPurificationException ex)
            {
                _outputManager.WriteLine("It seems there was an issue with your purification.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private void ReforgeArmor()
    {
        var items = _player.Inventory.Items.Where(i => i is Armor).ToList();
        if (items.Count == 0)
        {
            _outputManager.WriteLine("You have no armor to reforge.");
            return;
        }
        var choice = SelectItem("Select an item to reforge.", 0.33M, items);

        if (choice == null)
        {
            _outputManager.WriteLine("You didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                _outputManager.WriteLine($"CURRENT STATS {GetItemStats(choice)} |", ConsoleColor.Cyan);

                choice.Reforge();
                _outputManager.WriteLine($"{choice.Name} has been reforged!", ConsoleColor.Green);
                _outputManager.WriteLine($"NEW STATS {GetItemStats(choice)} |", ConsoleColor.Cyan);
            }
            catch (ItemReforgeException ex)
            {
                _outputManager.WriteLine("It seems there was an issue with reforging.", ConsoleColor.Red);
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
            _outputManager.WriteLine("You have no items to enchant.");
            return;
        }

        var choice = SelectItem("Select an item to enchant", 1.5M, items);

        if (choice == null)
        {
            _outputManager.WriteLine("You didn't select an item.", ConsoleColor.Red);
        }
        else
        {
            try
            {
                _outputManager.WriteLine($"CURRENT STATS | LVL: {choice.RequiredLevel}{GetItemStats(choice)} |", ConsoleColor.Cyan);
                choice.Enchant();
                _outputManager.WriteLine($"{choice.Name} has been enchanted!", ConsoleColor.Green);
                _outputManager.WriteLine($"NEW STATS | LVL: {choice.RequiredLevel}{GetItemStats(choice)} |", ConsoleColor.Cyan);
            }
            catch (ItemEnchantmentException ex)
            {
                _outputManager.WriteLine("It seems there was an issue with the enchantment process.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private Item? SelectItem(string prompt, decimal valueMultiplier, List<Item> items)
    {
        return _inputManager.SelectFromList(
            items,
            i => $"{i.Name} - {i.Value * valueMultiplier} [DUR: {i.Durability}{GetItemStats(i)}]",
            prompt,
            i => GetItemColor(i)
        );
    }
    private static string GetItemStats(Item item)
    {
        return item switch
        {
            Weapon weapon => $" | ATK: {weapon.AttackPower}",
            Armor armor => $" | DEF: {armor.DefensePower} | RES: {armor.Resistance}",
            Consumable consumable => $" | POW: {consumable.Power} | AFF: {consumable.ConsumableType}",
            _ => ""
        };
    }
    private static ConsoleColor GetItemColor(Item item)
    {
        return item switch
        {
            Weapon => ConsoleColor.Red,
            Armor => ConsoleColor.Green,
            Consumable => ConsoleColor.Blue,
            _ => ConsoleColor.Yellow
        };
    }
    private Dictionary<string, MerchantOptions> GetMenuOptions()
    {
        var options = new Dictionary<string, MerchantOptions>();

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
            .Where(i => (i.ItemType.Equals("Weapon") || i.ItemType.Equals("Armor")) 
            && i.RequiredLevel < _player.Level).Any())
        {
            AddMenuOption(options, "Enchant Item", MerchantOptions.Enchant);
        }


        AddMenuOption(options, "Buy Item", MerchantOptions.Buy);
        AddMenuOption(options, "Sell Item", MerchantOptions.Sell);
        AddMenuOption(options, "Exit", MerchantOptions.Exit);

        return options;
    }
    private static void AddMenuOption(Dictionary<string, MerchantOptions> options, string label, MerchantOptions option)
    {
        options.TryAdd(label, option);
    }
}
