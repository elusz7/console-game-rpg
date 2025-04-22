using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Data;

namespace ConsoleGame.Helpers;

public class CharacterManager
{
    private readonly GameContext _context;

    public CharacterManager(GameContext context)
    {
        _context = context;
    }
}
