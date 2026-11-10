using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGame.Helpers.AdventureEnums;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Menus;

public class RecipeMenu(IIngredientDao ingredientDao, IRecipeDao recipeDao,
    IInputManager inputManager, IOutputManager outputManager)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly IRecipeDao _recipeDao = recipeDao;
    private readonly IIngredientDao _ingredientDao = ingredientDao;

    public void RecipeMainMenu(int playerId)
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
                    ViewRecipes(playerId,"All");
                    break;
                case RecipeOptions.ViewCraftable:
                    ViewRecipes(playerId,"Craftable");
                    break;
                case RecipeOptions.SearchByName:
                    SearchRecipes(playerId,"Name");
                    break;
                case RecipeOptions.SearchByElement:
                    SearchRecipes(playerId,"Element");
                    break;
                case RecipeOptions.SearchByRarity:
                    SearchRecipes(playerId,"Rarity");
                    break;
                case RecipeOptions.Exit:
                    _outputManager.WriteLine();
                    return;
            }
        }
    }
    
    private void ViewRecipes(int playerId, string type)
    {
        var recipes = type switch
        {
            "All" => _recipeDao.GetAllRecipes(),
            "Craftable" => _recipeDao.GetCraftableRecipesByPlayerId(playerId),
            _ => throw new ArgumentException("Invalid recipe type.")
        };

        if (recipes.Count == 0)
        {
            _outputManager.WriteLine($"\nNo recipes found.\n", ConsoleColor.Red);
            return;
        }

        var availableIngredients = _ingredientDao.GetIngredientsByPlayerId(playerId);

        _inputManager.Viewer(
            recipes, r => ColorfulToStringHelper.RecipeToString(r, availableIngredients),
            $"Viewing {type} Recipes", r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
    }
    
    private void SearchRecipes(int playerId, string criteria)
    {
        var allRecipes = _recipeDao.GetAllRecipes();

        var recipes = criteria switch
        {
            "Name" => SearchByName(allRecipes),
            "Element" => SearchByElement(allRecipes),
            "Rarity" => SearchByRarity(allRecipes),
            _ => throw new ArgumentException("Invalid search criteria.")
        };

        if (recipes.Count == 0)
        {
            _outputManager.WriteLine(
                $"\nNo recipes found for {criteria.ToLower()}.\n",
                ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();

        var availableIngredients = _ingredientDao.GetIngredientsByPlayerId(playerId);

        _inputManager.Viewer(
            recipes,
            r => ColorfulToStringHelper.RecipeToString(r, availableIngredients),
            $"Viewing {criteria} Recipes",
            r => ColorfulToStringHelper.GetRarityColor(r.Rune.Rarity));
    }

    private List<Recipe> SearchByName(List<Recipe> recipes)
    {
        var searchTerm = _inputManager.ReadString("\nEnter recipe name: ");

        return recipes
            .Where(r => r.Name.Contains(
                searchTerm,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    
    private List<Recipe> SearchByElement(List<Recipe> recipes)
    {
        var element = _inputManager.GetEnumChoice<ElementType>("\nSelect Element");

        return recipes
            .Where(r => r.Rune.Element == element)
            .ToList();
    }

    private List<Recipe> SearchByRarity(List<Recipe> recipes)
    {
        var rarity = _inputManager.GetEnumChoice<RarityLevel>("\nSelect Rarity");

        return recipes
            .Where(r => r.Rune.Rarity == rarity)
            .ToList();
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