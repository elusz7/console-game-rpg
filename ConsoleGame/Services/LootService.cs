using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces;

namespace ConsoleGame.Services;

public class LootService
{
    // Transfers a monster's treasure and removes the monster
    // from the room and floor after the attempt.
    public void LootMonster(GameSession session, IMonster monster)
    {
        // Validate inputs before accessing game state.
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(monster);

        try
        {
            // Let Player handle treasure validation and inventory transfer.
            session.Player.Loot(monster);
        }
        finally
        {
            // Remove the same monster instance from the room.
            session.Player.CurrentRoom?.Monsters.RemoveAll(
                existingMonster => ReferenceEquals(existingMonster, monster));

            // Keep the floor's monster collection synchronized.
            session.CurrentFloor.Monsters.RemoveAll(
                existingMonster => ReferenceEquals(existingMonster, monster));
        }
    }
}