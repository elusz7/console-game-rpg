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
    Player? GetPlayerByIdNoTracking(int playerId);
    List<Player> GetAllPlayers(string name);
    List<Player> GetAllPlayers();
    List<Archetype> GetDistinctPlayerArchetypes();
    List<Player> GetPlayersByArchetypeId(int archetypeId);
    List<Player> GetPlayersByLevel(int level);
}