namespace ConsoleGameEntities.Helpers.Gameplay;

public class ActionLogger
{
    private readonly SortedList<long, string> _log = new();

    public void Log(string message)
    {
        long key = DateTime.Now.Ticks;

        // Prevent duplicate keys
        while (_log.ContainsKey(key)) key++;

        _log.Add(key, message);
    }

    public SortedList<long, string> GetOrderedLog() => new(_log);

    public void Clear() => _log.Clear();
}
