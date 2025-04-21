using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameEntities.Models.Items;

public class Armor : Item
{
    public int DefensePower { get; set; }

    public override int Use()
    {
        if (Durability > 0)
        {
            Durability--;
            return DefensePower;
        }
        else
        {
            return -1;
        }
    }

    public override string ToString()
    {
        return $"{Name} (Defense: {DefensePower}, Durability: {Durability})";
    }
}
