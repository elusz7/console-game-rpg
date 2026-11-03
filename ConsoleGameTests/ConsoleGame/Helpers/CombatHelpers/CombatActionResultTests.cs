using ConsoleGame.Helpers.CombatHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleGameTests.ConsoleGame.Helpers.CombatHelpers;

[TestClass]
public class CombatActionResultTests
{
    [TestMethod]
    public void Success_ReturnsSuccessfulResultAndConsumesTurn()
    {
        var result = CombatActionResult.Success();

        Assert.AreEqual(CombatActionOutcome.Success, result.Outcome);
        Assert.IsTrue(result.TurnConsumed);
    }

    [TestMethod]
    public void Failed_ReturnsFailedResultWithoutConsumingTurn()
    {
        var result = CombatActionResult.Failed();

        Assert.AreEqual(CombatActionOutcome.Failed, result.Outcome);
        Assert.IsFalse(result.TurnConsumed);
    }

    [TestMethod]
    public void Cancelled_ReturnsCancelledResultWithoutConsumingTurn()
    {
        var result = CombatActionResult.Cancelled();

        Assert.AreEqual(CombatActionOutcome.Cancelled, result.Outcome);
        Assert.IsFalse(result.TurnConsumed);
    }

    [TestMethod]
    public void Skipped_ReturnsSkippedResultAndConsumesTurn()
    {
        var result = CombatActionResult.Skipped();

        Assert.AreEqual(CombatActionOutcome.TurnSkipped, result.Outcome);
        Assert.IsTrue(result.TurnConsumed);
    }
}