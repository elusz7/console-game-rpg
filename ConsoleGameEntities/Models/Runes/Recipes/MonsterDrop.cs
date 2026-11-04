using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Runes.Recipes;

public class MonsterDrop
{
    public ElementType Element { get; set; }
    public ThreatLevel ThreatLevel { get; set; }
    public int IngredientId { get; set; }
    public virtual Ingredient Ingredient { get; set; }
    public decimal DropRate { get; set; } // Percentage chance, e.g. 5.00 for 5%
}