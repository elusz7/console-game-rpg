using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGame.Helpers.CombatHelpers;

/// <summary>
/// Collects, orders, and displays pending combat action messages.
/// </summary>
public class CombatActionDisplayHelper(IOutputManager outputManager)
{
    private readonly IOutputManager _outputManager = outputManager;

    /// <summary>
    /// Displays pending action messages from the player and monsters
    /// in chronological order, then clears their message collections.
    /// </summary>
    public void DisplayActionItems(Player player, List<Monster> monsters)
    {
        // SortedList orders messages by timestamp and avoids duplicate keys.
        var actions = new SortedList<long, string>();

        // Collect player action messages.
        foreach (var kvp in player.ActionItems)
        {
            long key = kvp.Key;
            while (!actions.TryAdd(key, kvp.Value))
                key++;
        }

        // Collect monster action messages.
        foreach (var monster in monsters)
        {
            foreach (var kvp in monster.ActionItems)
            {
                long key = kvp.Key;
                while (!actions.TryAdd(key, kvp.Value))
                    key++;
            }
        }

        // Display all collected messages in timestamp order.
        foreach (var action in actions)
        {
            _outputManager.WriteLine(action.Value, ConsoleColor.Cyan);
        }

        // Clear messages after they have been displayed.
        player.ClearActionItems();

        foreach (var monster in monsters)
        {
            monster.ClearActionItems();
        }
    }
}