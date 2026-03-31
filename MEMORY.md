# MEMORY.md — Settlers 7 Unity Clone

> Claude Code owns this file. Read at session start. Update at session end.
> CLAUDE.md is the developer's source of truth. This file tracks what changed since.

---

## Last Session Summary

### 2026-03-25 — Roslyn cleanup, Editor menu verified, UI stubs confirmed

**What was done:**
- Deleted incompatible Roslyn plugins from `Assets/Plugins/Roslyn/` (~12MB, 4 DLLs) — Unity 6 has its own compiler
- Removed empty `Assets/Plugins/` directory
- Verified `AssetGenerator.cs` + `AssetGeneratorMaps.cs` exist in Editor scripts
- Verified all 25 UI files present including PrestigeChartUI, TechTreeUI, TradeMapUI, ArmyPanel, TavernUI

### 2026-03-25 — ArmyPanel + TavernUI Full Implementation

**What was done:**
- Implemented ArmyPanel (162 lines) + ArmyPanelFactory (137 lines):
  - Two-column layout: Generals list (left) + Training controls (right)
  - Live refresh: general rows show ID, sector, unit composition, ATK/DEF stats, moving status
  - Training buttons for all 5 unit types with cost/time labels
  - Training queue display with progress percentages
- Implemented TavernUI (293 lines):
  - Inventory display: Beer, Coins, Tools, Generals count
  - Beer→Coins exchange (1 Beer = 3 Coins) with x1 and x5 buttons
  - Coins→Tools exchange (5 Coins = 1 Tool) with x1 and x3 buttons
  - Hire General button (10 Coins)
  - Feedback text with success/error messages (auto-clear after 3s)
  - Pre-validates resources before calling simulation layer

### 2026-03-25 — Fix CS0246 + CS0618 Errors + Cost-Saving Skill

**What was done:**
- Created 5 missing UI stub files that caused CS0246 errors: PrestigeChartUI.cs, TechTreeUI.cs, TradeMapUI.cs, ArmyPanel.cs, TavernUI.cs
  - Each stub includes fields/methods referenced by factories (PrestigeChartUIFactory, TechTreeUIFactory, TradeMapUIFactory) and BootstrapScene.UI.cs
  - Includes `IsVisible`, `Show()`, `Hide()`, `Toggle()`, static `Create()` methods
- Fixed CS0618 warning in VictoryPanel.cs: replaced deprecated `enableWordWrapping = false` with `textWrappingMode = TextWrappingModes.NoWrap` (lines 129, 155)
- Created `cost-saving` skill at correct path `.claude/skills/cost-saving/SKILL.md` (was previously at wrong path `SKILLS/cost-saving.md`)
- Fixed `/session-start` command: updated cost-saving skill reference to correct path
- Created `settlers-game-design` skill with full game design spec (§2–§13)

### 2026-03-25 — CLAUDE.md Review + Documentation System

**What was done:**
- Reviewed and patched CLAUDE.md, created MEMORY.md, created slash commands

### 2026-03-25 — Compilation Fix + Code Cleanup (UNCOMMITTED)

**What was done:**
- Fixed `Settlers.Game.asmdef`: added missing `Unity.InputSystem` assembly reference
- Major code reduction pass — 24 files changed, ~1,400 lines removed
- Added partial class files: BootstrapScene.UI.cs, GameController.Input.cs, etc.

---

## Current State

### File Counts (2026-03-25)
- 103 script files in `Assets/Scripts/`
- 23 test files in `Assets/Tests/Editor/`
- ~14,900 lines of C# total
- All files under 300 lines (architecture rule)

### Assembly Definitions
| Assembly | Location | Rule | References |
|----------|----------|------|------------|
| Settlers.Simulation | `Assets/Scripts/Simulation/` | Pure C#, `noEngineReferences: true` | (none) |
| Settlers.Game | `Assets/Scripts/` (root) | Presentation + UI + Data | Simulation, TMPro, InputSystem |
| Settlers.Editor | `Assets/Scripts/Editor/` | Editor-only | Simulation, Game |
| Settlers.Tests | `Assets/Tests/Editor/` | Editor-only, NUnit | Simulation, TestRunner |

