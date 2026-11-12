# Console Game RPG --- Updated Development Roadmap

**Current working branch:** `elemental-refactor`\
**Stable branch:** `master`\
**Previous branch:** ``architecture-refactor``

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

**Status: Complete**

**Goal:** Avoid duplicating damage-resolution rules across players,
monsters, skills, and elemental attacks.

-   [x] Identify common damage calculation responsibilities.
-   [x] Design a shared damage calculation abstraction.
-   [x] Separate damage calculation from applying damage.
-   [x] Support damage types and resistance modifiers.
-   [ ] Support critical hits and dodge calculations where
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

## Step 9 — Integrate Elemental Systems and Implement Persistent Rune and Ingredient Inventory

**Status: Complete**

**Working branch:** `elemental-refactor`

**Goal:** Consolidate the stable architecture and elemental gameplay into a single branch, establish reliable persistence for player-owned runes and ingredients, and integrate crafting, fusion, destruction, transmutation, and equipment interactions with the persistent ownership system.

### Phase 1 — Integrate Models and Update Mappings

Bring the required models and elemental functionality from `elemental-damage` into `elemental-refactor`. Reconcile overlapping classes and establish explicit EF Core relationships for rune and ingredient ownership.

- [x] Verify `architecture-refactor` is committed and pushed.
- [x] Verify `elemental-damage` is committed and pushed.
- [x] Create `elemental-refactor` from `architecture-refactor`.
- [x] Identify the elemental damage, rune, ingredient, recipe, and crafting changes to bring over.
- [x] Integrate elemental functionality from `elemental-damage`.
- [x] Reconcile overlapping classes and resolve integration conflicts.
- [x] Add ownership entities and navigation properties (`InventoryRune`, `InventoryIngredient`).
- [x] Configure all EF Core relationships (Inventory-to-ownership, ownership-to-definition, equipment-to-owned-rune).
- [x] Define cascade and restrict delete behaviors.
- [x] Build and test after each logical integration.
- [x] Verify existing architecture and gameplay functionality.

**Completion criteria:** `elemental-refactor` contains all required models, entities, and EF Core mappings for elemental damage, runes, ingredients, recipes, and ownership. The solution builds successfully and existing functionality remains intact.

### Phase 2 — Establish Migrations, Seed Data, and Verify Persistence

Create and apply EF Core migrations for the new ownership entities. Establish shared definition data for runes, ingredients, and recipes, then verify that player-owned inventory data persists correctly.

- [x] Create and review EF Core migrations.
- [x] Verify generated database schema and indexes.
- [x] Add seed data for shared Rune definitions.
- [x] Add seed data for shared Ingredient definitions.
- [x] Add seed data for Recipe and RecipeIngredient definitions.
- [x] Test saving and loading rune inventories.
- [x] Test saving and loading ingredient inventories.
- [x] Test inventory updates and deletion.
- [x] Verify ownership quantities remain correct after database reload.
- [x] Verify equipped rune relationships persist correctly.
- [x] Verify deleting or modifying ownership records does not unintentionally delete shared definitions.
- [x] Add persistence integration tests covering new and existing players.

**Completion criteria:** Rune and ingredient ownership persists correctly through EF Core. Shared definitions are seeded, ownership relationships behave as intended, and persistence operations are covered by integration tests.

### Phase 3 — Implement Persistent Crafting, Fusion, Destruction, and Progression

Replace the existing in-memory inventory implementations with persistent ownership operations. Update all rune and ingredient gameplay systems to use the new persistence architecture while preserving existing crafting and progression rules.

- [x] Implement persistent equipped weapon and armor relationships.
- [x] Update equipment operations to persist equipping and unequipping.
- [x] Verify equipped weapons and armor survive database reloads.
- [x] Implement adding and removing rune and ingredient quantities.
- [x] Validate positive quantities and prevent invalid ownership changes.
- [x] Prevent negative inventory quantities and remove or clean up empty ownership records consistently.
- [x] Update fuseable, destroyable, and transmutable inventory queries.
- [x] Ensure equipped rune quantities cannot be consumed or destroyed.
- [x] Update rune crafting to consume persistent ingredient quantities.
- [x] Update rune fusion to consume three available copies.
- [x] Implement player-specific tier progression and rarity evolution.
- [x] Update rune destruction to return recovered ingredients.
- [x] Update essence transmutation to consume and award persistent quantities.
- [x] Ensure multi-record crafting operations are atomic.
- [x] Preserve existing recipe requirements and crafting rules.
- [x] Add focused crafting and progression tests.
- [x] Verify crafting, fusion, destruction, and transmutation against database-reloaded inventories.

