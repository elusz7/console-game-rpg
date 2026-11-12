using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Entities.PlayerTests;

[TestClass]
public class PlayerHealTests
{
    private static Player CreatePlayer()
    {
        var player = new Player
        {
            CurrentHealth = 100,
            MaxHealth = 100,
            DodgeChance = 0
        };

        var archetype = new Archetype
        {
            Name = "Test",
            ArchetypeType = ArchetypeType.Martial,
            AttackMultiplier = 1.5m,
            MagicMultiplier = 1.2m,
            DefenseMultiplier = 1.0m,
            ResistanceMultiplier = 0.5m,
            SpeedMultiplier = 2.0m,
            ResourceMultiplier = 3.0m,
            ResourceName = "Energy",
            RecoveryRate = 2,
            RecoveryGrowth = 1,
            HealthBase = 10,
            MaxResource = 10,
            CurrentResource = 10
        };

        var inventory = new Inventory
        {
            Id = 1,
            Gold = 100,
            Capacity = 36.2M,
            Items = []
        };

        player.Archetype = archetype;
        player.Inventory = inventory;
        inventory.Player = player;

        return player;
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
    public void Heal_HealthRestored()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 50;

        player.Heal(30);

        Assert.AreEqual(80, player.CurrentHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("healed for 30")));
    }

    [TestMethod]
    public void Heal_HealthDoesNotExceedMax()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 90;

        player.Heal(30);

        Assert.AreEqual(player.CurrentHealth, player.MaxHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("healed for 10")));
    }

    [TestMethod]
    public void Heal_FrozenReducesHealingReceived()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 50;

        player.ActiveEffectManager.Register(
            CreateFrozenEffect(power: 10));

        player.Heal(30);

        Assert.AreEqual(70, player.CurrentHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("healed for 20")));
    }
    
    [TestMethod]
    public void Heal_FrozenCanReduceHealingToZero()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 50;

        player.ActiveEffectManager.Register(
            CreateFrozenEffect(power: 30));

        player.Heal(20);

        Assert.AreEqual(50, player.CurrentHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("healed for 0")));
    }
    
    [TestMethod]
    public void Heal_FrozenReductionAppliedBeforeMaxHealthCap()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 90;

        player.ActiveEffectManager.Register(
            CreateFrozenEffect(power: 10));

        player.Heal(30);

        Assert.AreEqual(100, player.CurrentHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("healed for 10")));
    }
    
    [TestMethod]
    public void Heal_RemovingFrozenRestoresNormalHealing()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 50;

        var frozen = CreateFrozenEffect(power: 10);
        player.ActiveEffectManager.Register(frozen);
        player.ActiveEffectManager.RemoveById(frozen.EffectId);

        player.Heal(30);

        Assert.AreEqual(80, player.CurrentHealth);
    }
    
    [TestMethod]
    public void Heal_NegativeAmountThrows()
    {
        var player = CreatePlayer();

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => player.Heal(-1));
    }
    
    [TestMethod]
    public void Heal_AtMaxHealthDoesNotIncreaseHealth()
    {
        var player = CreatePlayer();

        player.Heal(30);

        Assert.AreEqual(100, player.CurrentHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("healed for 0")));
    }
}