### Simulation Layer (Pure C# — 48 files)
All systems implemented and functional in isolation:
- **Core:** GameState, EventBus (pub/sub with generics), PlayerResources, SaveSystem (text serialization), SimulationRunner, Enums
- **Economy:** Building, WorkYard, Storehouse, ProductionSystem, LogisticsSystem, ConstructionSystem, PopulationSystem, FoodBoostCalculator, UpgradeSystem, RecipeDatabase, BuildingCosts
- **Military:** ArmySystem (generals, training, movement), CombatResolver, ConquestSystem (military/proselytism/bribery), FortificationSystem, General, UnitType
- **Technology:** TechTree (18 techs, 3 tiers, first-come-first-served), ResearchSystem, TechEffects
- **Trade:** TradeMap + TestTradeMapFactory, TradeSystem, TavernSystem (beer→coins, coins→tools, hire generals), FourPlayerTradeMapFactory
- **AI:** AIController + AIController.Strategy (fortify, multi-general, bribery, path switching), AIEconomy (resource-aware decisions + upgrades + quests)
- **Meta:** PrestigeSystem + PrestigeDatabase, VictorySystem (dynamic+permanent VPs, 3-min countdown), QuestSystem + QuestDatabase + QuestEvents, VictoryEvents
- **Maps:** MapFactory (7 maps), LargeMapFactory, FourPlayerMapFactory, TestMapFactory, Sector, SectorGraph (BFS pathfinding)

### Presentation Layer (21 files)
- **GameController** — 4 partials: main, Buildings, Input, SectorVisuals
- **BootstrapScene** — 2 partials: main, UI (procedural scene creation)
- **Camera:** SettlerCamera (spherical orbit, zoom-elevation coupling)
- **Buildings:** BuildingView, BuildingViewFactory, ConstructionView
- **Units:** ArmyView + ArmyViewManager, CarrierManager/View, ClericView, WorkerManager/View
- **Map:** SectorView, RoadView, MinimapController
- **Audio:** AudioManager (functional but needs real AudioClip assets)
- **Input:** BuildingPlacer (New Input System)
- **Save/Load:** SaveLoadController

### UI Layer (24 files)
All programmatically created (no prefabs needed):
- MainMenuUI, GameSetupUI (+Widgets partial), MapSelectionUI
- PauseMenuUI, SaveSlotUI (+SlotEntry partial)
- HUD, SectorPanel (+Actions partial), BuildMenu
- ArmyPanel, TavernUI, TechTreeUI + TechTreeUIFactory
- TradeMapUI + TradeMapUIFactory, PrestigeChartUI + PrestigeChartUIFactory
- VictoryPanel (+Create partial), NotificationUI
- UIFactory (shared helpers), UIColors (constants)

### Data Layer (7 ScriptableObject definitions)
BuildingDefinition, GameConstants, MapDefinition, ProductionRecipe, TechDefinition, WorkYardDefinition, PrestigeUnlockDefinition

### Editor Scripts (2 files)
AssetGenerator, AssetGeneratorMaps — menu items under `Settlers/` to generate .asset files

### Tests (23 files)
AITests, BuildingAndWorkYardTests, ConquestRewardTests, ConstructionTests, FoodBoostTests, FortificationTests, InputReservationTests, LargeMapTests, LogisticsTests, MapFactoryTests, MilitaryTests, PrestigeTests, ProductionFoodAndReservationTests, ProductionTests, QuestTests, SaveLoadTests, SectorGraphTests, TechEffectsIntegrationTests, TechEffectsTests, TechnologyTests, TradeTests, UpgradeTests, VictoryTests

---

## Known Bugs

_(none known — verify after Unity reimport)_

---

## Next Up

