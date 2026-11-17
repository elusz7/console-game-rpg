
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class MonsterSkillSelectorTests
{
    private MonsterSkillSelector _selector = null!;

    [TestInitialize]
    public void Setup()
    {
        _selector = new MonsterSkillSelector();
    }

    private static Monster CreateMonster(
        DamageType damageType = DamageType.Martial,
        int level = 10,
        params Skill[] skills)
    {
        return new Monster
        {
            Level = level,
            DamageType = damageType,
            Skills = [.. skills]
        };
    }

    private static MartialSkill Martial(
        int power,
        int requiredLevel = 1,
        TargetType target = TargetType.SingleEnemy)
    {
        return new MartialSkill
        {
            Power = power,
            RequiredLevel = requiredLevel,
            TargetType = target,
            Cooldown = 3,
            ElapsedTime = 3
        };
    }

    private static MagicSkill Magic(
        int power,
        int requiredLevel = 1,
        TargetType target = TargetType.SingleEnemy)
    {
        return new MagicSkill
        {
            Power = power,
            RequiredLevel = requiredLevel,
            TargetType = target,
            Cooldown = 3,
            ElapsedTime = 3
        };
    }

    private static SupportSkill Support(
        int power,
        StatType stat,
        TargetType target,
        int requiredLevel = 1)
    {
        return new SupportSkill
        {
            Power = power,
            RequiredLevel = requiredLevel,
            StatAffected = stat,
            TargetType = target,
            SupportEffect = target == TargetType.Self
                ? SupportEffectType.Boost
                : SupportEffectType.Reduce,
            Cooldown = 3,
            ElapsedTime = 3
        };
    }

    private static UltimateSkill Ultimate(
        int power,
        int requiredLevel = 1)
    {
        return new UltimateSkill
        {
            Power = power,
            RequiredLevel = requiredLevel,
            TargetType = TargetType.SingleEnemy,
            Cooldown = 5,
            ElapsedTime = 5
        };
    }

    private static BossSkill Boss(
        int power,
        int requiredLevel = 1)
    {
        return new BossSkill
        {
            Power = power,
            RequiredLevel = requiredLevel,
            TargetType = TargetType.SingleEnemy,
            Cooldown = 5,
            ElapsedTime = 5
        };
    }

    // -------------------------------------------------------
    // GetHighestDamageSkill (8)
    // -------------------------------------------------------

    [TestMethod]
    public void GetHighestDamageSkill_MartialMonster_SelectsHighestPowerMartialSkill()
    {
        var weak = Martial(10);
        var strong = Martial(30);
        var magic = Magic(100);
        var monster = CreateMonster(DamageType.Martial, 10, weak, strong, magic);

        var result = _selector.GetHighestDamageSkill(monster);

        Assert.AreSame(strong, result);
    }

    [TestMethod]
    public void GetHighestDamageSkill_MagicalMonster_SelectsHighestPowerMagicSkill()
    {
        var weak = Magic(10);
        var strong = Magic(30);
        var martial = Martial(100);
        var monster = CreateMonster(DamageType.Magical, 10, weak, strong, martial);

        var result = _selector.GetHighestDamageSkill(monster);

        Assert.AreSame(strong, result);
    }

    [TestMethod]
    public void GetHighestDamageSkill_HybridMonster_SelectsHighestPowerFromBothTypes()
    {
        var martial = Martial(20);
        var magic = Magic(40);
        var monster = CreateMonster(DamageType.Hybrid, 10, martial, magic);

        var result = _selector.GetHighestDamageSkill(monster);

        Assert.AreSame(magic, result);
    }

    [TestMethod]
    public void GetHighestDamageSkill_SkillAboveMonsterLevel_IsExcluded()
    {
        var available = Martial(10);
        var tooHigh = Martial(100, requiredLevel: 11);
        var monster = CreateMonster(DamageType.Martial, 10, available, tooHigh);

        var result = _selector.GetHighestDamageSkill(monster);

        Assert.AreSame(available, result);
    }

    [TestMethod]
    public void GetHighestDamageSkill_SkillOnCooldown_IsExcluded()
    {
        var available = Martial(10);
        var onCooldown = Martial(100);
        onCooldown.ElapsedTime = 1;

        var monster = CreateMonster(DamageType.Martial, 10, available, onCooldown);

        var result = _selector.GetHighestDamageSkill(monster);

        Assert.AreSame(available, result);
    }

    [TestMethod]
    public void GetHighestDamageSkill_AllEnemiesSkill_IsExcluded()
    {
        var singleTarget = Martial(10);
        var allEnemies = Martial(100, target: TargetType.AllEnemies);
        var monster = CreateMonster(DamageType.Martial, 10, singleTarget, allEnemies);

        var result = _selector.GetHighestDamageSkill(monster);

        Assert.AreSame(singleTarget, result);
    }

    [TestMethod]
    public void GetHighestDamageSkill_OnlySupportAndUltimateSkills_ReturnsNull()
    {
        var support = Support(20, StatType.Health, TargetType.Self);
        var ultimate = Ultimate(100);
        var monster = CreateMonster(DamageType.Martial, 10, support, ultimate);

        var result = _selector.GetHighestDamageSkill(monster);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetHighestDamageSkill_NoEligibleSkills_ReturnsNull()
    {
        var monster = CreateMonster();

        var result = _selector.GetHighestDamageSkill(monster);

        Assert.IsNull(result);
    }

    // -------------------------------------------------------
    // GetStrongestBossSkill (4)
    // -------------------------------------------------------

    [TestMethod]
    public void GetStrongestBossSkill_MultipleReadySkills_ReturnsHighestPower()
    {
        var weak = Boss(10);
        var strong = Boss(50);
        var monster = CreateMonster(DamageType.Martial, 10, weak, strong);

        var result = _selector.GetStrongestBossSkill(monster);

        Assert.AreSame(strong, result);
    }

    [TestMethod]
    public void GetStrongestBossSkill_StrongerSkillNotReady_SelectsWeakerReadySkill()
    {
        var ready = Boss(10);
        var notReady = Boss(100);
        notReady.ElapsedTime = 0;

        var monster = CreateMonster(DamageType.Martial, 10, ready, notReady);

        var result = _selector.GetStrongestBossSkill(monster);

        Assert.AreSame(ready, result);
    }

    [TestMethod]
    public void GetStrongestBossSkill_SkillAboveMonsterLevel_IsExcluded()
    {
        var available = Boss(10);
        var tooHigh = Boss(100, requiredLevel: 11);
        var monster = CreateMonster(DamageType.Martial, 10, available, tooHigh);

        var result = _selector.GetStrongestBossSkill(monster);

        Assert.AreSame(available, result);
    }

    [TestMethod]
    public void GetStrongestBossSkill_NoEligibleBossSkills_ReturnsNull()
    {
        var monster = CreateMonster(DamageType.Martial, 10, Martial(10));

        var result = _selector.GetStrongestBossSkill(monster);

        Assert.IsNull(result);
    }

    // -------------------------------------------------------
    // GetSupportSkills (4)
    // -------------------------------------------------------

    [TestMethod]
    public void GetSupportSkills_MixedSkillTypes_ReturnsOnlySupportSkills()
    {
        var support = Support(20, StatType.Health, TargetType.Self);
        var monster = CreateMonster(
            DamageType.Martial, 10,
            Martial(10), Magic(10), support, Ultimate(50), Boss(50));

        var result = _selector.GetSupportSkills(monster);

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(support, result[0]);
    }

    [TestMethod]
    public void GetSupportSkills_SkillAboveMonsterLevel_IsExcluded()
    {
        var available = Support(10, StatType.Health, TargetType.Self);
        var tooHigh = Support(50, StatType.Health, TargetType.Self, 11);
        var monster = CreateMonster(DamageType.Martial, 10, available, tooHigh);

        var result = _selector.GetSupportSkills(monster);

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(available, result[0]);
    }

    [TestMethod]
    public void GetSupportSkills_SkillOnCooldown_IsExcluded()
    {
        var available = Support(10, StatType.Health, TargetType.Self);
        var onCooldown = Support(50, StatType.Health, TargetType.Self);
        onCooldown.ElapsedTime = 1;

        var monster = CreateMonster(DamageType.Martial, 10, available, onCooldown);

        var result = _selector.GetSupportSkills(monster);

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(available, result[0]);
    }

    [TestMethod]
    public void GetSupportSkills_NoEligibleSupportSkills_ReturnsEmptyList()
    {
        var monster = CreateMonster(DamageType.Martial, 10, Martial(10));

        var result = _selector.GetSupportSkills(monster);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    // -------------------------------------------------------
    // GetDebuffSkill (4)
    // -------------------------------------------------------

    [TestMethod]
    public void GetDebuffSkill_MultipleDebuffs_ReturnsHighestPower()
    {
        var weak = Support(10, StatType.Attack, TargetType.SingleEnemy);
        var strong = Support(30, StatType.Defense, TargetType.SingleEnemy);
        var monster = CreateMonster(DamageType.Martial, 10, weak, strong);

        var result = _selector.GetDebuffSkill(monster);

        Assert.AreSame(strong, result);
    }

    [TestMethod]
    public void GetDebuffSkill_AffectedStatSpecified_SelectsMatchingStat()
    {
        var attack = Support(10, StatType.Attack, TargetType.SingleEnemy);
        var defense = Support(100, StatType.Defense, TargetType.SingleEnemy);
        var monster = CreateMonster(DamageType.Martial, 10, attack, defense);

        var result = _selector.GetDebuffSkill(monster, StatType.Attack);

        Assert.AreSame(attack, result);
    }

    [TestMethod]
    public void GetDebuffSkill_SelfTargetingSkill_IsExcluded()
    {
        var debuff = Support(10, StatType.Attack, TargetType.SingleEnemy);
        var self = Support(100, StatType.Attack, TargetType.Self);
        var monster = CreateMonster(DamageType.Martial, 10, debuff, self);

        var result = _selector.GetDebuffSkill(monster);

        Assert.AreSame(debuff, result);
    }

    [TestMethod]
    public void GetDebuffSkill_NoMatchingDebuffs_ReturnsNull()
    {
        var monster = CreateMonster(
            DamageType.Martial, 10,
            Support(10, StatType.Health, TargetType.Self));

        var result = _selector.GetDebuffSkill(monster);

        Assert.IsNull(result);
    }

    // -------------------------------------------------------
    // GetHealingSkill (5)
    // -------------------------------------------------------

    [TestMethod]
    public void GetHealingSkill_ExactPowerMatch_ReturnsMatchingSkill()
    {
        var weak = Support(10, StatType.Health, TargetType.Self);
        var exact = Support(30, StatType.Health, TargetType.Self);
        var strong = Support(50, StatType.Health, TargetType.Self);
        var monster = CreateMonster(DamageType.Martial, 10, weak, exact, strong);

        var result = _selector.GetHealingSkill(monster, 30);

        Assert.AreSame(exact, result);
    }

    [TestMethod]
    public void GetHealingSkill_NoExactMatch_ReturnsClosestPower()
    {
        var weak = Support(10, StatType.Health, TargetType.Self);
        var closest = Support(28, StatType.Health, TargetType.Self);
        var strong = Support(50, StatType.Health, TargetType.Self);
        var monster = CreateMonster(DamageType.Martial, 10, weak, closest, strong);

        var result = _selector.GetHealingSkill(monster, 30);

        Assert.AreSame(closest, result);
    }

    [TestMethod]
    public void GetHealingSkill_ClosestPowerBelowDesired_SelectsWeakerHeal()
    {
        var weaker = Support(25, StatType.Health, TargetType.Self);
        var stronger = Support(40, StatType.Health, TargetType.Self);
        var monster = CreateMonster(DamageType.Martial, 10, weaker, stronger);

        var result = _selector.GetHealingSkill(monster, 30);

        Assert.AreSame(weaker, result);
    }

    [TestMethod]
    public void GetHealingSkill_NonSelfTargetingHealthSkill_IsExcluded()
    {
        var selfHeal = Support(10, StatType.Health, TargetType.Self);
        var enemyTarget = Support(30, StatType.Health, TargetType.SingleEnemy);
        var monster = CreateMonster(DamageType.Martial, 10, selfHeal, enemyTarget);

        var result = _selector.GetHealingSkill(monster, 30);

        Assert.AreSame(selfHeal, result);
    }

    [TestMethod]
    public void GetHealingSkill_NoEligibleHealingSkills_ReturnsNull()
    {
        var monster = CreateMonster(
            DamageType.Martial, 10,
            Support(30, StatType.Defense, TargetType.Self));

        var result = _selector.GetHealingSkill(monster, 30);

        Assert.IsNull(result);
    }

    // -------------------------------------------------------
    // GetBuffSkill (4)
    // -------------------------------------------------------

    [TestMethod]
    public void GetBuffSkill_MultipleBuffs_ReturnsHighestPower()
    {
        var weak = Support(10, StatType.Attack, TargetType.Self);
        var strong = Support(30, StatType.Defense, TargetType.Self);
        var monster = CreateMonster(DamageType.Martial, 10, weak, strong);

        var result = _selector.GetBuffSkill(monster);

        Assert.AreSame(strong, result);
    }

    [TestMethod]
    public void GetBuffSkill_AffectedStatSpecified_SelectsMatchingStat()
    {
        var attack = Support(10, StatType.Attack, TargetType.Self);
        var defense = Support(100, StatType.Defense, TargetType.Self);
        var monster = CreateMonster(DamageType.Martial, 10, attack, defense);

        var result = _selector.GetBuffSkill(monster, StatType.Attack);

        Assert.AreSame(attack, result);
    }

    [TestMethod]
    public void GetBuffSkill_NonSelfTargetingSkill_IsExcluded()
    {
        var buff = Support(10, StatType.Attack, TargetType.Self);
        var enemy = Support(100, StatType.Attack, TargetType.SingleEnemy);
        var monster = CreateMonster(DamageType.Martial, 10, buff, enemy);

        var result = _selector.GetBuffSkill(monster);

        Assert.AreSame(buff, result);
    }

    [TestMethod]
    public void GetBuffSkill_NoMatchingBuffs_ReturnsNull()
    {
        var monster = CreateMonster(
            DamageType.Martial, 10,
            Support(10, StatType.Attack, TargetType.SingleEnemy));

        var result = _selector.GetBuffSkill(monster);

        Assert.IsNull(result);
    }
    
    [TestMethod]
    public void GetBuffSkill_HealingSkillWithHigherPower_IsExcluded()
    {
        var buff = Support(20, StatType.Attack, TargetType.Self);
        var healing = Support(100, StatType.Health, TargetType.Self);
        var monster = CreateMonster(DamageType.Martial, 10, buff, healing);

        var result = _selector.GetBuffSkill(monster);

        Assert.AreSame(buff, result);
    }

    // -------------------------------------------------------
    // GetUltimateSkill (3)
    // -------------------------------------------------------

    [TestMethod]
    public void GetUltimateSkill_MultipleReadySkills_ReturnsFirstReadySkill()
    {
        var first = Ultimate(10);
        var second = Ultimate(100);
        var monster = CreateMonster(DamageType.Martial, 10, first, second);

        var result = _selector.GetUltimateSkill(monster);

        Assert.AreSame(first, result);
    }

    [TestMethod]
    public void GetUltimateSkill_FirstSkillNotReady_SelectsNextReadySkill()
    {
        var notReady = Ultimate(100);
        notReady.ElapsedTime = 0;

        var ready = Ultimate(10);
        var monster = CreateMonster(DamageType.Martial, 10, notReady, ready);

        var result = _selector.GetUltimateSkill(monster);

        Assert.AreSame(ready, result);
    }

    [TestMethod]
    public void GetUltimateSkill_SkillAboveMonsterLevel_IsExcluded()
    {
        var tooHigh = Ultimate(100, requiredLevel: 11);
        var available = Ultimate(10);
        var monster = CreateMonster(DamageType.Martial, 10, tooHigh, available);

        var result = _selector.GetUltimateSkill(monster);

        Assert.AreSame(available, result);
    }
}
