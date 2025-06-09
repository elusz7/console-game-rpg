using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Managers;
public class InputManager(IOutputManager outputManager) : IInputManager
{
    private readonly IOutputManager _outputManager = outputManager;

    public ConsoleKeyInfo ReadKey()
    {
        return Console.ReadKey(intercept: true);
    }
    public int ReadMenuKey(int maxValue)
    {
        _outputManager.Display();
        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true); // Don't show the pressed key

            if (char.IsDigit(keyInfo.KeyChar) && int.TryParse(keyInfo.KeyChar.ToString(), out int option))
            {
                if (option > 0 && option <= maxValue)
                {
                    return option;
                }
            }

            _outputManager.WriteLine($"\nInvalid selection. Please enter a number between 1 and {maxValue}.", ConsoleColor.Red);
            _outputManager.Display();
        }
    }
    public int ReadInt(string prompt, int? max = null, bool cancel = false)
    {
        _outputManager.Write(prompt);
        _outputManager.Display();

        string input = Console.ReadLine()?.Trim() ?? "";
        bool isInt = int.TryParse(input, out int result);

        while (
            !isInt ||
            (max.HasValue && result > max.Value) ||
            (result < 1 && (!cancel || result != -1))
        )
        {
            string error = cancel
                ? "Invalid input. Please enter a valid positive integer or -1 to cancel: "
                : "Invalid input. Please enter a valid positive integer: ";

            _outputManager.Write(error);
            _outputManager.Display();
            input = Console.ReadLine()?.Trim() ?? "";
            isInt = int.TryParse(input, out result);
        }

        return result;
    }
    public decimal ReadDecimal(string prompt)
    {
        do
        {
            _outputManager.Write(prompt);
            _outputManager.Display();

            var input = Console.ReadLine()?.Trim() ?? "";

            if (decimal.TryParse(input, out var result) && result >= 0.0M)
                return result;

            prompt = "Invalid input. Please enter a valid, positive decimal number: ";
        }
        while (true);
    }
    public string ReadString(string prompt, string[]? validation = null)
    {
        do
        {
            _outputManager.Write(prompt);
            _outputManager.Display();

            var input = Console.ReadLine()?.Trim() ?? "";

            if (!string.IsNullOrEmpty(input) && (validation == null || validation.Any(v => string.Equals(v, input, StringComparison.OrdinalIgnoreCase))))
                return input;

            prompt = "Invalid input. Please try again: ";
        }
        while (true);
    }
    private T? PaginateWithFunction<T>(List<T> list, Func<T, string> displayTextSelector, string prompt, Func<T, ConsoleColor>? colorSelector = null, bool allowSelection = false)
    {
        const int pageSize = 9;
        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(list.Count / (double)pageSize);

        while (true)
        {
            _outputManager.WriteLine("=== List ===", ConsoleColor.Cyan);

            var pageItems = list.Skip(currentPage * pageSize).Take(pageSize).ToList();

            for (int i = 0; i < pageItems.Count; i++)
            {
                var item = pageItems[i];
                var display = $"{i + 1}. {displayTextSelector(item)}";
                var color = colorSelector?.Invoke(item) ?? ConsoleColor.White;
                _outputManager.WriteLine(display, color);
            }

            var pageInfo = $"{currentPage + 1} of {totalPages}";
            if (totalPages > 1)
            {
                if (currentPage > 0) pageInfo = "<-- " + pageInfo;
                if (currentPage < totalPages - 1) pageInfo += " -->";
            }
            _outputManager.WriteLine($"\n{pageInfo}", ConsoleColor.DarkGreen);

            string navigationPrompt = "\n";
            if (totalPages > 1)
                navigationPrompt += "Use <-- --> arrows to navigate pages. ";
            if (allowSelection)
                navigationPrompt += $"Enter number to {prompt}. ";
            navigationPrompt += "Press 'q' to quit.\n";

            _outputManager.Write(navigationPrompt);
            _outputManager.Display();

            while (true)
            {
                var key = ReadKey();

                if ((key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.DownArrow) && currentPage < totalPages - 1)
                {
                    currentPage++;
                    break;
                }
                else if ((key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.UpArrow) && currentPage > 0)
                {
                    currentPage--;
                    break;
                }
                else if (key.Key == ConsoleKey.Q)
                {
                    _outputManager.Clear();
                    return default;
                }
                else if (allowSelection && char.IsDigit(key.KeyChar))
                {
                    int selection = int.Parse(key.KeyChar.ToString());
                    if (selection > 0 && selection <= pageItems.Count)
                    {
                        return pageItems[selection - 1];
                    }
                }
                else
                {
                    _outputManager.WriteLine("Invalid input. Please try again.", ConsoleColor.Red);
                    _outputManager.Display();
                }
            }

            _outputManager.Clear();
        }
    }
    private T? SelectFromList<T>(List<T> items, Func<T, string> displayTextSelector, string prompt, Func<T, ConsoleColor>? colorSelector = null, bool allowSelection = false)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var display = $"{i + 1}. {displayTextSelector(item)}";
            var color = colorSelector?.Invoke(item) ?? ConsoleColor.White;
            _outputManager.WriteLine(display, color);
        }
        if (!allowSelection)
        {
            _outputManager.WriteLine();
            return default;
        }

        string fullPrompt = $"\n\t{prompt} (-1 to cancel): ";
        int choice = ReadInt(fullPrompt, items.Count, true);

        return choice == -1 ? default : items[choice - 1];
    }
    public T? Selector<T>(List<T> items, Func<T, string> displayTextSelector, string prompt, Func<T, ConsoleColor>? colorSelector = null)
     => items.Count < 13 ?
            SelectFromList(items, displayTextSelector, prompt, colorSelector, true)
            : PaginateWithFunction(items, displayTextSelector, prompt, colorSelector, true);
    public T? Viewer<T>(List<T> items, Func<T, string> displayTextSelector, string prompt, Func<T, ConsoleColor>? colorSelector = null)
    {
        return items.Count < 13 ?
            SelectFromList(items, displayTextSelector, prompt, colorSelector)
            : PaginateWithFunction(items, displayTextSelector, prompt, colorSelector);
    }
    public Item? SelectItem(string prompt, List<Item> items, string? purpose = null)
    {
        return Selector(
            items,
            i =>
            {
                decimal? value = purpose switch
                {
                    "enchant" => i is IEnchantable e ? e.GetEnchantmentPrice() : null,
                    "reforge" => i is IReforgable r ? r.GetReforgePrice() : null,
                    "purify" => i is ICursable c ? c.GetPurificationPrice() : null,
                    "buy" => i.GetBuyPrice(),
                    "sell" => i.GetSellPrice(),
                    _ => null
                };

                return ColorfulToStringHelper.ItemStatsString(i, value);
            },
            prompt,
            i => ColorfulToStringHelper.GetItemColor(i)
        );
    }
    public bool LoopAgain(string action)
    {
        string again = ReadString($"Would you like to {action} another? (y/n): ", ["y", "n"]).ToLower();
        return again == "y";
    }
    public bool ConfirmAction(string action)
    {
        string confirm = ReadString($"\nPlease confirm {action} (y/n): ", ["y", "n"]).ToLower();
        return confirm == "y";
    }
    public T GetEnumChoice<T>(string prompt) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();

        for (int i = 0; i < values.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {values[i]}");
        }

        var choice = ReadInt($"\n{prompt}: ", values.Count);
        return values[choice - 1];
    }
    public int DisplayEditMenu(List<string> properties)
    {
        for (int i = 0; i < properties.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. Change {properties[i]}");
        }
        _outputManager.WriteLine($"{properties.Count + 1}. Exit", ConsoleColor.Red);

        return ReadInt("\nWhat property would you like to edit? ", properties.Count + 1);
    }
}
