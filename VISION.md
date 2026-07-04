# VISION — Die Siedler 7, Faithfully Reborn

> The north star for this project. **CLAUDE.md** says *how* we build; this file says *what*
> we are building toward and *how we will know it is done*. Read it when a decision needs a
> tie-breaker: the option that serves this vision wins.

---

## The North Star

One evening, Normen sits down, starts a new game, and forgets he built it.

The map opens as a warm, storybook diorama — green fertile valleys against red infertile
soil, stone walls ringing his land, his home castle rising over it all with red flags and
gold trim. He lays down a farm, a lodge, a mill. Carriers trundle between storehouses.
Bread starts flowing; he boosts a work yard and watches it speed up. An enemy general is
massing to the east, a neutral sector to the north could be his by faith or by coin, and a
trade outpost across the map would lock in a victory point nobody could steal.

It plays like *Die Siedler 7*. Not "like a clone of" — **like the game**. That is done.

---

## Why This Project Exists

This is a personal recreation of a game Normen loves — built for the joy of building it and
the joy of playing the result. It is **local, single-player, offline, free, and forever
non-commercial**. Success is not downloads or revenue. Success is a game that faithfully
captures the feel of *Die Siedler 7* and that its maker is proud to play.

Every scope decision flows from that: we go deep on faithfulness and polish, and we refuse
anything that trades the game's soul for reach.

---

## The Six Pillars

These are what make it *feel* like Die Siedler 7. Each pillar is "done" when a player can
honestly say the sentence in **bold**.

1. **The Economy Is the Game.**
   Deep production chains — wood → planks, grain → flour → bread, ore → weapons — all
   flowing through storehouses, never building-to-building. Food-boosting speeds work yards
   and *halts* them when food runs dry. *"Watching my settlement run is satisfying in
   itself, and every good I make matters."*

2. **Three Genuine Paths to Victory.**
   Military (Stronghold → armies → generals → auto-combat), Technology (Church → clerics →
   monastery research), and Trade (Export Office → traders → the trade map). Each is fully
   viable, each has its own buildings, units, and UI, and the AI plays all three.
   *"I can win three completely different ways, and so can my opponents."*

3. **A Living Map of Sectors.**
   The world is a graph of sectors, not a hex grid. You expand by military conquest,
   proselytism, or bribery; walls physically mark what you own; each sector's terrain and
   deposits matter. *"The map is a place I fight over, not a spreadsheet."*

4. **Prestige & Meaningful Progression.**
   The prestige tree gates buildings and upgrades; locked buildings show as gray
   silhouettes so you always see what's ahead. *"I'm always climbing toward the next
   unlock, and the choices are real."*

5. **The Dreamy Fairy-Tale Look.**
   The original art director's mandate: warm light, cool saturated shadows, a storybook
   diorama. Achieved with original **procedural** art (no ripped assets) — Unity 6 URP
   lighting, fog, and color grading. *"It's beautiful, and it looks unmistakably like
   Siedler 7."*

6. **The Race for Victory Points.**
   The VP ring with its dynamic (stealable) and permanent points, the tension of the
   3-minute countdown when someone reaches the threshold. *"The endgame is a nail-biter."*

---

## Definition of Done

Four tiers, each a checklist of concrete, verifiable criteria. We advance a tier only when
every box in it is true. Current position: **Tier 1 in progress.**

### Tier 0 — Foundation ✅ COMPLETE
- [x] All simulation systems implemented as pure C#, no UnityEngine dependency
- [x] Full NUnit coverage, **all tests green** (487/487)
- [x] Playable end-to-end: menu → map select → setup → play → victory/defeat → restart
- [x] Save/load, pause, settings, bilingual EN/DE with test-enforced key parity
- [x] AI opponents that build economy, fight, research, trade, and race for victory

### Tier 1 — A Game, Not a Prototype  (in progress)
- [x] Terrain reads as *land* — fertile grass vs. sandy soil, trees, rocks (Sprint 1)
- [x] Warm fairy-tale lighting: soft shadows, ambient, fog, URP color grading (Sprint 1)
- [x] "Play Again" / clean match restart with no state leaks (Sprint 2)
- [x] AI actively contests the leader when someone nears victory (Sprint 2)
- [x] Audio framework wired to events; drops in CC0 clips with zero code (Sprint 2)
- [ ] Music + a full set of SFX actually present and audible *(needs CC0 files)*
- [ ] Buildings read as *buildings* — roofs, doors, windows; not toy blocks (Sprint 3)
- [ ] Units read as settlers / carriers / soldiers; not colored capsules (Sprint 4)
- [ ] Every player action has clear, immediate visual **and** audio feedback

