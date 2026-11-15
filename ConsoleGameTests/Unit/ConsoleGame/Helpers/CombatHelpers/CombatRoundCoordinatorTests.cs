using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Monsters.Strategies;
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
            MaxHealth = 100,
            CurrentHealth = 100,
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
    
    private static string GetTurnName(CombatTurn turn)
    {
        return turn.Type == CombatantType.Player
            ? "Player"
            : turn.Monster!.Name;
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
            new[] { fasterMonster },
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
    public void ExecuteRound_PlayerTurnCompletes_RecoversResourceOnce()
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

        // Once after the player turn and once at the end of the round.
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
    
    // ---- DOT Ticks
    private static ActiveEffect CreateDoT(
        ElementType element,
        int power,
        int duration)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(element),
            power,
            duration);
    }
    
    [TestMethod]
    public void ExecuteRound_PlayerDoT_ProcessesAtBeginningOfPlayerTurn()
    {
        var player = CreatePlayer(speed: 10);
        var monster = CreateMonster("Monster A", 5);

        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var actionExecuted = false;

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () =>
            {
                var result = player.ActiveEffectManager.ProcessDoTTicks(player);

                Assert.AreEqual(90, player.CurrentHealth);
                Assert.AreEqual(2, player.ActiveEffectManager.ActiveEffects.Single().RemainingDuration);

                actionExecuted = !result.EffectResults.Any(r => r.TargetDied);
            },
            beforeMonsterTurn: m => m.CurrentHealth = 0,
            displayActions: () => { });

        Assert.IsTrue(actionExecuted);
    }

    [TestMethod]
    public void ExecuteRound_PlayerDoT_KillsPlayer_SkipsPlayerActionAndStopsRound()
    {
        var player = CreatePlayer(speed: 10);
        player.CurrentHealth = 5;

        var monster = CreateMonster("Monster A", 5);
        int playerActionCount = 0;
        int monsterTurnCount = 0;

        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () =>
            {
                var result = player.ActiveEffectManager.ProcessDoTTicks(player);

                if (result.EffectResults.Any(r => r.TargetDied))
                    return;

                playerActionCount++;
            },
            beforeMonsterTurn: _ => monsterTurnCount++,
            displayActions: () => { });

        Assert.IsTrue(player.CurrentHealth <= 0);
        Assert.AreEqual(0, playerActionCount);
        Assert.AreEqual(0, monsterTurnCount);
    }
    
    [TestMethod]
    public void ExecuteRound_MonsterDoT_ProcessesAtBeginningOfMonsterTurn()
    {
        var player = CreatePlayer(speed: 1);
        player.MaxHealth = 1000;
        player.CurrentHealth = 1000;

        var monster = CreateMonster("Monster A", 10);

        var effect = CreateDoT(ElementType.Fire, 10, 3);
        monster.ActiveEffectManager.Register(effect);

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () => { },
            beforeMonsterTurn: m =>
            {
                // The DoT has already ticked before this callback.
                Assert.AreEqual(90, m.CurrentHealth);
                Assert.AreEqual(2, effect.RemainingDuration);
            },
            displayActions: () => { });

        Assert.AreEqual(90, monster.CurrentHealth);
        Assert.AreEqual(2, effect.RemainingDuration);
    }

    [TestMethod]
    public void ExecuteRound_MonsterDoT_KillsMonster_SkipsMonsterTurnButContinuesRound()
    {
        var player = CreatePlayer(speed: 1);
        var monster = CreateMonster("Monster A", 10, health: 5);

        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        int playerTurnCount = 0;
        int monsterTurnCount = 0;

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () => playerTurnCount++,
            beforeMonsterTurn: _ => monsterTurnCount++,
            displayActions: () => { });

        Assert.IsTrue(monster.CurrentHealth <= 0);

        // The round continues to the player's scheduled turn.
        Assert.AreEqual(1, playerTurnCount);

        // A monster killed by its DoT never reaches its pre-action callback.
        Assert.AreEqual(0, monsterTurnCount);
    }

    [TestMethod]
    public void ExecuteRound_MonsterKilledBeforeTurn_DoesNotProcessDoT()
    {
        var player = CreatePlayer(speed: 10);
        var monster = CreateMonster("Monster A", 5);

        var effect = CreateDoT(ElementType.Fire, 10, 3);
        monster.ActiveEffectManager.Register(effect);

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () => monster.CurrentHealth = 0,
            beforeMonsterTurn: _ => Assert.Fail("A defeated monster should not take a turn."),
            displayActions: () => { });

        Assert.AreEqual(0, monster.CurrentHealth);
        Assert.AreEqual(3, effect.RemainingDuration);
        Assert.AreEqual(1, monster.ActiveEffectManager.ActiveEffects.Count);
    }
    
    private static BossMonster CreateBoss(int speed = 10)
    {
        return new BossMonster(new MonsterSkillSelector())
        {
            Name = "Boss",
            Level = 1,
            AttackPower = 10,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = speed,
            MaxHealth = 300,
            CurrentHealth = 300,
            DamageType = DamageType.Martial,
            Skills = []
        };
    }
    
    [TestMethod]
    public void ExecuteRound_BossDoT_TriggersPhaseTransitionAndBossGetsTurn()
    {
        var player = CreatePlayer(speed: 1);
        player.MaxHealth = 1000;
        player.CurrentHealth = 1000;

        var boss = CreateBoss();
        boss.CurrentHealth = 10;

        boss.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        int bossTurnCount = 0;

        _coordinator.ExecuteRound(
            player,
            [boss],
            playerTurn: () => { },
            beforeMonsterTurn: _ => bossTurnCount++,
            displayActions: () => { });

        Assert.IsTrue(boss.CurrentHealth > 0);
        Assert.AreEqual(225, boss.MaxHealth);
        Assert.AreEqual(1, bossTurnCount);
    }

    [TestMethod]
    public void ExecuteRound_BossFinalPhaseDoT_KillsBossAndSkipsBossTurn()
    {
        var player = CreatePlayer(speed: 1);
        var boss = CreateBoss();

        // Advance the boss through phases two and three.
        boss.ApplyElementalDamage(100000, ElementType.Fire);
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        Assert.AreEqual(150, boss.MaxHealth);
        Assert.AreEqual(150, boss.CurrentHealth);

        boss.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 200, 3));

        int bossTurnCount = 0;
        int playerTurnCount = 0;

        _coordinator.ExecuteRound(
            player,
            [boss],
            playerTurn: () => playerTurnCount++,
            beforeMonsterTurn: _ => bossTurnCount++,
            displayActions: () => { });

        Assert.IsTrue(boss.CurrentHealth <= 0);
        Assert.AreEqual(0, bossTurnCount);
        Assert.AreEqual(1, playerTurnCount);
    }
    
    
    [TestMethod]
    public void ExecuteRound_PlayerSpeedChangesDuringRound_PreservesOriginalTurnOrder()
    {
        var player = CreatePlayer(speed: 5);
        var monsterA = CreateMonster("Monster A", 10);
        var monsterB = CreateMonster("Monster B", 7);

        var monsters = new List<Monster> { monsterA, monsterB };
        var turnOrder = new List<string>();

        _coordinator.ExecuteRound(
            player,
            monsters,
            playerTurn: () => turnOrder.Add("Player"),
            beforeMonsterTurn: monster =>
            {
                turnOrder.Add(monster.Name);

                if (monster == monsterA)
                {
                    var skill = CreateSelfSupportSkill();
                    skill.Activate(player);

                    Assert.AreEqual(9, player.GetStat(StatType.Speed));
                }

                monster.CurrentHealth = 0;
            },
            displayActions: () => { });

        CollectionAssert.AreEqual(
            new[] { "Monster A", "Monster B", "Player" },
            turnOrder);
    }

    [TestMethod]
    public void ExecuteRound_MonsterSpeedChangesDuringRound_PreservesOriginalTurnOrder()
    {
        var player = CreatePlayer(speed: 5);
        var monsterA = CreateMonster("Monster A", 10);
        var monsterB = CreateMonster("Monster B", 7);

        var monsters = new List<Monster> { monsterA, monsterB };
        var turnOrder = new List<string>();

        _coordinator.ExecuteRound(
            player,
            monsters,
            playerTurn: () => turnOrder.Add("Player"),
            beforeMonsterTurn: monster =>
            {
                turnOrder.Add(monster.Name);

                if (monster == monsterA)
                {
                    var skill = CreateSelfSupportSkill();
                    skill.Activate(monsterB);

                    Assert.AreEqual(11, monsterB.GetStat(StatType.Speed));
                }

                monster.CurrentHealth = 0;
            },
            displayActions: () => { });

        CollectionAssert.AreEqual(
            new[] { "Monster A", "Monster B", "Player" },
            turnOrder);
    }

    [TestMethod]
    public void ExecuteRound_SpeedEffectExpiresDuringRound_AffectsNextRoundOnly()
    {
        var player = CreatePlayer(speed: 5);
        var monster = CreateMonster("Monster A", 7);

        var skill = CreateSelfSupportSkill();
        skill.Duration = 1;
        skill.Activate(player);

        var firstRoundOrder = new List<string>();

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () =>
            {
                firstRoundOrder.Add("Player");
            },
            beforeMonsterTurn: m =>
            {
                firstRoundOrder.Add(m.Name);
                m.CurrentHealth = 0;
            },
            displayActions: () => { });

        // Player started with Speed 9 and therefore acted before the monster.
        CollectionAssert.AreEqual(
            new[] { "Player", "Monster A" },
            firstRoundOrder);

        // The Speed effect expired after the player's turn.
        Assert.AreEqual(0, player.ActiveEffectManager.ActiveEffects.Count);
        Assert.AreEqual(5, player.GetStat(StatType.Speed));

        // Restore the monster for the next round.
        monster.CurrentHealth = 100;

        var secondRoundOrder = new List<string>();

        _coordinator.ExecuteRound(
            player,
            [monster],
            playerTurn: () => secondRoundOrder.Add("Player"),
            beforeMonsterTurn: m =>
            {
                secondRoundOrder.Add(m.Name);
                m.CurrentHealth = 0;
            },
            displayActions: () => { });

        // The next round captures the player's base Speed of 5.
        CollectionAssert.AreEqual(
            new[] { "Monster A", "Player" },
            secondRoundOrder);
    }
}