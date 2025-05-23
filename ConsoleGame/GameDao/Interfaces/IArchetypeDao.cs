using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao.Interfaces;

public interface IArchetypeDao
{
    void AddArchetype(Archetype archetype);
    void UpdateArchetype(Archetype archetype);
    void DeleteArchetype(Archetype archetype);

    int GetArchetypeId(string archetype);

    List<string> GetArchetypeNames();
    List<Archetype> GetArchetypesByType(ArchetypeType archetypeType);

    List<Archetype> GetAllArchetypes();

    List<Archetype> GetAllNonCoreArchetypes();
}
