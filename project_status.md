# Project Status — Die Siedler 7 Unity Clone

> **The single source of truth for "where are we right now."** Read at session start (with
> CLAUDE.md and VISION.md); update at session end. Goal & Definition of Done: [VISION.md](VISION.md).
> How we build: [CLAUDE.md](CLAUDE.md). Read by the `/status` and `/validate` commands.
>
> Last updated: **2026-07-04**

## Current Position

**Definition of Done: Tier 1 (A Game, Not a Prototype) — in progress.**
Foundation (Tier 0) complete and stable. Visual roadmap Phases 1–2 done. **Next: Phase 3 —
building visual overhaul.** See the tier checklists and the 6-phase table in VISION.md.

## Health at a Glance

| Metric | Value |
|--------|-------|
| NUnit tests | **487 / 487 green** |
| Playable end-to-end | ✅ menu → map → play → victory/defeat → restart |
| Bilingual EN/DE | ✅ test-enforced key parity |
| Architecture (Simulation = pure C#) | ✅ no UnityEngine in Simulation/ |
| 300-line file rule | ⚠️ 3 files over (see Known Issues) |

## File Counts (2026-07-04)

| Layer | Path | Count |
|-------|------|-------|
| Simulation | `Assets/Scripts/Simulation/` | 94 |
| Presentation | `Assets/Scripts/Presentation/` | 35 |
| UI | `Assets/Scripts/UI/` | 51 |
| Data | `Assets/Scripts/Data/` | 7 |
| Editor | `Assets/Scripts/Editor/` | 2 |
| **Scripts total** | `Assets/Scripts/` | **189** |
| Tests | `Assets/Tests/Editor/` | 43 |

## Assembly Definitions

| Assembly | Rule |
|----------|------|
| `Settlers.Simulation` | Pure C#, `noEngineReferences: true` |
| `Settlers.Game` | Presentation + UI + Data; refs Simulation, TMPro, InputSystem, URP |
| `Settlers.Editor` | Editor-only |
| `Settlers.Tests` | NUnit, Editor-only |

## Simulation Systems (20, all implemented & tested)

Economy (Production, Logistics, Construction, Population, Upgrade, FoodBoost) · Military (Army,
Combat, Conquest, Fortification) · Technology (Research, TechEffects, TechTree) · Trade (Trade,
Tavern) · Meta (Victory, Prestige, Quest, Campaign, Achievement, Diplomacy, PostGame,
ConquestReward) · AI (Controller + Strategy + Economy + Personality/Difficulty/Profile,
now victory-race aware) · Maps (MapFactory + variants, MapEditor) · Core (GameState, EventBus,
SaveSystem, SimulationRunner, Tutorial) · Localization · Settings · Replay · Modding.

## Roadmap Progress (toward Definition of Done)

| # | Phase | Status |
|---|-------|--------|
| 1 | Terrain & Lighting — land reads as land, fairy-tale look | ✅ done |
| 2 | Playability Quick Wins — restart, AI victory-racing, audio wiring | ✅ done |
| 3 | Building Overhaul — procedural multi-part buildings, home castle | ▶ **next** |
| 4 | Unit Overhaul — recognizable settlers, carriers, generals | ○ pending |
| 5 | UI Fidelity — parchment trade map, stone tech tree, ÜBERSICHT | ○ pending |
| 6 | Polish & Balance — content, tuning, edge-cases, 60 fps | ○ pending |

## Known Issues / Tech Debt

- **3 files exceed the 300-line rule** and should be split:
  - `Assets/Scripts/Simulation/Core/SaveSystem.cs` — 381 (pre-existing; split by concern)
  - `Assets/Scripts/Presentation/BootstrapScene.cs` — 342 (grew in Sprint 2; extract game-launch/teardown into a partial)
  - `Assets/Scripts/Presentation/GameController.cs` — 332 (grew in Sprint 2; `TeardownGame` could move to a partial)
- **Audio has no clips yet** — framework wired, waiting on CC0 files in `Assets/Resources/Audio/`
  (names in that folder's README). User action.
- **PostGameSummary and VictoryPanel both draw a game-over overlay** — they overlap; cosmetic,
  one should be suppressed.
- **Stray minimap "Home" box** appears top-left on programmatic start (bootstrap leftover).

## Key Patterns (bite-you-if-forgotten)

- **Fresh EventBus per game:** every `StartGame` builds a new `GameState` → new `EventBus`.
  Long-lived subscribers (bootstrap wiring, AudioManager, VFX) must re-subscribe after a
  restart, or their handlers go silent. `AudioManager` compares `Events != _subscribedBus`
  in `Update`; `BootstrapScene.StartTrackedGame` re-calls the Wire* methods.
- **Clean teardown before restart:** `GameController.TeardownGame()` destroys spawned roots
  (MapRoot/Roads/Buildings/Units) and nulls State/runner; without it `InitializeGame` no-ops.
- **Mesh winding:** procedural flat meshes must wind clockwise-from-above or they face down and
  get backface-culled (this once hid the entire ground). Verify with `_Cull=0` when debugging.
- **Victory countdown needs two ticks:** first starts it, second decrements.

## Recent Sessions

- **2026-07-03 — Sprint 2 (Playability):** Play Again fix (+ TeardownGame), AI victory-race
  awareness (+4 tests), audio Resources loading. 483→487 green.
- **2026-07-02 — Sprint 1 (Terrain & Lighting):** procedural ground textures, trees/rocks,
  ambient+fog+URP grading; fixed a mesh-winding bug that had hidden the ground entirely.
- Detailed per-session logs live in the Wissensdatenbank (`AI_Sessions/Claude_Code/Siedler-Clone/`).
