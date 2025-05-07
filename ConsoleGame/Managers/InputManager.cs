using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
namespace ConsoleGame.Helpers;
public class InputManager(OutputManager outputManager)
{
    private readonly OutputManager _outputManager = outputManager;
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
            (result < 0 && (!cancel || result != -1))
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
    public T? PaginateList<T>(List<T> list, string? tType = null, string? purpose = null, bool allowSelection = false, bool clearScreen = true)
    {
        const int pageSize = 8;
        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(list.Count / (double)pageSize);

        if (clearScreen) _outputManager.Clear();

        while (true)
        {
            _outputManager.WriteLine("=== List ===", ConsoleColor.Cyan);

            var pageItems = list.Skip(currentPage * pageSize).Take(pageSize).ToList();

            for (int i = 0; i < pageItems.Count; i++)
            {
                _outputManager.Write($"[{i + 1}] ", ConsoleColor.DarkGray);

                switch (pageItems[i])
                {
                    case Item item:
                        ColorfulToStringHelper.ColorItemString(item, _outputManager);
                        break;
                    case Player player:
                        ColorfulToStringHelper.ColorPlayerOutput(player, _outputManager);
                        break;
                    case Room room:
                        ColorfulToStringHelper.ColorRoomOutput(room, _outputManager);
                        break;
                    case Monster monster:
                        ColorfulToStringHelper.ColorMonsterOutput(monster, _outputManager);
                        break;
                    case Skill skill:
                        ColorfulToStringHelper.ColorSkillOutput(skill, _outputManager);
                        break;
                    case Archetype archetype:
                        ColorfulToStringHelper.ColorArchetypeOutput(archetype, _outputManager);
                        break;
                    default:
                        _outputManager.WriteLine(pageItems[i]?.ToString() ?? "Unknown", ConsoleColor.Cyan);
                        break;
                }

                _outputManager.WriteLine();
            }

            var pageInfo = $"{currentPage + 1} of {totalPages}";
            if (totalPages > 1)
            {
                if (currentPage > 0) pageInfo = "<-- " + pageInfo;
                if (currentPage < totalPages - 1) pageInfo += " -->";
            }
            _outputManager.WriteLine($"\n{pageInfo}", ConsoleColor.DarkGreen);

            string prompt = "\n";
            if (totalPages > 1)
                prompt += "Use ←/→ arrows to navigate pages. ";
            if (allowSelection)
                prompt += $"Enter number (1-{pageItems.Count}) of {tType} to {purpose}. ";
            prompt += "Press 'q' to quit.\n";

            _outputManager.WriteLine(prompt);
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
}
