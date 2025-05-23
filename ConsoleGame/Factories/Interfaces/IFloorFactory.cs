using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.Models;

namespace ConsoleGame.Factories.Interfaces;

public interface IFloorFactory
{
    Floor CreateFloor(int level, bool campaign, bool randomMap);
}
