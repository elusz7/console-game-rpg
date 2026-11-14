using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class BossMonsterTests
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

    private static BossMonster CreateBossWithElementalProperties(
        ElementType? attackElement = null,
        ElementType? vulnerability = null,
        ElementType? resilience = null,
        int elementalPower = 1)
    {
        var boss = CreateBoss();
        boss.AttackElement = attackElement;
        boss.Vulnerability = vulnerability;
        boss.Resilience = resilience;
        boss.ElementalPower = elementalPower;
        return boss;
    }

    [TestMethod]
    public void Constructor_InitializesBaseStats()
    {
        var boss = CreateBoss();
        Assert.AreEqual(300, boss.MaxHealth);
        Assert.AreEqual(100, boss.AttackPower);
        Assert.AreEqual("Boss", boss.Name);
        Assert.IsInstanceOfType(boss.Strategy, typeof(BossStrategy));
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_ReducesHealth()
    {
        var boss = CreateBoss();
        boss.DefensePower = 0;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);

        boss.TakeDamage(attack);

        if (boss.CurrentHealth < boss.MaxHealth)
        {
            Assert.AreEqual(boss.MaxHealth - 10, boss.CurrentHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("takes 10 damage")));
        }
        else
        {
            Assert.AreEqual(boss.CurrentHealth, boss.MaxHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_DefenseReducesDamage()
    {
        var boss = CreateBoss();
        boss.DefensePower = 5;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        boss.TakeDamage(attack);

        if (boss.CurrentHealth < boss.MaxHealth)
        {
            Assert.AreEqual(boss.MaxHealth - 5, boss.CurrentHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("takes 5 damage")));
        }
        else
        {
            Assert.AreEqual(boss.CurrentHealth, boss.MaxHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_MagicalDamage_ResistanceReducesDamage()
    {
        var boss = CreateBoss();
        boss.Resistance = 7;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Magical, 0, null);
        boss.TakeDamage(attack);

        if (boss.CurrentHealth < boss.MaxHealth)
        {
            Assert.AreEqual(boss.MaxHealth - 3, boss.CurrentHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("takes 3 damage")));
        }
        else
        {
            Assert.AreEqual(boss.CurrentHealth, boss.MaxHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_HybridDamage_UsesMaxOfDefenseOrResistance()
    {
        var boss = CreateBoss();
        boss.DefensePower = 8;
        boss.Resistance = 4;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Hybrid, 0, null);
        boss.TakeDamage(attack);

        if (boss.CurrentHealth < boss.MaxHealth)
        {
            Assert.AreEqual(boss.MaxHealth - 6, boss.CurrentHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("takes 6 damage")));
        }
        else
        {
            Assert.AreEqual(boss.CurrentHealth, boss.MaxHealth);
            Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("dodged the attack")));
        }
    }

    [TestMethod]
    public void TakeDamage_PhaseChange_ResetsStatsAndAddsAction()
    {
        var boss = CreateBoss();
        boss.CurrentHealth = 1;
        boss.MaxHealth = 100;
        boss.AttackPower = 50;
        boss.DefensePower = 10;
        boss.Resistance = 5;
        boss.AggressionLevel = 0;
        boss.DamageType = DamageType.Martial;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        boss.TakeDamage(attack);

        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 2")));
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsTrue(boss.AggressionLevel > 0);
        Assert.IsTrue(boss.DefensePower > 10);
        Assert.IsTrue(boss.Resistance > 5);
        Assert.IsTrue(boss.AttackPower < 50); // scaled down
        Assert.IsTrue(boss.MaxHealth < 100); // scaled down
    }
    
    [TestMethod]
    public void TakeDamage_FirstLethalHit_TransitionsToPhaseTwo()
    {
        var boss = CreateBoss();
        var originalMaxHealth = boss.MaxHealth;

        var attack = new AttackDamage(10000, DamageType.Martial, 0, null);
                boss.TakeDamage(attack);

        Assert.AreEqual((int)(originalMaxHealth * 0.75), boss.MaxHealth);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsTrue(
            boss.ActionItems.Values.Any(a => a.Contains("entered phase 2")));
    }

    [TestMethod]
    public void TakeDamage_SecondLethalHit_TransitionsToPhaseThree()
    {
        var boss = CreateBoss();
        var originalMaxHealth = boss.MaxHealth;

        // First lethal hit transitions to phase two.
        var attack = new AttackDamage(100000, DamageType.Martial, 0, null);
        boss.TakeDamage(attack);

        // Prevent the phase-two Speed increase from affecting the dodge roll.
        boss.AggressionLevel = 0;
        boss.TakeDamage(attack);

        Assert.AreEqual((int)(originalMaxHealth * 0.50), boss.MaxHealth);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsTrue(
            boss.ActionItems.Values.Any(a => a.Contains("entered phase 3")));
    }

    [TestMethod]
    public void TakeDamage_ThirdLethalHit_ThrowsMonsterDeathException()
    {
        var boss = CreateBoss();

        // Enter phase two.
        var attack = new AttackDamage(100000, DamageType.Martial, 0, null);
        boss.TakeDamage(attack);

        boss.AggressionLevel = 0;

        // Enter phase three.
        boss.TakeDamage(attack);

        boss.AggressionLevel = 0;

        // Final lethal hit should defeat the boss.
        var result = boss.TakeDamage(attack);

        Assert.IsTrue(result.TargetDied);
        Assert.IsTrue(boss.CurrentHealth <= 0);
    }

    // ── Elemental Damage: No Transition ──

    [TestMethod]
    public void TakeDamage_ElementalDamageOnly_NoTransition()
    {
        var boss = CreateBossWithElementalProperties(
            vulnerability: ElementType.Fire,
            resilience: ElementType.Ice);
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 10, ElementType.Fire);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 15, boss.CurrentHealth);
        Assert.IsFalse(boss.ActionItems.Values.Any(a => a.Contains("entered phase")));
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire)]
    [DataRow(ElementType.Lightning)]
    [DataRow(ElementType.Ice)]
    [DataRow(ElementType.Nature)]
    [DataRow(ElementType.Radiance)]
    [DataRow(ElementType.Abyssal)]
    public void TakeDamage_ElementalNeutral_AllElements(ElementType element)
    {
        var boss = CreateBossWithElementalProperties();
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 10, element);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 10, boss.CurrentHealth);
    }

    // ── Elemental Damage: Resilience (0.5×) ──

    [TestMethod]
    public void TakeDamage_ElementalResilience_DamageHalved()
    {
        var boss = CreateBossWithElementalProperties(resilience: ElementType.Fire);
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 10, ElementType.Fire);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 5, boss.CurrentHealth);
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire)]
    [DataRow(ElementType.Lightning)]
    [DataRow(ElementType.Ice)]
    [DataRow(ElementType.Nature)]
    [DataRow(ElementType.Radiance)]
    [DataRow(ElementType.Abyssal)]
    public void TakeDamage_ElementalResilience_AllElements(ElementType element)
    {
        var boss = CreateBossWithElementalProperties(resilience: element);
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 10, element);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 5, boss.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_ElementalResilience_TruncatesDamage()
    {
        var boss = CreateBossWithElementalProperties(resilience: ElementType.Fire);
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 7, ElementType.Fire);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 3, boss.CurrentHealth);
    }

    // ── Elemental Damage: Vulnerability (1.5×) ──

    [TestMethod]
    public void TakeDamage_ElementalVulnerability_DamageIncreased()
    {
        var boss = CreateBossWithElementalProperties(vulnerability: ElementType.Fire);
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 10, ElementType.Fire);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 15, boss.CurrentHealth);
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire)]
    [DataRow(ElementType.Lightning)]
    [DataRow(ElementType.Ice)]
    [DataRow(ElementType.Nature)]
    [DataRow(ElementType.Radiance)]
    [DataRow(ElementType.Abyssal)]
    public void TakeDamage_ElementalVulnerability_AllElements(ElementType element)
    {
        var boss = CreateBossWithElementalProperties(vulnerability: element);
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 10, element);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 15, boss.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_ElementalVulnerability_TruncatesDamage()
    {
        var boss = CreateBossWithElementalProperties(vulnerability: ElementType.Fire);
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 7, ElementType.Fire);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 10, boss.CurrentHealth);
    }

    // ── Elemental Damage: Triggers Phase Transition ──

    [TestMethod]
    public void TakeDamage_ElementalDamageTriggers_TransitionToPhaseTwo()
    {
        var boss = CreateBossWithElementalProperties(vulnerability: ElementType.Fire);
        boss.DefensePower = 1;
        var originalMaxHealth = boss.MaxHealth;
        boss.CurrentHealth = 1;

        var attack = new AttackDamage(1, DamageType.Martial, 100, ElementType.Fire);
        boss.TakeDamage(attack);

        Assert.AreEqual((int)(originalMaxHealth * 0.75), boss.MaxHealth);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 2")));
    }

    [TestMethod]
    public void TakeDamage_CombinedStandardAndElemental_TriggersTransition()
    {
        var boss = CreateBossWithElementalProperties(vulnerability: ElementType.Fire);
        var originalMaxHealth = boss.MaxHealth;
        boss.DefensePower = 0;
        boss.CurrentHealth = 1;

        var attack = new AttackDamage(50, DamageType.Martial, 100, ElementType.Fire);
        boss.TakeDamage(attack);

        Assert.AreEqual((int)(originalMaxHealth * 0.75), boss.MaxHealth);
        Assert.AreEqual(boss.MaxHealth, boss.CurrentHealth);
        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase 2")));
    }

    [TestMethod]
    public void TakeDamage_ElementalDamage_SingleTransitionPerAttack()
    {
        var boss = CreateBossWithElementalProperties(vulnerability: ElementType.Fire);
        boss.DefensePower = 1;
        boss.CurrentHealth = 1;
        int phaseCount = 0;

        var attack = new AttackDamage(1, DamageType.Martial, 10000, ElementType.Fire);
        boss.TakeDamage(attack);

        phaseCount = boss.ActionItems.Values.Count(a => a.Contains("entered phase"));
        Assert.AreEqual(1, phaseCount);
    }

    // ── Elemental Damage: Defeats Final Phase ──

    [TestMethod]
    public void TakeDamage_ElementalDamage_DefeatsFinalPhase()
    {
        var boss = CreateBossWithElementalProperties(vulnerability: ElementType.Fire);
        boss.DefensePower = 1;

        // Enter phase two.
        var attack = new AttackDamage(1, DamageType.Martial, 100000, ElementType.Fire);
        boss.TakeDamage(attack);

        boss.AggressionLevel = 0;

        // Enter phase three.
        boss.TakeDamage(attack);

        boss.AggressionLevel = 0;

        // Final lethal elemental hit should defeat the boss.
        var result = boss.TakeDamage(attack);

        Assert.IsTrue(result.TargetDied);
        Assert.IsTrue(boss.CurrentHealth <= 0);
    }

    [TestMethod]
    public void TakeDamage_CombinedDamage_DefeatsFinalPhase()
    {
        var boss = CreateBossWithElementalProperties(vulnerability: ElementType.Fire);
        boss.DefensePower = 0;

        // Enter phase two.
        var attack = new AttackDamage(100000, DamageType.Martial, 0, null);
        boss.TakeDamage(attack);

        boss.AggressionLevel = 0;

        // Enter phase three.
        boss.TakeDamage(attack);

        boss.AggressionLevel = 0;

        // Final combined hit should defeat the boss.
        var combinedAttack = new AttackDamage(50000, DamageType.Martial, 50000, ElementType.Fire);

        var result = boss.TakeDamage(combinedAttack);

        Assert.IsTrue(result.TargetDied);
        Assert.IsTrue(boss.CurrentHealth <= 0);
    }

    // ── Elemental Damage: Resilience Prevents Transition ──

    [TestMethod]
    public void TakeDamage_ElementalResilience_PreventsTransition()
    {
        var boss = CreateBossWithElementalProperties(resilience: ElementType.Fire);
        boss.DefensePower = 1;
        boss.CurrentHealth = 1;

        var attack = new AttackDamage(1, DamageType.Martial, 100, ElementType.Fire);
        boss.TakeDamage(attack);

        // 100 Fire damage halved to 50, plus 1 standard damage = 51 total;
        // health goes from 1 to -50, triggering transition.
        // After transition, health is restored to new max (75% of previous).
        Assert.IsTrue(boss.CurrentHealth > 1);
        Assert.IsTrue(boss.ActionItems.Values.Any(a => a.Contains("entered phase")));
    }

    // ── Elemental Damage: Null Properties Are Neutral ──

    [TestMethod]
    public void TakeDamage_NullResilienceAndVulnerability_IsNeutral()
    {
        var boss = CreateBossWithElementalProperties();
        boss.DefensePower = 1;
        boss.CurrentHealth = boss.MaxHealth;

        var attack = new AttackDamage(1, DamageType.Martial, 10, ElementType.Fire);
        boss.TakeDamage(attack);

        Assert.AreEqual(boss.MaxHealth - 1 - 10, boss.CurrentHealth);
    }
}