# Console Game RPG --- Updated Development Roadmap

**Current working branch:** `architecture-refactor`\
**Stable branch:** `master`\
**Planned integration branch:** `elemental-refactor`

**Purpose:** Improve the architecture, reliability, maintainability, and
test coverage of the RPG, then consolidate the elemental, rune, and
crafting systems into a maintainable architecture.

Use `[x]` for completed tasks, `[ ]` for outstanding tasks, and `[~]`
for partially completed or deferred tasks.

------------------------------------------------------------------------

# A. Foundation and Development Infrastructure

## Step 1 --- Align .NET and Dependencies

**Status: Complete**

**Goal:** Establish a consistent, supported development environment.

-   [x] Align all projects to .NET 10.
-   [x] Align Entity Framework Core packages and tooling to version
    10.0.14.
-   [x] Address nullable reference warnings without suppressing them
    indiscriminately.
-   [x] Move environment-specific database configuration out of tracked
    source files.
-   [x] Verify environment-variable configuration overrides.
-   [x] Confirm the full solution builds with zero warnings.

**Completion criteria:** All projects use consistent dependencies and
build successfully without warnings.

## Step 2 --- Establish Automated Build and Testing

**Status: Complete**

**Goal:** Ensure changes are automatically validated.

-   [x] Create a GitHub Actions CI workflow.
-   [x] Automate dependency restoration.
-   [x] Automate Release builds.
-   [x] Automate test execution.
-   [x] Verify the workflow succeeds on GitHub.
-   [x] Update GitHub Actions versions and runner configuration.
-   [x] Confirm all 244 existing tests pass.

**Completion criteria:** Every relevant change is automatically built
and tested through CI.

------------------------------------------------------------------------

# B. Application Services and State Management

## Step 3 --- Improve AdventureService Architecture

**Status: Complete**

**Goal:** Reduce the responsibilities of AdventureService and make
gameplay orchestration easier to maintain and test.

-   [x] Review the current AdventureService implementation.
-   [x] Identify responsibilities that should belong to separate
    services.
-   [x] Introduce an explicit GameSession or AdventureState object.
    -   [x] Store player state.
    -   [x] Store current floor and room.
    -   [x] Store adventure mode and session status.
-   [x] Separate floor progression from general adventure orchestration.
-   [x] Separate room navigation and room resolution.
-   [x] Separate loot and treasure resolution.
-   [x] Separate campaign persistence from gameplay orchestration.
-   [x] Add tests for session state transitions.
-   [x] Add tests for floor progression and adventure completion.

**Completion criteria:** AdventureService coordinates gameplay without
owning every detail of the gameplay process.

## Step 4 --- Separate Persistence from Gameplay

**Status: Complete**

**Goal:** Make saving predictable and prevent persistence operations
from unexpectedly changing gameplay state.

-   [x] Review PlayerHelper.SavePlayer().
-   [x] Identify inventory modifications and other gameplay side effects
    occurring during saves.
-   [x] Make saving a persistence-only operation.
-   [x] Move inventory liquidation or campaign settlement into
    explicitly named gameplay operations.
-   [x] Define when each game mode saves player progress.
-   [x] Verify new-player insertion.
-   [x] Verify existing-player updates.
-   [x] Verify inventory preservation.
-   [x] Verify repeated saves do not duplicate or remove data.
-   [x] Add persistence integration tests.

**Completion criteria:** Saving a player never unexpectedly changes
their inventory, equipment, or gameplay state.

**Deferred:** Persisting `Weapon.Cursed` across game sessions.

------------------------------------------------------------------------

# C. Combat and Equipment Architecture

## Step 5 --- Extract Combat Architecture

**Status: Complete**

**Goal:** Separate combat responsibilities and make the combat system
easier to extend.

