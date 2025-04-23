namespace ConsoleGame.Helpers;
public class InputManager
{
    public int ReadInt()
    {
        string input = Console.ReadLine().Trim();
        if (int.TryParse(input, out int result))
        {
            return result;
        }
        else
        {
            return -1;
        }
    }

    public decimal ReadDecimal()
    {
        string input = Console.ReadLine().Trim();
        if (decimal.TryParse(input, out decimal result))
        {
            return result;
        }
        else
        {
            return -1;
        }
    }
    public string ReadString()
    {
        string input = Console.ReadLine();
        return input.Trim();
    }
}
