using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.EntityFrameworkCore;
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
}
