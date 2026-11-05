using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IMonsterDropDao
{
    List<MonsterDrop> GetDropsByElementType(ElementType element);
    List<MonsterDrop> GetDropsByThreatLevel(ThreatLevel level);
    List<MonsterDrop> GetDropsByElementTypeAndThreatLevel(ElementType element, ThreatLevel level);
}
