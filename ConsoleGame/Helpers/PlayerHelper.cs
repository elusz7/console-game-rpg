using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers;

public class PlayerHelper(
    IInputManager inputManager,
    IOutputManager outputManager,
    IPlayerDao playerDao,
    IArchetypeDao archetypeDao)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IPlayerDao _playerDao = playerDao;
    private readonly IArchetypeDao _archetypeDao = archetypeDao;

    public Player? InitializePlayer(bool campaign)
    {
        if (campaign)
        {
            return CreateCharacter();
        }
        else
        {
            return SelectCharacter();
        }
    }

    private Player? CreateCharacter()
    {
        var name = _inputManager.ReadString("\nWhat is the name of your character? ");
        _outputManager.WriteLine();

        var archetypes = _archetypeDao.GetAllArchetypes();

        var archetype = _inputManager.Selector(
            archetypes,
            a => ColorfulToStringHelper.ArchetypeToString(a),
            $"Select the archetype of {name}",
            a => ColorfulToStringHelper.GetArchetypeColor(a));

        if (archetype == null)
        {
            return null;
        }

        var player = new Player
        {
            Name = name,
            ArchetypeId = archetype.Id,
            Archetype = archetype
        };

        player.Initialize();

        return player;
    }

    private Player? SelectCharacter()
    {
        var availableCharacters = _playerDao.GetAllPlayers();

        var selection = SelectPlayer(
            "Select a character to play with",
            availableCharacters);

        if (selection == null)
            return null;

        var player = _playerDao.GetPlayerByIdNoTracking(selection.Id);

        if (player == null)
            return null;

        player.RebuildEquipment();
        LevelUpPlayer(player);

        return player;

        // var dbPlayer = _playerDao.GetPlayerByName("Kaenji Stormleaf") ??
        //                throw new InvalidDataException("No player found");
        // var player = _playerDao.GetPlayerByIdNoTracking(dbPlayer.Id);
        //
        // LevelUpPlayer(player);
        //
        // return player;
    }

    private Player? SelectPlayer(string prompt, List<Player> players)
    {
        return _inputManager.Selector(
            players,
            p => ColorfulToStringHelper.PlayerToString(p),
            prompt,
            i => ColorfulToStringHelper.GetArchetypeColor(i.Archetype)
        );
    }

    private static void LevelUpPlayer(Player player)
    {
        for (var level = 2; level <= player.Level; level++)
        {
            player.MaxHealth += player.Archetype.LevelUp(level);
        }

        foreach (var skill in player.Archetype.Skills)
        {
            skill.InitializeSkill(player.Level);
        }

        player.CurrentHealth = player.MaxHealth;
        player.Archetype.CurrentResource = player.Archetype.MaxResource;
    }

    public void SavePlayer(Player player)
    {
        // Find the existing player, including their tracked inventory and items.
        var dbPlayer = _playerDao.GetPlayerByName(player.Name);

        // If this is a new player, insert the complete player graph.
        if (dbPlayer == null)
        {
            _playerDao.AddPlayer(player);
            return;
        }

        // Copy the player's current progression and stats.
        dbPlayer.Level = player.Level;
        dbPlayer.MaxHealth = player.MaxHealth;

        // Synchronize inventory properties.
        dbPlayer.Inventory.Capacity = player.Inventory.Capacity;
        dbPlayer.Inventory.Gold = player.Inventory.Gold;

        // Remove saved items that are no longer in the current inventory.
        var itemsToRemove = dbPlayer.Inventory.Items
            .Where(savedItem => !player.Inventory.Items.Any(currentItem => currentItem.Id == savedItem.Id))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            dbPlayer.Inventory.Items.Remove(item);
        }

        // Add new items and retain existing items.
        foreach (var currentItem in player.Inventory.Items)
        {
            // Only match items that already have a database ID.
            // New items have ID 0 until EF Core inserts them.
            var savedItem = currentItem.Id > 0
                ? dbPlayer.Inventory.Items
                    .FirstOrDefault(item => item.Id == currentItem.Id)
                : null;

            if (savedItem == null)
            {
                // New items are added to the existing tracked inventory.
                currentItem.Inventory = dbPlayer.Inventory;
                currentItem.InventoryId = dbPlayer.Inventory.Id;
                dbPlayer.Inventory.Items.Add(currentItem);
            }
            else
            {
                // Copy gameplay changes to the existing tracked item.
                savedItem.Name = currentItem.Name;
                savedItem.Value = currentItem.Value;
                savedItem.Description = currentItem.Description;
                savedItem.Durability = currentItem.Durability;
                savedItem.Weight = currentItem.Weight;
                savedItem.RequiredLevel = currentItem.RequiredLevel;
            }
        }

        // Persist the updated tracked player graph.
        _playerDao.UpdatePlayer(dbPlayer);
    }
}