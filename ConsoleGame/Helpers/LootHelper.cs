using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.Helpers;

public class LootHelper(IMonsterDao monsterDao, IOutputManager outputManager)
{
    private readonly IMonsterDao _monsterDao = monsterDao;
    private readonly IOutputManager _outputManager = outputManager;

    public void LootMonster(Monster monster, Player player)
    {
        if (!monster.Looted)
            IngredientLoot(monster, player);
        monster.Looted = true; // Mark monster as looted after ingredient loot

        TreasureLoot(monster, player);
    }

    private void IngredientLoot(Monster monster, Player player)
    {
        var potentialLoot = _monsterDao.GetMonsterDrops(monster);
        var actualLoot = new List<Ingredient>();

        foreach (var loot in potentialLoot)
        {
            var dropRate = (double)(loot.DropRate / 100.0M); // Convert percentage to a decimal
            if (dropRate > new Random().NextDouble())
            {
                actualLoot.Add(loot.Ingredient);
            }
        }

        if (actualLoot.Count == 0)
        {
            _outputManager.WriteLine("\nYou weren't able to salvage anything of the monster.", ConsoleColor.Red);
            return;
        }
        else
        {
            var itemNames = actualLoot.Select(i => i.Name).ToList();
            _outputManager.WriteLine($"You found {string.Join(", ", itemNames)} on {monster.Name}!", ConsoleColor.Blue);
            player.Loot(actualLoot);
        }
    }

    private void TreasureLoot(Monster monster, Player player)
    {
        try
        {
            player.Loot(monster);

            _outputManager.WriteLine();
            foreach (var kvp in player.Logger.GetOrderedLog())
            {
                _outputManager.WriteLine($"{kvp.Value}", ConsoleColor.Magenta);
            }
            player.Logger.Clear();
        }
        catch (TreasureException ex)
        {
            _outputManager.WriteLine($"\n{ex.Message}", ConsoleColor.Red);
        }
    }
}