**Completion criteria:** All rune and ingredient inventory operations use persistent ownership. Crafting, fusion, destruction, and transmutation correctly update player-owned quantities, preserve equipped runes, enforce existing gameplay rules, and maintain data integrity through atomic operations and automated tests.

**Step 9 Completion criteria:** The elemental systems are integrated into `elemental-refactor`, database persistence is verified, and all rune and ingredient gameplay operates through the persistent ownership architecture.

------------------------------------------------------------------------

# E. Crafting and Elemental Gameplay Improvements

## Step 10 — Improve Crafting and Rune Progression Gameplay

**Status: Complete**

**Goal:** Improve the gameplay experience and balance of crafting now that persistence and ownership are reliable.

### A. Restore Monster Ingredient Rewards

- [x] Review legacy MonsterFactory elemental assignment.
- [x] Restore randomized monster elemental affinity.
- [x] Ensure every monster receives a valid elemental affinity.
- [x] Restore ingredient drop calculations in LootService.
- [x] Retrieve potential drops using monster element and threat level.
- [x] Preserve all-element essence drop availability.
- [x] Preserve element-specific core and monster-part drops.
- [x] Restore monster-level drop bonuses with probability limits.
- [x] Award ingredients through the persistent IngredientDao.
- [x] Integrate ingredient rewards with existing monster treasure rewards.
- [x] Add monster factory and loot integration tests.

### B. Review Crafting Economy and Progression

- [x] Analyze ingredient drop rates against monster threat levels.
- [x] Evaluate ingredient acquisition rates during player progression.
- [x] Review crafting costs and progression balance.
- [x] Review essence rarity progression and transmutation.
- [x] Review core and monster-part availability.
- [x] Review rune tier and rarity evolution rules.
- [x] Review rune fusion balance.
- [x] Review rune destruction and ingredient refunds.

### C. Validate Crafting Rules and Restrictions

- [x] Validate recipe definitions and ingredient requirements.
- [x] Prevent crafting from malformed or ingredient-free recipes.
- [x] Review resource ownership and quantity requirements.
- [x] Review equipped rune restrictions.
- [x] Review crafting, fusion, and destruction transaction behavior.
- [x] Ensure failed operations do not partially modify inventory.

### D. Improve Recipe Discovery and Presentation

- [x] Review existing recipe discovery infrastructure.
- [x] Implement a dedicated crafting menu.
- [x] Display available recipes and requirements.
- [x] Show missing ingredients and quantities.
- [x] Allow players to initiate crafting through the menu.
- [x] Display crafting results and resource changes.

### E. Improve Crafting Feedback and Gameplay

- [x] Improve crafting feedback and error handling.
- [x] Provide player-friendly failure messages.
- [x] Improve fusion and destruction feedback.
- [x] Consider additional crafting gameplay features where appropriate.
- [x] Add balance and edge-case tests.
- [x] Run the complete test suite.

**Completion criteria:** Monster ingredient acquisition, crafting, fusion, transmutation, and destruction provide 
consistent, balanced gameplay on top of the persistent ownership system. Players can discover recipes, 
understand their requirements, and receive clear feedback when performing crafting operations.

# Step 11 — Complete and Improve Elemental Damage and Status Effects

**Status:** In Progress
**Design baseline:** `Elemental Mechanics Specification` and `C0.2.2 — Active Effect Architecture Design (Final)`
**Current branch:** `elemental-refactor`

## Goal

Complete elemental gameplay using the consolidated combat and damage architecture.

Elemental damage, resistance, vulnerability, weapon and armor rune effects, monster elemental attacks, and status effects must integrate correctly with the shared damage, combat, equipment, and effect systems.

Status effects must behave deterministically across combat turns, including ticking, expiration, stacking, refreshing, and death handling.

