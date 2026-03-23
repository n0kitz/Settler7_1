# Siedler Clone — CLAUDE.md

A faithful recreation of Die Siedler 7's gameplay systems in Unity 6. Local only, no server, no monetization. Personal project built with Claude Code + Unity MCP.

## Tech Stack

- **Engine:** Unity 6 (URP), C# 9+
- **Target:** 60 FPS, single-player + AI opponents, PC/Mac standalone
- **No DOTS/ECS** — 200-500 entities don't benefit; use MonoBehaviour + SO + pure C#

## Three-Layer Architecture

| Layer | Location | Rule |
|-------|----------|------|
| **Data** | `Assets/Data/` | ScriptableObjects — editable in Inspector, no hardcoded values |
| **Simulation** | `Assets/Scripts/Simulation/` | Pure C# — NO UnityEngine (except Vector3/Mathf). NUnit testable. |
| **Presentation** | `Assets/Scripts/Presentation/` + `UI/` | MonoBehaviour — reads state via `GameController.Instance`, never modifies simulation directly |

## Code Style

- **Namespaces:** `Settlers.Simulation`, `Settlers.Presentation`, `Settlers.UI`, `Settlers.Data`
- Classes: `PascalCase` | Variables: `camelCase` | Constants: `UPPER_SNAKE_CASE` | Private fields: `_camelCase`
- One class per file, filename matches class. No file over 300 lines.
- XML docs on all public APIs. Use `[SerializeField] private` for Inspector fields.
- All magic numbers in GameConstants SO. All definitions in individual SO assets.

## Skill Reference

| Topic | Skill to read |
|-------|---------------|
| Game mechanics (buildings, production, food, military, trade, VPs) | `settlers-game-design` |
| Architecture patterns, SO patterns, testing, system update order | `settlers-unity-architecture` |
| Camera, terrain, building visuals, lighting, UI layout | `settlers-unity-visuals` |
| Map design, sector layouts, batch asset creation | `settlers-map-content` |

## Workflow

1. Claude Code writes C# in `Assets/Scripts/`, Unity auto-recompiles
2. Visual verification in Unity Editor (Claude Code can't see the game)
3. NUnit tests in `Assets/Tests/Editor/` — run without Play Mode
4. SO .asset files created via Editor menu scripts (`Settlers > Generate All`)
5. Prefabs, materials, lighting, 3D models → user configures in Unity Editor

## Game Design: §1 Map & Sectors

Maps are predefined, divided into sectors connected via a graph (18-43+ sectors). Maps support 1-4 players.

**Sector properties:** Owner (player/neutral/unowned), garrison strength, resource deposits (coal/iron/gold/stone), fertile land, forest, fishing, water, special buildings, event locations, build slots.

**Conquest — Three Methods:**
1. **Military** (neutral + enemy): General + army → auto-resolve combat. Fortified sectors need musketeers/cannons.
2. **Proselytism** (neutral only): ~6 clerics (unfortified) or ~12 (fortified). Clerics traverse neutral/enemy sectors.
3. **Bribery** (neutral only): Coins + Garments + Jewelry. Fastest but most expensive.

**Rewards:** +1 prestige + choice of reward package + sector resource access.

## Game Design: §2–§13

All game mechanics are documented in the `settlers-game-design` skill. **Always read it before implementing or modifying game logic.**

## Critical Rules (Prevent Bugs)

1. **Sector graph, not hex grid** — discrete sectors with terrain inside each
2. **Food boost halts on empty** — toggled on + no food = production STOPS (no fallback)
3. **Noble Residence needs food to function** — no food = all work yards idle
4. **All goods flow through storehouses** — workers never carry between buildings directly
5. **Technologies are first-come-first-served** — once researched, permanently locked for all others
6. **Trade outposts are first-come-first-served** — once claimed, exclusive to that player
7. **VPs can be dynamic (stealable) or permanent** — implement both types
8. **Each work yard needs 1 settler + 1 tool** — no tool = work yard idle

## Victory Overview

- **Military:** Stronghold → 5 unit types → Generals (35 soldiers, 5 max) → auto-combat
- **Technology:** Church → 3 cleric types → Monasteries (shared, blockable) → 18 techs in 3 tiers
- **Trade:** Export Office → 3 trader types → Trade Map (network graph, first-come outposts)
- **Win:** reach required VP count → 3-min countdown → win if still held

## Project State

See memory files for current phase status, file list, and architecture decisions. 88 scripts + 21 tests = 109 C# files as of 2026-03-23.

---

**The economy is the game. Everything — military, technology, trade, victory — is powered by production chains flowing through storehouses. Get the storehouse relay and food boosting right, and everything else falls into place.**
