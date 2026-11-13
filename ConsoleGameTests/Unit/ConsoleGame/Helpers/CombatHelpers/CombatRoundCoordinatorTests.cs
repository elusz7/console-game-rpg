using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.CombatHelpers;

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
        var skillSelector = new Mock<IMonsterSkillSelector>().Object;

        return new Monster
        {
            Name = name,
            AggressionLevel = speed,
            CurrentHealth = health,
            AttackPower = 1,
            DamageType = DamageType.Martial,
            Strategy = Monster.GetStrategyFor(
                MonsterBehaviorType.Default,
                skillSelector)
        };
    }
    
    private static SupportSkill CreateSelfSupportSkill()
    {
        return new SupportSkill
        {
            Name = "Speed Boost",
            Duration = 3,
            Cooldown = 5,
            Power = 4,
            StatAffected = StatType.Speed,
            SupportEffect = SupportEffectType.Boost,
            TargetType = TargetType.Self,
            ElapsedTime = 5
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

        var playerSkill = new MartialSkill
        {
            Name = "Player Skill",
            Cooldown = 5,
            ElapsedTime = 1
        };

        var monsterSkill = new MagicSkill
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
        var defeatedMonster = CreateMonster("Defeated Monster", 8);
        var livingMonster = CreateMonster("Living Monster", 5);

        var supportSkill = new SupportSkill
        {
            Name = "Speed Boost",
            Duration = 3,
            Cooldown = 5,
            Power = 4,
            StatAffected = StatType.Speed,
            SupportEffect = SupportEffectType.Boost,
            TargetType = TargetType.Self,
            ElapsedTime = 5
        };

        defeatedMonster.Skills.Add(supportSkill);

        // Apply the effect while the monster is alive.
        supportSkill.Activate(defeatedMonster);

        // Verify the effect was registered and applied.
        Assert.AreEqual(1, defeatedMonster.ActiveEffectManager.ActiveEffects.Count);
        Assert.AreEqual(12, defeatedMonster.GetStat(StatType.Speed));

        // Defeat the monster before the round executes.
        defeatedMonster.CurrentHealth = 0;

        _coordinator.ExecuteRound(
            player,
            [defeatedMonster, livingMonster],
            playerTurn: () => { },
            beforeMonsterTurn: monster => monster.CurrentHealth = 0,
            displayActions: () => { });

        // Verify the defeated monster's support effect was removed.
        Assert.AreEqual(0, defeatedMonster.ActiveEffectManager.ActiveEffects.Count);
        Assert.AreEqual(8, defeatedMonster.GetStat(StatType.Speed));

        // Verify the living monster remains unaffected.
        Assert.AreEqual(0, livingMonster.ActiveEffectManager.ActiveEffects.Count);
        Assert.AreEqual(5, livingMonster.GetStat(StatType.Speed));
    }
    
    [TestMethod]
    public void ExecuteRound_PlayerEffect_DecrementsAfterPlayerTurn()
    {
        var player = CreatePlayer(speed: 10);
        var monster = CreateMonster("Monster A", 5);

        var skill = CreateSelfSupportSkill();
        skill.Activate(player);

        var effect = player.ActiveEffectManager.ActiveEffects.Single();

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () =>
            {
                // The effect hasn't expired or decremented before the turn completes.
                Assert.AreEqual(3, effect.RemainingDuration);
            },
            beforeMonsterTurn: m => m.CurrentHealth = 0,
            displayActions: () => { });

        Assert.AreEqual(2, effect.RemainingDuration);
    }
    
    [TestMethod]
    public void ExecuteRound_MonsterEffect_DecrementsAfterMonsterTurn()
    {
        var player = CreatePlayer(speed: 1);
        player.MaxHealth = 1000;
        player.CurrentHealth = 1000;
        var monster = CreateMonster("Monster A", 10);

        var skill = CreateSelfSupportSkill();
        skill.Activate(monster);

        var effect = monster.ActiveEffectManager.ActiveEffects.Single();

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () => { },
            beforeMonsterTurn: m =>
            {
                // The effect hasn't decremented before the monster acts.
                Assert.AreEqual(3, effect.RemainingDuration);
            },
            displayActions: () => { });

        Assert.AreEqual(2, effect.RemainingDuration);
    }
    
    [TestMethod]
    public void ExecuteRound_MultipleCombatants_EachEffectDecrementsAfterItsOwnersTurn()
    {
        var player = CreatePlayer(speed: 10);
        var monster = CreateMonster("Monster A", 5);

        var playerSkill = CreateSelfSupportSkill();
        var monsterSkill = CreateSelfSupportSkill();

        playerSkill.Activate(player);
        monsterSkill.Activate(monster);

        var playerEffect = player.ActiveEffectManager.ActiveEffects.Single();
        var monsterEffect = monster.ActiveEffectManager.ActiveEffects.Single();

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () =>
            {
                Assert.AreEqual(3, playerEffect.RemainingDuration);
                Assert.AreEqual(3, monsterEffect.RemainingDuration);
            },
            beforeMonsterTurn: m =>
            {
                Assert.AreEqual(2, playerEffect.RemainingDuration);
                Assert.AreEqual(3, monsterEffect.RemainingDuration);

                m.CurrentHealth = 0;
            },
            displayActions: () => { });

        Assert.AreEqual(2, playerEffect.RemainingDuration);
        Assert.AreEqual(3, monsterEffect.RemainingDuration);
    }
    
    [TestMethod]
    public void ExecuteRound_MonsterKilledBeforeAction_DoesNotDecrementEffect()
    {
        var player = CreatePlayer(speed: 1);
        var monster = CreateMonster("Monster A", 10);

        var skill = CreateSelfSupportSkill();
        skill.Activate(monster);

        var effect = monster.ActiveEffectManager.ActiveEffects.Single();

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () => { },
            beforeMonsterTurn: m => m.CurrentHealth = 0,
            displayActions: () => { });

        Assert.AreEqual(3, effect.RemainingDuration);
        Assert.AreEqual(1, monster.ActiveEffectManager.ActiveEffects.Count);
    }
}