The implementation must preserve the existing shared damage architecture. Do not restore the legacy `EffectManager` architecture wholesale.

---

# A. Review and Define Elemental Mechanics

**Status:** Complete

### A1. Review Elemental Types and Relationships

- [x] Review elemental types and their relationships.
- [x] Review elemental damage calculations.
- [x] Review elemental resistance and vulnerability balance.
- [x] Define elemental damage multipliers and mitigation order.

### A2. Review Elemental Status Effects

- [x] Review elemental status effects.
- [x] Define the six elemental status-effect behaviors.
- [x] Review effect duration and expiration behavior.
- [x] Define status-effect stacking, refreshing, and replacement rules.
- [x] Review effect ticking and callback timing.

### A3. Review Elemental Interactions

- [x] Review elemental interactions and combinations.
- [x] Establish elemental combinations as intended gameplay.
- [x] Defer concrete reaction pairings, effects, and balance values to Phase D.

**Phase A acceptance criteria:**

- Elemental damage and mitigation behavior is documented.
- All six elemental status effects have defined behavior.
- Effect duration, ticking, stacking, and refreshing rules are established.
- Elemental combinations are identified as future gameplay work without introducing unapproved reactions.

**Phase A exit condition:** The Elemental Mechanics Specification is approved and serves as the gameplay baseline for implementation.

---

# B. Integrate Elemental Damage into Combat

**Status:** Complete

**Objective:** Integrate elemental damage into the existing shared combat architecture for both players and monsters.

### B1. Consolidate Monster Elemental Properties

- [x] Review and consolidate monster elemental properties restored during Step 10.
- [x] Keep `AttackElement` independent from vulnerability and resilience.
- [x] Maintain explicit elemental vulnerability and resilience data.
- [x] Ensure attack element does not implicitly determine elemental resilience.
- [x] Ensure vulnerability and resilience are independently represented.

**Acceptance:** Monster elemental properties are independently represented and integrated into the current model.

### B2. Integrate Shared Elemental Damage Resolution

- [x] Ensure elemental attacks use the shared damage architecture.
- [x] Establish elemental damage as a distinct damage channel.
- [x] Apply the approved affinity multipliers:
    - Neutral: 1.0x
    - Vulnerable: 1.5x
    - Resistant: 0.5x
- [x] Apply affinity multipliers before matching armor-rune mitigation.
- [x] Truncate fractional damage after applying the affinity multiplier.
- [x] Clamp final elemental damage to zero.
- [x] Preserve normal health and death handling.

**Acceptance:** Elemental damage resolves independently from conventional defense calculations while using the established health and death handling.

### B3. Connect Weapon Rune Elemental Damage to Player Attacks

- [x] Connect weapon rune elemental damage to player attacks.
- [x] Calculate immediate elemental damage using `Power * Tier`.
- [x] Apply elemental damage only after a successful attack.
- [x] Keep elemental damage independent from the probabilistic status-effect roll.
- [x] Ensure dodged attacks do not apply elemental damage or on-hit effects.

**Acceptance:** Equipped weapon runes contribute elemental damage to successful player attacks.

### B4. Integrate Elemental Damage with Boss Phase Transitions

- [x] Integrate elemental damage with boss phase transitions.
- [x] Preserve existing boss damage and phase-transition behavior.
- [x] Ensure elemental damage cannot bypass boss death handling.
- [x] Preserve effects across boss phase transitions.

**Acceptance:** Elemental damage respects existing boss mechanics and phase transitions.

### B5. Integrate Monster Elemental Attacks

- [x] Integrate monster elemental attacks into monster combat strategies.
- [x] Review monster elemental behavior.
- [x] Review player elemental behavior.
- [x] Treat monster elemental damage as a secondary component of its normal attack.
- [x] Ensure a successful dodge avoids both conventional and elemental components.
- [x] Preserve existing monster strategy-specific behavior.

**Acceptance:** Monster elemental attacks use the shared combat architecture without introducing independent attacks or duplicate dodge decisions.

### B6. Review and Implement Monster Elemental-Power Scaling

