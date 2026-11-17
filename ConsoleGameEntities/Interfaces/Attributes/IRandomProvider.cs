namespace ConsoleGameEntities.Interfaces.Attributes;

public interface IRandomProvider
{
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
}