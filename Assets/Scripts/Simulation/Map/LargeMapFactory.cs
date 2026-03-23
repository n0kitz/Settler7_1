using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Creates a larger 12-sector map for 3 players.
    /// Varied terrain with multiple conquest paths.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class LargeMapFactory
    {
        /// <summary>
        /// Creates a 12-sector map for 3 players.
        ///
        /// Layout:
        ///       [0]---[1]---[2]
        ///       / \   / \   / \
        ///     [3] [4] [5] [6] [7]
        ///       \ /   \ /   \ /
        ///       [8]---[9]--[10]
        ///              |
        ///             [11]
        ///
        /// P0=sector 0, P1=sector 2, P2=sector 10
        /// Sector 11 has VP reward (central prize).
        /// </summary>
        public static SectorGraph CreateTwelveSectorMap()
        {
            var graph = new SectorGraph();

            // Player starts
            graph.AddSector(new Sector(0, "Northern Forest",
                ownerId: 0, garrisonStrength: 0, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Forest, ResourceNodeType.Stone,
                      ResourceNodeType.FertileLand, ResourceNodeType.WaterSource },
                buildSlots: 8));

            graph.AddSector(new Sector(1, "Highland Pass",
                ownerId: Sector.NEUTRAL, garrisonStrength: 4, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Stone, ResourceNodeType.Coal },
                buildSlots: 5));

            graph.AddSector(new Sector(2, "Eastern Cliffs",
                ownerId: 1, garrisonStrength: 0, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Forest, ResourceNodeType.Stone,
                      ResourceNodeType.FertileLand, ResourceNodeType.WaterSource },
                buildSlots: 8));

            graph.AddSector(new Sector(3, "Western Meadows",
                ownerId: Sector.NEUTRAL, garrisonStrength: 3, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.FertileLand, ResourceNodeType.FishingGround },
                buildSlots: 6));

            graph.AddSector(new Sector(4, "Iron Hills",
                ownerId: Sector.NEUTRAL, garrisonStrength: 6, isFortified: true,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Iron, ResourceNodeType.Coal, ResourceNodeType.Stone },
                buildSlots: 6));

            graph.AddSector(new Sector(5, "Central Plains",
                ownerId: Sector.NEUTRAL, garrisonStrength: 4, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource },
                buildSlots: 7));

            graph.AddSector(new Sector(6, "Shepherd's Glen",
                ownerId: Sector.NEUTRAL, garrisonStrength: 4, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.FertileLand, ResourceNodeType.Forest },
                buildSlots: 6));

            graph.AddSector(new Sector(7, "Eastern Quarry",
                ownerId: Sector.NEUTRAL, garrisonStrength: 5, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Stone, ResourceNodeType.Iron },
                buildSlots: 5));

            graph.AddSector(new Sector(8, "Southern Farmlands",
                ownerId: Sector.NEUTRAL, garrisonStrength: 3, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.FertileLand, ResourceNodeType.FishingGround,
                      ResourceNodeType.WaterSource },
                buildSlots: 7));

            graph.AddSector(new Sector(9, "Gold River Valley",
                ownerId: Sector.NEUTRAL, garrisonStrength: 8, isFortified: true,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Gold, ResourceNodeType.Coal },
                buildSlots: 5));

            graph.AddSector(new Sector(10, "Southern Fortress",
                ownerId: 2, garrisonStrength: 0, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Forest, ResourceNodeType.Stone,
                      ResourceNodeType.FertileLand, ResourceNodeType.WaterSource },
                buildSlots: 8));

            graph.AddSector(new Sector(11, "Dragon's Peak",
                ownerId: Sector.NEUTRAL, garrisonStrength: 12, isFortified: true,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Gold, ResourceNodeType.Iron, ResourceNodeType.Stone },
                buildSlots: 4,
                vpRewardId: "vp_special_sector_dragons_peak"));

            // Edges — see layout diagram
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(0, 3);
            graph.AddEdge(0, 4);
            graph.AddEdge(1, 4);
            graph.AddEdge(1, 5);
            graph.AddEdge(2, 6);
            graph.AddEdge(2, 7);
            graph.AddEdge(1, 6);
            graph.AddEdge(3, 8);
            graph.AddEdge(4, 8);
            graph.AddEdge(4, 5);
            graph.AddEdge(5, 9);
            graph.AddEdge(5, 6);
            graph.AddEdge(6, 7);
            graph.AddEdge(7, 10);
            graph.AddEdge(8, 9);
            graph.AddEdge(9, 10);
            graph.AddEdge(9, 11);

            return graph;
        }
    }
}
