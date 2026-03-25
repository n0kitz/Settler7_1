using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Creates larger maps for 4 players.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class FourPlayerMapFactory
    {
        /// <summary>
        /// 18-sector map for 4 players. Mid-size campaign.
        ///
        /// Layout (diamond shape):
        ///          [0]---[4]---[1]
        ///         / |   / | \   | \
        ///       [8] | [12] |[13] | [9]
        ///         \ | / | \ | \ | /
        ///         [4a]-[14]-[15]-[5a]
        ///         / | \ | / | / | \
        ///      [10] |[16] |[17] |[11]
        ///         \ |   \ | /   | /
        ///          [2]---[6]---[3]
        ///
        /// P0=0 (NW), P1=1 (NE), P2=2 (SW), P3=3 (SE)
        /// </summary>
        public static SectorGraph CreateEighteenSectorMap()
        {
            var g = new SectorGraph();

            // Player starts (corners) — forest + stone + fertile + water, no gold/iron
            g.AddSector(S(0, "Northwestern Hold", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 8));
            g.AddSector(S(1, "Northeastern Castle", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 8));
            g.AddSector(S(2, "Southwestern Fort", 2, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 8));
            g.AddSector(S(3, "Southeastern Bastion", 3, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 8));

            // Player expansion sectors (2 per player, adjacent)
            g.AddSector(S(4, "Northern Plains", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 6));
            g.AddSector(S(5, "Eastern Meadows", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.Forest }, 6));
            g.AddSector(S(6, "Southern Fields", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 6));
            g.AddSector(S(7, "Western Pastures", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.Forest }, 6));

            // Resource ring (coal/iron accessible 2 sectors from start)
            g.AddSector(S(8, "Iron Ridge", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Coal }, 5));
            g.AddSector(S(9, "Coal Valley", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Coal, ResourceNodeType.Stone }, 5));
            g.AddSector(S(10, "Copper Hills", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Stone }, 5));
            g.AddSector(S(11, "Quarry Bluffs", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Coal }, 5));

            // Contested center (fortified, gold, VP rewards)
            g.AddSector(S(12, "King's Crossroads", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Iron }, 4,
                "vp_kings_crossroads"));
            g.AddSector(S(13, "Ancient Monastery", Sector.NEUTRAL, 6, true,
                new[] { ResourceNodeType.Forest, ResourceNodeType.WaterSource }, 5));
            g.AddSector(S(14, "Grand Market", Sector.NEUTRAL, 6, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 6,
                "vp_grand_market"));
            g.AddSector(S(15, "Dragon's Mine", Sector.NEUTRAL, 10, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Iron, ResourceNodeType.Coal }, 3,
                "vp_dragons_mine"));

            // Bridge sectors (connecting opposite corners)
            g.AddSector(S(16, "River Crossing", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.FishingGround, ResourceNodeType.WaterSource }, 5));
            g.AddSector(S(17, "Hill Pass", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Forest }, 5));

            // Edges — player territory to expansion
            g.AddEdge(0, 4); g.AddEdge(0, 7);
            g.AddEdge(1, 4); g.AddEdge(1, 5);
            g.AddEdge(2, 6); g.AddEdge(2, 7);
            g.AddEdge(3, 5); g.AddEdge(3, 6);

            // Expansion to resource ring
            g.AddEdge(4, 8); g.AddEdge(4, 9);
            g.AddEdge(5, 9); g.AddEdge(5, 11);
            g.AddEdge(6, 10); g.AddEdge(6, 11);
            g.AddEdge(7, 8); g.AddEdge(7, 10);

            // Resource ring to center
            g.AddEdge(8, 12); g.AddEdge(8, 13);
            g.AddEdge(9, 12); g.AddEdge(9, 13);
            g.AddEdge(10, 14); g.AddEdge(10, 16);
            g.AddEdge(11, 14); g.AddEdge(11, 17);

            // Central connections
            g.AddEdge(12, 15); g.AddEdge(13, 15);
            g.AddEdge(14, 15); g.AddEdge(15, 16);
            g.AddEdge(15, 17); g.AddEdge(16, 17);

            return g;
        }

        /// <summary>
        /// 24-sector map for 4 players. Full-scale map.
        ///
        /// Players start in corners. Three rings of sectors:
        /// 1. Inner ring: player starts + immediate expansion (8 sectors)
        /// 2. Middle ring: resource-rich contested zones (8 sectors)
        /// 3. Outer ring: fortified high-value + central prizes (8 sectors)
        /// </summary>
        public static SectorGraph CreateTwentyFourSectorMap()
        {
            var g = new SectorGraph();

            // === Ring 1: Player starts + expansion ===
            g.AddSector(S(0, "Verdant Haven", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 9));
            g.AddSector(S(1, "Stormwatch Keep", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 9));
            g.AddSector(S(2, "Sunstone Citadel", 2, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 9));
            g.AddSector(S(3, "Ironwood Hall", 3, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 9));

            // Expansion sectors (1 per player, adjacent)
            g.AddSector(S(4, "Amber Fields", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 7));
            g.AddSector(S(5, "Silver Brook", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 7));
            g.AddSector(S(6, "Thornwood Grove", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.FertileLand }, 7));
            g.AddSector(S(7, "Windmill Heights", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.Forest }, 7));

            // === Ring 2: Resource ring (contested) ===
            g.AddSector(S(8, "Northern Ironworks", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Coal }, 5));
            g.AddSector(S(9, "Eastern Coal Pits", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Coal, ResourceNodeType.Stone }, 5));
            g.AddSector(S(10, "Southern Quarries", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Iron }, 5));
            g.AddSector(S(11, "Western Smelters", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Coal }, 5));

            // Contested between 2 players each
            g.AddSector(S(12, "Lakeside Hamlet", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.FishingGround, ResourceNodeType.FertileLand,
                        ResourceNodeType.WaterSource }, 6));
            g.AddSector(S(13, "Vineyard Slopes", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 6));
            g.AddSector(S(14, "Shepherd's Rest", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.Forest }, 6));
            g.AddSector(S(15, "Timber Hollow", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.FishingGround }, 6));

            // === Ring 3: High-value contested center ===
            g.AddSector(S(16, "Golden Crown", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Iron }, 4,
                "vp_golden_crown"));
            g.AddSector(S(17, "Sacred Monastery", Sector.NEUTRAL, 7, true,
                new[] { ResourceNodeType.Forest, ResourceNodeType.WaterSource }, 4));
            g.AddSector(S(18, "Imperial Arena", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Iron }, 4,
                "vp_imperial_arena"));
            g.AddSector(S(19, "Merchant's Bazaar", Sector.NEUTRAL, 6, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 5,
                "vp_merchants_bazaar"));

            // Central sectors
            g.AddSector(S(20, "Throne of Kings", Sector.NEUTRAL, 12, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Iron,
                        ResourceNodeType.Coal, ResourceNodeType.Stone }, 3,
                "vp_throne_of_kings"));
            g.AddSector(S(21, "Ancient Library", Sector.NEUTRAL, 6, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.WaterSource }, 4));
            g.AddSector(S(22, "Dragon's Hoard", Sector.NEUTRAL, 10, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Coal }, 3,
                "vp_dragons_hoard"));
            g.AddSector(S(23, "Crystal Caverns", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Stone }, 4));

            // === Edges ===
            // Player starts to expansion
            g.AddEdge(0, 4); g.AddEdge(0, 7);
            g.AddEdge(1, 4); g.AddEdge(1, 5);
            g.AddEdge(2, 6); g.AddEdge(2, 5);
            g.AddEdge(3, 6); g.AddEdge(3, 7);

            // Expansion to resource ring
            g.AddEdge(4, 8); g.AddEdge(4, 12);
            g.AddEdge(5, 9); g.AddEdge(5, 13);
            g.AddEdge(6, 10); g.AddEdge(6, 14);
            g.AddEdge(7, 11); g.AddEdge(7, 15);

            // Resource ring internal
            g.AddEdge(8, 12); g.AddEdge(8, 15);
            g.AddEdge(9, 12); g.AddEdge(9, 13);
            g.AddEdge(10, 13); g.AddEdge(10, 14);
            g.AddEdge(11, 14); g.AddEdge(11, 15);

            // Resource ring to high-value
            g.AddEdge(8, 16); g.AddEdge(9, 17);
            g.AddEdge(10, 18); g.AddEdge(11, 19);
            g.AddEdge(12, 16); g.AddEdge(12, 17);
            g.AddEdge(13, 17); g.AddEdge(13, 18);
            g.AddEdge(14, 18); g.AddEdge(14, 19);
            g.AddEdge(15, 19); g.AddEdge(15, 16);

            // Central connections
            g.AddEdge(16, 20); g.AddEdge(17, 20);
            g.AddEdge(18, 20); g.AddEdge(19, 20);
            g.AddEdge(16, 22); g.AddEdge(17, 21);
            g.AddEdge(18, 23); g.AddEdge(19, 21);
            g.AddEdge(20, 21); g.AddEdge(20, 22);
            g.AddEdge(20, 23); g.AddEdge(21, 22);
            g.AddEdge(22, 23); g.AddEdge(21, 23);

            return g;
        }

        private static Sector S(int id, string name, int owner, int garrison,
            bool fortified, ResourceNodeType[] resources, int buildSlots,
            string vpRewardId = null)
        {
            return new Sector(id, name, owner, garrison, fortified,
                new List<ResourceNodeType>(resources), buildSlots, vpRewardId);
        }
    }
}
