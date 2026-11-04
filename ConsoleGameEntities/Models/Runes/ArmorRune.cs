using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleGameEntities.Models.Runes;

public class ArmorRune : Rune
{
    [NotMapped]
    public virtual EquippedRune? EquippedRune { get; set; }
    
    public int Use(int tier)
    {
        return Power * tier;
    }
}