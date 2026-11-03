using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGame.Helpers.CombatHelpers;

[TestClass]
public class CombatRoundCoordinatorTests
{
    private CombatRoundCoordinator _coordinator = null!;

    [TestInitialize]
    public void Setup()
    {
        _coordinator = new CombatRoundCoordinator(new CombatTurnScheduler());
    }

    // Creates a player with predictable Speed and resource values.
    private static Player CreatePlayer(
        int speed = 10,
        int currentResource = 5,
        int maxResource = 10,
        int recoveryRate = 2)
    {
        return new Player
        {
            Archetype = new Archetype
            {
                Speed = speed,
                CurrentResource = currentResource,
                MaxResource = maxResource,
                RecoveryRate = recoveryRate
            }
        };
    }

    // Creates a monster with predictable Speed and health.
    private static Monster CreateMonster(string name, int speed, int health = 100)
    {
        return new Monster
        {
            Name = name,
            AggressionLevel = speed,
            CurrentHealth = health
        };
    }

    // Executes a round with callbacks that prevent actual combat attacks.
    private void ExecuteRoundWithoutAttacks(Player player, List<Monster> monsters)
    {
        _coordinator.ExecuteRound(
            player,
            monsters,
            playerTurn: () => { },
            beforeMonsterTurn: monster => monster.CurrentHealth = 0,
            displayActions: () => { });
    }

    // The player's turn callback executes exactly once per round.
    [TestMethod]
    public void ExecuteRound_PlayerTurn_PlayerCallbackExecutesOnce()
    {
        var player = CreatePlayer();
        var monster = CreateMonster("Monster A", 5);
        int playerTurnCount = 0;

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () => playerTurnCount++,
            beforeMonsterTurn: m => m.CurrentHealth = 0,
            displayActions: () => { });

        Assert.AreEqual(1, playerTurnCount);
    }

    // Every scheduled monster gets its pre-turn callback.
    [TestMethod]
    public void ExecuteRound_MultipleMonsters_ProcessesEachMonsterTurn()
    {
        var player = CreatePlayer(speed: 1);
        var monsters = new List<Monster>
        {
            CreateMonster("Monster A", 8),
            CreateMonster("Monster B", 5),
            CreateMonster("Monster C", 3)
        };

        var processedMonsters = new List<Monster>();

        _coordinator.ExecuteRound(
            player,
            monsters,
            playerTurn: () => { },
            beforeMonsterTurn: monster =>
            {
                processedMonsters.Add(monster);
                monster.CurrentHealth = 0;
            },
            displayActions: () => { });

        CollectionAssert.AreEqual(monsters, processedMonsters);
    }

    // A monster defeated before its scheduled attack does not attack.
    [TestMethod]
    public void ExecuteRound_MonsterDefeatedBeforeTurn_SkipsItsAttack()
    {
        var player = CreatePlayer(speed: 5);
        var fasterMonster = CreateMonster("Faster Monster", 10);
        var slowerMonster = CreateMonster("Slower Monster", 3);

        var processedMonsters = new List<Monster>();

        _coordinator.ExecuteRound(
            player,
            [fasterMonster, slowerMonster],
            playerTurn: () => slowerMonster.CurrentHealth = 0,
            beforeMonsterTurn: monster =>
            {
                processedMonsters.Add(monster);
                monster.CurrentHealth = 0;
            },
            displayActions: () => { });

        // The callback still runs for the scheduled monster, but its attack is skipped.
        CollectionAssert.AreEqual(
            new[] { fasterMonster, slowerMonster },
            processedMonsters);
    }

    // Player and monster skill cooldowns each advance once per round.
    [TestMethod]
    public void ExecuteRound_SkillsExist_AdvancesCooldownsOnce()
    {
        var player = CreatePlayer();
        var monster = CreateMonster("Monster A", 5);

        var playerSkill = new Skill
        {
            Name = "Player Skill",
            Cooldown = 5,
            ElapsedTime = 1
        };

        var monsterSkill = new Skill
        {
            Name = "Monster Skill",
            Cooldown = 5,
            ElapsedTime = 2
        };

        player.Archetype.Skills.Add(playerSkill);
        monster.Skills.Add(monsterSkill);

        ExecuteRoundWithoutAttacks(player, [monster]);

        Assert.AreEqual(2, playerSkill.ElapsedTime);
        Assert.AreEqual(3, monsterSkill.ElapsedTime);
    }

    // Player resource recovery occurs exactly once at the end of the round.
    [TestMethod]
    public void ExecuteRound_PlayerSkipsTurn_RecoversResourceOnce()
    {
        var player = CreatePlayer(
            currentResource: 3,
            maxResource: 10,
            recoveryRate: 2);

        var monster = CreateMonster("Monster A", 5);

        ExecuteRoundWithoutAttacks(player, [monster]);

        Assert.AreEqual(5, player.Archetype.CurrentResource);
    }

    // Resource recovery never exceeds the player's maximum resource.
    [TestMethod]
    public void ExecuteRound_PlayerResourceNearMaximum_DoesNotExceedMaximum()
    {
        var player = CreatePlayer(
            currentResource: 9,
            maxResource: 10,
            recoveryRate: 5);

        var monster = CreateMonster("Monster A", 5);

        ExecuteRoundWithoutAttacks(player, [monster]);

        Assert.AreEqual(10, player.Archetype.CurrentResource);
    }

    // The display callback executes after player and monster actions and at round end.
    [TestMethod]
    public void ExecuteRound_TurnsExecute_DisplayCallbackRunsAtExpectedPoints()
    {
        var player = CreatePlayer();
        var monster = CreateMonster("Monster A", 5);
        int displayCount = 0;

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () => { },
            beforeMonsterTurn: m => m.CurrentHealth = 0,
            displayActions: () => displayCount++);

        // Once after the player turn, once after the monster turn is skipped? No:
        // the monster attack is skipped, so only the final round display follows.
        Assert.AreEqual(2, displayCount);
    }

    // Defeated monsters have their active support effects reset.
    [TestMethod]
    public void ExecuteRound_DefeatedMonster_ResetsItsSupportSkills()
    {
        var player = CreatePlayer();
        var defeatedMonster = CreateMonster("Defeated Monster", 8, health: 0);
        var livingMonster = CreateMonster("Living Monster", 5);

        var supportSkill = new SupportSkill
        {
            Name = "Speed Boost",
            Duration = 3,
            Cooldown = 5,
            Power = 4,
            StatAffected = StatType.Speed,
            SupportEffect = (int)SupportEffectType.Boost,
            TargetType = TargetType.Self
        };

        defeatedMonster.Skills.Add(supportSkill);

        // Apply the effect before the monster is defeated.
        supportSkill.ApplyEffect(defeatedMonster);

        int boostedSpeed = defeatedMonster.GetStat(StatType.Speed);
        Assert.AreEqual(12, boostedSpeed);
        Assert.AreEqual(1, supportSkill.TargetsAffected.Count);

        // Include the defeated monster so the coordinator can clean up its effects.
        _coordinator.ExecuteRound(
            player,
            [defeatedMonster, livingMonster],
            playerTurn: () => { },
            beforeMonsterTurn: monster => monster.CurrentHealth = 0,
            displayActions: () => { });

        Assert.AreEqual(8, defeatedMonster.GetStat(StatType.Speed));
        Assert.AreEqual(0, supportSkill.TargetsAffected.Count);
    }
}