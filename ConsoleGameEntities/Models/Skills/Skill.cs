using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public class Skill : ISkill
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int RequiredLevel { get; set; }
    public int Cost { get; set; }
    public int Power { get; set; }
    [NotMapped]
    public int ElapsedTime { get; set; }
    public int Cooldown { get; set; }
    [NotMapped]
    public bool IsOnCooldown => ElapsedTime < Cooldown;
    public TargetType TargetType { get; set; }
    public string SkillType { get; set; }
    public SkillCategory SkillCategory { get; set; }

    public int? ArchetypeId { get; set; }
    public virtual Archetype? Archetype { get; set; }
    public int? MonsterId { get; set; }
    public virtual Monster? Monster { get; set; }

    public virtual void InitializeSkill(int level)
    {
        ElapsedTime = Cooldown + 1;
    }
    public virtual void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException($"Skill '{Name}' is on cooldown!");

        ValidateCasterLevel(caster);
        ValidateTargets(caster, singleEnemy, multipleEnemies);

        if (caster is Player player)
        {
            ConsumePlayerResources(player);
            player.Logger.Log($"You use {Name}!");
        }
        else if (caster is Monster monster)
        {
            monster.Logger.Log($"{monster.Name} uses {Name}!");
        }

        ApplySkillEffect(caster, singleEnemy, multipleEnemies);

        Reset();
    }
    private void ValidateCasterLevel(ITargetable caster)
    {
        if (caster is Player p)
        {
            if (p.Level < RequiredLevel)
                throw new InvalidSkillLevelException($"{p.Name} does not meet the required level ({RequiredLevel}) for '{Name}'.");
        }
        else if (caster is Monster m)
        {
            if (m.Level < RequiredLevel)
                throw new InvalidSkillLevelException($"{m.Name} does not meet the required level ({RequiredLevel}) for '{Name}'.");
        }
    }
    private void ValidateTargets(ITargetable caster, ITargetable? singleEnemy, List<ITargetable>? multipleEnemies)
    {
        switch (TargetType)
        {
            case TargetType.SingleEnemy:
                if (singleEnemy == null)
                    throw new InvalidTargetException("Single enemy target not provided.");
                break;
            case TargetType.AllEnemies:
                if (multipleEnemies == null || multipleEnemies.Count == 0)
                    throw new InvalidTargetException("Multiple enemies not provided.");
                break;
            case TargetType.Self:
                break;
        }
    }
    private void ConsumePlayerResources(Player player)
    {
        if (player.Level < RequiredLevel)
            throw new InvalidSkillLevelException("Your level is too low to use this skill!");

        try
        {
            player.Archetype.UseResource(Cost);
        }
        catch (InvalidOperationException ex)
        {
            throw new SkillResourceException(ex.Message);
        }
    }
    private void ApplySkillEffect(ITargetable caster, ITargetable? singleEnemy, List<ITargetable>? multipleEnemies)
    {
        if (this is SupportSkill support)
        {
            switch (TargetType)
            {
                case TargetType.Self:
                    support.ApplyEffect(caster);
                    break;
                case TargetType.SingleEnemy:
                    support.ApplyEffect(singleEnemy);
                    break;
                case TargetType.AllEnemies:
                    foreach (var target in multipleEnemies)
                        support.ApplyEffect(target);
                    break;
            }
        }
        else if (this is DamageSkill damageSkill)
        {
            switch (TargetType)
            {
                case TargetType.SingleEnemy:
                    singleEnemy.TakeDamage(Power, damageSkill.DamageType);
                    break;
                case TargetType.AllEnemies:
                    foreach (var target in multipleEnemies)
                        target.TakeDamage(Power, damageSkill.DamageType);
                    break;
            }
        }
    }
    public virtual void UpdateElapsedTime()
    {
        ElapsedTime++;
    }
    public virtual void Reset()
    {
        ElapsedTime = 0;
    }

    public virtual Skill Clone()
    {
        return new Skill
        {
            Name = this.Name,
            Description = this.Description,
            RequiredLevel = this.RequiredLevel,
            Cost = this.Cost,
            Power = this.Power,
            Cooldown = this.Cooldown,
            TargetType = this.TargetType,
            SkillCategory = this.SkillCategory
        };
    }
}
