using ConsoleGame.Helpers;
using ConsoleGame.Models;

namespace ConsoleGame.Services;

public class CampaignService(PlayerHelper playerHelper)
{
    // Handles the existing player-saving operation.
    private readonly PlayerHelper _playerHelper =
        playerHelper ?? throw new ArgumentNullException(nameof(playerHelper));

    // Saves campaign progress only when the session has ended.
    public void SaveIfCampaign(GameSession session)
    {
        // Ensure a valid session was provided.
        ArgumentNullException.ThrowIfNull(session);

        // Adventure sessions are temporary and should never be saved.
        if (!session.IsCampaign)
        {
            return;
        }

        // Only persist sessions that have actually ended.
        if (session.Status is not (AdventureStatus.Quit or AdventureStatus.PlayerDead))
        {
            return;
        }

        // Delegate the actual save operation to PlayerHelper.
        _playerHelper.SavePlayer(session.Player);
    }
}