using ConsoleGame.Managers.Interfaces;

namespace ConsoleGame.Helpers.CombatHelpers;

/// <summary>
/// Handles the presentation of the player's combat action menu.
/// </summary>
public class CombatMenuDisplayHelper(IOutputManager outputManager)
{
    private readonly IOutputManager _outputManager = outputManager;

    /// <summary>
    /// Displays the available combat options as a numbered menu.
    /// </summary>
    public void DisplayOptions(IEnumerable<string> options)
    {
        var index = 1;

        foreach (var option in options)
        {
            _outputManager.WriteLine(
                $"{index}. {option}",
                ConsoleColor.Yellow);

            index++;
        }
    }
}