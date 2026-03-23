---
name: settlers-game-design
description: "Game design spec: buildings, production chains, food boosting, logistics, military, technology, trade, prestige, VPs. Read before implementing game mechanics."
---

# Settlers 7 Clone — Game Design Skill

Before implementing ANY game system, read the relevant section in CLAUDE.md first. This skill is the quick-reference index.

## Where To Find What

| Topic | CLAUDE.md Section |
|-------|-------------------|
| Map structure, sectors, conquest | §1 |
| All buildings, work yards, costs | §2 |
| Every production chain + ratios | §3 |
| Food boosting (×2/×3 multiplier) | §4 |
| Storehouse relay logistics | §5 |
| Population, settlers, tools | §6 |
| Military (units, generals, combat) | §7-Military |
| Technology (clerics, monasteries, tech tree) | §7-Technology |
| Trade (traders, trade map, exchanges) | §7-Trade |
| Prestige system + unlock tree | §8 |
| Victory points (dynamic + permanent) | §9 |
| Tavern functions | §10 |
| Fortifications | §11 |
| Predefined economy buildings | §12 |
| Quests & event locations | §13 |

## The Core Rule

**The economy is the game.** Everything — military, technology, trade, victory — is powered by production chains flowing through storehouses. Food boosting is the dominant economic lever.

## Critical Constants (in GameConstants ScriptableObject)

```
prestigePointsPerLevel = 5
maxWorkYardsPerBuilding = 3
carrierMaxItems = 3
victoryCountdownSeconds = 180
maxSoldiersPerGeneral = 35
maxGenerals = 5
residenceBasePop = 4 (upgrade +4 each, max 12, +2 with Hygiene)
nobleResidenceBasePop = 5 (upgrade +5 each, max 15, +4 with Hygiene)
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

## Complete Building → Work Yard Map

**Lodge (3 Planks, 1 pop):** Forester, Woodcutter, Sawmill, Fisher, Hunter
**Farm (3 Planks, 1 pop):** Grain Barn, Windmill, Piggery, Shepherd, Stable
**Mountain Shelter (2P+1S, 1 pop):** Quarry, Coal Miner, Iron Miner, Gold Miner, Iron Smelter, Coking Plant
**Residence (2P+1S, 4 pop):** Bakery, Brewery, Paper Mill, Weaving Mill, Wheelwright, Toolmaker
**Noble Residence (3P+2S, 5 pop):** Butcher, Blacksmith, Mint, Goldsmith, Bookbinder, Tailor

## Complete Resource Flow

```
Raw:        Wood, Stone, Coal, Iron Ore, Gold Ore, Grain, Fish, Animal, Water, Wool
Processed:  Planks, Iron Bars, Flour, Bread, Sausages, Beer, Paper, Books,
            Cloth, Garments, Coins, Weapons, Tools, Wheels, Horses, Jewelry
```

## Food Boosting Quick Reference

| Building Type | No Food | Plain Food | Fancy Food |
|---------------|---------|------------|------------|
| Lodge/Farm/MtShelter/Residence | ×1 | ×2 | ×3 |
| Noble Residence | IDLE | ×1 | ×2 |

## Victory Points Quick Reference

**Dynamic (stealable):** Field Marshal (≥20 army), Metropolis (≥25 workers), Emperor (≥3 sectors), Banker (≥25 coins), Sun King (≥5 prestige), Trading Company (≥5 outposts), Fountain of Knowledge (≥3 techs), Pacifist (≥10min), Economist (≥75%), Generalissimo (≥20 kills)

**Permanent:** Abbey, Genius, Special Sector, Special Trade Outpost, Domination, Quest VPs

**Win condition:** Reach required VP count → 3-min countdown → win if still held
