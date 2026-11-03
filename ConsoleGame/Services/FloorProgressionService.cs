using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Services;

public class FloorProgressionService(IFloorFactory floorFactory)
{
    // Factory used to generate the next floor.
    private readonly IFloorFactory _floorFactory =
        floorFactory ?? throw new ArgumentNullException(nameof(floorFactory));

    // Returns the items successfully collected so AdventureService can display them.
    public List<Item> AdvanceToNextFloor(GameSession session, bool generateRandomMap)
    {
        //Ensure that session exists
        ArgumentNullException.ThrowIfNull(session);

        // Keep a reference to the current player and floor.
        var player = session.Player;
        var currentFloor = session.CurrentFloor;

        // Remember whether the player has already met the merchant.
        // This progress carries over to the next floor.
        bool merchant = currentFloor.HasMetMerchant;

        // Level up the player before generating the next floor.
        player.LevelUp();

        // Collect any treasure remaining on the current floor.
        var collectedItems = AutomaticLoot(player, currentFloor);

        // Generate the next floor using the player's updated level.
        var nextFloor = _floorFactory.CreateFloor(
            player.Level,
            session.IsCampaign,
            generateRandomMap);

        // Update the session through its controlled state-changing method.
        session.SetCurrentFloor(nextFloor);

        // Preserve merchant progress between floors.
        nextFloor.HasMetMerchant = merchant;

        // Place the player at the entrance of the new floor.
        player.CurrentRoom = nextFloor.GetEntrance();

        // Return collected loot to the caller for display.
        return collectedItems;
    }

    private static List<Item> AutomaticLoot(Player player, Floor floor)
    {
        // Only collect monsters that still have treasure.
        var monstersWithLoot = floor.Monsters
            .Where(monster => monster.Treasure != null)
            .ToList();

        var collectedItems = new List<Item>();

        foreach (var monster in monstersWithLoot)
        {
            var item = monster.Treasure!;

            try
            {
                // Add the treasure to the player's inventory.
                player.Inventory.AddItem(item);

                // Clear the monster's treasure only after a successful transfer.
                monster.Treasure = null;
                collectedItems.Add(item);
            }
            catch (InventoryException)
            {
                // If inventory is full, leave the treasure on the monster.
            }
        }

        // Displaying this message remains AdventureService's responsibility.
        // The service simply returns the items successfully collected.
        return collectedItems;
    }
}