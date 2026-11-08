namespace ConsoleGame.Services.Interfaces;

public interface ICraftingService
{
    void CraftRune(int playerId, int recipeId);
    void FuseRune(int playerId, int runeId, int tier);
    void DestroyRune(int playerId, int runeId, int tier);
    void TransmuteEssence(int playerId, int essenceId);
}