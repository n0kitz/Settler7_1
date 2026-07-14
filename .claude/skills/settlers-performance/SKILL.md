---
name: settlers-performance
description: "Performance rules from the Sprint-8a 60-fps work: SRP-batching material rules (MPB ban), view culling layers, view-manager hygiene, and the bisection method for frame-rate mysteries. Read before touching renderers, materials, or view managers."
---

# Settlers Clone — Performance Skill

Sprint 8a took this project from 14.8 → 100+ fps. These are the rules that did it and the
method that found them. Target stays: **60 fps at 40+ sectors, 200+ buildings, 100+ units**
(reference scene: `the_frontier` 4p max-load — see `settlers-playmode-testing`).

## Rule 1 — NEVER tint per-part via MaterialPropertyBlock

**Per-part MPBs disable SRP batching.** One MPB per building/wall/tree/figure primitive =
thousands of unbatched draw calls; this alone was the 14.8-fps root cause. The palette is
small and fixed, so tint via cached material swap instead:

```csharp
// BuildingViewFactory.GetColorMaterial(baseMat, color) — one Material per palette color,
// cached in a static Dictionary<Color32, Material>; assign renderer.sharedMaterial.
renderer.sharedMaterial = BuildingViewFactory.GetColorMaterial(renderer.sharedMaterial, color);
```

The ONLY sanctioned MPB use left is per-sector `_BaseMap_ST` UV offsets (TerrainStyle).
If you need a new per-instance color anywhere: extend the material cache, do not reach for MPB.

## Rule 2 — Retint on state CHANGE only

Never re-apply a tint every frame/sync. Track the state (`_idleTint` bool pattern in
WorkerView) and swap the material only when it flips.

## Rule 3 — ViewLayers culling (Presentation/ViewLayers.cs)

- Units → layer 30, cull distance 70. Building detail parts → layer 29, cull distance 260.
- Applied via `Camera.layerCullDistances` + `layerCullSpherical = true` in
  `ViewLayers.ApplyCullDistances(cam)` (called from BootstrapScene.CreateCamera).
- New small/detail view objects MUST go on the right layer (`ViewLayers.SetLayerRecursive`)
  or they render at any distance.
- Unit figures: `ShadowCastingMode.Off` (`ViewLayers.DisableShadows`).

## Rule 4 — View managers must prune

Any manager syncing sim entities → views needs an **alive-set diff each Sync**: build the set
of currently valid sim ids, destroy views whose id is absent. WorkerManager leaked 340 zombie
figures through conquest churn before this. A "views only ever get added" sync loop is a bug.

## Rule 5 — Views spawn from EVENTS, not call sites

Building views spawn from `BuildingPlacedEvent` (fired by PlaceBuilding AND RestoreBuilding)
— never from the human placement path alone (AI/loaded buildings were invisible for months).
Coordinates on buildings are **sector-LOCAL** (`world − sectorPos`); world position =
`GetSectorPosition(sectorId) + new Vector3(LocalX, 0, LocalZ)`.

## The Bisection Method (when fps drops and the cause is unknown)

Work top-down with hard numbers, one variable per step — this found the MPB root cause:

1. **Stopwatch the simulation tick** (pure C#, measure ms/tick). 0.02 ms = innocent → it's rendering.
2. **Disable whole view roots** (Units, Buildings) one at a time; note fps deltas.
3. Inside the guilty root, **disable renderers only** (logic keeps running). If fps recovers →
   draw submission, not scripts.
4. Distinguish shadow cost (shadows off) from batching cost (Frame Debugger / SRP batcher
   stats). "Thousands of draw calls, few visible objects" = batching broken.
5. Fix, then re-measure the SAME closeup + overview camera positions before/after.

Record before/after fps numbers in project_status.md — "feels faster" is not a result.

## Cheap Habits That Keep It Fast

- Shared static materials on procedural factories (SectorDecorView/SectorLandmarkView pattern).
- Deterministic `System.Random(seed)` for decoration — no per-frame randomness.
- No colliders on decoration/landmarks (sector clicks must reach the hex).
- Deferred `Destroy()` counts double for one frame — settle a frame before asserting leaks.
