using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using static ConsoleGame.Helpers.AdventureEnums;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Services.Interfaces;
using ConsoleGameEntities.Exceptions.Recipes;
using ConsoleGameEntities.Exceptions.Runes;

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

        // Select rune to craft
        var recipe = _inputManager.Selector(craftableRecipes, r => r.Name, "Select a rune to craft:", 
            r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
        if (recipe == null)
        {
            _outputManager.WriteLine("\nNo rune selected.\n", ConsoleColor.Red);
            return;
        }

        // Craft Rune
        try
        {
            _craftingService.CraftRune(playerId, recipe.Id);
            _outputManager.WriteLine($"\nSuccessfully crafted {recipe.Rune.Name}!\n", ConsoleColor.Green);
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

        // Select rune to fuse
        var rune = _inputManager.Selector(fuseableRunes, ColorfulToStringHelper.RuneToString, "Select a rune to fuse:", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));

        if (rune == null)
        {
            _outputManager.WriteLine("\nNo rune selected for fusion.\n", ConsoleColor.Red);
            return;
        }

        // fuse runes
        try
        {
            _craftingService.FuseRune(playerId, rune.RuneId, rune.Tier);
            _outputManager.WriteLine($"\nSuccessfully fused {rune.Rune.Name}!\n", ConsoleColor.Green);
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
        
        var rune = _inputManager.Selector(destroyableRunes, ColorfulToStringHelper.RuneToString, 
            "Select a rune to destroy:", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
        if (rune == null)
        {
            _outputManager.WriteLine("\nNo rune selected for destruction.\n", ConsoleColor.Red);
            return;
        }
        
        var confirm = _inputManager.ReadString(
            $"\nAre you sure you want to destroy {rune.Rune.Name}? This action cannot be undone. (y/n): ",
            ["y", "n"]);

        if (!confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            _outputManager.WriteLine("\nRune destruction cancelled.\n", ConsoleColor.Red);
            return;
        }

        try
        {
            _craftingService.DestroyRune(playerId, rune.RuneId, rune.Tier);
            _outputManager.WriteLine($"\nSuccessfully destroyed {rune.Rune.Name}!\n", ConsoleColor.Green);
        }
        catch (RuneException ex)
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

        var oldEssence = _inputManager.Selector(transmutableEssence, e => e.Ingredient.Name, "Select an essence to transmute:");
        if (oldEssence == null)
        {
            _outputManager.WriteLine("\nNo essence selected for transmutation.\n", ConsoleColor.Red);
            return;
        }
        
        try
        {
            _craftingService.TransmuteEssence(playerId, oldEssence.IngredientId);
            _outputManager.WriteLine($"\nSuccessfully transmuted {oldEssence.Ingredient.Name}!\n", ConsoleColor.Green);
        }
        catch (IngredientException ex)
        {
            _outputManager.WriteLine($"\nError transmuting essence: {ex.Message}\n", ConsoleColor.Red);
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
}