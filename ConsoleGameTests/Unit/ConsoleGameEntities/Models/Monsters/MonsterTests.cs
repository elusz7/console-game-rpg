using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters;

[TestClass]
public class MonsterTests
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

    private static Item CreateItem()
    {
        return new Item
        {
            Id = 6,
            Name = "Test Item",
            Value = 56.7M,
            RequiredLevel = 1,
            Durability = 1
        };
    }
    
    private static Player CreateTestPlayer()
    {
        var archetype = CreateArchetype(
            "Test Archetype",
            ArchetypeType.Martial);

        var player = CreatePlayer(
            "Test Player",
            archetype,
            CreateInventory(100));

        player.MaxHealth = 100;
        player.CurrentHealth = 100;
        player.DodgeChance = 0;

        return player;
    }
    private sealed class TestPlayer(AttackResolutionResult result) : Player
    {
        public override AttackResolutionResult TakeDamage(AttackDamage attack)
        {
            return result;
        }
    }
    

    private static Monster CreateElementalMonster(int elementalPower)
    {
        var monster = CreateMonster();

        monster.AttackPower = 0;
        monster.AttackElement = ElementType.Fire;
        monster.ElementalPower = elementalPower;
        monster.ThreatLevel = ThreatLevel.Low;

        return monster;
    }

    [TestMethod]
    public void SetLevel_SetsExpectedStatRanges_ForThreatLevel()
    {
        var monster = CreateMonster();
        monster.ThreatLevel = ThreatLevel.Medium;
        int level = 5;

        monster.SetLevel(level);

        Assert.AreEqual(level, monster.Level);

        // Based on the ThreatLevel.Medium multipliers:
        Assert.IsTrue(monster.AttackPower >= level * 3 && monster.AttackPower <= level * 6);
        Assert.IsTrue(monster.DefensePower >= level * 2 && monster.DefensePower <= level * 4);
        Assert.IsTrue(monster.Resistance >= level * 2 && monster.Resistance <= level * 4);
        Assert.IsTrue(monster.AggressionLevel is >= 2 and <= 4);
        Assert.IsTrue(monster.MaxHealth >= level * 8 && monster.MaxHealth <= level * 12);
        Assert.AreEqual(monster.MaxHealth, monster.CurrentHealth);
    }

    [TestMethod]
    public void Heal_IncreasesCurrentHealth()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 50;

        monster.Heal(20);

        Assert.AreEqual(70, monster.CurrentHealth);
    }

    [TestMethod]
    public void ClearActionItems_RemovesAll()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        monster.AddActionItem("Test action");
        Assert.IsTrue(monster.ActionItems.Count > 0);

        monster.ClearActionItems();

        Assert.AreEqual(0, monster.ActionItems.Count);
    }

    [TestMethod]
    public void AddActionItem_AddsAction()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        monster.AddActionItem("Test action");
        Assert.IsTrue(monster.ActionItems.Values.Contains("Test action"));
    }

    [TestMethod]
    public void AddActionItem_Skill_AddsAction()
    {
        var monster = CreateMonster();
        monster.CurrentHealth = 10;
        var skill = new MartialSkill { Name = "Fireball" };
        monster.AddActionItem(skill);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains("uses Fireball")));
    }

    [TestMethod]
    public void Loot_NoLoot()
    {
        var monster = CreateMonster();

        var item = monster.Loot();

        Assert.IsNull(item);
    }

    [TestMethod]
    public void Loot_HasLoot()
    {
        var monster = CreateMonster();
        var item = CreateItem();

        monster.SetLoot(item);

        var loot = monster.Loot();

        Assert.IsNotNull(loot);
        Assert.AreSame(item, loot);
        Assert.IsNull(loot.Monster);
        Assert.IsNull(loot.MonsterId);
        Assert.IsNull(monster.Treasure);
    }

    [TestMethod]
    public void SetLoot()
    {
        var monster = CreateMonster();
        var loot = CreateItem();

        Assert.IsNull(loot.Monster);
        Assert.IsNull(loot.MonsterId);
        Assert.IsNull(monster.Treasure);

        monster.SetLoot(loot);

        Assert.IsNotNull(loot.Monster);
        Assert.IsNotNull(loot.MonsterId);
        Assert.IsNotNull(monster.Treasure);
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_RegistersDamageOverTime()
    {
        var monster = CreateElementalMonster(8);
        var player = CreateTestPlayer();

        monster.MakeBasicAttack(player);

        var effects = player.ActiveEffectManager.GetByType(
            ActiveEffectType.ElementalDamageOverTime);

        Assert.AreEqual(1, effects.Count);
        Assert.AreEqual(2, effects[0].Power);
        Assert.AreEqual(3, effects[0].Duration);
        Assert.AreSame(monster, effects[0].SourceCaster);
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_UsesQuarterElementalPowerForDoT()
    {
        var monster = CreateElementalMonster(12);
        var player = CreateTestPlayer();

        monster.MakeBasicAttack(player);

        var effect = player.ActiveEffectManager
            .GetByType(ActiveEffectType.ElementalDamageOverTime)
            .Single();

        Assert.AreEqual(3, effect.Power);
    }
    
    [TestMethod]
    public void MakeBasicAttack_LowElementalPower_UsesMinimumDoTPowerOfOne()
    {
        var monster = CreateElementalMonster(1);
        var player = CreateTestPlayer();

        monster.MakeBasicAttack(player);

        var effect = player.ActiveEffectManager
            .GetByType(ActiveEffectType.ElementalDamageOverTime)
            .Single();

        Assert.AreEqual(1, effect.Power);
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_RegistersThreeTurnDoT()
    {
        var monster = CreateElementalMonster(8);
        var player = CreateTestPlayer();

        monster.MakeBasicAttack(player);

        var effect = player.ActiveEffectManager
            .GetByType(ActiveEffectType.ElementalDamageOverTime)
            .Single();

        Assert.AreEqual(3, effect.Duration);
        Assert.AreEqual(3, effect.RemainingDuration);
    }
    
    [TestMethod]
    public void MakeBasicAttack_ElementalHit_UsesAttackElementForDoTIdentity()
    {
        var monster = CreateElementalMonster(8);
        monster.AttackElement = ElementType.Ice;

        var player = CreateTestPlayer();

        monster.MakeBasicAttack(player);

        var effect = player.ActiveEffectManager
            .GetByType(ActiveEffectType.ElementalDamageOverTime)
            .Single();

        Assert.AreEqual(
            new ElementalDamageOverTimeEffectKey(ElementType.Ice),
            effect.EffectIdentityKey);
    }
    
    [TestMethod]
    public void MakeBasicAttack_DodgedAttack_DoesNotRegisterDamageOverTime()
    {
        var monster = CreateElementalMonster(8);

        var player = new TestPlayer(new AttackResolutionResult(
            IncomingStandardDamage: 0,
            FinalStandardDamage: 0,
            StandardDamageType: DamageType.Martial,
            IncomingElementalDamage: 8,
            FinalElementalDamage: 0,
            Element: ElementType.Fire,
            IsDodged: true,
            IsCritical: false));

        monster.MakeBasicAttack(player);

        var effects = player.ActiveEffectManager.GetByType(
            ActiveEffectType.ElementalDamageOverTime);

        Assert.AreEqual(0, effects.Count);
    }
    
    [TestMethod]
    public void MakeBasicAttack_NegatedElementalDamage_DoesNotRegisterDamageOverTime()
    {
        var monster = CreateElementalMonster(8);

        var player = new TestPlayer(new AttackResolutionResult(
            IncomingStandardDamage: 0,
            FinalStandardDamage: 0,
            StandardDamageType: DamageType.Martial,
            IncomingElementalDamage: 8,
            FinalElementalDamage: 0,
            Element: ElementType.Fire,
            IsDodged: false,
            IsCritical: false));

        monster.MakeBasicAttack(player);

        var effects = player.ActiveEffectManager.GetByType(
            ActiveEffectType.ElementalDamageOverTime);

        Assert.AreEqual(0, effects.Count);
    }
}