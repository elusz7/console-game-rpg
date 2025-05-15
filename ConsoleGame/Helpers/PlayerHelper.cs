using ConsoleGame.GameDao;
using ConsoleGame.Managers;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers;

public class PlayerHelper(InputManager inputManager, OutputManager outputManager, PlayerDao playerDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly PlayerDao _playerDao = playerDao;

    public Player? InitializePlayer(bool campaign)
    {
        if (campaign)
        {
            //generate a new player
            return default;
        }
        else
        {
            return SelectCharacter();
        }
    }

    private Player? SelectCharacter()
    {
        var availableCharacters = _playerDao.GetAllPlayers();

        if (availableCharacters.Count == 0)
        {
            throw new InvalidOperationException("No characters available for adventuring.");
        }

        var selection = SelectPlayer("\n\tSelect a character to play with:", availableCharacters);

        if (selection == null)
        {
            return null;
        }

        var player = _playerDao.GetPlayer(selection);

        LevelUpPlayer(player);

        return player;
    }

    private Player? SelectPlayer(string prompt, List<Player> players)
    {
        return _inputManager.SelectFromList(
            players,
            p => $"{p.Name} [Level {p.Level} {p.Archetype.Name}]",
            prompt,
            i => GetPlayerColor(i)
        );
    }
    private static ConsoleColor GetPlayerColor(Player player)
    {
        return player.Archetype.Name switch
        {
            "Warrior" => ConsoleColor.Red,
            "Bruiser" => ConsoleColor.Magenta,
            "Rogue" => ConsoleColor.Blue,
            "Mage" => ConsoleColor.Green,
            _ => ConsoleColor.Yellow
        };
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
            skill.InitializeSkill();
        }

        player.CurrentHealth = player.MaxHealth;
        player.Archetype.CurrentResource = player.Archetype.MaxResource;
    }
}