-   [x] Review CombatHelper and existing combat-related classes.
-   [x] Identify responsibilities currently combined in CombatHelper.
-   [x] Extract a combat turn scheduler.
-   [x] Extract a combat-round coordinator.
-   [x] Introduce explicit combat action representations.
-   [x] Introduce explicit combat result representations.
-   [x] Separate combat decisions from console output.
-   [x] Review player and monster turn ordering.
-   [x] Review speed ties and speed changes during combat.
-   [x] Review cooldown and resource recovery timing.
-   [x] Add focused combat architecture tests.

**Completion criteria:** Combat turn processing is separated into
understandable components with testable responsibilities.

**Deferred improvements:**

1.  Replace `EquipmentHelper.UseConsumable()`'s Boolean result with a
    more expressive result type.
2.  Refine combat exception handling to distinguish expected gameplay
    failures from unexpected programming errors.

## Step 6 --- Extract and Improve Equipment Architecture

**Status: Complete --- tested, committed, and pushed**

**Goal:** Establish consistent equipment behavior and reduce coupling
between equipment and combat.

-   [x] Review existing equipment, item, and inventory classes.
-   [x] Review how equipment modifies player and monster statistics.
-   [x] Separate equipment rules from combat execution.
-   [x] Review equip and unequip behavior.
-   [x] Review equipment restrictions and validation.
-   [x] Review equipment modification and upgrade behavior.
-   [x] Add tests for equipment changes and stat calculations.
-   [x] Ensure equipment behavior is consistent across game modes.

**Completion criteria:** Equipment changes are handled independently and
produce predictable character statistics.

**Deferred:**

-   Equipment persistence: Save and restore equipped weapon and armor
    state across sessions. Revisit if players later need to resume
    directly in combat or preserve equipment state on load.
-   Cursed equipment cannot be unequipped. The player must purify it to
    remove it.
-   Cursed equipment doesn't lose durability. It remains usable until
    purified, so the player isn't punished by having it break and
    effectively escape the curse.

## Step 7 --- Correct Damage Calculation and Death Handling

**Status: Complete --- committed**

**Goal:** Make damage resolution consistent and ensure death is handled
correctly.

-   [x] Review physical and magical damage calculations.
-   [x] Review player damage handling.
-   [x] Review monster damage handling.
-   [x] Review elemental damage handling.
-   [x] Correct damage-type inconsistencies.
-   [x] Ensure player death is detected consistently.
-   [x] Ensure monster death is detected consistently.
-   [x] Review damage mitigation and resistance calculations.
-   [x] Review dodge and critical-hit interactions.
-   [x] Add tests for damage boundaries and death conditions.

**Completion criteria:** All damage paths resolve consistently, and
death handling cannot be bypassed by an individual attack type.

**Deferred feature:** Critical-hit implementation.

## Step 8 --- Introduce a Reusable Damage Calculation System

**Status: Foundational implementation complete**

**Goal:** Avoid duplicating damage-resolution rules across players,
monsters, skills, and elemental attacks.

-   [x] Identify common damage calculation responsibilities.
-   [x] Design a shared damage calculation abstraction.
-   [x] Separate damage calculation from applying damage.
-   [x] Support damage types and resistance modifiers.
-   \[\~\] Support critical hits and dodge calculations where
    appropriate.
-   [ ] Support additional elemental modifiers.
-   [x] Return explicit damage calculation results.
-   [x] Integrate the system with player combat.
-   [x] Integrate the system with monster combat.
-   [x] Add comprehensive damage calculation tests.

**Completion criteria:** Physical, magical, and currently implemented
damage types use a consistent, extensible calculation model.

**Deferred feature work:** - Critical-hit chance and damage
multipliers. - Additional elemental modifiers and resistance
interactions.

------------------------------------------------------------------------

# D. Branch Integration, Inventory, Runes, and Crafting

## Step 9 --- Integrate Elemental Systems and Implement Persistent Rune and Ingredient Inventory

**Status: Not started**