- [x] Review the current `MonsterFactory` elemental-power assignment.
- [x] Replace the current random 1–2 elemental damage range.
- [x] Establish a deterministic, level- and threat-aware scaling formula.
- [x] Keep elemental power proportionate to ordinary attack power and combat difficulty.
- [x] Update `MonsterFactory`.
- [x] Replace tests asserting only the old 1–2 range.
- [x] Test scaling across monster levels and threat categories.

**Acceptance:** Monster elemental power scales consistently with monster difficulty.

### B7. Phase B Integration Verification

- [x] Verify player elemental damage.
- [x] Verify monster elemental damage.
- [x] Verify independent vulnerability and resilience.
- [x] Verify combined conventional and elemental attacks.
- [x] Verify dodge behavior.
- [x] Verify boss phase transitions.
- [x] Verify death handling.
- [x] Run focused and relevant regression tests.

**Phase B exit condition:** Elemental damage works throughout player and monster combat, and monster elemental-power scaling is implemented and tested.

---

# C. Integrate Elemental Equipment and Status Effects

**Status:** In Progress

**Objective:** Integrate elemental equipment effects and status effects through a unified active-effect architecture.

This phase establishes `ActiveEffectManager` as the authoritative source for active effects and temporary combat modifiers.

## C1. Introduce the Active-Effect Architecture

- [x] Implement the `ActiveEffect` model.
- [x] Define effect identity and type.
- [x] Define effect source information, including source skill and caster references where applicable.
- [x] Define effect Power, Duration, and RemainingDuration.
- [x] Implement `ActiveEffectManager`.
- [x] Ensure each `ITargetable` owns an `ActiveEffectManager`.
- [x] Implement effect registration and lookup.
- [x] Implement duplicate prevention.
- [x] Implement approved stacking and refresh behavior.
- [x] Preserve original effect identity and source ownership when stronger DoTs refresh existing effects.
- [x] Define removal-result contracts for expiration and clearing operations.

**Acceptance:** Each combatant has one authoritative store for active effects, with deterministic identity, stacking, refreshing, and removal behavior.


## C2. Integrate Temporary Combat Modifiers
- [ ] Route temporary stat modifications through `ActiveEffectManager`.
- [ ] Implement effective-stat calculations using active modifiers.
- [ ] Integrate Charred with effective defense.
- [ ] Integrate Shocked with effective resilience.
- [ ] Integrate Frozen with healing received.
- [ ] Integrate Snared with effective speed.
- [ ] Integrate Blinded with direct incoming damage.
- [ ] Integrate Corrupted with outgoing damage.
- [ ] Preserve base stats.
- [ ] Enforce supported minimum values for derived stats.

**Status-effect behavior:**

| Element   | Status    | Behavior                                       |
| --------- | --------- | ---------------------------------------------- |
| Fire      | Charred   | Reduces effective defense by effect Power.     |
| Lightning | Shocked   | Reduces effective resilience by effect Power.  |
| Ice       | Frozen    | Reduces healing received by effect Power.      |
| Nature    | Snared    | Reduces effective Speed by effect Power.       |
| Radiance  | Blinded   | Increases damage received from direct attacks. |
| Abyssal   | Corrupted | Reduces damage dealt by the affected source.   |

**Important damage interaction rules:**

- Blinded applies to direct attacks only, not DoT.
- Blinded's direct-damage bonus is split between standard and elemental components.
- The standard component receives the odd extra point.
- Each bonus component is applied before its corresponding mitigation.
- Do not additionally apply a generic post-resolution Blinded bonus.
- Corrupted reduces outgoing damage and must also be evaluated for DoT ticks using the original DoT source's current modifier.

**Acceptance:** Status effects modify combat calculations without permanently mutating base stats or creating parallel modifier storage.

## C3. Replace Existing Active Modifier Dictionaries

The introduction of `ActiveEffectManager` replaces the existing active-stat-modifier dictionaries on `Player` and `Monster`.

The implementation must not retain those dictionaries as parallel storage or maintain compatibility synchronization between them and the new effect system.

### 1. Remove Existing Modifier Dictionaries

- [ ] Remove the existing `Dictionary<StatType, int>` active-modifier storage from `Player` and `Monster`.
- [ ] Do not retain these dictionaries as compatibility layers, caches, or secondary sources of truth.
- [ ] Ensure all active stat modifiers are represented by registered `ActiveEffect` instances managed by the target's `ActiveEffectManager`.

