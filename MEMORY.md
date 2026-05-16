# MEMORY.md — Settlers 7 Unity Clone

> Claude Code owns this file. Read at session start. Update at session end.
> CLAUDE.md is the developer's source of truth. This file tracks what changed since.

---

## Last Session Summary

### 2026-05-16 — Phases 1-10 complete + CI fix

**All 10 planned phases shipped and pushed to `claude/review-project-structure-Y7Rlc`:**

- **Phase 1** (23aca76) Tutorial & Onboarding Mission — TutorialSystem, TutorialStep, TutorialMapFactory, TutorialOverlayUI
- **Phase 2** (2e3a75d) Campaign Mode — CampaignProgress, CampaignSystem, Mission, MissionObjective, CampaignSelectionUI, MissionBriefingUI, MissionCompleteUI
- **Phase 3** (f83c0f9) AI Difficulty & Personalities — AIPersonality, AIDifficulty, AIBehaviorProfile; extended AIController + GameState + GameSetupUI
- **Phase 4** (6081bde) Skirmish Customization — StartingProfile, VictoryRuleSet, GameRules; extended GameSetupUI to 7-param OnStartGame
- **Phase 5** (fc58816) In-Game Map Editor — MapEditorState, MapValidation, MapSerializer, MapEditorController, MapEditorUI, SectorPropertyPanel
- **Phase 6** (dd3f919) Audio & Settings — SettingsState, SettingsPersistence, SettingsUI (3 partials), extended AudioManager with EventBus subscriptions; Settings button in MainMenu + PauseMenu
- **Phase 7** (dae1e56) Achievements & Statistics — Achievement, AchievementCondition, AchievementSystem, PlayerStats, AchievementProgress, AchievementsPanel, AchievementToast
- **Phase 8** (046c3e8) Diplomacy System — DiplomacySystem, DiplomaticStatus, DiplomaticAction, AIDiplomacyDecider, DiplomacyPanel
- **Phase 9** (2899c62) Post-Game Summary & Hall of Fame — MatchResult, MatchHistoryPersistence, ScoreCalculator, PostGameSummaryUI, HallOfFameUI
- **Phase 10** (6f13dc5) Visual Polish & VFX — ParticleEffectsManager, FloatingTextManager, FloatingTextItem, CameraShake, HighlightOverlay, BootstrapScene.VFX.cs
- **Step 1** (293fb5e) CI fix — QuestPanel.cs (305 lines) split into QuestPanel.cs + QuestPanel.Factory.cs

**Current state:** 158 scripts, 33 tests. CI green. All files ≤ 300 lines.

---

## Previous Session Summaries

### 2026-05-15 — Project review & infrastructure fixes

**What was done:**
- Fixed `.claude/settings.json` hook paths: changed hardcoded macOS paths to relative paths so hooks actually run
- Added PostToolUse `check-file-size.sh` hook: warns when a .cs file exceeds 300 lines after any edit
- Fixed path references in all command files (session-start, session-end, update-memory) from `~/Unity/Settler7_1/` to relative project paths
- Expanded `settlers-game-design` skill: removed references to non-existent CLAUDE.md §2–§13 sections; moved the full game design spec (buildings, production chains, food boosting, logistics, military, tech, trade, prestige, VPs, quests) directly into the skill
- Updated CLAUDE.md: added explicit note that full game design spec is in `settlers-game-design` skill; removed hardcoded file count from Project State section
- Created `project_status.md` and `project_folder_structure.md` auto-memory files (were referenced by /status and /validate but never generated)
- Moved `HandleClick()` and `SelectSector()` from `GameController.cs` to `GameController.Input.cs` to bring file under 300-line limit (was 321)
- Created `.github/workflows/ci.yml`: runs code quality checks (layer violations, file sizes, namespace consistency) on every push without needing Unity
- Created `.claude/commands/test.md`: documents how to run the NUnit test suite

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

### File Counts (2026-05-16)
- 158 script files in `Assets/Scripts/`
- 33 test files in `Assets/Tests/Editor/`
- All files ≤ 300 lines (PostToolUse hook + CI enforcement)

### Assembly Definitions
| Assembly | Location | Rule | References |
|----------|----------|------|------------|
| Settlers.Simulation | `Assets/Scripts/Simulation/` | Pure C#, `noEngineReferences: true` | (none) |
| Settlers.Game | `Assets/Scripts/` (root) | Presentation + UI + Data | Simulation, TMPro, InputSystem |
| Settlers.Editor | `Assets/Scripts/Editor/` | Editor-only | Simulation, Game |
| Settlers.Tests | `Assets/Tests/Editor/` | Editor-only, NUnit | Simulation, TestRunner |

