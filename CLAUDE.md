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
| Screenshot workflow, UI reference, visual fidelity | `settlers-7-fidelity` |
| Cost saving, session protocol, token budgets | `cost-saving` |

## Session Protocol

1. Read **CLAUDE.md** (this file) + **MEMORY.md** at session start
2. Read relevant skill(s) matching the session's layer
3. **Screenshot workflow:** user provides screenshots → analyse HUD, buildings, UI → derive concrete tasks
4. Update MEMORY.md at session end

## Workflow

1. Claude Code writes C# in `Assets/Scripts/`, Unity auto-recompiles
2. Visual verification via user screenshots from Unity Editor Play Mode
3. NUnit tests in `Assets/Tests/Editor/` — run without Play Mode
4. SO .asset files created via Editor menu scripts (`Settlers > Generate All`)
5. Prefabs, materials, lighting, 3D models → user configures in Unity Editor

## Assembly Definitions

- `Settlers.Simulation` — Pure C#, `noEngineReferences: true`. Any `using UnityEngine` = compile error.
- `Settlers.Game` — Presentation + UI + Data layers. References Simulation + TMPro + InputSystem.
- `Settlers.Editor` — Editor-only scripts. References Simulation + Game.
- `Settlers.Tests` — Editor test assembly. References Simulation + NUnit.

---

## Game Design: §1 Map & Sectors

Maps are predefined, divided into sectors connected via a graph (18-43+ sectors). Maps support 1-4 players.

**Sector properties:** Owner (player/neutral/unowned), garrison strength, resource deposits (coal/iron/gold/stone), fertile land, forest, fishing, water, special buildings, event locations, build slots.

**Conquest — Three Methods:**
1. **Military** (neutral + enemy): General + army → auto-resolve combat. Fortified sectors need musketeers/cannons.
2. **Proselytism** (neutral only): ~6 clerics (unfortified) or ~12 (fortified). Clerics traverse neutral/enemy sectors.
3. **Bribery** (neutral only): Coins + Garments + Jewelry. Fastest but most expensive.

**Rewards:** +1 prestige + choice of reward package (see §14.3) + sector resource access.

---

## Critical Rules (Prevent Bugs)

1. **Sector graph, not hex grid** — discrete sectors with terrain inside each
2. **Food boost halts on empty** — toggled on + no food = production STOPS (no fallback)
3. **Noble Residence needs food to function** — no food = all work yards idle
4. **All goods flow through storehouses** — workers never carry between buildings directly
5. **Technologies are first-come-first-served** — once researched, permanently locked for all others
6. **Trade outposts are first-come-first-served** — once claimed, exclusive to that player
7. **VPs can be dynamic (stealable) or permanent** — implement both types
8. **Each work yard needs 1 settler + 1 tool** — no tool = work yard idle
9. **Locked buildings show as gray silhouettes** — never hidden from the build menu
10. **Reward modal is player-choice** — never auto-grant; always show BELOHNUNGEN modal after neutral conquest

---

## Victory Overview

- **Military:** Stronghold → 5 unit types → Generals (35 soldiers, 5 max) → auto-combat
- **Technology:** Church → 3 cleric types → Monasteries (shared, blockable) → 18 techs in 3 tiers
- **Trade:** Export Office → 3 trader types → Trade Map (network graph, first-come outposts)
- **Win:** reach required VP count → 3-min countdown → win if still held

---

## Game Design: §14 UI Reference (verified from original screenshots)

> All German strings are verified 1:1 from original Die Siedler 7 screenshots (2026-06-10).
> Use these EXACT strings in the UI. Do not paraphrase or translate.

### §14.1 Verified UI Strings (exact German text)

| Context | Exact string |
|---------|-------------|
| Carrier idle — no goods | `Einige Güter fehlen. Ich muss warten.` |
| Carrier impatient | `Wer trödelt denn da? Ihr verschwendet meine Zeit!` |
| Prestige panel title | `PRESTIGE-OPTIONEN` (+ available points as green number) |
| Reward panel title | `BELOHNUNGEN` |
| Build menu title | `BAUEN` |
| VP overview title | `ÜBERSICHT` |
| Production overview label | `Produktionsübersicht: <Ware>` |
| Stats panel columns | `ERFORDERT` / `PRODUZIERT VON` / `ERBRINGT` / `VERBRAUCHT VON` |

### §14.2 Victory Points — complete German names (14 VPs)

The VP ring shows all VPs. Player-held = green highlight; others = silver/gray.

| German VP name | Path / Category |
|----------------|-----------------|
| `Wunderkind` | Technology — first to research the Special Technology |
| `Quelle der Weisheit` | Technology — research milestone |
| `Bischofssitz` | Technology — church/clergy |
| `Generalissimus` | Military — army/generals |
| `Sonnenkönig` | Military / Prestige |
| `Nebelsumpf` | Special sector (map event location) |
| `Handelsaußenposten` | Trade — trade outpost (appears per-player) |
| `Handelsgesellschaft` | Trade — trading company |
| `Sparfuchs` | Economy — coins/savings (Banker equivalent) |
| `Imperator` | Economy — sector count (Emperor) |
| `Metropole` | Economy — population (Metropolis) |
| `Spezieller Sektor` | Special sector control (appears per-player) |

> `Handelsaußenposten` and `Spezieller Sektor` can appear **twice** in the ring —
> once per relevant player/instance. The VP system must support the same VP category
> being held by different players simultaneously.

**Wunderkind VP — exact description text:**
```
Seid der Erste, der über die Spezielle Technologie verfügt, um diesen Siegpunkt zu erhalten.
Dazu müsst Ihr in Eurer Kirche Geistliche anwerben und sie Technologien erforschen lassen.
```

