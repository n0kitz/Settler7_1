---
name: settlers-debugging
description: "Debugging protocol + the complete trap catalogue (simulation, presentation, tooling) — every entry cost a real debugging session. Read FIRST when anything behaves unexpectedly, before forming a theory."
---

# Settlers Clone — Debugging Skill

Before theorizing, check this catalogue — most "mysteries" in this project have been solved
once already. CLAUDE.md → Engine Gotchas holds the 4 canonical traps; this skill is the
complete field guide.

## Protocol

1. `read_console` (errors first, then warnings). Known noise: "MCP-FOR-UNITY: Client handler
   error: Cannot access a disposed object" and the Coplay toolbar warning — ignore both.
2. **Check this catalogue** for a matching symptom.
3. Simulation bug → reproduce as an EditMode NUnit test FIRST (pure C#, fastest loop), fix,
   keep the test.
4. Presentation/visual bug → play-mode reproduction via `settlers-playmode-testing` recipes.
5. Unknown perf drop → bisection method in `settlers-performance`.
6. New trap found → add it HERE (and to CLAUDE.md Engine Gotchas if it's canonical-tier).

## Simulation Traps

| Symptom | Cause |
|---------|-------|
| Fallback/carrier test expects idle carrier but one is free | `Storehouse.CarrierCount = Level + 1` — level 1 has **2** carriers; occupy ALL in a loop before asserting fallback |
| Goods counted twice after production | Routed goods credit ONLY on `CarrierDeliveryEvent`; `RouteDelivery` returning true must NOT also credit immediately (the delivery handler already adds) |
| Victory never triggers in test despite huge Tick | Countdown needs TWO ticks: first starts, second decrements (Engine Gotcha #4) |
| CS0117 `BuildingState.Operational` | Enum is Planned/UnderConstruction/**Complete**/Upgrading; `IsOperational` is a property |
| Old saves break after adding a resource | `ResourceType` is save-safe ONLY append-at-END (saves serialize by NAME) |
| AI economy starves despite many buildings | Settler math: each work yard = 1 settler + 1 tool; utility buildings house 1 pop but carry 3 yard slots → cap attachment to `GetAvailableSettlers` |
| Sim test suddenly needs UnityEngine | Forbidden — asmdef blocks it. The design is wrong, not the asmdef |

## Presentation Traps

| Symptom | Cause |
|---------|-------|
| Sounds/end screen/VFX silently dead after restart | Fresh EventBus per StartGame — subscriptions died with the old bus; only `StartTrackedGame` re-wires (Engine Gotcha #1) |
| "New game" keeps the old game | Missing `TeardownGame()` — `InitializeGame` no-ops while `_gameRunning` (Engine Gotcha #2) |
| Procedural mesh invisible | CCW winding → faces down → backface-culled; wind clockwise-seen-from-above; debug with `_Cull=0` (Engine Gotcha #3) |
| `FindAnyObjectByType` can't find a panel | Default excludes inactive GOs → pass `FindObjectsInactive.Include` |
| A polling UI (Update-based) never opens | Component sits ON the deactivated panel GO → needs an always-active wrapper root (full-screen stretched, or the child lands bottom-left) |
| Building appears at wrong world position | LocalX/LocalZ are sector-LOCAL; storing world coords there is the classic silent inconsistency |
| AI/loaded entities have no views | Views must spawn from events (`BuildingPlacedEvent`), not from the human input path |
| Stale figures/views accumulate | View manager lacks the alive-set prune (see `settlers-performance` Rule 4) |
| One string stays in the old language after locale switch | Factory-baked text without the Show()-refresh pattern (see `settlers-localization`) |
| Object counts double right after Destroy | `Destroy()` is deferred — settle one frame before asserting |
| Camera drifts to a map corner during MCP runs | Edge scroll while editor unfocused → disable `_enableEdgeScroll` via reflection |

## Tooling & Environment Traps

| Symptom | Cause / Fix |
|---------|-------------|
| CS0246 after creating a new .cs via MCP | Use `refresh_unity scope=all force` — scripts-only refresh misses new files |
| `run_tests` "did not start within timeout", 0 completed | Transient — retry with `init_timeout=120000` |
| Piped `grep`/`sort`/`awk` fail with "unknown option" | zsh alias quirk on this machine — use absolute paths `/usr/bin/grep` etc., or write to a temp file |
| Escaped `\,` shows up literally in UI text | CSV parser splits on FIRST comma only — never escape commas (see `settlers-localization`) |
| coplay-mcp "Unity Editor is not running at project root" | Call `set_unity_project_root` first — roots reset between MCP restarts. Note: `com.coplaydev.coplay` (Coplay product, cloud sign-in) ≠ `com.coplaydev.unity-mcp` (MCP-for-Unity, the always-working bridge) |
| Coplay generation returns 401 | Not a bug — Normen must sign in inside the Coplay panel (may need credits) |
| Everything broke mid-edit | Scripts were edited while play mode was active — stop play BEFORE editing, always |
| MCP validation stalls, frameCount frozen | Editor unfocused without `Application.runInBackground = true` (BootstrapScene sets it — check it wasn't removed) |

## When a Fix Is Found

- Simulation fix → ships with a regression test, same session.
- Trap was general → new row in the table above; canonical-tier → also CLAUDE.md Engine Gotchas.
- Symptom was misdiagnosed at first → record the WRONG theory too (e.g. 8a: "shadows" looked
  guilty, MPB batching was the cause) — it saves the next session from the same detour.
