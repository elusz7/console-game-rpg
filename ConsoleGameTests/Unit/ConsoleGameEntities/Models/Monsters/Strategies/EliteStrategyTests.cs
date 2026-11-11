using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class EliteStrategyTests
{
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
}