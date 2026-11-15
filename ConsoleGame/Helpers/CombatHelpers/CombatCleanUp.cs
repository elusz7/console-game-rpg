using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGame.Helpers.CombatHelpers;

public static class CombatCleanUp
{
    public static void CleanupDeadMonster(Monster monster, Player player, IEnumerable<Monster> monsters)
    {
        monster.ActiveEffectManager.ClearAll();

        player.ActiveEffectManager
            .ClearSupportSkillEffectsBySourceCaster(monster);

        foreach (var otherMonster in monsters)
        {
            if (ReferenceEquals(otherMonster, monster))
                continue;

            otherMonster.ActiveEffectManager
                .ClearSupportSkillEffectsBySourceCaster(monster);
        }
    }
}