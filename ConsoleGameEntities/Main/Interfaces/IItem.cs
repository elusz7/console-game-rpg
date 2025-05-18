using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Main.Models.Entities;
using ConsoleGameEntities.Main.Models.Monsters;

namespace ConsoleGameEntities.Main.Interfaces;

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

    void Purify();
    void Equip();
    void Unequip();
    void Use();
    void Reforge();
    void Enchant();
    bool IsEquipped();
    void RecoverDurability(int power);
    void CalculateStatsByLevel();
    void CalculateLevelByStats();
    void CalculateValue(bool calculateValuable);
    public decimal GetBuyPrice();
    public decimal GetSellPrice();
    public decimal GetPurificationPrice();
    public decimal GetReforgePrice();
    public decimal GetEnchantmentPrice();
}
