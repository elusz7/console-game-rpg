using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Services;

public class LootService(IIngredientDao ingredientDao, IMonsterDropDao monsterDropDao, ITransactionManager transactionManager)
{
    private readonly IIngredientDao _ingredientDao = ingredientDao;
    private readonly IMonsterDropDao _monsterDropDao = monsterDropDao;
    private readonly ITransactionManager _transactionManager = transactionManager;
    
    // Transfers a monster's treasure and removes the monster
    // from the room and floor after the attempt.
    public void LootMonster(GameSession session, IMonster monster)
    {
        // Validate inputs before accessing game state.
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(monster);

        if (monster.CurrentHealth >= 1)
            throw new TreasureException(
                $"You think it might be better to defeat {monster.Name} before trying to take its treasure.");
        
        try
        {
            if (!monster.IngredientsLooted)
            {
                LootIngredientsFromMonster(monster, session.Player.Id);
                monster.IngredientsLooted = true;
            }

            if (!monster.TreasureLooted)
            {
                if (monster.Treasure is null)
                {
                    monster.TreasureLooted = true;
                }
                else
                {
                    session.Player.Loot(monster);
                    monster.TreasureLooted = true;
                }
            }
        }
        finally
        {
            if (monster is { TreasureLooted: true, IngredientsLooted: true })
            {
                session.Player.CurrentRoom?.Monsters.RemoveAll(existingMonster =>
                    ReferenceEquals(existingMonster, monster));

                session.CurrentFloor.Monsters.RemoveAll(existingMonster =>
                    ReferenceEquals(existingMonster, monster));
            }
        }
    }

    private void LootIngredientsFromMonster(IMonster monster, int playerId)
    {
        if (monster.AttackElement is not { } element)
            return;

        var monsterDrops = _monsterDropDao.GetDropsByElementTypeAndThreatLevel(element, monster.ThreatLevel);

        if (monsterDrops.Count == 0)
            return;

        _transactionManager.ExecuteInTransaction(() =>
        {
            foreach (var drop in monsterDrops)
            {
                var dropChance = Math.Min(100.0, (double)drop.DropRate + monster.Level * 2.0);

                if (Random.Shared.NextDouble() * 100 < dropChance)
                {
                    _ingredientDao.AddIngredientToPlayer(drop.IngredientId, playerId);
                }
            }
        });
    }
}