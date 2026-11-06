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
    void UpdateInventory(Inventory inventory);

    List<Item> GetAvailableItems(Player player);
}