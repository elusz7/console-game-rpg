namespace ConsoleGame.Helpers;
public class InputManager(OutputManager outputManager)
{
    private readonly OutputManager _outputManager = outputManager;
    public ConsoleKeyInfo ReadKey()
    {
        return Console.ReadKey();
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
}
