# Game Concept & Design Document
**Project:** Rainstorm
**Engine:** Unity 2022.3
**Date:** June 2026
**Author:** Ignatius

---

## 1. Game Concept

### 1.1 Elevator Pitch
Rainstorm is a top-down 2D action RPG in which the player takes on the role of a magical forest courier — a knight tasked with delivering urgent messages through an enchanted and hostile wilderness. You must fight through monsters, manage stamina, and race against time to complete your delivery.

### 1.2 Core Experience
The player should feel:
- **Empowered** — fluid 8-directional movement and a responsive combat system (melee, sprint-attack, block, special) reward skillful play.
- **Pressured** — a time-based delivery mechanic creates urgency; lingering too long costs the player.
- **Curious** — the world hints at deeper lore (an enchanted forest, mysterious creatures) that slowly unfolds.

### 1.3 Genre & Inspiration
- **Genre:** Top-down action RPG with roguelike influences.
- **Tone:** Dark fantasy action; somewhere between *Hades* and *Cult of the Lamb*.
- **Platform:** PC (Windows) — keyboard-controlled.

---

## 2. Game Design Principles

### 2.1 Feel Before Complexity
All core actions (move, attack, block) must feel responsive and satisfying before adding depth. Hit-stop, knockback, and visual feedback (enemy flash on hit) are treated as non-negotiable for combat clarity.

### 2.2 Readable Threat Hierarchy
Each enemy type communicates its intent clearly:
- **Bat** — fast, dashes, telegraphy via a visible windup before the dash.
- **Wendigo** — slower, more powerful; three distinct attacks (claw / stomp / ranged) escalating in range and damage.

Players should be able to learn enemy patterns within a few encounters without a tutorial.

### 2.3 Risk vs. Reward Movement
Sprinting enables a stronger sprint-attack (15 dmg vs 10 dmg for normal melee) but reduces the player's ability to dodge, creating a meaningful trade-off between aggression and safety.

### 2.4 Minimal UI, Maximum Clarity
HUD displays health only. The time mechanic is communicated through environmental cues (e.g., darkening sky) to avoid cluttering the screen.

---

## 3. Gameplay Systems

### 3.1 Player
| Input | Action | Notes |
|---|---|---|
| WASD / Arrow Keys | 8-directional movement | Blend Tree with MoveX/MoveY |
| Hold Shift | Sprint | Enables sprint-attack |
| Z | Melee attack (10 dmg) | Locked during block |
| Shift + Z | Sprint attack (15 dmg) | Larger hitbox |
| Hold X | Block | Cancels into attack |
| C | Special (20 dmg) | AoE hitbox; animation-gated |

**Health:** 100 HP. Losing all HP ends the run.

### 3.2 Enemy — Bat
| Property | Value |
|---|---|
| Detection range | 8 units |
| Dash speed | 12 units/s |
| Dash damage | 10 HP |
| Stun duration | 3 s |

State machine: Idle → Chase → Windup → Dash → (back to Chase or Stunned).

### 3.3 Enemy — Wendigo
| Attack | Range | Damage | Notes |
|---|---|---|---|
| Claw | 1.5 units | 15 HP | Hitbox via animation events |
| Stomp | 3.0 units (AoE) | 20 HP | Area burst, no hitbox object |
| Ranged (branch) | 8.0 units | 10 HP | Spawns BranchAttack prefab |

Attack selection rotates round-robin by distance, preventing repetitive patterns.

### 3.4 Time Mechanic (Planned)
A delivery timer counts down. Running out of time fails the mission. Killing enemies may drop small time bonuses, encouraging combat rather than avoidance.

---

## 4. Development Scope

### 4.1 Current State (Demo Build)
- [x] Player: movement, sprinting, 8-directional animation (Blend Tree)
- [x] Player: melee, sprint-attack, block, special — all animation-event-driven hitboxes
- [x] Player: health system, hit response (PlayerHit.cs)
- [x] Enemy AI: Bat (FSM: Idle/Chase/Windup/Dash/Stunned)
- [x] Enemy AI: Wendigo (FSM: Idle/Chase/Attacking/Pause/Stunned, 3 attacks)
- [x] Shared systems: Hitbox, Hurtbox, Health, IStunnable interface
- [x] Camera follow
- [x] One playable level (Level1) with both enemy types
- [x] Start screen UI

### 4.2 Out of Scope (Current Build)
- Time-based delivery mechanic (architecture exists, not fully wired)
- Multiple levels / procedural generation
- Sound effects and background music
- Save/load system
- Boss fights
- Item pickups or inventory

### 4.3 Why This Scope
The demo focuses on proving the core combat loop. One polished level with two enemy archetypes (fast/aggressive vs. slow/multi-attack) is sufficient to demonstrate the game's feel and potential. Expanding scope before the core loop is satisfying is a common indie pitfall; this build intentionally avoids it.

