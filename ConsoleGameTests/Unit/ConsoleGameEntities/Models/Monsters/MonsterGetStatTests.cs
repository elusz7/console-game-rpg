using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class MonsterGetStatTests
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
    
    private static ActiveEffect CreateStatusEffect(
        ElementalStatusEffectType statusType,
        int power,
        int duration = 3)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(statusType),
            power,
            duration);
    }

    [TestMethod]
    public void GetStat_ReturnsCorrectStat()
    {
        var monster = CreateMonster();
        monster.AttackPower = 10;

        int attack = monster.GetStat(StatType.Attack);

        Assert.AreEqual(10, attack);
    }

    [TestMethod]
    public void GetStat_InvalidStat_Throws()
    {
        var monster = CreateMonster();

        Assert.ThrowsException<StatTypeException>(() => monster.GetStat((StatType)999));
    }
    
    [TestMethod]
    public void GetStat_ReturnsBaseDefense()
    {
        var monster = CreateMonster();

        Assert.AreEqual(50, monster.GetStat(StatType.Defense));
    }

    [TestMethod]
    public void GetStat_ReturnsBaseResistance()
    {
        var monster = CreateMonster();

        Assert.AreEqual(30, monster.GetStat(StatType.Resistance));
    }

    [TestMethod]
    public void GetStat_ReturnsBaseSpeed()
    {
        var monster = CreateMonster();

        Assert.AreEqual(10, monster.GetStat(StatType.Speed));
    }

    [TestMethod]
    public void GetStat_ReturnsCurrentHealth()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 75;

        Assert.AreEqual(75, monster.GetStat(StatType.Health));
    }
    
    [TestMethod]
    public void GetStat_Charred_ReducesDefense()
    {
        var monster = CreateMonster();

        var effect = CreateStatusEffect(ElementalStatusEffectType.Charred, 15);
        monster.ActiveEffectManager.Register(effect);

        Assert.AreEqual(35, monster.GetStat(StatType.Defense));
    }

    [TestMethod]
    public void GetStat_Shocked_ReducesResistance()
    {
        var monster = CreateMonster();

        var effect = CreateStatusEffect(ElementalStatusEffectType.Shocked, 10);
        monster.ActiveEffectManager.Register(effect);

        Assert.AreEqual(20, monster.GetStat(StatType.Resistance));
    }

    [TestMethod]
    public void GetStat_Snared_ReducesSpeed()
    {
        var monster = CreateMonster();

        var effect = CreateStatusEffect(ElementalStatusEffectType.Snared, 4);
        monster.ActiveEffectManager.Register(effect);

        Assert.AreEqual(6, monster.GetStat(StatType.Speed));
    }
    
    [TestMethod]
    public void GetStat_Charred_CannotReduceDefenseBelowZero()
    {
        var monster = CreateMonster();

        monster.ActiveEffectManager.Register(
            CreateStatusEffect(ElementalStatusEffectType.Charred, 100));

        Assert.AreEqual(0, monster.GetStat(StatType.Defense));
    }

    [TestMethod]
    public void GetStat_Shocked_CannotReduceResistanceBelowZero()
    {
        var monster = CreateMonster();

        monster.ActiveEffectManager.Register(
            CreateStatusEffect(ElementalStatusEffectType.Shocked, 100));

        Assert.AreEqual(0, monster.GetStat(StatType.Resistance));
    }

    [TestMethod]
    public void GetStat_Snared_CannotReduceSpeedBelowZero()
    {
        var monster = CreateMonster();

        monster.ActiveEffectManager.Register(
            CreateStatusEffect(ElementalStatusEffectType.Snared, 100));

        Assert.AreEqual(0, monster.GetStat(StatType.Speed));
    }
    
    [TestMethod]
    public void GetStat_AfterRemovingEffect_RestoresBaseStat()
    {
        var monster = CreateMonster();

        var effect = CreateStatusEffect(ElementalStatusEffectType.Charred, 15);
        monster.ActiveEffectManager.Register(effect);

        Assert.AreEqual(35, monster.GetStat(StatType.Defense));

        monster.ActiveEffectManager.RemoveById(effect.EffectId);

        Assert.AreEqual(50, monster.GetStat(StatType.Defense));
    }

    [TestMethod]
    public void GetStat_AfterStrongerEffectRefresh_UsesUpdatedPower()
    {
        var monster = CreateMonster();

        var original = CreateStatusEffect(ElementalStatusEffectType.Charred, 10);
        monster.ActiveEffectManager.Register(original);

        Assert.AreEqual(40, monster.GetStat(StatType.Defense));

        var stronger = CreateStatusEffect(ElementalStatusEffectType.Charred, 20);
        monster.ActiveEffectManager.Register(stronger);

        Assert.AreEqual(30, monster.GetStat(StatType.Defense));
    }
    
    [TestMethod]
    public void GetStat_Martial_UsesAttackModifier()
    {
        var monster = CreateMonster();
        monster.DamageType = DamageType.Martial;

        monster.ModifyStat(StatType.Attack, 10);

        Assert.AreEqual(110, monster.GetStat(StatType.Attack));
    }

    [TestMethod]
    public void GetStat_Magical_UsesMagicModifier()
    {
        var monster = CreateMonster();
        monster.DamageType = DamageType.Magical;

        monster.ModifyStat(StatType.Magic, 10);

        Assert.AreEqual(110, monster.GetStat(StatType.Magic));
    }

    [TestMethod]
    public void GetStat_Hybrid_AveragesAttackAndMagicModifiers()
    {
        var monster = CreateMonster();
        monster.DamageType = DamageType.Hybrid;

        monster.ModifyStat(StatType.Attack, 10);
        monster.ModifyStat(StatType.Magic, 20);

        Assert.AreEqual(115, monster.GetStat(StatType.Attack));
    }
}
