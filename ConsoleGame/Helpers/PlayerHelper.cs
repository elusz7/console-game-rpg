using System.Security.Cryptography.X509Certificates;
using ConsoleGame.GameDao;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers;
using ConsoleGameEntities.Main.Models.Entities;
using ConsoleGameEntities.Main.Models.Items;

namespace ConsoleGame.Helpers;

public class PlayerHelper(InputManager inputManager, OutputManager outputManager, PlayerDao playerDao, ArchetypeDao archetypeDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly PlayerDao _playerDao = playerDao;
    private readonly ArchetypeDao _archetypeDao = archetypeDao;

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
        var name = _inputManager.ReadString("What is the name of your character? ");
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

        if (availableCharacters.Count == 0)
        {
            throw new InvalidOperationException("No characters available for adventuring.");
        }

        var selection = SelectPlayer("Select a character to play with", availableCharacters);

        if (selection == null)
        {
            return null;
        }

        var player = _playerDao.GetPlayerNoTracking(selection);

        LevelUpPlayer(player);

        return player;
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
        if (player.Level > 1)
        {
            for (int i = 2; i <= player.Level; i++)
                player.Archetype.LevelUp(i); //all archetypes start at a base of level 1
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
        var dbPlayer = _playerDao.GetPlayerByName(player.Name);
        
        SellInventory(player);
        player.RoomId = null;
        player.CurrentRoom = null;

        if (dbPlayer == null)
        {
            _playerDao.AddPlayer(player);
        }
        else
        {
            _playerDao.UpdatePlayer(dbPlayer);
        }
    }

    private static void SellInventory(Player player)
    {
        var equipment = player.Equipment.ToList();
        foreach (var equippedItem in equipment)
        {
            player.Unequip(equippedItem);
        }
        
        var inventory = player.Inventory.Items.ToList();
        foreach (var item in inventory)
        {
            player.Sell(item);
        }
    }
}