### 2. Make ActiveEffectManager the Authoritative Effect Store

- [ ] Ensure each `ITargetable` owns an `ActiveEffectManager`.
- [ ] Make the manager authoritative for active stat modifiers, elemental statuses, and DoT effects.
- [ ] Ensure temporary support-skill modifiers and elemental status effects use the same infrastructure.
- [ ] Ensure effect aggregation and stacking are handled according to approved effect categories and stacking rules.

### 3. Preserve Existing Stat Calculation Responsibilities

`GetStat()` must continue combining the existing stat sources:

1. Base stat.
2. Equipment modifiers.
3. Archetype modifiers.
4. Active-effect modifiers obtained through `Effects.GetStatModifier()`.

- [ ] Preserve base-stat ownership outside `ActiveEffectManager`.
- [ ] Preserve equipment-stat ownership outside `ActiveEffectManager`.
- [ ] Preserve archetype-stat ownership outside `ActiveEffectManager`.
- [ ] Preserve existing stat clamping and established stat-calculation behavior.

### 4. Migrate Support-Skill Modifiers

- [ ] Migrate support skills that apply temporary stat modifiers to register corresponding `ActiveEffect` instances.
- [ ] Retain the source skill instance on each effect.
- [ ] Ensure effects can be identified and cleared by their originating skill.
- [ ] Ensure expiration removes effects through `ActiveEffectManager`.
- [ ] Do not reverse expired modifiers through opposite `ModifyStat()` operations.
- [ ] Preserve SupportSkill per-target duplicate-application protections.
- [ ] Keep target tracking separate from effect Power, Duration, and expiration state.

**Target-tracking lifecycle:**

- `SupportSkill._targets` is a historical cleanup list, not a representation of currently active effects.
- Retain previously affected targets until the skill's `Reset()` lifecycle runs.
- `Reset()` may call `ClearBySource(this)` for targets whose effects have already expired.
- `ClearBySource` must safely do nothing when no matching effects remain.
- Duplicate-application checks must inspect actual active effects rather than relying solely on historical target tracking.

### 5. Remove Duplicate Effect-Lifecycle Responsibilities

- [ ] Make `ActiveEffectManager` responsible for registration, aggregation, duration, stacking, and expiration.
- [ ] Keep `CombatRoundCoordinator` responsible for scheduling appropriate effect advancement.
- [ ] Ensure `SupportSkill` does not independently decrement or expire managed effects.
- [ ] Keep `SupportSkill.UpdateElapsedTime()` responsible for skill cooldown progression, not active-effect duration.
- [ ] Ensure clearing effects by source or clearing all effects operates through `ActiveEffectManager`.

### 6. Define Effect Removal Contracts

- [ ] Make natural expiration expose the removed effect instances.
- [ ] Make `ClearBySource` expose the removed effect instances.
- [ ] Make `ClearAll` expose the removed effect instances.
- [ ] Define the result contracts for `TickRoundBased`, `TickTurnBased`, and `TickDoT`.
- [ ] Distinguish processed ticks from effects that have expired or been removed.
- [ ] Keep callbacks and notifications outside `ActiveEffectManager`.

### Acceptance Criteria

- Neither `Player` nor `Monster` retains a separate active-stat-modifier dictionary.
- No code path reads or writes the removed dictionaries.
- `GetStat()` correctly includes active modifiers through `ActiveEffectManager`.
- Support-skill modifiers and elemental status modifiers coexist in the same effect infrastructure.
- Applying, refreshing, replacing, and expiring effects does not require synchronizing separate modifier stores.
- Expiration does not accidentally reverse a modifier twice or alter unrelated stat sources.
- Existing support-skill duplicate-application protections remain effective.
- Tests cover stat aggregation, effect application, stacking, expiration, and cleanup.

## C4. Connect Weapon Rune Status-Effect Application to Combat

- [ ] Connect weapon rune status-effect application to successful player attacks.
- [ ] Preserve the existing rarity-based status-effect probabilities:
    - Common: 1%
    - Uncommon: 3%
    - Rare: 8%
    - Epic: 12%
    - Legendary: 20%
    - Mythic: 35%
