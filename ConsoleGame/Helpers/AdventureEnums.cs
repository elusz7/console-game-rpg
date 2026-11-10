namespace ConsoleGame.Helpers;

public static class AdventureEnums
{
    public enum AdventureOptions
    {
        Attack = 0,
        Examine = 1,
        Rest = 3,
        Merchant = 4,
        Equipment = 5,
        Move = 6,
        Craft = 7,
        Quit = 8
    }

    public enum MerchantOptions
    {
        Buy = 0,
        Sell = 1,
        Enchant = 2, //raise item level
        Reforge = 3, //reroll armor defense/resistance power
        Purify = 4, //remove curse from item
        Exit = 5,
    }

    public enum CombatOptions
    {
        Attack = 0,
        UseSkill = 1,
        UseConsumable = 2,
        SkipTurn = 3
    }
    
    public enum CraftingOptions
    {
        CraftRune = 0,
        FuseRunes = 1,
        DestroyRune = 2,
        TransmuteEssence = 3,
        ViewRecipes = 4,
        Exit = 5
    }

    public enum RecipeOptions
    {
        ViewAll = 0,
        ViewCraftable = 1,
        SearchByName = 2,
        SearchByElement = 3,
        SearchByRarity = 4,
        Exit = 5
    }
}