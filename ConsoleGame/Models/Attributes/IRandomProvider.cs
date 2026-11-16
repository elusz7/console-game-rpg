namespace ConsoleGame.Models.Attributes;

public interface IRandomProvider
{
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
}