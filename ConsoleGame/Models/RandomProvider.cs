using ConsoleGame.Models.Attributes;

namespace ConsoleGame.Models;

public class RandomProvider : IRandomProvider
{
    public int Next(int maxValue)
    {
        return Random.Shared.Next(maxValue);
    }
    
    public int Next(int minValue, int maxValue)
    {
        return Random.Shared.Next(minValue, maxValue);
    }
}