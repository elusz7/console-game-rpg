using ConsoleGame.Helpers;
using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.CombatHelpers;

[TestClass]
public class CombatRunnerElementalEffectTests
{
    private static readonly ElementType[] Elements =
    [
        ElementType.Fire,
        ElementType.Ice,
        ElementType.Lightning,
        ElementType.Nature,
        ElementType.Radiance,
        ElementType.Abyssal
    ];

    private static Monster CreateMonster()
    {
        return new Monster
        {
            Name = "Effect Test Monster",
            Level = 1,
            AttackPower = 10,
            DefensePower = 5,
            Resistance = 3,
            AggressionLevel = 10,
            MaxHealth = 200,
            CurrentHealth = 200,
            DodgeChance = 0,
            DamageType = DamageType.Martial
        };
    }

    private static ActiveEffect CreateDot(ElementType element, int power, int duration)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalDamageOverTime,
            new ElementalDamageOverTimeEffectKey(element),
            power,
            duration,
            sourceCaster: Mock.Of<ITargetable>());
    }

    private static ActiveEffect CreateStatus(
        ElementalStatusEffectType status,
        int power,
        int duration)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(status),
            power,
            duration,
            sourceCaster: Mock.Of<ITargetable>());
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire)]
    [DataRow(ElementType.Ice)]
    [DataRow(ElementType.Lightning)]
    [DataRow(ElementType.Nature)]
    [DataRow(ElementType.Radiance)]
    [DataRow(ElementType.Abyssal)]
    public void CombatRunner_ElementalDoT_ResilienceAppliesToAllElements(ElementType element)
    {
        var target = CreateMonster();
        target.Resilience = element;
        target.ActiveEffectManager.Register(CreateDot(element, 12, 2));

        var result = target.ActiveEffectManager.ProcessDoTTicks(target);

        Assert.AreEqual(200, target.CurrentHealth, $"{element} resilience should negate its DoT.");
        Assert.AreEqual(0, result.EffectResults.Single().FinalDamage);
        Assert.AreEqual(element, result.EffectResults.Single().Element);
    }
    
    [DataTestMethod]
    [DataRow(ElementType.Fire)]
    [DataRow(ElementType.Ice)]
    [DataRow(ElementType.Lightning)]
    [DataRow(ElementType.Nature)]
    [DataRow(ElementType.Radiance)]
    [DataRow(ElementType.Abyssal)]
    public void CombatRunner_ElementalDoT_VulnerabilityAppliesToAllElements(ElementType element)
    {
        var target = CreateMonster();
        target.Vulnerability = element;
        target.ActiveEffectManager.Register(CreateDot(element, 10, 2));

        var result = target.ActiveEffectManager.ProcessDoTTicks(target);

        Assert.AreEqual(185, target.CurrentHealth, $"{element} vulnerability should increase its DoT by 50%.");
        Assert.AreEqual(15, result.EffectResults.Single().FinalDamage);
    }
    
    [TestMethod]
    public void CombatRunner_ElementalDoT_RefreshPreservesOriginalDurationRules()
    {
        var manager = new ActiveEffectManager();
        var original = CreateDot(ElementType.Fire, 10, 5);
        manager.Register(original);
        manager.ProcessDoTTicks(CreateMonster());
        Assert.AreEqual(4, original.RemainingDuration);

        var stronger = CreateDot(ElementType.Fire, 15, 3);
        var strengthened = manager.Register(stronger);
        Assert.AreEqual(RegistrationStatus.Strengthened, strengthened.Registration);
        Assert.AreEqual(15, original.Power);
        Assert.AreEqual(3, original.Duration);
        Assert.AreEqual(3, original.RemainingDuration);

        manager.ProcessDoTTicks(CreateMonster());
        Assert.AreEqual(2, original.RemainingDuration);
        
        var equal = CreateDot(ElementType.Fire, 15, 6);
        var refreshed = manager.Register(equal);
        Assert.AreEqual(RegistrationStatus.Refreshed, refreshed.Registration);
        Assert.AreEqual(15, original.Power);
        Assert.AreEqual(6, original.Duration);
        Assert.AreEqual(6, original.RemainingDuration);

        var weaker = CreateDot(ElementType.Fire, 8, 9);
        var rejected = manager.Register(weaker);
        Assert.AreEqual(RegistrationStatus.RejectedWeaker, rejected.Registration);
        Assert.AreEqual(15, original.Power);
        Assert.AreEqual(6, original.RemainingDuration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
    }
    
    [TestMethod]
    public void CombatRunner_ElementalDoT_DifferentElementsCoexist()
    {
        var manager = new ActiveEffectManager();
        var effects = Elements.Select(element => CreateDot(element, 5, 3)).ToList();

        foreach (var effect in effects)
            manager.Register(effect);

        Assert.AreEqual(Elements.Length, manager.ActiveEffects.Count);
        CollectionAssert.AreEquivalent(
            Elements,
            manager.ActiveEffects
                .Select(effect => ((ElementalDamageOverTimeEffectKey)effect.EffectIdentityKey).Element)
                .ToArray());

        var target = CreateMonster();
        var tick = manager.ProcessDoTTicks(target);
        Assert.AreEqual(Elements.Length, tick.ProcessedEffects.Count);
        Assert.AreEqual(Elements.Length, tick.EffectResults.Count);
        Assert.AreEqual(200 - (Elements.Length * 5), target.CurrentHealth);
    }
    
    [TestMethod]
    public void CombatRunner_ElementalStatus_RefreshDoesNotDuplicateEffect()
    {
        var manager = new ActiveEffectManager();
        var original = CreateStatus(ElementalStatusEffectType.Charred, 3, 2);
        manager.Register(original);
        manager.TickTurnBased();

        var refreshed = manager.Register(CreateStatus(ElementalStatusEffectType.Charred, 5, 4));

        Assert.AreEqual(RegistrationStatus.Strengthened, refreshed.Registration);
        Assert.AreEqual(1, manager.ActiveEffects.Count);
        Assert.AreSame(original, manager.ActiveEffects.Single());
        Assert.AreEqual(5, original.Power);
        Assert.AreEqual(4, original.RemainingDuration);
        Assert.AreEqual(-5, manager.GetStatModifier(StatType.Defense));
    }
    
    [TestMethod]
    public void CombatRunner_ElementalStatus_ExpirationRestoresBaseStats()
    {
        var monster = CreateMonster();
        var baseDefense = monster.GetStat(StatType.Defense);
        var baseResistance = monster.GetStat(StatType.Resistance);
        var baseSpeed = monster.GetStat(StatType.Speed);

        monster.ActiveEffectManager.Register(CreateStatus(ElementalStatusEffectType.Charred, 4, 1));
        monster.ActiveEffectManager.Register(CreateStatus(ElementalStatusEffectType.Shocked, 3, 1));
        monster.ActiveEffectManager.Register(CreateStatus(ElementalStatusEffectType.Snared, 2, 1));

        Assert.AreEqual(baseDefense - 4, monster.GetStat(StatType.Defense));
        Assert.AreEqual(baseResistance - 3, monster.GetStat(StatType.Resistance));
        Assert.AreEqual(baseSpeed - 2, monster.GetStat(StatType.Speed));

        var tick = monster.ActiveEffectManager.TickTurnBased();

        Assert.AreEqual(3, tick.ExpiredEffects.Count);
        Assert.AreEqual(baseDefense, monster.GetStat(StatType.Defense));
        Assert.AreEqual(baseResistance, monster.GetStat(StatType.Resistance));
        Assert.AreEqual(baseSpeed, monster.GetStat(StatType.Speed));
        Assert.AreEqual(0, monster.ActiveEffectManager.ActiveEffects.Count);
    }
    
        [TestMethod]
    public void CombatRunner_ElementalStatus_DeathCleanupRemovesEffects()
    {
        var player = new Player
        {
            Name = "Cleanup Test Player",
            Level = 1,
            CurrentHealth = 1,
            MaxHealth = 100,
            DodgeChance = 0,
            Archetype = new Archetype
            {
                Name = "Cleanup Archetype",
                ArchetypeType = ArchetypeType.Martial,
                Skills = [],
                CurrentResource = 10,
                MaxResource = 10,
                RecoveryRate = 0
            },
            Inventory = new Inventory { Items = [] }
        };
        player.ActiveEffectManager.Register(CreateStatus(ElementalStatusEffectType.Charred, 4, 3));
        player.ActiveEffectManager.Register(CreateDot(ElementType.Fire, 5, 3));

        var monster = new LethalTestMonster
        {
            Name = "Lethal Test Monster",
            Level = 1,
            AttackPower = 100,
            DefensePower = 1,
            Resistance = 1,
            AggressionLevel = 100,
            MaxHealth = 100,
            CurrentHealth = 100,
            DodgeChance = 0,
            DamageType = DamageType.Martial
        };
        monster.ActiveEffectManager.Register(CreateStatus(ElementalStatusEffectType.Shocked, 2, 3));

        var input = new Mock<IInputManager>();
        var output = new Mock<IOutputManager>();
        var equipment = new EquipmentHelper(input.Object, output.Object);
        var coordinator = new CombatRoundCoordinator(new CombatTurnScheduler());
        var combat = new CombatHelper(
            input.Object,
            output.Object,
            equipment,
            coordinator,
            new CombatActionDisplayHelper(output.Object),
            new CombatMenuDisplayHelper(output.Object),
            new CombatSkillHelper());

        combat.CombatRunner(player, [monster]);

        Assert.AreEqual(0, player.CurrentHealth);
        Assert.AreEqual(0, player.ActiveEffectManager.ActiveEffects.Count,
            "Player elemental effects should be cleared when the player dies.");
        Assert.AreEqual(0, monster.ActiveEffectManager.ActiveEffects.Count,
            "Combatant effects should be cleared when combat ends due to player death.");
    }
    
    private sealed class LethalTestMonster : Monster
    {
        public override AttackResolutionResult Attack(IPlayer target)
        {
            target.CurrentHealth = 0;
            CurrentHealth = 0;
            return new AttackResolutionResult(0, 0, DamageType.Martial, 0, 0, null, false, false);
        }
    }
    
}