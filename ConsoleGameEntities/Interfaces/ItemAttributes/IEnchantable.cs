﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameEntities.Interfaces.ItemAttributes;

public interface IEnchantable
{
    void Enchant();
    decimal GetEnchantmentPrice();
}
