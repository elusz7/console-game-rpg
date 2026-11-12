using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class MonsterHealTests
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

    private static ActiveEffect CreateFrozenEffect(int power)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Frozen),
            power: power,
            duration: 3);
    }

    [TestMethod]
    public void Heal_Success()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = monster.MaxHealth - 30;

        monster.Heal(20);

        Assert.AreEqual(monster.MaxHealth - 10, monster.CurrentHealth);
    }

    [TestMethod]
    public void Heal_HitsMaximum()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = monster.MaxHealth - 10;

        monster.Heal(20);

        Assert.AreEqual(monster.MaxHealth, monster.CurrentHealth);
    }

    [TestMethod]
    public void Heal_FrozenReducesHealingReceived()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 100;

        monster.ActiveEffectManager.Register(
            CreateFrozenEffect(power: 10));

        monster.Heal(30);

        Assert.AreEqual(120, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("heals for 20")));
    }

    [TestMethod]
    public void Heal_FrozenCanReduceHealingToZero()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 100;

        monster.ActiveEffectManager.Register(
            CreateFrozenEffect(power: 30));

        monster.Heal(20);

        Assert.AreEqual(100, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("heals for 0")));
    }

    [TestMethod]
    public void Heal_FrozenReductionAppliedBeforeMaxHealthCap()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 190;

        monster.ActiveEffectManager.Register(
            CreateFrozenEffect(power: 10));

        monster.Heal(30);

        Assert.AreEqual(200, monster.CurrentHealth);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("heals for 10")));
    }

    [TestMethod]
    public void Heal_RemovingFrozenRestoresNormalHealing()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 100;

        var frozen = CreateFrozenEffect(power: 10);
        monster.ActiveEffectManager.Register(frozen);
        monster.ActiveEffectManager.RemoveById(frozen.EffectId);

        monster.Heal(30);

        Assert.AreEqual(130, monster.CurrentHealth);
    }

    [TestMethod]
    public void Heal_NegativeAmountThrows()
    {
        var monster = CreateMonster();

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => monster.Heal(-1));
    }
}
