using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using static ConsoleGame.Helpers.AdventureEnums;

namespace ConsoleGame.Helpers;

public class MerchantHelper(IInputManager inputManager, IOutputManager outputManager)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;

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

    private void FirstMerchantMeeting()
    {
        _outputManager.WriteLine("\nYou approach the stranger and she greets you with a smile.");
        _floor.HasMetMerchant = true;
        _outputManager.WriteLine($"Hello, {_player.Name}. I am {_floor.GetMerchantName()}.");

        Gift();

        _outputManager.WriteLine("\nI also have some other services you might be interested in. Come back soon.");
        _outputManager.WriteLine($"{_floor.GetMerchantName()} smiles and waves goodbye as you leave.");
    }

    private void Gift()
    {
        var items = _floor.GetMerchantGifts(_player.Archetype.ArchetypeType, _player.Level);
        if (items.Count == 0)
        {
            return;
        }

        _outputManager.WriteLine("Take these items as a token of our new friendship.\n");

        foreach (var item in items)
        {
            try
            {
                _player.Inventory.AddItem(item);
                _outputManager.WriteLine($"{item.Name} has been gifted to you!", ConsoleColor.Green);
            }
            catch (InventoryException) { }
        }
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
                _outputManager.WriteLine($"{index}. {option.Value}", ConsoleColor.Yellow);
                index++;
            }

            var choice = options.ElementAt(_inputManager.ReadInt("\tSelect an option: ", options.Count) - 1);

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
            _outputManager.WriteLine("My stock is currently empty. Come back later.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var item = _inputManager.SelectItem("Select an item to purchase", items, purpose: "buy");

        if (item == null) return;

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
    private void SellItems()
    {
        var items = _player.Inventory.Items.ToList();
        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no items to sell.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var item = _inputManager.SelectItem("Select an item to sell", items, purpose: "sell");

        if (item == null) return;

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
    private void PurifyItems()
    {
        var items = _player.Inventory.Items
            .Where(i => i is ICursable c && c.IsCursed())
            .ToList();

        if (items.Count == 0)
        {
            _outputManager.WriteLine("You have no cursed items to purify.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var choice = _inputManager.SelectItem("Select an item to purify", items, purpose: "purify");

        if (choice == null) return;

        if (choice is ICursable cursed)
        {
            try
            {
                cursed.Purify();
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
        var items = _player.Inventory.Items.Where(i => i is IReforgable).ToList();
        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no armor to reforge.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var choice = _inputManager.SelectItem("Select an item to reforge.", items, purpose: "reforge");

        if (choice == null) return;

        if (choice is Armor armor)
        {
            try
            {
                int oldDefense = armor.DefensePower;
                int oldResistance = armor.Resistance;

                armor.Reforge();
                _outputManager.WriteLine($"\nUPDATED [{choice.Name}] LVL: {choice.RequiredLevel} | DEF: {oldDefense} --> {armor.DefensePower} | RES: {oldResistance} --> {armor.Resistance}", ConsoleColor.Blue);
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
            .Where(i => i is IEnchantable && i.RequiredLevel < _player.Level)
            .ToList();

        if (items.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no items to enchant.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var choice = _inputManager.SelectItem("Select an item to enchant", items, purpose: "enchant");

        if (choice == null) return;

        if (choice is IEnchantable enchanter)
        {
            try
            {
                // Capture pre-enchant stats
                int oldLevel = choice.RequiredLevel;

                if (choice is Weapon weapon)
                {
                    int oldAttack = weapon.AttackPower;

                    enchanter.Enchant();

                    _outputManager.WriteLine($"\n{choice.Name} | LVL: {oldLevel} --> {choice.RequiredLevel} | ATK: {oldAttack} --> {weapon.AttackPower}", ConsoleColor.Magenta);
                }
                else if (choice is Armor armor)
                {
                    int oldDefense = armor.DefensePower;
                    int oldResistance = armor.Resistance;

                    enchanter.Enchant();

                    _outputManager.WriteLine($"\n{choice.Name} | LVL: {oldLevel} --> {choice.RequiredLevel} | DEF: {oldDefense} --> {armor.DefensePower} | RES: {oldResistance} --> {armor.Resistance}", ConsoleColor.Magenta);
                }
            }
            catch (ItemEnchantmentException ex)
            {
                _outputManager.WriteLine("\nIt seems there was an issue with the enchantment process.", ConsoleColor.Red);
                _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
    private Dictionary<string, MerchantOptions> GetMenuOptions()
    {
        var options = new Dictionary<string, MerchantOptions>();

        AddMenuOption(options, "Buy Item", MerchantOptions.Buy);
        AddMenuOption(options, "Sell Item", MerchantOptions.Sell);

        if (_player.Inventory.Items.Where(i => i is ICursable c && c.IsCursed()).Any())
        {
            AddMenuOption(options, "Purify Item", MerchantOptions.Purify);
        }

        if (_player.Inventory.Items.OfType<IReforgable>().Any())
        {
            AddMenuOption(options, "Reforge Armor", MerchantOptions.Reforge);
        }

        //add option to raise weapon/armor level if player has lower level items
        if (_player.Inventory.Items.Where(i => i is IEnchantable && i.RequiredLevel < _player.Level).Any())
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
