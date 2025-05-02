using System.Text;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleGameEntities.Models.Entities;

public class Player : ITargetable, IPlayer
{
    public int Experience { get; set; }
    public int Level { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    [NotMapped]
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int ArchetypeId { get; set; }
    public virtual Archetype Archetype { get; set; }
    public virtual Inventory Inventory { get; set; }
    public int? RoomId { get; set; }
    public virtual Room? CurrentRoom { get; set; }
    
    public void Attack(ITargetable target) { }
    public void TakeDamage(int damage, SkillTypeEnum? skillType) { }
    public void Heal(int regainedHealth) { }
    public void LevelUp()
    {
        Level++;
        int boost = Archetype.LevelUp(Level);
        MaxHealth += boost;
        CurrentHealth = MaxHealth;
        Inventory.Capacity += (decimal)boost;
    }
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append("Name: ");
        builder.Append(Name);

        builder.Append(", Health: ");
        builder.Append(MaxHealth);

        builder.Append(", Experience: ");
        builder.Append(Experience);

        builder.Append(", Capacity: ");
        builder.Append(Inventory?.Capacity.ToString() ?? "N/A");

        builder.Append(", Gold: ");
        builder.Append(Inventory?.Gold.ToString() ?? "N/A");

        builder.Append("\n\tInventory: ");
        builder.Append(Inventory?.Items != null && Inventory.Items.Any()
            ? string.Join(", ", Inventory.Items.Select(i => i.Name))
            : "Inventory is empty currently.");

        return builder.ToString();
    }

    public int GetStat(StatType stat) { return 0; }
    public void BoostStat(StatType stat, int power, int? original = null) { }

    public void ReduceStat(StatType stat, int power, int? original = null) { }

    public void ModifyStat(StatType stat, int amount)
    {
        throw new NotImplementedException();
    }
}
