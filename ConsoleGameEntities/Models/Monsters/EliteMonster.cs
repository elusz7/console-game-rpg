using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;

public class EliteMonster : Monster
{
    public EliteMonster() { }
    public EliteMonster(IMonsterSkillSelector skillSelector, MonsterBehaviorType behavior)
    {
        Strategy = new EliteStrategy(skillSelector, behavior);
    }
}
