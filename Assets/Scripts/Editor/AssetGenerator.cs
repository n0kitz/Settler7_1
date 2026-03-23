using UnityEditor;
using UnityEngine;
using Settlers.Data;
using Settlers.Simulation;

namespace Settlers.Editor
{
    /// <summary>
    /// Editor menu items that generate ScriptableObject assets.
    /// Map-related generators are in AssetGeneratorMaps.cs.
    /// </summary>
    public static class AssetGenerator
    {
        [MenuItem("Settlers/Generate All Assets")]
        public static void GenerateAll()
        {
            GenerateGameConstants();
            GenerateBuildingDefinitions();
            GenerateWorkYardAndRecipeAssets();
            AssetGeneratorMaps.GenerateMapDefinitions();
            GeneratePrestigeUnlocks();
            GenerateTechDefinitions();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("=== All Settlers assets generated ===");
        }

        [MenuItem("Settlers/Generate GameConstants")]
        public static void GenerateGameConstants()
        {
            const string path = "Assets/Data/GameConstants.asset";
            AssetGeneratorUtil.EnsureDirectory("Assets/Data");

            var existing = AssetDatabase.LoadAssetAtPath<GameConstants>(path);
            if (existing != null)
            {
                Debug.Log("GameConstants already exists at " + path);
                Selection.activeObject = existing;
                return;
            }

            var asset = ScriptableObject.CreateInstance<GameConstants>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            Debug.Log("Created GameConstants at " + path);
        }

        [MenuItem("Settlers/Generate Building Definitions")]
        public static void GenerateBuildingDefinitions()
        {
            AssetGeneratorUtil.EnsureDirectory("Assets/Data/Buildings");

            CreateBuildingDef("Lodge", "Lodge", BaseBuildingType.Lodge,
                3, 0, 1, 0, 0, false, new Color(0.55f, 0.35f, 0.15f));
            CreateBuildingDef("Farm", "Farm", BaseBuildingType.Farm,
                3, 0, 1, 0, 0, false, new Color(0.45f, 0.65f, 0.25f));
            CreateBuildingDef("MountainShelter", "Mountain Shelter", BaseBuildingType.MountainShelter,
                2, 1, 1, 0, 0, false, new Color(0.5f, 0.5f, 0.55f));
            CreateBuildingDef("Residence", "Residence", BaseBuildingType.Residence,
                2, 1, 4, 4, 2, false, new Color(0.75f, 0.6f, 0.4f));
            CreateBuildingDef("NobleResidence", "Noble Residence", BaseBuildingType.NobleResidence,
                3, 2, 5, 5, 2, true, new Color(0.85f, 0.75f, 0.5f));

            AssetDatabase.SaveAssets();
            Debug.Log("Generated all 5 building definitions in Assets/Data/Buildings/");
        }

        private static void CreateBuildingDef(string fileName, string displayName,
            BaseBuildingType type, int plankCost, int stoneCost, int basePop,
            int upgradePop, int maxUpgrade, bool requiresFood, Color color)
        {
            string path = $"Assets/Data/Buildings/{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path) != null) return;

            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.buildingId = fileName.ToLowerInvariant();
            def.displayName = displayName;
            def.type = type;
            def.plankCost = plankCost;
            def.stoneCost = stoneCost;
            def.basePopulation = basePop;
            def.upgradePopulationBonus = upgradePop;
            def.maxUpgradeLevel = maxUpgrade;
            def.requiresFoodToFunction = requiresFood;
            def.maxWorkYards = 3;
            def.buildingColor = color;

            AssetDatabase.CreateAsset(def, path);
        }

