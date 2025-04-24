using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame.Helpers;

public class RoomManager(InputManager inputManager, OutputManager outputManager, MapManager mapManager)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly MapManager _mapManager = mapManager;
}