- [ ] Calculate status Power using `(int)Rarity + Tier`.
- [ ] Use the rune's configured Duration.
- [ ] Keep immediate elemental damage independent of the status-effect roll.
- [ ] Keep DoT application independent of the probabilistic status-effect roll.
- [ ] Ensure dodged attacks do not apply status effects.
- [ ] Add tests for status-effect application and probability boundaries.

**Acceptance:** Weapon runes independently apply immediate elemental damage, guaranteed DoT, and probabilistic status effects on successful hits.

## C5. Implement Damage-over-Time Lifecycle

- [ ] Implement DoT registration through `ActiveEffectManager`.
- [ ] Tick DoT at the beginning of the affected combatant's turn.
- [ ] Ensure newly applied DoT never ticks immediately.
- [ ] Ensure a duration of three produces three scheduled ticks.
- [ ] Decrement RemainingDuration only after a scheduled tick has been processed.
- [ ] Consume duration even when mitigation reduces tick damage to zero.
- [ ] Expire the effect after its final scheduled tick.
- [ ] Implement stronger/equal/weaker refresh behavior.
- [ ] Preserve original source ownership during stronger refreshes.
- [ ] Stop processing subsequent effects if a DoT tick kills its target.
- [ ] Clear effects through the normal death and cleanup paths.
- [ ] Route DoT damage through `ITargetable.ApplyElementalDamage()`.

**DoT damage rules:**

- Corrupted applies dynamically using the original source's current modifier.
- Blinded does not apply to DoT.
- DoT does not use the ordinary attack pipeline.
- DoT does not trigger dodge or armor-durability handling.
- DoT must preserve normal health, death, and boss-phase-transition behavior.

**Acceptance:** DoT timing, damage, ownership, and expiration are deterministic and independently tested.

## C6. Integrate Elemental Armor Mitigation

**Status:** Armor rune power is already connected to elemental damage mitigation. Verify that the implementation conforms to the approved mechanics.

- [x] Connect armor rune power to elemental damage mitigation.
- [ ] Verify that all equipped matching armor runes contribute their `Use(tier)` values.
- [ ] Verify that nonmatching armor runes contribute nothing.
- [ ] Verify that affinity multipliers are applied before armor mitigation.
- [ ] Verify that matching armor mitigation is summed.
- [ ] Verify that final elemental damage is clamped to zero.
- [ ] Verify that mitigation cannot cause healing.
- [ ] Test multiple matching runes and nonmatching runes.
- [ ] Verify mitigation for both player and monster targets.

**Acceptance:** Armor runes mitigate elemental damage consistently without causing negative damage or healing.

## C7. Implement Effect Scheduling and Lifecycle Cleanup

### Effect Scheduling

- [ ] Integrate effect ticking with `CombatRoundCoordinator`.
- [ ] Implement round-based and turn-based effect advancement.
- [ ] Integrate DoT processing at the beginning of the affected combatant's turn.
- [ ] Ensure non-DoT turn-based effects decrement according to their defined end-of-turn lifecycle.
- [ ] Ensure DoT duration is consumed exclusively by scheduled ticks.

### Round-Based Speed Scheduling

- [ ] Update `CombatTurnScheduler` to capture effective Speed for every living combatant at the beginning of each round.
- [ ] Use captured Speed values for the entire round.
- [ ] Ensure Speed changes during a round do not reorder remaining turns.
- [ ] Ensure Speed changes affect turn order beginning with the next round.
- [ ] Test Speed changes during a round.
- [ ] Test Speed-effect expiration during a round.

### Death and Encounter Cleanup

- [ ] Clear effects when combatants die.
- [ ] Stop processing effects for dead combatants.
- [ ] Clear player and monster effects when an encounter terminates.
- [ ] Use guaranteed cleanup, including when combat exits through exceptions.
- [ ] Preserve effects across boss phase transitions.
- [ ] Clear player effects during level-up as specified.
- [ ] Ensure historical SupportSkill target tracking cannot cause effects to survive encounter cleanup.
- [ ] Verify that `Reset()` safely clears previously affected targets.

**Acceptance:** Effects cannot leak between encounters, survive death, or produce inconsistent turn scheduling.

## C8. Preserve Dedicated Health Modification Paths