**Goal:** Consolidate the stable architecture and elemental gameplay
into a single branch, then implement reliable persistence for
player-owned runes and ingredients, including crafting, fusion, and
equipment integration.

### Phase 1 --- Create the Integration Branch

-   [x] Verify `architecture-refactor` is committed and pushed.
-   [x] Verify `elemental-damage` is committed and pushed.
-   [x] Create `elemental-refactor` from `architecture-refactor`.
-   [ ] Identify the elemental damage, rune, ingredient, recipe, and
    crafting changes to bring over.
-   [ ] Integrate elemental functionality from `elemental-damage`.
-   [ ] Reconcile overlapping classes and resolve integration conflicts.
-   [ ] Build and test after each logical integration.
-   [ ] Verify existing architecture and gameplay functionality.

**Completion criteria:** `elemental-refactor` contains the stable
architecture and existing elemental damage, rune, ingredient, recipe,
and crafting functionality.

### Phase 2 --- Review Existing Rune and Ingredient Architecture

-   [ ] Review the Inventory model.
-   [ ] Review Rune, WeaponRune, ArmorRune, and Ingredient models.
-   [ ] Review Weapon and Armor rune relationships.
-   [ ] Review Player and EquipmentManager responsibilities.
-   [ ] Review CraftingHelper, Recipe, RecipeIngredient, and related
    DAOs.
-   [ ] Review existing EF Core mappings and migrations.
-   [ ] Identify shared database definitions versus player-owned
    instances.
-   [ ] Identify existing gameplay behavior that must be preserved.

**Completion criteria:** The existing rune and ingredient architecture,
including its persistence gaps and gameplay dependencies, is fully
understood.

### Phase 3 --- Design Persistent Ownership

-   [ ] Design the `InventoryRune` ownership entity.
-   [ ] Design the `InventoryIngredient` ownership entity.
-   [ ] Define rune identity using database Rune IDs.
-   [ ] Define player-specific rune tier progression.
-   [ ] Define ingredient and rune quantity rules.
-   [ ] Define rune stacking and uniqueness constraints.
-   [ ] Define equipment references to owned rune records.
-   [ ] Define how equipped runes reserve inventory quantities.
-   [ ] Define ownership and deletion constraints.
-   [ ] Ensure shared Rune and Ingredient definitions cannot be mutated
    by player progression.

**Completion criteria:** A reviewed ownership model supports persistent
quantities, player-specific rune progression, and equipped rune tracking
without modifying shared definitions.

### Phase 4 --- Implement EF Core Persistence

-   [ ] Add ownership entities and navigation properties.
-   [ ] Configure Inventory-to-InventoryRune relationships.
-   [ ] Configure Inventory-to-InventoryIngredient relationships.
-   [ ] Configure ownership-to-definition relationships.
-   [ ] Configure equipment-to-owned-rune relationships.
-   [ ] Define appropriate cascade and restrict delete behaviors.
-   [ ] Create and review EF Core migrations.
-   [ ] Verify generated database schema and indexes.
-   [ ] Test saving and loading rune inventories.
-   [ ] Test saving and loading ingredient inventories.
-   [ ] Test inventory updates and deletion.

**Completion criteria:** Rune and ingredient ownership persists
correctly through EF Core, with appropriate relationship and deletion
constraints.

### Phase 5 --- Update Inventory Operations

-   [ ] Replace the `[NotMapped]` rune dictionary with persistent
    ownership operations.
-   [ ] Replace the `[NotMapped]` ingredient dictionary with persistent
    ownership operations.
-   [ ] Implement adding and removing rune quantities.
-   [ ] Implement adding and removing ingredient quantities.
-   [ ] Validate positive quantities.
-   [ ] Prevent negative quantities and invalid ownership changes.
-   [ ] Preserve existing rune and ingredient stacking behavior where
    appropriate.
-   [ ] Update fuseable, destroyable, and transmutable inventory
    queries.
