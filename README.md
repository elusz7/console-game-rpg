# Console RPG

A turn-based, console-based role-playing game built with C# and .NET. Explore connected rooms, fight monsters with tactical skills, collect and manage equipment, interact with a merchant, and advance through increasingly challenging floors. The solution also includes an administrative data-management interface and a separate domain/entities project backed by Entity Framework Core.

> **Repository note:** This README documents the source tree as supplied. The application targets .NET 8, while the entities/data project targets .NET 6. Database setup depends on a SQL Server instance and a valid connection string. See [Configuration](#configuration-and-database) before running the game.

## Contents

- [Features](#features)
- [Solution structure](#solution-structure)
- [Technology stack](#technology-stack)
- [Requirements](#requirements)
- [Getting started](#getting-started)
- [Configuration and database](#configuration-and-database)
- [Running the game](#running-the-game)
- [Gameplay overview](#gameplay-overview)
- [Administrative tools](#administrative-tools)
- [Running tests](#running-tests)
- [Architecture](#architecture)
- [Known limitations and improvement opportunities](#known-limitations-and-improvement-opportunities)
- [Contributing](#contributing)

## Features

### Adventure and campaign play
- Start an adventure or a campaign with a selected player.
- Progress through floors; clearing a floor advances the player and generates the next floor.
- Choose between the configured map and a randomly generated map in standard adventure mode.
- Explore rooms connected by cardinal directions.
- Encounter monsters in rooms and clear them before continuing.
- Rest at the entrance room to restore health.
- Meet a merchant at the entrance.

### Characters and progression
- Create and manage player characters and archetypes through the game's data-management features.
- Character attributes and archetype multipliers influence combat.
- Gain levels as floors are completed.
- Archetypes have a resource pool used to activate skills.
- Campaign play saves player progression when the campaign is quit or the player dies.

### Tactical combat and monster behavior
- Turn-based encounters against one or more monsters.
- Basic attacks, skills, consumables, and the option to skip a turn.
- Turn ordering considers speed.
- Skills can have target types, resource costs, level requirements, cooldowns, and effects.
- Monster types include standard, elite, and boss variants.
- Monster behavior is implemented through strategy classes, including default, berserker, offensive, defensive, cautious, cunning, elite, and boss strategies.

### Items, inventory, and merchant
- Item families include weapons, armor, valuables, and consumables.
- Equipment can affect player combat statistics and has durability-related behavior.
- Inventory has a capacity/weight model and tracks gold.
- Consumables support health, durability, and resource use.
- Merchant functionality includes buying and selling, as well as item enchantment, reforging, and curse purification.
- Monsters can carry treasure that can be collected after combat.

### Administrative and data management
- An administrator menu exposes management workflows for players, archetypes, items, inventories, monsters, rooms/connections, and skills.
- Data access is separated behind DAO interfaces and implementations.
- Entity Framework Core migrations and seed data are included in the entities project.

### Tests
- The test project uses MSTest and Moq.
- The supplied test tree includes tests for entities, items, skills, monster behavior strategies, factories, and selected management helpers.

## Solution structure

```text
.
├── ConsoleGame/
│   ├── Factories/             # Floor, map, item, and monster generation
│   ├── GameDao/               # Data-access implementations and interfaces
│   ├── Helpers/               # Combat, equipment, map, merchant, player,
│   │                          # display, and CRUD workflows
│   ├── Managers/              # Console input/output and map management
│   ├── Menus/                 # Start, adventure administration, and CRUD menus
│   ├── Models/                # Runtime floor model
│   ├── Services/              # Game engine and adventure orchestration
│   ├── Startup.cs             # Dependency-injection registrations
│   └── Program.cs             # Application entry point
├── ConsoleGameEntities/
│   ├── Data/                  # EF Core DbContext and design-time factory
│   ├── Exceptions/            # Domain-specific exceptions
│   ├── Helpers/               # Configuration and migration helpers
│   ├── Interfaces/            # Domain contracts and attributes
│   ├── Migrations/            # EF Core migrations and model snapshot
│   └── Models/                # Players, archetypes, rooms, items,
│                              # monsters, skills, and AI strategies
├── ConsoleGameTests/
│   ├── ConsoleGame/           # Application-layer tests
│   ├── ConsoleGameEntities/   # Domain/entity tests
│   └── TestHelpers/
├── console-game-rpg.sln
└── README.md
```

### Project responsibilities

| Project | Responsibility | Target framework |
|---|---|---|
| `ConsoleGame` | Console executable, menus, game loop, combat/adventure orchestration, factories, and data-access layer | .NET 8 |
| `ConsoleGameEntities` | Domain models, interfaces, exceptions, EF Core context, and migrations | .NET 6 |
| `ConsoleGameTests` | Automated tests for the application and domain projects | .NET 8 |

## Technology stack

- **C#**
- **.NET 8** application and test projects
- **.NET 6** entities/data project
- **Entity Framework Core 6.0.35**
- **SQL Server** provider for EF Core
- **EF Core lazy-loading proxies**
- **Microsoft.Extensions.DependencyInjection** for dependency injection
- **Microsoft.Extensions.Logging** and `NReco.Logging.File` for logging
- **MSTest** and **Moq** for tests

## Requirements

- .NET 8 SDK.
- A compatible .NET 6 targeting/runtime pack as required by your SDK and environment for the `ConsoleGameEntities` project.
- SQL Server. The checked-in default connection string uses SQL Server LocalDB, which is generally a Windows development setup and is not available on every operating system.
- Git, if cloning the repository.

Check installed SDKs and runtimes:

```bash
dotnet --list-sdks
dotnet --list-runtimes
```

## Getting started

Clone the repository and enter its root directory:

```bash
git clone <repository-url>
cd <repository-directory>
```

Restore and build the solution:

```bash
dotnet restore console-game-rpg.sln
dotnet build console-game-rpg.sln
```

If the build reports that the .NET 6 target is unavailable, install the appropriate .NET 6 targeting pack/runtime for your environment or consider upgrading `ConsoleGameEntities` to .NET 8 as described under [Improvement opportunities](#1-align-target-frameworks-and-package-versions).

## Configuration and database

The application reads `appsettings.json` through `ConfigurationHelper`. The console project and entities project each contain an `appsettings.json`; the console application's startup path uses the current working directory to locate configuration. The supplied default connection string is:

```text
Server=(localdb)\mssqllocaldb;Database=GameDatabaseFinal.12;Trusted_Connection=True;
```

This expects SQL Server LocalDB. For another SQL Server instance, replace `ConnectionStrings:DefaultConnection` in the configuration used by the running application.

Example using environment-variable configuration:

```bash
# Bash
export ConnectionStrings__DefaultConnection='Server=localhost;Database=ConsoleRpg;User Id=...;Password=...;TrustServerCertificate=True;'
```

```powershell
# PowerShell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=ConsoleRpg;User Id=...;Password=...;TrustServerCertificate=True;"
```

Use an appropriate authentication method for your SQL Server environment. Do not commit credentials or production connection strings. The repository's `ConsoleGameEntities/appsettings.json` contains a masked production connection-string entry; provide real secrets through environment variables or a secret store rather than source control.

### Applying migrations

EF Core migrations are located in `ConsoleGameEntities/Migrations`. The project includes an initial schema migration and a seed-data migration. The exact EF CLI command can depend on the working directory and startup configuration because the `DbContext` is in `ConsoleGameEntities` while the executable startup project is `ConsoleGame`.

After configuring a reachable SQL Server instance, install or invoke the EF Core CLI compatible with the EF Core 6 toolchain and inspect the context/migration discovery before applying migrations. A typical explicit project selection is:

```bash
dotnet ef database update \
  --project ConsoleGameEntities/ConsoleGameEntities.csproj \
  --startup-project ConsoleGame/ConsoleGame.csproj
```

If the command cannot load configuration or construct `GameContext`, resolve the startup project's configuration/working-directory setup first. Do not run migrations against a production database without a backup and a reviewed migration script.

## Running the game

From the repository root:

```bash
dotnet run --project ConsoleGame/ConsoleGame.csproj
```

The game starts at its start menu. Follow the numbered prompts. The main menu offers:

1. Administrator View
2. Adventure
3. Campaign
4. Quit

The application is an interactive console program; use a terminal that supports standard input and output. Some display helpers use console colors, so rendering may vary between terminal hosts.

## Gameplay overview

### Adventure flow

1. Choose **Adventure** or **Campaign** from the main menu.
2. Select/initialize a player when prompted.
3. In standard adventure mode, choose the configured or randomly generated map.
4. Explore rooms, fight living monsters, and collect treasure from defeated monsters.
5. Use the entrance room's rest and merchant services when available.
6. Clear the floor to level up and proceed to the next floor.

### Combat actions

Depending on the current situation, combat presents actions such as:

- **Attack** — perform a basic attack against a selected enemy.
- **Use Skill** — choose an available archetype skill. Skills may require resources, a minimum level, or a cooldown to be ready.
- **Use Consumable** — use an inventory consumable when one is available.
- **Skip Turn** — forfeit the player's current action.

Combat order is influenced by player and monster speed. Skill effects and targeting depend on the skill's configured target type.

### Equipment and merchant services

The entrance-room equipment workflow lets the player manage equipped items. The merchant workflow includes purchasing and selling items and several item-modification services. Availability and valid operations depend on the player's inventory, gold, and item state.

### Campaign persistence

The campaign workflow uses the player persistence layer. The code saves player state when a campaign is explicitly quit and on the player-death path. Do not assume that every intermediate action or every standard adventure is saved automatically.

## Administrative tools

The Administrator View contains menu-driven CRUD workflows for the game's backing data. The codebase includes management helpers for:

- Players
- Archetypes
- Items
- Inventories
- Monsters
- Rooms and room connections
- Skills

These tools are intended to manage game content stored through the EF Core data layer. Use a development database when experimenting with administrative changes.

## Running tests

Run the test project:

```bash
dotnet test ConsoleGameTests/ConsoleGameTests.csproj
```

Or run all tests in the solution:

```bash
dotnet test console-game-rpg.sln
```

The test project uses MSTest and Moq. Its supplied test files cover a range of domain behavior, including player, inventory, item, monster, skill, and monster-strategy behavior, plus selected factories and management helpers.

For a CI-friendly test result:

```bash
dotnet test ConsoleGameTests/ConsoleGameTests.csproj --logger "trx;LogFileName=test-results.trx"
```

## Architecture

The solution has a layered structure:

- **Console/presentation layer:** Menus and display helpers format prompts, options, maps, and game feedback.
- **Application/gameplay layer:** `GameEngine` and `AdventureService` coordinate the main loop, floors, exploration, and progression. Helpers such as `CombatHelper`, `MerchantHelper`, and `EquipmentHelper` implement focused gameplay workflows.
- **Data-access layer:** DAO interfaces and implementations encapsulate reads and writes for players, rooms, items, monsters, skills, archetypes, and inventories.
- **Domain layer:** `ConsoleGameEntities` contains the game entities and behavior. Interfaces and domain-specific exceptions describe supported operations and failure cases.
- **Persistence layer:** `GameContext` maps the domain model to SQL Server using EF Core. Migrations define schema evolution and seed data.
- **Composition root:** `Startup.ConfigureServices` registers services, DAOs, factories, managers, and helpers with dependency injection.

The domain uses inheritance/discriminators for item, monster, and skill subtypes. Monster AI is separated into strategy implementations, while factories create floors, maps, monsters, and loot.

## Known limitations and improvement opportunities

The following are code-review observations from static inspection of the supplied source. They are recommendations, not claims that runtime failures have been reproduced. The .NET SDK is not available in this review environment, so compilation and test execution could not be verified here.

### 1. Align target frameworks and package versions

`ConsoleGame` and `ConsoleGameTests` target .NET 8, but `ConsoleGameEntities` targets .NET 6. The entities project also references EF Core 6 and Microsoft.Extensions configuration packages 6, while the console project references `Microsoft.Extensions.Logging.Console` 9.

**Why it matters:** Mixed framework/package generations increase compatibility and maintenance risk and make the required SDK/runtime story less clear.

**Suggested improvement:**
- Prefer moving all projects to `net8.0` (or a single currently supported .NET target) together.
- Upgrade EF Core packages and tooling to the matching major version.
- Align Microsoft.Extensions package versions.
- Build and test the full solution after the upgrade and review generated migration changes carefully.

### 2. Make database configuration environment-safe

The default configuration assumes LocalDB. The entities configuration also contains a separate masked production connection-string key.

**Why it matters:** LocalDB is not portable to Linux/macOS, and environment-specific secrets should not live in checked-in configuration.

**Suggested improvement:**
- Keep only safe local-development defaults in source control.
- Use environment variables, .NET user secrets, or a deployment secret manager for credentials.
- Document supported SQL Server configurations and authentication modes.
- Add a clear startup validation message that identifies a missing or unreachable database without printing credentials.

### 3. Add automated build and test verification

The repository contains a substantial test suite, but no CI workflow was present in the inspected file list.

**Suggested improvement:**
- Add GitHub Actions (or equivalent) to restore, build, and test on every pull request.
- Publish TRX test results and optionally collect coverage.
- Add a separate integration-test job using a disposable SQL Server service/container if database workflows are tested.
- Make framework/package upgrades part of a repeatable CI matrix where appropriate.

### 4. Add integration tests for the full gameplay loop

The test tree is strongest around domain models, strategies, and factories. Important orchestration paths such as `AdventureService`, `GameEngine`, merchant transactions, and DAO/database behavior deserve explicit coverage.

**Suggested improvement:**
- Test floor completion, floor transitions, death, quitting, campaign saving, and treasure collection.
- Test combat turn ordering, dead-target handling, skill cooldown/resource consumption, and consumable use.
- Test merchant purchase/sale and item modification edge cases.
- Use an isolated test database/provider for persistence behavior rather than relying on a developer's LocalDB.

### 5. Improve separation of gameplay state and orchestration

`AdventureService` holds mutable player/floor/campaign state in instance fields and coordinates many gameplay concerns.

**Why it matters:** Stateful orchestration can make repeated runs, tests, and future multiplayer or save/load features harder to reason about.

**Suggested improvement:**
- Introduce an explicit `GameSession` or `AdventureState` object for player, floor, and mode.
- Pass session state to smaller services rather than keeping it implicitly on a long-lived service.
- Separate floor progression, room navigation, loot resolution, and campaign persistence into focused services.
- Make state transitions explicit and test them independently.

### 6. Strengthen error handling and input validation

Some gameplay paths catch broad exceptions and display the exception message directly. The entry point also resolves `GameEngine` with `GetService`, allowing a missing registration to result in a silent no-op.

**Suggested improvement:**
- Use `GetRequiredService<GameEngine>()` so dependency-registration errors fail clearly.
- Catch expected domain exceptions at the appropriate boundary; log unexpected exceptions with context.
- Show user-friendly messages rather than raw internal exception text.
- Validate user input and domain invariants before mutating state or persisting changes.
- Add a top-level exception boundary that logs fatal errors and exits cleanly.

### 7. Review persistence consistency and save semantics

Campaign saving is visible on explicit quit and player death, while standard adventure behavior differs. `PlayerHelper.SavePlayer` also modifies/sells inventory as part of saving.

**Why it matters:** Combining persistence with gameplay side effects makes the meaning of "save" surprising and can make data loss or duplication harder to diagnose.

**Suggested improvement:**
- Make saving a persistence-only operation.
- Move inventory liquidation or campaign settlement into an explicitly named gameplay operation.
- Define which modes persist characters and exactly when.
- Add tests for new-player insert, existing-player update, inventory preservation, and repeated saves.
- Consider transactional updates for related player/inventory records.

### 8. Make combat turn processing easier to reason about

`CombatHelper` combines turn scheduling, player choice, monster actions, skill timing, resource recovery, and action-message output.

**Suggested improvement:**
- Extract a turn scheduler and a combat-round coordinator.
- Represent combat actions/results explicitly rather than relying on side effects and accumulated action messages.
- Add tests for speed ties, player speed changes, monster death during a round, and cooldown/resource timing.
- Review the intended behavior when a support skill or consumable is followed by an optional attack so that action economy is consistent and documented.

### 9. Remove or formalize debug output

Several factories/gameplay paths write diagnostic details through `Debug.WriteLine`.

**Suggested improvement:**
- Replace ad hoc debug output with structured `ILogger<T>` calls.
- Use configurable log levels and avoid logging sensitive configuration.
- Keep normal gameplay output separate from diagnostics.
- Add context such as floor, room, entity identifier, and operation when useful.

### 10. Improve data model and EF Core consistency

`GameContext` has a mixture of nullable and non-nullable `DbSet` declarations. The model also uses lazy-loading proxies and several inheritance/discriminator mappings.

**Suggested improvement:**
- Use consistent non-nullable `DbSet<TEntity>` declarations.
- Review navigation-property nullability and `virtual` requirements for lazy loading.
- Add explicit indexes, uniqueness constraints, delete behaviors, and required-field rules where the domain requires them.
- Review cascade/restrict/set-null behavior for room connections, inventories, treasure, and player-room relationships.
- Add persistence tests for inheritance/discriminator mappings and relationship behavior.
- Consider whether eager loading or explicit query projections would be clearer than pervasive lazy loading.

### 11. Improve usability and accessibility of the console UI

The interface relies on numbered prompts and console colors.

**Suggested improvement:**
- Make menus resilient to empty collections and invalid input.
- Support cancellation consistently (for example, a documented back/cancel key).
- Avoid depending on color alone to convey success, warnings, or errors.
- Check terminal capabilities and degrade gracefully when color/clear operations are unsupported.
- Add concise contextual help and a command summary.

### 12. Add developer-facing documentation and operational safeguards

The repository would benefit from clearer setup and maintenance guidance beyond the user-facing README.

**Suggested improvement:**
- Add a short architecture decision record for the framework, database, and persistence choices.
- Document how to add a new item, skill, monster strategy, or migration.
- Add a database backup/reset guide for development.
- Add logging location/rotation guidance and ensure log directories are created reliably.
- Add a license, contribution guidelines, and issue templates if this is intended as a public project.

## Suggested improvement roadmap

| Priority | Work | Expected benefit |
|---|---|---|
| P0 | Align target frameworks/packages; document and secure database configuration | Reliable setup and fewer compatibility surprises |
| P0 | Add CI restore/build/test pipeline | Catch regressions on every change |
| P1 | Add integration tests for campaign, combat, persistence, and merchant flows | Confidence in end-to-end behavior |
| P1 | Separate save operations from gameplay side effects | Safer and more predictable persistence |
| P1 | Extract explicit session state from `AdventureService` | Easier testing and future feature work |
| P2 | Improve exception boundaries and structured logging | Better diagnostics and user experience |
| P2 | Review EF Core relationships and lazy-loading strategy | More predictable data access |
| P3 | Improve console accessibility and contributor documentation | Better usability and maintainability |

## Contributing

Contributions are welcome. A useful workflow is:

1. Open an issue describing the bug or proposed change.
2. Create a focused branch:

   ```bash
   git checkout -b feature/short-description
   ```

3. Make the change and add or update tests.
4. Run the solution build and test suite:

   ```bash
   dotnet build console-game-rpg.sln
   dotnet test console-game-rpg.sln
   ```

5. Submit a pull request describing the behavior change, test coverage, and any database migration implications.

For changes to EF Core entities or relationships, include the corresponding migration and explain whether existing data requires transformation.

## License

No license file was present in the supplied repository snapshot. Add a `LICENSE` file before distributing or accepting contributions under an open-source license.
