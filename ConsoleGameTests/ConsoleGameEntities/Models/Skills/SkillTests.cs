using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Skills;

using System;
using System.Collections.Generic;
using global::ConsoleGameEntities.Exceptions;
using global::ConsoleGameEntities.Interfaces.Attributes;
using global::ConsoleGameEntities.Models.Entities;
using global::ConsoleGameEntities.Models.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static global::ConsoleGameEntities.Models.Entities.ModelEnums;

[TestClass]
public class SkillTests
{
    private class TestSkill : Skill
    {
        public bool EffectApplied { get; private set; }

        public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
        {
            EffectApplied = true;
            Reset();
        }
    }

    [TestMethod]
    public void Activate_Throws_WhenSkillIsOnCooldown()
    {
        var skill = new Skill { Cooldown = 5, ElapsedTime = 0 };
        var caster = new Player();

        var ex = Assert.ThrowsException<SkillCooldownException>(() => skill.Activate(caster));
        StringAssert.Contains(ex.Message, "on cooldown");
    }
    
    [TestMethod]
    public void Activate_Throws_WhenCasterLevelTooLow()
    {
        var caster = new Player() { Level = 3 };

        var skill = new Skill { RequiredLevel = 5, ElapsedTime = 10 };

        var ex = Assert.ThrowsException<InvalidSkillLevelException>(() => skill.Activate(caster));
        StringAssert.Contains(ex.Message, "does not meet the required level");
    }
    
    [TestMethod]
    public void Activate_Throws_WhenTargetMissing_ForSingleEnemy()
    {
        var player = new Player { Level = 3 };

        var skill = new Skill { RequiredLevel = 1, TargetType = TargetType.SingleEnemy, ElapsedTime = 10 };

        var ex = Assert.ThrowsException<InvalidTargetException>(() => skill.Activate(player, null, null));
        StringAssert.Contains(ex.Message, "Single enemy target not provided");
    }

    [TestMethod]
    public void Activate_ConsumesResources_AndAddsActionItem_ForPlayer()
    {
        var archetype = new Archetype() { CurrentResource = 10, MaxResource = 12 };

        var player = new Player() { Level= 3, Archetype = archetype };

        var skill = new Skill { RequiredLevel = 1, Cost = 5, ElapsedTime = 10 };

        skill.Activate(player);

        Assert.AreEqual(5, archetype.CurrentResource);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You use ")));
    }

    [TestMethod]
    public void Activate_Throws_SkillResourceException_WhenUseResourceThrows()
    {
        var archetypeMock = new Mock<Archetype>();
        archetypeMock.Setup(a => a.UseResource(It.IsAny<int>()))
                        .Throws(new InvalidOperationException("Not enough resource"));

        var player = new Player() { Level = 3, Archetype = archetypeMock.Object };

        var skill = new Skill { RequiredLevel = 1, Cost = 5, ElapsedTime = 10 };

        var ex = Assert.ThrowsException<SkillResourceException>(() => skill.Activate(player));
        StringAssert.Contains(ex.Message, "Not enough resource");
    }

    [TestMethod]
    public void Activate_ResetsElapsedTime_AfterActivation()
    {
        var archetype = new Archetype() { CurrentResource = 10, MaxResource = 12 };
        var player = new Player() { Level = 3, Archetype = archetype };
        var skill = new TestSkill { RequiredLevel = 1, ElapsedTime = 10 };
        

        skill.Activate(player);

        Assert.AreEqual(0, skill.ElapsedTime);
        Assert.IsTrue(skill.EffectApplied);
    }

    [TestMethod]
    public void UpdateElapsedTime_IncrementsElapsedTime()
    {
        var skill = new Skill { ElapsedTime = 5 };
        skill.UpdateElapsedTime();
        Assert.AreEqual(6, skill.ElapsedTime);
    }

    [TestMethod]
    public void InitializeSkill_SetsElapsedTimeToCooldownPlusOne()
    {
        var skill = new Skill { Cooldown = 5 };
        skill.InitializeSkill(1);
        Assert.AreEqual(6, skill.ElapsedTime);
    }

    [TestMethod]
    public void IsOnCooldown_ReturnsTrue_WhenElapsedTimeLessThanCooldown()
    {
        var skill = new Skill { Cooldown = 5, ElapsedTime = 3 };
        Assert.IsTrue(skill.IsOnCooldown);
    }

    [TestMethod]
    public void IsOnCooldown_ReturnsFalse_WhenElapsedTimeEqualsOrGreaterThanCooldown()
    {
        var skill = new Skill { Cooldown = 5, ElapsedTime = 5 };
        Assert.IsFalse(skill.IsOnCooldown);

        skill.ElapsedTime = 10;
        Assert.IsFalse(skill.IsOnCooldown);
    }
}