1. **Verify Unity compilation** — delete Library/, reopen Unity, check Console for errors
2. **Run `Settlers > Generate All Assets`** — creates SO .asset files in `Assets/Data/`
4. **Configure SO assets in Inspector** — wire definitions with actual values
5. **Import TMP Essential Resources** — `Window > TextMeshPro > Import TMP Essential Resources` (needed for fonts)
6. **Create prefabs** — replace procedural primitives with 3D models
7. **Materials + URP polish** — proper terrain/building materials, lighting, post-processing
8. **Audio import** — real sound effects and music clips for AudioManager
9. **Playtesting + balance** — economy speeds, AI difficulty, VP thresholds
10. **Map editor** (stretch goal)

---

## Decisions Made

### Process
- **Cost-saving rules documented in `.claude/skills/cost-saving/SKILL.md`** — read every session. Contains token budgets, golden rules, and learned mistakes.

### Architecture
- **Sector graph, not hex grid** — sectors are discrete nodes in a graph, terrain is visual only inside each. BFS pathfinding on the graph, not A*.
- **Three-layer separation** enforced by assembly definitions: Simulation (pure C#, no UnityEngine), Game (Presentation+UI+Data), Editor, Tests.
- **No DOTS/ECS** — 200-500 entities don't benefit. MonoBehaviour + SO + pure C#.
- **Procedural scene creation** — BootstrapScene creates everything at runtime. No scene file dependencies during early development.
- **Reflection wiring** — `SetPrivateField()` / `UIFactory.SetField()` wires `[SerializeField] private` fields at runtime. Replace with Inspector drag-and-drop when proper prefabs exist.
- **New Input System only** — `Mouse.current`, `Keyboard.current` from `UnityEngine.InputSystem`. Legacy `Input.*` throws exceptions.
- **MaterialPropertyBlock for sectors** — per-sector color without material cloning.
- **Spherical camera** — azimuth/elevation/distance orbiting a target. Elevation auto-adjusts with zoom.
- **UIFactory dedup** — shared `CreateLabel()`, `CreateButton()`, `SetField()` used by all UI factories. Eliminates hundreds of lines of duplicated UI creation code.
- **Sector IDs are 0-based** matching insertion order in SectorGraph. `UNOWNED = -1`, `NEUTRAL = -2`.

### Game Design Locks
- **Technologies are first-come-first-served** — once researched by any player, locked for all others.
- **Trade outposts are first-come-first-served** — once claimed, exclusive.
- **Food boost halts on empty** — toggled on + no food = production STOPS (no fallback).
- **Noble Residence needs food** — no food = all work yards idle.
- **All goods flow through storehouses** — workers never carry between buildings directly.
- **VPs can be dynamic (stealable) or permanent** — both types implemented in VictorySystem.

---

## What NOT To Do

- **Do NOT use DOTS/ECS** — wrong scale for this project (see CLAUDE.md). Would require rewriting everything.
- **Do NOT reference UnityEngine in Simulation code** — `noEngineReferences: true` in asmdef. Compile error. Only `System.*` allowed.
- **Do NOT implement multiple systems in one session** — causes context overflow and API overload errors. ONE file/feature per turn.
- **Do NOT create UI MonoBehaviour classes without ensuring all referenced types exist first** — missing types cause CS0246 errors that block the entire project from opening in Unity.
- **Do NOT use legacy Input API** (`Input.GetMouseButtonDown`, etc.) — project uses New Input System package. Legacy calls throw `InvalidOperationException`.
- **Do NOT edit .meta files or scene files** — Unity owns those. Only edit .cs, .asmdef, and data files.
- **Do NOT delete Library/ folder without closing Unity first** — corrupts the project cache.
- **Do NOT amend existing git commits** — always create new commits. Amending after hook failure can destroy previous work.
- **Do NOT add Roslyn DLLs to Assets/Plugins** — Unity 6 has its own compiler, they are incompatible and cause plugin errors.
- **Do NOT batch-write multiple files in one turn** — user preference: ONE file per turn, stop, wait for feedback.
- **Always create stub files before referencing a class anywhere** — CS0246 errors block the entire project from opening in Unity. Every class referenced in code must have a .cs file, even if it's just a stub.
