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
    public T? PaginateList<T>(List<T> list, Func<T, string> formatter, string? tType = null, string? purpose = null, bool allowSelection = false)
    {
        int pageSize = 9;
        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(list.Count / (double)pageSize);

        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine("=== List ===", ConsoleColor.Cyan);

            var pageItems = list.Skip(currentPage * pageSize).Take(pageSize).ToList();

            for (int i = 0; i < pageItems.Count; i++)
            {
                if (i % 2 == 1)
                    _outputManager.WriteLine($"{i + 1}. {formatter(pageItems[i])}", ConsoleColor.Yellow);
                else
                    _outputManager.WriteLine($"{i + 1}. {formatter(pageItems[i])}", ConsoleColor.Blue);
            }

            var pageInfo = $"{currentPage + 1} of {totalPages}";

            if (totalPages > 1)
            {
                if (currentPage > 0) pageInfo = " <-- " + pageInfo;
                if (currentPage < totalPages - 1) pageInfo += " -->";
            }
            pageInfo = "\n" + pageInfo;

            _outputManager.WriteLine(pageInfo, ConsoleColor.DarkGreen);

            
            var prompt = "\nUse arrow keys to navigate pages, ";

            if (allowSelection)
            {
                prompt += $"Enter number of {tType} to {purpose}, ";
            }
            prompt += "or 'q' to quit.\n";


            _outputManager.WriteLine(prompt);
            _outputManager.Display();

            while (true)
            {
                var key = ReadKey();

                if ((key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.RightArrow) && (currentPage < totalPages - 1))
                {
                    currentPage++;
                    break;
                }
                else if ((key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.LeftArrow) && (currentPage > 0))
                {
                    currentPage--;
                    break;
                }
                else if (key.Key == ConsoleKey.Q)
                {
                    _outputManager.WriteLine();
                    return default;
                }
                else if (allowSelection && char.IsDigit(key.KeyChar))
                {
                    int selection = int.Parse(key.KeyChar.ToString());
                    if (selection > 0 && selection <= pageItems.Count)
                    {
                        _outputManager.WriteLine();
                        return pageItems[selection - 1];
                    }
                }
                else
                {
                    _outputManager.WriteLine("Invalid input. Please try again.", ConsoleColor.Red);
                    _outputManager.Display();
                }
            }
        }
    }
}
