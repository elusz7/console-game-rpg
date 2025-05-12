using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame.Helpers;

public class AdventureEnums
{
    public enum AdventureOptions
    {
        Attack = 0,
        Examine = 1,
        Rest = 3,
        Merchant = 4,
        Equipment = 5,
        Move = 6,
        Quit = 7
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
}
