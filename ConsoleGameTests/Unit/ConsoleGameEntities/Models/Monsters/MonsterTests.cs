using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters;

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
            MaxHealth = 200,
            DodgeChance = 0
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
    
    private static SupportSkill CreateSupportSkill(StatType stat, SupportEffectType effect)
    {
        return new SupportSkill
        {
            Name = "Support Boost",
            Duration = 3,
            Cooldown = 5,
            Power = 4,
            StatAffected = stat,
            SupportEffect = effect,
            TargetType = TargetType.Self,
            ElapsedTime = 5
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
        Assert.IsTrue(monster.AggressionLevel is >= 2 and <= 4);
        Assert.IsTrue(monster.MaxHealth >= level * 8 && monster.MaxHealth <= level * 12);
        Assert.AreEqual(monster.MaxHealth, monster.CurrentHealth);
    }

    [TestMethod]
    public void Heal_IncreasesCurrentHealth()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 50;

        monster.Heal(20);

        Assert.AreEqual(70, monster.CurrentHealth);
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
        var skill = new MartialSkill { Name = "Fireball" };
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
        Assert.IsNull(monster.Treasure);
    }

    [TestMethod]
    public void SetLoot()
    {
        var monster = CreateMonster();
        var loot = CreateItem();

        Assert.IsNull(loot.Monster);
        Assert.IsNull(loot.MonsterId);
        Assert.IsNull(monster.Treasure);

        monster.SetLoot(loot);

        Assert.IsNotNull(loot.Monster);
        Assert.IsNotNull(loot.MonsterId);
        Assert.IsNotNull(monster.Treasure);
    }
}