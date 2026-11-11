# Elemental Mechanics Specification

**Project step:** Step 11 — Complete and Improve Elemental Damage and Status Effects  
**Phase:** A — Review and Define Elemental Mechanics  
**Status:** Approved design baseline  
**Implementation:** Deferred to the relevant Step 11 integration phases

This document records the approved Phase A decisions. It defines the behavior that later implementation and tests must follow; it does not claim that the elemental system is already implemented.

## 1. Element model

The six supported elements remain:

- Fire
- Lightning
- Ice
- Nature
- Radiance
- Abyssal

Keep these monster concepts separate:

- `AttackElement` identifies the element a monster uses when attacking.
- `Vulnerability` identifies the element to which the monster takes increased damage.
- `Resilience` identifies the element to which the monster takes reduced damage.]
- `ElementalPower` is the monster's base elemental attack damage.

`AttackElement` must not implicitly act as a resilience. Add/retain explicit vulnerability and resilience data rather than inferring one property from another. A target should not have the same element assigned as both its vulnerability and resilience.

## 2. Elemental damage multipliers and mitigation order

Use these affinity multipliers:

| Target affinity | Multiplier |
| --- | ---: |
| No matching vulnerability or resilience | 1.0x |
| Matching vulnerability | 1.5x |
| Matching resilience | 0.5x |

Elemental damage is a distinct damage channel from the existing martial, magical, and hybrid calculations. Do not automatically apply conventional defense or resilience to elemental damage.

Resolve elemental damage in this order:

1. Start with the source's base elemental damage.
2. Apply the target's matching vulnerability or resilience multiplier.
3. Convert the multiplied value to integer damage by truncating the fractional part.
4. Sum the mitigation from every equipped armor rune matching the incoming element. Each rune contributes its existing `Use(tier)` value (`Power * tier`).
5. Subtract the total matching armor mitigation.
6. Clamp the result to a minimum of zero, then pass the final damage through the normal health/death handling path.

Armor mitigation therefore cannot cause negative damage or healing. There is no additional arbitrary mitigation cap; the final zero-damage floor is the cap. Nonmatching armor runes contribute nothing.

## 3. Weapon rune elemental damage and status application

On a successful weapon-rune hit, apply the rune's elemental damage using its existing damage value (`Power * tier`). The status-effect chance is a separate roll and does not gate the elemental damage itself.

Preserve the existing rarity-based status chances:

| Rarity | Chance |
| --- | ---: |
| Common | 1% |
| Uncommon | 3% |
| Rare | 8% |
| Epic | 12% |
| Legendary | 20% |
| Mythic | 35% |

When a status is successfully applied, its initial power is `(int)Rarity + Tier`, using the existing zero-based rarity enum. Its duration comes from the rune's configured `Duration` (currently seeded as 3 for weapon runes).

## 4. Six elemental status effects

Retain the existing element-to-status mapping and define each status as follows. `Power` is a flat magnitude unless a later implementation detail requires a safe stat floor.

| Element | Status | Behavior |
| --- | --- | --- |
| Fire | Charred | Reduces the affected target's effective defense by the effect's power. |
| Lightning | Shocked | Reduces the affected target's effective resilience by the effect's power. |
| Ice | Frozen | Reduces healing received by the effect's power; it does not automatically prevent all healing. |
| Nature | Snared | Reduces the affected target's speed by the effect's power, affecting turn order. |
| Radiance | Blinded | Increases damage received by the affected target by the effect's power. |
| Abyssal | Corrupted | Reduces damage dealt by the affected target by the effect's power. |

Temporary reductions must not permanently mutate a combatant's base stats. Apply them as active combat modifiers and ensure derived values cannot become invalid (for example, speed or effective defenses below their supported minimum).

## 5. Effect duration, ticking, and expiration

- Apply effects immediately when their application succeeds.
- Tick damage-over-time effects at the beginning of the affected combatant's turn.
- A duration of 3 means three scheduled ticks for a damage-over-time effect, beginning with the next applicable turn after application.
- - If a DoT is applied during or after the affected combatant's turn, its first tick occurs at the beginning of that combatant's next turn. It never ticks immediately upon application.
- `RemainingDuration` decreases only when a scheduled DoT tick is processed, after the damage for that tick has been resolved. Ending a combatant's turn does not independently decrease DoT duration.
- A DoT expires after its final scheduled tick, when `RemainingDuration` reaches zero. A tick consumes duration even if mitigation reduces its resulting damage to zero.
- Decrement an effect's remaining duration after the affected combatant completes its turn. Expire it and run its expiration handling when the duration reaches zero.
- If a damage-over-time tick kills the affected combatant, stop its turn and do not process later effects for that dead combatant.
- Dead combatants do not continue ticking effects. Clear their active effects through the normal death/cleanup path.
- Elemental effects are combat-scoped and do not carry into a later encounter.

## 6. Stacking, refreshing, and replacement

- Different status-effect types may coexist on the same combatant.
- Do not stack multiple copies of the same status type or same elemental damage-over-time effect.
- When the same effect is reapplied at equal or greater power, keep the stronger/new effect and refresh its duration.
- When a weaker copy is reapplied, retain the existing stronger effect and do not reduce its power or shorten its remaining duration.
- A refresh must not cause an effect to tick twice in one turn.

## 7. Monster elemental attacks

A monster's elemental attack is a secondary component of its normal attack, not a separate independent attack. Resolve the normal and elemental components as one attack attempt: if the target dodges, both components are avoided. If the attack is not dodged, resolve the conventional component and then the elemental component through their respective damage paths.

Use `AttackElement` and `ElementalPower` for the elemental component. Apply the same target affinity multipliers and matching armor-rune mitigation defined above. Route resulting damage through the existing death and boss-phase-transition handling.

## 8. Elemental combinations

Elemental combinations are approved as part of the intended gameplay system. Phase A establishes that interactions/reactions between elements are in scope, but does not invent individual pairings, reaction effects, or bonus values. Define and balance those concrete combinations in Step 11 Phase D, alongside elemental skill mechanics. Until then, no unapproved automatic reaction multiplier or chain reaction should be applied.

## 9. Monster elemental-power scaling review

The current `MonsterFactory` assigns elemental power with `Random.Next(1, 3)`, limiting generated monsters to 1–2 elemental damage regardless of monster level or threat. Existing factory tests encode this range. This is a known balance gap, not an approved final damage curve.

Step 11 Phase B must review and implement a deterministic, level- and threat-aware scaling rule that remains consistent with the monster's ordinary attack power and combat difficulty. Update the factory tests to validate the chosen scaling behavior across levels and threat levels rather than asserting only the current 1–2 range. The exact formula and balance values remain an implementation/balance decision for that work; they are not fabricated in Phase A.

## 10. Implementation boundaries

This specification is the design baseline for Step 11. Phase A is complete once these decisions are recorded. Implementation belongs to Phases B–D, and verification/balance belongs to Phase E. Preserve the current shared damage architecture; do not restore the legacy `EffectManager` architecture wholesale merely because its behaviors informed this specification.
