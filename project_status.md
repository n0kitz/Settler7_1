# Project Status — Die Siedler 7 Unity Clone

> **The single source of truth for "where are we right now."** Read at session start (with
> CLAUDE.md and VISION.md); update at session end. Goal & Definition of Done: [VISION.md](VISION.md).
> How we build: [CLAUDE.md](CLAUDE.md). Read by the `/status` and `/validate` commands.
>
> Last updated: **2026-07-08**

## Current Position

**Definition of Done: Tier 1 (A Game, Not a Prototype) — in progress; Tier 2 well underway.**
Foundation (Tier 0) complete and stable. Visual roadmap **Phases 1–5 done** — Phase 5 landed
the parchment trade map (§14.7), the stone-and-candlelight tech tree WITH the
Geistliche/Mönche/Prälaten cost mechanic (§14.6, new `ClericSystem`), and the new ÜBERSICHT
production stats panel (§14.1, verified columns live in both locales, O hotkey).
**Next: Phase 6 — Polish & Balance** (content, tuning, edge cases, the 60 fps bar).
See the tier checklists and the 6-phase table in VISION.md.

## Health at a Glance

| Metric | Value |
|--------|-------|
| NUnit tests | **497 / 497 green** |
| Playable end-to-end | ✅ menu → map → play → victory/defeat → restart |
| Bilingual EN/DE | ✅ test-enforced key parity |
| Architecture (Simulation = pure C#) | ✅ no UnityEngine in Simulation/ |
| 300-line file rule | ⚠️ 4 files over (see Known Issues) |

## File Counts (2026-07-08)

| Layer | Path | Count |
|-------|------|-------|
| Simulation | `Assets/Scripts/Simulation/` | 95 |
| Presentation | `Assets/Scripts/Presentation/` | 37 |
| UI | `Assets/Scripts/UI/` | 54 |
| Data | `Assets/Scripts/Data/` | 7 |
| Editor | `Assets/Scripts/Editor/` | 2 |
| **Scripts total** | `Assets/Scripts/` | **195** |
| Tests | `Assets/Tests/Editor/` | 44 |

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
| 3 | Building Overhaul — procedural multi-part buildings, home castle | ✅ done |
| 4 | Unit Overhaul — recognizable settlers, carriers, generals | ✅ done |
| 5 | UI Fidelity — parchment trade map, stone tech tree, ÜBERSICHT | ✅ done |
| 6 | Polish & Balance — content, tuning, edge-cases, 60 fps | ▶ **next** |

## Known Issues / Tech Debt

- **4 files exceed the 300-line rule** and should be split:
  - `Assets/Scripts/Simulation/Core/SaveSystem.cs` — 405 (pre-existing, +cleric block; split by concern)
  - `Assets/Scripts/Presentation/BootstrapScene.cs` — 342 (grew in Sprint 2; extract game-launch/teardown into a partial)
  - `Assets/Scripts/Presentation/GameController.cs` — 332 (grew in Sprint 2; `TeardownGame` could move to a partial)
  - `Assets/Scripts/Presentation/GameController.SectorVisuals.cs` — 318 (grew in Sprint 3; landmark wiring could move to its own partial)
- **Audio has no clips yet** — framework wired, waiting on CC0 files in `Assets/Resources/Audio/`
  (names in that folder's README). User action.
- **Cleric recruiting costs may need balance tuning** — Novice 1 Bread+2 Coins, Brother
  +1 Books, Father +Books/Garments (`ClericSystem.RECRUIT_COSTS`); AI Tier-2+ research stalls
  until its economy produces Books/Garments. Revisit in Phase 6 playtests.
- **Recipe/outpost display names are EN-only** (`RecipeDatabase.DisplayName`,
  `TradeOutpost.DisplayName`) — simulation data strings, not in the string tables; ÜBERSICHT
  and trade map columns show them untranslated in DE.
- **Factory-built panel titles/legends resolve at creation locale** (BuildMenu, TechTree/TradeMap
  legends) — live-switch applies on next game start; ÜBERSICHT headers/selector are the exception
  (refreshed on every Show).
- **PostGameSummary and VictoryPanel both draw a game-over overlay** — they overlap; cosmetic,
  one should be suppressed.
- **Stray minimap "Home" box** appears top-left on programmatic start (bootstrap leftover).

## Key Patterns (bite-you-if-forgotten)

The durable engine traps now live in **[CLAUDE.md → Engine Gotchas](CLAUDE.md)** (fresh EventBus
per game, tear down before restart, mesh winding, victory countdown needs two ticks). Record
*new* traps here as you hit them, then promote the lasting ones to CLAUDE.md so they survive the
next status rewrite.

## Recent Sessions

- **2026-07-08 — Sprint 5b (§14.6 cleric research costs, complete):** new
  `Simulation/Technology/ClericSystem.cs` — recruit Geistliche/Mönche/Prälaten for goods
  (Bread/Books/Garments/Coins), research OCCUPIES the tech's cost triplet for its duration and
  releases on completion/cancel. `TechDef` gained tier-scaled costs (3/0/0 → 4/2/0 → 5/2/1);
  `ResearchSystem(events, clerics)` gates StartResearch (null clerics = ungated, so all existing
  standalone-constructed tests stayed green); GameState wires it, SaveSystem round-trips counts
  (`clerics.{p}=n,b,f`), AI recruits before researching (`RecruitClericsFor`). TechTreeUI got a
  recruit bar (counts avail/total + [+] buttons) and `[n/b/f]` cost triplets on every card; 5 new
  string keys both locales. 487→**497 green** (10 new tests incl. save round-trip); play-verified:
  research refused without clerics, bar shows 0/3 while researching, releases after completion.
- **2026-07-08 — Sprint 5 (UI Fidelity, complete):** (1) **Trade map §14.7** — new `MapArtFactory`
  (procedural parchment/stone/glow/disc textures, compass rose, dotted routes, candles) +
  `TradeMapUIFactory` rewrite: full parchment map, lat/long grid, capital castle node (gold
  frame, new `IconFactory.Castle`), outposts on a golden-angle spiral with `in → out [traders]`
  cards, gold/red/gray frames by claim state, chest icon (`IconFactory.Chest`) on special nodes;
  nodes now rebuilt from the LIVE trade map on every Show (was: stale bootstrap TestTradeMap).
  (2) **Tech tree §14.6 look** — stone background, 4 wall candles with warm glow, stone cards
  with status gems (gold=researched, green=available, red=taken, blue=in progress). (3) **NEW
  ÜBERSICHT panel §14.1** — `StatsOverviewUI(+Factory)`: good selector (all recipe resources),
  four verified columns ERFORDERT/PRODUZIERT VON/ERBRINGT/VERBRAUCHT VON filled from
  RecipeDatabase; headers+selector live-switch locale on Show (autosized so German never
  truncates); wired to action bar + O hotkey + ESC-close. 29 new string keys in BOTH tables.
  487/487 green; all three panels play-verified in EN and DE.
- **2026-07-08 — Sprint 4 (Unit Overhaul, complete):** new `UnitFigureFactory` assembles small
  procedural humanoids (legs/tunic torso/arms/head, feet at y=0, facing +Z) with role gear:
  worker straw hat, carrier cap + goods crate on a `Hands` anchor, soldier helmet + spear,
  cleric full robe + hood, general helmet + gold band + red plume. Rewrote WorkerView (tunic
  grays when idle, faces walk direction, walk bob), CarrierView (crate tinted by resource,
  faces route), ClericView (orbits facing tangent); ArmyView gained a general figure beside the
  banner and a soldier squad ring that grows with `TotalSoldiers` (1–6 figures, rebuilt on
  bucket change). 487/487 green; play-verified via figure lineup + live worker/army screenshots.
  Note: no carrier task arose during the short play run, so CarrierView is compile/lineup-verified
  only — glance at a live carrier next time one spawns.
- **2026-07-08 — Sprint 3 (Building Overhaul, complete):** (1) new `SectorLandmarkView` builds a
  dominating multi-story home castle (corner towers, cone roofs, gold finials, player banner) on
  each player's home sector and a cannon-studded stronghold on guarded neutral sectors (§14.10),
  wired via `GameController.SectorVisuals` (`ComputeHomeSectors`/`AttachLandmark`), built once at
  spawn with no colliders. (2) `BuildingViewFactory.AddDetails` adds doors, warm-lit windows,
  chimneys and rooftop flags (red on Residence, gold on Noble) to the five base buildings.
  487/487 green, both play-mode verified.
- **2026-07-03 — Sprint 2 (Playability):** Play Again fix (+ TeardownGame), AI victory-race
  awareness (+4 tests), audio Resources loading. 483→487 green.
- **2026-07-02 — Sprint 1 (Terrain & Lighting):** procedural ground textures, trees/rocks,
  ambient+fog+URP grading; fixed a mesh-winding bug that had hidden the ground entirely.
- Detailed per-session logs live in the Wissensdatenbank (`AI_Sessions/Claude_Code/Siedler-Clone/`).
