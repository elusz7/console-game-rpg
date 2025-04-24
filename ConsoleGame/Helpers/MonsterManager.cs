using ConsoleGameEntities.Data;

namespace ConsoleGame.Helpers;

public class MonsterManager(GameContext context, InputManager inputManager, OutputManager outputManager)
{
    private readonly GameContext _context = context;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
}
