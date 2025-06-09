using ConsoleGameEntities.Models.Runes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IRuneDao
{
    Rune FuseRune(Rune rune);
}