        [MenuItem("Settlers/Generate Work Yard + Recipe Assets")]
        public static void GenerateWorkYardAndRecipeAssets()
        {
            AssetGeneratorUtil.EnsureDirectory("Assets/Data/WorkYards");
            AssetGeneratorUtil.EnsureDirectory("Assets/Data/Recipes");

            foreach (var recipe in RecipeDatabase.All)
            {
                string recipePath = $"Assets/Data/Recipes/{recipe.WorkYardId}.asset";
                ProductionRecipe recipeAsset;
                var existing = AssetDatabase.LoadAssetAtPath<ProductionRecipe>(recipePath);
                if (existing != null)
                {
                    recipeAsset = existing;
                }
                else
                {
                    recipeAsset = ScriptableObject.CreateInstance<ProductionRecipe>();
                    recipeAsset.recipeId = recipe.WorkYardId;
                    recipeAsset.displayName = recipe.DisplayName;
                    recipeAsset.cycleDuration = recipe.CycleDuration;
                    recipeAsset.requiredResourceNode = recipe.RequiredNode;

                    recipeAsset.inputs = new ResourceAmount[recipe.Inputs.Length];
                    for (int i = 0; i < recipe.Inputs.Length; i++)
                        recipeAsset.inputs[i] = new ResourceAmount(recipe.Inputs[i].type, recipe.Inputs[i].amount);

                    recipeAsset.outputs = new ResourceAmount[recipe.Outputs.Length];
                    for (int i = 0; i < recipe.Outputs.Length; i++)
                        recipeAsset.outputs[i] = new ResourceAmount(recipe.Outputs[i].type, recipe.Outputs[i].amount);

                    AssetDatabase.CreateAsset(recipeAsset, recipePath);
                }

                string wyPath = $"Assets/Data/WorkYards/{recipe.WorkYardId}.asset";
                if (AssetDatabase.LoadAssetAtPath<WorkYardDefinition>(wyPath) != null) continue;

                var wyDef = ScriptableObject.CreateInstance<WorkYardDefinition>();
                wyDef.workYardId = recipe.WorkYardId;
                wyDef.displayName = recipe.DisplayName;
                wyDef.parentBuildingType = recipe.ParentBuilding;
                wyDef.requiredResourceNode = recipe.RequiredNode;
                wyDef.recipe = recipeAsset;

                AssetDatabase.CreateAsset(wyDef, wyPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Generated {RecipeDatabase.All.Count} recipe + work yard assets");
        }

        [MenuItem("Settlers/Generate Prestige Unlocks")]
        public static void GeneratePrestigeUnlocks()
        {
            AssetGeneratorUtil.EnsureDirectory("Assets/Data/Prestige");

            foreach (var def in PrestigeDatabase.All)
            {
                string path = $"Assets/Data/Prestige/{def.Id}.asset";
                if (AssetDatabase.LoadAssetAtPath<PrestigeUnlockDefinition>(path) != null) continue;

                var asset = ScriptableObject.CreateInstance<PrestigeUnlockDefinition>();
                asset.unlockId = def.Id;
                asset.displayName = def.DisplayName;
                asset.description = def.Description;
                asset.branch = def.Branch;
                asset.minLevel = def.MinLevel;
                AssetDatabase.CreateAsset(asset, path);
            }

            // Link prerequisites
            foreach (var def in PrestigeDatabase.All)
            {
                if (def.PrerequisiteId == null) continue;
                var asset = AssetDatabase.LoadAssetAtPath<PrestigeUnlockDefinition>(
                    $"Assets/Data/Prestige/{def.Id}.asset");
                var prereq = AssetDatabase.LoadAssetAtPath<PrestigeUnlockDefinition>(
                    $"Assets/Data/Prestige/{def.PrerequisiteId}.asset");
                if (asset != null && prereq != null)
                {
                    asset.prerequisite = prereq;
                    EditorUtility.SetDirty(asset);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Generated {PrestigeDatabase.All.Count} prestige unlock assets");
        }

        [MenuItem("Settlers/Generate Tech Definitions")]
        public static void GenerateTechDefinitions()
        {
            AssetGeneratorUtil.EnsureDirectory("Assets/Data/Technologies");

            foreach (var def in TechTree.All)
            {
                string path = $"Assets/Data/Technologies/{def.Id}.asset";
                if (AssetDatabase.LoadAssetAtPath<TechDefinition>(path) != null) continue;

                var asset = ScriptableObject.CreateInstance<TechDefinition>();
                asset.techId = def.Id;
                asset.displayName = def.DisplayName;
                asset.description = def.Description;
                asset.tier = def.Tier;
                asset.researchTime = def.ResearchTime;
                AssetDatabase.CreateAsset(asset, path);
            }

            // Link prerequisites
            foreach (var def in TechTree.All)
            {
                if (def.PrerequisiteId == null) continue;
                var asset = AssetDatabase.LoadAssetAtPath<TechDefinition>(
                    $"Assets/Data/Technologies/{def.Id}.asset");
                var prereq = AssetDatabase.LoadAssetAtPath<TechDefinition>(
                    $"Assets/Data/Technologies/{def.PrerequisiteId}.asset");
                if (asset != null && prereq != null)
                {
                    asset.prerequisite = prereq;
                    EditorUtility.SetDirty(asset);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Generated {TechTree.All.Count} tech definition assets");
        }
    }
}
