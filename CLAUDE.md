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
| Cost saving, session protocol, token budgets | `cost-saving` |

## Session Protocol

1. Read **CLAUDE.md** (this file) + **MEMORY.md** at session start
2. Read relevant skill(s) matching the session's layer
3. Update MEMORY.md at session end

## Workflow

1. Claude Code writes C# in `Assets/Scripts/`, Unity auto-recompiles
2. Visual verification in Unity Editor (Claude Code can't see the game)
3. NUnit tests in `Assets/Tests/Editor/` — run without Play Mode
4. SO .asset files created via Editor menu scripts (`Settlers > Generate All`)
5. Prefabs, materials, lighting, 3D models → user configures in Unity Editor

## Assembly Definitions
- `Settlers.Simulation` — Pure C#, `noEngineReferences: true`. Any `using UnityEngine` = compile error.
- `Settlers.Game` — Presentation + UI + Data layers. References Simulation + TMPro + InputSystem.
- `Settlers.Editor` — Editor-only scripts. References Simulation + Game.
- `Settlers.Tests` — Editor test assembly. References Simulation + NUnit.

## Game Design: §1 Map & Sectors

Maps are predefined, divided into sectors connected via a graph (18-43+ sectors). Maps support 1-4 players.

**Sector properties:** Owner (player/neutral/unowned), garrison strength, resource deposits (coal/iron/gold/stone), fertile land, forest, fishing, water, special buildings, event locations, build slots.

**Conquest — Three Methods:**
1. **Military** (neutral + enemy): General + army → auto-resolve combat. Fortified sectors need musketeers/cannons.
2. **Proselytism** (neutral only): ~6 clerics (unfortified) or ~12 (fortified). Clerics traverse neutral/enemy sectors.
3. **Bribery** (neutral only): Coins + Garments + Jewelry. Fastest but most expensive.

**Rewards:** +1 prestige + choice of reward package + sector resource access.

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

All 15 simulation systems complete. 103 script files + 23 test files.
Current phase and task queue tracked in MEMORY.md.

---

Game Design: §14 UI Reference (verified from original screenshots)

All German strings below are verified 1:1 from original Die Siedler 7 screenshots.
Use these EXACT strings in the German UI. Do not paraphrase or translate.

§14.1 Verified UI Strings (exact German text)
ContextExact stringCarrier idle — no goodsEinige Güter fehlen. Ich muss warten.Carrier impatientWer trödelt denn da? Ihr verschwendet meine Zeit!Prestige panel titlePRESTIGE-OPTIONEN (+ available points as green number)Reward panel titleBELOHNUNGENBuild menu titleBAUENVP overview titleÜBERSICHTProduction overview labelProduktionsübersicht: <Ware>Stats columnsERFORDERT / PRODUZIERT VON / ERBRINGT / VERBRAUCHT VON
§14.2 Victory Points — complete German names (14 VPs)
The VP ring shows all victory points. Player-held VPs are green; others silver/gray.
These map to the three paths (Military / Technology / Trade) plus economy VPs.
German VP namePath / CategoryWunderkindTechnology — first to research the Special TechnologyQuelle der WeisheitTechnology — research milestoneBischofssitzTechnology — church/clergyGeneralissimusMilitary — army/generalsSonnenkönigMilitary / PrestigeNebelsumpfSpecial sector (map event location)HandelsaußenpostenTrade — trade outpost (can appear per-player)HandelsgesellschaftTrade — trading companySparfuchsEconomy — coins/savings (Banker equivalent)ImperatorEconomy — sector count (Emperor)MetropoleEconomy — population (Metropolis)Spezieller SektorSpecial sector control (can appear per-player)

NOTE: Handelsaußenposten and Spezieller Sektor can appear twice in the ring —
once per relevant player/instance. The VP system must support the same VP category
being contested by multiple players simultaneously.

Wunderkind VP description (exact):
Seid der Erste, der über die Spezielle Technologie verfügt, um diesen Siegpunkt zu erhalten.
Dazu müsst Ihr in Eurer Kirche Geistliche anwerben und sie Technologien erforschen lassen.
VP overview filter categories (sidebar, exact strings):
Kriegsführung · Arbeitsgebäude · Verpflegung · Geologe · Konstruktion
§14.3 Conquest Reward Modal (solves reward-package selection)
After conquering a NEUTRAL sector, a BELOHNUNGEN modal appears with selectable
reward packages. The player picks exactly ONE. Verified layout:
BELOHNUNGEN
├── Bevölkerungsbelohnung      (1× population reward)
├── Eroberungsbelohnung        (conquest reward variant)
├── Eroberungsbelohnung        (conquest reward variant)
└── Eroberungsbelohnung        (conquest reward variant)
Implementation: 1 population-type package + 3 conquest-type packages, all clickable,
single-select. This is the reward-choice UI that must replace any auto-grant stub.
§14.4 Build Menu — three-tab structure (BAUEN)
The build menu is a small grid modal with THREE tabs:
TabIconCategoryContents1HouseBase/economy buildingsResidence, Noble Residence, Lodge, Farm, Mountain Shelter + variants (3×3 grid)2Shield/CastleSpecial & prestige-gatedLocked buildings shown as GRAY silhouettes (not hidden)3CrownPrestige objects / militaryStronghold, Church, Export Office + prestige objects; locked = gray silhouette

KEY RULE: Locked buildings render as gray silhouettes, NOT hidden. The player can
see what they'll unlock. Prestige-level gates which tab-3 buildings are buildable.

§14.5 Prestige Panel (PRESTIGE-OPTIONEN)
Opens via the crown icon. Title shows PRESTIGE-OPTIONEN + available points (green number).
Layout: rows, each with a left main option (gold frame = activatable) and a right
upgrade chain of 2–3 stages (connected by a line; locked stages grayed out).
Left-column option types observed (by icon):

Hammer icon → Construction upgrades
Figures icon → Population/Residence upgrades
Up-arrow icon → Sector/tier upgrades

Right column: staged upgrade chains (e.g. Residence → upgraded → max), each stage
unlocked individually as prestige level rises.
§14.6 Technology Tree (Monastery research)
Dark stone background with candle holders. Each tech is a card showing research cost
in the format Geistliche / Mönche / Prälaten (clerics / monks / prelates).
Verified tech costs (clerics/monks/prelates):
Tech (visual)CostHolzfäller (woodcutter)3/0/0Stiefel (boots)3/0/0Rüstung (armor)3/0/0Fernrohr (telescope)4/1/0Kanone (cannon)4/1/0Fischerei (fishing)4/2/0Alchemist4/2/0Medkit4/2/0Statue5/2/1Stein (stone)5/2/1Schriftrolle (scroll)5/2/1Ritter (knight)5/2/1Star tech (center)9/7/5 (most expensive)

Gold-framed techs = VP techs (Wunderkind mechanic: first to research gets the VP).
Tier scales with the 3-cleric-type cost. Center star tech (9/7/5) is the apex.

§14.7 Trade Map (Handelskarte)
Full-screen parchment world map (compass rose, lat/long grid, dashed connection lines).
Network graph of trade nodes:

Player capital: castle icon, gold frame (center)
Trade outpost: small card with input→output goods + trader count + coin cost
Treasure node: chest icon with 2–3 coins
Empty outpost: gray frame, no trader assigned (claimable)
Enemy outpost: red frame (claimed by opponent — exclusive)

Node format: [input good] → [output good]  [trader count]
Example values observed: 4 → 6 [2], 3 → 6 [2], 2 → 4 [2]

Outposts are first-come-first-served (already a locked design rule). The map UI must
show claimed (red), claimable (gray), and player-owned (gold) states distinctly.

§14.8 HUD layout (top bar)
Top-center resource bar (left to right): population 34/46, coins, weapons/tools,
tools, food. Prestige level shown as star with number (bottom center, e.g. star "6").
Bottom action bar icons (left to right): map/scaffolding, crown (prestige),
treasure/coins, stats book, star+level, chest/shield (military overview),
globe (trade map), swords (combat), cross/T (church/tech).
§14.9 Visual style notes for asset replacement

Sector borders = physical stone walls (3D objects, not abstract lines). The wall
runs the full perimeter of each owned sector.
Roads: unpaved dirt by default; paved stone roads only after the prestige unlock.
Terrain: fertile land (green grass) vs infertile (red/sand) clearly distinct.
Single trees inside sectors, not just at borders. Rocks as natural borders + decoration.
Main castle: multi-story, red flags, gold accents — dominates the home sector visually.
Enemy stronghold: massive fortified structure with red flags + stone walls,
visible units (soldiers) on paths, cannon emplacements.
Lighting (original art direction): shadows tend toward cool saturated tones,
lit areas bathed in warm colors — "dreamy fairy tale look." Aim for this in URP.

---

**The economy is the game. Everything — military, technology, trade, victory — is powered by production chains flowing through storehouses. Get the storehouse relay and food boosting right, and everything else falls into place.**
