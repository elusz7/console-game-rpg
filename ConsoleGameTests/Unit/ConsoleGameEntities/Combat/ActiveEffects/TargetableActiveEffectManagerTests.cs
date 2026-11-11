using ConsoleGameEntities.Data;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Combat.ActiveEffects;

[TestClass]
public class TargetableActiveEffectManagerTests
{
    [TestMethod]
    public void NewCombatants_InitializeActiveEffectManager()
    {
        ITargetable player = new Player();
        ITargetable monster = new Monster();
        ITargetable boss = new BossMonster();
        ITargetable elite = new EliteMonster();

        Assert.IsNotNull(player.ActiveEffectManager);
        Assert.IsNotNull(monster.ActiveEffectManager);
        Assert.IsNotNull(boss.ActiveEffectManager);
        Assert.IsNotNull(elite.ActiveEffectManager);
    }

    [TestMethod]
    public void DifferentCombatants_DoNotShareActiveEffectManager()
    {
        ITargetable player1 = new Player();
        ITargetable player2 = new Player();

        ITargetable monster1 = new Monster();
        ITargetable monster2 = new Monster();

        ITargetable boss = new BossMonster();
        ITargetable elite = new EliteMonster();

        Assert.AreNotSame(
            player1.ActiveEffectManager,
            player2.ActiveEffectManager);

        Assert.AreNotSame(
            monster1.ActiveEffectManager,
            monster2.ActiveEffectManager);

        Assert.AreNotSame(
            player1.ActiveEffectManager,
            monster1.ActiveEffectManager);

        Assert.AreNotSame(
            boss.ActiveEffectManager,
            elite.ActiveEffectManager);
    }
    
    [TestMethod]
    public void ActiveEffectManager_IsNotMappedByEntityFramework()
    {
        var options = new DbContextOptionsBuilder<GameContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var context = new GameContext(options);

        var playerEntity = context.Model.FindEntityType(typeof(Player));
        var monsterEntity = context.Model.FindEntityType(typeof(Monster));

        Assert.IsNotNull(playerEntity);
        Assert.IsNotNull(monsterEntity);

        Assert.IsNull(
            playerEntity.FindProperty(nameof(Player.ActiveEffectManager)));

        Assert.IsNull(
            monsterEntity.FindProperty(nameof(Monster.ActiveEffectManager)));
    }
}