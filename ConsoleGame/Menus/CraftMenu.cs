using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using static ConsoleGame.Helpers.AdventureEnums;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Services.Interfaces;
using ConsoleGameEntities.Exceptions.Recipes;
using ConsoleGameEntities.Exceptions.Runes;
using static ConsoleGame.Models.Results.Crafting.CraftingResults;

namespace ConsoleGame.Menus;

public class CraftMenu(IOutputManager outputManager, IInputManager inputManager, IRecipeDao recipeDao, 
    IRuneDao runeDao, IIngredientDao ingredientDao, ICraftingService craftingService, RecipeMenu recipeMenu)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly IRecipeDao _recipeDao = recipeDao;
    private readonly IRuneDao _runeDao = runeDao;
    private readonly IIngredientDao _ingredientDao = ingredientDao;
    private readonly ICraftingService _craftingService = craftingService;
    private readonly RecipeMenu _recipeMenu = recipeMenu;

    public void CraftingTable(int playerId)
    {
        while (true)
        {
            var options = GetCraftingOptions();

            var index = 1;
            foreach (var option in options)
            {
                _outputManager.WriteLine($"{index}. {option.Key}", ConsoleColor.Yellow);
                index++;
            }

            var choice = options.ElementAt(_inputManager.ReadInt("\tSelect an option: ", options.Count) - 1);

            switch (choice.Value)
            {
                case CraftingOptions.CraftRune:
                    CraftRune(playerId);
                    break;
                case CraftingOptions.FuseRunes:
                    FuseRunes(playerId);
                    break;
                case CraftingOptions.DestroyRune:
                    DestroyRune(playerId);
                    break;
                case CraftingOptions.TransmuteEssence:
                    TransmuteEssence(playerId);
                    break;
                case CraftingOptions.ViewRecipes:
                    _recipeMenu.RecipeMainMenu(playerId);
                    break;
                case CraftingOptions.Exit:
                    _outputManager.WriteLine("Leaving the crafting table.", ConsoleColor.Green);
                    return;
            }
        }
    }

    private void CraftRune(int playerId)
    {
        var craftableRecipes = _recipeDao.GetCraftableRecipesByPlayerId(playerId);

        if (craftableRecipes.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no recipes available to craft.\n", ConsoleColor.Red);
            return;
        }

        var recipe = _inputManager.Selector(craftableRecipes, r => r.Name,
            "Select a rune to craft:", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));

        if (recipe == null)
        {
            _outputManager.WriteLine("\nNo rune selected.\n", ConsoleColor.Red);
            return;
        }

        try
        {
            var result = _craftingService.CraftRune(playerId, recipe.Id);
            DisplayCraftRuneResult(result);
        }
        catch (RecipeException ex)
        {
            _outputManager.WriteLine($"\nError crafting rune: {ex.Message}\n", ConsoleColor.Red);
        }
    }

    private void FuseRunes(int playerId)
    {
        var fuseableRunes = _runeDao.GetFuseableRunesByPlayerId(playerId);

        if (fuseableRunes.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no runes available to fuse.\n", ConsoleColor.Red);
            return;
        }

        var rune = _inputManager.Selector(fuseableRunes, ColorfulToStringHelper.RuneToString,
            "Select a rune to fuse:", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));

        if (rune == null)
        {
            _outputManager.WriteLine("\nNo rune selected for fusion.\n", ConsoleColor.Red);
            return;
        }

        try
        {
            var result = _craftingService.FuseRune(playerId, rune.RuneId, rune.Tier);
            DisplayFuseRuneResult(result);
        }
        catch (RuneException ex)
        {
            _outputManager.WriteLine($"\nError fusing rune: {ex.Message}\n", ConsoleColor.Red);
        }
    }

    private void DestroyRune(int playerId)
    {
        var destroyableRunes = _runeDao.GetDestroyableRunesByPlayerId(playerId);

        if (destroyableRunes.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no runes available to destroy.\n", ConsoleColor.Red);
            return;
        }

        var rune = _inputManager.Selector(
            destroyableRunes,
            ColorfulToStringHelper.RuneToString,
            "Select a rune to destroy:",
            r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));

        if (rune == null)
        {
            _outputManager.WriteLine("\nNo rune selected for destruction.\n", ConsoleColor.Red);
            return;
        }

        var confirm = _inputManager.ReadString(
            $"\nAre you sure you want to destroy {rune.Rune.Name} (Tier {{rune.Tier}})? This action cannot be undone. (y/n): ",
            ["y", "n"]);

        if (!confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            _outputManager.WriteLine("\nRune destruction cancelled.\n", ConsoleColor.Red);
            return;
        }

        try
        {
            var result = _craftingService.DestroyRune(playerId, rune.RuneId, rune.Tier);
            DisplayDestroyRuneResult(result);
        }
        catch (RuneException ex)
        {
            _outputManager.WriteLine($"\nError destroying rune: {ex.Message}\n", ConsoleColor.Red);
        }
        catch (RecipeException ex)
        {
            _outputManager.WriteLine($"\nError destroying rune: {ex.Message}\n", ConsoleColor.Red);
        }
    }

    private void TransmuteEssence(int playerId)
    {
        var transmutableEssence = _ingredientDao.GetTransmutableEssencesByPlayerId(playerId);

        if (transmutableEssence.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no essence available to transmute.\n", ConsoleColor.Red);
            return;
        }

        var oldEssence = _inputManager.Selector(
            transmutableEssence,
            e => e.Ingredient.Name,
            "Select an essence to transmute:");

        if (oldEssence == null)
        {
            _outputManager.WriteLine("\nNo essence selected for transmutation.\n", ConsoleColor.Red);
            return;
        }

        try
        {
            var result = _craftingService.TransmuteEssence(playerId, oldEssence.IngredientId);
            DisplayTransmuteEssenceResult(result);
        }
        catch (IngredientException ex)
        {
            _outputManager.WriteLine(
                $"\nError transmuting essence: {ex.Message}\n",
                ConsoleColor.Red);
        }
    }
    
    private static Dictionary<string, CraftingOptions> GetCraftingOptions()
    {
        var options = new Dictionary<string, CraftingOptions>();

        AddMenuOption(options, "Craft Runes", CraftingOptions.CraftRune);
        AddMenuOption(options, "Fuse Runes", CraftingOptions.FuseRunes);
        AddMenuOption(options, "Destroy Rune", CraftingOptions.DestroyRune);
        AddMenuOption(options, "Transmute Essence", CraftingOptions.TransmuteEssence);
        AddMenuOption(options, "View Recipes", CraftingOptions.ViewRecipes);

        AddMenuOption(options, "Exit", CraftingOptions.Exit);

        return options;
    }
    
    private static void AddMenuOption<T>(Dictionary<string, T> options, string label, T option) where T : Enum
    {
        options.TryAdd(label, option);
    }
    
    private void DisplayCraftRuneResult(CraftRuneResult result)
    {
        _outputManager.WriteLine(
            $"\nSuccessfully crafted {result.CraftedRune.Name} (Tier 1)!",
            ConsoleColor.Green);

        _outputManager.WriteLine("\nIngredients consumed:", ConsoleColor.Yellow);

        foreach (var ingredient in result.IngredientsConsumed)
        {
            _outputManager.WriteLine(
                $"\t{ingredient.Ingredient.Name} x{ingredient.Quantity}");
        }

        _outputManager.WriteLine("");
    }
    
    private void DisplayFuseRuneResult(FuseRuneResult result)
    {
        _outputManager.WriteLine(
            $"\nConsumed: {result.ConsumedRune.Name} (Tier {result.ConsumedTier}) x3",
            ConsoleColor.Yellow);

        _outputManager.WriteLine(
            $"Created: {result.ResultRune.Name} (Tier {result.ResultTier})",
            ColorfulToStringHelper.GetRarityColor(result.ResultRune.Rarity));

        _outputManager.WriteLine("");
    }
    
    private void DisplayDestroyRuneResult(DestroyRuneResult result)
    {
        _outputManager.WriteLine(
            $"\nSuccessfully destroyed {result.DestroyedRune.Name} (Tier {result.DestroyedTier})!",
            ConsoleColor.Green);

        _outputManager.WriteLine("\nIngredients recovered:", ConsoleColor.Yellow);

        foreach (var ingredient in result.IngredientsReturned)
        {
            _outputManager.WriteLine(
                $"\t{ingredient.Ingredient.Name} x{ingredient.Quantity}");
        }

        _outputManager.WriteLine("");
    }
    
    private void DisplayTransmuteEssenceResult(TransmuteEssenceResult result)
    {
        _outputManager.WriteLine(
            $"\nSuccessfully transmuted {result.ConsumedEssence.Name}!",
            ConsoleColor.Green);
    
        _outputManager.WriteLine(
            $"\nConsumed: {result.ConsumedEssence.Name} x5",
            ConsoleColor.Yellow);
    
        _outputManager.WriteLine(
            $"Produced: {result.ProducedEssence.Name} x1",
            ConsoleColor.Green);
    
        _outputManager.WriteLine("");
    }
}