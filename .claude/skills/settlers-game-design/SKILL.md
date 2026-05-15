---
name: settlers-game-design
description: "Game design spec: buildings, production chains, food boosting, logistics, military, technology, trade, prestige, VPs. Read before implementing game mechanics."
---

# Settlers 7 Clone — Game Design Spec

Read this skill before implementing ANY game system. This is the authoritative reference for all mechanics not already covered in CLAUDE.md §1.

## The Core Rule

**The economy is the game.** Everything — military, technology, trade, victory — is powered by production chains flowing through storehouses. Food boosting is the dominant economic lever.

## Critical Constants (GameConstants ScriptableObject)

```
prestigePointsPerLevel = 5
maxWorkYardsPerBuilding = 3
carrierMaxItems = 3
victoryCountdownSeconds = 180
maxSoldiersPerGeneral = 35
maxGenerals = 5
residenceBasePop = 4       (upgrade +4 each, max 12, +2 with Hygiene)
nobleResidenceBasePop = 5  (upgrade +5 each, max 15, +4 with Hygiene)
plainFoodMultiplier = 2
fancyFoodMultiplier = 3
```

## 8 Rules That Prevent Bugs

1. **Sector graph, not hex grid** — map is discrete sectors with terrain inside each
2. **Food boost halts on empty** — toggled on + no food = production STOPS (no fallback)
3. **Noble Residence needs food to function** — no food = all work yards idle
4. **All goods flow through storehouses** — workers never carry between buildings directly
5. **Technologies are first-come-first-served** — once researched, permanently locked for all others
6. **Trade outposts are first-come-first-served** — once claimed, exclusive to that player
7. **VPs can be dynamic (stealable) or permanent** — implement both types
8. **Each work yard needs 1 settler + 1 tool** — no tool = work yard idle

---

## §2 Buildings & Work Yards

Each building costs resources, houses 1 population unit, and provides work yard slots.

**Lodge (3 Planks, 1 pop):** Forester, Woodcutter, Sawmill, Fisher, Hunter
**Farm (3 Planks, 1 pop):** Grain Barn, Windmill, Piggery, Shepherd, Stable
**Mountain Shelter (2 Planks + 1 Stone, 1 pop):** Quarry, Coal Miner, Iron Miner, Gold Miner, Iron Smelter, Coking Plant
**Residence (2 Planks + 1 Stone, 4 pop):** Bakery, Brewery, Paper Mill, Weaving Mill, Wheelwright, Toolmaker
**Noble Residence (3 Planks + 2 Stone, 5 pop):** Butcher, Blacksmith, Mint, Goldsmith, Bookbinder, Tailor

**Special buildings (predefined, not built by player):**
- Stronghold — military HQ, enables Generals
- Church — enables clerics + research
- Export Office — enables traders + trade map
- Monastery — shared research building (first-come-first-served)
- Trade Post — trade map outpost (first-come-first-served)
- Noble Residence (existing in sector) — can be food-boosted

---

## §3 Production Chains

```
Raw:        Wood, Stone, Coal, Iron Ore, Gold Ore, Grain, Fish, Animal, Water, Wool
Processed:  Planks, Iron Bars, Flour, Bread, Sausages, Beer, Paper, Books,
            Cloth, Garments, Coins, Weapons, Tools, Wheels, Horses, Jewelry
```

**Key chains:**
- Wood → Planks (Sawmill)
- Iron Ore + Coal → Iron Bars (Iron Smelter)
- Grain → Flour → Bread (Windmill → Bakery)
- Animal → Sausages (Piggery → Butcher)
- Grain → Beer (Windmill → Brewery)
- Iron Bars + Coal → Weapons (Blacksmith)
- Iron Bars → Tools (Toolmaker)
- Gold Ore → Coins (Mint)
- Cloth → Garments (Tailor)
- Gold Ore + Coins → Jewelry (Goldsmith)
- Paper → Books (Bookbinder)
- Paper = Wood pulp (Paper Mill)

---

## §4 Food Boosting

Each building can have food boosting toggled on/off per work yard. Food is consumed from the sector storehouse each production cycle.

| Building Type | No Food | Plain Food | Fancy Food |
|---------------|---------|------------|------------|
| Lodge / Farm / Mountain Shelter / Residence | ×1 | ×2 | ×3 |
| Noble Residence | **IDLE** | ×1 | ×2 |

**Plain food:** Bread, Fish, Sausages
**Fancy food:** Sausages + Beer (must have both)

**Critical:** If food boost is toggled ON and food runs out → production halts immediately. No fallback to ×1. The player must manually turn off boosting or resupply.

---

## §5 Storehouse & Logistics

