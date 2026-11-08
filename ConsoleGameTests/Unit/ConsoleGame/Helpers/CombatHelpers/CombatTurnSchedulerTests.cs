using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.CombatHelpers;

[TestClass]
public class CombatTurnSchedulerTests
{
    private CombatTurnScheduler _scheduler = null!;

    [TestInitialize]
    public void Setup()
    {
        _scheduler = new CombatTurnScheduler();
    }

    // Creates a player with a predictable base Speed.
    private static Player CreatePlayer(int speed)
    {
        var archetype = new Archetype
        {
            Speed = speed
        };

        return new Player
        {
            Archetype = archetype
        };
    }

    // Creates a monster with a predictable Speed and health.
    private static Monster CreateMonster(string name, int speed, int health = 100)
    {
        return new Monster
        {
            Name = name,
            AggressionLevel = speed,
            CurrentHealth = health
        };
    }

    // Player acts before every monster when faster than all of them.
    [TestMethod]
    public void ScheduleTurns_PlayerFasterThanAllMonsters_PlayerActsFirst()
    {
        var player = CreatePlayer(10);
        var monsters = new List<Monster>
        {
            CreateMonster("Monster A", 8),
            CreateMonster("Monster B", 5)
        };

        var turns = _scheduler.ScheduleTurns(player, monsters).ToList();

        Assert.AreEqual(3, turns.Count);
        Assert.AreEqual(CombatantType.Player, turns[0].Type);
        Assert.AreSame(monsters[0], turns[1].Monster);
        Assert.AreSame(monsters[1], turns[2].Monster);
    }

    // Faster monsters act before the player.
    [TestMethod]
    public void ScheduleTurns_PlayerSlowerThanAllMonsters_PlayerActsLast()
    {
        var player = CreatePlayer(3);
        var monsters = new List<Monster>
        {
            CreateMonster("Monster A", 8),
            CreateMonster("Monster B", 5)
        };

        var turns = _scheduler.ScheduleTurns(player, monsters).ToList();

        Assert.AreEqual(3, turns.Count);
        Assert.AreSame(monsters[0], turns[0].Monster);
        Assert.AreSame(monsters[1], turns[1].Monster);
        Assert.AreEqual(CombatantType.Player, turns[2].Type);
    }

    // The player wins a Speed tie against a monster.
    [TestMethod]
    public void ScheduleTurns_PlayerTiedWithMonster_PlayerActsBeforeMonster()
    {
        var player = CreatePlayer(7);
        var monster = CreateMonster("Monster A", 7);

        var turns = _scheduler.ScheduleTurns(player, [monster]).ToList();

        Assert.AreEqual(2, turns.Count);
        Assert.AreEqual(CombatantType.Player, turns[0].Type);
        Assert.AreSame(monster, turns[1].Monster);
    }

    // Monsters with equal Speed retain their original list order.
    [TestMethod]
    public void ScheduleTurns_MonstersHaveEqualSpeed_OriginalOrderIsPreserved()
    {
        var player = CreatePlayer(1);
        var monsters = new List<Monster>
        {
            CreateMonster("Monster A", 8),
            CreateMonster("Monster B", 8),
            CreateMonster("Monster C", 8)
        };

        var turns = _scheduler.ScheduleTurns(player, monsters).ToList();

        Assert.AreSame(monsters[0], turns[0].Monster);
        Assert.AreSame(monsters[1], turns[1].Monster);
        Assert.AreSame(monsters[2], turns[2].Monster);
        Assert.AreEqual(CombatantType.Player, turns[3].Type);
    }

    // The player receives exactly one turn, regardless of monster count.
    [TestMethod]
    public void ScheduleTurns_MultipleMonsters_PlayerActsExactlyOnce()
    {
        var player = CreatePlayer(5);
        var monsters = new List<Monster>
        {
            CreateMonster("Monster A", 8),
            CreateMonster("Monster B", 5),
            CreateMonster("Monster C", 3),
            CreateMonster("Monster D", 1)
        };

        var turns = _scheduler.ScheduleTurns(player, monsters).ToList();

        Assert.AreEqual(1, turns.Count(t => t.Type == CombatantType.Player));
        Assert.AreEqual(4, turns.Count(t => t.Type == CombatantType.Monster));

        // Monster A is faster; player wins the tie with Monster B.
        Assert.AreSame(monsters[0], turns[0].Monster);
        Assert.AreEqual(CombatantType.Player, turns[1].Type);
    }

    // A Speed change during enumeration affects the player's placement.
    [TestMethod]
    public void ScheduleTurns_PlayerSpeedChangesDuringRound_UsesUpdatedSpeed()
    {
        var player = CreatePlayer(5);
        var monsters = new List<Monster>
        {
            CreateMonster("Monster A", 10),
            CreateMonster("Monster B", 7)
        };

        using var enumerator = _scheduler.ScheduleTurns(player, monsters).GetEnumerator();

        // First turn: Monster A acts because it is faster.
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual(CombatantType.Monster, enumerator.Current.Type);
        Assert.AreSame(monsters[0], enumerator.Current.Monster);

        // Increase player Speed before Monster B's turn.
        player.ModifyStat(StatType.Speed, 3);

        // The player now ties Monster B and acts first.
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual(CombatantType.Player, enumerator.Current.Type);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(monsters[1], enumerator.Current.Monster);

        Assert.IsFalse(enumerator.MoveNext());
    }

    // Dead monsters are excluded from the scheduled turns.
    [TestMethod]
    public void ScheduleTurns_DeadMonsters_AreExcluded()
    {
        var player = CreatePlayer(5);
        var aliveMonster = CreateMonster("Alive Monster", 8);
        var deadMonster = CreateMonster("Dead Monster", 10, 0);

        var turns = _scheduler.ScheduleTurns(
            player,
            [deadMonster, aliveMonster]).ToList();

        Assert.AreEqual(2, turns.Count);
        Assert.AreSame(aliveMonster, turns[0].Monster);
        Assert.AreEqual(CombatantType.Player, turns[1].Type);
    }
}
