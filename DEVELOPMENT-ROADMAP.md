# Console Game RPG — Development Roadmap

**Working branch:** `architecture-refactor`  
**Stable branch:** `master`  
**Purpose:** Improve the architecture, reliability, maintainability, and test coverage of the RPG before reintroducing the expanded elemental and crafting systems.

Use `[x]` for completed tasks and `[ ]` for outstanding tasks.

---

# A. Foundation and Development Infrastructure

<badge color="success">Completed</badge>

## Step 1 — Align .NET and Dependencies

**Goal:** Establish a consistent, supported development environment.

- [x] Align all projects to .NET 10.
- [x] Align Entity Framework Core packages and tooling to version 10.0.14.
- [x] Address nullable reference warnings without suppressing them indiscriminately.
- [x] Move environment-specific database configuration out of tracked source files.
- [x] Verify environment-variable configuration overrides.
- [x] Confirm the full solution builds with zero warnings.

## Step 2 — Establish Automated Build and Testing

**Goal:** Ensure changes are automatically validated.

- [x] Create a GitHub Actions CI workflow.
- [x] Automate dependency restoration.
- [x] Automate Release builds.
- [x] Automate test execution.
- [x] Verify the workflow succeeds on GitHub.
- [x] Update GitHub Actions versions and runner configuration.
- [x] Confirm all 244 existing tests pass.

---

# B. Application Services and State Management

<badge color="info">P1 — High priority</badge>

## Step 3 — Improve AdventureService Architecture

**Goal:** Reduce the responsibilities of AdventureService and make gameplay orchestration easier to maintain and test.

- [x] Review the current AdventureService implementation.
- [x] Identify responsibilities that should belong to separate services.
- [x] Introduce an explicit GameSession or AdventureState object.
    - [x] Store player state.
    - [x] Store current floor and room.
    - [x] Store adventure mode and session status.
- [x] Separate floor progression from general adventure orchestration.
- [x] Separate room navigation and room resolution.
- [x] Separate loot and treasure resolution.
- [x] Separate campaign persistence from gameplay orchestration.
- [x] Add tests for session state transitions.
- [x] Add tests for floor progression and adventure completion.

**Completion criteria:** AdventureService coordinates gameplay without owning every detail of the gameplay process.

## Step 4 — Separate Persistence from Gameplay

**Goal:** Make saving predictable and prevent persistence operations from unexpectedly changing gameplay state.

- [x] Review PlayerHelper.SavePlayer().
- [x] Identify inventory modifications and other gameplay side effects occurring during saves.
- [x] Make saving a persistence-only operation.
- [x] Move inventory liquidation or campaign settlement into explicitly named gameplay operations.
- [x] Define when each game mode saves player progress.
- [x] Verify new-player insertion.
- [x] Verify existing-player updates.
- [x] Verify inventory preservation.
- [x] Verify repeated saves do not duplicate or remove data.
- [x] Add persistence integration tests.

**Completion criteria:** Saving a player never unexpectedly changes their inventory, equipment, or gameplay state.

Deferred: Persisting Weapon.Cursed across game sessions.

---

# C. Combat and Equipment Architecture

<badge color="info">P1 — High priority</badge>

## Step 5 — Extract Combat Architecture

**Goal:** Separate combat responsibilities and make the combat system easier to extend.

- [x] Review CombatHelper and existing combat-related classes.
- [x] Identify responsibilities currently combined in CombatHelper.
- [x] Extract a combat turn scheduler.
- [x] Extract a combat-round coordinator.
- [x] Introduce explicit combat action representations.
- [x] Introduce explicit combat result representations.
- [x] Separate combat decisions from console output.
- [x] Review player and monster turn ordering.
- [x] Review speed ties and speed changes during combat.
- [x] Review cooldown and resource recovery timing.
- [x] Add focused combat architecture tests.

**Completion criteria:** Combat turn processing is separated into understandable components with testable responsibilities.

Deferred improvements
These are intentionally outside Step 5:
1. Replace EquipmentHelper.UseConsumable()'s Boolean result with a more expressive result type.
2. Refine combat exception handling to distinguish expected gameplay failures from unexpected programming errors.

## Step 6 — Extract and Improve Equipment Architecture

**Goal:** Establish consistent equipment behavior and reduce coupling between equipment and combat.

- [ ] Review existing equipment, item, and inventory classes.
- [ ] Review how equipment modifies player and monster statistics.
- [ ] Separate equipment rules from combat execution.
- [ ] Review equip and unequip behavior.
- [ ] Review equipment restrictions and validation.
- [ ] Review equipment modification and upgrade behavior.
- [ ] Add tests for equipment changes and stat calculations.
- [ ] Ensure equipment behavior is consistent across game modes.

**Completion criteria:** Equipment changes are handled independently and produce predictable character statistics.

