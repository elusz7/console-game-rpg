using System.Runtime.InteropServices;
using ConsoleGame.GameDao;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGameEntities.Main.Models.Entities;
using static ConsoleGameEntities.Main.Models.Entities.ModelEnums;

namespace ConsoleGame.Managers.CrudHelpers;

public class ArchetypeManagement(InputManager inputManager, OutputManager outputManager, ArchetypeDao archetypeDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly ArchetypeDao _archetypeDao = archetypeDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Archetype Management Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Create Archetype"
                + "\n2. Edit Archetype"
                + "\n3. Delete Archetype"
                + "\n4. Return to Archetype Main Menu");

            var choice = _inputManager.ReadMenuKey(4);

            switch (choice)
            {
                case 1:
                    AddArchetype();
                    break;
                case 2:
                    EditArchetype();
                    break;
                case 3:
                    DeleteArchetype();
                    break;
                case 4:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void AddArchetype()
    {
        do
        {
            CreateArchetype();
        } while (_inputManager.LoopAgain("create"));
    }
    private void CreateArchetype()
    {
        _outputManager.WriteLine("1. Martial Archetypes\n2. Magical Archetypes", ConsoleColor.Yellow);
        var typeChoice = _inputManager.ReadInt("Choose Archetype Type: ", 2);
        var archetypeType = typeChoice == 1 ? ArchetypeType.Martial : ArchetypeType.Magical;

        var damage = archetypeType == ArchetypeType.Martial ? "Attack" : "Magic";
        var resourceName = archetypeType == ArchetypeType.Martial ? "Stamina" : "Mana";

        int[] statOrderArray = GetStatPriorities();

        var damageBonus = _inputManager.ReadInt("Enter the bonus for " + damage + ": ");

        // Base multipliers and growths
        decimal damageMultiplier = 0.2M, defenseMultiplier = 0.2M, resistanceMultiplier = 0.2M, speedMultiplier = 0.2M, resourceMultiplier = 0.2M;
        int recovery = 1, growth = 1;

        for (int i = 1; i <= 6; i++)
        {
            switch (statOrderArray[i - 1])
            {
                case 1: damageMultiplier = i / 10M; break;
                case 2: defenseMultiplier = i / 10M; break;
                case 3: resistanceMultiplier = i / 10M; break;
                case 4: speedMultiplier = i / 10M; break;
                case 5: recovery = (int)Math.Round(i / 2.0); break;
                case 6:
                    resourceMultiplier = i / 10M;
                    growth = (int)Math.Round(i / 4.0);
                    break;
            }
        }

        var archetype = new Archetype
        {
            Name = _inputManager.ReadString("Enter Archetype Name: "),
            Description = _inputManager.ReadString("Enter Archetype Description: "),
            HealthBase = _inputManager.ReadInt("Enter Health Base: "),
            AttackBonus = archetypeType == ArchetypeType.Martial ? damageBonus : 0,
            MagicBonus = archetypeType == ArchetypeType.Magical ? damageBonus : 0,
            DefenseBonus = _inputManager.ReadInt("Enter Defense Bonus: "),
            ResistanceBonus = _inputManager.ReadInt("Enter Resistance Bonus: "),
            Speed = _inputManager.ReadInt("Enter Speed: "),
            ArchetypeType = archetypeType,
            ResourceName = resourceName,
            MaxResource = _inputManager.ReadInt("Enter Max Resource: "),
            RecoveryRate = recovery,
            AttackMultiplier = archetypeType == ArchetypeType.Martial ? damageMultiplier : 0,
            MagicMultiplier = archetypeType == ArchetypeType.Magical ? damageMultiplier : 0,
            DefenseMultiplier = defenseMultiplier,
            ResistanceMultiplier = resistanceMultiplier,
            SpeedMultiplier = speedMultiplier,
            ResourceMultiplier = resourceMultiplier,
            RecoveryGrowth = growth
        };

        _archetypeDao.AddArchetype(archetype);
        _outputManager.WriteLine($"Archetype '{archetype.Name}' created successfully!", ConsoleColor.Green);
    }
    private void EditArchetype()
    {
        var archetypes = _archetypeDao.GetAllNonCoreArchetypes();

        if (archetypes.Count == 0)
        {
            _outputManager.WriteLine("No archetypes available to edit.", ConsoleColor.Red);
            return;
        }

        var archetype = _inputManager.Selector(
            archetypes,
            a => ColorfulToStringHelper.ArchetypeToString(a),
            "Select an archetype to edit",
            b => ColorfulToStringHelper.GetArchetypeColor(b)
        );

        if (archetype == null)
        {
            _outputManager.WriteLine("No archetype selected.", ConsoleColor.Red);
            return;
        }

        var propertyActions = new Dictionary<string, Action>
        {
            { "Name", () => archetype.Name = _inputManager.ReadString("\nEnter new value for Name: ") },
            { "Health", () => archetype.HealthBase = _inputManager.ReadInt("\nEnter new value for Health: ") },
            { "Speed", () => archetype.Speed = _inputManager.ReadInt("\nEnter new value for Speed: ") },
            { "Damage Bonus", () =>
                {
                    if (archetype.ArchetypeType == ArchetypeType.Martial)
                    {
                        archetype.AttackBonus = _inputManager.ReadInt("\nEnter new value for Damage Bonus: ");
                    }
                    else
                    {
                        archetype.MagicBonus = _inputManager.ReadInt("\nEnter new value for Damage Bonus: ");
                    }
                }},
            { "Defense Bonus", () => archetype.DefenseBonus = _inputManager.ReadInt("\nEnter new value for defense bonus: ") },
            { "Resistance Bonus", () => archetype.ResistanceBonus = _inputManager.ReadInt("\nEnter new value for resistance bonus: ") },
            { "Max Resource", () => archetype.MaxResource = _inputManager.ReadInt("\nEnter new value for max resource: ") },
            { "Resource Recovery", () => archetype.RecoveryRate = _inputManager.ReadInt("\nEnter new value for resource recovery: ") },
            { "Stat Growth Priorities", () =>
            {
                int[] statOrderArray = GetStatPriorities();

                for (int i = 1; i <= 6; i++)
                {
                    switch (statOrderArray[i - 1])
                    {
                        case 1: var damageMultiplier = i / 10M;
                            if (archetype.ArchetypeType == ArchetypeType.Martial)
                                archetype.AttackMultiplier = damageMultiplier;
                            else
                                archetype.MagicMultiplier = damageMultiplier;
                            break;
                        case 2: archetype.DefenseMultiplier = i / 10M; break;
                        case 3: archetype.ResistanceMultiplier = i / 10M; break;
                        case 4: archetype.SpeedMultiplier = i / 10M; break;
                        case 5: archetype.RecoveryRate = (int)Math.Round(i / 2.0); break;
                        case 6:
                            archetype.ResourceMultiplier = i / 10M;
                            archetype.RecoveryGrowth = (int)Math.Round(i / 4.0);
                            break;
                    }
                }
            }},
        };

        while (true)
        {
            _outputManager.WriteLine("Editing Archetype: " + archetype.Name, ConsoleColor.Cyan);
            _outputManager.WriteLine($"{archetype}", ConsoleColor.Green);

            int option = _inputManager.DisplayEditMenu([.. propertyActions.Keys]);

            if (option == propertyActions.Count + 1)
            {
                _archetypeDao.UpdateArchetype(archetype);
                _outputManager.WriteLine($"\nExiting. Any changes made have been successfully applied to {archetype.Name}\n", ConsoleColor.Green);
                return;
            }

            string selectedProperty = propertyActions.Keys.ElementAt(option - 1);
            propertyActions[selectedProperty].Invoke();
        }
    }
    private void DeleteArchetype()
    {
        var archetypes = _archetypeDao.GetAllNonCoreArchetypes();

        if (archetypes.Count == 0)
        {
            _outputManager.WriteLine("No archetypes available to delete.", ConsoleColor.Red);
            return;
        }

        var archetypeToDelete =
            _inputManager.Selector(
                archetypes,
                a => ColorfulToStringHelper.ArchetypeToString(a),
                "Select an archetype to delete",
                b => ColorfulToStringHelper.GetArchetypeColor(b)
            );

        if (archetypeToDelete == null)
        {
            _outputManager.WriteLine("No archetype selected for deletion.", ConsoleColor.Red);
            return;
        }

        if (!_inputManager.ConfirmAction("delete"))
        {
            _outputManager.WriteLine("Deletion cancelled.", ConsoleColor.Red);
            return;
        }

        _archetypeDao.DeleteArchetype(archetypeToDelete);
        _outputManager.WriteLine($"Archetype '{archetypeToDelete.Name}' deleted successfully!", ConsoleColor.Green);
    }
    private int[] GetStatPriorities()
    {
        _outputManager.WriteLine("Please rank these stats in order of *least* important to *most* important for your archetype, separated by commas (e.g., 1,2,3,4,5,6):");
        _outputManager.WriteLine($"1. Damage\n2. Defense\n3. Resistance\n4. Speed\n5. Resource Recovery\n6. Resource Growth", ConsoleColor.Yellow);
        int[] statOrderArray;
        do
        {
            var statOrder = _inputManager.ReadString("Enter the order of importance: ");
            statOrderArray = statOrder.Split(',').Select(s => int.TryParse(s.Trim(), out int val) ? val : -1).ToArray();
            if (statOrderArray.Length != 6 || statOrderArray.Distinct().Count() != 6 || statOrderArray.Any(x => x < 1 || x > 6))
            {
                _outputManager.WriteLine("Invalid input. Please enter 6 distinct numbers from 1 to 6.", ConsoleColor.Red);
            }
            else
            {
                break;
            }
        } while (true);
        return statOrderArray;
    }
}