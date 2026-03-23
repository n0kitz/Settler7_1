using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Creates predefined test maps for development and testing.
    /// </summary>
    public static class TestMapFactory
    {
        /// <summary>
        /// Creates a 6-sector test map for 2 players.
        ///
        /// Layout (sector IDs):
        ///       [0]---[1]
        ///      / |     | \
        ///    [2] |     | [3]
        ///      \ |     | /
        ///       [4]---[5]
        ///
        /// Player 0 starts with sector 0 (top-left).
        /// Player 1 starts with sector 1 (top-right).
        /// Sectors 2–5 are neutral with garrisons.
        /// </summary>
        public static SectorGraph CreateSixSectorMap()
        {
            var graph = new SectorGraph();

            // Sector 0: Player 0 start — forest + stone + fertile land + water
            graph.AddSector(new Sector(
                id: 0,
                name: "Greenwood Heights",
                ownerId: 0,
                garrisonStrength: 0,
                isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Forest, ResourceNodeType.Stone,
                      ResourceNodeType.FertileLand, ResourceNodeType.WaterSource },
                buildSlots: 8
            ));

            // Sector 1: Player 1 start — forest + stone + fertile land + water
            graph.AddSector(new Sector(
                id: 1,
                name: "Redcliff Valley",
                ownerId: 1,
                garrisonStrength: 0,
                isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Forest, ResourceNodeType.Stone,
                      ResourceNodeType.FertileLand, ResourceNodeType.WaterSource },
                buildSlots: 8
            ));

            // Sector 2: Neutral — fertile land + fishing
            graph.AddSector(new Sector(
                id: 2,
                name: "Riverside Meadows",
                ownerId: Sector.NEUTRAL,
                garrisonStrength: 4,
                isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.FertileLand, ResourceNodeType.FishingGround },
                buildSlots: 6
            ));

            // Sector 3: Neutral — iron + coal (fortified)
            graph.AddSector(new Sector(
                id: 3,
                name: "Ironpeak Pass",
                ownerId: Sector.NEUTRAL,
                garrisonStrength: 8,
                isFortified: true,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Iron, ResourceNodeType.Coal },
                buildSlots: 6
            ));

            // Sector 4: Neutral — stone + fertile land
            graph.AddSector(new Sector(
                id: 4,
                name: "Stonefield Plains",
                ownerId: Sector.NEUTRAL,
                garrisonStrength: 4,
                isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Stone, ResourceNodeType.FertileLand },
                buildSlots: 6
            ));

            // Sector 5: Neutral — gold + stone (fortified, grants VP)
            graph.AddSector(new Sector(
                id: 5,
                name: "Goldcrest Summit",
                ownerId: Sector.NEUTRAL,
                garrisonStrength: 8,
                isFortified: true,
                resourceNodes: new List<ResourceNodeType>
                    { ResourceNodeType.Gold, ResourceNodeType.Stone },
                buildSlots: 5,
                vpRewardId: "vp_special_sector_goldcrest"
            ));

            // Edges (see layout diagram above)
            graph.AddEdge(0, 1);  // top horizontal
            graph.AddEdge(0, 2);  // top-left to mid-left
            graph.AddEdge(0, 4);  // top-left to bottom-left
            graph.AddEdge(1, 3);  // top-right to mid-right
            graph.AddEdge(1, 5);  // top-right to bottom-right
            graph.AddEdge(4, 5);  // bottom horizontal
            graph.AddEdge(2, 4);  // mid-left to bottom-left
            graph.AddEdge(3, 5);  // mid-right to bottom-right

            return graph;
        }
    }
}
