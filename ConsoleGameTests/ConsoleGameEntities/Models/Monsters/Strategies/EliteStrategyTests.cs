using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters.Strategies;

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
        Assert.IsFalse(fallbackStrategy.AttackExecuted, "Fallback strategy should NOT execute when ultimate skill is used");
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
}
