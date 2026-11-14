using ConsoleGameEntities.Interfaces;

namespace ConsoleGameEntities.Models.Combat;

public record SkillResolutionResult(
    ISkill Skill,
    IReadOnlyList<AttackResolutionResult> AttackResults);