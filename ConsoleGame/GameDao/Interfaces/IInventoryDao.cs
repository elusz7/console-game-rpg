using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao.Interfaces;

public interface IInventoryDao
{
    List<Player> GetAllPlayers();

    void UpdateInventory(Inventory inventory);

    List<Item> GetEquippableItems(Player player);
}
