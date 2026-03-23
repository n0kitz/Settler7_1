using UnityEditor;
using UnityEngine;
using Settlers.Data;
using Settlers.Simulation;

namespace Settlers.Editor
{
    /// <summary>
    /// Map-related asset generation (extracted from AssetGenerator for file size discipline).
    /// </summary>
    public static class AssetGeneratorMaps
    {
        [MenuItem("Settlers/Generate Test Map Definition")]
        public static void GenerateTestMapDefinition()
        {
            const string path = "Assets/Data/Maps/TestMap6Sectors.asset";
            AssetGeneratorUtil.EnsureDirectory("Assets/Data/Maps");

            var existing = AssetDatabase.LoadAssetAtPath<MapDefinition>(path);
            if (existing != null)
            {
                Debug.Log("TestMap6Sectors already exists at " + path);
                Selection.activeObject = existing;
                return;
            }

            var map = ScriptableObject.CreateInstance<MapDefinition>();
            map.mapId = "test_6_sectors";
            map.displayName = "Test Valley (6 Sectors)";
            map.playerCount = 2;
            map.victoryPointsRequired = 4;

            map.sectors = new SectorDefinition[]
            {
                Sector("Greenwood Heights", new Vector3(-9f, 0f, 18f), 0, 0, false, 8,
                    new[] { ResourceNodeType.Forest, ResourceNodeType.WaterSource }),
                Sector("Redcliff Valley", new Vector3(9f, 0f, 18f), 1, 0, false, 8,
                    new[] { ResourceNodeType.Forest, ResourceNodeType.WaterSource }),
                Sector("Riverside Meadows", new Vector3(-18f, 0f, 0f), -2, 4, false, 6,
                    new[] { ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }),
                Sector("Ironpeak Pass", new Vector3(18f, 0f, 0f), -2, 8, true, 6,
                    new[] { ResourceNodeType.Iron, ResourceNodeType.Coal }),
                Sector("Stonefield Plains", new Vector3(-9f, 0f, -18f), -2, 4, false, 6,
                    new[] { ResourceNodeType.Stone, ResourceNodeType.FertileLand }),
                Sector("Goldcrest Summit", new Vector3(9f, 0f, -18f), -2, 8, true, 5,
                    new[] { ResourceNodeType.Gold, ResourceNodeType.Stone })
            };

            map.edges = new EdgeDefinition[]
            {
                new() { sectorA = 0, sectorB = 1 },
                new() { sectorA = 0, sectorB = 2 },
                new() { sectorA = 0, sectorB = 4 },
                new() { sectorA = 1, sectorB = 3 },
                new() { sectorA = 1, sectorB = 5 },
                new() { sectorA = 2, sectorB = 4 },
                new() { sectorA = 3, sectorB = 5 },
                new() { sectorA = 4, sectorB = 5 }
            };

            map.playerStarts = new PlayerStartDefinition[]
            {
                new() { sectorIndex = 0, playerId = 0 },
                new() { sectorIndex = 1, playerId = 1 }
            };

            AssetDatabase.CreateAsset(map, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = map;
            Debug.Log("Created TestMap6Sectors at " + path);
        }

        [MenuItem("Settlers/Generate Map Definitions")]
        public static void GenerateMapDefinitions()
        {
            AssetGeneratorUtil.EnsureDirectory("Assets/Data/Maps");

            foreach (var mapId in MapFactory.GetMapIds())
            {
                var info = MapFactory.CreateMap(mapId);
                string path = $"Assets/Data/Maps/{mapId}.asset";
                if (AssetDatabase.LoadAssetAtPath<MapDefinition>(path) != null)
                {
                    Debug.Log($"Map {mapId} already exists at {path}");
                    continue;
                }

                var map = ScriptableObject.CreateInstance<MapDefinition>();
                map.mapId = info.Id;
                map.displayName = info.DisplayName;
                map.playerCount = info.PlayerCount;
                map.victoryPointsRequired = info.VPRequired;

                int sectorCount = info.Graph.SectorCount;
                map.sectors = new SectorDefinition[sectorCount];
                for (int i = 0; i < sectorCount; i++)
                {
                    var sector = info.Graph.GetSector(i);
                    var nodes = new ResourceNodeType[sector.ResourceNodes.Count];
                    for (int j = 0; j < sector.ResourceNodes.Count; j++)
                        nodes[j] = sector.ResourceNodes[j];

                    map.sectors[i] = new SectorDefinition
                    {
                        sectorName = sector.Name,
                        position = Vector3.zero,
                        initialOwner = sector.OwnerId,
                        garrisonStrength = sector.GarrisonStrength,
                        isFortified = sector.IsFortified,
                        buildSlots = sector.BuildSlots,
                        resourceNodes = nodes
                    };
                }

                var edges = new System.Collections.Generic.List<EdgeDefinition>();
                var drawn = new System.Collections.Generic.HashSet<long>();
                for (int i = 0; i < sectorCount; i++)
                {
                    foreach (int n in info.Graph.GetNeighbors(i))
                    {
                        long key = System.Math.Min(i, n) * 1000L + System.Math.Max(i, n);
                        if (drawn.Add(key))
                            edges.Add(new EdgeDefinition { sectorA = i, sectorB = n });
                    }
                }
                map.edges = edges.ToArray();

                var starts = new System.Collections.Generic.List<PlayerStartDefinition>();
                for (int i = 0; i < sectorCount; i++)
                {
                    var sector = info.Graph.GetSector(i);
                    if (sector.IsPlayerOwned)
                        starts.Add(new PlayerStartDefinition
                            { sectorIndex = i, playerId = sector.OwnerId });
                }
                map.playerStarts = starts.ToArray();

                AssetDatabase.CreateAsset(map, path);
                Debug.Log($"Created map definition: {path}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Generated {MapFactory.GetMapIds().Length} map definitions.");
        }

        private static SectorDefinition Sector(string name, Vector3 pos, int owner,
            int garrison, bool fortified, int slots, ResourceNodeType[] nodes)
        {
            return new SectorDefinition
            {
                sectorName = name, position = pos, initialOwner = owner,
                garrisonStrength = garrison, isFortified = fortified,
                buildSlots = slots, resourceNodes = nodes
            };
        }
    }

    /// <summary>Shared utility for asset generators.</summary>
    public static class AssetGeneratorUtil
    {
        public static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }
    }
}
