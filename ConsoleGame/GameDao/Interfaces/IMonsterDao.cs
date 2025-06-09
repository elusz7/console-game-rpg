using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao.Interfaces;

public interface IMonsterDao
{
    void AddMonster(Monster monster);

    List<Monster> GetAllMonsters();

    List<Monster> GetMonstersByName(string name);

    void UpdateMonster(Monster monster);

    void DeleteMonster(Monster monster);

    List<Monster> GetAllNonCoreMonsters();

    List<Monster> GetMonstersByThreatLevel(ThreatLevel threatLevel);

    List<Monster> GetMonstersByDamageType(DamageType damageType);

    List<Monster> GetNonBossMonstersByMaxLevel(int level);

    List<Monster> GetMonstersByMaxLevelAndThreatLevel(int level, int threatLevel);

    List<MonsterDrop> GetMonsterDrops(Monster monster);
}
