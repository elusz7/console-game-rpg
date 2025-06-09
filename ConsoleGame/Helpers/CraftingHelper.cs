using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGame.Helpers.AdventureEnums;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers;

public class CraftingHelper(IOutputManager outputManager, IInputManager inputManager, IRecipeDao recipeDao, IRuneDao runeDao)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly IRecipeDao _recipeDao = recipeDao;
    private readonly IRuneDao _runeDao = runeDao;
    private Player _player;

    public void CraftingTable(Player player)
    {
        _player = player;

        var options = GetCraftingOptions();

        var index = 1;
        foreach (var option in options)
        {
            _outputManager.WriteLine($"{index}. {option.Value}", ConsoleColor.Yellow);
            index++;
        }

        var choice = options.ElementAt(_inputManager.ReadInt("\tSelect an option: ", options.Count) - 1);

        switch (choice.Value)
        {
            case CraftingOptions.CraftRune:
                CraftRune();
                break;
            case CraftingOptions.FuseRunes:
                FuseRunes();
                break;
            case CraftingOptions.DestroyRune:
                DestroyRune();
                break;
            case CraftingOptions.ViewRecipes:
                ViewRecipeMenu();
                break;
            case CraftingOptions.Exit:
                _outputManager.WriteLine("Leaving the crafting table.", ConsoleColor.Green);
                return;
        }
    }

    private void CraftRune()
    {
        // List available runes for crafting
        List<Recipe> craftableRecipes = _recipeDao.GetCraftableRecipes(_player.Inventory.Ingredients);
        if (craftableRecipes.Count == 0)
        {
            _outputManager.WriteLine("You have no recipes available to craft.", ConsoleColor.Red);
            return;
        }

        // Select rune to craft
        Recipe? recipe = _inputManager.Selector(craftableRecipes, r => ColorfulToStringHelper.RecipeToString(r), "Select a rune to craft:", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
        if (recipe == null)
        {
            _outputManager.WriteLine("No rune selected.", ConsoleColor.Red);
            return;
        }

        // Craft Rune
        try
        {
            Rune craftedRune = recipe.CraftRune(_player.Inventory.Ingredients);
            _player.Inventory.AddRune(craftedRune);
            _outputManager.WriteLine($"Successfully crafted {craftedRune.Name}!", ConsoleColor.Green);
        }
        catch (RecipeException ex)
        {
            _outputManager.WriteLine($"Error crafting rune: {ex.Message}", ConsoleColor.Red);
            return;
        }
    }

    private void FuseRunes()
    {
        // fusing runes increases the rune's tier. a rune starts at tier 1 and goes up to tier 3. when it reaches tier 4, it evolves into the next rarity level
        var runesToFuse = _player.Inventory.GetFuseableRunes();
        if (runesToFuse.Count == 0)
        {
            _outputManager.WriteLine("You have no runes available to fuse.", ConsoleColor.Red);
            return;
        }

        // Select rune to fuse
        (var rune, var _) = _inputManager.Selector(runesToFuse, r => ColorfulToStringHelper.RuneToString(r.rune), "Select a rune to fuse:", r => ColorfulToStringHelper.GetRarityColor(r.rune.Rarity));

        if (rune == null)
        {
            _outputManager.WriteLine("No rune selected for fusion.", ConsoleColor.Red);
            return;
        }

        // fuse runes
        _player.Inventory.RemoveRune(rune, 4); // remove 4 runes used for fusion
        var fusedRune = _runeDao.FuseRune(rune); // fuse runes, this will increase the tier of the rune
        _player.Inventory.AddRune(fusedRune); // add the fused rune to the inventory
    }

    private void DestroyRune()
    {
        // List runes available for destruction
        var runesToDestroy = _player.Inventory.GetDestroyableRunes();
        if (runesToDestroy.Count == 0)
        {
            _outputManager.WriteLine("You have no runes available to destroy.", ConsoleColor.Red);
            return;
        }
        // Select rune to destroy
        var rune = _inputManager.Selector(runesToDestroy, r => ColorfulToStringHelper.RuneToString(r), "Select a rune to destroy:", r => ColorfulToStringHelper.GetRarityColor(r.Rarity));
        if (rune == null)
        {
            _outputManager.WriteLine("No rune selected for destruction.", ConsoleColor.Red);
            return;
        }

        // Confirm destruction
        var confirm = _inputManager.ReadString($"Are you sure you want to destroy {rune.Name}? This action cannot be undone. (y/n): ", ["y", "n"]).ToLower();
        if (confirm.Equals("y"))
        {
            _outputManager.WriteLine("Rune destruction cancelled.", ConsoleColor.Yellow);
            return;
        }

        // Destroy Rune
        _player.Inventory.DestroyRune(rune); // remove the rune from the inventory
        _outputManager.WriteLine($"Successfully destroyed {rune.Name}!", ConsoleColor.Green);
    }

    private void ViewRecipeMenu()
    {
        var options = GetRecipeOptions();

        var index = 1;
        foreach (var option in options)
        {
            _outputManager.WriteLine($"{index}. {option.Value}", ConsoleColor.Yellow);
            index++;
        }

        var choice = options.ElementAt(_inputManager.ReadInt("\tSelect an option: ", options.Count) - 1);

        switch (choice.Value)
        {
            case RecipeOptions.ViewAll:
                ViewRecipes("All");
                break;
            case RecipeOptions.ViewCraftable:
                ViewRecipes("Craftable");
                break;
            case RecipeOptions.SearchByName:
                SearchRecipes("Name");
                break;
            case RecipeOptions.SearchByElement:
                SearchRecipes("Element");
                break;
            case RecipeOptions.SearchByRarity:
                SearchRecipes("Rarity");
                break;
            case RecipeOptions.Exit:
                return;
        }
    }
    private void ViewRecipes(string type)
    {
        List<Recipe> recipes = type switch
        {
            "All" => _recipeDao.GetAllRecipes(),
            "Craftable" => _recipeDao.GetCraftableRecipes(_player.Inventory.Ingredients),
            _ => throw new ArgumentException("Invalid recipe type.")
        };

        if (recipes.Count == 0)
        {
            _outputManager.WriteLine($"No recipes found.", ConsoleColor.Red);
            return;
        }

        _inputManager.Viewer(recipes, r => ColorfulToStringHelper.RecipeToString(r), $"Viewing {type} Recipes", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
    }

    private void SearchRecipes(string criteria)
    {
        var searchTerm = criteria switch
        {
            "Name" => _inputManager.ReadString("Enter recipe name: "),
            "Element" => _inputManager.GetEnumChoice<ElementType>("Select Element : ").ToString(),
            "Rarity" => _inputManager.GetEnumChoice<RarityLevel>("Select Rarity: ").ToString(),
            _ => throw new ArgumentException("Invalid search criteria.")
        };

        List<Recipe> recipes = criteria switch
        {
            "Name" => _recipeDao.FindRecipesByName(searchTerm),
            "Element" => _recipeDao.FindRecipesByElement(searchTerm),
            "Rarity" => _recipeDao.FindRecipesByRarity(searchTerm),
            _ => throw new ArgumentException("Invalid search criteria.")
        };

        if (recipes.Count == 0)
        {
            _outputManager.WriteLine($"No recipes found for {criteria.ToLower()} '{searchTerm}'.", ConsoleColor.Red);
            return;
        }

        _inputManager.Viewer(recipes, r => ColorfulToStringHelper.RecipeToString(r), $"Viewing Recipes by {criteria}", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
    }

    private static Dictionary<string, CraftingOptions> GetCraftingOptions()
    {
        var options = new Dictionary<string, CraftingOptions>();

        AddMenuOption(options, "Craft Runes", CraftingOptions.CraftRune);
        AddMenuOption(options, "Fuse Runes", CraftingOptions.FuseRunes);
        AddMenuOption(options, "Destroy Rune", CraftingOptions.DestroyRune);
        AddMenuOption(options, "View Recipes", CraftingOptions.ViewRecipes);

        AddMenuOption(options, "Exit", CraftingOptions.Exit);

        return options;
    }
    private static Dictionary<string, RecipeOptions> GetRecipeOptions()
    {
        var options = new Dictionary<string, RecipeOptions>();

        AddMenuOption(options, "View All Recipes", RecipeOptions.ViewAll);
        AddMenuOption(options, "View Craftable Recipes", RecipeOptions.ViewCraftable);
        AddMenuOption(options, "Find A Recipe By Name", RecipeOptions.SearchByName);
        AddMenuOption(options, "Find Recipes With Element", RecipeOptions.SearchByElement);
        AddMenuOption(options, "Find Recipes With Rarity", RecipeOptions.SearchByRarity);

        AddMenuOption(options, "Exit", RecipeOptions.Exit);

        return options;
    }
    private static void AddMenuOption<T>(Dictionary<string, T> options, string label, T option) where T : Enum
    {
        options.TryAdd(label, option);
    }
}
