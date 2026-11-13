using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Skills;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;

namespace ConsoleGameTests.Integration.ConsoleGame.GameDao;

[TestClass]
public class SkillDaoTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private SkillDao _skillDao = null!;

    [TestInitialize]
    public void Initialize()
    {
        // Create and open an isolated in-memory SQLite database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Configure the real GameContext to use SQLite.
        var options = new DbContextOptionsBuilder<GameContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new GameContext(options);

        // Create the database schema.
        _context.Database.EnsureCreated();

        // Use the real DAO
        _skillDao = new SkillDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void AddSkill_PersistsSkill()
    {
        // Arrange
        var skill = new MartialSkill()
        {
            Name = "Test Skill",
            Description = "A basic test skill.",
            RequiredLevel = 1,
            Cost = 2,
            Power = 10,
            Cooldown = 3,
            TargetType = TargetType.SingleEnemy,
            IsCore = true,
            DamageType = DamageType.Magical
        };

        // Act
        _skillDao.AddSkill(skill);

        _context.ChangeTracker.Clear();
        var persistedSkill = _context.Skills.Single(s => s.Name == skill.Name);

        // Assert
        Assert.IsTrue(persistedSkill.Id > 0);
        AssertSkillMatches(skill, persistedSkill);
    }

    [TestMethod]
    public void AddSkill_PersistsAllSkillProperties()
    {
        // Arrange
        var skill = (MartialSkill)CreateSkill("Test Martial Skill", TestSkillType.Martial);

        skill.Description = "A fully configured martial skill.";
        skill.IsCore = false;
        skill.RequiredLevel = 5;
        skill.Cost = 7;
        skill.Power = 25;
        skill.Cooldown = 6;
        skill.TargetType = TargetType.SingleEnemy;
        skill.SkillCategory = SkillCategory.Basic;
        skill.DamageType = DamageType.Hybrid;

        // Act
        _skillDao.AddSkill(skill);

        _context.ChangeTracker.Clear();
        var persistedSkill = _context.Skills.Single(s => s.Name == skill.Name);

        // Assert
        Assert.IsTrue(persistedSkill.Id > 0);
        AssertSkillMatches(skill, persistedSkill);
    }

    [TestMethod]
    public void AddSkill_PersistsSkillWithArchetype()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Magical);
        var skill = CreateSkill("Archetype Skill", TestSkillType.Martial);
        skill.Archetype = archetype;

        // Act
        _skillDao.AddSkill(skill);
        _context.ChangeTracker.Clear();

        var persistedSkill = _context.Skills
            .Include(s => s.Archetype)
            .Single(s => s.Name == skill.Name);

        // Assert
        Assert.IsTrue(persistedSkill.Id > 0);
        Assert.IsNotNull(persistedSkill.Archetype);
        Assert.AreEqual(archetype.Name, persistedSkill.Archetype.Name);
        Assert.AreEqual(persistedSkill.Archetype.Id, persistedSkill.ArchetypeId);
    }

    [TestMethod]
    public void AddSkill_PersistsSkillWithMonster()
    {
        // Arrange
        var monster = CreateMonster("Test Monster", ThreatLevel.Boss, DamageType.Martial, TestMonsterType.Common);
        var skill = CreateSkill("Monster Skill", TestSkillType.Martial);
        skill.Monster = monster;

        // Act
        _skillDao.AddSkill(skill);
        _context.ChangeTracker.Clear();

        var persistedSkill = _context.Skills
            .Include(s => s.Monster)
            .Single(s => s.Name == skill.Name);

        // Assert
        Assert.IsTrue(persistedSkill.Id > 0);
        Assert.IsNotNull(persistedSkill.Monster);
        Assert.AreEqual(monster.Name, persistedSkill.Monster.Name);
        Assert.AreEqual(persistedSkill.Monster.Id, persistedSkill.MonsterId);
    }

    [TestMethod]
    public void AddSkill_PersistsNonCoreSkill()
    {
        // Arrange
        var skill = CreateSkill("Non-Core Skill", TestSkillType.Martial);
        skill.IsCore = false;

        // Act
        _skillDao.AddSkill(skill);
        _context.ChangeTracker.Clear();
        
        var persistedSkill = _context.Skills.Single(s => s.Name == skill.Name);

        // Assert
        Assert.IsFalse(persistedSkill.IsCore);
        AssertSkillMatches(skill, persistedSkill);
    }

    [TestMethod]
    public void AddSkill_PersistsMartialSkill()
    {
        // Arrange
        var skill = CreateSkill("Martial Skill", TestSkillType.Martial);

        // Act
        _skillDao.AddSkill(skill);
        _context.ChangeTracker.Clear();
        
        var persistedSkill = _context.Skills.Single(s => s.Name == skill.Name);

        // Assert
        Assert.AreEqual(typeof(MartialSkill), persistedSkill.GetType());
        AssertSkillMatches(skill, persistedSkill);
    }

    [TestMethod]
    public void AddSkill_PersistsMagicSkill()
    {
        // Arrange
        var skill = CreateSkill("Magic Skill", TestSkillType.Magical);

        // Act
        _skillDao.AddSkill(skill);
        _context.ChangeTracker.Clear();
        
        var persistedSkill = _context.Skills.Single(s => s.Name == skill.Name);

        // Assert
        Assert.AreEqual(typeof(MagicSkill), persistedSkill.GetType());
        AssertSkillMatches(skill, persistedSkill);
    }

    [TestMethod]
    public void AddSkill_PersistsOtherDerivedSkillTypes()
    {
        // Arrange
        var supportSkill = CreateSkill("Support Skill", TestSkillType.Support);
        var ultimateSkill = CreateSkill("Ultimate Skill", TestSkillType.Ultimate);
        var bossSkill = CreateSkill("Boss Skill", TestSkillType.Boss);

        var skills = new Skill[]
        {
            supportSkill,
            ultimateSkill,
            bossSkill
        };

        // Act
        foreach (var skill in skills)
        {
            _skillDao.AddSkill(skill);
        }
        _context.ChangeTracker.Clear();

        var persistedSkills = _context.Skills
            .Where(s => skills.Select(x => x.Name).Contains(s.Name))
            .ToList();

        // Assert
        Assert.AreEqual(3, persistedSkills.Count);

        foreach (var originalSkill in skills)
        {
            var persistedSkill = persistedSkills.Single(s => s.Name == originalSkill.Name);

            AssertSkillMatches(originalSkill, persistedSkill);
        }

        Assert.IsInstanceOfType(
            persistedSkills.Single(s => s.Name == supportSkill.Name),
            typeof(SupportSkill));

        Assert.IsInstanceOfType(
            persistedSkills.Single(s => s.Name == ultimateSkill.Name),
            typeof(UltimateSkill));

        Assert.IsInstanceOfType(
            persistedSkills.Single(s => s.Name == bossSkill.Name),
            typeof(BossSkill));
    }

    [TestMethod]
    public void UpdateSkill_UpdatesSkillProperties()
    {
        // Arrange
        var skill = CreateSkill("Original Skill", TestSkillType.Martial);
        _skillDao.AddSkill(skill);

        skill.Name = "Updated Skill";
        skill.Description = "Updated description.";
        skill.RequiredLevel = 5;
        skill.Cost = 8;
        skill.Power = 30;
        skill.Cooldown = 7;
        skill.TargetType = TargetType.AllEnemies;

        // Act
        _skillDao.UpdateSkill(skill);

        _context.ChangeTracker.Clear();
        var persistedSkill = _context.Skills.Single(s => s.Id == skill.Id);

        // Assert
        AssertSkillMatches(skill, persistedSkill);
    }

    [TestMethod]
    public void UpdateSkill_UpdatesArchetypeAssociation()
    {
        // Arrange
        var originalArchetype = CreateArchetype("Original Archetype", ArchetypeType.Magical);
        var updatedArchetype = CreateArchetype("Updated Archetype", ArchetypeType.Martial);

        _context.Archetypes.AddRange(originalArchetype, updatedArchetype);
        _context.SaveChanges();

        var skill = CreateSkill("Test Skill", TestSkillType.Martial);
        skill.Archetype = originalArchetype;

        _skillDao.AddSkill(skill);

        // Act
        skill.Archetype = updatedArchetype;
        skill.ArchetypeId = updatedArchetype.Id;

        _skillDao.UpdateSkill(skill);

        _context.ChangeTracker.Clear();

        var persistedSkill = _context.Skills
            .Include(s => s.Archetype)
            .Single(s => s.Id == skill.Id);

        // Assert
        Assert.AreEqual(updatedArchetype.Id, persistedSkill.ArchetypeId);
        Assert.IsNotNull(persistedSkill.Archetype);
        Assert.AreEqual("Updated Archetype", persistedSkill.Archetype.Name);
    }

    [TestMethod]
    public void UpdateSkill_UpdatesMonsterAssociation()
    {
        // Arrange
        var originalMonster = CreateMonster("Original Monster", ThreatLevel.Elite, DamageType.Martial, TestMonsterType.Common);
        var updatedMonster = CreateMonster("Updated Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Elite);

        _context.Monsters.AddRange(originalMonster, updatedMonster);
        _context.SaveChanges();

        var skill = CreateSkill("Test Skill", TestSkillType.Martial);
        skill.Monster = originalMonster;

        _skillDao.AddSkill(skill);

        // Act
        skill.Monster = updatedMonster;
        skill.MonsterId = updatedMonster.Id;

        _skillDao.UpdateSkill(skill);

        _context.ChangeTracker.Clear();

        var persistedSkill = _context.Skills
            .Include(s => s.Monster)
            .Single(s => s.Id == skill.Id);

        // Assert
        Assert.AreEqual(updatedMonster.Id, persistedSkill.MonsterId);
        Assert.IsNotNull(persistedSkill.Monster);
        Assert.AreEqual("Updated Monster", persistedSkill.Monster.Name);
    }

    [TestMethod]
    public void UpdateSkill_UpdatesIsCore()
    {
        // Arrange
        var skill = CreateSkill("Test Skill", TestSkillType.Martial);
        skill.IsCore = true;

        _skillDao.AddSkill(skill);

        // Act
        skill.IsCore = false;
        _skillDao.UpdateSkill(skill);

        _context.ChangeTracker.Clear();
        var persistedSkill = _context.Skills.Single(s => s.Id == skill.Id);

        // Assert
        Assert.IsFalse(persistedSkill.IsCore);
    }

    [TestMethod]
    public void UpdateSkill_PreservesDerivedSkillTypes()
    {
        // Arrange
        var martialSkill = CreateSkill("Martial Skill", TestSkillType.Martial);
        var magicSkill = CreateSkill("Magic Skill", TestSkillType.Magical);
        var supportSkill = CreateSkill("Support Skill", TestSkillType.Support);
        var ultimateSkill = CreateSkill("Ultimate Skill", TestSkillType.Ultimate);
        var bossSkill = CreateSkill("Boss Skill", TestSkillType.Boss);

        Skill[] skills =
        [
            martialSkill,
            magicSkill,
            supportSkill,
            ultimateSkill,
            bossSkill
        ];

        foreach (var skill in skills)
        {
            _skillDao.AddSkill(skill);
        }

        // Act
        foreach (var skill in skills)
        {
            skill.Name += " Updated";
            skill.Description += " Updated description.";
            skill.Power += 10;
            skill.Cooldown += 2;

            _skillDao.UpdateSkill(skill);
        }

        _context.ChangeTracker.Clear();

        var persistedSkills = _context.Skills.ToList();

        // Assert
        Assert.AreEqual(5, persistedSkills.Count);

        foreach (var originalSkill in skills)
        {
            var persistedSkill = persistedSkills.Single(s => s.Id == originalSkill.Id);

            Assert.AreEqual(originalSkill.GetType(), persistedSkill.GetType());
            AssertSkillMatches(originalSkill, persistedSkill);
        }
    }

    [TestMethod]
    public void DeleteSkill_RemovesSkill()
    {
        // Arrange
        var skill = CreateSkill("Test Skill", TestSkillType.Martial);
        _skillDao.AddSkill(skill);

        var skillId = skill.Id;

        // Act
        _skillDao.DeleteSkill(skill);

        _context.ChangeTracker.Clear();

        var persistedSkill = _context.Skills
            .SingleOrDefault(s => s.Id == skillId);

        // Assert
        Assert.IsNull(persistedSkill);
    }

    [TestMethod]
    public void DeleteSkill_ClearsArchetypeSkillAssociation()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Magical);

        _context.Archetypes.Add(archetype);
        _context.SaveChanges();

        var skill = CreateSkill("Test Skill", TestSkillType.Martial);
        skill.Archetype = archetype;

        _skillDao.AddSkill(skill);

        var archetypeId = archetype.Id;
        var skillId = skill.Id;

        // Act
        _skillDao.DeleteSkill(skill);

        _context.ChangeTracker.Clear();

        var persistedArchetype = _context.Archetypes
            .Include(a => a.Skills)
            .Single(a => a.Id == archetypeId);

        var persistedSkill = _context.Skills
            .SingleOrDefault(s => s.Id == skillId);

        // Assert
        Assert.IsNull(persistedSkill);
        Assert.AreEqual(0, persistedArchetype.Skills.Count);
        Assert.IsTrue(_context.Archetypes.Any(a => a.Id == archetypeId));
    }

    [TestMethod]
    public void DeleteSkill_ClearsMonsterSkillAssociation()
    {
        // Arrange
        var monster = CreateMonster("Test Monster", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);

        _context.Monsters.Add(monster);
        _context.SaveChanges();

        var skill = CreateSkill("Test Skill", TestSkillType.Martial);
        skill.Monster = monster;

        _skillDao.AddSkill(skill);

        var monsterId = monster.Id;
        var skillId = skill.Id;

        // Act
        _skillDao.DeleteSkill(skill);

        _context.ChangeTracker.Clear();

        var persistedMonster = _context.Monsters
            .Include(m => m.Skills)
            .Single(m => m.Id == monsterId);

        var persistedSkill = _context.Skills
            .SingleOrDefault(s => s.Id == skillId);

        // Assert
        Assert.IsNull(persistedSkill);
        Assert.AreEqual(0, persistedMonster.Skills.Count);
        Assert.IsTrue(_context.Monsters.Any(m => m.Id == monsterId));
    }

    [TestMethod]
    public void GetAllSkills_ReturnsAllSkills()
    {
        // Arrange
        var martial = CreateSkill("Martial Skill", TestSkillType.Martial);
        var magic = CreateSkill("Magic Skill", TestSkillType.Magical);
        var support = CreateSkill("Support Skill", TestSkillType.Support);

        _skillDao.AddSkill(martial);
        _skillDao.AddSkill(magic);
        _skillDao.AddSkill(support);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetAllSkills();

        // Assert
        Assert.AreEqual(3, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { martial.Id, magic.Id, support.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetAllSkills_ReturnsEmptyListWhenNoSkillsExist()
    {
        var skills = _skillDao.GetAllSkills();
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetSkillsByName_ReturnsExactAndPartialMatches()
    {
        // Arrange
        var exactMatch = CreateSkill("Fireball", TestSkillType.Magical);
        var partialMatch = CreateSkill("Greater Fireball", TestSkillType.Magical);
        var noMatch = CreateSkill("Healing Light", TestSkillType.Support);

        _skillDao.AddSkill(exactMatch);
        _skillDao.AddSkill(partialMatch);
        _skillDao.AddSkill(noMatch);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByName("Fireball");

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { exactMatch.Id, partialMatch.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetSkillsByName_IsCaseInsensitive()
    {
        // Arrange
        var skill = CreateSkill("Fireball", TestSkillType.Magical);
        _skillDao.AddSkill(skill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByName("fireBALL");

        // Assert
        Assert.AreEqual(1, skills.Count);
        Assert.AreEqual(skill.Id, skills[0].Id);
    }

    [TestMethod]
    public void GetSkillsByName_ReturnsAllMatchingSkills()
    {
        // Arrange
        var fireball = CreateSkill("Fireball", TestSkillType.Magical);
        var greaterFireball = CreateSkill("Greater Fireball", TestSkillType.Magical);
        var fireBlast = CreateSkill("Fire Blast", TestSkillType.Magical);
        var healing = CreateSkill("Healing Light", TestSkillType.Support);

        _skillDao.AddSkill(fireball);
        _skillDao.AddSkill(greaterFireball);
        _skillDao.AddSkill(fireBlast);
        _skillDao.AddSkill(healing);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByName("Fire");

        // Assert
        Assert.AreEqual(3, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { fireball.Id, greaterFireball.Id, fireBlast.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetSkillsByName_ReturnsEmptyListWhenNoMatch()
    {
        // Arrange
        var fireball = CreateSkill("Fireball", TestSkillType.Magical);
        var healing = CreateSkill("Healing Light", TestSkillType.Support);

        _skillDao.AddSkill(fireball);
        _skillDao.AddSkill(healing);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByName("Lightning");

        // Assert
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetAllNonCoreSkills_ReturnsOnlyNonCoreSkills()
    {
        // Arrange
        var nonCoreMartial = CreateSkill("Non-Core Martial", TestSkillType.Martial);
        var nonCoreMagic = CreateSkill("Non-Core Magic", TestSkillType.Magical);
        var coreSkill = CreateSkill("Core Skill", TestSkillType.Support);

        nonCoreMartial.IsCore = false;
        nonCoreMagic.IsCore = false;
        coreSkill.IsCore = true;

        _skillDao.AddSkill(nonCoreMartial);
        _skillDao.AddSkill(nonCoreMagic);
        _skillDao.AddSkill(coreSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetAllNonCoreSkills();

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { nonCoreMartial.Id, nonCoreMagic.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetAllNonCoreSkills_ReturnsEmptyListWhenNoNonCoreSkillsExist()
    {
        // Arrange
        var coreMartial = CreateSkill("Core Martial", TestSkillType.Martial);
        var coreMagic = CreateSkill("Core Magic", TestSkillType.Magical);

        coreMartial.IsCore = true;
        coreMagic.IsCore = true;

        _skillDao.AddSkill(coreMartial);
        _skillDao.AddSkill(coreMagic);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetAllNonCoreSkills();

        // Assert
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetArchetypes_ReturnsDistinctAssociatedArchetypes()
    {
        // Arrange
        var warrior = CreateArchetype("Warrior", ArchetypeType.Magical);
        var mage = CreateArchetype("Mage", ArchetypeType.Martial);

        var skill1 = CreateSkill("Slash", TestSkillType.Martial);
        var skill2 = CreateSkill("Heavy Slash", TestSkillType.Martial);
        var skill3 = CreateSkill("Fireball", TestSkillType.Magical);

        skill1.Archetype = warrior;
        skill2.Archetype = warrior;
        skill3.Archetype = mage;

        _skillDao.AddSkill(skill1);
        _skillDao.AddSkill(skill2);
        _skillDao.AddSkill(skill3);

        // Act
        _context.ChangeTracker.Clear();
        var archetypes = _skillDao.GetArchetypes();

        // Assert
        Assert.AreEqual(2, archetypes.Count);
        CollectionAssert.AreEquivalent(
            new[] { warrior.Id, mage.Id },
            archetypes.Select(a => a.Id).ToArray());
    }

    [TestMethod]
    public void GetArchetypes_ExcludesUnassociatedSkills()
    {
        // Arrange
        var warrior = CreateArchetype("Warrior", ArchetypeType.Magical);

        var associatedSkill = CreateSkill("Slash", TestSkillType.Martial);
        var unassociatedSkill = CreateSkill("Fireball", TestSkillType.Magical);

        associatedSkill.Archetype = warrior;

        _skillDao.AddSkill(associatedSkill);
        _skillDao.AddSkill(unassociatedSkill);

        // Act
        _context.ChangeTracker.Clear();
        var archetypes = _skillDao.GetArchetypes();

        // Assert
        Assert.AreEqual(1, archetypes.Count);
        Assert.AreEqual(warrior.Id, archetypes[0].Id);
    }

    [TestMethod]
    public void GetArchetypes_ReturnsEmptyListWhenNoAssociationsExist()
    {
        // Arrange
        var skill1 = CreateSkill("Slash", TestSkillType.Martial);
        var skill2 = CreateSkill("Fireball", TestSkillType.Magical);

        _skillDao.AddSkill(skill1);
        _skillDao.AddSkill(skill2);

        // Act
        _context.ChangeTracker.Clear();
        var archetypes = _skillDao.GetArchetypes();

        // Assert
        Assert.AreEqual(0, archetypes.Count);
    }

    [TestMethod]
    public void GetMonsters_ReturnsDistinctAssociatedMonsters()
    {
        // Arrange
        var monster1 = CreateMonster(
            "Goblin",
            ThreatLevel.Low,
            DamageType.Martial,
            TestMonsterType.Boss);

        var monster2 = CreateMonster(
            "Orc",
            ThreatLevel.High,
            DamageType.Magical,
            TestMonsterType.Elite);

        var skill1 = CreateSkill("Slash", TestSkillType.Martial);
        var skill2 = CreateSkill("Heavy Slash", TestSkillType.Martial);
        var skill3 = CreateSkill("Fireball", TestSkillType.Magical);

        skill1.Monster = monster1;
        skill2.Monster = monster1;
        skill3.Monster = monster2;

        _skillDao.AddSkill(skill1);
        _skillDao.AddSkill(skill2);
        _skillDao.AddSkill(skill3);

        // Act
        _context.ChangeTracker.Clear();
        var monsters = _skillDao.GetMonsters();

        // Assert
        Assert.AreEqual(2, monsters.Count);
        CollectionAssert.AreEquivalent(
            new[] { monster1.Id, monster2.Id },
            monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetMonsters_ExcludesUnassociatedSkills()
    {
        // Arrange
        var monster = CreateMonster(
            "Goblin",
            ThreatLevel.Medium,
            DamageType.Martial,
            TestMonsterType.Common);

        var associatedSkill = CreateSkill("Slash", TestSkillType.Martial);
        var unassociatedSkill = CreateSkill("Fireball", TestSkillType.Magical);

        associatedSkill.Monster = monster;

        _skillDao.AddSkill(associatedSkill);
        _skillDao.AddSkill(unassociatedSkill);

        // Act
        _context.ChangeTracker.Clear();
        var monsters = _skillDao.GetMonsters();

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(monster.Id, monsters[0].Id);
    }

    [TestMethod]
    public void GetMonsters_ReturnsEmptyListWhenNoAssociationsExist()
    {
        // Arrange
        var skill1 = CreateSkill("Slash", TestSkillType.Martial);
        var skill2 = CreateSkill("Fireball", TestSkillType.Magical);

        _skillDao.AddSkill(skill1);
        _skillDao.AddSkill(skill2);

        // Act
        _context.ChangeTracker.Clear();
        var monsters = _skillDao.GetMonsters();

        // Assert
        Assert.AreEqual(0, monsters.Count);
    }

    [TestMethod]
    public void GetSkillsByLevel_ReturnsSkillsAtExactLevel()
    {
        // Arrange
        var level1 = CreateSkill("Level One", TestSkillType.Martial);
        var level2A = CreateSkill("Level Two A", TestSkillType.Magical);
        var level2B = CreateSkill("Level Two B", TestSkillType.Support);
        var level3 = CreateSkill("Level Three", TestSkillType.Ultimate);

        level1.RequiredLevel = 1;
        level2A.RequiredLevel = 2;
        level2B.RequiredLevel = 2;
        level3.RequiredLevel = 3;

        _skillDao.AddSkill(level1);
        _skillDao.AddSkill(level2A);
        _skillDao.AddSkill(level2B);
        _skillDao.AddSkill(level3);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByLevel(2);

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { level2A.Id, level2B.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetSkillsByLevel_ReturnsEmptyListWhenNoSkillsMatch()
    {
        // Arrange
        var skill1 = CreateSkill("Level One", TestSkillType.Martial);
        var skill2 = CreateSkill("Level Three", TestSkillType.Magical);

        skill1.RequiredLevel = 1;
        skill2.RequiredLevel = 3;

        _skillDao.AddSkill(skill1);
        _skillDao.AddSkill(skill2);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByLevel(2);

        // Assert
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetSkillsByArchetype_ReturnsOnlyAssociatedSkills()
    {
        // Arrange
        var warrior = CreateArchetype("Warrior", ArchetypeType.Magical);
        var mage = CreateArchetype("Mage", ArchetypeType.Martial);

        var warriorSkill1 = CreateSkill("Slash", TestSkillType.Martial);
        var warriorSkill2 = CreateSkill("Heavy Slash", TestSkillType.Martial);
        var mageSkill = CreateSkill("Fireball", TestSkillType.Magical);
        var unassignedSkill = CreateSkill("Healing Light", TestSkillType.Support);

        warriorSkill1.Archetype = warrior;
        warriorSkill2.Archetype = warrior;
        mageSkill.Archetype = mage;

        _skillDao.AddSkill(warriorSkill1);
        _skillDao.AddSkill(warriorSkill2);
        _skillDao.AddSkill(mageSkill);
        _skillDao.AddSkill(unassignedSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByArchetype(warrior);

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { warriorSkill1.Id, warriorSkill2.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetSkillsByArchetype_ReturnsEmptyListWhenNoSkillsAreAssociated()
    {
        // Arrange
        var warrior = CreateArchetype("Warrior", ArchetypeType.Martial);
        var mageSkill = CreateSkill("Fireball", TestSkillType.Magical);
        var unassignedSkill = CreateSkill("Healing Light", TestSkillType.Support);

        mageSkill.Archetype = CreateArchetype("Mage", ArchetypeType.Magical);

        _skillDao.AddSkill(mageSkill);
        _skillDao.AddSkill(unassignedSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByArchetype(warrior);

        // Assert
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetSkillsByMonster_ReturnsOnlyAssociatedSkills()
    {
        // Arrange
        var goblin = CreateMonster(
            "Goblin",
            ThreatLevel.Elite,
            DamageType.Martial,
            TestMonsterType.Common);

        var orc = CreateMonster(
            "Orc",
            ThreatLevel.High,
            DamageType.Magical,
            TestMonsterType.Boss);

        var goblinSkill1 = CreateSkill("Goblin Slash", TestSkillType.Martial);
        var goblinSkill2 = CreateSkill("Goblin Strike", TestSkillType.Martial);
        var orcSkill = CreateSkill("Orc Fireball", TestSkillType.Magical);
        var unassignedSkill = CreateSkill("Healing Light", TestSkillType.Support);

        goblinSkill1.Monster = goblin;
        goblinSkill2.Monster = goblin;
        orcSkill.Monster = orc;

        _skillDao.AddSkill(goblinSkill1);
        _skillDao.AddSkill(goblinSkill2);
        _skillDao.AddSkill(orcSkill);
        _skillDao.AddSkill(unassignedSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByMonster(goblin);

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { goblinSkill1.Id, goblinSkill2.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetSkillsByMonster_ReturnsEmptyListWhenNoSkillsAreAssociated()
    {
        // Arrange
        var goblin = CreateMonster(
            "Goblin",
            ThreatLevel.Medium,
            DamageType.Martial,
            TestMonsterType.Elite);

        var orcSkill = CreateSkill("Orc Fireball", TestSkillType.Magical);
        var unassignedSkill = CreateSkill("Healing Light", TestSkillType.Support);

        orcSkill.Monster = CreateMonster(
            "Orc",
            ThreatLevel.Low,
            DamageType.Magical,
            TestMonsterType.Common);

        _skillDao.AddSkill(orcSkill);
        _skillDao.AddSkill(unassignedSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetSkillsByMonster(goblin);

        // Assert
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetUnassignedSkills_ReturnsOnlySkillsWithoutAssociations()
    {
        // Arrange
        var unassigned1 = CreateSkill("Unassigned Martial", TestSkillType.Martial);
        var unassigned2 = CreateSkill("Unassigned Magic", TestSkillType.Magical);

        var archetype = CreateArchetype(
            "Warrior",
            ArchetypeType.Martial);

        var monster = CreateMonster(
            "Goblin",
            ThreatLevel.Low,
            DamageType.Martial,
            TestMonsterType.Common);

        var archetypeSkill = CreateSkill("Archetype Skill", TestSkillType.Martial);
        var monsterSkill = CreateSkill("Monster Skill", TestSkillType.Magical);

        archetypeSkill.Archetype = archetype;
        monsterSkill.Monster = monster;

        _skillDao.AddSkill(unassigned1);
        _skillDao.AddSkill(unassigned2);
        _skillDao.AddSkill(archetypeSkill);
        _skillDao.AddSkill(monsterSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetUnassignedSkills();

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { unassigned1.Id, unassigned2.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetUnassignedSkills_ReturnsEmptyListWhenAllSkillsAreAssigned()
    {
        // Arrange
        var archetype = CreateArchetype(
            "Warrior",
            ArchetypeType.Magical);

        var monster = CreateMonster(
            "Goblin",
            ThreatLevel.Low,
            DamageType.Martial,
            TestMonsterType.Common);

        var archetypeSkill = CreateSkill("Archetype Skill", TestSkillType.Martial);
        var monsterSkill = CreateSkill("Monster Skill", TestSkillType.Magical);

        archetypeSkill.Archetype = archetype;
        monsterSkill.Monster = monster;

        _skillDao.AddSkill(archetypeSkill);
        _skillDao.AddSkill(monsterSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetUnassignedSkills();

        // Assert
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetUnassignedNonCoreSkills_ReturnsOnlyUnassignedNonCoreSkills()
    {
        // Arrange
        var unassignedNonCore1 = CreateSkill("Unassigned Non-Core Martial", TestSkillType.Martial);
        var unassignedNonCore2 = CreateSkill("Unassigned Non-Core Magic", TestSkillType.Magical);
        var unassignedCore = CreateSkill("Unassigned Core", TestSkillType.Support);

        var archetype = CreateArchetype(
            "Warrior",
            ArchetypeType.Magical);

        var assignedNonCore = CreateSkill("Assigned Non-Core", TestSkillType.Martial);
        var assignedCore = CreateSkill("Assigned Core", TestSkillType.Magical);

        unassignedNonCore1.IsCore = false;
        unassignedNonCore2.IsCore = false;
        unassignedCore.IsCore = true;
        assignedNonCore.IsCore = false;
        assignedCore.IsCore = true;

        assignedNonCore.Archetype = archetype;
        assignedCore.Archetype = archetype;

        _skillDao.AddSkill(unassignedNonCore1);
        _skillDao.AddSkill(unassignedNonCore2);
        _skillDao.AddSkill(unassignedCore);
        _skillDao.AddSkill(assignedNonCore);
        _skillDao.AddSkill(assignedCore);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetUnassignedNonCoreSkills();

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { unassignedNonCore1.Id, unassignedNonCore2.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetUnassignedNonCoreSkills_ReturnsEmptyListWhenNoSkillsMatch()
    {
        // Arrange
        var coreSkill = CreateSkill("Core Skill", TestSkillType.Martial);
        coreSkill.IsCore = true;

        var assignedNonCore = CreateSkill("Assigned Non-Core", TestSkillType.Magical);
        assignedNonCore.IsCore = false;
        assignedNonCore.Archetype = CreateArchetype(
            "Mage",
            ArchetypeType.Martial);

        _skillDao.AddSkill(coreSkill);
        _skillDao.AddSkill(assignedNonCore);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetUnassignedNonCoreSkills();

        // Assert
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetAllAssignedNonCoreSkills_ReturnsOnlyAssignedNonCoreSkills()
    {
        // Arrange
        var archetype = CreateArchetype(
            "Warrior",
            ArchetypeType.Martial);

        var monster = CreateMonster(
            "Goblin",
            ThreatLevel.Low,
            DamageType.Martial,
            TestMonsterType.Common);

        var assignedNonCoreArchetype = CreateSkill("Assigned Non-Core Archetype", TestSkillType.Martial);
        var assignedNonCoreMonster = CreateSkill("Assigned Non-Core Monster", TestSkillType.Magical);
        var assignedCore = CreateSkill("Assigned Core", TestSkillType.Support);
        var unassignedNonCore = CreateSkill("Unassigned Non-Core", TestSkillType.Ultimate);

        assignedNonCoreArchetype.IsCore = false;
        assignedNonCoreMonster.IsCore = false;
        assignedCore.IsCore = true;
        unassignedNonCore.IsCore = false;

        assignedNonCoreArchetype.Archetype = archetype;
        assignedNonCoreMonster.Monster = monster;
        assignedCore.Archetype = archetype;

        _skillDao.AddSkill(assignedNonCoreArchetype);
        _skillDao.AddSkill(assignedNonCoreMonster);
        _skillDao.AddSkill(assignedCore);
        _skillDao.AddSkill(unassignedNonCore);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetAllAssignedNonCoreSkills();

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { assignedNonCoreArchetype.Id, assignedNonCoreMonster.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetAllAssignedNonCoreSkills_ReturnsEmptyListWhenNoSkillsMatch()
    {
        // Arrange
        var unassignedNonCore = CreateSkill("Unassigned Non-Core", TestSkillType.Martial);
        var assignedCore = CreateSkill("Assigned Core", TestSkillType.Magical);

        unassignedNonCore.IsCore = false;
        assignedCore.IsCore = true;
        assignedCore.Archetype = CreateArchetype(
            "Mage",
            ArchetypeType.Magical);

        _skillDao.AddSkill(unassignedNonCore);
        _skillDao.AddSkill(assignedCore);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetAllAssignedNonCoreSkills();

        // Assert
        Assert.AreEqual(0, skills.Count);
    }

    [TestMethod]
    public void GetUnassignedSkillsByMaxLevel_ReturnsSkillsAtOrBelowMaxLevel()
    {
        // Arrange
        var level1 = CreateSkill("Level One", TestSkillType.Martial);
        var level2 = CreateSkill("Level Two", TestSkillType.Magical);
        var level3 = CreateSkill("Level Three", TestSkillType.Support);

        level1.RequiredLevel = 1;
        level2.RequiredLevel = 2;
        level3.RequiredLevel = 3;

        _skillDao.AddSkill(level1);
        _skillDao.AddSkill(level2);
        _skillDao.AddSkill(level3);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetUnassignedSkillsByMaxLevel(2);

        // Assert
        Assert.AreEqual(2, skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { level1.Id, level2.Id },
            skills.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void GetUnassignedSkillsByMaxLevel_ExcludesSkillsAboveMaxLevel()
    {
        // Arrange
        var eligibleSkill = CreateSkill("Eligible Skill", TestSkillType.Martial);
        var aboveMaxSkill = CreateSkill("Above Max Skill", TestSkillType.Magical);

        eligibleSkill.RequiredLevel = 2;
        aboveMaxSkill.RequiredLevel = 5;

        _skillDao.AddSkill(eligibleSkill);
        _skillDao.AddSkill(aboveMaxSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetUnassignedSkillsByMaxLevel(3);

        // Assert
        Assert.AreEqual(1, skills.Count);
        Assert.AreEqual(eligibleSkill.Id, skills[0].Id);
    }

    [TestMethod]
    public void GetUnassignedSkillsByMaxLevel_ExcludesAssignedSkills()
    {
        // Arrange
        var archetype = CreateArchetype(
            "Warrior",
            ArchetypeType.Magical);

        var monster = CreateMonster(
            "Goblin",
            ThreatLevel.Low,
            DamageType.Martial,
            TestMonsterType.Common);

        var unassignedSkill = CreateSkill("Unassigned Skill", TestSkillType.Martial);
        var archetypeSkill = CreateSkill("Archetype Skill", TestSkillType.Magical);
        var monsterSkill = CreateSkill("Monster Skill", TestSkillType.Support);

        unassignedSkill.RequiredLevel = 2;
        archetypeSkill.RequiredLevel = 2;
        monsterSkill.RequiredLevel = 2;

        archetypeSkill.Archetype = archetype;
        monsterSkill.Monster = monster;

        _skillDao.AddSkill(unassignedSkill);
        _skillDao.AddSkill(archetypeSkill);
        _skillDao.AddSkill(monsterSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetUnassignedSkillsByMaxLevel(3);

        // Assert
        Assert.AreEqual(1, skills.Count);
        Assert.AreEqual(unassignedSkill.Id, skills[0].Id);
    }

    [TestMethod]
    public void GetUnassignedSkillsByMaxLevel_ReturnsEmptyListWhenNoSkillsMatch()
    {
        // Arrange
        var aboveMaxSkill = CreateSkill("Above Max Skill", TestSkillType.Martial);
        var assignedSkill = CreateSkill("Assigned Skill", TestSkillType.Magical);

        aboveMaxSkill.RequiredLevel = 5;
        assignedSkill.RequiredLevel = 2;
        assignedSkill.Archetype = CreateArchetype(
            "Mage",
            ArchetypeType.Martial);

        _skillDao.AddSkill(aboveMaxSkill);
        _skillDao.AddSkill(assignedSkill);

        // Act
        _context.ChangeTracker.Clear();
        var skills = _skillDao.GetUnassignedSkillsByMaxLevel(3);

        // Assert
        Assert.AreEqual(0, skills.Count);
    }


    private static void AssertSkillMatches(Skill originalSkill, Skill persistedSkill)
    {
        Assert.AreEqual(originalSkill.GetType(), persistedSkill.GetType());
        Assert.AreEqual(originalSkill.Name, persistedSkill.Name);
        Assert.AreEqual(originalSkill.Description, persistedSkill.Description);
        Assert.AreEqual(originalSkill.IsCore, persistedSkill.IsCore);
        Assert.AreEqual(originalSkill.RequiredLevel, persistedSkill.RequiredLevel);
        Assert.AreEqual(originalSkill.Cost, persistedSkill.Cost);
        Assert.AreEqual(originalSkill.Power, persistedSkill.Power);
        Assert.AreEqual(originalSkill.Cooldown, persistedSkill.Cooldown);
        Assert.AreEqual(originalSkill.TargetType, persistedSkill.TargetType);
        Assert.AreEqual(originalSkill.SkillCategory, persistedSkill.SkillCategory);

        switch (originalSkill)
        {
            case DamageSkill originalDamage when
                persistedSkill is DamageSkill persistedDamage:
                Assert.AreEqual(originalDamage.DamageType, persistedDamage.DamageType);
                break;
            case SupportSkill originalSupport when
                persistedSkill is SupportSkill persistedSupport:
                Assert.AreEqual(originalSupport.Duration, persistedSupport.Duration);
                Assert.AreEqual(originalSupport.StatAffected, persistedSupport.StatAffected);
                Assert.AreEqual(originalSupport.SupportEffect, persistedSupport.SupportEffect);
                break;
        }
    }
}