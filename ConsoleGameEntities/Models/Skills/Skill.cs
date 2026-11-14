using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Exceptions.Skills;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.IdentityModel.Protocols.Configuration;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public abstract class Skill : ISkill
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsCore { get; set; }
    public int RequiredLevel { get; set; }
    public int Cost { get; set; }
    public int Power { get; set; }
    [NotMapped] public int ElapsedTime { get; set; }
    public int Cooldown { get; set; }
    [NotMapped] public bool IsOnCooldown => ElapsedTime < Cooldown;
    public TargetType TargetType { get; set; }
    public string SkillType { get; set; }
    public SkillCategory SkillCategory { get; set; }

    public int? ArchetypeId { get; set; }
    public virtual Archetype? Archetype { get; set; }
    public int? MonsterId { get; set; }
    public virtual Monster? Monster { get; set; }
    [NotMapped] protected virtual bool AllowsAllyTargets => false;

    public virtual void InitializeSkill(int level)
    {
        ElapsedTime = Cooldown + 1;
    }

    public abstract SkillResolutionResult Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null);

    protected void ValidateCasterLevel(ITargetable caster)
    {
        if (caster is Player p)
        {
            if (p.Level < RequiredLevel)
                throw new InvalidSkillLevelException(
                    $"{p.Name} does not meet the required level ({RequiredLevel}) for '{Name}'.");
        }
        else if (caster is Monster m && m.Level < RequiredLevel)
            throw new InvalidSkillLevelException(
                $"{m.Name} does not meet the required level ({RequiredLevel}) for '{Name}'.");
    }

    protected void ValidateTargets(
        ITargetable caster,
        ITargetable? singleEnemy,
        List<ITargetable>? multipleEnemies)
    {
        if (!Enum.IsDefined(TargetType)) //has to be a defined target type
            throw new InvalidTargetException("Unsupported skill target type.");

        if (!(caster is Player) && !(caster is Monster)) //has to be a player or monster
            throw new InvalidTargetException("Unsupported skill caster.");

        switch (TargetType)
        {
            case TargetType.SingleEnemy:
                if (singleEnemy == null)
                    throw new InvalidTargetException("Single enemy target not provided."); //has to provide a single enemy for single enemy targets

                ValidateEnemyTarget(caster, singleEnemy);
                break;

            case TargetType.AllEnemies:
                if (multipleEnemies == null || multipleEnemies.Count == 0)
                    throw new InvalidTargetException("Multiple enemies not provided."); //multiple enemies have to be provided

                foreach (var target in multipleEnemies)
                {
                    if (target == null)
                        throw new InvalidTargetException("Invalid enemy target."); //all enemies have to be valid
                    if (!AllowsAllyTargets)
                        ValidateEnemyTarget(caster, target);
                }
                break;

            case TargetType.Self:
                break;

            default:
                throw new InvalidTargetException("Unsupported skill target type.");
        }
    }

    private void ValidateEnemyTarget(ITargetable caster, ITargetable target)
    {
        bool isValid = caster switch
        {
            Player _ => target is Monster, //a player has to target a monster
            Monster _ => target is Player, //a monster has to target a player
            _ => false
        };

        if (!isValid)
            throw new InvalidTargetException("Invalid enemy target.");
    }

    protected void ConsumePlayerResources(Player player)
    {
        try
        {
            player.Archetype.UseResource(Cost);
        }
        catch (InvalidOperationException ex)
        {
            throw new SkillResourceException(ex.Message);
        }
    }
    
    protected void RefundPlayerResources(Player player)
    {
        player.Archetype.RefundResource(Cost);
    }

    public virtual void UpdateElapsedTime()
    {
        ElapsedTime++;
    }

    public virtual void Reset()
    {
        ElapsedTime = 0;
    }

    public abstract Skill Clone();

    protected void CopyTo(Skill clone)
    {
        clone.Name = Name;
        clone.Description = Description;
        clone.IsCore = IsCore;
        clone.RequiredLevel = RequiredLevel;
        clone.Cost = Cost;
        clone.Power = Power;
        clone.Cooldown = Cooldown;
        clone.TargetType = TargetType;
        clone.SkillType = SkillType;
        clone.SkillCategory = SkillCategory;
    }
}