Deferred: 
- Equipment persistence: Save and restore equipped weapon and armor state across sessions. Revisit if players later need to resume directly in combat or preserve equipment state on load.
- Cursed equipment cannot be unequipped. The player must purify it to remove it.
- Cursed equipment doesn't lose durability. It remains usable until purified, so the player isn't punished by having it break and effectively escape the curse.


## Step 7 — Correct Damage Calculation and Death Handling

**Goal:** Make damage resolution consistent and ensure death is handled correctly.

- [ ] Review physical and magical damage calculations.
- [ ] Review player damage handling.
- [ ] Review monster damage handling.
- [ ] Review elemental damage handling.
- [ ] Correct damage-type inconsistencies.
- [ ] Ensure player death is detected consistently.
- [ ] Ensure monster death is detected consistently.
- [ ] Review damage mitigation and resistance calculations.
- [ ] Review dodge and critical-hit interactions.
- [ ] Add tests for damage boundaries and death conditions.

**Completion criteria:** All damage paths resolve consistently, and death handling cannot be bypassed by an individual attack type.

## Step 8 — Introduce a Reusable Damage Calculation System

**Goal:** Avoid duplicating damage-resolution rules across players, monsters, skills, and elemental attacks.

- [ ] Identify common damage calculation responsibilities.
- [ ] Design a shared damage calculation abstraction.
- [ ] Separate damage calculation from applying damage.
- [ ] Support damage types and resistance modifiers.
- [ ] Support critical hits and dodge calculations where appropriate.
- [ ] Support elemental modifiers.
- [ ] Return explicit damage calculation results.
- [ ] Integrate the system with player combat.
- [ ] Integrate the system with monster combat.
- [ ] Add comprehensive damage calculation tests.

**Completion criteria:** Physical, magical, and elemental damage use a consistent, extensible calculation model.

---

# D. Inventory, Runes, and Crafting Persistence

<badge color="info">P1 — High priority</badge>

## Step 9 — Design Persistent Rune and Ingredient Inventory

**Goal:** Ensure player-owned runes and ingredients are represented and persisted correctly.

- [ ] Review the current Inventory model.
- [ ] Review the Rune and Ingredient models.
- [ ] Review existing EF Core mappings.
- [ ] Distinguish shared item definitions from player-owned item instances.
- [ ] Design persistent rune ownership.
- [ ] Design persistent ingredient ownership.
- [ ] Define quantity and stacking rules.
- [ ] Define ownership and relationship constraints.
- [ ] Create and review EF Core migrations.
- [ ] Test saving and loading rune inventories.
- [ ] Test saving and loading ingredient inventories.
- [ ] Test inventory updates and deletion.

**Completion criteria:** Runes and ingredients persist correctly and cannot accidentally mutate shared item definitions.

## Step 10 — Reintroduce Crafting and Rune Progression

**Goal:** Build crafting on top of the improved inventory and persistence architecture.

- [ ] Review existing crafting and recipe implementations.
- [ ] Review rune fusion and progression rules.
- [ ] Separate recipe definitions from crafted item instances.
- [ ] Implement ingredient validation and consumption.
- [ ] Implement rune crafting.
- [ ] Implement rune fusion.
- [ ] Implement rune progression and tier rules.
- [ ] Validate resource costs and crafting outcomes.
- [ ] Add crafting and fusion tests.
- [ ] Add persistence tests for crafted items.

**Completion criteria:** Crafting and rune progression work reliably without duplicating, losing, or mutating shared item definitions.

---

# E. Elemental Gameplay Reintroduction

<badge color="info">P2 — Medium priority</badge>

## Step 11 — Reintroduce Elemental Damage and Status Effects

**Goal:** Bring elemental gameplay back using the new combat and damage architecture.

- [ ] Review the original elemental implementation.
- [ ] Review elemental types and their relationships.
- [ ] Reintroduce elemental damage.
- [ ] Reintroduce elemental resistance and vulnerability.
- [ ] Reintroduce elemental status effects.
- [ ] Review effect duration and expiration behavior.
- [ ] Review effect ticking and callback timing.
- [ ] Ensure effects interact correctly with player death and monster death.
- [ ] Add tests for each elemental type.
- [ ] Add tests for status effect application, ticking, and expiration.

**Completion criteria:** Elemental mechanics integrate with the shared damage and combat systems.

## Step 12 — Add Additional Elemental Gameplay Mechanics

**Goal:** Expand elemental interactions after the core systems are stable.

- [ ] Review elemental interactions and combinations.
- [ ] Add additional elemental skill mechanics.
- [ ] Add elemental equipment interactions.
- [ ] Add elemental resistance and vulnerability interactions.
- [ ] Review monster elemental behavior.
- [ ] Review player elemental behavior.
- [ ] Add balance and edge-case tests.

