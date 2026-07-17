# Project Status — Die Siedler 7 Unity Clone

> **The single source of truth for "where are we right now."** Read at session start (with
> CLAUDE.md and VISION.md); update at session end. Goal & Definition of Done: [VISION.md](VISION.md).
> How we build: [CLAUDE.md](CLAUDE.md). Read by the `/status` and `/validate` commands.
>
> Last updated: **2026-07-14**

## Current Position

**ALL EIGHT ROADMAP PHASES ARE COMPLETE (2026-07-12). The only open roadmap item is the
final acceptance test — Normen plays one full match per victory path (military / technology /
trade); findings become a punch-list sprint if needed.**

The road here, compressed (full details per sprint in Recent Sessions below):
- **Phases 1–5**: terrain & fairy-tale lighting · playability (restart, AI victory-racing) ·
  procedural building overhaul + home castles · unit overhaul · UI fidelity (parchment trade
  map §14.7, stone tech tree with Geistliche/Mönche/Prälaten costs §14.6, ÜBERSICHT §14.1).
- **Phase 6 (Polish & Balance)**: Technology-AI economy works end to end (clergy chain +
  over-building fixed → Tier-2 research completes) · DE localization of all database display
  strings with Show()-time re-resolution · UI defect sweep (single game-over screen, minimap
  fix, placement feedback) · audio at placeholder quality · zero files over 300 lines.
- **Phase 7 (Content)**: 3 new skirmish maps (20/30/40 sectors, fairness-tested) → 10 total ·
  campaign made actually functional (was never ticked/unlockable) with a 10-mission arc ·
  economy depth: real storehouse-relay carriers (Critical Rule #4), §14.9 goods closed
  (Meat/Fur/Leather chains; Spice/Wine trade-only), map-specific trade networks.
- **Phase 8 (Performance & Strings)**: max-load 14.8 → 100+ fps (root cause: per-part
  MaterialPropertyBlocks broke SRP batching; plus zombie-figure leak and invisible AI/loaded
  buildings fixed) · §14 string sweep done — mission texts, end screens, sector panel and all
  panel chrome localized with live locale switching; §14.1/§14.2 strings enforced 1:1 by test.

**Deliberate leftovers for the acceptance pass** (also in Known Issues): audio clips are
synthesized placeholders (regenerate via Coplay after sign-in, or CC0 drop-in) · all new German
prose awaits Normen's review (recipes, outposts, techs, prestige, mission texts — marked in the
CSVs) · military goods split (Kanonen/Kanonenkugeln/Schwerter) deferred · meta screens
(GameSetup, map names, achievements) still EN-first · tech AI needs a tool source on iron-poor
maps.

## Health at a Glance

