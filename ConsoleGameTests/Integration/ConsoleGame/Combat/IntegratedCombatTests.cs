using ConsoleGame.Helpers;
using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Integration.ConsoleGame.Combat;

/// <summary>
/// End-to-end integration tests that exercise CombatHelper.CombatRunner from entry to exit.
/// No mocks or fakes are used: every component is a real instance.
/// Input is driven by a queue-based concrete IInputManager, and output is captured by a
/// concrete IOutputManager that records WriteLine calls for assertion.
/// </summary>
[TestClass]
public class IntegratedCombatTests
{
    // ---------------------------------------------------------------------------
    // Concrete (non-mocked) IInputManager implementation
    // ---------------------------------------------------------------------------

    private sealed class QueueInputManager : IInputManager
    {
        private readonly Queue<int> _intQueue;
        private readonly Queue<string> _stringQueue;
        private readonly Queue<Item?> _itemQueue;
        private readonly Queue<object> _selectorQueue;

        public QueueInputManager(IEnumerable<int>? intQueue = null, IEnumerable<string>? stringQueue = null,
            IEnumerable<Item?>? itemQueue = null, IEnumerable<object>? selectorQueue = null)
        {
            _intQueue = new Queue<int>(intQueue ?? Array.Empty<int>());
            _stringQueue = new Queue<string>(stringQueue ?? Array.Empty<string>());
            _itemQueue = new Queue<Item?>(itemQueue ?? Array.Empty<Item?>());
            _selectorQueue = new Queue<object>(selectorQueue ?? Array.Empty<object>());
        }

        public ConsoleKeyInfo ReadKey() => default;
        public int ReadMenuKey(int maxValue) => DequeueInt();
        public int ReadInt(string prompt, int? max = null, bool cancel = false) => DequeueInt();
        public decimal ReadDecimal(string prompt) => 0m;
        public string ReadString(string prompt, string[]? validation = null) => _stringQueue.Dequeue();

        public Action<int>? OnIntDequeued { get; set; }

        public T? Selector<T>(List<T> items, Func<T, string> displayTextSelector, string prompt,
            Func<T, ConsoleColor>? colorSelector = null)
        {
            if (typeof(T) == typeof(Item))
                return (T)(object)(_itemQueue.Dequeue() ?? throw new InvalidOperationException("No item in queue"));

            if (_selectorQueue.TryDequeue(out var obj) && obj is T typed)
                return typed;

            return items.Count > 0 ? items[0] : default;
        }

        public T? Viewer<T>(List<T> items, Func<T, string> displayTextSelector, string prompt,
            Func<T, ConsoleColor>? colorSelector = null)
        {
            return items.Count > 0 ? items[0] : default;
        }

        public Item? SelectItem(string prompt, List<Item> items, string? purpose = null)
        {
            return _itemQueue.Dequeue();
        }

        public bool LoopAgain(string action) => true;
        public bool ConfirmAction(string action) => true;
        public T GetEnumChoice<T>(string prompt) where T : Enum => default!;
        public int DisplayEditMenu(List<string> properties) => 0;

        private int DequeueInt()
        {
            if (_intQueue.Count == 0)
                throw new InvalidOperationException("No more integers in input queue.");

            var value = _intQueue.Dequeue();
            OnIntDequeued?.Invoke(value);
            return value;
        }
    }

    // ---------------------------------------------------------------------------
    // Concrete (non-mocked) IOutputManager implementation
    // ---------------------------------------------------------------------------

    private sealed class CaptureOutputManager : IOutputManager
    {
        public List<string> WrittenLines { get; } = new();

        public void Clear()
        {
        }

        public void Display()
        {
        }

        public void Write(string message, ConsoleColor color = ConsoleColor.White)
        {
        }

        public void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            WrittenLines.Add(message);
        }

