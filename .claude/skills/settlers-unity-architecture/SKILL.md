---
name: settlers-unity-architecture
description: "Unity C# architecture: three-layer separation, SO patterns, namespaces, testing, system update order. Read when creating systems or classes."
---

# Settlers Clone — Unity Architecture Skill

## Three-Layer Rule

```
Layer 1: DATA          → ScriptableObjects in Assets/Data/
Layer 2: SIMULATION    → Pure C# in Assets/Scripts/Simulation/ (NO UnityEngine*)
Layer 3: PRESENTATION  → MonoBehaviour in Assets/Scripts/Presentation/
```

**Layer 2 (Simulation) must NEVER reference UnityEngine** except Vector3/Mathf. This enables NUnit testing without Play Mode and keeps Claude Code productive (pure C# is fast to write and debug).

## Namespaces

```csharp
namespace Settlers.Data { }         // ScriptableObject definitions
namespace Settlers.Simulation { }   // Pure C# game logic
namespace Settlers.Presentation { } // MonoBehaviour visuals
namespace Settlers.UI { }           // Canvas UI panels
```

## ScriptableObject Pattern

Every game definition is a ScriptableObject. This means:
- `[CreateAssetMenu]` attribute on every definition class
- All costs, stats, durations → fields in the SO, not hardcoded
- Reference other SOs via serialized fields (drag in Inspector)
- Use `GameConstants` SO for all magic numbers

```csharp
[CreateAssetMenu(fileName = "NewWorkYard", menuName = "Settlers/WorkYard")]
public class WorkYardDefinition : ScriptableObject
{
    public string workYardId;
    public string displayName;
    public BaseBuildingType parentBuildingType; // which base building this attaches to
    public ProductionRecipe recipe;
    public ResourceNodeType requiredResourceNode;
    public Sprite icon;
    public GameObject prefab;
}
```

## Simulation ↔ Presentation Bridge

`GameController.cs` (MonoBehaviour) owns the `GameState` (simulation) and calls `SimulationRunner.Tick(dt)`. Presentation scripts read `GameState` via `GameController.Instance`.

```csharp
// GameController.cs — the bridge
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    
    [SerializeField] private GameConstants _constants;
    [SerializeField] private MapDefinition _currentMap;
    
    public GameState State { get; private set; }
    private SimulationRunner _runner;
    
    void Awake()
    {
        Instance = this;
        State = new GameState(_constants, _currentMap);
        _runner = new SimulationRunner(State);
    }
    
    void Update()
    {
        _runner.Tick(Time.deltaTime);
    }
}
```

Presentation scripts read state:
```csharp
// BuildingView.cs — reads simulation, never writes
var building = GameController.Instance.State.GetBuilding(buildingId);
transform.position = building.Position;
```

Player commands go through `GameController`:
```csharp
GameController.Instance.PlaceBuilding(type, sectorId, position);
```

## Event Bus (Simulation Internal)

```csharp
public class EventBus
{
    private Dictionary<Type, List<Delegate>> _handlers = new();
    
    public void Subscribe<T>(Action<T> handler) { ... }
    public void Publish<T>(T evt) { ... }
}

// Events
public record BuildingPlaced(int BuildingId, int SectorId);
public record ProductionComplete(int WorkYardId, ResourceType Output, int Amount);
public record ResourceChanged(int PlayerId, ResourceType Type, int NewAmount);
public record SectorConquered(int SectorId, int NewOwnerId);
public record VPChanged(int PlayerId, string VPId, bool Gained);
public record TechResearched(int PlayerId, string TechId);
```

## System Update Order (in SimulationRunner.Tick)

1. ConstructionSystem
2. PopulationSystem
3. ProductionSystem (includes food boosting)
4. LogisticsSystem (carrier dispatch)
5. ArmySystem (movement)
6. CombatResolver (if army at target)
7. ResearchSystem (monastery progress)
8. TradeSystem (trader movement + exchanges)
9. PrestigeSystem (level recalculation)
10. VictorySystem (VP evaluation + countdown)
11. AIController (AI decisions)

## Testing Strategy

Write NUnit tests for ALL simulation logic. These run in Edit Mode (no Play Mode needed).

```csharp
[TestFixture]
public class FoodBoostTests
{
    [Test]
    public void PlainFood_DoublesOutput_OnLodge()
    {
        var multiplier = FoodBoostCalculator.GetMultiplier(
            BaseBuildingType.Lodge, FoodSetting.Plain);
        Assert.AreEqual(2, multiplier);
    }
    
    [Test]
    public void NoFood_HaltsNobleResidence()
    {
        var multiplier = FoodBoostCalculator.GetMultiplier(
            BaseBuildingType.NobleResidence, FoodSetting.None);
        Assert.AreEqual(0, multiplier); // 0 = halted
    }
}
```

## File Size Discipline
- No C# file over 300 lines — split into partial classes or extract helpers
- One class per file, filename matches class name
- Group related classes in folders, not in mega-files

## GameConstants Reference

```csharp
prestigePointsPerLevel = 5;    maxWorkYardsPerBuilding = 3;
carrierMaxItems = 3;           victoryCountdownSeconds = 180f;
maxSoldiersPerGeneral = 35;    maxGenerals = 5;
residenceBasePop = 4;          residenceUpgradePop = 4;
nobleResidenceBasePop = 5;     nobleResidenceUpgradePop = 5;
hygieneResidenceBonus = 2;     hygieneNobleBonus = 4;
plainFoodMultiplier = 2;       fancyFoodMultiplier = 3;
nobleResidencePlainMultiplier = 1; nobleResidenceFancyMultiplier = 2;
```

## Efficiency Rules

| Avoid | Instead |
|-------|---------|
| Writing stubs to fill in later | Implement fully or skip entirely |
| Extensive XML docs on private methods | Only document public APIs |
| Writing a MonoBehaviour for pure logic | Keep it in Simulation layer as pure C# |
| Refactoring working code for style | Only refactor if blocking new features |
| Separate test file per tiny class | Test complex systems only |
