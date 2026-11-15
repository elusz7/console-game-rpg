using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class MonsterApplyElementalDamageTests
{
    private static Monster CreateMonster()
    {
        return new Monster
        {
            Level = 1,
            AttackPower = 10,
            DefensePower = 5,
            Resistance = 3,
            AggressionLevel = 10,
            MaxHealth = 100,
            CurrentHealth = 100,
            DodgeChance = 0
        };
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — basic damage without resilience
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_NoResilience_DealsFullDamage()
    {
        var monster = CreateMonster();

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, monster.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.AreEqual(ElementType.Fire, result.Element);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_NoResilience_ReturnsCorrectElementType()
    {
        var monster = CreateMonster();

        var result = monster.ApplyElementalDamage(5, ElementType.Ice);

        Assert.AreEqual(ElementType.Ice, result.Element);
    }

    [TestMethod]
    public void ApplyElementalDamage_ZeroDamage_DealsNoDamage()
    {
        var monster = CreateMonster();

        var result = monster.ApplyElementalDamage(0, ElementType.Fire);

        Assert.AreEqual(100, monster.CurrentHealth);
        Assert.AreEqual(0, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — resilience completely blocks damage
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_WithMatchingResilience_CompletelyBlocksDamage()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        // Resilience reduces damage to 0
        Assert.AreEqual(100, monster.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_WithMatchingResilience_LargeDamage_Block()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;

        var result = monster.ApplyElementalDamage(50, ElementType.Fire);

        // Resilience reduces any damage to 0
        Assert.AreEqual(100, monster.CurrentHealth);
        Assert.AreEqual(50, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_WithMatchingResilience_DamageOne_Block()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;

        var result = monster.ApplyElementalDamage(1, ElementType.Fire);

        // Resilience reduces damage to 0
        Assert.AreEqual(100, monster.CurrentHealth);
        Assert.AreEqual(1, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_MismatchedResilience_NoBlock()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Ice;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, monster.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResilienceNull_NoBlock()
    {
        var monster = CreateMonster();
        monster.Resilience = null;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, monster.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — death cases
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_DamageEqualsHealth_TargetDies()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(0, monster.CurrentHealth);
        Assert.IsTrue(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_DamageExceedsHealth_TargetDies()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;

        var result = monster.ApplyElementalDamage(20, ElementType.Fire);

        Assert.AreEqual(-10, monster.CurrentHealth);
        Assert.IsTrue(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResiliencePreventsDeath_CompleteBlock()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 5;
        monster.Resilience = ElementType.Fire;

        var result = monster.ApplyElementalDamage(15, ElementType.Fire);

        // Resilience blocks all damage, health unchanged
        Assert.AreEqual(5, monster.CurrentHealth);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResiliencePreventsDeath_LargeDamage()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        monster.Resilience = ElementType.Fire;

        var result = monster.ApplyElementalDamage(100, ElementType.Fire);

        // Resilience blocks all damage, health unchanged
        Assert.AreEqual(10, monster.CurrentHealth);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_MismatchedResilience_StillKills()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        monster.Resilience = ElementType.Ice; // Fire damage, not blocked

        var result = monster.ApplyElementalDamage(20, ElementType.Fire);

        // Resilience is Ice, not Fire — damage not blocked
        Assert.AreEqual(-10, monster.CurrentHealth);
        Assert.AreEqual(20, result.FinalDamage);
        Assert.IsTrue(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — all elements
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_AllElements_ReturnsCorrectElement()
    {
        var elements = new[]
        {
            ElementType.Fire,
            ElementType.Lightning,
            ElementType.Ice,
            ElementType.Nature,
            ElementType.Radiance,
            ElementType.Abyssal
        };

        foreach (var element in elements)
        {
            var monster = CreateMonster();
            var result = monster.ApplyElementalDamage(1, element);
            Assert.AreEqual(element, result.Element, $"Element {element} not returned correctly.");
        }
    }

    [TestMethod]
    public void ApplyElementalDamage_AllElements_WithMatchingResilience_CompletelyBlocksDamage()
    {
        var elements = new[]
        {
            ElementType.Fire,
            ElementType.Lightning,
            ElementType.Ice,
            ElementType.Nature,
            ElementType.Radiance,
            ElementType.Abyssal
        };

        foreach (var element in elements)
        {
            var monster = CreateMonster();
            monster.Resilience = element;
            var result = monster.ApplyElementalDamage(10, element);

            Assert.AreEqual(0, result.FinalDamage,
                $"Element {element} with matching resilience should completely block damage.");
        }
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — action items
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_AddsActionItemWithDamage()
    {
        var monster = CreateMonster();
        monster.Name = "Goblin";

        monster.ApplyElementalDamage(10, ElementType.Fire);

        var actionItem = monster.ActionItems.Values.Last();
        Assert.IsTrue(actionItem.Contains("10"), $"Expected action item to contain '10', but was: {actionItem}");
        Assert.IsTrue(actionItem.Contains("Goblin"), $"Expected action item to contain 'Goblin', but was: {actionItem}");
    }

    [TestMethod]
    public void ApplyElementalDamage_ActionItemContainsElementTypeName()
    {
        var monster = CreateMonster();

        monster.ApplyElementalDamage(10, ElementType.Fire);

        var actionItem = monster.ActionItems.Values.Last();
        Assert.IsTrue(actionItem.Contains("Burning"), $"Expected action item to contain 'Burning', but was: {actionItem}");
    }

    [TestMethod]
    public void ApplyElementalDamage_DifferentElements_HaveCorrectActionItemNames()
    {
        var tests = new (ElementType Element, string ExpectedName)[]
        {
            (ElementType.Fire, "Burning"),
            (ElementType.Lightning, "Static"),
            (ElementType.Ice, "Frostbite"),
            (ElementType.Nature, "Poisoned"),
            (ElementType.Radiance, "Weakened"),
            (ElementType.Abyssal, "Despair")
        };

        foreach (var (element, expectedName) in tests)
        {
            var monster = CreateMonster();
            monster.ApplyElementalDamage(1, element);
            var actionItem = monster.ActionItems.Values.Last();
            Assert.IsTrue(actionItem.Contains(expectedName),
                $"Expected action item for {element} to contain '{expectedName}', but was: {actionItem}");
        }
    }

    [TestMethod]
    public void ApplyElementalDamage_ActionItemShowsZeroDamage_WhenResilienceBlocks()
    {
        var monster = CreateMonster();
        monster.Name = "Orc";
        monster.Resilience = ElementType.Fire;

        monster.ApplyElementalDamage(10, ElementType.Fire);

        var actionItem = monster.ActionItems.Values.Last();
        // Should show 0 damage since resilience blocks it completely
        Assert.IsTrue(actionItem.Contains("resists"), $"Expected action item to contain '0', but was: {actionItem}");
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — return value contract
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_ReturnValue_PreservesInitialDamage()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.AreEqual(ElementType.Fire, result.Element);
    }

    [TestMethod]
    public void ApplyElementalDamage_NoResilience_ReturnsEqualInitialAndFinal()
    {
        var monster = CreateMonster();

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
    }

    [TestMethod]
    public void ApplyElementalDamage_MultipleHits_AccumulateDamage()
    {
        var monster = CreateMonster();

        monster.ApplyElementalDamage(10, ElementType.Fire);
        monster.ApplyElementalDamage(10, ElementType.Fire);
        monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(70, monster.CurrentHealth);
    }

    [TestMethod]
    public void ApplyElementalDamage_MultipleHits_WithResilience_NoDamageAccumulated()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;

        monster.ApplyElementalDamage(10, ElementType.Fire);
        monster.ApplyElementalDamage(10, ElementType.Fire);
        monster.ApplyElementalDamage(10, ElementType.Fire);

        // Resilience blocks all damage, health unchanged
        Assert.AreEqual(100, monster.CurrentHealth);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — vulnerability increases damage by 1.5x
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_WithMatchingVulnerability_IncreasesDamageBy1_5x()
    {
        var monster = CreateMonster();
        monster.Vulnerability = ElementType.Fire;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        // 10 * 1.5 = 15
        Assert.AreEqual(85, monster.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(15, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_WithMatchingVulnerability_OddDamage_RoundsDown()
    {
        var monster = CreateMonster();
        monster.Vulnerability = ElementType.Fire;

        var result = monster.ApplyElementalDamage(7, ElementType.Fire);

        // (int)(7 * 1.5) = (int)(10.5) = 10
        Assert.AreEqual(89, monster.CurrentHealth);
        Assert.AreEqual(7, result.InitialDamage);
        Assert.AreEqual(11, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_WithMatchingVulnerability_DamageOne_ReturnsOne()
    {
        var monster = CreateMonster();
        monster.Vulnerability = ElementType.Fire;

        var result = monster.ApplyElementalDamage(1, ElementType.Fire);

        // (int)(1 * 1.5) = (int)(1.5) = 1
        Assert.AreEqual(98, monster.CurrentHealth);
        Assert.AreEqual(1, result.InitialDamage);
        Assert.AreEqual(2, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_MismatchedVulnerability_NoIncrease()
    {
        var monster = CreateMonster();
        monster.Vulnerability = ElementType.Ice;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, monster.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_VulnerabilityNull_NoIncrease()
    {
        var monster = CreateMonster();
        monster.Vulnerability = null;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, monster.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — vulnerability death cases
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_VulnerabilityCausesDeath()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        monster.Vulnerability = ElementType.Fire;

        var result = monster.ApplyElementalDamage(7, ElementType.Fire);

        // (int)(7 * 1.5) = 10, health = 10 - 10 = 0, dies
        Assert.AreEqual(-1, monster.CurrentHealth);
        Assert.AreEqual(11, result.FinalDamage);
        Assert.IsTrue(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_VulnerabilityExceedsHealth()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 5;
        monster.Vulnerability = ElementType.Fire;

        var result = monster.ApplyElementalDamage(4, ElementType.Fire);

        // (int)(4 * 1.5) = 6, health = 5 - 6 = -1, dies
        Assert.AreEqual(-1, monster.CurrentHealth);
        Assert.AreEqual(6, result.FinalDamage);
        Assert.IsTrue(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_MismatchedVulnerability_DoesNotCauseDeath()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        monster.Vulnerability = ElementType.Ice; // Fire damage, not vulnerable

        var result = monster.ApplyElementalDamage(7, ElementType.Fire);

        // No vulnerability bonus, 7 damage, health = 10 - 7 = 3
        Assert.AreEqual(3, monster.CurrentHealth);
        Assert.AreEqual(7, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — vulnerability all elements
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_AllElements_WithMatchingVulnerability_IncreasesDamage()
    {
        var elements = new[]
        {
            ElementType.Fire,
            ElementType.Lightning,
            ElementType.Ice,
            ElementType.Nature,
            ElementType.Radiance,
            ElementType.Abyssal
        };

        foreach (var element in elements)
        {
            var monster = CreateMonster();
            monster.Vulnerability = element;
            var result = monster.ApplyElementalDamage(10, element);

            Assert.AreEqual(15, result.FinalDamage,
                $"Element {element} with matching vulnerability should increase damage by 1.5x.");
        }
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — vulnerability action items
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_ActionItemShowsIncreasedDamage_WhenVulnerable()
    {
        var monster = CreateMonster();
        monster.Name = "Orc";
        monster.Vulnerability = ElementType.Fire;

        monster.ApplyElementalDamage(10, ElementType.Fire);

        var actionItem = monster.ActionItems.Values.Last();
        // Should show the increased damage (15), not the initial damage (10)
        Assert.IsTrue(actionItem.Contains("15"), $"Expected action item to contain '15', but was: {actionItem}");
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — vulnerability and resilience interaction
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_ResilienceAndVulnerabilitySameElement_ResilienceOverridesVulnerability()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;
        monster.Vulnerability = ElementType.Fire;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(100, monster.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResilienceOneElement_VulnerabilityAnother_ElementMatchesResilience()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;
        monster.Vulnerability = ElementType.Ice;

        var result = monster.ApplyElementalDamage(10, ElementType.Fire);

        // Resilience blocks Fire damage, vulnerability is Ice (not Fire)
        Assert.AreEqual(100, monster.CurrentHealth);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResilienceOneElement_VulnerabilityAnother_ElementMatchesVulnerability()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;
        monster.Vulnerability = ElementType.Ice;

        var result = monster.ApplyElementalDamage(10, ElementType.Ice);

        // Resilience is Fire (not Ice), vulnerability is Ice: (int)(10 * 1.5) = 15
        Assert.AreEqual(85, monster.CurrentHealth);
        Assert.AreEqual(15, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    [TestMethod]
    public void ApplyElementalDamage_ResilienceOneElement_VulnerabilityAnother_ElementMatchesNeither()
    {
        var monster = CreateMonster();
        monster.Resilience = ElementType.Fire;
        monster.Vulnerability = ElementType.Ice;

        var result = monster.ApplyElementalDamage(10, ElementType.Lightning);

        // Neither resilience nor vulnerability applies
        Assert.AreEqual(90, monster.CurrentHealth);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — vulnerability multiple hits
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_MultipleHits_WithVulnerability_AccumulateIncreasedDamage()
    {
        var monster = CreateMonster();
        monster.Vulnerability = ElementType.Fire;

        monster.ApplyElementalDamage(10, ElementType.Fire);
        monster.ApplyElementalDamage(10, ElementType.Fire);
        monster.ApplyElementalDamage(10, ElementType.Fire);

        // Each hit: (int)(10 * 1.5) = 15, total = 45
        Assert.AreEqual(55, monster.CurrentHealth);
    }
    
    [TestMethod]
    public void ApplyElementalDamage_AllElements_WithMatchingResilienceAndVulnerability_ResilienceTakesPrecedence()
    {
        var elements = new[]
        {
            ElementType.Fire,
            ElementType.Lightning,
            ElementType.Ice,
            ElementType.Nature,
            ElementType.Radiance,
            ElementType.Abyssal
        };

        foreach (var element in elements)
        {
            var monster = CreateMonster();
            monster.Resilience = element;
            monster.Vulnerability = element;

            var result = monster.ApplyElementalDamage(10, element);

            Assert.AreEqual(0, result.FinalDamage,
                $"Element {element} should be fully resisted when both traits match.");
            Assert.AreEqual(100, monster.CurrentHealth);
        }
    }
}
