using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.GameDao;

internal class IngredientDao(GameContext context) : IIngredientDao
{
    private readonly GameContext _context = context;

    public Ingredient TransmuteEssence(Ingredient ingredient)
    {
        return _context.Ingredients.First(i => i.Id == ingredient.Id + 1);
    }
}
