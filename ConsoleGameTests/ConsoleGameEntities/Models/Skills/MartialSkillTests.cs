using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Skills;

[TestClass]
public class MartialSkillTests
{
    private static MartialSkill GetBasicMartialSkill(TargetType targetType = TargetType.SingleEnemy)
    {
        return new MartialSkill
        {
            RequiredLevel = 1,
            Cooldown = 3,
            ElapsedTime = 10,
            Power = 10,
            Cost = 2,
            TargetType = targetType,
            DamageType = DamageType.Martial
        };
    }

    [TestMethod]
    public void Activate_Throws_WhenOnCooldown()
    {
        var skill = GetBasicMartialSkill();
        skill.ElapsedTime = 0;

        var caster = new Player { Level = 2 };

        Assert.ThrowsException<SkillCooldownException>(() => skill.Activate(caster, new Monster()));
    }

    [TestMethod]
    public void Activate_Throws_WhenCasterLevelTooLow()
    {
        var skill = GetBasicMartialSkill();
        var caster = new Player { Level = 0 };

        Assert.ThrowsException<InvalidSkillLevelException>(() => skill.Activate(caster, new Monster()));
    }

    [TestMethod]
    public void Activate_Throws_WhenTargetsAreMissing()
    {
        var skill = GetBasicMartialSkill(TargetType.AllEnemies);
        var caster = new Player { Level = 5 };

        Assert.ThrowsException<InvalidTargetException>(() => skill.Activate(caster, null, null));
    }

    [TestMethod]
    public void Activate_Throws_WhenNotEnoughResource()
    {
        var archetypeMock = new Mock<Archetype>();
        archetypeMock.Setup(a => a.UseResource(It.IsAny<int>())).Throws(new InvalidOperationException());

        var skill = GetBasicMartialSkill();
        var caster = new Player
        {
            Level = 5,
            Archetype = archetypeMock.Object
        };

        Assert.ThrowsException<SkillResourceException>(() => skill.Activate(caster, new Monster()));
    }

    [TestMethod]
    public void Activate_DealsDamageToSingleEnemy()
    {
        var skill = GetBasicMartialSkill();
        var monster = new Monster();
        var caster = new Player
        {
            Level = 5,
            Archetype = new Archetype { CurrentResource = 10, MaxResource = 10 }
        };

        monster.CurrentHealth = 50;
        skill.Activate(caster, monster);

        Assert.IsTrue(monster.CurrentHealth < 50);
    }

    [TestMethod]
    public void Activate_DealsDamageToAllEnemies()
    {
        var skill = GetBasicMartialSkill(TargetType.AllEnemies);
        var monsters = new List<ITargetable>
        {
            new Monster { CurrentHealth = 40 },
            new Monster { CurrentHealth = 40 }
        };
        var caster = new Player
        {
            Level = 5,
            Archetype = new Archetype { CurrentResource = 10, MaxResource = 10 }
        };

        skill.Activate(caster, null, monsters);

        foreach (var monster in monsters.Cast<Monster>())
            Assert.IsTrue(monster.CurrentHealth < 40);
    }

    [TestMethod]
    public void Activate_ThrowsMonsterDeathException_IfOneMonsterDies()
    {
        var skill = GetBasicMartialSkill(TargetType.AllEnemies);
        var monsters = new List<ITargetable>
        {
            new Monster { CurrentHealth = 5 },
            new Monster { CurrentHealth = 100 }
        };
        var caster = new Player
        {
            Level = 5,
            Archetype = new Archetype { CurrentResource = 10, MaxResource = 10 }
        };

        Assert.ThrowsException<MonsterDeathException>(() => skill.Activate(caster, null, monsters));
    }

    [TestMethod]
    public void Activate_ResetsCooldownAfterUse()
    {
        var skill = GetBasicMartialSkill();
        var monster = new Monster { CurrentHealth = 100 };
        var caster = new Player
        {
            Level = 5,
            Archetype = new Archetype { CurrentResource = 10, MaxResource = 10 }
        };

        skill.Activate(caster, monster);
        Assert.AreEqual(0, skill.ElapsedTime);
    }

    [TestMethod]
    public void Activate_MonsterCaster_AttacksPlayer()
    {
        var skill = GetBasicMartialSkill();
        var player = new Player()
        {
            CurrentHealth = 100,
            Archetype = new Archetype() { DefenseBonus = 0 }
        };
        player.CurrentHealth = 100;

        var monster = new Monster
        {
            Level = 5
        };

        skill.Activate(monster, player);

        Assert.IsTrue(player.CurrentHealth < 100);
    }
}