**Completion criteria:** Elemental gameplay is consistent across characters, monsters, equipment, and skills.

---

# F. Application Reliability and Data Access

<badge color="info">P2 — Medium priority</badge>

## Step 13 — Improve Error Handling and Dependency Injection

**Goal:** Make failures easier to diagnose and prevent silent application errors.

- [ ] Review broad exception handling.
- [ ] Catch expected domain exceptions at appropriate boundaries.
- [ ] Log unexpected exceptions with useful context.
- [ ] Replace GetService() with GetRequiredService() where appropriate.
- [ ] Review dependency registration and service lifetimes.
- [ ] Add clear database configuration validation.
- [ ] Ensure errors never expose connection strings or credentials.
- [ ] Add a top-level exception boundary.

**Completion criteria:** Errors are handled intentionally, logged appropriately, and presented to players in a user-friendly way.

## Step 14 — Introduce Structured Logging

**Goal:** Replace scattered diagnostic output with consistent application logging.

- [ ] Locate existing Debug.WriteLine() calls.
- [ ] Replace diagnostic output with ILogger<T>.
- [ ] Establish appropriate log levels.
- [ ] Separate diagnostic logging from normal console gameplay output.
- [ ] Add useful context to important operations.
- [ ] Review sensitive information in logs.
- [ ] Document logging configuration and output locations.

**Completion criteria:** Application diagnostics are consistent, configurable, and separate from player-facing messages.

## Step 15 — Review EF Core Data Model and Relationships

**Goal:** Improve persistence consistency and reduce unexpected database behavior.

- [ ] Review GameContext DbSet declarations.
- [ ] Review navigation-property nullability.
- [ ] Review lazy-loading proxy requirements.
- [ ] Review entity inheritance and discriminator mappings.
- [ ] Review cascade, restrict, and set-null behavior.
- [ ] Add appropriate indexes and uniqueness constraints.
- [ ] Review inventory and equipment relationships.
- [ ] Review room and campaign relationships.
- [ ] Add persistence integration tests.
- [ ] Review all generated migrations before applying them.

**Completion criteria:** Database relationships and persistence behavior are explicit, predictable, and covered by tests.

---

# G. Console Experience and Documentation

<badge color="info">P3 — Lower priority</badge>

## Step 16 — Improve Console UI and Input Handling

**Goal:** Make the game easier and safer to navigate.

- [ ] Review menu input validation.
- [ ] Handle empty collections safely.
- [ ] Standardize cancellation and back-navigation behavior.
- [ ] Improve contextual help.
- [ ] Avoid relying on color alone for important messages.
- [ ] Handle unsupported terminal color and clear operations.
- [ ] Review error and confirmation messages.

**Completion criteria:** Players can navigate menus reliably and recover from invalid input.

## Step 17 — Improve Developer Documentation

**Goal:** Make the project easier to understand, maintain, and contribute to.

- [ ] Document the overall architecture.
- [ ] Document service responsibilities.
- [ ] Document how to add a new item.
- [ ] Document how to add a new skill.
- [ ] Document how to add a monster and its behavior.
- [ ] Document database configuration.
- [ ] Document migration creation and application.
- [ ] Document development database backup and reset procedures.
- [ ] Add architecture decision records for major technical choices.
- [ ] Review license and contribution documentation.

**Completion criteria:** A developer can understand the architecture and make common changes without having to reverse-engineer the entire project.

---

# H. Final Verification and Integration

## Step 18 — Prepare Architecture Refactor for Master

**Goal:** Ensure the completed refactor is stable before merging into master.

- [ ] Review all completed roadmap items.
- [ ] Run a clean dependency restore.
- [ ] Build the complete solution in Release configuration.
- [ ] Run the complete test suite.
- [ ] Confirm GitHub Actions passes.
- [ ] Review database migrations and migration history.
- [ ] Verify database compatibility and existing data preservation.
- [ ] Review outstanding warnings and technical debt.
- [ ] Review README and setup instructions.
- [ ] Review the final branch diff against master.
- [ ] Merge architecture-refactor into master when ready.

**Completion criteria:** The architecture refactor is stable, tested, documented, and safe to integrate.

---

# Working Guidelines

1. Complete one step at a time whenever practical.
2. Understand existing behavior before changing architecture.
3. Avoid large rewrites without tests.
4. Run relevant tests after each meaningful change.
5. Keep Git commits focused and descriptive.
6. Do not merge incomplete architectural work into master.
7. Review database migrations carefully, especially when preserving existing data.
8. Update this roadmap as tasks are completed or priorities change.

**Current position:** Steps 1 and 2 are complete. Step 3 is the next task.

**Overall objective:** Establish a clean, maintainable RPG architecture that supports the original gameplay and makes future systems—especially elemental combat, runes, and crafting—easier to build and extend.
