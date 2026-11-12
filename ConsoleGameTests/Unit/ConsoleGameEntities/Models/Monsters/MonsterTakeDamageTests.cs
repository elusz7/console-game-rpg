using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class MonsterTakeDamageTests
{
    private static Monster CreateMonster()
    {
        return new Monster
        {
            Level = 1,
            AttackPower = 100,
            DefensePower = 50,
            Resistance = 30,
            AggressionLevel = 10,
            MaxHealth = 200,
            DodgeChance = 0
        };
    }
    
    private static ActiveEffect CreateBlindedEffect(int power)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Blinded),
            power: power,
            duration: 3);
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_ReducesHealth()
    {
        var monster = CreateMonster();
        monster.DefensePower = 0;
        monster.CurrentHealth = monster.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(monster.MaxHealth - 10, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 10 damage")));
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_DefenseReducesDamage()
    {
        var monster = CreateMonster();
        monster.DefensePower = 5;
        monster.CurrentHealth = monster.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(monster.MaxHealth - 5, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 5 damage")));
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_DefenseGreaterThanDamage()
    {
        var monster = CreateMonster();
        monster.DefensePower = 12;
        monster.CurrentHealth = monster.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(monster.MaxHealth - 1, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 1 damage")));
    }

    [TestMethod]
    public void TakeDamage_MagicalDamage_ReducesHealth()
    {
        var monster = CreateMonster();
        monster.Resistance = 0;
        monster.CurrentHealth = monster.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Magical, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(monster.MaxHealth - 10, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 10 damage")));
    }

    [TestMethod]
    public void TakeDamage_MagicalDamage_ResistanceReducesDamage()
    {
        var monster = CreateMonster();
        monster.Resistance = 5;
        monster.CurrentHealth = monster.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Magical, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(monster.MaxHealth - 5, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 5 damage")));
    }

    [TestMethod]
    public void TakeDamage_MagicalDamage_ResistanceGreaterThanDamage()
    {
        var monster = CreateMonster();
        monster.Resistance = 12;
        monster.CurrentHealth = monster.MaxHealth;

        var attack = new AttackDamage(10, DamageType.Magical, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(monster.MaxHealth - 1, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 1 damage")));
    }

    [TestMethod]
    public void TakeDamage_MonsterDies()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 1;
        
        var attack = new AttackDamage(1000, DamageType.Magical, 0, null);

        Assert.ThrowsException<MonsterDeathException>(() => monster.TakeDamage(attack));
    }
    
    [TestMethod]
    public void TakeDamage_Blinded_StandardDamageOnly_AppliesEntireBonusToStandard()
    {
        var monster = CreateMonster();
        monster.DefensePower = 0;
        monster.CurrentHealth = monster.MaxHealth;
        monster.ActiveEffectManager.Register(CreateBlindedEffect(5));

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(185, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 15 damage")));
    }
    
    [TestMethod]
    public void TakeDamage_Blinded_ElementalDamageOnly_AppliesEntireBonusToElemental()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = monster.MaxHealth;
        monster.ActiveEffectManager.Register(CreateBlindedEffect(5));

        var attack = new AttackDamage(0, DamageType.Magical, 8, ElementType.Fire);
        monster.TakeDamage(attack);

        // Fire has no matching vulnerability or resilience by default.
        Assert.AreEqual(187, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("takes 13 Fire damage")));
    }
    
    [TestMethod]
    public void TakeDamage_Blinded_CombinedDamage_EvenBonus_SplitsEvenly()
    {
        var monster = CreateMonster();
        monster.DefensePower = 0;
        monster.CurrentHealth = monster.MaxHealth;
        monster.ActiveEffectManager.Register(CreateBlindedEffect(6));

        var attack = new AttackDamage(10, DamageType.Martial, 8, ElementType.Fire);
        monster.TakeDamage(attack);

        // Standard: 10 + 3 = 13.
        // Elemental: 8 + 3 = 11.
        Assert.AreEqual(176, monster.CurrentHealth);
    }
    
    [TestMethod]
    public void TakeDamage_Blinded_CombinedDamage_OddBonus_GivesExtraPointToStandard()
    {
        var monster = CreateMonster();
        monster.DefensePower = 0;
        monster.CurrentHealth = monster.MaxHealth;
        monster.ActiveEffectManager.Register(CreateBlindedEffect(5));

        var attack = new AttackDamage(10, DamageType.Martial, 8, ElementType.Fire);
        monster.TakeDamage(attack);

        // Standard: 10 + 3 = 13.
        // Elemental: 8 + 2 = 10.
        Assert.AreEqual(177, monster.CurrentHealth);
    }
    
    [TestMethod]
    public void TakeDamage_Blinded_StandardDamage_AppliesDefenseAfterBonus()
    {
        var monster = CreateMonster();
        monster.DefensePower = 8;
        monster.CurrentHealth = monster.MaxHealth;
        monster.ActiveEffectManager.Register(CreateBlindedEffect(5));

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        monster.TakeDamage(attack);

        // 10 + 5 - 8 = 7.
        Assert.AreEqual(193, monster.CurrentHealth);
    }
    
    [TestMethod]
    public void TakeDamage_Blinded_ElementalDamage_AppliesResilienceAfterBonus()
    {
        var monster = CreateMonster();
        monster.DefensePower = 0;
        monster.CurrentHealth = monster.MaxHealth;
        monster.Resilience = ElementType.Fire;
        monster.ActiveEffectManager.Register(CreateBlindedEffect(5));

        var attack = new AttackDamage(1, DamageType.Magical, 8, ElementType.Fire);
        monster.TakeDamage(attack);

        // Standard: 1 + 3 = 4, reduced by Resistance 30 to minimum 1.
        // Elemental: 8 + 2 = 10, then Fire resilience halves it to 5.
        // Total: 1 + 5 = 6.
        Assert.AreEqual(194, monster.CurrentHealth);
    }
    
    [TestMethod]
    public void TakeDamage_OtherElementalStatuses_DoNotIncreaseIncomingDamage()
    {
        var monster = CreateMonster();
        monster.DefensePower = 0;
        monster.CurrentHealth = monster.MaxHealth;

        monster.ActiveEffectManager.Register(new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            power: 5,
            duration: 3));

        monster.ActiveEffectManager.Register(new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked),
            power: 5,
            duration: 3));

        monster.ActiveEffectManager.Register(new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Frozen),
            power: 5,
            duration: 3));

        monster.ActiveEffectManager.Register(new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Snared),
            power: 5,
            duration: 3));

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(190, monster.CurrentHealth);
    }
    [TestMethod]
    public void TakeDamage_Blinded_RemovedBeforeAttack_AppliesNoBonus()
    {
        var monster = CreateMonster();
        monster.DefensePower = 0;
        monster.CurrentHealth = monster.MaxHealth;

        var blindedEffect = CreateBlindedEffect(5);
        monster.ActiveEffectManager.Register(blindedEffect);
        monster.ActiveEffectManager.RemoveById(blindedEffect.EffectId);

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        monster.TakeDamage(attack);

        Assert.AreEqual(190, monster.CurrentHealth);
    }
}
