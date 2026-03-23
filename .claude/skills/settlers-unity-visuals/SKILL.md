---
name: settlers-unity-visuals
description: "URP presentation layer: camera, terrain, building visuals, workers, roads, lighting, UI layout, performance."
---

# Settlers Clone — Unity Visuals & Presentation Skill

## Architecture Rule

All visual code lives in `Assets/Scripts/Presentation/`. It reads `GameState` from `GameController.Instance.State`. It NEVER modifies simulation state directly — it sends commands through `GameController`.

## Camera (Settlers 7 Style)

`SettlerCamera.cs` — MonoBehaviour on the main camera:
- **Pan:** WASD keys or mouse drag (middle button or edge scrolling)
- **Zoom:** Scroll wheel (min distance ~15, max ~200)
- **Rotate:** Q/E keys or middle-mouse drag
- **Tilt:** Tied to zoom level (close = more angled ~30°, far = more top-down ~70°)
- **Sector Overview:** When distance > threshold, switch to simplified view showing sector polygons with ownership colors and army/resource icons

```csharp
public class SettlerCamera : MonoBehaviour
{
    [SerializeField] private float _panSpeed = 20f;
    [SerializeField] private float _zoomSpeed = 10f;
    [SerializeField] private float _rotateSpeed = 90f;
    [SerializeField] private float _minDistance = 15f;
    [SerializeField] private float _maxDistance = 200f;
    [SerializeField] private float _minElevation = 0.5f; // ~30 degrees
    [SerializeField] private float _maxElevation = 1.3f;  // ~75 degrees
    
    private Vector3 _target;
    private float _distance = 50f;
    private float _azimuth = 0f;
    private float _elevation = 0.8f;
}
```

## Terrain per Sector

Each sector uses either:
- **Unity Terrain** component (good for editor painting, but heavy for many sectors)
- **Custom mesh** from heightmap data (lighter, better for 20+ sectors)

Recommendation: **Custom mesh** for sectors, with terrain splatting material:
```
Sector mesh = PlaneGeometry with heightmap vertex displacement
Material = URP Lit with splat map (grass/dirt/rock/fertile blending)
Sector border = LineRenderer or flat colored mesh outline
```

## Building Visuals — Progressive Quality

**Phase 1 (prototype):** Unity primitives
- Lodge = brown Cube + cone roof
- Farm = flat green Cube (wide) + small barn
- Mountain Shelter = grey Cube against a rock
- Residence = beige Cube + peaked roof
- Noble Residence = larger beige Cube + ornate roof
- Each type: unique color + shape for instant recognition

**Phase 2:** Low-poly models (Blender → FBX/glTF import)
**Phase 3:** Textured models with baked AO

Work yard attachment: child GameObjects positioned at the 3 green-triangle slots around the base building.

## Workers & Carriers

- Simple capsule + sphere meshes (or low-poly character model later)
- Color-coded by type: worker (brown), carrier (blue), soldier (red), cleric (white), trader (gold)
- Walk animation: sinusoidal Y bobbing + face movement direction
- Carrying items: small mesh attached to hand/back position
- Use object pooling (create pool at start, activate/deactivate)

## Roads

- Flat Quad meshes following terrain contour, slightly raised (+0.05)
- Basic road: warm brown texture/color (`#C4A882`)
- Paved road: grey texture/color (`#808080`)
- Place as chain of segments between connected points

## Lighting (Settlers 7 Warm Diorama)

```
URP Volume Profile:
  - Bloom: low intensity for warm glow
  - Ambient Occlusion: subtle
  - Color Grading: warm temperature shift

Directional Light (Sun):
  - Color: warm white (#FFF5E1)
  - Intensity: 1.2
  - Shadows: Soft, medium resolution
  - Rotation: ~30° from horizon (morning/evening feel)

Environment:
  - Skybox: gradient (light blue → warm horizon)
  - Ambient: sky-based, warm tint
```

## UI (Canvas-based, NOT 3D)

All UI panels are Unity Canvas → Screen Space Overlay. Do not render UI in world space unless it's a building health bar or selection ring.

Key panels:
- **HUD** (top bar): resources, population count, prestige level, minimap
- **Build Menu** (bottom/side): building categories, shows costs
- **Sector Panel** (side): selected sector info, garrison, resources
- **Trade Map** (overlay): 2D network graph rendered on a Canvas panel
- **Tech Tree** (overlay): visual tech tree on Canvas
- **Prestige Chart** (overlay): unlock tree on Canvas
- **VP Tracker** (corner): current VP count + countdown timer

## Color Palette

| Element | Hex |
|---------|-----|
| Player 1 | #2E75B6 |
| Player 2 | #C0392B |
| Player 3 | #27AE60 |
| Player 4 | #F39C12 |
| Neutral | #95A5A6 |
| Grass | #4A7C3F |
| Fertile | #6B8E23 |
| Mountain | #8B7D6B |
| Water | #4A90D9 |
| Road basic | #C4A882 |
| Road paved | #808080 |

## Performance

- **Object pooling** for workers and carriers
- **LOD** for buildings (simplified mesh at distance)
- **Occlusion culling** enabled in project settings
- **Static batching** for terrain and road meshes
- **GPU instancing** enabled on all shared materials
- Target: 60fps with 40+ sectors, 200+ buildings, 100+ moving units
