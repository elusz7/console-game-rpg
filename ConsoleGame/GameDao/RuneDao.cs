using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao;

public class RuneDao(GameContext context) : IRuneDao
{
    private readonly GameContext _context = context;
    public Rune FuseRune(Rune rune)
    {
        var newRune = _context.Runes.FirstOrDefault(r => r.Id == rune.Id) ?? throw new InvalidOperationException("Rune not found in the database.");
        newRune.Tier = rune.Tier + 1;

        if (newRune.Tier >= 4)
        {
            var nextRarity = newRune.Rarity switch
            {
                RarityLevel.Common => RarityLevel.Uncommon,
                RarityLevel.Uncommon => RarityLevel.Rare,
                RarityLevel.Rare => RarityLevel.Epic,
                RarityLevel.Epic => RarityLevel.Legendary,
                RarityLevel.Legendary => RarityLevel.Mythic,
                _ => throw new InvalidOperationException("Cannot evolve rune beyond Mythic rarity.")
            };

            var evolvedRune = _context.Runes.FirstOrDefault(r =>
                r.Element == newRune.Element &&
                r.Rarity == nextRarity &&
                r.RuneType == newRune.RuneType) ?? throw new InvalidOperationException("No evolved rune found in the database.");

            evolvedRune.Tier = 1; // reset tier on evolution
            return evolvedRune;
        }

        return newRune;
    }

}
