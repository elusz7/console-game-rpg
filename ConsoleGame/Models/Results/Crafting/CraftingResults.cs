using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.Models.Results.Crafting;

public static class CraftingResults
{
    public record CraftRuneResult(
        Rune CraftedRune,
        IReadOnlyList<IngredientChange> IngredientsConsumed);

    public record FuseRuneResult(
        Rune ResultRune,
        int ResultTier,
        Rune ConsumedRune,
        int ConsumedTier);

    public record DestroyRuneResult(
        Rune DestroyedRune,
        int DestroyedTier,
        IReadOnlyList<IngredientChange> IngredientsReturned);

    public record TransmuteEssenceResult(
        Ingredient ConsumedEssence,
        Ingredient ProducedEssence);

    public record IngredientChange(
        Ingredient Ingredient,
        int Quantity);
}