### Tier 2 — Unmistakably Die Siedler 7
- [ ] The fairy-tale art direction is obvious at a glance, no explanation needed
- [ ] Home castle dominates its sector; enemy strongholds are imposing; walls ring owned land
- [ ] Every UI string verified 1:1 against the original (BAUEN, PRESTIGE-OPTIONEN, BELOHNUNGEN, VP names) — see CLAUDE.md §14
- [ ] Tech tree as stone-and-candlelight cards with Geistliche/Mönche/Prälaten costs (§14.6)
- [ ] Trade map as a parchment world map with compass and dashed routes (§14.7)
- [ ] Stats ÜBERSICHT with the four verified columns (§14.1)
- [ ] The economy delivers the original's depth and the satisfaction of a settlement humming along

### Tier 3 — Finished & Proud
- [ ] Content complete: a coherent campaign arc, enough skirmish maps, all three paths fully fleshed
- [ ] Balanced through real playtests: economy speeds, AI difficulty, VP thresholds
- [ ] No rough edges: no debug UI, no primitive placeholders, no stale or untranslated strings
- [ ] Holds 60 fps with 40+ sectors, 200+ buildings, 100+ moving units
- [ ] **The acceptance test:** Normen plays a full match on any path and it feels like the real game

---

## Quality Bars (always true, every session)

- **Architecture is sacred.** Simulation stays pure C# — a single `using UnityEngine` there
  is a failure. Presentation reads state, never mutates it directly.
- **Tests stay green.** No change ships with a red suite. Simulation changes come with tests.
- **German is verified, never invented.** Every UI string is 1:1 from the original (CLAUDE.md
  §14). When in doubt, ask — do not paraphrase or translate.
- **No magic numbers, no giant files.** Values live in ScriptableObjects / GameConstants; no
  file exceeds 300 lines; one class per file.
- **Procedural art is the art direction**, not an apology. "Done" never requires a 3D artist
  or paid assets — richness comes from procedural composition and URP.
- **Every change is verified in the engine:** compile clean → tests green → play-mode
  screenshot. We trust what we've seen run, not what should work.

---

## Explicitly *Not* This Project

Naming the non-goals protects the vision from drift:

- **No multiplayer, networking, or hotseat.** Single-player vs. AI, full stop.
- **No monetization, server, telemetry, or always-online** — the sin the original was rightly
  criticized for. This game respects its player completely.
- **No DOTS/ECS.** 200–500 entities don't benefit; MonoBehaviour + SO + pure C# is correct here.
- **No ripped or imported original assets.** Original procedural art *inspired by* Siedler 7,
  never copied from it.
- **No feature sprawl beyond the original's scope.** We finish *this* game faithfully before
  dreaming up anything the original never had.

---

## The Path There

Six phases carry us from foundation to done. Each ends with tests green and a play-mode
screenshot. (Detailed status lives in `project_status.md`; the active plan in `.claude/plans/`.)

| # | Phase | Delivers | Status |
|---|-------|----------|--------|
| 1 | Terrain & Lighting | Land that reads as land; the fairy-tale look | ✅ done |
| 2 | Playability Quick Wins | Clean restart, AI victory-racing, audio wiring | ✅ done |
| 3 | Building Overhaul | Procedural multi-part buildings; a dominant home castle | ▶ next |
| 4 | Unit Overhaul | Recognizable settlers, carriers with goods, generals & armies | ○ |
| 5 | UI Fidelity | Parchment trade map, stone tech tree, ÜBERSICHT columns | ○ |
| 6 | Polish & Balance | Content, tuning, edge-cases, the 60 fps bar | ○ |

---

## How We Know We're Done

Not by a checklist alone. By the north star at the top of this file coming true:

**Normen starts a game, plays a full match down any of the three paths, and forgets he built
it — because it plays like Die Siedler 7.**

Everything in this document exists to make that one sentence true.
