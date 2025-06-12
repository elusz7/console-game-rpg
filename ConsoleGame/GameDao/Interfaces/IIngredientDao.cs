using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IIngredientDao
{
    Ingredient TransmuteEssence(Ingredient ingredient);
}