-   [ ] Ensure equipped rune quantities cannot be consumed or destroyed.

**Completion criteria:** Inventory operations consistently maintain
persistent ownership and valid quantities.

### Phase 6 --- Update Crafting and Rune Progression

-   [ ] Update rune crafting to consume persistent ingredient
    quantities.
-   [ ] Update rune fusion to consume four available copies.
-   [ ] Implement player-specific tier progression.
-   [ ] Implement rarity evolution without modifying shared Rune
    definitions.
-   [ ] Update rune destruction to return recovered ingredients.
-   [ ] Update essence transmutation to consume and award persistent
    quantities.
-   [ ] Preserve existing recipe requirements and crafting rules.
-   [ ] Ensure multi-record crafting operations are atomic.
-   [ ] Add focused crafting and progression tests.

**Completion criteria:** Crafting, fusion, destruction, and
transmutation correctly update persistent player-owned resources.

### Phase 7 --- Update Equipment and Rune Application

-   [ ] Update Weapon and Armor to reference owned rune records.
-   [ ] Preserve access to shared rune definitions.
-   [ ] Update EquipmentManager rune application.
-   [ ] Update rune replacement behavior.
-   [ ] Preserve the existing rule that replacing a rune destroys it and
    returns its ingredients, rather than returning the rune itself.
-   [ ] Validate rune compatibility with equipment.
-   [ ] Validate equipment ownership before rune application.
-   [ ] Prevent equipping more copies than the player owns.
-   [ ] Release reserved rune quantities when equipment is changed or
    removed.
-   [ ] Ensure equipment and inventory changes are atomic.

**Completion criteria:** Rune application and replacement consistently
respect player ownership, equipped quantities, and existing gameplay
rules.

### Phase 8 --- Update Combat Integration

-   [ ] Update WeaponRune effects to use the owned rune's tier.
-   [ ] Update ArmorRune effects to use the owned rune's tier.
-   [ ] Preserve existing elemental damage and status-effect
    calculations.
-   [ ] Verify rune effects remain correct after saving and loading.
-   [ ] Ensure shared Rune definitions remain unchanged during combat.
-   [ ] Add tests for equipped rune effects and player-specific tiers.

**Completion criteria:** Equipped rune effects use the correct
player-owned rune progression and remain consistent across game
sessions.

### Phase 9 --- Final Integration and Verification

-   [ ] Add SQLite or appropriate database integration tests for rune
    ownership.
-   [ ] Add integration tests for ingredient ownership.
-   [ ] Test crafting, fusion, destruction, and transmutation
    persistence.
-   [ ] Test applying, replacing, and removing equipped runes.
-   [ ] Test saving and reloading equipped rune relationships.
-   [ ] Verify multiple players cannot affect each other's rune or
    ingredient quantities.
-   [ ] Run the full Release build.
-   [ ] Run the full test suite.
-   [ ] Review migrations and database schema.
-   [ ] Commit and push the completed work.

**Completion criteria:** The consolidated architecture supports
persistent runes, ingredients, crafting, progression, and equipment
integration without shared-definition mutation or ownership
inconsistencies.

------------------------------------------------------------------------

# E. Crafting and Elemental Gameplay Improvements

## Step 10 --- Improve Crafting and Rune Progression Gameplay

**Status: Not started**

**Goal:** Improve the gameplay experience and balance of crafting now
that persistence and ownership are reliable.

-   [ ] Review crafting costs and progression balance.
-   [ ] Review rune tier and rarity evolution rules.
-   [ ] Review recipe discovery and presentation.
-   [ ] Review crafting restrictions and resource requirements.
-   [ ] Review rune fusion balance.
-   [ ] Improve crafting feedback and error handling.
-   [ ] Add additional crafting gameplay features where appropriate.
-   [ ] Add balance and edge-case tests.