**VP overview sidebar filter labels (exact):**
`Kriegsführung` · `Arbeitsgebäude` · `Verpflegung` · `Geologe` · `Konstruktion`

### §14.3 Conquest Reward Modal (`BELOHNUNGEN`)

After conquering a **neutral** sector, a modal appears. Player picks **exactly one** package.

```
BELOHNUNGEN
├── Bevölkerungsbelohnung      (population reward — 1 option)
├── Eroberungsbelohnung        (conquest reward variant A)
├── Eroberungsbelohnung        (conquest reward variant B)
└── Eroberungsbelohnung        (conquest reward variant C)
```

Implementation: 1 population-type + 3 conquest-type packages, all clickable, single-select.
**Never auto-grant.** This is Critical Rule #10.

### §14.4 Build Menu — three-tab structure (`BAUEN`)

Small grid modal (3x3 icons per tab), three tabs:

| Tab | Icon | Category | Contents |
|-----|------|----------|----------|
| 1 | House | Base / economy buildings | Residence, Noble Residence, Lodge, Farm, Mountain Shelter + variants |
| 2 | Shield | Prestige-gated specials | Locked = gray silhouette (not hidden) |
| 3 | Crown | Military / prestige objects | Stronghold, Church, Export Office; locked = gray silhouette |

Locked buildings always render as **gray silhouettes** — the player sees what unlocks at higher prestige.

### §14.5 Prestige Panel (`PRESTIGE-OPTIONEN`)

Opens via crown icon in bottom bar. Title: `PRESTIGE-OPTIONEN` + green number (available points).

Layout — each row has:
- **Left column:** main option with gold frame (activatable when points available)
- **Right column:** upgrade chain of 2-3 stages connected by a line; locked stages grayed out

Left-column option types (by icon):
- Hammer → construction upgrades
- Figures → population / residence upgrades
- Up-arrow → sector / tier upgrades

### §14.6 Technology Tree (Monastery research)

Dark stone background, candle holders on the sides. Each technology = a card.
Research cost format: **Geistliche / Mönche / Prälaten** (clerics / monks / prelates).

| Technology | Cost G/M/P |
|------------|------------|
| Holzfäller (woodcutter) | 3/0/0 |
| Stiefel (boots) | 3/0/0 |
| Rüstung (armor) | 3/0/0 |
| Fernrohr (telescope) | 4/1/0 |
| Kanone (cannon) | 4/1/0 |
| Fischerei (fishing) | 4/2/0 |
| Alchemist | 4/2/0 |
| Medkit | 4/2/0 |
| Statue | 5/2/1 |
| Stein (stone) | 5/2/1 |
| Schriftrolle (scroll) | 5/2/1 |
| Ritter (knight) | 5/2/1 |
| Star tech (center node) | 9/7/5 (apex) |

Gold-framed cards = VP techs (Wunderkind mechanic: first to research wins the VP).

### §14.7 Trade Map (`Handelskarte`)

Full-screen parchment world map (compass rose, lat/long grid, dashed connection lines).

| Node state | Visual | Meaning |
|------------|--------|---------|
| Player capital | Castle icon, gold frame | Home base (center) |
| Player outpost | Card, gold frame | Claimed by this player |
| Empty outpost | Card, gray frame | Claimable — no trader assigned |
| Enemy outpost | Card, red frame | Exclusive to opponent |
| Treasure node | Chest icon, 2-3 coins | Loot node |

Node card format: `[input good] → [output good]  [trader count]`
Example values from screenshot: `4 → 6 [2]`, `3 → 6 [2]`, `2 → 4 [2]`

### §14.8 HUD Layout Reference

**Top bar (center):** `[population current/max]  [coins]  [weapons]  [tools]  [food]`
Example: `34/46  0  16  12  24`

**Bottom action bar (left to right):**
`map` · `crown (prestige)` · `treasure` · `stats book` · `star level` · `military overview` · `trade map` · `combat` · `church/tech`

Prestige level shown as star + number on the bottom bar (e.g. star 6).
Player VP badges: floating portrait + colored circle + VP count. Red = enemy, green = own/allied.

### §14.9 Resource Inventory — verified goods list

| Category | Goods |
|----------|-------|
| Wood chain | Holz, Planken, Kohle |
| Stone chain | Stein |
| Food chain | Getreide, Mehl, Brot, Bier, Fisch, Fleisch, Würste, Wasser |
| Luxury | Gewürz, Wein, Schmuck, Kleidung, Pelz, Leder |
| Military | Waffen, Kanonen, Kanonenkugeln, Schwerter |
| Economy | Münzen, Werkzeug |

### §14.10 Visual Style Reference (for asset replacement pass)

- **Sector borders:** Physical stone-wall 3D objects — not abstract lines. Wall runs the full perimeter of every owned sector.
- **Roads:** Unpaved dirt by default. Paved stone roads unlocked via prestige upgrade only.
- **Terrain:** Green grass (fertile) clearly distinct from red/sandy soil (infertile). Single trees inside sectors, rocks as natural borders and decoration.
- **Home castle:** Multi-story, red flags, gold accents — dominates the home sector visually.
- **Enemy stronghold:** Large fortified structure, red flags, stone walls, soldiers visible on paths, cannon emplacements.
- **Lighting / art direction:** Shadows toward cool saturated tones, lit areas in warm colors. Target: "dreamy fairy tale look" (original art director mandate). Implement via URP lighting + color grading post-processing.

---

## Project State

All 15 simulation systems complete. 158 script files + 33 test files. All 10 roadmap phases committed. CI green.
Current phase and open tasks tracked in MEMORY.md.

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
