using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Builds the dedicated tutorial map.
    /// Small 5-sector layout: one player, no AI, with clear expansion targets.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class TutorialMapFactory
    {
        /// <summary>Create the tutorial map (single player, no opponents).</summary>
        public static MapFactory.MapInfo Create()
        {
            var g = new SectorGraph();

            // Sector 0: Player starting settlement — good all-round resources
            g.AddSector(new Sector(0, "Your Settlement", ownerId: 0, garrisonStrength: 0,
                isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                {
                    ResourceNodeType.Forest,
                    ResourceNodeType.Stone,
                    ResourceNodeType.FertileLand,
                    ResourceNodeType.WaterSource,
                },
                buildSlots: 12));

            // Sector 1: Easy neutral — farmland for food chain lesson
            g.AddSector(new Sector(1, "Green Meadows", ownerId: Sector.NEUTRAL,
                garrisonStrength: 2, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                {
                    ResourceNodeType.FertileLand,
                    ResourceNodeType.FishingGround,
                },
                buildSlots: 8));

            // Sector 2: Easy neutral — forest/stone for basic production
            g.AddSector(new Sector(2, "Birchwood", ownerId: Sector.NEUTRAL,
                garrisonStrength: 2, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                {
                    ResourceNodeType.Forest,
                    ResourceNodeType.Stone,
                },
                buildSlots: 6));

            // Sector 3: Medium neutral — iron & coal, teaches military need
            g.AddSector(new Sector(3, "Iron Hill", ownerId: Sector.NEUTRAL,
                garrisonStrength: 5, isFortified: false,
                resourceNodes: new List<ResourceNodeType>
                {
                    ResourceNodeType.Iron,
                    ResourceNodeType.Coal,
                },
                buildSlots: 6));

            // Sector 4: Hard neutral — gold, fortified, endgame goal
            g.AddSector(new Sector(4, "Gold Peak", ownerId: Sector.NEUTRAL,
                garrisonStrength: 8, isFortified: true,
                resourceNodes: new List<ResourceNodeType>
                {
                    ResourceNodeType.Gold,
                    ResourceNodeType.Stone,
                },
                buildSlots: 4));

            // Graph edges: linear expansion with a shortcut
            g.AddEdge(0, 1);
            g.AddEdge(0, 2);
            g.AddEdge(1, 3);
            g.AddEdge(2, 3);
            g.AddEdge(3, 4);

            return new MapFactory.MapInfo
            {
                Id = "tutorial",
                DisplayName = "Tutorial Valley",
                PlayerCount = 1,
                VPRequired = 3,
                Graph = g,
            };
        }
    }
}
