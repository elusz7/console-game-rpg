using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Models;

public class GameSession
{
    public Player Player { get; }
    public Floor CurrentFloor { get; private set; }
    public bool IsCampaign { get; }
    public AdventureStatus Status { get; private set; }

    // A floor is cleared when no monsters on it are still alive.
    public bool IsFloorCleared =>
        !CurrentFloor.Monsters.Any(monster => monster.CurrentHealth > 0);

    public GameSession(Player player, Floor currentFloor, bool isCampaign)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        CurrentFloor = currentFloor ?? throw new ArgumentNullException(nameof(currentFloor));
        IsCampaign = isCampaign;
        Status = AdventureStatus.Running;
    }

    public void SetCurrentFloor(Floor floor)
    {
        CurrentFloor = floor ?? throw new ArgumentNullException(nameof(floor));
    }

    public void End(AdventureStatus status)
    {
        if (status == AdventureStatus.Running ||
            status == AdventureStatus.NotStarted)
        {
            throw new ArgumentException("Invalid ending status.", nameof(status));
        }

        Status = status;
    }
}

public enum AdventureStatus
{
    NotStarted,
    Running,
    PlayerDead,
    Completed,
    Quit
}