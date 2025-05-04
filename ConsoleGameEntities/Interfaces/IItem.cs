using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameEntities.Interfaces;

public interface IItem
{
    int Id { get; set; }
    string Name { get; set; }
    decimal Value { get; set; }
    string Description { get; set; }
    int Durability { get; set; }
    decimal Weight { get; set; }
    bool IsCursed { get; set; }
    int RequiredLevel { get; set; }

    Inventory? Inventory { get; set; }
    Monster? Monster { get; set; }

    void Use();
    void Purify();
    void Sell();
    void Equip();
    void Unequip();
    bool IsEquipped();
    void RecoverDurability(int power);
}