- [ ] Keep healing routed through `Heal()`.
- [ ] Keep direct health damage routed through its dedicated damage path.
- [ ] Do not use `ModifyStat(Health, negativeAmount)` as a substitute for direct health damage.
- [ ] Ensure any retained Health compatibility behavior rejects or explicitly handles negative values.
- [ ] Preserve death and boss-phase-transition handling for direct health damage.
- [ ] Ensure health-affecting SupportSkills distinguish healing from direct health damage and life drain.

**Acceptance:** Health modification does not bypass established damage, healing, or death behavior.

## C9. Phase C Integration Verification

- [ ] Test all six elemental status effects.
- [ ] Test all six elemental DoT types.
- [ ] Test status-effect application.
- [ ] Test stacking and refresh behavior.
- [ ] Test original DoT source ownership.
- [ ] Test Corrupted and Blinded interactions.
- [ ] Test armor mitigation.
- [ ] Test duration and expiration.
- [ ] Test a duration-three DoT applied during the target's turn produces three ticks.
- [ ] Test damage reduced to zero by mitigation still consumes a DoT tick.
- [ ] Test death during DoT processing.
- [ ] Test boss phase transitions.
- [ ] Test encounter cleanup and exceptions.
- [ ] Test SupportSkill cleanup.
- [ ] Test Speed snapshot scheduling.
- [ ] Run focused and integration test suites.

**Phase C exit condition:** Weapon rune status effects, armor mitigation, active-effect storage, temporary modifiers, DoT, scheduling, and cleanup are fully integrated.

---

# D. Expand Elemental Gameplay

**Status:** Pending

**Objective:** Expand the elemental system through skill mechanics and elemental combinations using the established shared effect and damage architecture.

## D1. Review Existing Elemental Skill Mechanics

- [ ] Review existing elemental skill models and implementations.
- [ ] Identify skills that interact with elemental effects.
- [ ] Define how skills apply, consume, modify, or remove effects.
- [ ] Define health-affecting SupportSkill behavior explicitly.
- [ ] Distinguish healing, direct health damage, and life drain.
- [ ] Preserve the shared damage and active-effect architecture.

## D2. Define Elemental Combinations

- [ ] Define approved elemental pairings.
- [ ] Define reaction triggering conditions.
- [ ] Define reaction effects and balance values.
- [ ] Establish whether reactions consume or preserve existing effects.
- [ ] Establish rules preventing unintended reaction chains.
- [ ] Document the approved reaction mechanics.

## D3. Implement Elemental Skills and Reactions

- [ ] Implement approved elemental skill mechanics.
- [ ] Implement approved elemental combinations.
- [ ] Route effects through `ActiveEffectManager`.
- [ ] Route damage through the established shared damage architecture.
- [ ] Add deterministic tests for each skill and reaction.

## D4. Validate Skill and Reaction Interactions

- [ ] Test interactions with existing status effects.
- [ ] Test interactions with DoT.
- [ ] Test interactions with bosses.
- [ ] Test interactions with effect expiration and refresh.
- [ ] Test interactions with elemental armor mitigation.
- [ ] Verify that skills cannot bypass normal damage or death handling.
- [ ] Verify that reactions do not produce unintended chains.

**Phase D exit condition:** Elemental skills and combinations work through the established damage and active-effect architecture.

---

# E. Testing and Balance

**Status:** Pending

**Objective:** Verify that the complete elemental system is balanced, understandable, and reliable.

## E1. Balance Elemental Damage

- [ ] Review player elemental damage across rune rarities and tiers.
- [ ] Review monster elemental power across levels and threat.
- [ ] Review vulnerability and resilience multipliers.
- [ ] Review armor-rune mitigation.
- [ ] Review the interaction between conventional and elemental damage.
- [ ] Check for disproportionate damage scaling.

## E2. Balance Status Effects

- [ ] Review status-effect power progression.
- [ ] Review status-effect probabilities.
- [ ] Review duration and refresh behavior.
- [ ] Evaluate the relative value of all six effects.
- [ ] Review DoT damage and tick frequency.
- [ ] Review the effectiveness of Corrupted and Blinded.
- [ ] Adjust balance values only through deliberate, tested decisions.

## E3. Improve Gameplay Feedback

