
using ConsoleGame.Models;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Services;

public class RoomNavigationService
{
    // Moves the player to a connected room in the requested direction.
    public bool ChangeCurrentRoom(GameSession session, string direction)
    {
        // Ensure the session and direction are valid before attempting movement.
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(direction);
        
        // Resolve the destination before changing the player's current room.
        var room = GetRoom(session, direction);

        // Remove monsters that no longer have treasure to collect.
        CleanUpDeadMonsters(session);

        // Leaving the entrance updates the merchant's available inventory.
        if (session.Player.CurrentRoom == session.CurrentFloor.GetEntrance())
        {
            session.CurrentFloor.UpdateMerchantItems();
        }

        // Complete the move by updating the player's current room.
        session.Player.CurrentRoom = room;

        return true;
    }

    // Finds the room connected to the player's current room in the given direction.
    private static Room GetRoom(GameSession session, string direction)
    {
        var connections = session.Player.CurrentRoom?.GetConnections();

        if (connections == null || !connections.TryGetValue(direction, out var room))
        {
            throw new InvalidOperationException(
                $"No room found in direction: {direction}");
        }

        return room!;
    }

    // Removes monsters without treasure from both the current room and the floor.
    public void CleanUpDeadMonsters(GameSession session)
    {
        // Ensure a valid session was provided.
        ArgumentNullException.ThrowIfNull(session);
        
        var currentRoom = session.Player.CurrentRoom;

        if (currentRoom?.Monsters == null)
        {
            return;
        }

        // Find monsters that no longer have treasure to collect.
        var monstersToRemove = currentRoom.Monsters
            .Where(monster => monster.Treasure == null)
            .ToHashSet();

        if (monstersToRemove.Count == 0)
        {
            return;
        }

        // Keep the room and floor monster collections synchronized.
        currentRoom.Monsters.RemoveAll(monstersToRemove.Contains);
        session.CurrentFloor.Monsters.RemoveAll(monstersToRemove.Contains);
    }
}