- Every sector has one storehouse (auto-created)
- All goods produced anywhere in the sector go to the sector storehouse
- Workers carry goods from storehouse to work yard inputs (not building-to-building)
- Carriers transport goods between sector storehouses across roads
- Carrier capacity: `carrierMaxItems = 3` per trip
- Roads between adjacent sectors are required for carrier routes
- Paved roads (prestige unlock) double carrier speed

**Relay rule:** Goods can relay through multiple storehouses across sectors to reach their destination. Relay is triggered when a sector needs a resource it doesn't produce.

---

## §6 Population & Settlers

- Population lives in Residences and Noble Residences
- Each work yard needs exactly 1 settler + 1 tool to operate
- Settlers are assigned automatically from available population
- Tools come from storehouse inventory (produced by Toolmaker)
- No settler → work yard idle. No tool → work yard idle.
- Population cap set by residence count × pop per residence

---

## §7 Military

**Units (trained in Stronghold):** Militia, Bowman, Swordsman, Musketeer, Cannon
**Generals:** Hold up to 35 soldiers (mix of unit types), max 5 generals per player
**Combat:** Auto-resolved when General enters enemy/neutral sector
- Fortified sectors require Musketeers or Cannons
- Combat outcome based on ATK/DEF stats of unit composition

**Unit costs & stats defined in GameConstants.**

---

## §7 Technology

- **Church** required to train clerics and research techs
- **3 cleric types:** Standard, Senior, Arch (increasing research speed)
- **18 technologies in 3 tiers** (tier 1 unlocks tier 2, etc.)
- **Monasteries:** Shared buildings. First player to send a cleric claims research slot.
- Technologies are **first-come-first-served** — once researched by any player, locked for all

**Tech effects** include production bonuses, population bonuses, military bonuses, prestige unlocks.

---

## §7 Trade

- **Export Office** required to send traders
- **3 trader types:** Standard, Senior, Master (increasing speed/capacity)
- **Trade Map:** Network graph of outposts connected by routes
- **Outposts:** First-come-first-served, exclusive to claiming player
- Traders travel routes, claim outposts, exchange goods at trade prices
- Tavern exchanges: Beer→Coins, Coins→Tools, Coins→Hire General

---

## §8 Prestige

- Prestige points earned by: conquering sectors (+1), completing quests, specific VPs
- Prestige levels unlock abilities: paved roads, fortifications, Noble Residence food boost, extra carriers, etc.
- `prestigePointsPerLevel = 5` — level up every 5 points
- Prestige unlocks are **per player**, not global

---

## §9 Victory Points

**Dynamic VPs (stealable — lost if condition drops):**
| VP | Condition |
|----|-----------|
| Field Marshal | ≥20 total soldiers |
| Metropolis | ≥25 employed workers |
| Emperor | ≥3 owned sectors (after starting sector) |
| Banker | ≥25 coins in storehouse |
| Sun King | ≥5 prestige levels |
| Trading Company | ≥5 claimed trade outposts |
| Fountain of Knowledge | ≥3 researched technologies |
| Pacifist | ≥10 min without attacking |
| Economist | ≥75% work yard staffing |
| Generalissimo | ≥20 units killed |

**Permanent VPs (kept once earned):**
- Abbey (research monastery), Genius (research tier 3 tech), Special Sector conquest, Special Trade Outpost claimed, Domination (hold all sectors), Quest completion VPs

**Win condition:** Reach required VP count → 3-minute countdown → win if still held when countdown expires.

---

## §10 Tavern

Tavern is a special sector building (predefined, not built by player).
- Beer → Coins exchange (1 Beer = 3 Coins)
- Coins → Tools exchange (5 Coins = 1 Tool)
- Coins → Hire General (10 Coins, must have < maxGenerals)

---

## §11 Fortifications

- Costs 10 Stone, requires prestige unlock
- Fortified sectors need Musketeers or Cannons to conquer (military) or ~12 clerics (proselytism)
- Fortification reduces garrison loss rate during enemy attack
- One fortification per sector

---

## §12 Economy Buildings (Predefined)

Some buildings exist in the map by default (not built by player):
- **Stronghold** — player HQ, cannot be destroyed
- **Tavern** — beer/coin/tool exchange
- **Church** — enables clerics
- **Export Office** — enables traders
- **Monastery** — shared research site
- **Noble Residence** — already staffed in some sectors

---

## §13 Quests & Events

- Quest system tracks progress toward specific milestones (e.g., "produce 10 bread", "train 5 soldiers")
- Completing quests awards VP and/or prestige
- Event locations are special sector properties that trigger quests when conquered
- Quest definitions are in `QuestDatabase` ScriptableObject-equivalent

---

## Food Boosting Quick Reference (summary)

| Building | No Food | Plain | Fancy |
|----------|---------|-------|-------|
| Lodge/Farm/MtShelter/Residence | ×1 | ×2 | ×3 |
| Noble Residence | **IDLE** | ×1 | ×2 |
