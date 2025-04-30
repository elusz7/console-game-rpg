using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameEntities.Interfaces;

public interface IItem
{
    int Id { get; set; }
    string Name { get; set; }
    decimal Value { get; set; }
    string Description { get; set; }
    int Durability { get; set; }
    decimal Weight { get; set; }

    int Use();
}
