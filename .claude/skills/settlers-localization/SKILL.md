---
name: settlers-localization
description: "Localization architecture: StringTable CSV rules, LocalizedNames resolver pattern, the Show()-refresh pattern for factory-built UI, §14 verified-string discipline. Read before adding/changing ANY UI string or CSV key."
---

# Settlers Clone — Localization Skill

Every rule here comes from a bug that shipped or a sed-cleanup that was needed. The
localization layer is test-enforced — breaking these rules turns the suite red.

## Architecture (never violate)

- **Simulation stays English.** `DisplayName` fields on recipes/outposts/techs/missions are
  stable EN identifiers, never localized. Localization happens at DISPLAY time only.
- Resolution path: `L.Get(key)` for plain keys; `LocalizedNames.X(id)` for data-driven names
  (Resource, Recipe, Outpost, Tech(+Description), Prestige(+Description),
  MissionTitle/Briefing/Objective). Every resolver falls back to the EN DisplayName /
  enum name when the key is missing — a missing key degrades gracefully, never crashes.
- Adding a new data-driven name category = add a resolver to
  `Simulation/Localization/LocalizedNames.cs` following the existing prefix pattern
  (`ui.recipe.` / `ui.outpost.` / `ui.techname.` / `ui.prestige.name.` / `ui.mission.<id>.`).

## StringTable CSV Rules (Assets/Resources/Localization/StringTable.{en,de}.csv)

1. **The parser splits each line on the FIRST comma only.** Commas inside values are fine.
   **NEVER escape commas** — `\,` stays literally in the displayed text (cost a sed-cleanup once).
2. **Key parity is test-enforced**: every EN key must exist in DE and vice versa. Always add
   to BOTH files in the same edit.
3. Multi-line display blocks (e.g. end-screen stats) = ONE key with `\n` in the value and
   `string.Format` placeholders — not N separate keys.
4. `L.Get` on a missing key returns the key itself — visible `ui.foo.bar` on screen means a
   missing/typo'd key, not a code bug.

## German String Discipline (§14)

- German UI strings come ONLY from the verified CLAUDE.md §14 tables (titles, columns, VP
  names, carrier lines, goods). `LocalizedNamesTests.VerifiedGermanStrings_MatchOriginal`
  asserts them 1:1 — paraphrasing turns tests red by design.
- Where new DE prose is unavoidable (mission briefings, recipe names not in §14):
  write it minimal, mark the CSV section with a comment "zur Prüfung durch Normen",
  and list it in the session log. Never silently invent "verified-sounding" German.

## The Show()-Refresh Pattern (factory-built panels)

Factory-built UI resolves strings at CREATION time → a later locale switch leaves stale text.
The proven fix (used by ÜBERSICHT, TechTree, PrestigeChart, TradeMap, BuildMenu, Mission UIs):

1. The factory stores label references ON the component
   (`menu._titleLabel = title;` — internal field, or a `RegisterNodeLabels(...)` call).
2. The component's `Show()` calls `RefreshLocaleTexts()` / `RefreshChrome()` which re-resolves
   every baked string via `L.Get` / `LocalizedNames`.
3. **Click/feedback handlers receive KEYS, not resolved strings** — resolve at click time.

Symptom that this pattern is missing somewhere: switch language in settings → open the panel →
one string is still in the old language. Fix by extending the panel's refresh method, never by
re-creating the panel.

Live-polling UIs (HUD, VPRing, ActionBar) refresh on their own timer — they need no pattern.

## New-Key Checklist

1. Key in EN CSV + DE CSV (same edit, parity test).
2. Resolved via `L.Get`/`LocalizedNames` at display time (never cached in a field without a
   refresh path).
3. If a factory bakes it → register label + refresh in `Show()`.
4. New DE prose → mark for review.
5. Verify: play mode → settings → switch locale → open the changed panel → screenshot EN + DE,
   zero leftovers.