---

## 5. Tools, Assets & Technology

### 5.1 Engine — Unity 2022.3 LTS
**Why:** LTS release guarantees stability for a student/indie build. Unity's Animator and Rigidbody2D are well-suited for 2D top-down character controllers. The 2D Physics system and Tilemap tools cover all current needs without requiring a custom engine or heavier framework.

### 5.2 IDE — JetBrains Rider
**Why:** Superior C# IntelliSense and Unity integration compared to VS Code or MonoDevelop, making refactoring the FSM-heavy AI code significantly faster.

### 5.3 Art Assets
All assets are third-party purchased/free-to-use from itch.io. No original art was created; art was selected to be visually consistent (pixel art, top-down perspective, fantasy theme).

| Asset | Creator | License |
|---|---|---|
| Winter Forest Tileset | Seliel the Shaper | Mana Seed User License |
| Wendigo sprites | Akari21 | itch.io standard license |
| Bat sprites (Monster Starter Pack) | bobddadoo | itch.io standard license |
| Knight character (HD 8-Directional) | SmallScaleInt | itch.io standard license |
| TextMesh Pro | Unity Technologies | Unity Package (built-in) |

### 5.4 AI Assistance
Claude (Anthropic) was used as a development assistant for code generation and design guidance. All AI-generated code was reviewed, understood, and manually integrated by the developer.

---

## 6. Legal, Ethical, Social & Accessibility Considerations

### 6.1 Legal
- All third-party art assets are licensed for commercial or non-commercial use as specified above. The Mana Seed User License (Winter Forest Tileset) permits use in games, including commercial releases, with attribution required — attribution is provided in README.md.
- No trademarked characters, IP, or copyrighted music is used.
- Unity Personal License is used; no revenue threshold has been exceeded.
- AI-generated code: Claude's output is not copyrightable; the developer retains authorship of the final assembled work.

### 6.2 Ethical
- The game features fantasy combat (sword vs. fantasy creatures). No human characters are depicted as enemies. Violence is abstracted (no gore, no realistic injury).
- AI was used as a coding tool, not to replace creative authorship. All design decisions remain the developer's own.

### 6.3 Social
- The game does not contain multiplayer, online features, microtransactions, loot boxes, or any systems that could target vulnerable users.
- No user data is collected or stored.
- The fantasy setting avoids real-world cultural appropriation; the Wendigo enemy is a generic fantasy creature design, not a direct depiction of Indigenous mythology.

### 6.4 Accessibility
| Area | Current State | Notes |
|---|---|---|
| Control remapping | Not implemented | Keyboard-only; WASD + arrows provided as alternatives |
| Colorblind support | Partial — no explicit colorblind mode | Red flash on enemy hit is the main color cue |
| Text size | TextMesh Pro used | Font sizes configurable in UI prefabs |
| Difficulty | Single difficulty | No adjustable options in demo build |

**Planned improvements:** Key rebinding and a simple difficulty slider (enemy health multiplier) are identified as high-value accessibility additions for post-demo development.

### 6.5 Security
As a single-player offline PC game with no networking, no database, and no user accounts, the attack surface is effectively zero. No security hardening is required for the current build.

---

## 7. Development Plan

### Phase 1 — Core Loop Polish (Complete)
| Task | Status |
|---|---|
| 8-directional player movement + animation | Done |
| Melee / sprint-attack / block / special combat | Done |
| Bat AI (dash enemy) | Done |
| Wendigo AI (multi-attack boss-lite) | Done |
| Hitbox / Hurtbox / Health architecture | Done |
| Level 1 layout and camera | Done |
| Start screen | Done |

### Phase 2 — Vertical Slice (Next)
| Task | Priority |
|---|---|
| Wire up delivery timer UI | High |
| Enemy drop: time bonus pickups | High |
| Sound effects (attack, hit, death) | High |
| Background music loop | Medium |
| Polish: hit-stop frames, screen shake on stomp | Medium |
| Player death screen / restart flow | High |

### Phase 3 — Content Expansion (Post-Demo)
| Task | Notes |
|---|---|
| Level 2 — new biome | Requires additional tileset |
| New enemy type | Melee-range tank to complement fast Bat |
| Player stamina system | Limit infinite sprinting |
| Procedural room layout | Optional roguelike element |
| Key rebinding menu | Accessibility improvement |


## 8. Summary

Rainstorm is a top-down 2D action RPG demo built around a tight combat loop. The current build delivers two enemy archetypes, a full player combat system, and a clean extensible architecture. All assets are properly licensed and the project carries no ethical, legal, or security concerns.
