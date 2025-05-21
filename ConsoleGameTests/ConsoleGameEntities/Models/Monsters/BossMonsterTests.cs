using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using System.Threading;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class BossMonsterTests
{
    private static BossMonster CreateBoss()
    {
        return new BossMonster (new MonsterSkillSelector())
        {
            Name = "Boss",
            Level = 1,
            AttackPower = 100,
            DefensePower = 20,
            Resistance = 10,
            AggressionLevel = 5,
            MaxHealth = 300,
            CurrentHealth = 300,
            DamageType = DamageType.Martial,
            Skills = []
        };
    }

    [TestMethod]
    public void Constructor_InitializesBaseStats()
    {
        var boss = CreateBoss();
        Assert.AreEqual(300, boss.MaxHealth);
        Assert.AreEqual(100, boss.AttackPower);
        Assert.AreEqual("Boss", boss.Name);
        Assert.IsInstanceOfType(boss.Strategy, typeof(BossStrategy));
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_ReducesHealth()
    {
        var boss = CreateBoss();
        boss.DefensePower = 0;
        boss.CurrentHealth = boss.MaxHealth;

        boss.TakeDamage(10, DamageType.Martial);

        if (boss.CurrentHealth < boss.MaxHealth)
        {
            Assert.AreEqual(boss.MaxHealth - 10, boss.CurrentHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("takes 10 damage")));
        }
        else
        {
            Assert.AreEqual(boss.CurrentHealth, boss.MaxHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_DefenseReducesDamage()
    {
        var boss = CreateBoss();
        boss.DefensePower = 5;
        boss.CurrentHealth = boss.MaxHealth;

        boss.TakeDamage(10, DamageType.Martial);

        if (boss.CurrentHealth < boss.MaxHealth)
        {
            Assert.AreEqual(boss.MaxHealth - 5, boss.CurrentHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("takes 5 damage")));
        }
        else
        {
            Assert.AreEqual(boss.CurrentHealth, boss.MaxHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_MagicalDamage_ResistanceReducesDamage()
    {
        var boss = CreateBoss();
        boss.Resistance = 7;
        boss.CurrentHealth = boss.MaxHealth;

        boss.TakeDamage(10, DamageType.Magical);

        if (boss.CurrentHealth < boss.MaxHealth)
        {
            Assert.AreEqual(boss.MaxHealth - 3, boss.CurrentHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("takes 3 damage")));
        }
        else
        {
            Assert.AreEqual(boss.CurrentHealth, boss.MaxHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_HybridDamage_UsesMaxOfDefenseOrResistance()
    {
        var boss = CreateBoss();
        boss.DefensePower = 8;
        boss.Resistance = 4;
        boss.CurrentHealth = boss.MaxHealth;

        boss.TakeDamage(10, DamageType.Hybrid);
        
        if (boss.CurrentHealth < boss.MaxHealth)
        {
            Assert.AreEqual(boss.MaxHealth - 6, boss.CurrentHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("takes 6 damage")));
        }
        else
        {
            Assert.AreEqual(boss.CurrentHealth, boss.MaxHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }        
    }

    [TestMethod]
    public void TakeDamage_PhaseChange_ResetsStatsAndAddsAction()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;
        boss.MaxHealth = 100;
        boss.AttackPower = 50;
        boss.DefensePower = 10;
        boss.Resistance = 5;
        boss.AggressionLevel = 0;
        boss.DamageType = DamageType.Martial;

        boss.TakeDamage(10, DamageType.Martial);

        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 2")));
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsTrue(boss.AggressionLevel > 0);
        Assert.IsTrue(boss.DefensePower > 10);
        Assert.IsTrue(boss.Resistance > 5);
        Assert.IsTrue(boss.AttackPower < 50); // scaled down
        Assert.IsTrue(boss.MaxHealth < 100);  // scaled down
    }

    [TestMethod]
    public void TakeDamage_PhaseChange_ThrowsOnFinalPhase()
    {
        var boss = CreateBoss();
        boss.DefensePower = 0;
        
        boss.CurrentHealth = 10;
        boss.TakeDamage(100, DamageType.Martial); //should set phase to 2

        boss.CurrentHealth = 10;
        boss.TakeDamage(100, DamageType.Martial); //should set phase to 3


        boss.CurrentHealth = 10;
        Assert.ThrowsException<MonsterDeathException>(() => boss.TakeDamage(100, DamageType.Martial));
    }
}
