using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Managers.Interfaces;

public interface IInputManager
{
    ConsoleKeyInfo ReadKey();
    int ReadMenuKey(int maxValue);
    int ReadInt(string prompt, int? max = null, bool cancel = false);
    decimal ReadDecimal(string prompt);
    string ReadString(string prompt, string[]? validation = null);
    T? Selector<T>(List<T> items, Func<T, string> displayTextSelector, string prompt, Func<T, ConsoleColor>? colorSelector = null);
    T? Viewer<T>(List<T> items, Func<T, string> displayTextSelector, string prompt, Func<T, ConsoleColor>? colorSelector = null);
    Item? SelectItem(string prompt, List<Item> items, string? purpose = null);
    bool LoopAgain(string action);
    bool ConfirmAction(string action);
    T GetEnumChoice<T>(string prompt) where T : Enum;
    int DisplayEditMenu(List<string> properties);
}
