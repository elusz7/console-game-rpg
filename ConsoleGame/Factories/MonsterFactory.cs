using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public static class MonsterFactory
{
    public static Monster CreateMonster(string name, int level, MonsterBehaviorType behavior)
    {
        var monster = new Monster
        {
            Name = name,
            Level = level,
            Strategy = Monster.GetStrategyFor(behavior),
        };

        return monster;
    }
}