**Completion criteria:** Crafting and rune progression provide
consistent, balanced gameplay on top of the persistent ownership system.

## Step 11 --- Complete and Improve Elemental Damage and Status Effects

**Status: Not started**

**Goal:** Complete elemental gameplay using the consolidated combat and
damage architecture.

-   [ ] Review elemental types and their relationships.
-   [ ] Review elemental damage calculations.
-   [ ] Review elemental resistance and vulnerability balance.
-   [ ] Review elemental status effects.
-   [ ] Review effect duration and expiration behavior.
-   [ ] Review effect ticking and callback timing.
-   [ ] Ensure effects interact correctly with player death and monster
    death.
-   [ ] Review elemental interactions and combinations.
-   [ ] Add additional elemental skill mechanics where appropriate.
-   [ ] Review monster elemental behavior.
-   [ ] Review player elemental behavior.
-   [ ] Add tests for each elemental type.
-   [ ] Add tests for status-effect application, ticking, and
    expiration.
-   [ ] Add balance and edge-case tests.

**Completion criteria:** Elemental mechanics integrate correctly with
shared damage, combat, equipment, and status-effect systems.

## Step 12 --- Implement Additional Combat Features

**Status: Not started**

**Goal:** Extend combat after the foundational damage and elemental
systems are stable.

-   [ ] Implement critical-hit chance and damage multipliers.
-   [ ] Review critical-hit interactions with dodge and damage
    mitigation.
-   [ ] Review additional damage modifiers and resistance interactions.
-   [ ] Review combat effect stacking and interactions.
-   [ ] Add focused tests for new combat mechanics.

**Completion criteria:** Additional combat mechanics extend the shared
damage system without introducing duplicated or inconsistent
calculations.

------------------------------------------------------------------------

# F. Application Reliability and Data Access

## Step 13 --- Improve Error Handling and Dependency Injection

**Status: Not started**

**Goal:** Make failures easier to diagnose and prevent silent
application errors.

-   [ ] Review broad exception handling.
-   [ ] Catch expected domain exceptions at appropriate boundaries.
-   [ ] Log unexpected exceptions with useful context.
-   [ ] Replace `GetService()` with `GetRequiredService()` where
    appropriate.
-   [ ] Review dependency registration and service lifetimes.
-   [ ] Add clear database configuration validation.
-   [ ] Ensure errors never expose connection strings or credentials.
-   [ ] Add a top-level exception boundary.

**Completion criteria:** Errors are handled intentionally, logged
appropriately, and presented to players in a user-friendly way.

## Step 14 --- Introduce Structured Logging

**Status: Not started**

**Goal:** Replace scattered diagnostic output with consistent
application logging.

-   [ ] Locate existing `Debug.WriteLine()` calls.
-   [ ] Replace diagnostic output with `ILogger<T>`.
-   [ ] Establish appropriate log levels.
-   [ ] Separate diagnostic logging from normal console gameplay output.
-   [ ] Add useful context to important operations.
-   [ ] Review sensitive information in logs.
-   [ ] Document logging configuration and output locations.

**Completion criteria:** Application diagnostics are consistent,
configurable, and separate from player-facing messages.

## Step 15 --- Review EF Core Data Model and Relationships

**Status: Not started**

**Goal:** Improve persistence consistency and reduce unexpected database
behavior.

-   [ ] Review GameContext DbSet declarations.
-   [ ] Review navigation-property nullability.
-   [ ] Review lazy-loading proxy requirements.
-   [ ] Review entity inheritance and discriminator mappings.
-   [ ] Review cascade, restrict, and set-null behavior.
-   [ ] Add appropriate indexes and uniqueness constraints.
-   [ ] Review inventory and equipment relationships.
-   [ ] Review room and campaign relationships.
-   [ ] Add persistence integration tests.
-   [ ] Review all generated migrations before applying them.

**Completion criteria:** Database relationships and persistence behavior
are explicit, predictable, and covered by tests.

