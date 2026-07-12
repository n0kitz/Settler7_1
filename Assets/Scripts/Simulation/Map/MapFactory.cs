using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Factory for all predefined maps. Each map has a unique layout, resource
    /// distribution, and victory point requirement.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class MapFactory
    {
        public struct MapInfo
        {
            public string Id;
            public string DisplayName;
            public int PlayerCount;
            public int VPRequired;
            public SectorGraph Graph;
        }

        /// <summary>Get a map by ID.</summary>
        public static MapInfo CreateMap(string mapId)
        {
            return mapId switch
            {
                "tutorial"      => TutorialMapFactory.Create(),
                "test_valley"   => CreateTestValley(),
                "twin_rivers"   => CreateTwinRivers(),
                "mountain_pass" => CreateMountainPass(),
                "island_chain"   => CreateIslandChain(),
                "large_valley"   => CreateLargeValley(),
                "crown_war"      => CreateCrownWar(),
                "empire"         => CreateEmpire(),
                "highland_duel"  => SkirmishMapFactory.CreateHighlandDuel(),
                "golden_meadows" => SkirmishMapFactory.CreateGoldenMeadows(),
                "the_frontier"   => SkirmishMapFactory.CreateTheFrontier(),
                _                => CreateTestValley()
            };
        }

        /// <summary>Get all skirmish map IDs (excludes tutorial).</summary>
        public static string[] GetMapIds() => new[]
        {
            "test_valley", "twin_rivers", "mountain_pass", "island_chain", "large_valley",
            "crown_war", "empire", "highland_duel", "golden_meadows", "the_frontier"
        };

        /// <summary>True if this map ID is the tutorial.</summary>
        public static bool IsTutorial(string mapId) => mapId == "tutorial";

        // --- Map 1: Test Valley (6 sectors, 2 players) ---
        // This is the existing TestMapFactory map, kept for compatibility.
        public static MapInfo CreateTestValley()
        {
            return new MapInfo
            {
                Id = "test_valley",
                DisplayName = "Test Valley (6 Sectors)",
                PlayerCount = 2,
                VPRequired = 4,
                Graph = TestMapFactory.CreateSixSectorMap()
            };
        }

        // --- Map 2: Twin Rivers (10 sectors, 2 players) ---
        // Two river valleys separated by a mountain ridge.
        // Each player starts in their valley with good resources.
        // Central mountain sectors are fortified and resource-rich.
        public static MapInfo CreateTwinRivers()
        {
            var g = new SectorGraph();

            // Player 0 valley (west)
            g.AddSector(S(0, "Western Settlement", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 10));
            g.AddSector(S(1, "Western Farmlands", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 8));
            g.AddSector(S(2, "Western Quarry", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Forest }, 6));

            // Player 1 valley (east)
            g.AddSector(S(3, "Eastern Settlement", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 10));
            g.AddSector(S(4, "Eastern Farmlands", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 8));
            g.AddSector(S(5, "Eastern Quarry", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Forest }, 6));

            // Mountain ridge (center, contested)
            g.AddSector(S(6, "Iron Ridge", Sector.NEUTRAL, 6, true,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Coal }, 5));
            g.AddSector(S(7, "Gold Peak", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Stone }, 4));
            g.AddSector(S(8, "Mountain Pass", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Coal }, 6));
            g.AddSector(S(9, "Dragon's Lair", Sector.NEUTRAL, 10, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Iron }, 3));

            // West valley edges
            g.AddEdge(0, 1); g.AddEdge(0, 2); g.AddEdge(1, 2);
            // East valley edges
            g.AddEdge(3, 4); g.AddEdge(3, 5); g.AddEdge(4, 5);
            // Valley to mountain
            g.AddEdge(2, 6); g.AddEdge(2, 8);
            g.AddEdge(5, 7); g.AddEdge(5, 8);
            // Mountain internal
            g.AddEdge(6, 8); g.AddEdge(7, 8);
            g.AddEdge(6, 9); g.AddEdge(7, 9);

            return new MapInfo
            {
                Id = "twin_rivers",
                DisplayName = "Twin Rivers (10 Sectors)",
                PlayerCount = 2,
                VPRequired = 5,
                Graph = g
            };
        }

        // --- Map 3: Mountain Pass (12 sectors, 3 players) ---
        // Three players start in corners, fighting over a central pass.
        public static MapInfo CreateMountainPass()
        {
            var g = new SectorGraph();

            // Player 0 (northwest)
            g.AddSector(S(0, "Northern Keep", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 10));
            g.AddSector(S(1, "Northern Fields", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 7));

            // Player 1 (northeast)
            g.AddSector(S(2, "Eastern Fortress", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 10));
            g.AddSector(S(3, "Eastern Pastures", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand }, 7));

            // Player 2 (south)
            g.AddSector(S(4, "Southern Citadel", 2, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 10));
            g.AddSector(S(5, "Southern Mines", 2, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.Iron }, 7));

            // Neutral contested zones
            g.AddSector(S(6, "North Pass", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Forest }, 6));
            g.AddSector(S(7, "East Pass", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.Coal, ResourceNodeType.Forest }, 6));
            g.AddSector(S(8, "South Pass", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.Iron, ResourceNodeType.FertileLand }, 6));
            g.AddSector(S(9, "Central Stronghold", Sector.NEUTRAL, 10, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Iron, ResourceNodeType.Coal }, 5));
            g.AddSector(S(10, "Ancient Ruins", Sector.NEUTRAL, 6, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Gold }, 4));
            g.AddSector(S(11, "Sacred Grove", Sector.NEUTRAL, 6, true,
                new[] { ResourceNodeType.Forest, ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 5));

            // Player territory edges
            g.AddEdge(0, 1); g.AddEdge(2, 3); g.AddEdge(4, 5);
            // Expansion paths
            g.AddEdge(0, 6); g.AddEdge(1, 6);
            g.AddEdge(2, 7); g.AddEdge(3, 7);
            g.AddEdge(4, 8); g.AddEdge(5, 8);
            // Pass to center
            g.AddEdge(6, 9); g.AddEdge(7, 9); g.AddEdge(8, 9);
            g.AddEdge(6, 11); g.AddEdge(7, 10); g.AddEdge(8, 10);
            g.AddEdge(9, 10); g.AddEdge(9, 11);
            g.AddEdge(10, 11);

            return new MapInfo
            {
                Id = "mountain_pass",
                DisplayName = "Mountain Pass (12 Sectors, 3 Players)",
                PlayerCount = 3,
                VPRequired = 5,
                Graph = g
            };
        }

        // --- Map 4: Island Chain (8 sectors, 2 players) ---
        // Linear island chain — each player starts on an end.
        // Must expand through the middle. Few alternate paths.
        public static MapInfo CreateIslandChain()
        {
            var g = new SectorGraph();

            g.AddSector(S(0, "Western Isle", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 9));
            g.AddSector(S(1, "Coral Bay", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 6));
            g.AddSector(S(2, "Volcano Island", Sector.NEUTRAL, 6, true,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Coal, ResourceNodeType.Stone }, 5));
            g.AddSector(S(3, "Treasure Atoll", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Stone }, 4));
            g.AddSector(S(4, "Palm Harbor", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.Forest }, 6));
            g.AddSector(S(5, "Jade Reef", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.FishingGround, ResourceNodeType.WaterSource }, 5));
            g.AddSector(S(6, "Spice Island", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.Forest }, 6));
            g.AddSector(S(7, "Eastern Isle", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 9));

            // Linear chain with one shortcut
            g.AddEdge(0, 1); g.AddEdge(1, 2); g.AddEdge(2, 3);
            g.AddEdge(3, 4); g.AddEdge(4, 5); g.AddEdge(5, 6);
            g.AddEdge(6, 7);
            // Shortcut bridges
            g.AddEdge(1, 5); // North bridge
            g.AddEdge(2, 4); // South bridge

            return new MapInfo
            {
                Id = "island_chain",
                DisplayName = "Island Chain (8 Sectors)",
                PlayerCount = 2,
                VPRequired = 4,
                Graph = g
            };
        }

        // --- Map 5: Large Valley (12 sectors, 3 players) ---
        // Expansive valley with varied terrain. Uses LargeMapFactory.
        public static MapInfo CreateLargeValley()
        {
            return new MapInfo
            {
                Id = "large_valley",
                DisplayName = "Large Valley (12 Sectors, 3 Players)",
                PlayerCount = 3,
                VPRequired = 5,
                Graph = LargeMapFactory.CreateTwelveSectorMap()
            };
        }

        // --- Map 6: Crown War (18 sectors, 4 players) ---
        // Diamond layout with 4 player corners, resource ring, and contested center.
        public static MapInfo CreateCrownWar()
        {
            return new MapInfo
            {
                Id = "crown_war",
                DisplayName = "Crown War (18 Sectors, 4 Players)",
                PlayerCount = 4,
                VPRequired = 5,
                Graph = FourPlayerMapFactory.CreateEighteenSectorMap()
            };
        }

        // --- Map 7: Empire (24 sectors, 4 players) ---
        // Full-scale map with three rings of expansion.
        public static MapInfo CreateEmpire()
        {
            return new MapInfo
            {
                Id = "empire",
                DisplayName = "Empire (24 Sectors, 4 Players)",
                PlayerCount = 4,
                VPRequired = 6,
                Graph = FourPlayerMapFactory.CreateTwentyFourSectorMap()
            };
        }

        private static Sector S(int id, string name, int owner, int garrison,
            bool fortified, ResourceNodeType[] resources, int buildSlots)
        {
            return new Sector(id, name, owner, garrison, fortified,
                new List<ResourceNodeType>(resources), buildSlots);
        }
    }
}
