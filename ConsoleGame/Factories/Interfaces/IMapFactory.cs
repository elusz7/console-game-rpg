using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Factories.Interfaces;

public interface IMapFactory
{
    List<Room> GenerateMap(int level, bool campaign, bool randomMap);
}
