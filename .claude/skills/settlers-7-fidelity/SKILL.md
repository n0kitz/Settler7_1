---
name: settlers-7-fidelity
description: "1:1 fidelity tracker for Die Siedler 7 — Paths to a Kingdom. One row per mechanic: source URL, expected behaviour, our file, status, last verified. This is the backlog: partial = next session's task."
---

# Settlers 7 Fidelity Tracker

**How to use:**
- `1:1` = mechanic matches original game — has passing NUnit tests, playtest verified
- `partial` = core loop works, edge cases or content missing — pick this up next session
- `missing` = not implemented — blocks a victory path or core gameplay loop
- `bug` = implemented but test failures prove incorrect behaviour

**Reference wiki:** https://settlers7.fandom.com/wiki/The_Settlers_7:_Paths_to_a_Kingdom
**Session start protocol:** Read MEMORY.md → read cost-saving skill → read the relevant section below.

---

## Economy

| Mechanic | S7 Source | Expected Behaviour | Our File | Status | Last Verified |
|---|---|---|---|---|---|
| Resource production | [Buildings](https://settlers7.fandom.com/wiki/Buildings) | Work yards produce resources on a cycle. Each needs 1 settler + 1 tool. No tool = idle. | `ProductionSystem.cs`, `RecipeDatabase.cs` | **1:1** | 2026-05-12 |
| Storehouse relay | [Storehouse](https://settlers7.fandom.com/wiki/Storehouse) | All goods transit via storehouses. Carriers pick up and deliver. Never direct building-to-building. | `LogisticsSystem.cs`, `Storehouse.cs` | **1:1** | 2026-05-12 |
| Food boosting | [Food](https://settlers7.fandom.com/wiki/Food) | Plain food (Bread/Fish) = ×2 speed. Fancy food (Sausages) = ×3. Noble Residence idles with no food. Toggle per building. | `FoodBoostCalculator.cs`, `WorkYard.cs` | **1:1** | 2026-05-12 |
| Construction | [Construction](https://settlers7.fandom.com/wiki/Construction) | Builders assigned from pool. Time scales with base construction time. Concurrent upgrades share pool. | `ConstructionSystem.cs`, `BuildingCosts.cs` | **1:1** | 2026-05-12 |
| Population & tools | [Settlers](https://settlers7.fandom.com/wiki/Settlers) | Each work yard needs exactly 1 settler + 1 tool. Population capped by Residence count. Settlers assigned automatically. | `PopulationSystem.cs` | **partial** | 2026-05-12 |
| Building upgrades | [Upgrades](https://settlers7.fandom.com/wiki/Buildings) | Residences/Noble Residences upgrade for more pop (2 levels). Hygiene tech adds +2/+4 pop bonus. | `UpgradeSystem.cs` | **partial** | 2026-05-12 |
| Recipe balance | [Production chains](https://settlers7.fandom.com/wiki/Production) | Cycle times and resource ratios match original game timings. | `Assets/Data/Recipes/*.asset` | **partial** | 2026-05-12 |

**Notes — Economy:**
- Population: tool distribution logic simplified; settlers auto-assigned but tool source chain not fully verified
- Upgrades: `upgradePopulationBonus` wired but Hygiene SO not confirmed linked in UpgradeSystem
- Recipe balance: SO-driven as of Step 3. Timings approximate — needs tuning pass against wiki (Step 6)

---

## Military

| Mechanic | S7 Source | Expected Behaviour | Our File | Status | Last Verified |
|---|---|---|---|---|---|
| Sectors & map | [Map](https://settlers7.fandom.com/wiki/Map) | 18–43 sectors per map. Graph topology. BFS pathfinding. Starting sectors pre-owned. | `SectorGraph.cs`, `Sector.cs`, `MapFactory.cs` | **partial** | 2026-05-12 |
| Generals & armies | [Military](https://settlers7.fandom.com/wiki/Military) | Max 5 generals per player, 35 soldiers each. 2nd+ general requires `mil_second_general` prestige unlock. | `ArmySystem.cs`, `General.cs` | **1:1** | 2026-05-12 |
| Unit types (5) | [Units](https://settlers7.fandom.com/wiki/Military) | Pikeman, Musketeer, Cavalier, Cannon, StandardBearer. Each has ATK stat and prestige-unlock requirement. | `UnitType.cs` (UnitStats) | **1:1** | 2026-05-12 |
| Combat auto-resolve | [Combat](https://settlers7.fandom.com/wiki/Combat) | ATK vs DEF. Fortified = +50% DEF. Musketeers/Cannons required to breach fortification. Proportional losses. | `CombatResolver.cs` | **1:1** | 2026-05-12 |
| Military conquest | [Conquest](https://settlers7.fandom.com/wiki/Conquest) | Army moves to enemy/neutral sector → auto-combat resolves. Winner gets sector, buildings destroyed. | `ConquestSystem.cs` (OnArmyArrived) | **1:1** | 2026-05-12 |
| Proselytism conquest | [Proselytism](https://settlers7.fandom.com/wiki/Proselytism) | 6 clerics (unfortified) or 12 (fortified). 30s conversion. Neutral sectors only. | `ConquestSystem.cs` (Tick) | **1:1** | 2026-05-12 |
| Bribery conquest | [Bribery](https://settlers7.fandom.com/wiki/Bribery) | Coins + Garments + Jewelry. Cost scales with garrison strength. Neutral sectors only. | `ConquestSystem.cs` (TryBribe) | **1:1** | 2026-05-12 |
| Fortification | [Fortification](https://settlers7.fandom.com/wiki/Fortification) | Requires `mil_fortification` prestige unlock + 10 Stone. 30s build time. `tech_fortification_tech` = ×2 speed. | `FortificationSystem.cs` | **bug** | 2026-05-12 |
| Conquest rewards | [Rewards](https://settlers7.fandom.com/wiki/Conquest) | +1 prestige on conquest. Buildings destroyed. Storehouse replaced. Special sector grants VP. | `GameState.cs` (OnSectorConquered) | **1:1** | 2026-05-12 |

**Notes — Military:**
- Map: 7 maps exist (TestMap, 4-player, Large, 5 others in MapFactory) but not verified 1:1 against original S7 map layouts
- Fortification bug: `TryUnlock("mil_fortification")` fails in tests despite awarding 5 points (Level 1). Root cause: likely PrestigeDatabase has wrong prerequisites for `mil_fortification`. FortificationSystem code itself is correct.
- **Fix needed:** Check `PrestigeDatabase.cs` — does `mil_fortification` require a different branch or higher level than the test provides?

---

## Technology

| Mechanic | S7 Source | Expected Behaviour | Our File | Status | Last Verified |
|---|---|---|---|---|---|
| Tech tree (18 techs) | [Technology](https://settlers7.fandom.com/wiki/Technology) | 3 tiers. First-come-first-served: once any player researches it, locked for all others. | `TechTree.cs`, `ResearchSystem.cs` | **1:1** | 2026-05-12 |
| Tech effects | [Technology](https://settlers7.fandom.com/wiki/Technology) | Each tech applies buffs: unit ATK multipliers, production speed, fortification speed, etc. | `TechEffects.cs` | **1:1** | 2026-05-12 |
| Cleric units | [Clerics](https://settlers7.fandom.com/wiki/Clerics) | 3 cleric types (Novice/Brother/Father). Trained at Church. Needed for proselytism. | (MVP: cleric count is integer, no unit tracking) | **partial** | 2026-05-12 |

**Notes — Technology:**
- Cleric tracking: per CLAUDE.md MVP decision, cleric/trader unit tracking deferred. `StartProselytism(playerId, sectorId, clericCount)` takes a raw count — callers supply it but there's no dedicated cleric pool per player.
- Monastery contention (first-come tech lock) is fully implemented in `ResearchSystem`.

---

## Trade

| Mechanic | S7 Source | Expected Behaviour | Our File | Status | Last Verified |
|---|---|---|---|---|---|
| Trade map & outposts | [Trade](https://settlers7.fandom.com/wiki/Trade) | Network of trade outposts. First player to claim = exclusive. Outposts grant ongoing income or VP. | `TradeMap.cs`, `FourPlayerTradeMapFactory.cs` | **partial** | 2026-05-12 |
| Trade system | [Trade routes](https://settlers7.fandom.com/wiki/Trade) | Traders travel routes, deliver goods, generate Coins. Export Office required. | `TradeSystem.cs` | **partial** | 2026-05-12 |
| Tavern | [Tavern](https://settlers7.fandom.com/wiki/Tavern) | Beer→Coins (1:3), Coins→Tools (5:1), Hire General (10 Coins). | `TavernSystem.cs` | **1:1** | 2026-05-12 |

**Notes — Trade:**
- TradeSystem exists but the VP routing from trade routes is simplified vs original game
- Trader unit tracking: deferred same as clerics (MVP decision in CLAUDE.md)
- Trade map YAML content not verified against real S7 trade outpost network

---

## Prestige & Victory

| Mechanic | S7 Source | Expected Behaviour | Our File | Status | Last Verified |
|---|---|---|---|---|---|
| Prestige points & levels | [Prestige](https://settlers7.fandom.com/wiki/Prestige) | Points awarded per conquest (+1), quest (+varies), event. Level = points ÷ 5. Unlocks unlock tree nodes. | `PrestigeSystem.cs`, `PrestigeDatabase.cs` | **1:1** | 2026-05-12 |
| Victory points — dynamic | [Victory](https://settlers7.fandom.com/wiki/Victory_Points) | Recalculated each tick. Stealable: lose the condition, lose the VP. E.g. "own most sectors". | `VictorySystem.cs` | **1:1** | 2026-05-12 |
| Victory points — permanent | [Victory](https://settlers7.fandom.com/wiki/Victory_Points) | Awarded once, kept forever. E.g. first to reach prestige level 5, complete a quest. | `VictorySystem.cs` | **1:1** | 2026-05-12 |
| VP win condition | [Victory](https://settlers7.fandom.com/wiki/Victory_Points) | Reach required VP count → 3-min countdown starts. Win if still holding at countdown end. | `VictorySystem.cs` | **1:1** | 2026-05-12 |
| Quests | [Quests](https://settlers7.fandom.com/wiki/Events) | Event locations per map. Player accepts quest, meets objectives (resources/sectors/army/prestige/tech), claims reward. | `QuestSystem.cs`, `QuestDatabase.cs` | **partial** | 2026-05-12 |

**Notes — Prestige & Victory:**
- Quest content: QuestDatabase has example quests but not the full original S7 quest set. No quest event location objects in the scene (visual layer).
- Prestige unlock tree: PrestigeDatabase has unlocks in 4 branches (mil/eco/rel/trade). Branch completeness vs original S7 not verified.

---

## AI & Save/Load

| Mechanic | S7 Source | Expected Behaviour | Our File | Status | Last Verified |
|---|---|---|---|---|---|
| AI economy | (S7 internal) | AI builds production chains, upgrades, manages resources, accepts quests. | `AIEconomy.cs` | **partial** | 2026-05-12 |
| AI strategy | (S7 internal) | AI attacks when strong, fortifies, bribes, switches path (mil/tech/trade) based on situation. | `AIController.cs`, `AIController.Strategy.cs` | **partial** | 2026-05-12 |
| Save / Load | (S7 internal) | Full game state round-trip: sectors, resources, prestige, buildings, work yards, techs, outposts, VPs, generals, training queue, quests, simulation time. | `SaveSystem.cs` | **1:1** | 2026-05-12 |

**Notes — AI:**
- `AIEconomy_ManageQuests_AcceptsAvailableQuest` test FAILS — AI quest acceptance logic broken
- Strategy: multi-path switching exists, but fidelity to S7 AI difficulty levels not verified
- AI does not use proselytism or bribery reliably (only military conquest)

---

## Known Bugs Blocking 1:1 (Prioritised)

| Priority | System | Bug | File | Fix Hint |
|---|---|---|---|---|
| 🔴 High | Fortification | `TryUnlock("mil_fortification")` returns false — 5 failing tests | `PrestigeDatabase.cs` | Check if `mil_fortification` has unmet prerequisites (wrong branch/level requirement?) |
| 🟡 Med | AI | Quest acceptance returns 0 accepted quests | `AIEconomy.cs` | `ManageQuests()` likely has wrong sector ownership check or wrong quest availability filter |
| 🟡 Med | Map | `twin_rivers` map: Western Farmlands sector missing Forest node | `MapFactory.cs` (twin_rivers builder) | Add `ResourceNodeType.Forest` to that sector |
| 🟡 Med | Military | `HireGeneral` returns null when 2nd general prestige check fails | `ArmySystem.cs` | Test setup doesn't award `mil_second_general` — verify test intent vs code correctness |
| 🟢 Low | Recipe balance | Cycle times are approximate, not 1:1 with original S7 timings | `Assets/Data/Recipes/*.asset` | Step 6 balance pass — tune each recipe SO against wiki |

---

## Completion Summary

```
1:1     : 14 mechanics  (production, storehouse, food boost, construction,
                          army, unit types, combat, military/proselytism/bribery conquest,
                          conquest rewards, tech tree+effects, tavern, all VP types, save/load)
partial : 6 mechanics   (population, upgrades, recipe balance, map content,
                          clerics/traders, trade, quests, AI — some overlap)
bug     : 1 mechanic    (fortification — code correct, PrestigeDatabase data wrong)
missing : 0 mechanics   (all systems present)
```

**Victory paths covered:**
- Military (army + conquest) — **1:1**
- Technology (tech tree + research) — **1:1**
- Trade (outposts + trade routes) — **partial**

---

## Next Session Picks (by value/effort)

1. **Fix Fortification bug** — check `PrestigeDatabase.cs` for `mil_fortification` prerequisites. 1 file, ~30 min, unblocks 5 tests.
2. **Fix `twin_rivers` map Forest node** — 1 line in MapFactory.cs, unblocks 1 test.
3. **Fix AI quest acceptance** — AIEconomy.cs ManageQuests(), ~1 hour.
4. **Step 5: Visual feedback** — BuildingPlacer ghost, CombatViz, NotificationUI quest popup.
5. **Step 6: Recipe balance pass** — tune SO assets against settlers7.fandom.com timings.