------------------------------------------------------------------------

# G. Console Experience and Documentation

## Step 16 --- Improve Console UI and Input Handling

**Status: Not started**

**Goal:** Make the game easier and safer to navigate.

-   [ ] Review menu input validation.
-   [ ] Handle empty collections safely.
-   [ ] Standardize cancellation and back-navigation behavior.
-   [ ] Improve contextual help.
-   [ ] Avoid relying on color alone for important messages.
-   [ ] Handle unsupported terminal color and clear operations.
-   [ ] Review error and confirmation messages.

**Completion criteria:** Players can navigate menus reliably and recover
from invalid input.

## Step 17 --- Improve Developer Documentation

**Status: Not started**

**Goal:** Make the project easier to understand, maintain, and
contribute to.

-   [ ] Document the overall architecture.
-   [ ] Document service responsibilities.
-   [ ] Document how to add a new item.
-   [ ] Document how to add a new skill.
-   [ ] Document how to add a monster and its behavior.
-   [ ] Document database configuration.
-   [ ] Document migration creation and application.
-   [ ] Document development database backup and reset procedures.
-   [ ] Add architecture decision records for major technical choices.
-   [ ] Review license and contribution documentation.

**Completion criteria:** A developer can understand the architecture and
make common changes without having to reverse-engineer the entire
project.

------------------------------------------------------------------------

# H. Final Verification and Integration

## Step 18 --- Prepare the Consolidated Architecture for Master

**Status: Not started**

**Goal:** Ensure the completed refactor is stable before merging into
master.

-   [ ] Review all completed roadmap items.
-   [ ] Run a clean dependency restore.
-   [ ] Build the complete solution in Release configuration.
-   [ ] Run the complete test suite.
-   [ ] Confirm GitHub Actions passes.
-   [ ] Review database migrations and migration history.
-   [ ] Verify database compatibility and existing data preservation.
-   [ ] Review outstanding warnings and technical debt.
-   [ ] Review README and setup instructions.
-   [ ] Review the final branch diff against master.
-   [ ] Merge `elemental-refactor` into `master` when ready.

**Completion criteria:** The consolidated architecture is stable,
tested, documented, and safe to integrate into master.

------------------------------------------------------------------------

# Working Guidelines

1.  Complete one step at a time whenever practical.
2.  Understand existing behavior before changing architecture.
3.  Avoid large rewrites without tests.
4.  Run relevant tests after each meaningful change.
5.  Keep Git commits focused and descriptive.
6.  Do not merge incomplete architectural work into master.
7.  Review database migrations carefully, especially when preserving
    existing data.
8.  Update this roadmap as tasks are completed or priorities change.
9.  Preserve existing gameplay behavior unless a change is explicitly
    agreed upon.
10. Use `elemental-refactor` as the consolidated development branch once
    Step 9 Phase 1 is complete.

------------------------------------------------------------------------

# Current Progress

  Step                                              Status
  ------------------------------------------------- --------------------------------------------
  1\. Align .NET and Dependencies                   Complete
  2\. Automated Build and Testing                   Complete
  3\. AdventureService Architecture                 Complete
  4\. Persistence Separation                        Complete
  5\. Combat Architecture                           Complete
  6\. Equipment Architecture                        Complete --- tested, committed, and pushed
  7\. Damage and Death Handling                     Complete --- committed
  8\. Reusable Damage System                        Foundational work complete
  9\. Integration and Rune/Ingredient Persistence   Not started
  10--18                                            Not started

**Current position:** Step 9 --- Integrate Elemental Systems and
Implement Persistent Rune and Ingredient Inventory.

**Working branch:** `architecture-refactor` until the new
`elemental-refactor` branch is created.

**Overall objective:** Establish a clean, maintainable RPG architecture
that supports the original gameplay and provides a reliable foundation
for elemental combat, runes, crafting, and future gameplay systems.
