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

public class CraftingHelper(IOutputManager outputManager, IInputManager inputManager, IRecipeDao recipeDao, 
    IRuneDao runeDao, IIngredientDao ingredientDao)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly IRecipeDao _recipeDao = recipeDao;
    private readonly IRuneDao _runeDao = runeDao;
    private readonly IIngredientDao _ingredientDao = ingredientDao;
    private Player _player;

    public void CraftingTable(Player player)
    {
        _player = player;

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
                    CraftRune();
                    break;
                case CraftingOptions.FuseRunes:
                    FuseRunes();
                    break;
                case CraftingOptions.DestroyRune:
                    DestroyRune();
                    break;
                case CraftingOptions.TransmuteEssence:
                    TransmuteEssence();
                    break;
                case CraftingOptions.ViewRecipes:
                    ViewRecipeMenu();
                    break;
                case CraftingOptions.Exit:
                    _outputManager.WriteLine("Leaving the crafting table.", ConsoleColor.Green);
                    return;
            }
        }
    }

    private void CraftRune()
    {
        // List available runes for crafting
        List<Recipe> craftableRecipes = _recipeDao.GetCraftableRecipes(_player.Inventory.Ingredients);
        if (craftableRecipes.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no recipes available to craft.\n", ConsoleColor.Red);
            return;
        }

        // Select rune to craft
        Recipe? recipe = _inputManager.Selector(craftableRecipes, r => ColorfulToStringHelper.RecipeToString(r, _player.Inventory.Ingredients), "Select a rune to craft:", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
        if (recipe == null)
        {
            _outputManager.WriteLine("\nNo rune selected.\n", ConsoleColor.Red);
            return;
        }

        // Craft Rune
        try
        {
            Rune craftedRune = recipe.CraftRune(_player.Inventory.Ingredients);
            _player.Inventory.AddRune(craftedRune);
            _outputManager.WriteLine($"\nSuccessfully crafted {craftedRune.Name}!\n", ConsoleColor.Green);
        }
        catch (RecipeException ex)
        {
            _outputManager.WriteLine($"\nError crafting rune: {ex.Message}\n", ConsoleColor.Red);
            return;
        }
    }

    private void FuseRunes()
    {
        // fusing runes increases the rune's tier. a rune starts at tier 1 and goes up to tier 3. when it reaches tier 4, it evolves into the next rarity level
        var runesToFuse = _player.Inventory.GetFuseableRunes();
        if (runesToFuse.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no runes available to fuse.\n", ConsoleColor.Red);
            return;
        }

        // Select rune to fuse
        (var rune, var _) = _inputManager.Selector(runesToFuse, r => ColorfulToStringHelper.RuneToString(r.rune), "Select a rune to fuse:", r => ColorfulToStringHelper.GetRarityColor(r.rune.Rarity));

        if (rune == null)
        {
            _outputManager.WriteLine("\nNo rune selected for fusion.\n", ConsoleColor.Red);
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
            _outputManager.WriteLine("\nYou have no runes available to destroy.\n", ConsoleColor.Red);
            return;
        }
        // Select rune to destroy
        var rune = _inputManager.Selector(runesToDestroy, r => ColorfulToStringHelper.RuneToString(r), "Select a rune to destroy:", r => ColorfulToStringHelper.GetRarityColor(r.Rarity));
        if (rune == null)
        {
            _outputManager.WriteLine("\nNo rune selected for destruction.\n", ConsoleColor.Red);
            return;
        }

        // Confirm destruction
        var confirm = _inputManager.ReadString($"\nAre you sure you want to destroy {rune.Name}? This action cannot be undone. (y/n): ", ["y", "n"]).ToLower();
        if (!confirm.Equals("y"))
        {
            _outputManager.WriteLine("\nRune destruction cancelled.\n", ConsoleColor.Red);
            return;
        }

        // Destroy Rune
        _player.Inventory.DestroyRune(rune); // remove the rune from the inventory
        _outputManager.WriteLine($"\nSuccessfully destroyed {rune.Name}!\n", ConsoleColor.Green);
    }
    private void TransmuteEssence()
    {
        var transmutableEssence = _player.Inventory.GetTransmutableEssence();
        if (transmutableEssence.Count == 0)
        {
            _outputManager.WriteLine("\nYou have no essence available to transmute.\n", ConsoleColor.Red);
            return;
        }

        var oldEssence = _inputManager.Selector(transmutableEssence, e => e.Name, "Select an essence to transmute:");
        if (oldEssence == null)
        {
            _outputManager.WriteLine("\nNo essence selected for transmutation.\n", ConsoleColor.Red);
            return;
        }

        var newEssence = _ingredientDao.TransmuteEssence(oldEssence);

        try
        {
            _player.Inventory.TransmuteEssence(oldEssence, newEssence);
            _outputManager.WriteLine($"\nSuccessfully transmuted {oldEssence.Name} into {newEssence.Name}!\n", ConsoleColor.Green);
        }
        catch (IngredientNotFoundException ex)
        {
            _outputManager.WriteLine($"\nError transmuting essence: {ex.Message}\n", ConsoleColor.Red);
        }
    }
    private void ViewRecipeMenu()
    {
        var options = GetRecipeOptions();

        while (true)
        {
            var index = 1;
            _outputManager.WriteLine();
            foreach (var option in options)
            {
                _outputManager.WriteLine($"{index}. {option.Key}", ConsoleColor.Yellow);
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
                    _outputManager.WriteLine();
                    return;
            }
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
            _outputManager.WriteLine($"\nNo recipes found.\n", ConsoleColor.Red);
            return;
        }

        _inputManager.Viewer(recipes, r => ColorfulToStringHelper.RecipeToString(r, _player.Inventory.Ingredients), $"Viewing {type} Recipes", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
    }

    private void SearchRecipes(string criteria)
    {
        var searchTerm = criteria switch
        {
            "Name" => _inputManager.ReadString("\nEnter recipe name: "),
            "Element" => _inputManager.GetEnumChoice<ElementType>("\nSelect Element").ToString(),
            "Rarity" => _inputManager.GetEnumChoice<RarityLevel>("\nSelect Rarity").ToString(),
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
            _outputManager.WriteLine($"\nNo recipes found for {criteria.ToLower()} '{searchTerm}'.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        _inputManager.Viewer(recipes, r => ColorfulToStringHelper.RecipeToString(r, _player.Inventory.Ingredients), $"Viewing Recipes by {criteria}", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
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
