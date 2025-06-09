using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers;

public class PlayerHelper(IInputManager inputManager, IOutputManager outputManager,
    IPlayerDao playerDao, IArchetypeDao archetypeDao)
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
        /*var availableCharacters = _playerDao.GetAllPlayers();

        if (availableCharacters.Count == 0)
        {
            throw new InvalidOperationException("No characters available for adventuring.");
        }

        var selection = SelectPlayer("Select a character to play with", availableCharacters);

        if (selection == null)
        {
            return null;
        }
        
        var player = _playerDao.GetPlayerNoTracking(selection);*/

        var dbPlayer = _playerDao.GetPlayerByName("Kaenji Stormleaf") ?? throw new InvalidDataException("No player found");
        var player = _playerDao.GetPlayerNoTracking(dbPlayer);

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
            dbPlayer.Level = player.Level;
            dbPlayer.Inventory.Capacity = player.Inventory.Capacity;
            dbPlayer.Inventory.Gold = player.Inventory.Gold;
            dbPlayer.MaxHealth = player.MaxHealth;
            dbPlayer.Inventory.Items.Clear();

            _playerDao.UpdatePlayer(dbPlayer);
        }
    }

    private static void SellInventory(Player player)
    {
        var equipment = player.Equipment.OfType<Item>().ToList();
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
