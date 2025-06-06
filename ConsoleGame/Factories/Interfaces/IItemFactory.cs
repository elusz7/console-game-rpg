﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories.Interfaces;

public interface IItemFactory
{
    (List<Item>, int) GenerateLoot(int level, int numMonsters, bool campaign);
    List<Item> CreateMerchantItems(int level);
    List<Item> GetMerchantGifts(ArchetypeType type, int level);
}
