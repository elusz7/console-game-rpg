using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;

public class EliteMonster : Monster
{
    [NotMapped]
    public override double DodgeChance { get; set; } = 0.02; // 2% base dodge chance for elite monsters
    public EliteMonster() { }
    public EliteMonster(IMonsterSkillSelector skillSelector, MonsterBehaviorType behavior)
    {
        Strategy = new EliteStrategy(skillSelector, behavior);
    }
}
