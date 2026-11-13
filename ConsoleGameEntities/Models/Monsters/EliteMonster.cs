using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;

public sealed class EliteMonster : Monster
{
    public EliteMonster()
    {
    }

    public EliteMonster(IMonsterSkillSelector skillSelector, MonsterBehaviorType behavior)
    {
        Strategy = new EliteStrategy(skillSelector, behavior);
    }
}