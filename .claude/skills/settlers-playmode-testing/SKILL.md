---
name: settlers-playmode-testing
description: "Proven MCP play-mode recipes: start/teardown games, force game states, screenshots, slow-motion, max-load scenario. Read before ANY play-mode verification, acceptance testing, or balance soak."
---

# Settlers Clone — Play-Mode Testing Skill

Every recipe here has been used successfully in a real session. Do not improvise a new
variant when one of these fits — improvised play-mode code is the #1 source of wasted
debugging time in this project.

## Ground Rules

1. **NEVER edit scripts while play mode is active.** Stop play → edit → refresh → compile-check → play again. (Violated twice; both times cost a broken domain reload.)
2. `execute_code` compiles with **CodeDom = C# 6**: no string interpolation edge cases, no `dynamic`, no local functions in expressions. Keep snippets simple.
3. New `.cs` files need `refresh_unity scope=all force` — a scripts-only refresh leaves CS0246.
4. `run_tests` occasionally reports "did not start within timeout" with 0 completed — transient, retry with `init_timeout=120000`. Not a real failure.
5. `Application.runInBackground = true` is set in BootstrapScene — required, else an unfocused editor freezes frameCount and MCP-driven validation silently stalls.
6. Console message "MCP-FOR-UNITY: Client handler error: Cannot access a disposed object" is harness noise, not a compile error.

## Starting a Game — ALWAYS via StartTrackedGame

**Never call `gc.StartGame(...)` directly** in play-mode validation. Every StartGame builds a
fresh EventBus (Engine Gotcha #1); direct starts leave VFX/audio/summary/campaign subscriptions
on the dead old bus — end screens and sounds silently vanish. `BootstrapScene.StartTrackedGame`
re-wires everything. Invoke it via reflection (it has optional parameters):

```csharp
var bs = UnityEngine.Object.FindAnyObjectByType<Settlers.Presentation.BootstrapScene>();
var mi = typeof(Settlers.Presentation.BootstrapScene).GetMethod("StartTrackedGame",
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var ps = mi.GetParameters();
object[] args = new object[ps.Length];
for (int i = 0; i < ps.Length; i++) args[i] = ps[i].DefaultValue;   // fill optionals!
args[0] = "twin_rivers"; args[1] = 2; args[2] = 4;                  // map, players, VP
mi.Invoke(bs, args);
```

Then hide the menu: `FindAnyObjectByType<MainMenuUI>(FindObjectsInactive.Include).Hide();`

## Forcing Game States

| Goal | Recipe |
|------|--------|
| Force a win | `AwardPermanentVP` × VPRequired for the player, then `Victory.Tick(500f)` **TWICE** (1st tick starts countdown, 2nd decrements — one big tick only STARTS it) |
| Place a building | `ConstructionSystem.PlaceBuilding(type, sectorId, ownerId, maxWorkYards, localX, localZ, currentBuildCount, maxSlots)` |
| Restore a built building instantly | `RestoreBuilding(type, sectorId, ownerId, maxWY, lx, lz, BuildingState.Complete, prog, upLevel, FoodSetting)` — returns the Building and fires `BuildingPlacedEvent` (views spawn) |
| Sectors of a player | `GetSectorsOwnedBy(p)` returns `List<int>` (ids, NOT sectors) |
| Fast-forward | loop `runner.Tick(0.5f)` — 2400× ≈ 20 sim-minutes |
| Building states | enum is Planned / UnderConstruction / **Complete** / Upgrading — there is NO "Operational" value (`IsOperational` is a property) |

## Screenshots

- Path must be under the project (`Application.dataPath + "/../Captures/..."`) — external paths fail.
- Hide `MainMenuUI` GO and disable extra `*Camera` components before framing.
- Set camera via reflection on `SettlerCamera`: `_target`, `_distance`, and `_enableEdgeScroll=false` (edge scroll drifts the camera to a map corner whenever the editor loses focus).
- **Catching carriers/units in motion:** `Time.timeScale = 0.02f` (adjacent-sector deliveries finish in seconds at 1×). Restore to `1f` after.
- **During long fast-forwards an AI may WIN** (~6 min on twin_rivers if the player idles) → the PostGameSummary overlay covers the screenshot. Hide it first: `FindAnyObjectByType<PostGameSummaryUI>(...).Hide()`.

## Max-Load Scenario (performance / soak baseline)

`the_frontier`, 4 players → 2400 × `runner.Tick(0.5f)` → fill with `RestoreBuilding` to ~200
buildings; AI attaches yards to the synthetic homes (settler budget applies) → ~340 yards.
This is the reference scene for the 60-fps bar (Sprint 8a hit 100+ fps here).

## Balance Soak (AI-vs-AI, for acceptance support)

Per match: StartTrackedGame with 0 human-relevant input → fast-forward in `runner.Tick(0.5f)`
batches → after each batch check `State.Victory` for game over → log winner id, victory path,
sim time, map → screenshot → teardown via a fresh StartTrackedGame (it tears down first).
Collect results as a table (map × path × duration). This feeds Tier 3 "balanced through real
playtests" without manual matches.

## Verification Order (every change, no exceptions)

`read_console` (errors) → EditMode tests ≥ baseline (currently 517) → play-mode screenshot of
the changed surface. Trust what ran, never "should compile".
