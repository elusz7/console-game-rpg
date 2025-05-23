using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao.Interfaces;

public interface IPlayerDao
{
    void AddPlayer(Player player);
    void UpdatePlayer(Player player);
    void DeletePlayer(Player player);
    Player? GetPlayerByName(string name);
    Player GetPlayerNoTracking(Player player);
    List<Player> GetAllPlayers(string name);
    List<Player> GetAllPlayers();
    List<Archetype> GetAllPlayerArchetypes();
    List<Player> GetAllPlayersByArchetype(Archetype archetype);
    List<Player> GetAllPlayersByLevel(int level);
}