        public void WriteLine()
        {
        }
    }

    // ---------------------------------------------------------------------------
    // Helper: No-op monster skill selector (returns null for everything)
    // ---------------------------------------------------------------------------

    private sealed class EmptySkillSelector : MonsterSkillSelector
    {
        public override Skill? GetHighestDamageSkill(IMonster monster) => null;
        public override BossSkill? GetStrongestBossSkill(IMonster monster) => null;
        public override SupportSkill? GetDebuffSkill(IMonster monster, StatType? affectedStat = null) => null;
        public override SupportSkill? GetHealingSkill(IMonster monster, int desiredHealingPower) => null;
        public override SupportSkill? GetBuffSkill(IMonster monster, StatType? affectedStat = null) => null;
        public override List<SupportSkill> GetSupportSkills(IMonster monster) => [];
        public override UltimateSkill? GetUltimateSkill(IMonster monster) => null;
    }

    // ---------------------------------------------------------------------------
    // Helper: Monster that ends combat after its first attack (sets own health to 0)
    // ---------------------------------------------------------------------------

    private class EndingCombatMonster : Monster
    {
        public int HealthAtScheduledTurn { get; private set; }

        public override AttackResolutionResult Attack(IPlayer target)
        {
            HealthAtScheduledTurn = CurrentHealth;
            CurrentHealth = 0;
            return new AttackResolutionResult(0, 0, DamageType.Martial, 0, 0, null, false, false);
        }
    }

    private sealed class EffectObservingMonster : EndingCombatMonster
    {
        public int AttackCount { get; private set; }

        public Action<Player>? OnAttack { get; set; }

        // Keep this monster alive until the specified attack.
        public int KillAfterAttack { get; set; } = 1;

        public override AttackResolutionResult Attack(IPlayer target)
        {
            AttackCount++;

            OnAttack?.Invoke((Player)target);

            var result = base.Attack(target);

            if (AttackCount < KillAfterAttack)
                CurrentHealth = MaxHealth;

            return result;
        }
    }

    private sealed class ObservingBossMonster : BossMonster
    {
        public Action? OnPhaseTransition { get; set; }

        public List<int> MaxHealthAtAttack { get; } = [];

        public List<AttackResolutionResult> AttackResults { get; } = [];

        public ObservingBossMonster(IMonsterSkillSelector skillSelector)
            : base(skillSelector)
        {
        }

        public override AttackResolutionResult TakeDamage(AttackDamage attack)
        {
            var previousMaxHealth = MaxHealth;
            var result = base.TakeDamage(attack);

            if (MaxHealth != previousMaxHealth)
                OnPhaseTransition?.Invoke();

            return result;
        }

        public override AttackResolutionResult Attack(IPlayer target)
        {
            MaxHealthAtAttack.Add(MaxHealth);

            var result = base.Attack(target);
            AttackResults.Add(result);

            return result;
        }
    }

    private static void RegisterSupportEffect(
        ITargetable target,
        Monster sourceMonster)
    {
        var supportSkill = new SupportSkill
        {
            Name = "Test Support",
            Power = 10,
            Duration = 3,
            StatAffected = StatType.Attack,
            SupportEffect = SupportEffectType.Boost
        };

        sourceMonster.Skills.Add(supportSkill);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(supportSkill),
            power: 10,
            duration: 3,
            sourceCaster: sourceMonster,
            sourceSkill: supportSkill);

        target.ActiveEffectManager.Register(effect);
    }

    private static EffectObservingMonster CreateEffectObservingMonster(
        string name,
        int health = 100,
        int speed = 1,
        int killAfterAttack = 1)
    {
        return new EffectObservingMonster
        {
            Name = name,
            CurrentHealth = health,
            MaxHealth = health,
            AggressionLevel = speed,
            KillAfterAttack = killAfterAttack
        };
    }

    private static ObservingBossMonster CreateObservingBoss(
        int health = 30,
        int attackPower = 20,
        int speed = 1)
    {
        return new ObservingBossMonster(new EmptySkillSelector())
        {
            Name = "Boss",
            Level = 1,
            AttackPower = attackPower,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = speed,
            MaxHealth = health,
            CurrentHealth = health,
            DamageType = DamageType.Martial,
            Skills = new List<Skill>()
        };
    }

    private static ActiveEffect RegisterSpeedBoostEffect(
        Monster target,
        int power,
        int duration)
    {
        var skill = new SupportSkill
        {
            Name = "Test Speed Boost",
            Power = power,
            Duration = duration,
            StatAffected = StatType.Speed,
            SupportEffect = SupportEffectType.Boost
        };

        target.Skills.Add(skill);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            power: power,
            duration: duration,
            sourceCaster: target,
            sourceSkill: skill);

        target.ActiveEffectManager.Register(effect);

        return effect;
    }

    // ---------------------------------------------------------------------------
    // Factory helpers
    // ---------------------------------------------------------------------------

    private static CombatHelper CreateCombatHelper(
        IInputManager inputManager,
        IOutputManager outputManager)
    {
        var equipmentHelper = new EquipmentHelper(inputManager, outputManager);
        var turnScheduler = new CombatTurnScheduler();
        var roundCoordinator = new CombatRoundCoordinator(turnScheduler);
        var actionDisplayHelper = new CombatActionDisplayHelper(outputManager);
        var menuDisplayHelper = new CombatMenuDisplayHelper(outputManager);
        var skillHelper = new CombatSkillHelper();

        return new CombatHelper(
            inputManager,
            outputManager,
            equipmentHelper,
            roundCoordinator,
            actionDisplayHelper,
            menuDisplayHelper,
            skillHelper);
    }

    private static CombatHelper CreateThrowingCombatHelper(
        Exception exception,
        CaptureOutputManager output)
    {
        var inputMock = new Mock<IInputManager>();

        inputMock
            .Setup(x => x.ReadInt(
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<bool>()))
            .Throws(exception);

        var equipmentHelper = new EquipmentHelper(
            inputMock.Object,
            output);

        var roundCoordinator = new CombatRoundCoordinator(
            new CombatTurnScheduler());

        var actionDisplayHelper = new CombatActionDisplayHelper(output);
        var menuDisplayHelper = new CombatMenuDisplayHelper(output);
        var skillHelper = new CombatSkillHelper();

        return new CombatHelper(
            inputMock.Object,
            output,
            equipmentHelper,
            roundCoordinator,
            actionDisplayHelper,
            menuDisplayHelper,
            skillHelper);
    }

    private static Player CreatePlayer(
        int speed = 10,
        int health = 100,
        int currentResource = 100,
        int maxResource = 100,
        int recoveryRate = 0,
        ArchetypeType archetypeType = ArchetypeType.Martial)
    {
        var player = new Player
        {
            Name = "Test Player",
            Level = 1,
            CurrentHealth = health,
            MaxHealth = health,
            DodgeChance = 0,
            Archetype = new Archetype
            {
                Name = "Test Archetype",
                ArchetypeType = archetypeType,
                Skills = new List<Skill>(),
                CurrentResource = currentResource,
                MaxResource = maxResource,
                RecoveryRate = recoveryRate,
                Speed = speed
            }
        };

        player.Inventory = new Inventory
        {
            Player = player,
            Items = new List<Item>(),
            Runes = new List<ConsoleGameEntities.Models.Entities.Inventories.InventoryRune>(),
            Ingredients = new List<ConsoleGameEntities.Models.Entities.Inventories.InventoryIngredient>()
        };

        return player;
    }

    private static void EquipTestWeapon(Player player)
    {
        var weapon = new Weapon
        {
            Name = "Test Sword",
            AttackPower = 20,
            DamageType = DamageType.Martial,
            Durability = 100,
            RequiredLevel = 1,
            Inventory = player.Inventory
        };

        player.Inventory.Items.Add(weapon);
        player.Equip(weapon);
    }

    private static Monster CreateTestMonster(string name = "Test Monster", int speed = 1, int health = 100)
    {
        return new EndingCombatMonster
        {
            Name = name,
            CurrentHealth = health,
            MaxHealth = health,
            AggressionLevel = speed,
            AttackPower = 20,
            DodgeChance = 0,
            DamageType = DamageType.Martial
        };
    }

    private static ActiveEffect CreateDoT(
        ElementType element,
        int power,
        int duration,
        ITargetable? sourceCaster = null)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(element),
            power,
            duration,
            sourceCaster: sourceCaster);
    }

    private static ActiveEffect CreateElementalStatus(
        ElementalStatusEffectType statusType,
        int power,
        int duration,
        ITargetable? sourceCaster = null)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(statusType),
            power,
            duration,
            sourceCaster: sourceCaster);
    }

    // ===========================================================================
    // 1. Encounter lifecycle and guaranteed cleanup
    // ===========================================================================

    /// <summary>
    /// Player defeats all monsters; all active effects are cleared.
    /// </summary>
    [TestMethod]
    public void CombatRunner_Victory_ClearsAllCombatantEffects()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 1;
        EquipTestWeapon(player);

        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));
        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [1]); // Attack
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(monster.CurrentHealth <= 0, "Monster should be defeated.");
        Assert.AreEqual(0, player.ActiveEffectManager.ActiveEffects.Count,
            "Player effects should be cleared after victory.");
        Assert.AreEqual(0, monster.ActiveEffectManager.ActiveEffects.Count,
            "Monster effects should be cleared after victory.");
    }

    /// <summary>
    /// Player dies; all active effects are cleared.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerDeath_ClearsAllCombatantEffects()
    {
        // Arrange
        var player = CreatePlayer(health: 5);
        var monster = CreateTestMonster(speed: 0);

        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));
        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager();
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(player.CurrentHealth <= 0, "Player should die from DoT.");
        Assert.AreEqual(0, player.ActiveEffectManager.ActiveEffects.Count,
            "Player effects should be cleared after death.");
        Assert.AreEqual(0, monster.ActiveEffectManager.ActiveEffects.Count,
            "Monster effects should be cleared after player death.");
    }

    /// <summary>
    /// Exception escapes combat; all active effects are cleared.
    /// </summary>
    [TestMethod]
    public void CombatRunner_Exception_ClearsAllCombatantEffects()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster("Test Monster", 100);

        player.ActiveEffectManager.Register(
            CreateDoT(ElementType.Fire, 10, 3, null));

        monster.ActiveEffectManager.Register(
            CreateDoT(ElementType.Ice, 10, 3, null));

        var expectedException = new InvalidOperationException(
            "Simulated input failure.");

        var output = new CaptureOutputManager();
        var combatHelper = CreateThrowingCombatHelper(
            expectedException,
            output);

        // Act
        Assert.ThrowsException<InvalidOperationException>(() => combatHelper.CombatRunner(player, [monster]));

        // Assert
        Assert.AreEqual(
            0,
            player.ActiveEffectManager.ActiveEffects.Count,
            "Player effects should be cleared when combat exits through an exception.");

        Assert.AreEqual(
            0,
            monster.ActiveEffectManager.ActiveEffects.Count,
            "Monster effects should be cleared when combat exits through an exception.");
    }

    /// <summary>
    /// Cleanup does not swallow or replace the original exception.
    /// </summary>
    [TestMethod]
    public void CombatRunner_Exception_PropagatesOriginalException()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster("Test Monster", 100);

        var expectedException = new InvalidOperationException(
            "Original exception.");

        var output = new CaptureOutputManager();
        var combatHelper = CreateThrowingCombatHelper(
            expectedException,
            output);

        // Act
        var actualException =
            Assert.ThrowsException<InvalidOperationException>(() => combatHelper.CombatRunner(player, [monster]));

        // Assert
        Assert.AreSame(
            expectedException,
            actualException,
            "CombatRunner should propagate the original exception instance.");
    }

    /// <summary>
    /// Player starts the encounter with full resource power.
    /// </summary>
    [TestMethod]
    public void CombatRunner_EncounterStartsWithFullResources()
    {
        // Arrange
        var player = CreatePlayer(currentResource: 25, maxResource: 100);
        var monster = CreateTestMonster();

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(player.Archetype.MaxResource, player.Archetype.CurrentResource,
            "Player resource should be restored to max at encounter start.");
    }

    /// <summary>
    /// No active effects leak into a subsequent encounter.
    /// </summary>
    [TestMethod]
    public void CombatRunner_EncounterEndsWithNoActiveEffects()
    {
        // Arrange — first encounter
        var player = CreatePlayer();
        var monster1 = CreateTestMonster("Monster1");
        monster1.CurrentHealth = 1;
        EquipTestWeapon(player);

        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output1 = new CaptureOutputManager();
        var input1 = new QueueInputManager(intQueue: [1]); // Attack
        var combatHelper1 = CreateCombatHelper(input1, output1);

        // Act — first encounter
        combatHelper1.CombatRunner(player, [monster1]);

        // Assert — first encounter cleared effects
        Assert.AreEqual(0, player.ActiveEffectManager.ActiveEffects.Count,
            "Effects should be cleared after first encounter.");

        // Arrange — second encounter with the same player
        var monster2 = CreateTestMonster("Monster2");
        monster2.CurrentHealth = 1;

        var output2 = new CaptureOutputManager();
        var input2 = new QueueInputManager(intQueue: [1]); // Attack
        var combatHelper2 = CreateCombatHelper(input2, output2);

        // Act — second encounter
        combatHelper2.CombatRunner(player, [monster2]);

        // Assert — no effects leaked from first encounter
        Assert.AreEqual(0, player.ActiveEffectManager.ActiveEffects.Count,
            "No effects should leak into the second encounter.");
        Assert.IsTrue(monster2.CurrentHealth <= 0, "Second monster should be defeated.");
    }

    // ===========================================================================
    // 2. Death cleanup and dead-combatant processing
    // ===========================================================================

    /// <summary>
    /// Dead monster's own effects are cleared when killed by player attack.
    /// </summary>
    [TestMethod]
    public void CombatRunner_MonsterKilledByPlayerAttack_ClearsDeadMonsterEffects()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 1;
        EquipTestWeapon(player);

        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [1]); // Attack
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(monster.CurrentHealth <= 0, "Monster should be dead.");
        Assert.AreEqual(0, monster.ActiveEffectManager.ActiveEffects.Count,
            "Dead monster's effects should be cleared.");
    }

    /// <summary>
    /// Skill-caused death triggers identical cleanup.
    /// </summary>
    [TestMethod]
    public void CombatRunner_MonsterKilledByPlayerSkill_ClearsDeadMonsterEffects()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 1;

        var skill = new MagicSkill
        {
            Name = "Fireball",
            Description = "Test spell.",
            RequiredLevel = 1,
            Cost = 10,
            Power = 20,
            Cooldown = 2,
            TargetType = TargetType.SingleEnemy,
            SkillCategory = SkillCategory.Basic,
            DamageType = DamageType.Magical
        };
        skill.InitializeSkill(1);
        player.Archetype.Skills.Add(skill);

        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [2, 1]); // Use Skill, then skill index 1
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(monster.CurrentHealth <= 0, "Monster should be dead from skill.");
        Assert.AreEqual(0, monster.ActiveEffectManager.ActiveEffects.Count,
            "Dead monster's effects should be cleared after skill kill.");
    }

    /// <summary>
    /// DoT-caused death triggers identical cleanup.
    /// </summary>
    [TestMethod]
    public void CombatRunner_MonsterKilledByDoT_ClearsDeadMonsterEffects()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 5;

        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(monster.CurrentHealth <= 0, "Monster should be dead from DoT.");
        Assert.AreEqual(0, monster.ActiveEffectManager.ActiveEffects.Count,
            "Dead monster's effects should be cleared after DoT kill.");
    }

    /// <summary>
    /// Dead monster's support effects on player are removed.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DeadMonster_RemovesItsSupportEffectsFromPlayer()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var deadMonster = CreateTestMonster("Dead Monster", 1);
        deadMonster.CurrentHealth = 1;

        var observer = CreateEffectObservingMonster("Observer");

        RegisterSupportEffect(player, deadMonster);

        var supportEffectPresentWhenObserverActs = true;

        observer.OnAttack = _ =>
        {
            supportEffectPresentWhenObserverActs =
                player.ActiveEffectManager.ActiveEffects.Any(effect =>
                    ReferenceEquals(effect.SourceCaster, deadMonster));
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [deadMonster, observer]);

        // Assert
        Assert.IsFalse(
            supportEffectPresentWhenObserverActs,
            "The dead monster's support effect should be removed from the player before another monster acts.");
    }

    /// <summary>
    /// Dead monster's support effects on surviving monsters are removed.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DeadMonster_RemovesItsSupportEffectsFromOtherMonsters()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var deadMonster = CreateTestMonster("Dead Monster", 1);
        deadMonster.CurrentHealth = 1;

        var observer = CreateEffectObservingMonster("Observer");

        RegisterSupportEffect(observer, deadMonster);

        var supportEffectPresentWhenObserverActs = true;

        observer.OnAttack = _ =>
        {
            supportEffectPresentWhenObserverActs =
                observer.ActiveEffectManager.ActiveEffects.Any(effect =>
                    ReferenceEquals(effect.SourceCaster, deadMonster));
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [deadMonster, observer]);

        // Assert
        Assert.IsFalse(
            supportEffectPresentWhenObserverActs,
            "The dead monster's support effect should be removed from another monster before that monster acts.");
    }

    /// <summary>
    /// Elemental status effects and DoTs from dead source remain on survivors.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DeadMonster_PreservesItsElementalEffectsOnSurvivors()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var deadMonster = CreateTestMonster("Dead Monster", 1);
        deadMonster.CurrentHealth = 1;

        var observer = CreateEffectObservingMonster("Observer");

        var elementalEffect = CreateDoT(
            ElementType.Fire,
            power: 1,
            duration: 3,
            sourceCaster: deadMonster);

        observer.ActiveEffectManager.Register(elementalEffect);

        var elementalEffectPreservedWhenObserverActs = false;

        observer.OnAttack = _ =>
        {
            elementalEffectPreservedWhenObserverActs =
                observer.ActiveEffectManager.ActiveEffects.Any(effect => ReferenceEquals(effect, elementalEffect));
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [deadMonster, observer]);

        // Assert
        Assert.IsTrue(
            elementalEffectPreservedWhenObserverActs,
            "The dead monster's elemental effect should remain on the surviving monster.");
    }

    /// <summary>
    /// Dead monster cannot act again.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DeadMonster_DoesNotReceiveFurtherTurns()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var sourceMonster = CreateEffectObservingMonster(
            "Source Monster",
            health: 20,
            killAfterAttack: int.MaxValue);

        var observer = CreateEffectObservingMonster(
            "Observer",
            killAfterAttack: 2);

        var sourceAttackCountWhenObserverActs = -1;

        observer.OnAttack = _ => { sourceAttackCountWhenObserverActs = sourceMonster.AttackCount; };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3, 1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [sourceMonster, observer]);

        // Assert
        Assert.AreEqual(
            1,
            sourceAttackCountWhenObserverActs,
            "The source monster should have acted once before dying, but must not receive another turn after its death.");
    }

    /// <summary>
    /// Dead monster receives no further DoT processing.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DeadMonster_DoesNotProcessFurtherDoTTicks()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var sourceMonster = CreateEffectObservingMonster(
            "Source Monster",
            health: 20,
            killAfterAttack: int.MaxValue);

        var observer = CreateEffectObservingMonster(
            "Observer",
            killAfterAttack: 2);

        var dot = CreateDoT(
            ElementType.Fire,
            power: 1,
            duration: 3,
            sourceCaster: player);

        sourceMonster.ActiveEffectManager.Register(dot);

        var remainingDurationWhenObserverActs = -1;

        observer.OnAttack = _ => { remainingDurationWhenObserverActs = dot.RemainingDuration; };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3, 1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [sourceMonster, observer]);

        // Assert
        Assert.AreEqual(
            2,
            remainingDurationWhenObserverActs,
            "The DoT should tick on the source monster's first turn, but not again after the source monster dies.");
    }

    /// <summary>
    /// Monster killed at the beginning of its scheduled turn does not attack.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DoTKillsMonster_SkipsMonsterAction()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 5;

        // Register a DoT that will kill the monster on its first tick
        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn since player's speed is higher
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(monster.CurrentHealth <= 0, "Monster should be dead from DoT.");
        // The player should not have been attacked
        Assert.AreEqual(100, player.CurrentHealth, "Player should not have taken damage.");
    }

    /// <summary>
    /// Player killed by DoT does not execute their combat action.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerKilledByDoT_DoesNotTakeTurn()
    {
        // Arrange
        var player = CreatePlayer(health: 5);
        var monster = CreateTestMonster(speed: 0);

        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(); // No input should be consumed
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(player.CurrentHealth <= 0, "Player should be dead from DoT.");
        // The player should not have reached the combat menu
        Assert.AreEqual(0, output.WrittenLines.Count(l => l.Contains("Your turn")),
            "Player should not have taken a turn.");
    }

    // ===========================================================================
    // 3. Boss phase transitions and death
    // ===========================================================================

    /// <summary>
    /// Boss retains active effects across a phase transition.
    /// </summary>
    [TestMethod]
    public void CombatRunner_BossPhaseTransition_PreservesBossEffects()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var boss = CreateObservingBoss();

        var bossEffect = CreateElementalStatus(
            ElementalStatusEffectType.Charred,
            power: 5,
            duration: 5);

        boss.ActiveEffectManager.Register(bossEffect);

        var effectPreservedAtTransition = false;
        var remainingDurationAtTransition = -1;

        boss.OnPhaseTransition = () =>
        {
            effectPreservedAtTransition =
                boss.ActiveEffectManager.ActiveEffects.Any(effect => ReferenceEquals(effect, bossEffect));

            remainingDurationAtTransition = bossEffect.RemainingDuration;
        };

        // Keep combat running through the transition, then end it.
        var observer = CreateEffectObservingMonster(
            "Observer",
            speed: 0,
            killAfterAttack: 2);

        observer.OnAttack = _ =>
        {
            if (observer.AttackCount == 2)
                boss.CurrentHealth = 0;
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([1, 1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [boss, observer]);

        // Assert
        Assert.IsTrue(
            effectPreservedAtTransition,
            "The boss's active effect should survive its phase transition.");

        Assert.IsTrue(
            remainingDurationAtTransition > 0,
            "The boss's effect should still have remaining duration at the transition.");
    }

    /// <summary>
    /// Other combatants retain their effects when boss changes phase.
    /// </summary>
    [TestMethod]
    public void CombatRunner_BossPhaseTransition_PreservesEffectsOnOtherCombatants()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var boss = CreateObservingBoss();

        var playerEffect = CreateDoT(
            ElementType.Fire,
            power: 5,
            duration: 5);

        player.ActiveEffectManager.Register(playerEffect);

        var effectPreservedAtTransition = false;
        var remainingDurationAtTransition = -1;

        boss.OnPhaseTransition = () =>
        {
            effectPreservedAtTransition =
                player.ActiveEffectManager.ActiveEffects.Any(effect => ReferenceEquals(effect, playerEffect));

            remainingDurationAtTransition = playerEffect.RemainingDuration;
        };

        var observer = CreateEffectObservingMonster(
            "Observer",
            speed: 0,
            killAfterAttack: 2);

        observer.OnAttack = _ =>
        {
            if (observer.AttackCount == 2)
                boss.CurrentHealth = 0;
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([1, 1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [boss, observer]);

        // Assert
        Assert.IsTrue(
            effectPreservedAtTransition,
            "The player's elemental effect should survive the boss's phase transition.");

        Assert.IsTrue(
            remainingDurationAtTransition > 0,
            "The player's effect should still have remaining duration at the transition.");
    }

    /// <summary>
    /// Boss survives the transition and acts normally.
    /// </summary>
    [TestMethod]
    public void CombatRunner_BossPhaseTransition_BossStillGetsScheduledTurn()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var boss = CreateObservingBoss();

        var output = new CaptureOutputManager();
        var input = new QueueInputManager([1, 1, 1, 1, 1]);

        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [boss]);

        // Assert
        Assert.IsTrue(
            boss.CurrentHealth <= 0,
            "Boss should be defeated after all three phases.");

        Assert.AreEqual(
            5,
            boss.AttackResults.Count,
            "Boss should receive five scheduled attacks; its increased speed lets it act before the final player attack.");

        CollectionAssert.AreEqual(
            new[] { 30, 22, 22, 15, 15 },
            boss.MaxHealthAtAttack,
            "Boss should receive scheduled turns in all three phases, including its faster phase-three turn.");

        var totalDamage = boss.AttackResults.Sum(result => result.FinalStandardDamage + result.FinalElementalDamage);

        Assert.AreEqual(
            100 - totalDamage,
            player.CurrentHealth,
            "Player health should reflect the damage dealt by the boss's scheduled attacks.");

        Assert.IsTrue(
            output.WrittenLines.Any(line => line.Contains("phase 2")),
            "Boss should have transitioned to phase 2.");

        Assert.IsTrue(
            output.WrittenLines.Any(line => line.Contains("phase 3")),
            "Boss should have transitioned to phase 3.");
    }

    /// <summary>
    /// Final boss death clears its own effects.
    /// </summary>
    [TestMethod]
    public void CombatRunner_BossFinalPhaseDeath_ClearsBossEffects()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var boss = CreateObservingBoss(health: 20);

        var bossEffect = CreateElementalStatus(
            ElementalStatusEffectType.Charred,
            power: 5,
            duration: 10);

        boss.ActiveEffectManager.Register(bossEffect);

        var bossEffectCountAfterDeath = -1;
        var bossEffectDurationAfterDeath = -1;

        var observer = CreateEffectObservingMonster(
            "Observer",
            speed: 0,
            killAfterAttack: 3);

        observer.OnAttack = _ =>
        {
            if (boss.CurrentHealth <= 0)
            {
                bossEffectCountAfterDeath =
                    boss.ActiveEffectManager.ActiveEffects.Count;

                bossEffectDurationAfterDeath = bossEffect.RemainingDuration;
            }
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([1, 1, 1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [boss, observer]);

        // Assert
        Assert.IsTrue(boss.CurrentHealth <= 0, "Boss should be dead.");

        Assert.AreEqual(
            0,
            bossEffectCountAfterDeath,
            "Boss effects should be cleared immediately after its final death.");

        Assert.IsTrue(
            bossEffectDurationAfterDeath > 0,
            "The boss effect should still have duration remaining, proving it wasn't merely expired naturally.");
    }

    /// <summary>
    /// Final boss death removes its support effects from survivors.
    /// </summary>
    [TestMethod]
    public void CombatRunner_BossFinalPhaseDeath_RemovesBossSupportEffectsFromSurvivors()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var boss = CreateObservingBoss(health: 20);

        var supportSkill = new SupportSkill
        {
            Name = "Boss Buff",
            Duration = 10,
            Cooldown = 5,
            Power = 4,
            StatAffected = StatType.Speed,
            SupportEffect = SupportEffectType.Boost,
            TargetType = TargetType.Self,
            ElapsedTime = 5
        };

        supportSkill.InitializeSkill(1);
        boss.Skills.Add(supportSkill);

        var supportEffect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(supportSkill),
            power: 4,
            duration: 10,
            sourceCaster: boss,
            sourceSkill: supportSkill);

        player.ActiveEffectManager.Register(supportEffect);

        var supportEffectPresentAfterBossDeath = true;
        var supportEffectDurationAfterDeath = -1;

        var observer = CreateEffectObservingMonster(
            "Observer",
            speed: 0,
            killAfterAttack: 3);

        observer.OnAttack = _ =>
        {
            if (boss.CurrentHealth <= 0)
            {
                supportEffectPresentAfterBossDeath =
                    player.ActiveEffectManager.ActiveEffects.Any(effect => ReferenceEquals(effect, supportEffect));

                supportEffectDurationAfterDeath = supportEffect.RemainingDuration;
            }
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([1, 1, 1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [boss, observer]);

        // Assert
        Assert.IsFalse(
            supportEffectPresentAfterBossDeath,
            "The boss's support effect should be removed from the player when the boss dies.");

        Assert.IsTrue(
            supportEffectDurationAfterDeath > 0,
            "The support effect should still have duration remaining, proving it was removed because of boss death rather than natural expiration.");
    }

    /// <summary>
    /// Elemental effects from boss remain on survivors (until combat end).
    /// </summary>
    [TestMethod]
    public void CombatRunner_BossFinalPhaseDeath_PreservesElementalEffectsOnSurvivors()
    {
        // Arrange
        var player = CreatePlayer();
        EquipTestWeapon(player);

        var boss = CreateObservingBoss(health: 20);

        var elementalEffect = CreateDoT(
            ElementType.Fire,
            power: 1,
            duration: 10,
            sourceCaster: boss);

        player.ActiveEffectManager.Register(elementalEffect);

        var elementalEffectPreservedAfterBossDeath = false;
        var remainingDurationAfterBossDeath = -1;

        var observer = CreateEffectObservingMonster(
            "Observer",
            speed: 0,
            killAfterAttack: 3);

        observer.OnAttack = _ =>
        {
            if (boss.CurrentHealth <= 0)
            {
                elementalEffectPreservedAfterBossDeath =
                    player.ActiveEffectManager.ActiveEffects.Any(effect => ReferenceEquals(effect, elementalEffect));

                remainingDurationAfterBossDeath = elementalEffect.RemainingDuration;
            }
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([1, 1, 1]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [boss, observer]);

        // Assert
        Assert.IsTrue(
            elementalEffectPreservedAfterBossDeath,
            "The boss's elemental DoT should remain on the player after the boss dies.");

        Assert.IsTrue(
            remainingDurationAfterBossDeath > 0,
            "The elemental DoT should still have duration remaining after the boss dies.");
    }

    // ===========================================================================
    // 4. Effect timing and duration
    // ===========================================================================

    /// <summary>
    /// Player DoT ticks before player action.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerDoT_ProcessesAtBeginningOfPlayerTurn()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster();

        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(90, player.CurrentHealth,
            "Player should have taken 10 DoT damage on the first tick.");
    }

    /// <summary>
    /// Monster DoT ticks before monster action.
    /// </summary>
    [TestMethod]
    public void CombatRunner_MonsterDoT_ProcessesAtBeginningOfMonsterTurn()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = new RecordingMonster
        {
            Name = "Recording Monster",
            MaxHealth = 100,
            CurrentHealth = 50,
            AttackPower = 20,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = 1,
            DamageType = DamageType.Martial
        };

        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(40, monster.HealthAtScheduledTurn,
            "Monster should have taken 10 DoT damage on the first tick.");
    }

    private sealed class ThreeTurnMonster : Monster
    {
        private int _turnCount;

        public override AttackResolutionResult Attack(IPlayer target)
        {
            _turnCount++;

            // End combat after the third scheduled monster turn.
            if (_turnCount == 3)
                CurrentHealth = 0;

            // The monster does not damage the player.
            return new AttackResolutionResult(
                0, 0, DamageType.Martial,
                0, 0, null,
                false, false);
        }
    }

    /// <summary>
    /// Duration-three DoT produces exactly three scheduled ticks.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DurationThreeDoT_ProducesExactlyThreeTicks()
    {
        // Arrange
        var player = CreatePlayer(health: 100);
        var monster = new ThreeTurnMonster
        {
            Name = "Three Turn Monster",
            CurrentHealth = 100,
            MaxHealth = 100,
            AggressionLevel = 1,
            AttackPower = 1,
            DodgeChance = 0,
            DamageType = DamageType.Martial
        };

        // Duration 3 DoT with power 10 — should tick 3 times for 30 total damage
        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        // Skip turn each round — 3 rounds of DoT ticks
        var input = new QueueInputManager(intQueue: [3, 3, 3]);
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(70, player.CurrentHealth,
            "Player should have taken 30 damage from 3 DoT ticks (100 - 30 = 70).");
    }

    /// <summary>
    /// Newly applied DoT does not tick prematurely.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DoTAppliedDuringCombat_StartsAtNextScheduledTick()
    {
        // Arrange
        var player = CreatePlayer();

        var monster = CreateEffectObservingMonster(
            "Monster",
            killAfterAttack: 2);

        ActiveEffect? appliedDot = null;
        var durationImmediatelyAfterApplication = -1;

        monster.OnAttack = target =>
        {
            if (appliedDot != null)
                return;

            // The monster applies the DoT during its first scheduled attack.
            appliedDot = CreateDoT(
                ElementType.Fire,
                power: 10,
                duration: 3,
                sourceCaster: monster);

            target.ActiveEffectManager.Register(appliedDot);
            durationImmediatelyAfterApplication = appliedDot.RemainingDuration;
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3, 3]), // Skip both player turns
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsNotNull(appliedDot);

        Assert.AreEqual(
            3,
            durationImmediatelyAfterApplication,
            "The DoT must not tick immediately when applied during the monster's turn.");

        Assert.AreEqual(
            90,
            player.CurrentHealth,
            "The DoT should deal 10 damage at the beginning of the player's next scheduled turn.");

        Assert.AreEqual(
            2,
            appliedDot.RemainingDuration,
            "The DoT should consume exactly one duration when it ticks on the player's next turn.");
    }

    /// <summary>
    /// Once a DoT kills its target, subsequent DoTs do not process.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DoTDeath_StopsSubsequentEffects()
    {
        // Arrange
        var player = CreatePlayer(health: 5);
        var monster = CreateTestMonster(speed: 0);

        // Register two DoTs — the first one kills the player
        player.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));
        player.ActiveEffectManager.Register(CreateDoT(ElementType.Ice, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager();
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(player.CurrentHealth <= 0, "Player should be dead.");
        // The first DoT (Fire, 10 damage) kills the player (5 HP). The second DoT should not process.
        // Player health should be -5 (5 - 10 = -5), not -15 (5 - 10 - 10 = -15).
        Assert.AreEqual(-5, player.CurrentHealth,
            "Only the first DoT should have processed; the second should be skipped.");
    }

    /// <summary>
    /// A fully mitigated tick still consumes duration.
    /// </summary>
    [TestMethod]
    public void CombatRunner_ZeroDamageDoTTick_StillConsumesDuration()
    {
        // Arrange
        var player = CreatePlayer();

        var affectedMonster = CreateEffectObservingMonster(
            "Affected Monster",
            killAfterAttack: int.MaxValue);

        affectedMonster.Resilience = ElementType.Fire;

        var observer = CreateEffectObservingMonster(
            "Observer",
            killAfterAttack: 1);

        var dot = CreateDoT(
            ElementType.Fire,
            power: 10,
            duration: 3,
            sourceCaster: player);

        affectedMonster.ActiveEffectManager.Register(dot);

        var healthAfterDotTick = -1;
        var durationAfterDotTick = -1;

        observer.OnAttack = _ =>
        {
            healthAfterDotTick = affectedMonster.CurrentHealth;
            durationAfterDotTick = dot.RemainingDuration;

            // End the encounter after recording the state.
            affectedMonster.CurrentHealth = 0;
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [affectedMonster, observer]);

        // Assert
        Assert.AreEqual(
            100,
            healthAfterDotTick,
            "Fire resilience should prevent the DoT from damaging the affected monster.");

        Assert.AreEqual(
            2,
            durationAfterDotTick,
            "A zero-damage DoT tick must still consume one duration.");
    }

    /// <summary>
    /// Player's non-DoT effect decrements at the correct boundary.
    /// </summary>
    [TestMethod]
    public void CombatRunner_NonDoTEffect_DurationDecrementsAtEndOfPlayerTurn()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateEffectObservingMonster("Monster");

        RegisterSupportEffect(player, monster);

        var effect = player.ActiveEffectManager.ActiveEffects.Single();
        var durationAtMonsterTurn = -1;

        monster.OnAttack = _ => { durationAtMonsterTurn = effect.RemainingDuration; };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(
            2,
            durationAtMonsterTurn,
            "The player's non-DoT effect should lose one duration at the end of the player's turn.");
    }

    /// <summary>
    /// Monster's non-DoT effect decrements at the correct boundary.
    /// </summary>
    [TestMethod]
    public void CombatRunner_NonDoTEffect_DurationDecrementsAtEndOfMonsterTurn()
    {
        // Arrange
        var player = CreatePlayer();

        var affectedMonster = CreateEffectObservingMonster(
            "Affected Monster",
            killAfterAttack: 1);

        var observer = CreateEffectObservingMonster(
            "Observer",
            killAfterAttack: 1);

        RegisterSupportEffect(affectedMonster, affectedMonster);

        var effect = affectedMonster.ActiveEffectManager.ActiveEffects.Single();
        var durationAfterAffectedMonsterTurn = -1;

        observer.OnAttack = _ => { durationAfterAffectedMonsterTurn = effect.RemainingDuration; };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [affectedMonster, observer]);

        // Assert
        Assert.AreEqual(
            2,
            durationAfterAffectedMonsterTurn,
            "The non-DoT effect should lose one duration at the end of its owner's monster turn.");
    }

    /// <summary>
    /// Other effect advancement does not consume DoT duration.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DoTDuration_IsConsumedOnlyByScheduledTicks()
    {
        // Arrange
        var player = CreatePlayer();

        var affectedMonster = CreateEffectObservingMonster(
            "Affected Monster",
            killAfterAttack: 1);

        var observer = CreateEffectObservingMonster(
            "Observer",
            killAfterAttack: 1);

        var dot = CreateDoT(
            ElementType.Fire,
            power: 0,
            duration: 3,
            sourceCaster: player);

        affectedMonster.ActiveEffectManager.Register(dot);

        RegisterSupportEffect(affectedMonster, affectedMonster);

        var nonDotEffect = affectedMonster.ActiveEffectManager.ActiveEffects
            .Single(effect => effect.EffectType == ActiveEffectType.SupportStatModifier);

        var dotDurationAfterTurn = -1;
        var nonDotDurationAfterTurn = -1;

        observer.OnAttack = _ =>
        {
            dotDurationAfterTurn = dot.RemainingDuration;
            nonDotDurationAfterTurn = nonDotEffect.RemainingDuration;
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [affectedMonster, observer]);

        // Assert
        Assert.AreEqual(
            2,
            dotDurationAfterTurn,
            "The DoT should consume one duration through its scheduled tick, but not through general turn-based advancement.");

        Assert.AreEqual(
            2,
            nonDotDurationAfterTurn,
            "The non-DoT effect should consume one duration through end-of-turn advancement.");
    }

    // ===========================================================================
    // 5. Speed scheduling
    // ===========================================================================

    /// <summary>
    /// Effective Speed determines initial turn order.
    /// </summary>
    [TestMethod]
    public void CombatRunner_UsesEffectiveSpeedAtRoundStart()
    {
        // Arrange
        var order = new List<string>();

        var player = CreatePlayer(speed: 5);

        var fastMonster = CreateEffectObservingMonster(
            "Fast Monster",
            speed: 5);

        var mediumMonster = CreateEffectObservingMonster(
            "Medium Monster",
            speed: 10);

        RegisterSpeedBoostEffect(fastMonster, power: 10, duration: 3);

        fastMonster.OnAttack = _ => order.Add("Fast Monster");
        mediumMonster.OnAttack = _ => order.Add("Medium Monster");

        var input = new QueueInputManager([3])
        {
            OnIntDequeued = _ => order.Add("Player")
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [mediumMonster, fastMonster]);

        // Assert
        CollectionAssert.AreEqual(
            new[] { "Fast Monster", "Medium Monster", "Player" },
            order,
            "Turn order should use effective Speed, including active Speed modifiers.");
    }

    /// <summary>
    /// Player Speed changes do not alter remaining scheduled turns.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerSpeedChangesDuringRound_DoesNotReorderTurns()
    {
        // Arrange
        var order = new List<string>();

        var player = CreatePlayer(speed: 5);

        var firstMonster = CreateEffectObservingMonster(
            "First Monster",
            speed: 10,
            killAfterAttack: int.MaxValue);

        var secondMonster = CreateEffectObservingMonster(
            "Second Monster",
            speed: 1,
            killAfterAttack: int.MaxValue);

        firstMonster.OnAttack = _ =>
        {
            order.Add("First Monster");

            // The player should still retain their scheduled position
            // ahead of Second Monster this round.
            player.Archetype.Speed = 0;
        };

        secondMonster.OnAttack = target =>
        {
            order.Add("Second Monster");

            // End combat after recording the final scheduled action.
            target.CurrentHealth = 0;
        };

        var input = new QueueInputManager([3])
        {
            OnIntDequeued = _ => order.Add("Player")
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [firstMonster, secondMonster]);

        // Assert
        CollectionAssert.AreEqual(
            new[] { "First Monster", "Player", "Second Monster" },
            order,
            "Changing player Speed during a round must not change the already scheduled player turn.");
    }

    /// <summary>
    /// Monster Speed changes do not alter remaining scheduled turns.
    /// </summary>
    [TestMethod]
    public void CombatRunner_MonsterSpeedChangesDuringRound_DoesNotReorderTurns()
    {
        // Arrange
        var order = new List<string>();

        var player = CreatePlayer(speed: 5);

        var firstMonster = CreateEffectObservingMonster(
            "First Monster",
            speed: 10);

        var secondMonster = CreateEffectObservingMonster(
            "Second Monster",
            speed: 7);

        firstMonster.OnAttack = _ =>
        {
            order.Add("First Monster");

            // Second Monster becomes slower than the player.
            secondMonster.AggressionLevel = 0;
        };

        secondMonster.OnAttack = _ => order.Add("Second Monster");

        var input = new QueueInputManager([3])
        {
            OnIntDequeued = _ => order.Add("Player")
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [firstMonster, secondMonster]);

        // Assert
        CollectionAssert.AreEqual(
            new[] { "First Monster", "Second Monster", "Player" },
            order,
            "Changing a monster's Speed during a round must not reorder its already scheduled turn.");
    }

    /// <summary>
    /// Expiration affects the next round's schedule, not the current one.
    /// </summary>
    [TestMethod]
    public void CombatRunner_SpeedEffectExpiresDuringRound_AffectsNextRoundOnly()
    {
        // Arrange
        var order = new List<string>();
        var playerSpeedsAtTurn = new List<int>();

        var player = CreatePlayer(speed: 10);

        var monster = CreateEffectObservingMonster(
            "Monster",
            speed: 5,
            killAfterAttack: 2);

        var speedEffect = RegisterSpeedBoostEffect(
            monster,
            power: 10,
            duration: 1);

        monster.OnAttack = _ => order.Add("Monster");

        var input = new QueueInputManager([3, 3])
        {
            OnIntDequeued = _ =>
            {
                order.Add("Player");
                playerSpeedsAtTurn.Add(monster.GetStat(StatType.Speed));
            }
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        CollectionAssert.AreEqual(
            new[] { "Monster", "Player", "Player", "Monster" },
            order,
            "The monster should act first while boosted, but the player should act first after the effect expires.");

        CollectionAssert.AreEqual(
            new[] { 5, 5 },
            playerSpeedsAtTurn,
            "The monster's effective Speed should be back to its base value when the player acts.");

        Assert.AreEqual(
            0,
            speedEffect.RemainingDuration,
            "The one-turn Speed effect should expire at the end of the monster's first turn.");
    }

    /// <summary>
    /// Next round captures the updated effective Speed values.
    /// </summary>
    [TestMethod]
    public void CombatRunner_NewRound_UsesUpdatedSpeedValues()
    {
        // Arrange
        var order = new List<string>();

        var player = CreatePlayer(speed: 5);

        var monster = CreateEffectObservingMonster(
            "Monster",
            speed: 10,
            killAfterAttack: 2);

        monster.OnAttack = _ =>
        {
            order.Add("Monster");

            // Change player Speed after the first round's schedule
            // has already been created.
            if (monster.AttackCount == 1)
                player.Archetype.Speed = 20;
        };

        var input = new QueueInputManager([3, 3])
        {
            OnIntDequeued = _ => order.Add("Player")
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        CollectionAssert.AreEqual(
            new[] { "Monster", "Player", "Player", "Monster" },
            order,
            "The first round should retain its original order, while the second round should use the player's updated Speed.");

        Assert.AreEqual(
            20,
            player.GetStat(StatType.Speed),
            "The player's updated Speed should be used for subsequent rounds.");
    }

    // ===========================================================================
    // 6. Elemental integration
    // ===========================================================================

    private sealed class RecordingAttackingMonster : Monster
    {
        public AttackResolutionResult? LastAttackResult { get; private set; }

        public Action<IPlayer>? BeforeAttack { get; set; }

        public override AttackResolutionResult Attack(IPlayer target)
        {
            BeforeAttack?.Invoke(target);

            LastAttackResult = base.Attack(target);
            CurrentHealth = 0;

            return LastAttackResult;
        }
    }

    /// <summary>
    /// All six elemental DoTs process correctly.
    /// </summary>
    [TestMethod]
    [DataRow(ElementType.Fire)]
    [DataRow(ElementType.Lightning)]
    [DataRow(ElementType.Ice)]
    [DataRow(ElementType.Nature)]
    [DataRow(ElementType.Radiance)]
    [DataRow(ElementType.Abyssal)]
    public void CombatRunner_ElementalDoT_AllSixElements_ApplyCorrectDamage(ElementType element)
    {
        // Arrange
        var player = CreatePlayer(health: 100);
        var monster = CreateTestMonster();

        player.ActiveEffectManager.Register(CreateDoT(element, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(90, player.CurrentHealth,
            $"Player should have taken 10 {element} DoT damage.");
    }

    /// <summary>
    /// All six status effects apply their intended interactions.
    /// </summary>
    [TestMethod]
    [DataRow(ElementalStatusEffectType.Charred)]
    [DataRow(ElementalStatusEffectType.Shocked)]
    [DataRow(ElementalStatusEffectType.Snared)]
    [DataRow(ElementalStatusEffectType.Frozen)]
    [DataRow(ElementalStatusEffectType.Blinded)]
    [DataRow(ElementalStatusEffectType.Corrupted)]
    public void CombatRunner_ElementalStatus_AllSixElements_ApplyCorrectEffects(
        ElementalStatusEffectType statusType)
    {
        // Arrange
        var player = CreatePlayer(speed: 10);
        player.Archetype.DefenseBonus = 10;
        player.Archetype.ResistanceBonus = 10;

        var monster = CreateEffectObservingMonster("Observer");

        player.ActiveEffectManager.Register(
            CreateElementalStatus(statusType, power: 5, duration: 3));

        var observedValue = -1;

        monster.OnAttack = _ =>
        {
            observedValue = statusType switch
            {
                ElementalStatusEffectType.Charred =>
                    player.GetStat(StatType.Defense),

                ElementalStatusEffectType.Shocked =>
                    player.GetStat(StatType.Resistance),

                ElementalStatusEffectType.Snared =>
                    player.GetStat(StatType.Speed),

                ElementalStatusEffectType.Frozen =>
                    player.ActiveEffectManager.GetHealingReduction(),

                ElementalStatusEffectType.Blinded =>
                    player.ActiveEffectManager.GetIncomingDamageIncrease(),

                ElementalStatusEffectType.Corrupted =>
                    player.ActiveEffectManager.GetOutgoingDamageDecrease(),

                _ => throw new ArgumentOutOfRangeException(nameof(statusType))
            };
        };

        var input = new QueueInputManager([3]); // Skip Turn
        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        var expectedValue = statusType switch
        {
            ElementalStatusEffectType.Charred => 5, // Defense: 10 - 5
            ElementalStatusEffectType.Shocked => 5, // Resistance: 10 - 5
            ElementalStatusEffectType.Snared => 5, // Speed: 10 - 5
            ElementalStatusEffectType.Frozen => 5, // Healing reduction
            ElementalStatusEffectType.Blinded => 5, // Incoming damage increase
            ElementalStatusEffectType.Corrupted => 5, // Outgoing damage decrease
            _ => throw new ArgumentOutOfRangeException(nameof(statusType))
        };

        Assert.AreEqual(
            expectedValue,
            observedValue,
            $"{statusType} should apply its intended modifier.");
    }

    /// <summary>
    /// Corruption affects actual player outgoing damage.
    /// </summary>
    [TestMethod]
    public void CombatRunner_CorruptedPlayer_AttackDamageIsReduced()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 100;

        EquipTestWeapon(player); // 20 attack power

        // Apply corruption — reduces outgoing damage by 15
        player.ActiveEffectManager.Register(
            CreateElementalStatus(ElementalStatusEffectType.Corrupted, 15, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [1, 3]); // Attack, then Skip
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        var endingMonster = (EndingCombatMonster)monster;

        Assert.AreEqual(95, endingMonster.HealthAtScheduledTurn,
            "Corrupted player should deal reduced damage (20 - 15 = 5).");
    }

    /// <summary>
    /// Corruption affects actual monster outgoing damage.
    /// </summary>
    [TestMethod]
    public void CombatRunner_CorruptedMonster_AttackDamageIsReduced()
    {
        // Arrange
        var player = CreatePlayer(health: 100);
        player.DodgeChance = 0;

        var monster = new RecordingAttackingMonster
        {
            Name = "Corrupted Monster",
            CurrentHealth = 100,
            MaxHealth = 100,
            AttackPower = 20,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = 1,
            DodgeChance = 0,
            DamageType = DamageType.Martial,
            Strategy = new DefaultStrategy(new EmptySkillSelector())
        };

        monster.ActiveEffectManager.Register(
            CreateElementalStatus(ElementalStatusEffectType.Corrupted, 15, 3));

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3]), // Player skips; monster attacks
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsNotNull(monster.LastAttackResult);

        Assert.AreEqual(
            5,
            monster.LastAttackResult.FinalStandardDamage,
            "Corruption should reduce the monster's 20 attack damage by 15.");

        Assert.AreEqual(
            95,
            player.CurrentHealth,
            "The player should receive only 5 damage.");
    }

    /// <summary>
    /// Blind affects actual attack accuracy (increases incoming damage).
    /// </summary>
    [TestMethod]
    public void CombatRunner_BlindedCombatant_TakesIncreasedIncomingDamage()
    {
        // Arrange
        var player = CreatePlayer(health: 100);
        player.DodgeChance = 0;

        player.ActiveEffectManager.Register(
            CreateElementalStatus(ElementalStatusEffectType.Blinded, 5, 3));

        var monster = new RecordingAttackingMonster
        {
            Name = "Attacking Monster",
            CurrentHealth = 100,
            MaxHealth = 100,
            AttackPower = 20,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = 1,
            DodgeChance = 0,
            DamageType = DamageType.Martial,
            Strategy = new DefaultStrategy(new EmptySkillSelector())
        };

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsNotNull(monster.LastAttackResult);

        Assert.AreEqual(
            25,
            monster.LastAttackResult.FinalStandardDamage,
            "Blinded should add 5 incoming damage to the monster's 20 damage attack.");

        Assert.AreEqual(
            75,
            player.CurrentHealth,
            "The player should receive 25 damage while Blinded.");
    }

    /// <summary>
    /// Charred affects defense.
    /// </summary>
    [TestMethod]
    public void CombatRunner_CharredCombatant_DefenseIsReduced()
    {
        // Arrange
        var player = CreatePlayer(health: 100);
        player.DodgeChance = 0;
        player.Archetype.DefenseBonus = 10;

        player.ActiveEffectManager.Register(
            CreateElementalStatus(ElementalStatusEffectType.Charred, 5, 3));

        var monster = new RecordingAttackingMonster
        {
            Name = "Attacking Monster",
            CurrentHealth = 100,
            MaxHealth = 100,
            AttackPower = 20,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = 1,
            DodgeChance = 0,
            DamageType = DamageType.Martial,
            Strategy = new DefaultStrategy(new EmptySkillSelector())
        };

        // Capture Defense before the attack, while Charred is still active.
        var defenseAtAttack = -1;
        monster.BeforeAttack = _ =>
            defenseAtAttack = player.GetStat(StatType.Defense);

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(5, defenseAtAttack,
            "Charred should reduce effective Defense from 10 to 5.");

        Assert.IsNotNull(monster.LastAttackResult);
        Assert.AreEqual(15, monster.LastAttackResult.FinalStandardDamage,
            "Reduced Defense should allow 15 damage through.");
    }

    /// <summary>
    /// Shocked affects resistance.
    /// </summary>
    [TestMethod]
    public void CombatRunner_ShockedCombatant_ResistanceIsReduced()
    {
        // Arrange
        var player = CreatePlayer(health: 100);
        player.DodgeChance = 0;
        player.Archetype.ResistanceBonus = 10;

        player.ActiveEffectManager.Register(
            CreateElementalStatus(ElementalStatusEffectType.Shocked, 5, 3));

        var monster = new RecordingAttackingMonster
        {
            Name = "Attacking Monster",
            CurrentHealth = 100,
            MaxHealth = 100,
            AttackPower = 20,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = 1,
            DodgeChance = 0,
            DamageType = DamageType.Magical,
            Strategy = new DefaultStrategy(new EmptySkillSelector())
        };

        var resistanceAtAttack = -1;
        monster.BeforeAttack = _ =>
            resistanceAtAttack = player.GetStat(StatType.Resistance);

        var combatHelper = CreateCombatHelper(
            new QueueInputManager([3]),
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(
            5,
            resistanceAtAttack,
            "Shocked should reduce effective Resistance from 10 to 5.");

        Assert.IsNotNull(monster.LastAttackResult);

        Assert.AreEqual(
            15,
            monster.LastAttackResult.FinalStandardDamage,
            "Reduced Resistance should allow 15 Magical damage through.");

        Assert.AreEqual(
            85,
            player.CurrentHealth,
            "The player should receive 15 damage.");
    }

    /// <summary>
    /// Snared affects Speed and next-round scheduling.
    /// </summary>
    [TestMethod]
    public void CombatRunner_SnaredCombatant_SpeedIsReduced()
    {
        // Arrange
        var order = new List<string>();
        var player = CreatePlayer(speed: 10);

        player.ActiveEffectManager.Register(
            CreateElementalStatus(ElementalStatusEffectType.Snared, 5, 3));

        var monster = CreateEffectObservingMonster(
            "Monster",
            speed: 7,
            killAfterAttack: 2);

        var playerSpeedAtTurn = -1;

        monster.OnAttack = _ => order.Add("Monster");

        var input = new QueueInputManager([3, 3])
        {
            OnIntDequeued = _ =>
            {
                order.Add("Player");
                playerSpeedAtTurn = player.GetStat(StatType.Speed);
            }
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(
            5,
            playerSpeedAtTurn,
            "Snared should reduce the player's effective Speed from 10 to 5.");

        CollectionAssert.AreEqual(
            new[] { "Monster", "Player", "Monster", "Player" },
            order,
            "The faster monster should act before the Snared player in both rounds.");
    }

    /// <summary>
    /// Frozen affects actual healing.
    /// </summary>
    [TestMethod]
    public void CombatRunner_FrozenCombatant_HealingIsReduced()
    {
        // Arrange
        var player = CreatePlayer(health: 100);
        player.CurrentHealth = 80;
        var monster = CreateTestMonster();

        // Apply frozen — reduces healing by 10
        player.ActiveEffectManager.Register(CreateElementalStatus(ElementalStatusEffectType.Frozen, 10, 3));

        var potion = new Consumable
        {
            Name = "Health Potion",
            Description = "Restores health.",
            RequiredLevel = 1,
            Power = 20,
            Durability = 1,
            ConsumableType = ConsumableType.Health,
            Inventory = player.Inventory
        };
        player.Inventory.Items.Add(potion);

        var output = new CaptureOutputManager();
        // Use Consumable (3), select the potion, then Skip (4)
        var input = new QueueInputManager(
            intQueue: [3, 4],
            stringQueue: ["n"],
            itemQueue: [potion, null]);
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        // Player had 80 HP, potion heals 20, frozen reduces by 10, so actual healing is 10.
        // Player should have 90 HP (80 + 10 = 90).
        Assert.AreEqual(90, player.CurrentHealth,
            "Frozen player should receive reduced healing (20 - 10 = 10).");
    }

    private sealed class RecordingMonster : Monster
    {
        public int HealthAtScheduledTurn { get; private set; }

        public override AttackResolutionResult Attack(IPlayer target)
        {
            HealthAtScheduledTurn = CurrentHealth;
            CurrentHealth = 0;

            return new AttackResolutionResult(0, 0, DamageType.Martial, 0, 0, null, false, false);
        }
    }

    /// <summary>
    /// Resilience correctly mitigates scheduled DoT damage.
    /// </summary>
    [TestMethod]
    public void CombatRunner_ElementalDoT_ResilienceNegatesDamage()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = new RecordingMonster
        {
            Name = "Recording Monster",
            MaxHealth = 100,
            CurrentHealth = 100,
            AttackPower = 20,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = 1,
            DamageType = DamageType.Martial,
            Resilience = ElementType.Fire
        };

        monster.Resilience = ElementType.Fire;
        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(100, monster.HealthAtScheduledTurn,
            "Monster with Fire resilience should take 0 Fire DoT damage.");
    }

    /// <summary>
    /// Vulnerability correctly increases scheduled DoT damage.
    /// </summary>
    [TestMethod]
    public void CombatRunner_ElementalDoT_VulnerabilityIncreasesDamage()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = new RecordingMonster
        {
            Name = "Recording Monster",
            MaxHealth = 100,
            CurrentHealth = 100,
            AttackPower = 20,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = 1,
            DamageType = DamageType.Martial,
            Vulnerability = ElementType.Fire
        };

        monster.Vulnerability = ElementType.Fire;
        monster.ActiveEffectManager.Register(CreateDoT(ElementType.Fire, 10, 3));

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn
        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        // Vulnerability increases damage by 1.5x: 10 * 1.5 = 15
        Assert.AreEqual(85, monster.HealthAtScheduledTurn,
            "Monster vulnerable to Fire should take 15 Fire DoT damage (10 * 1.5).");
    }

    /// <summary>
    /// Stronger refresh retains original DoT source ownership.
    /// </summary>
    [TestMethod]
    public void CombatRunner_RefreshedDoT_PreservesOriginalSource()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = new RecordingMonster
        {
            Name = "Recording Monster",
            MaxHealth = 100,
            CurrentHealth = 100,
            AttackPower = 20,
            DefensePower = 0,
            Resistance = 0,
            AggressionLevel = 1,
            DamageType = DamageType.Martial
        };

        var sourceCaster = new Player
        {
            Name = "Source",
            Level = 1,
            CurrentHealth = 100,
            MaxHealth = 100,
            Archetype = new Archetype
            {
                Name = "Source Archetype",
                ArchetypeType = ArchetypeType.Martial,
                Skills = new List<Skill>(),
                CurrentResource = 100,
                MaxResource = 100,
                RecoveryRate = 0
            }
        };
        sourceCaster.Inventory = new Inventory
        {
            Player = sourceCaster,
            Items = new List<Item>(),
            Runes = new List<ConsoleGameEntities.Models.Entities.Inventories.InventoryRune>(),
            Ingredients = new List<ConsoleGameEntities.Models.Entities.Inventories.InventoryIngredient>()
        };

        // Source A applies the original DoT.
        var originalDoT = CreateDoT(
            ElementType.Fire, 10, 3,
            sourceCaster: sourceCaster);

        // Source B applies a stronger DoT of the same element.
        var secondCaster = CreatePlayer();

        var strongerDoT = CreateDoT(
            ElementType.Fire, 20, 5,
            sourceCaster: secondCaster);

        monster.ActiveEffectManager.Register(originalDoT);
        monster.ActiveEffectManager.Register(strongerDoT);

        // Verify refresh ownership before CombatRunner's cleanup can clear it.
        var refreshedEffect = monster.ActiveEffectManager.ActiveEffects.Single();

        Assert.AreSame(sourceCaster, refreshedEffect.SourceCaster,
            "Refreshing a DoT should preserve its original source caster.");

        var output = new CaptureOutputManager();
        var input = new QueueInputManager(intQueue: [3]); // Skip Turn
        var combatHelper = CreateCombatHelper(input, output);

// Act
        combatHelper.CombatRunner(player, [monster]);

// Assert
        Assert.AreEqual(80, monster.HealthAtScheduledTurn,
            "The refreshed Fire DoT should deal 20 damage.");
    }

    /// <summary>
    /// Elemental damage triggers boss phase transition.
    /// </summary>
    [TestMethod]
    public void CombatRunner_ElementalDoT_TriggersBossPhaseTransition()
    {
        // Arrange
        var player = CreatePlayer(health: 5);

        var boss = CreateObservingBoss(
            health: 20,
            attackPower: 20,
            speed: 11);

        boss.ActiveEffectManager.Register(
            CreateDoT(
                ElementType.Fire,
                power: 20,
                duration: 3,
                sourceCaster: player));

        var output = new CaptureOutputManager();

        var combatHelper = CreateCombatHelper(
            new QueueInputManager(),
            output);

        // Act
        combatHelper.CombatRunner(player, [boss]);

        // Assert
        Assert.IsTrue(
            player.CurrentHealth <= 0,
            "The boss should defeat the player after its phase transition.");

        Assert.AreEqual(
            15,
            boss.MaxHealth,
            "The 20-point Fire DoT should defeat the first phase and transition the boss to phase two.");

        Assert.AreEqual(
            15,
            boss.CurrentHealth,
            "The boss should have its phase-two health after transitioning.");

        Assert.IsTrue(
            output.WrittenLines.Any(line => line.Contains("phase 2")),
            "The elemental damage should trigger the phase-two transition.");
    }

    private sealed class LethalMonster : Monster
    {
        public int AttackCount { get; private set; }

        public Action? OnAttack { get; set; }

        public override AttackResolutionResult Attack(IPlayer target)
        {
            AttackCount++;
            OnAttack?.Invoke();

            target.CurrentHealth = 0;

            return new AttackResolutionResult(
                0, 0, DamageType.Martial,
                0, 0, null,
                false, false);
        }
    }

    // ===========================================================================
// 7. Additional combat scheduling and lifecycle integration
// ===========================================================================

    /// <summary>
    /// Player wins a Speed tie against a monster.
    /// </summary>
    [TestMethod]
    public void CombatRunner_EqualSpeed_PlayerActsBeforeMonster()
    {
        // Arrange
        var order = new List<string>();

        var player = CreatePlayer(speed: 5);

        var monster = CreateEffectObservingMonster(
            "Equal Speed Monster",
            speed: 5);

        monster.OnAttack = _ => order.Add("Monster");

        var input = new QueueInputManager([3])
        {
            OnIntDequeued = _ => order.Add("Player")
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        CollectionAssert.AreEqual(
            new[] { "Player", "Monster" },
            order,
            "The player should act before a monster with equal effective Speed.");
    }

    /// <summary>
    /// The player receives exactly one scheduled turn per round,
    /// regardless of the number of living monsters.
    /// </summary>
    [TestMethod]
    public void CombatRunner_MultipleMonsters_PlayerActsOnlyOncePerRound()
    {
        // Arrange
        var order = new List<string>();

        var player = CreatePlayer(speed: 10);

        var firstMonster = CreateEffectObservingMonster(
            "First Monster",
            speed: 5);

        var secondMonster = CreateEffectObservingMonster(
            "Second Monster",
            speed: 1);

        firstMonster.OnAttack = _ => order.Add("First Monster");
        secondMonster.OnAttack = _ => order.Add("Second Monster");

        var input = new QueueInputManager([3, 3])
        {
            OnIntDequeued = _ => order.Add("Player")
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [firstMonster, secondMonster]);

        // Assert
        CollectionAssert.AreEqual(
            new[] { "Player", "First Monster", "Second Monster" },
            order,
            "The player should receive exactly one turn before the scheduled monster turns.");

        Assert.AreEqual(
            1,
            firstMonster.AttackCount,
            "The first monster should act once.");

        Assert.AreEqual(
            1,
            secondMonster.AttackCount,
            "The second monster should act once.");
    }

    /// <summary>
    /// A monster killed by the player before its scheduled turn is skipped.
    /// </summary>
    [TestMethod]
    public void CombatRunner_MonsterKilledBeforeScheduledTurn_DoesNotAct()
    {
        // Arrange
        var player = CreatePlayer(speed: 10);
        EquipTestWeapon(player);

        var monster = CreateEffectObservingMonster(
            "Defeated Monster",
            health: 1,
            speed: 1);

        var input = new QueueInputManager([1]); // Attack

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(
            monster.CurrentHealth <= 0,
            "The player should defeat the monster.");

        Assert.AreEqual(
            0,
            monster.AttackCount,
            "A monster killed before its scheduled turn must not attack.");
    }

    /// <summary>
    /// When a monster kills the player, all remaining scheduled turns stop.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerKilledByMonsterAttack_StopsRemainingTurns()
    {
        // Arrange
        var order = new List<string>();

        var player = CreatePlayer(
            speed: 1,
            health: 5);

        player.DodgeChance = 0;

        var lethalMonster = new LethalMonster
        {
            Name = "Lethal Monster",
            CurrentHealth = 100,
            MaxHealth = 100,
            AggressionLevel = 10,
            AttackPower = 100,
            DamageType = DamageType.Martial
        };

        lethalMonster.OnAttack = () => order.Add("Lethal Monster");

        var remainingMonster = CreateEffectObservingMonster(
            "Remaining Monster",
            speed: 5,
            killAfterAttack: int.MaxValue);

        remainingMonster.OnAttack = _ => order.Add("Remaining Monster");

        var input = new QueueInputManager()
        {
            OnIntDequeued = _ => order.Add("Player")
        };

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(
            player,
            [lethalMonster, remainingMonster]);

        // Assert
        Assert.IsTrue(
            player.CurrentHealth <= 0,
            "The lethal monster should kill the player.");

        CollectionAssert.AreEqual(
            new[] { "Lethal Monster" },
            order,
            "No remaining monster or player turns should execute after player death.");

        Assert.AreEqual(
            0,
            remainingMonster.AttackCount,
            "The remaining monster must not act after the player dies.");
    }

    /// <summary>
    /// Monsters that are already dead when combat begins are excluded.
    /// </summary>
    [TestMethod]
    public void CombatRunner_DeadMonsterAtEncounterStart_DoesNotAct()
    {
        // Arrange
        var player = CreatePlayer(speed: 10);

        var deadMonster = CreateEffectObservingMonster(
            "Already Dead Monster");

        deadMonster.CurrentHealth = 0;

        var livingMonster = CreateEffectObservingMonster(
            "Living Monster",
            speed: 1);

        var input = new QueueInputManager([3]);

        var combatHelper = CreateCombatHelper(
            input,
            new CaptureOutputManager());

        // Act
        combatHelper.CombatRunner(
            player,
            [deadMonster, livingMonster]);

        // Assert
        Assert.AreEqual(
            0,
            deadMonster.AttackCount,
            "A monster that is dead at encounter start must not receive a scheduled turn.");

        Assert.AreEqual(
            1,
            livingMonster.AttackCount,
            "The living monster should receive its scheduled turn.");
    }

    /// <summary>
    /// Combat does not start a round when every monster is already dead.
    /// </summary>
    [TestMethod]
    public void CombatRunner_NoLivingMonsters_DoesNotStartRound()
    {
        // Arrange
        var player = CreatePlayer(
            currentResource: 25,
            maxResource: 100,
            recoveryRate: 10);

        var firstDeadMonster = CreateEffectObservingMonster(
            "First Dead Monster");

        var secondDeadMonster = CreateEffectObservingMonster(
            "Second Dead Monster");

        firstDeadMonster.CurrentHealth = 0;
        secondDeadMonster.CurrentHealth = 0;

        var inputCount = 0;

        var input = new QueueInputManager()
        {
            OnIntDequeued = _ => inputCount++
        };

        var output = new CaptureOutputManager();

        var combatHelper = CreateCombatHelper(input, output);

        // Act
        combatHelper.CombatRunner(
            player,
            [firstDeadMonster, secondDeadMonster]);

        // Assert
        Assert.AreEqual(
            0,
            inputCount,
            "No player input should be requested when there are no living monsters.");

        Assert.AreEqual(
            player.Archetype.CurrentResource,
            player.Archetype.MaxResource,
            "Player resources should be at full at the start of every combat encounter.");

        Assert.AreEqual(
            0,
            firstDeadMonster.AttackCount,
            "The first dead monster must not act.");

        Assert.AreEqual(
            0,
            secondDeadMonster.AttackCount,
            "The second dead monster must not act.");

        Assert.AreEqual(
            0,
            output.WrittenLines.Count(line => line.Contains("Your turn")),
            "The player should not receive a combat turn when no living monsters exist.");
    }
}