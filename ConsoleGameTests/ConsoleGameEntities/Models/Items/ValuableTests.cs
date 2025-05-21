using ConsoleGameEntities.Models.Items;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Items
{
    [TestClass]
    public class ValuableTests
    {
        private static Valuable CreateValuable(decimal value = 100, int requiredLevel = 1, int durability = 2)
        {
            return new Valuable
            {
                Name = "Test Valuable",
                Value = value,
                RequiredLevel = requiredLevel,
                Durability = durability
            };
        }

        [TestMethod]
        public void Use_ThrowsInvalidOperationException()
        {
            var valuable = CreateValuable();
            Assert.ThrowsException<InvalidOperationException>(() => valuable.Use());
        }

        [TestMethod]
        public void RecoverDurability_ThrowsInvalidOperationException()
        {
            var valuable = CreateValuable();
            Assert.ThrowsException<InvalidOperationException>(() => valuable.RecoverDurability(1));
        }

        [TestMethod]
        public void GetSellPrice_ReturnsValue()
        {
            var valuable = CreateValuable(value: 123.45M);
            Assert.AreEqual(123.45M, valuable.GetSellPrice());
        }

        [TestMethod]
        public void CalculateValue_SetsValueGreaterThanZero()
        {
            var valuable = CreateValuable(requiredLevel: 3, durability: 2);
            valuable.CalculateValue();
            Assert.IsTrue(valuable.Value > 0);
        }

        [TestMethod]
        public void CalculateStatsByLevel_SetsDurabilityBasedOnRequiredLevel()
        {
            var valuable = CreateValuable(requiredLevel: 5, durability: 0);
            valuable.CalculateStatsByLevel();
            Assert.AreEqual(5 % 2 + 1, valuable.Durability);
        }

        [TestMethod]
        public void CalculateLevelByStats_SetsRequiredLevelBasedOnValueAndDurability()
        {
            var valuable = CreateValuable(value: 50, durability: 2);
            valuable.CalculateLevelByStats();
            Assert.IsTrue(valuable.RequiredLevel >= 1);
        }

        [TestMethod]
        public void CalculateLevelByStats_SetsRequiredLevelToOneIfEstimatedBaseValueIsZeroOrNegative()
        {
            var valuable = CreateValuable(value: 2, durability: 2); // durabilityValue = 3, so estimatedBaseValue = -1
            valuable.CalculateLevelByStats();
            Assert.AreEqual(1, valuable.RequiredLevel);
        }
    }
}
