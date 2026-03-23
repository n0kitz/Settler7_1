---
name: settlers-map-content
description: "Map design and SO data creation: sector layouts, resource placement, trade routes, batch asset generation."
---

# Settlers Clone — Map & Content Creation (Unity)

## ScriptableObject Data Creation

Claude Code creates SO class definitions. Assets are created either:
1. **In Unity Editor** via Create menu (right-click → Create → Settlers → ...)
2. **Via Editor script** that batch-creates assets programmatically

### Batch Asset Creation Pattern
```csharp
#if UNITY_EDITOR
using UnityEditor;

public static class RecipeGenerator
{
    [MenuItem("Settlers/Generate All Recipes")]
    public static void GenerateAll()
    {
        CreateRecipe("wood_to_planks", "Saw Planks",
            new[] { (ResourceType.Wood, 1) },
            new[] { (ResourceType.Planks, 1) },
            3.0f);
        
        CreateRecipe("grain_to_flour", "Mill Flour",
            new[] { (ResourceType.Grain, 1) },
            new[] { (ResourceType.Flour, 1) },
            2.5f);
        
        // ... all recipes from CLAUDE.md §3
        
        AssetDatabase.SaveAssets();
    }
    
    static void CreateRecipe(string id, string name, 
        (ResourceType, int)[] inputs, (ResourceType, int)[] outputs, float duration)
    {
        var recipe = ScriptableObject.CreateInstance<ProductionRecipe>();
        recipe.recipeId = id;
        recipe.displayName = name;
        recipe.inputs = inputs.Select(i => new ResourceAmount { type = i.Item1, amount = i.Item2 }).ToArray();
        recipe.outputs = outputs.Select(o => new ResourceAmount { type = o.Item1, amount = o.Item2 }).ToArray();
        recipe.cycleDuration = duration;
        AssetDatabase.CreateAsset(recipe, $"Assets/Data/Recipes/{id}.asset");
    }
}
#endif
```

**This pattern should be used for:** Recipes, Building Definitions, Work Yard Definitions, Tech Definitions, Unit Definitions, General Definitions, Trade Outpost Definitions, VP Definitions, Prestige Unlocks.

## MapDefinition ScriptableObject

```csharp
[CreateAssetMenu(fileName = "NewMap", menuName = "Settlers/Map")]
public class MapDefinition : ScriptableObject
{
    public string mapId;
    public string displayName;
    public int playerCount;
    public int victoryPointsRequired;
    
    public SectorData[] sectors;
    public SectorConnection[] connections;
    public StartingPosition[] startingPositions;
    public MonasteryData[] monasteries;
    public TradeMapData tradeMap;
    public string[] activeVictoryPoints; // VP IDs active on this map
}

[System.Serializable]
public class SectorData
{
    public string sectorId;
    public string displayName;
    public Vector3 worldPosition;       // center of sector in world space
    public Vector3[] boundaryPoints;    // polygon outline
    public SectorOwner startingOwner;
    public GarrisonData[] garrison;
    public ResourceDepositData[] resources;
    public string specialBuilding;      // null if none
    public RewardPackage[] conquestRewards;
}
```

## Map Design Guidelines

### Sector Count
| Size | Sectors | Players | VPs Required |
|------|---------|---------|-------------|
| Small | 18-22 | 2 | 4-5 |
| Medium | 25-35 | 2-3 | 5-6 |
| Large | 35-45 | 3-4 | 6-7 |

### Resource Rules
- Starting sector: forest + stone quarry + fertile land (ALWAYS)
- Starting sector: NO gold, NO iron (force expansion)
- Coal: within 1-2 sectors of start
- Iron: within 2-3 sectors
- Gold: within 3-4 sectors
- Every map quadrant: at least 1 sector with all mine types
- Fishing: near water/coast sectors

### Connectivity
- Starting sectors: 2-3 adjacent neutral sectors
- Create chokepoint sectors controlling resource access
- Contested sectors (VPs/special buildings) between players
- No player completely boxed in — always 2+ expansion paths

### Victory Point Distribution
- Mix of dynamic (6-8) and permanent (3-5) VPs
- At least 2 VPs per victory path
- 1-2 map-specific VPs
- No single strategy can claim all VPs — force diversification

### Trade Map Layout
- Tier 1 outposts: 3-4 steps from start
- Tier 2: 5-7 steps
- Tier 3: 8-10 steps
- VP outposts at Tier 2-3
- Branching paths force route choices

### Balancing Checklist
- [ ] Equal basic resource access for all players
- [ ] Symmetrical expansion paths
- [ ] No VP rush without opposition
- [ ] Trade map reachable from all starts
- [ ] 2+ contestable monasteries
- [ ] Special sectors reward different strategies