| Metric | Value |
|--------|-------|
| NUnit tests | **517 / 517 green** |
| Playable end-to-end | ✅ menu → map → play → victory/defeat → restart |
| Bilingual EN/DE | ✅ test-enforced key parity |
| Architecture (Simulation = pure C#) | ✅ no UnityEngine in Simulation/ |
| 300-line file rule | ✅ zero files over (largest 293) |

## File Counts (2026-07-09)

| Layer | Path | Count |
|-------|------|-------|
| Simulation | `Assets/Scripts/Simulation/` | 96 |
| Presentation | `Assets/Scripts/Presentation/` | 37 |
| UI | `Assets/Scripts/UI/` | 54 |
| Data | `Assets/Scripts/Data/` | 7 |
| Editor | `Assets/Scripts/Editor/` | 2 |
| **Scripts total** | `Assets/Scripts/` | **196** |
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
| 6 | Polish & Balance — AI economy, localization, UI defects, audio¹, refactors | ✅ done |
| 7 | Content — skirmish maps (7a), campaign arc (7b), economy depth (7c) | ✅ done |
| 8 | Performance & Acceptance — 60 fps bar (8a) ✅, §14 string sweep (8b) ✅ | ✅ done¹ |

¹ Only the **final acceptance test** remains: Normen plays one full match per victory path.

¹ Audio clips are placeholders — regenerate via Coplay once signed in (or CC0 drop-in).

## Known Issues / Tech Debt

- **Audio clips are synthesized placeholders** — all 9 README names exist and play, but they're
  simple procedural tones/noise, not the "warm storybook" bar. Regenerate via Coplay
  `generate_sfx`/`generate_music` once Normen signs the Coplay plugin in (cloud returns 401
  Unauthorized), or replace with CC0 files — same filenames, drop-in.
- **AI needs a tool source on iron-poor maps (residual, minor).** The over-building blocker is
  *fixed* (see below) and the Technology AI now completes Tier-2 research. The one remaining gap:
  on maps whose AI-owned sectors contain no Iron deposit (e.g. `twin_rivers`), the AI can't smelt
  IronBars → can't make Tools domestically, capping its staffable yards at whatever tools it can
  trade/conquer for. Verified by granting tools in play-mode. A future AI-strategy item: the tech
  AI should trade for or conquer an iron sector (or bias tool imports) when its sectors lack iron.
  Cleric `RECRUIT_COSTS` tuning remains optional (recruiting was gated on Bread, now produced).
- **Settings controls rows always show "?" as the bound key** (pre-existing, found during the
  8c locale check) — `SettingsUI.Controls` bakes `_keyBindings?.Get(action) ?? "?"` into the
  key labels at `Create()` time, before `Initialize()` loads the bindings, and nothing
  refreshes them afterwards (only `ResetKeyBind` updates its own row). Fix: store the key
  labels and refresh them in `Show()`/after `LoadKeyBindings()`.
- **In-game side panels still unlocalized** — `TavernUI` ("Tavern [V]", "Hire General"),
  `QuestPanel` ("Quests [Q]", "Active/Available Quests", "Accept Quest" + data-driven quest
  titles/descriptions), `DiplomacyPanel` ("Close"), `SectorPropertyPanel` ("Delete Sector",
  map editor). Zero `L.Get` calls, no `ui.tavern/quest/diplomacy` keys yet — next string batch
  (quest text needs a `LocalizedNames` resolver like scenarios got in 8c).
- **DE names for recipes/outposts/techs/prestige are new prose, flagged for Normen's review**
  (Sprint 6b) — goods vocabulary follows the verified §14.9 list, but work-yard names
  (Kornspeicher, Wagnerei, …), outpost names (…-Kontor) and tech/prestige names are Claude's
  German, not screenshot-verified. Review in `StringTable.de.csv` (sections after
  "Panel-Statusmeldungen").
- **Military §14.9 goods are still generic** — Kanonen/Kanonenkugeln/Schwerter exist in the
  original goods list but the game uses a single `Weapons` good (+ cannons as a prestige
  unlock). Splitting them means touching ArmySystem/unit costs — defer until a balance pass
  wants it; the ÜBERSICHT §14.9 coverage is otherwise complete.
- **SO `.asset` regeneration pending** — 3 new recipes (trapper/smokehouse/tannery) exist in
  `RecipeDatabase` (runtime source of truth); run `Settlers > Generate All` in the editor when
  convenient to refresh the Inspector-side assets.
## Key Patterns (bite-you-if-forgotten)

The durable engine traps now live in **[CLAUDE.md → Engine Gotchas](CLAUDE.md)** (fresh EventBus
per game, tear down before restart, mesh winding, victory countdown needs two ticks). Record
*new* traps here as you hit them, then promote the lasting ones to CLAUDE.md so they survive the
next status rewrite.

- **Settler economics: utility buildings are settler-negative when fully yarded.** Living space
  is Lodge/Farm/MountainShelter = 1, Residence = 4 (+4/upgrade), NobleResidence = 5 (+5/upgrade)
  — but every building hosts up to 3 work yards, each needing 1 settler + 1 tool
  (`PopulationSystem`). So a 1-pop utility building fully yarded is −2 settlers. Any AI (or player
  helper) that attaches a yard per slot regardless of population ends with a swarm of idle yards.
  The AI now caps yard attachment to `GetAvailableSettlers` and building sprawl to
  `3 × buildings ≤ livingSpace + slack`. Staffed yards ≈ living space, not `3 × buildings`.

## Recent Sessions

- **2026-07-17 — Sprint 8c completed (meta/shell screen localization, 518/518 green):**
  finished the in-flight meta-screen sweep and closed every shell-screen string gap.
  Verified the CSV parser skips `#` comments (`StringTablePersistence.cs`) and added the
  missing reverse parity test `EnglishTable_CoversEveryGermanKey` (DE keys without EN now
  fail the suite; parity is now enforced both directions). Wired the dormant
  `ui.pause_menu.*`/`ui.settings.*` keys (existed since early sprints, never consumed):
  PauseMenuUI + SettingsUI (all 4 partials — headers, rows, quality names via
  `ui.settings.quality.*`, ON/OFF via `ui.general.on/off`, shared Toggle button) with the
  label-registry + `Show()`-refresh pattern. HallOfFameUI (title/close/empty/Win/Loss +
  localized map names), SaveSlotUI (title reuses pause-menu keys; slot rows via new
  `ui.saveslot.*`), ScenarioSelectionUI (new `LocalizedNames.ScenarioName/Description`
  resolvers with mod-scenario EN fallback, `ui.scenario.*`), VictoryPanel end-screen
  leftovers (`ui.endscreen.return_menu`, new `ui.endscreen.player_stats` format key).
  New DE prose marked "zur Prüfung durch Normen" in the DE CSV. Remaining string gap:
  in-game side panels (see Known Issues). Play-mode locale check PASSED: every changed panel
  (MainMenu, Settings all sections, PauseMenu, SaveSlot, HallOfFame, MapSelection, GameSetup,
  Achievements, ScenarioSelection, VictoryPanel fallback overlay + PostGameSummary) verified
  by label dump in DE and again after switching back to EN — zero stale strings. Side
  findings: keybinding "?" bug (see Known Issues) and 3 junk match-history records
  (`MapId='m'`, old synthetic test data) rendering literally in the Ruhmeshalle.
- **2026-07-14 — Skill harvest (no code change):** distilled the Phase-1–8 lessons into five
  new project skills — `settlers-playmode-testing` (MCP recipes, force states, soak),
  `settlers-localization` (CSV rules, Show()-refresh pattern, §14 discipline),
  `settlers-performance` (MPB ban, culling, bisection method), `settlers-debugging`
  (full trap catalogue + protocol), `settlers-acceptance` (finding triage, punch-list
  sprints, deferred list). CLAUDE.md Skill Reference table extended accordingly.
- **2026-07-12 — Sprint 8b (rough-edge + §14 string sweep — ROADMAP CODE-COMPLETE):**
  localized the last gameplay-visible EN strings: all 10 mission titles/briefings/objectives
  (`ui.mission.<id>.*` keys, resolved via `LocalizedNames.MissionTitle/Briefing/Objective`,
  DE prose flagged for review), MissionBriefing/MissionComplete chrome (headers + buttons,
  re-resolved on Show), PostGameSummary (VICTORY/DEFEAT header, stats block as ONE format key,
  Score, buttons), VictoryPanel VP-tracker/countdown/fallback header (`SP (Ziel {n}): Ihr/S{p}`),
  SectorPanel ("(im Bau)", "(untätig)", Kost-Labels) + all 9 action-feedback strings, and the
  BuildMenu title now re-resolves on Show (was baked). §14 audit hardened into a test:
  `VerifiedGermanStrings_MatchOriginal` now asserts ALL §14.1 titles/columns/carrier lines and
  ALL §14.2 VP names 1:1, plus `EnglishTable_HasAllMissionKeys` per mission/objective.
  Critical Rule 9 visually re-verified (Empire tab: Festung/Kirche/Exportkontor as visible gray
  silhouettes); Rule 10 (BELOHNUNGEN modal, never auto-grant) was play-verified in earlier
  sessions. **517/517 green.** CSV parser note: commas in values are FINE (split on first
  comma) — do NOT escape them. All prior work was committed 2026-07-12 as 11 logical commits;
  the 8b BuildMenu-title fix is the only uncommitted diff.
- **2026-07-11 — Sprint 8a (60 fps bar — Phase 8 started):** max-load scenario (the_frontier,
  4 AIs, 20 sim-minutes, synthetic fill to 203 buildings / 340 yards) measured **14.8 fps** —
  then bisected (units root off? renderers only off? shadows off? sim stopwatch = 0.02ms/tick).
  Three real bugs + two levers: (1) **THE fix: per-part `MaterialPropertyBlock`s disabled SRP
  batching world-wide** (`BuildingViewFactory.SetColor` allocated an MPB per primitive part —
  buildings, walls, figures, trees) → now a cached material per palette color
  (`GetColorMaterial`), everything batches again → closeup 33→124 fps, overview 39→100 fps.
  (2) `WorkerManager` never removed views for unregistered yards → 340 zombie figures after
  conquest churn; now prunes per Sync. (3) **AI + save-loaded buildings had NO views ever**
  (spawn lived only in the human placement path) → views now spawn from `BuildingPlacedEvent`
  for everyone; human path stores sector-LOCAL coords (was world coords — pre-existing
  inconsistency). (4) `ViewLayers`: units (layer 30, cull 70) + building detail (layer 29,
  cull 260) distance-cull via `layerCullDistances`; figures cast no shadows. WorkerView tints
  only on state change and swaps cached materials instead of MPB. 516/516 green; colors
  verified visually intact. Uncommitted.
- **2026-07-11 — Sprint 7c (economy depth — Phase 7 COMPLETE):** three pieces. (1) **Storehouse
  relay** (Critical Rule #4): new `ProductionSystem.RouteDelivery` delegate — null/false =
  credit immediately (standalone tests untouched, back-compat pattern); GameState wires it to
  `Logistics.RequestDelivery(fromSector → GetHomeSector(player))`. KEY SEMANTICS: routed goods
  are credited on `CarrierDeliveryEvent` (existing handler), NOT at production time — naive
  wiring would DOUBLE-credit. Fallbacks (home-sector production, busy carriers — note:
  `Storehouse.CarrierCount = Level + 1`, so level 1 has TWO — unreachable paths) credit
  immediately so goods are never lost. Carriers now actually spawn in real games: play-verified
  live (3 CarrierViews on route, slow-motion `Time.timeScale=0.02` to catch them; adjacent-
  sector deliveries complete in seconds). +3 StorehouseRelayTests. (2) **§14.9 goods closed**:
  +5 ResourceTypes (Meat/Fur/Leather/Spice/Wine), +3 recipes (trapper Lodge→Fur, smokehouse
  Residence Animal→Meat, tannery NobleResidence Fur→Leather; recipe count 30→33, per-building
  tests updated), Spice/Wine deliberately trade-only. ÜBERSICHT verified in DE: Pelz →
  Fallensteller/1×Leder/Gerberei. (3) **Trade networks** for the 7a maps: new
  `SkirmishTradeMapFactory` (10/12/14 outposts, Spice/Wine sources, 3 specials each) +
  36 outpost keys per table; outpost-coverage test now iterates all 7 networks. **516/516
  green**. Military goods split (Kanonen/Schwerter) deliberately deferred (Known Issue).
  Uncommitted.
- **2026-07-11 — Sprint 7b (campaign arc — campaign made functional):** the campaign was
  half-dead: nobody instantiated `CampaignSystem`, ticked it, called `CampaignProgress.
  MarkComplete` (mission 2+ could NEVER unlock), applied `Mission.StartingResources`, or
  evaluated `BuildBuilding`/`DefendSector` objectives. Fixes: (1) `BootstrapScene.WireCampaign`
  on mission start — creates the system, `SetActiveMission` (now RESETS the shared static
  objectives for replays), `ApplyStartingResources`, subscribes completion → `MarkComplete` +
  `MissionCompleteUI.Show`; (2) `GameController.ActiveCampaign` ticked after the runner,
  cleared in teardown; (3) completion fires ONCE (`_completeFired`), `BuildBuilding` (operational
  count by type) and `DefendSector` (hold sector-id until N seconds) implemented. Catalogue
  moved to `CampaignSystem.Missions.cs` (partial, 300-line rule) and extended 7→10 missions in
  ONE unlock chain: new "Hearth and Home" (economy, highland_duel, uses StartingResources +
  BuildBuilding), "The Meadow Fair" (trade, golden_meadows), "The Last Frontier" (finale,
  the_frontier). +3 CampaignTests (chain visits every mission exactly once; objective
  evaluation + starting resources + fires-once on real GameState; replay reset) and the
  valid-maps test now derives from `GetMapIds` → **513/513 green**. Play-verified end-to-end:
  mission 1 via real flow → objectives complete → progress persisted → MissionCompleteUI →
  mission 2 unlocked, starts on highland_duel with 30 Planks/15 Stone/10 Tools. Mission texts
  are EN-only (new Known Issue → Phase-8 sweep). Uncommitted.
- **2026-07-11 — Sprint 7a (skirmish map set — Phase 7 started):** new
  `Simulation/Map/SkirmishMapFactory.cs` (223 lines) with three maps: **Highland Duel**
  (20 sectors, 2p, VP 5 — hand-mirrored: west mining ridge 4-5-6-7-8, east loch route
  9-10-11-12-13, wooded flank alternates, two contested golds linked by an Old-Bridge/
  Standing-Stones center web), **Golden Meadows** (30, 3p, VP 6) and **The Frontier**
  (40, 4p, VP 7) — both assembled from a shared `AddWedge` 9-sector player template
  (Coal 1 step, Iron 2, Gold 3 from every home; two expansion paths each; wedge ring via
  border-woods↔watch-hill; shared contested centers). Registered in `MapFactory.CreateMap` +
  `GetMapIds` (7→10) — MapSelectionUI menu and GameFlowSmokeTests iterate `GetMapIds`, so both
  picked the maps up with zero extra wiring. +4 MapFactoryTests incl. a BFS gold-distance
  fairness test across all players of each new map → **510/510 green**. Play-verified: all
  three maps boot via StartTrackedGame (20/30/40 sectors, correct player counts), sector
  overview screenshots taken. Trade maps: unknown ids fall back to the test trade network —
  map-specific networks are a 7c candidate. Uncommitted.
- **2026-07-11 — Sprint 6e (300-line refactors — Phase 6 COMPLETE):** the three remaining
  over-limit files split into concern-named partials, zero behavior change:
  `GameController.cs` 332 → 169 + `GameController.Lifecycle.cs` 179 (StartGame/Initialize/
  Teardown/InitializeGame + override fields + running flags); `GameController.SectorVisuals.cs`
  318 → 229 + `GameController.MapDressing.cs` 107 (landmarks, world ground, roads);
  `BootstrapScene.cs` 342 → 215 + `BootstrapScene.MenuFlow.cs` 144 (all menu click handlers).
  **Zero files over 300 project-wide** (largest 293). Verified: compile clean, 506/506 green,
  play-mode smoke test start → sim 30s → Serialize → fresh StartGame (teardown OK, 0 buildings)
  → ApplyToState (6 buildings + simTime restored) → restart clean. Roadmap tables in
  project_status.md AND VISION.md extended with Phase 7/8 rows; Phase 7a (skirmish maps) is next.
  Uncommitted (with all Phase-6 sprints).
- **2026-07-11 — Sprint 6d (audio, placeholder) + SaveSystem split (6e part 1):** Coplay plugin
  got installed and its editor bridge connects, but cloud generation returns **401 Unauthorized**
  (Normen must sign in; then regenerate). Fallback: 9 procedurally synthesized WAVs (stdlib
  Python — plucked/bell/horn tones, noise bursts, 33s pastoral loop for `music_main`) written to
  `Assets/Resources/Audio/` under the exact README names. Found and fixed two dead clips:
  `building_placed` had NO event subscription (added `BuildingPlacedEvent` → PlaySFX) and
  `ui_click`/`PlayUIClick()` had no caller (now fired by every `UIFactory.CreateButton`).
  Play-verified: music loops, placement SFX fires, `_subscribedBus` re-subscribe after restart
  works (one-frame gap after StartGame is by design). Also completed the first 6e split:
  `SaveSystem.cs` 405 → 190 + `SaveSystem.Apply.cs` 240 (partials, no API change). 506/506
  green. Uncommitted (with 6a/AI-eco/6b/6c).
- **2026-07-10 — Sprint 6c (UI defect sweep + Tier-1 feedback audit):** (1) **Single game-over
  screen** — `VictoryPanel` now defers to `PostGameSummaryUI` and only shows its own overlay as
  a fallback when no summary actually made it on screen (`summary.IsVisible` check — an
  existence check alone fails when a game is started outside the bootstrap wiring, Engine Gotcha
  #1; both paths play-verified). (2) **Stray minimap "Home" box fixed** — `MinimapController`
  initialized once against the bootstrap placeholder state and never rebuilt; it now rebuilds
  whenever `gc.Graph` changes, treats `MapId == "bootstrap"` as no map, and hides its background
  pre-game. (3) **Tier-1 feedback audit**: coverage already good (SectorPanel action feedback,
  Built!/Conquered!/+1 VP floats, particles, stall speech, selection ring); added the missing
  instant **placement feedback** (`BuildingPlacedEvent` → "Im Bau"/"Under construction" float
  for player 0) and localized all FloatingTextManager strings (5 new `ui.float.*` keys per
  table). **Finding:** carriers can never spawn in real games — `RequestDelivery` has no
  game-code caller; queued for 7c (new Known Issue). 506/506 green; console clean. Uncommitted.
- **2026-07-10 — Sprint 6b (localization completion — DE everywhere):** all EN-only database
  display strings now localized at display time via `LocalizedNames` (extended with
  `Recipe`/`Outpost`/`Tech`/`TechDescription`/`Prestige`/`PrestigeDescription`; fallback = EN
  `DisplayName`, simulation stays EN). +199 keys per string table: `ui.recipe.*` (30),
  `ui.outpost.*` (52, all 4 trade maps), `ui.techname/techdesc.*` (36), `ui.prestige.name/desc.*`
  (48), prestige branch headers, and 12 formerly hardcoded status messages (TradeMapUI,
  TechTreeUI, PrestigeChartUI). Locale-baked factory texts fixed with the ÜBERSICHT pattern —
  `RefreshLocaleTexts()` on `Show()` re-resolves tech cards, tier headers, legends, prestige
  nodes, build-menu tiles, capital node; BuildMenu empire hints now pass keys, resolved at click
  time. +3 LocalizedNamesTests (EN key coverage for all 4 databases + all trade maps; DE via
  existing parity test; locale-switch + fallback behavior) → **506/506 green**. Play-verified in
  DE: ÜBERSICHT (Bäckerei), Handelskarte (Eisenhütten-Kontor, Gewürzroute…), Technologie
  (Fischerei, Fruchtwechsel…), PRESTIGE-OPTIONEN (Stufe/Frei/Punkte). DE names are new prose —
  flagged for Normen's review. Uncommitted (like 6a + AI-economy).
- **2026-07-09 — AI-economy sprint (over-building fixed — AI now completes Tier-2 research):**
  three changes in `AIEconomy`. (1) **Attachment cap** — `AttachWorkYards` attaches yards only
  within a settler budget (`Population.GetAvailableSettlers`), so it never stands up more yards
  than it can staff. (2) **Sprawl cap** — `BuildEconomy` stops raising utility buildings once
  committed work-yard slots (`3 × operational buildings`) outrun living space + a slack of 12;
  population homes (Residence/NobleResidence) stay exempt. (3) **Farm placement** — the clergy
  `ChooseBuildingType` now guarantees a Farm and prefers a WaterSource+FertileLand sector so the
  well (Water → Bread) actually runs. Split the file (was 339 lines) into `AIEconomy.cs` (183) +
  new `AIEconomy.BuildingChoice.cs` (169). +2 AITests (attach-budget stop, sprawl stop) → **503
  green**. Play-verified: work yards 108→**37, all 37 operational** (was 30/108); full Bread+Books
  chains flow (Water 1267, Flour 2968, Books 75); AI recruits Brothers and **completes 6 Tier-2
  techs** (scoreboard: 12 techs total). Remaining: tool sourcing on iron-poor maps (see Known
  Issues). Sprint-6a + this sprint's code is uncommitted (awaiting ask).
- **2026-07-09 — Sprint 6a (Cleric/AI economy, partial — Books chain fixed, over-building found):**
  committed the pending Sprint 3/4/5/5b work as three logical commits first. Then made the
  Technology AI bias its economy toward the goods its clerics consume (§14.6): new
  `prioritizeClergyGoods` flag threaded through `AIEconomy.BuildEconomy`/`AttachWorkYards`/
  `ChooseBuildingType`/`GetWorkYardPriority` (default `false` → all existing behaviour/tests
  unchanged), driven by `AIController.WantsClergyGoods` (`_chosenPath == Technology`). Clergy
  orderings: Farm `grain_barn,windmill,well,shepherd…` (well = Water for Bread); Residence
  `bakery,toolmaker,paper_mill…` (the three Tier-2 essentials fill 3 slots); NobleResidence
  `bookbinder,tailor,mint…`. ChooseBuildingType clergy branch raises a Residence + Noble
  Residence even on mineral sectors. +4 AITests (bookbinder-first, butcher-first contrast,
  Residence-on-mineral, full Books-chain stand-up). 497→**501 green**. Play-verified: the tech AI
  now builds `Residence[bakery,toolmaker,paper_mill]` + `NobleResidence[bookbinder,tailor,mint]`
  and produces **Books from nothing** — impossible before. BUT it still can't complete a Tier-2
  tech: it over-builds (~108 yards, ~30 staffable) so Water/Bread/Tools stay 0. Logged as the
  top Known Issue for a dedicated AI-economy sprint. Sprint 6a code is uncommitted (awaiting ask).
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
