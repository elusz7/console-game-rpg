using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class BossMonsterApplyElementalDamageTests
{
    private static BossMonster CreateBoss()
    {
        return new BossMonster(new MonsterSkillSelector())
        {
            Name = "Boss",
            Level = 1,
            AttackPower = 100,
            DefensePower = 20,
            Resistance = 10,
            AggressionLevel = 5,
            MaxHealth = 300,
            CurrentHealth = 300,
            DodgeChance = 0,
            DamageType = DamageType.Martial,
            Skills = []
        };
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — non-lethal damage delegates to base (no phase change)
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_NonLethal_NoPhaseChange()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 100;

        var result = boss.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(90, boss.CurrentHealth);
        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(10, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
        Assert.IsFalse(boss.ActionItems.Values.Any(a => a.Contains("entered phase")));
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — lethal damage triggers phase change (phase 1 → 2)
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_LethalDamage_TransitionsToPhaseTwo()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 10;
        var originalMaxHealth = boss.MaxHealth;

        var result = boss.ApplyElementalDamage(50, ElementType.Fire);

        // Phase change: health restored to 75% of original max
        Assert.AreEqual((int)(originalMaxHealth * 0.75), boss.MaxHealth);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsFalse(result.TargetDied);
        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 2")));
    }

    [TestMethod]
    public void ApplyElementalDamage_LethalDamage_PhaseTwoHealthRestored()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;

        boss.ApplyElementalDamage(100, ElementType.Fire);

        // Boss should be alive with full phase 2 health
        Assert.IsTrue(boss.CurrentHealth > 0);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
    }

    [TestMethod]
    public void ApplyElementalDamage_LethalDamage_AttackPowerScalesDown()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;
        var originalAttack = boss.AttackPower;

        boss.ApplyElementalDamage(100, ElementType.Fire);

        // Phase 2 attack power is 75% of original
        Assert.AreEqual((int)(originalAttack * 0.75), boss.AttackPower);
    }

    [TestMethod]
    public void ApplyElementalDamage_LethalDamage_AggressionIncreases()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;
        boss.AggressionLevel = 0;
        var originalAggression = boss.AggressionLevel;

        boss.ApplyElementalDamage(100, ElementType.Fire);

        Assert.IsTrue(boss.AggressionLevel > originalAggression);
    }

    [TestMethod]
    public void ApplyElementalDamage_LethalDamage_DefenseAndResistanceIncrease()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;
        boss.DamageType = DamageType.Martial;
        var originalDefense = boss.DefensePower;
        var originalResistance = boss.Resistance;

        boss.ApplyElementalDamage(100, ElementType.Fire);

        // Martial: Defense +2, Resistance +1
        Assert.IsTrue(boss.DefensePower > originalDefense);
        Assert.IsTrue(boss.Resistance > originalResistance);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — second lethal triggers phase 2 → 3
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_SecondLethalHit_TransitionsToPhaseThree()
    {
        var boss = CreateBoss();
        var originalMaxHealth = boss.MaxHealth;

        // First lethal hit transitions to phase 2
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Second lethal hit transitions to phase 3
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        Assert.AreEqual((int)(originalMaxHealth * 0.50), boss.MaxHealth);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 3")));
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — third lethal defeats the boss (phase 3 → death)
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_ThirdLethalHit_BossDies()
    {
        var boss = CreateBoss();

        // Enter phase 2
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Enter phase 3
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Final lethal hit — boss is defeated, no exception thrown
        var result = boss.ApplyElementalDamage(100000, ElementType.Fire);

        Assert.IsTrue(result.TargetDied);
        Assert.IsTrue(boss.CurrentHealth <= 0);
    }

    [TestMethod]
    public void ApplyElementalDamage_ThirdLethalHit_BossReachesPhaseThreeBeforeDeath()
    {
        var boss = CreateBoss();

        // Enter phase 2
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Enter phase 3
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Verify the boss was in phase 3 before the final hit
        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 3")));

        // Third lethal hit defeats the boss
        var result = boss.ApplyElementalDamage(100000, ElementType.Fire);

        Assert.IsTrue(result.TargetDied);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — resilience prevents phase change
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_ResilienceBlocksDamage_NoPhaseChange()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;
        boss.Resilience = ElementType.Fire;

        var result = boss.ApplyElementalDamage(100, ElementType.Fire);

        // Resilience blocks all damage, health unchanged, no phase change
        Assert.AreEqual(1, boss.CurrentHealth);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
        Assert.IsFalse(boss.ActionItems.Values.Any(a => a.Contains("entered phase")));
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — vulnerability increases damage, still triggers phase
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_VulnerabilityIncreasesDamage_TriggersPhaseChange()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 10;
        boss.Vulnerability = ElementType.Fire;
        var originalMaxHealth = boss.MaxHealth;

        // 10 damage * 1.5 = 15, health goes to -5, triggers phase change
        var result = boss.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual((int)(originalMaxHealth * 0.75), boss.MaxHealth);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsFalse(result.TargetDied);
        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 2")));
    }

    [TestMethod]
    public void ApplyElementalDamage_Vulnerability_PreservesIncreasedDamageInResult()
    {
        var boss = CreateBoss();
        boss.Vulnerability = ElementType.Fire;

        var result = boss.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(10, result.InitialDamage);
        Assert.AreEqual(15, result.FinalDamage);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — multiple non-lethal hits do not trigger phase
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_MultipleNonLethalHits_NoPhaseChange()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 100;

        boss.ApplyElementalDamage(10, ElementType.Fire);
        boss.ApplyElementalDamage(10, ElementType.Fire);
        boss.ApplyElementalDamage(10, ElementType.Fire);

        Assert.AreEqual(70, boss.CurrentHealth);
        Assert.IsFalse(boss.ActionItems.Values.Any(a => a.Contains("entered phase")));
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — action items on phase change
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_PhaseChange_AddsPhaseActionItem()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;

        boss.ApplyElementalDamage(100, ElementType.Fire);

        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("Boss [Boss] has entered phase 2")));
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — result targetDied is false after phase change
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_LethalDamage_ResultTargetDiedIsFalse_AfterPhaseChange()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;

        var result = boss.ApplyElementalDamage(1000, ElementType.Fire);

        // Even though base result had targetDied=true, phase change makes it false
        Assert.IsFalse(result.TargetDied);
        Assert.IsTrue(boss.CurrentHealth > 0);
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — element preserved in result
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_ElementPreservedInResult()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;

        var result = boss.ApplyElementalDamage(100, ElementType.Ice);

        Assert.AreEqual(ElementType.Ice, result.Element);
    }

    [TestMethod]
    public void ApplyElementalDamage_AllElements_TriggerPhaseChange()
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
            var boss = CreateBoss();
            boss.CurrentHealth = 1;

            var result = boss.ApplyElementalDamage(100, element);

            Assert.IsFalse(result.TargetDied, $"Element {element} should trigger phase change");
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 2")),
                $"Element {element} should trigger phase change");
        }
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — phase 2 lethal damage triggers phase 3
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_PhaseTwoLethalDamage_TransitionsToPhaseThree()
    {
        var boss = CreateBoss();
        var originalMaxHealth = boss.MaxHealth;

        // Enter phase 2
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Now in phase 2, apply lethal damage again
        var result = boss.ApplyElementalDamage(100000, ElementType.Fire);

        Assert.AreEqual((int)(originalMaxHealth * 0.50), boss.MaxHealth);
        Assert.IsFalse(result.TargetDied);
        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 3")));
    }

    // ---------------------------------------------------------------------------
    // ApplyElementalDamage — phase 3 scaling verification
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ApplyElementalDamage_PhaseThree_AttackPowerAt50Percent()
    {
        var boss = CreateBoss();
        var originalAttack = boss.AttackPower;

        // Enter phase 2
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Enter phase 3
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Phase 3 attack power is 50% of original
        Assert.AreEqual((int)(originalAttack * 0.50), boss.AttackPower);
    }

    [TestMethod]
    public void ApplyElementalDamage_PhaseThree_MaxHealthAt50Percent()
    {
        var boss = CreateBoss();
        var originalMaxHealth = boss.MaxHealth;

        // Enter phase 2
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        // Enter phase 3
        boss.ApplyElementalDamage(100000, ElementType.Fire);

        Assert.AreEqual((int)(originalMaxHealth * 0.50), boss.MaxHealth);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
    }
    
    [TestMethod]
    public void ApplyElementalDamage_ResilienceAndVulnerabilitySameElement_ResiliencePreventsPhaseChange()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;
        boss.Resilience = ElementType.Fire;
        boss.Vulnerability = ElementType.Fire;

        var result = boss.ApplyElementalDamage(100, ElementType.Fire);

        Assert.AreEqual(1, boss.CurrentHealth);
        Assert.AreEqual(0, result.FinalDamage);
        Assert.IsFalse(result.TargetDied);
        Assert.IsFalse(boss.ActionItems.Values.Any(a => a.Contains("entered phase")));
    }
}
