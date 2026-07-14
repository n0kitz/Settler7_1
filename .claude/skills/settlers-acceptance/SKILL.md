---
name: settlers-acceptance
description: "Workflow for the acceptance phase (post-roadmap): triaging Normen's match findings into punch-list sprints, running balance soaks, and guarding against scope creep near the finish line. Read at the start of every post-roadmap session."
---

# Settlers Clone — Acceptance & Punch-List Skill

The roadmap is code-complete (2026-07-12, 517/517 green). Everything from here serves ONE
sentence: *Normen plays a full match down any of the three paths and forgets he built it.*
This skill governs how findings become work — and how work stays small.

## The Loop

```
Normen plays a match  →  findings (screenshots + notes)  →  triage  →  punch-list sprint
        ↑                                                                    │
        └────────────────────  play-verified fix batch  ◄───────────────────┘
```

Balance soaks (AI-vs-AI, recipe in `settlers-playmode-testing`) run as supporting evidence,
never as a substitute for Normen's matches.

## Finding Intake

For each finding, capture before touching code:
1. **Symptom** (Normen's words + screenshot if any).
2. **Path/context** (military / technology / trade match; map; minute).
3. **First suspicion check**: search `settlers-debugging` — many symptoms are known traps
   with known fixes.
4. **Severity**: A = breaks the north star (wrong rules, dead path, crash) ·
   B = breaks the feel (§14 deviation, visual glitch, balance) · C = cosmetic.

## Triage Rules

- **A-findings**: fix in the current sprint, always with a regression test if simulation-side.
- **B-findings**: batch by TYPE (all string findings together, all visual findings together —
  cost-saving Rule 7), one batch per session.
- **C-findings**: collect; do them only when a session has leftover room, never standalone.
- **Anything that is a new FEATURE is not a finding.** VISION.md non-goals apply doubly now —
  the answer to "wouldn't it be nice if…" this close to done is: note it under
  "Nach der Abnahme" in project_status.md and move on.

## Punch-List Sprint Structure (same discipline as Phases 6–8)

1. Session start protocol (`/session-start`), read this skill + the skill matching the
   findings' layer (`settlers-7-fidelity` for visual findings, `settlers-game-design` for
   rule findings, `settlers-localization` for strings, `settlers-performance` for fps).
2. One batch per session. Fix → `read_console` → tests ≥ baseline → play-mode verification
   of EVERY finding in the batch (screenshot per finding, same camera angle as Normen's
   report where possible).
3. Diff review before commit-ask: `csharp-reviewer` agent over the changed files, then
   `/code-review` on low/medium.
4. `/session-end`; the session log lists each finding as fixed/deferred with its screenshot.

## Balance Soak Sessions

Goal: a table `map × victory path × winner × duration` across all skirmish maps, so balance
claims rest on data. Run via the soak recipe in `settlers-playmode-testing`; a `/loop` may
pace multi-match runs. Red flags to report (not silently fix): a path that never wins on some
map, matches ending < 8 or > 40 sim-minutes, the tech AI stalling on iron-poor maps (known
deferred issue — confirm, don't rediscover).

## Standing Deferred List (do not re-litigate)

These are DECIDED deferrals; only Normen reopens them:
placeholder audio (Coplay sign-in → regenerate), DE prose review (CSV-marked, his task),
military goods split (Kanonen/Kanonenkugeln/Schwerter), meta screens EN-first,
tech-AI tool sourcing on iron-poor maps, `Settlers > Generate All` for the 3 new recipe SOs.

## Done Means Done

When Normen's three matches pass and the punch list is empty: update VISION.md's final row,
write the closing session log — and resist inventing Phase 9. The project has an ending;
reaching it is the achievement.
