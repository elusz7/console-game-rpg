using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGameEntities.Models;

[TestClass]
public class ArchetypeTests
{
    private static Archetype CreateArchetype(ArchetypeType type = ArchetypeType.Martial)
    {
        return new Archetype
        {
            Name = "Test",
            ArchetypeType = type,
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
    }

    [TestMethod]
    public void UseResource_SufficientResource_ShouldDeduct()
    {
        var archetype = CreateArchetype();
        archetype.UseResource(3);
        Assert.AreEqual(7, archetype.CurrentResource);
    }

    [TestMethod]
    public void UseResource_InsufficientResource_ShouldThrow()
    {
        var archetype = CreateArchetype();
        archetype.CurrentResource = 1;

        Assert.ThrowsException<InvalidOperationException>(() => archetype.UseResource(3));
    }

    [TestMethod]
    public void RecoverResource_DefaultRate_ShouldRecover()
    {
        var archetype = CreateArchetype();
        archetype.CurrentResource = 5;
        archetype.RecoverResource();
        Assert.AreEqual(7, archetype.CurrentResource);
    }

    [TestMethod]
    public void RecoverResource_DefaultRate_ShouldNotExceedMax()
    {
        var archetype = CreateArchetype();
        archetype.CurrentResource = 9;
        archetype.RecoverResource();
        Assert.AreEqual(10, archetype.CurrentResource);
    }

    [TestMethod]
    public void RecoverResource_WithParam_ShouldRecover()
    {
        var archetype = CreateArchetype();
        archetype.CurrentResource = 3;
        archetype.RecoverResource(4);
        Assert.AreEqual(7, archetype.CurrentResource);
    }

    [TestMethod]
    public void RecoverResource_WithParam_ShouldNotExceedMax()
    {
        var archetype = CreateArchetype();
        archetype.CurrentResource = 8;
        archetype.RecoverResource(5);
        Assert.AreEqual(10, archetype.CurrentResource);
    }

    [TestMethod]
    public void LevelUp_Martial_ShouldScaleStats()
    {
        var archetype = CreateArchetype(ArchetypeType.Martial);
        var newLevel = 4;

        var oldAttackBonus = archetype.AttackBonus;
        var result = archetype.LevelUp(newLevel);

        Assert.IsTrue(result >= 2 && result <= archetype.HealthBase);
        Assert.IsTrue(archetype.AttackBonus > oldAttackBonus);
        Assert.AreEqual(archetype.MaxResource, archetype.CurrentResource);
    }

    [TestMethod]
    public void LevelUp_Magical_ShouldScaleStats()
    {
        var archetype = CreateArchetype(ArchetypeType.Magical);
        var newLevel = 4;

        var oldMagicBonus = archetype.MagicBonus;
        var result = archetype.LevelUp(newLevel);

        Assert.IsTrue(result >= 2 && result <= archetype.HealthBase);
        Assert.IsTrue(archetype.MagicBonus > oldMagicBonus);
        Assert.AreEqual(archetype.MaxResource, archetype.CurrentResource);
    }
}