- [ ] Improve elemental damage messages.
- [ ] Improve status-effect application messages.
- [ ] Improve status-effect resistance and failure feedback.
- [ ] Improve DoT tick and expiration messages.
- [ ] Improve elemental skill and reaction feedback.
- [ ] Ensure messages explain gameplay outcomes without exposing implementation details.

## E4. Add Edge-Case and Regression Tests

- [ ] Test zero-damage elemental hits.
- [ ] Test damage overkill and death.
- [ ] Test multiple simultaneous effects.
- [ ] Test repeated effect refreshes.
- [ ] Test effects expiring during combat.
- [ ] Test boss phase transitions during elemental damage.
- [ ] Test encounter interruption and exceptions.
- [ ] Test level-up cleanup.
- [ ] Test Speed changes during combat.
- [ ] Test DoT application and expiration around turn boundaries.
- [ ] Test the complete combat and crafting regression suites.

## E5. Final Verification

- [ ] Build the complete solution.
- [ ] Run all relevant unit tests.
- [ ] Run integration tests.
- [ ] Resolve regressions.
- [ ] Review the final implementation against `C0.2.2 — Active Effect Architecture Design (Final)`.
- [ ] Review the final implementation against the `Elemental Mechanics Specification`.
- [ ] Verify that all approved reconciliation decisions are implemented.
- [ ] Update the Step 11 results document.
- [ ] Commit the completed work.

**Phase E exit condition:** The elemental system is implemented, balanced, tested, and documented.

---

# Implementation Workflow

Execute implementation work in small, reviewable increments.

For each increment:

1. Inspect the relevant current-branch implementation and tests.
2. Identify the exact files, interfaces, and dependencies affected.
3. Agree on the smallest coherent implementation change.
4. Implement the change.
5. Run focused tests.
6. Review the result against the design documents.
7. Run relevant regression tests.
8. Commit at meaningful milestones.

## Phase Boundaries

| Phase | Responsibility                                   | Must not absorb                                |
| ----- | ------------------------------------------------ | ---------------------------------------------- |
| A     | Review and define elemental mechanics            | Implementation work                            |
| B     | Integrate elemental damage into combat           | Status lifecycle, new reactions                |
| C     | Integrate elemental equipment and status effects | New elemental reactions or skill combinations  |
| D     | Expand elemental gameplay                        | Unapproved changes to core damage architecture |
| E     | Testing and balance                              | New mechanics disguised as balance adjustments |

## Implementation Starting Point

Continue with **Phase B6 — Review and Implement Monster Elemental-Power Scaling**, unless subsequent branch inspection establishes that it has already been completed.

After Phase B is verified, proceed to **Phase C1 — Introduce the Active-Effect Architecture**.

The current `elemental-refactor` branch is the implementation source of truth. The obsolete `elemental-damage-implementation-TOBEDELETED` folder must not be restored or treated as the target architecture.

## Governing Design Decisions

The following decisions are authoritative throughout implementation:

- `ActiveEffectManager` is the sole authoritative store for active effects and temporary modifiers.
- DoT duration is consumed only by scheduled ticks.
- A duration-three DoT produces three ticks, including when applied during the target's turn.
- Stronger DoT refreshes preserve the original effect identity and source ownership.
- Corrupted applies dynamically to DoT using the original source's current modifier.
- Blinded applies to direct attacks only, using the approved pre-mitigation split; it does not apply to DoT.
- Elemental DoT uses `ITargetable.ApplyElementalDamage()` rather than the ordinary attack pipeline.
- Speed scheduling uses a snapshot of effective Speed captured at the beginning of each round.
- Boss phase transitions preserve active effects.
- Death and encounter cleanup remove active effects through the established lifecycle.
- Health healing and direct health damage use their dedicated operations.
- Elemental reactions remain deferred until Phase D.

When an earlier implementation detail or design description conflicts with these decisions, follow the approved `Elemental Mechanics Specification` and the final reconciliation section of `C0.2.2`.


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


Future Player stat simplification
- Replace the separate Player Attack and Magic power contributions with a single Attack Power value.
- Let the Player's archetype/damage type determine whether that power is used for martial, magical, or hybrid damage.
- Avoid maintaining two independently calculated offensive stats when the distinction belongs to the damage type.