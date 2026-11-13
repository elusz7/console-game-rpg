using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class EliteStrategyTests
{
    // Dummy ultimate skill that is never ready (Cooldown > ElapsedTime)
    private class NotReadyUltimateSkill : UltimateSkill
    {
        public bool Activated { get; private set; } = false;

        public NotReadyUltimateSkill()
        {
            Cooldown = 999; // ElapsedTime defaults to 0, so IsReady = 0 >= 999 = false
        }

        public override void Activate(ITargetable caster, ITargetable? singleEnemy = null,
            List<ITargetable>? multipleEnemies = null)
        {
            Activated = true;
        }
    }
    [TestMethod]
    public void ExecuteAttack_UsesUltimateSkill_WhenAvailable()
    {
        var ultimateSkill = new DummyUltimateSkill();
        var skillSelector = new FakeSkillSelector(ultimateSkill: ultimateSkill);

        var fallbackStrategy = new DummyFallbackStrategy();

        // Mock Monster.GetStrategyFor to return fallbackStrategy
        // Here we simulate it by injecting fallbackStrategy directly into EliteStrategy _strategy via reflection or a test helper
        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        // Replace the internal _strategy with dummy fallbackStrategy for test
        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, fallbackStrategy);

        var monster = new DummyMonster
        {
            Strategy = eliteStrategy
        };

        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(ultimateSkill.Activated, "Ultimate skill should be activated");
        Assert.IsFalse(fallbackStrategy.AttackExecuted,
            "Fallback strategy should NOT execute when ultimate skill is used");
    }

    [TestMethod]
    public void ExecuteAttack_FallsBackToBehaviorStrategy_WhenNoUltimateSkill()
    {
        var skillSelector = new FakeSkillSelector(); // No ultimate skill

        var fallbackStrategy = new DummyFallbackStrategy();

        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        // Replace the internal _strategy with dummy fallbackStrategy for test
        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, fallbackStrategy);

        var monster = new DummyMonster
        {
            Strategy = eliteStrategy
        };

        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(fallbackStrategy.AttackExecuted, "Fallback strategy should execute when no ultimate skill");
    }

    // ── Elemental Damage ──

    [TestMethod]
    public void ExecuteAttack_FallbackStrategy_WithElementalDamage()
    {
        var skillSelector = new FakeSkillSelector(); // No ultimate skill

        // Use a real DefaultStrategy as fallback so it creates AttackDamage
        var innerSelector = new FakeSkillSelector();
        var defaultStrategy = new DefaultStrategy(innerSelector);

        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        // Replace the internal _strategy with DefaultStrategy for test
        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, defaultStrategy);

        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = eliteStrategy
        };

        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_FallbackStrategy_WithoutElementalProperties_IsNeutral()
    {
        var skillSelector = new FakeSkillSelector(); // No ultimate skill

        var innerSelector = new FakeSkillSelector();
        var defaultStrategy = new DefaultStrategy(innerSelector);

        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, defaultStrategy);

        var monster = new DummyMonster()
        {
            Strategy = eliteStrategy
        };

        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire)]
    [DataRow(ElementType.Lightning)]
    [DataRow(ElementType.Ice)]
    [DataRow(ElementType.Nature)]
    [DataRow(ElementType.Radiance)]
    [DataRow(ElementType.Abyssal)]
    public void ExecuteAttack_FallbackStrategy_AllElements(ElementType element)
    {
        var skillSelector = new FakeSkillSelector(); // No ultimate skill

        var innerSelector = new FakeSkillSelector();
        var defaultStrategy = new DefaultStrategy(innerSelector);

        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, defaultStrategy);

        var monster = new DummyMonster(attackElement: element, elementalPower: 3)
        {
            Strategy = eliteStrategy
        };

        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.AreEqual(3, player.ElementalDamageTaken);
        Assert.AreEqual(element, player.ElementDealt);
    }

    // ── Boundary and Decision Coverage ──

    [TestMethod]
    public void ExecuteAttack_UltimateNotReady_UsesBehaviorStrategy()
    {
        // When an ultimate skill exists but IsReady returns false,
        // the elite strategy should fall back to the behavior strategy.
        var notReadyUltimate = new NotReadyUltimateSkill();
        var skillSelector = new FakeSkillSelector(ultimateSkill: notReadyUltimate);

        var fallbackStrategy = new DummyFallbackStrategy();
        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, fallbackStrategy);

        var monster = new DummyMonster
        {
            Strategy = eliteStrategy
        };

        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsFalse(notReadyUltimate.Activated, "Not-ready ultimate should NOT activate");
        Assert.IsTrue(fallbackStrategy.AttackExecuted, "Fallback strategy should execute when ultimate is not ready");
    }

    [TestMethod]
    public void ExecuteAttack_DelegatesToConfiguredBehaviorStrategy()
    {
        // When no ultimate skill is available, the elite strategy should delegate
        // to the configured behavior strategy, which should receive the attack.
        var skillSelector = new FakeSkillSelector(); // No ultimate skill

        var innerSelector = new FakeSkillSelector();
        var defaultStrategy = new DefaultStrategy(innerSelector);

        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, defaultStrategy);

        var monster = new DummyMonster
        {
            Strategy = eliteStrategy
        };

        var player = new DummyPlayer();

        monster.Attack(player);

        // The delegated DefaultStrategy should perform a basic attack
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UltimateAvailable_DoesNotPerformBasicAttack()
    {
        // When the ultimate skill activates, no basic attack should follow.
        var ultimateSkill = new DummyUltimateSkill();
        var skillSelector = new FakeSkillSelector(ultimateSkill: ultimateSkill);

        var fallbackStrategy = new DummyFallbackStrategy();
        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, fallbackStrategy);

        var monster = new DummyMonster
        {
            Strategy = eliteStrategy
        };

        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(ultimateSkill.Activated);
        Assert.IsFalse(fallbackStrategy.AttackExecuted, "No basic attack after ultimate");
        Assert.AreEqual(0, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_WithCorruptedEffect_DelegatesCorruptedHandling()
    {
        // When the elite strategy delegates to the inner strategy for a basic attack,
        // corrupted should still reduce damage through the shared MakeBasicAttack method.
        var skillSelector = new FakeSkillSelector(); // No ultimate skill

        var innerSelector = new FakeSkillSelector();
        var defaultStrategy = new DefaultStrategy(innerSelector);

        var eliteStrategy = new EliteStrategy(skillSelector, MonsterBehaviorType.Default);

        typeof(EliteStrategy)
            .GetField("_strategy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(eliteStrategy, defaultStrategy);

        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = eliteStrategy
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 10,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        var player = new DummyPlayer();

        monster.Attack(player);

        // Standard damage = 50 - 10 = 40 (DefaultStrategy has 1.0x multiplier)
        Assert.AreEqual(monster.AttackPower - 10, player.DamageTaken);
        // Elemental damage = max(0, 5 - 10) = 0
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }
}