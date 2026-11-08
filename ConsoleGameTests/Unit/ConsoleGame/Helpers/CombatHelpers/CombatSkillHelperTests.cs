using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.CombatHelpers;

[TestClass]
public class CombatSkillHelperTests
{
    private CombatSkillHelper _combatSkillHelper = null!;

    [TestInitialize]
    public void Setup()
    {
        _combatSkillHelper = new CombatSkillHelper();
    }

    [TestMethod]
    public void GetAvailableSkills_RegularSkillReady_ReturnsUsableSkill()
    {
        var player = CreateTestPlayer();
        var skill = CreateTestSkill();

        player.Archetype.Skills.Add(skill);

        var result = _combatSkillHelper.GetAvailableSkills(player);

        Assert.AreEqual(1, result.UsableSkills.Count);
        Assert.AreSame(skill, result.UsableSkills[0]);
        Assert.AreEqual(0, result.WaitingSkills.Count);
    }

    [TestMethod]
    public void GetAvailableSkills_SkillRequiresHigherLevel_ExcludesSkill()
    {
        var player = CreateTestPlayer();
        var skill = CreateTestSkill(requiredLevel: 5);

        player.Archetype.Skills.Add(skill);

        var result = _combatSkillHelper.GetAvailableSkills(player);

        Assert.AreEqual(0, result.UsableSkills.Count);
        Assert.AreEqual(0, result.WaitingSkills.Count);
    }

    [TestMethod]
    public void GetAvailableSkills_SkillOnCooldown_ReturnsWaitingSkill()
    {
        var player = CreateTestPlayer();
        var skill = CreateTestSkill(cooldown: 3);

        skill.ElapsedTime = 0;
        player.Archetype.Skills.Add(skill);

        var result = _combatSkillHelper.GetAvailableSkills(player);

        Assert.AreEqual(0, result.UsableSkills.Count);
        Assert.AreEqual(1, result.WaitingSkills.Count);
        Assert.AreSame(skill, result.WaitingSkills[0]);
    }

    [TestMethod]
    public void GetAvailableSkills_InsufficientCurrentResource_ReturnsWaitingSkill()
    {
        var player = CreateTestPlayer();
        player.Archetype.CurrentResource = 5;

        var skill = CreateTestSkill(cost: 10);
        player.Archetype.Skills.Add(skill);

        var result = _combatSkillHelper.GetAvailableSkills(player);

        Assert.AreEqual(0, result.UsableSkills.Count);
        Assert.AreEqual(1, result.WaitingSkills.Count);
        Assert.AreSame(skill, result.WaitingSkills[0]);
    }

    [TestMethod]
    public void GetAvailableSkills_CostExceedsMaximumResource_ExcludesWaitingSkill()
    {
        var player = CreateTestPlayer();
        player.Archetype.MaxResource = 20;
        player.Archetype.CurrentResource = 10;

        var skill = CreateTestSkill(cost: 30);
        player.Archetype.Skills.Add(skill);

        var result = _combatSkillHelper.GetAvailableSkills(player);

        Assert.AreEqual(0, result.UsableSkills.Count);
        Assert.AreEqual(0, result.WaitingSkills.Count);
    }

    [TestMethod]
    public void GetAvailableSkills_ReadyAndAffordableUltimate_ReturnsUsableSkill()
    {
        var player = CreateTestPlayer();

        var ultimate = CreateTestUltimateSkill(cost: 20);
        ultimate.ElapsedTime = ultimate.Cooldown;

        player.Archetype.Skills.Add(ultimate);

        var result = _combatSkillHelper.GetAvailableSkills(player);

        Assert.AreEqual(1, result.UsableSkills.Count);
        Assert.AreSame(ultimate, result.UsableSkills[0]);
        Assert.AreEqual(0, result.WaitingSkills.Count);
    }

    [TestMethod]
    public void GetAvailableSkills_UltimateNotReady_ReturnsWaitingSkill()
    {
        var player = CreateTestPlayer();

        var ultimate = CreateTestUltimateSkill();
        ultimate.ElapsedTime = 0;

        player.Archetype.Skills.Add(ultimate);

        var result = _combatSkillHelper.GetAvailableSkills(player);

        Assert.AreEqual(0, result.UsableSkills.Count);
        Assert.AreEqual(1, result.WaitingSkills.Count);
        Assert.AreSame(ultimate, result.WaitingSkills[0]);
    }

    [TestMethod]
    public void GetAvailableSkills_UltimateReadyButUnaffordable_ExcludesFromUsableSkills()
    {
        var player = CreateTestPlayer();
        player.Archetype.CurrentResource = 5;

        var ultimate = CreateTestUltimateSkill(cost: 20);
        ultimate.ElapsedTime = ultimate.Cooldown;

        player.Archetype.Skills.Add(ultimate);

        var result = _combatSkillHelper.GetAvailableSkills(player);

        Assert.AreEqual(0, result.UsableSkills.Count);
        Assert.AreEqual(0, result.WaitingSkills.Count);
    }

    private static Player CreateTestPlayer()
    {
        return new Player
        {
            Name = "Test Player",
            Level = 1,
            CurrentHealth = 100,
            MaxHealth = 100,
            Archetype = new Archetype
            {
                Name = "Test Archetype",
                ArchetypeType = ArchetypeType.Martial,
                Skills = new List<Skill>(),
                CurrentResource = 100,
                MaxResource = 100,
                RecoveryRate = 0
            }
        };
    }

    private static Skill CreateTestSkill(
        int requiredLevel = 1,
        int cost = 10,
        int cooldown = 2)
    {
        var skill = new Skill
        {
            Name = "Test Skill",
            Description = "A test skill.",
            RequiredLevel = requiredLevel,
            Cost = cost,
            Power = 10,
            Cooldown = cooldown,
            TargetType = TargetType.Self,
            SkillCategory = SkillCategory.Support
        };

        // Initialize the skill so it starts off cooldown.
        skill.InitializeSkill(1);

        return skill;
    }

    private static UltimateSkill CreateTestUltimateSkill(int cost = 20)
    {
        return new UltimateSkill
        {
            Name = "Test Ultimate",
            Description = "A test ultimate.",
            RequiredLevel = 1,
            Cost = cost,
            Power = 50,
            Cooldown = 3,
            TargetType = TargetType.SingleEnemy,
            SkillCategory = SkillCategory.Ultimate,
            DamageType = DamageType.Magical
        };
    }
}
