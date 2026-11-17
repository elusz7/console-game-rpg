using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Entities;

public class RandomProvider : IRandomProvider
{
    private static readonly Random Random = Random.Shared;
    
    public int Next()
    {
        return Random.Next();
    }
    public int Next(int maxValue)
    {
        return Random.Next(maxValue);
    }
    
    public int Next(int minValue, int maxValue)
    {
        return Random.Next(minValue, maxValue);
    }

    public double NextDouble()
    {
        return Random.NextDouble();
    }
}