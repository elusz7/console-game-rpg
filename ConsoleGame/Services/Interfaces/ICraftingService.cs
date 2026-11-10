using static ConsoleGame.Models.Results.Crafting.CraftingResults;

namespace ConsoleGame.Services.Interfaces;

public interface ICraftingService
{
    CraftRuneResult CraftRune(int playerId, int recipeId);
    FuseRuneResult FuseRune(int playerId, int runeId, int tier);
    DestroyRuneResult DestroyRune(int playerId, int runeId, int tier);
    TransmuteEssenceResult TransmuteEssence(int playerId, int essenceId);
}