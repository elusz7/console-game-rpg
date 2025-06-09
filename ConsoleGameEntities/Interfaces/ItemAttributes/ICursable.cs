namespace ConsoleGameEntities.Interfaces.ItemAttributes;

public interface ICursable
{
    void Curse();
    void Purify();
    void TargetCursed();
    bool IsCursed();
    decimal GetPurificationPrice();
}
