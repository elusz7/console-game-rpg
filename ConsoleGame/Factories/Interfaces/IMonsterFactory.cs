using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGame.Factories.Interfaces;

public interface IMonsterFactory
{
    List<Monster> GenerateMonsters(int level, bool campaign);
}
