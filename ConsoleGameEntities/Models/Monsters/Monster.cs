using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;
public abstract class Monster : IMonster, ITargetable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    [NotMapped]
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int AggressionLevel { get; set; }
    public string MonsterType { get; set; }
    public ICollection<Skill>? Skills { get; set; } = new List<Skill>();

    public int? RoomId { get; set; }
    public Room? Room { get; set; }
    public int DefensePower { get; set; }
    public int AttackPower { get; set; }
    public int Resistance { get; set; }

    public abstract void Attack(ITargetable target);

    public virtual void TakeDamage(int damage, SkillTypeEnum? skillType = null)
    {
        MaxHealth -= damage;

        if (MaxHealth <= 0)
        {
            Console.WriteLine($"{Name} has been defeated!");
        }
    }
    public void Heal(int regainedHealth) { }

    public int GetStat(StatType stat) { return 0; }
    public void BoostStat(StatType stat, int power, int? original = null) { }
    public void ReduceStat(StatType stat, int power, int? original = null) { }
}