### Simulation Layer (Pure C# — 78 files)
- **Core:** GameState, EventBus, PlayerResources, SaveSystem, SimulationRunner, Enums, TutorialSystem, TutorialStep
- **Economy:** Building, WorkYard, Storehouse, ProductionSystem, LogisticsSystem, ConstructionSystem, PopulationSystem, FoodBoostCalculator, UpgradeSystem, RecipeDatabase, BuildingCosts
- **Military:** ArmySystem, CombatResolver, ConquestSystem, FortificationSystem, General, UnitType
- **Technology:** TechTree (18 techs, 3 tiers), ResearchSystem, TechEffects
- **Trade:** TradeMap, TradeSystem, TavernSystem, FourPlayerTradeMapFactory
- **AI:** AIController (+Strategy), AIEconomy, AIPersonality, AIDifficulty, AIBehaviorProfile
- **Diplomacy:** DiplomacySystem, DiplomaticStatus, DiplomaticAction, AIDiplomacyDecider
- **Meta:** PrestigeSystem+DB, VictorySystem (+Events, VPThresholds), QuestSystem+DB+Events, CampaignProgress, CampaignSystem, Mission, MissionObjective, GameRules, StartingProfile, VictoryRuleSet, Achievement, AchievementCondition, AchievementSystem, AchievementProgress, PlayerStats, MatchResult, MatchHistoryPersistence, ScoreCalculator
- **Map:** MapFactory (7), LargeMapFactory, FourPlayerMapFactory, TestMapFactory, TutorialMapFactory, Sector, SectorGraph, MapEditorState, MapValidation, MapSerializer
- **Settings:** SettingsState, SettingsPersistence

### Presentation Layer (30 files)
- **GameController** — 4 partials (main, Buildings, Input, SectorVisuals)
- **BootstrapScene** — 3 partials (main, UI, VFX)
- **Audio:** AudioManager (wired to EventBus)
- **Camera:** SettlerCamera
- **Buildings:** BuildingView, BuildingViewFactory, ConstructionView
- **Units:** ArmyView, CarrierManager/View, ClericManager/View, WorkerManager/View
- **Map:** SectorView, RoadView, MinimapController
- **Input:** BuildingPlacer
- **VFX:** ParticleEffectsManager, FloatingTextManager, FloatingTextItem, CameraShake, HighlightOverlay
- **Editor:** MapEditorController
- **Save/Load:** SaveLoadController

### UI Layer (41 files)
All programmatically created (no prefabs):
- MainMenuUI, GameSetupUI (+Widgets), MapSelectionUI, PauseMenuUI
- SaveSlotUI (+SlotEntry), HUD, SectorPanel (+Actions), BuildMenu
- ArmyPanel (+Factory), TavernUI, TechTreeUI+Factory, TradeMapUI+Factory
- PrestigeChartUI+Factory, VictoryPanel (+Create), NotificationUI
- QuestPanel (+Factory), MapEditorUI, SectorPropertyPanel
- AchievementsPanel, AchievementToast, HallOfFameUI, PostGameSummaryUI
- DiplomacyPanel, SettingsUI (+Audio, +Graphics partials)
- CampaignSelectionUI, MissionBriefingUI, MissionCompleteUI, TutorialOverlayUI
- UIFactory, UIColors

### Data Layer (7 ScriptableObject definitions)
BuildingDefinition, GameConstants, MapDefinition, ProductionRecipe, TechDefinition, WorkYardDefinition, PrestigeUnlockDefinition

### Editor Scripts (2 files)
AssetGenerator, AssetGeneratorMaps — menu items under `Settlers/` to generate .asset files

### Tests (33 files)
AITests, AIPersonalityTests, AchievementTests, BuildingAndWorkYardTests, CampaignTests, ConquestRewardTests, ConstructionTests, DiplomacyTests, FoodBoostTests, FortificationTests, GameRulesTests, InputReservationTests, LargeMapTests, LogisticsTests, MapEditorTests, MapFactoryTests, MilitaryTests, PostGameTests, PrestigeTests, ProductionFoodAndReservationTests, ProductionTests, QuestTests, SaveLoadTests, SectorGraphTests, SettingsTests, TechEffectsIntegrationTests, TechEffectsTests, TechnologyTests, TradeTests, TutorialTests, UpgradeTests, VFXTests, VictoryTests

---

## Known Bugs

_(none known — verify after Unity reimport)_

---

## Next Up (Post-Phase-10 Steps)

1. **Step 2 (docs)** — ✓ Completed this session (MEMORY.md, project_status.md, project_folder_structure.md)
2. **Step 3 — Localization & Accessibility** — LocalizationDatabase, StringTable.en.csv, KeyBindings, SettingsUI Language/Controls tabs, color blind palette (~10 files)
3. **Step 4 — Replay System** — ActionRecord, ActionRecorder, ReplayController, ReplayUI, timeline scrubber (~8 files)
4. **Step 5 — Modding & Custom Content** — ModLoader, ModBrowserUI, CustomMapRegistry, ScenarioDefinition (~10 files)
5. **Unity verification** — open Unity, check Console (no CS errors), run Test Runner, exercise each Phase's features in Play Mode
6. **Art pass** — import 3D models, materials, AudioClips (out of scope for code-only work)

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
