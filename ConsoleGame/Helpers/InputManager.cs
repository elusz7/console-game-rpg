namespace ConsoleGame.Helpers;
public class InputManager(OutputManager outputManager)
{
    private readonly OutputManager _outputManager = outputManager;
    public void ReadKey()
    {
        Console.ReadKey();
    }
    public int ReadInt(string prompt, int? max = null, bool cancel = false)
    {
        _outputManager.Write(prompt);
        _outputManager.Display();

        string input = Console.ReadLine().Trim();
        bool isInt = int.TryParse(input, out int result);

        while (
            !isInt ||
            (max.HasValue && result > max.Value) ||
            (result < 0 && !cancel)
        )
        {
            _outputManager.Write("Invalid input. Please enter a valid, positive integer: ");
            _outputManager.Display();
            input = Console.ReadLine().Trim();
            isInt = int.TryParse(input, out result);
        }

        return result;
    }

    public decimal ReadDecimal(string prompt)
    {
        _outputManager.Write(prompt);
        _outputManager.Display();

        string input = Console.ReadLine().Trim();
        bool isDecimal = decimal.TryParse(input, out decimal result);

        while (!isDecimal && result < 0.0M)
        {
            _outputManager.Write("Invalid input. Please enter a valid, positive decimal number: ");
            _outputManager.Display();
            input = Console.ReadLine().Trim();
            isDecimal = decimal.TryParse(input, out result);
        }

        return result;
    }
    public string ReadString(string prompt, string[]? validation = null)
    {
        _outputManager.Write(prompt);
        _outputManager.Display();

        string input = Console.ReadLine().Trim();

        if (validation != null)
        {
            while (!validation.Any(v => string.Equals(v, input, StringComparison.OrdinalIgnoreCase)))
            {
                _outputManager.Write("Invalid input. Please try again: ");
                _outputManager.Display();
                input = Console.ReadLine().Trim();
            }
        }

        return input;
    }
}
