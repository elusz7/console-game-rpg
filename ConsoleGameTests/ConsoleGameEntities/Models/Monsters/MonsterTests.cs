using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class MonsterTests
{
    private static Monster CreateMonster()
    {
        return new Monster
        {
            Level = 1,
            AttackPower = 100,
            DefensePower = 50,
            Resistance = 30,
            AggressionLevel = 10,
            MaxHealth = 200
        };
    }
    private static Item CreateItem()
    {
        return new Item
        {
            Id = 6,
            Name = "Test Item",
            Value = 56.7M,
            RequiredLevel = 1,
            Durability = 1
        };
    }

    [TestMethod]
    public void SetLevel_SetsExpectedStatRanges_ForThreatLevel()
    {
        var monster = CreateMonster();
        monster.ThreatLevel = ThreatLevel.Medium;
        int level = 5;

        monster.SetLevel(level);

        Assert.AreEqual(level, monster.Level);

        // Based on the ThreatLevel.Medium multipliers:
        Assert.IsTrue(monster.AttackPower >= level * 3 && monster.AttackPower <= level * 6);
        Assert.IsTrue(monster.DefensePower >= level * 2 && monster.DefensePower <= level * 4);
        Assert.IsTrue(monster.Resistance >= level * 2 && monster.Resistance <= level * 4);
        Assert.IsTrue(monster.AggressionLevel >= 2 && monster.AggressionLevel <= 4);
        Assert.IsTrue(monster.MaxHealth >= level * 8 && monster.MaxHealth <= level * 12);
        Assert.AreEqual(monster.MaxHealth, monster.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_ReducesHealth()
    {
        var monster = CreateMonster();
        monster.DefensePower = 0;
        monster.CurrentHealth = monster.MaxHealth;

        monster.TakeDamage(10, DamageType.Martial);

        if (monster.CurrentHealth < monster.MaxHealth)
        {
            Assert.AreEqual(monster.MaxHealth - 10, monster.CurrentHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 10 damage")));
        }
        else
        {
            Assert.AreEqual(monster.CurrentHealth, monster.MaxHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_DefenseReducesDamage()
    {
        var monster = CreateMonster();
        monster.DefensePower = 5;
        monster.CurrentHealth = monster.MaxHealth;

        monster.TakeDamage(10, DamageType.Martial);

        if (monster.CurrentHealth < monster.MaxHealth)
        {
            Assert.AreEqual(monster.MaxHealth - 5, monster.CurrentHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 5 damage")));
        }
        else
        {
            Assert.AreEqual(monster.CurrentHealth, monster.MaxHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }
    [TestMethod]
    public void TakeDamage_MartialDamage_DefenseGreaterThanDamage()
    {
        var monster = CreateMonster();
        monster.DefensePower = 12;
        monster.CurrentHealth = monster.MaxHealth;

        monster.TakeDamage(10, DamageType.Martial);

        if (monster.CurrentHealth < monster.MaxHealth)
        {
            Assert.AreEqual(monster.MaxHealth - 1, monster.CurrentHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 1 damage")));
        }
        else
        {
            Assert.AreEqual(monster.CurrentHealth, monster.MaxHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }
    [TestMethod]
    public void TakeDamage_MagicalDamage_ReducesHealth()
    {
        var monster = CreateMonster();
        monster.Resistance = 0;
        monster.CurrentHealth = monster.MaxHealth;

        monster.TakeDamage(10, DamageType.Magical);

        if (monster.CurrentHealth < monster.MaxHealth)
        {
            Assert.AreEqual(monster.MaxHealth - 10, monster.CurrentHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 10 damage")));
        }
        else
        {
            Assert.AreEqual(monster.CurrentHealth, monster.MaxHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_MagicalDamage_ResistanceReducesDamage()
    {
        var monster = CreateMonster();
        monster.Resistance = 5;
        monster.CurrentHealth = monster.MaxHealth;

        monster.TakeDamage(10, DamageType.Magical);

        if (monster.CurrentHealth < monster.MaxHealth)
        {
            Assert.AreEqual(monster.MaxHealth - 5, monster.CurrentHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 5 damage")));
        }
        else
        {
            Assert.AreEqual(monster.CurrentHealth, monster.MaxHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }
    [TestMethod]
    public void TakeDamage_MagicalDamage_ResistanceGreaterThanDamage()
    {
        var monster = CreateMonster();
        monster.Resistance = 12;
        monster.CurrentHealth = monster.MaxHealth;

        monster.TakeDamage(10, DamageType.Magical);

        if (monster.CurrentHealth < monster.MaxHealth)
        {
            Assert.AreEqual(monster.MaxHealth - 1, monster.CurrentHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 1 damage")));
        }
        else
        {
            Assert.AreEqual(monster.CurrentHealth, monster.MaxHealth);
            Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_MonsterDies()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 1;

        Assert.ThrowsException<MonsterDeathException>(() =>  monster.TakeDamage(1000, DamageType.Magical));
    }

    [TestMethod]
    public void Heal_Success()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = monster.MaxHealth - 30;

        monster.Heal(20);

        Assert.AreEqual(monster.MaxHealth - 10, monster.CurrentHealth);
    }

    [TestMethod]
    public void Heal_HitsMaximum()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = monster.MaxHealth - 10;

        monster.Heal(20);

        Assert.AreEqual(monster.MaxHealth, monster.CurrentHealth);
    }
    [TestMethod]
    public void GetStat_ReturnsCorrectStat()
    {
        var monster = CreateMonster();
        monster.AttackPower = 10;

        int attack = monster.GetStat(StatType.Attack);

        Assert.AreEqual(10, attack);
    }

    [TestMethod]
    public void GetStat_InvalidStat_Throws()
    {
        var monster = CreateMonster();

        Assert.ThrowsException<StatTypeException>(() => monster.GetStat((StatType)999));
    }
    [TestMethod]
    public void ModifyStat_Health_Heals()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 50;

        monster.ModifyStat(StatType.Health, 20);

        Assert.AreEqual(70, monster.CurrentHealth);
    }

    [TestMethod]
    public void ModifyStat_OtherStat_ModifiesActiveEffect()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 50;
        monster.AggressionLevel = 0;

        monster.ModifyStat(StatType.Speed, 5);

        Assert.AreEqual(5, monster.GetStat(StatType.Speed));
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("is changed by")));
    }

    [TestMethod]
    public void ModifyStat_InvalidStat_Throws()
    {
        var monster = CreateMonster();

        Assert.ThrowsException<StatTypeException>(() => monster.ModifyStat((StatType)999, 5));
    }
    [TestMethod]
    public void ClearActionItems_RemovesAll()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        monster.AddActionItem("Test action");
        Assert.IsTrue(monster.ActionItems.Count > 0);

        monster.ClearActionItems();

        Assert.AreEqual(0, monster.ActionItems.Count);
    }

    [TestMethod]
    public void AddActionItem_AddsAction()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        monster.AddActionItem("Test action");
        Assert.IsTrue(monster.ActionItems.Values.Contains("Test action"));
    }

    [TestMethod]
    public void AddActionItem_Skill_AddsAction()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        var skill = new Skill { Name = "Fireball" };
        monster.AddActionItem(skill);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("uses Fireball")));
    }
    [TestMethod]
    public void Loot_NoLoot()
    {
        var monster = CreateMonster();

        var item = monster.Loot();

        Assert.IsNull(item);
    }

    [TestMethod]
    public void Loot_HasLoot()
    {
        var monster = CreateMonster();
        var item = CreateItem();

        monster.SetLoot(item);

        var loot = monster.Loot();

        Assert.IsNotNull(loot);
        Assert.AreSame(item, loot);
        Assert.IsNull(loot.Monster);
        Assert.IsNull(loot.MonsterId);
        Assert.IsNull(monster.ItemId);
        Assert.IsNull(monster.Treasure);
    }

    [TestMethod]
    public void SetLoot()
    {
        var monster = CreateMonster();
        var loot = CreateItem();

        Assert.IsNull(loot.Monster);
        Assert.IsNull(loot.MonsterId);
        Assert.IsNull(monster.ItemId);
        Assert.IsNull(monster.Treasure);

        monster.SetLoot(loot);

        Assert.IsNotNull(loot.Monster);
        Assert.IsNotNull(loot.MonsterId);
        Assert.IsNotNull(monster.ItemId);
        Assert.IsNotNull(monster.Treasure);
    }
}
