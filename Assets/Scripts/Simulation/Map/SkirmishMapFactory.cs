using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Sprint-7a skirmish maps: Highland Duel (20, 2p), Golden Meadows (30, 3p),
    /// The Frontier (40, 4p). The two larger maps are built from identical
    /// player wedges, so resource distances are fair by construction (§1 map
    /// rules: start has Forest/Stone/FertileLand, no Gold/Iron; Coal 1 step,
    /// Iron 2, Gold 3; chokepoints; 2+ expansion paths per player).
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class SkirmishMapFactory
    {
        // --- Highland Duel (20 sectors, 2 players) --------------------------
        // Mirrored duel: a western mining ridge and an eastern loch route,
        // wooded flanks as alternate paths, gold contested at both mid-points.
        public static MapFactory.MapInfo CreateHighlandDuel()
        {
            var g = new SectorGraph();

            // Player homes + fields (mirror pair)
            g.AddSector(S(0, "Western Hold", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 10));
            g.AddSector(S(1, "Western Fields", 0, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 8));
            g.AddSector(S(2, "Eastern Hold", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 10));
            g.AddSector(S(3, "Eastern Fields", 1, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 8));

            // Mining ridge (P0 enters at 4, P1 at 8; gold summit contested mid)
            g.AddSector(S(4, "Coal Foothills", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.Coal, ResourceNodeType.Stone }, 6));
            g.AddSector(S(5, "Iron Crags", Sector.NEUTRAL, 5, true,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Coal }, 5));
            g.AddSector(S(6, "Golden Summit", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Stone }, 4));
            g.AddSector(S(7, "Iron Scree", Sector.NEUTRAL, 5, true,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Stone }, 5));
            g.AddSector(S(8, "Peat Slopes", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.Coal, ResourceNodeType.FertileLand }, 6));

            // Loch route (P0 enters at 9, P1 at 13; gold isle contested mid)
            g.AddSector(S(9, "Heather Meadows", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 7));
            g.AddSector(S(10, "Loch Shore", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FishingGround, ResourceNodeType.FertileLand }, 6));
            g.AddSector(S(11, "Loch Isle", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.FishingGround }, 4));
            g.AddSector(S(12, "Reed Banks", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FishingGround, ResourceNodeType.FertileLand }, 6));
            g.AddSector(S(13, "Bloom Meadows", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 7));

            // Wooded flanks (alternate approaches)
            g.AddSector(S(14, "Northwest Woods", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone }, 6));
            g.AddSector(S(15, "Northeast Woods", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.FertileLand }, 6));
            g.AddSector(S(16, "Southwest Woods", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone }, 6));
            g.AddSector(S(17, "Southeast Woods", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.FertileLand }, 6));

            // Contested center between the two routes
            g.AddSector(S(18, "Old Bridge", Sector.NEUTRAL, 6, false,
                new[] { ResourceNodeType.Stone }, 5));
            g.AddSector(S(19, "Standing Stones", Sector.NEUTRAL, 7, true,
                new[] { ResourceNodeType.Stone, ResourceNodeType.FertileLand }, 4));

            // Home territory
            g.AddEdge(0, 1); g.AddEdge(2, 3);
            // Mining ridge chain: P0 → 4-5-6-7-8 → P1
            g.AddEdge(0, 4); g.AddEdge(4, 5); g.AddEdge(5, 6);
            g.AddEdge(6, 7); g.AddEdge(7, 8); g.AddEdge(8, 2);
            // Loch route chain: P0 → 9-10-11-12-13 → P1
            g.AddEdge(1, 9); g.AddEdge(9, 10); g.AddEdge(10, 11);
            g.AddEdge(11, 12); g.AddEdge(12, 13); g.AddEdge(13, 3);
            // Flank alternates (2nd expansion path per player, both routes)
            g.AddEdge(0, 14); g.AddEdge(14, 4);
            g.AddEdge(1, 15); g.AddEdge(15, 10);
            g.AddEdge(2, 16); g.AddEdge(16, 8);
            g.AddEdge(3, 17); g.AddEdge(17, 12);
            // Center web linking the two contested golds
            g.AddEdge(6, 18); g.AddEdge(11, 18); g.AddEdge(18, 19);
            g.AddEdge(10, 19); g.AddEdge(12, 19);

            return new MapFactory.MapInfo
            {
                Id = "highland_duel",
                DisplayName = "Highland Duel (20 Sectors)",
                PlayerCount = 2,
                VPRequired = 5,
                Graph = g
            };
        }

        // --- Golden Meadows (30 sectors, 3 players) -------------------------
        public static MapFactory.MapInfo CreateGoldenMeadows()
        {
            var g = new SectorGraph();
            string[] prefixes = { "Northern", "Eastern", "Western" };

            for (int p = 0; p < 3; p++)
                AddWedge(g, p, p * WEDGE_SIZE, prefixes[p]);

            // Contested center (ids 27-29)
            g.AddSector(S(27, "Grand Monastery", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 5));
            g.AddSector(S(28, "Golden Basin", Sector.NEUTRAL, 9, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Coal }, 4));
            g.AddSector(S(29, "Crossroads", Sector.NEUTRAL, 5, false,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Forest }, 6));

            for (int p = 0; p < 3; p++)
            {
                int b = p * WEDGE_SIZE;
                // Wedge ring: this wedge's border woods meet the next watch hill
                g.AddEdge(b + 7, ((p + 1) % 3) * WEDGE_SIZE + 8);
                // Every wedge reaches all three center sectors
                g.AddEdge(b + 5, 27); g.AddEdge(b + 6, 28); g.AddEdge(b + 7, 29);
            }
            g.AddEdge(27, 28); g.AddEdge(27, 29); g.AddEdge(28, 29);

            return new MapFactory.MapInfo
            {
                Id = "golden_meadows",
                DisplayName = "Golden Meadows (30 Sectors, 3 Players)",
                PlayerCount = 3,
                VPRequired = 6,
                Graph = g
            };
        }

        // --- The Frontier (40 sectors, 4 players) ---------------------------
        public static MapFactory.MapInfo CreateTheFrontier()
        {
            var g = new SectorGraph();
            string[] prefixes = { "Northern", "Eastern", "Southern", "Western" };

            for (int p = 0; p < 4; p++)
                AddWedge(g, p, p * WEDGE_SIZE, prefixes[p]);

            // Contested center (ids 36-39)
            g.AddSector(S(36, "Frontier Citadel", Sector.NEUTRAL, 12, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Iron }, 4));
            g.AddSector(S(37, "Great Plains", Sector.NEUTRAL, 6, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 8));
            g.AddSector(S(38, "Twin Mines", Sector.NEUTRAL, 8, true,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Coal }, 5));
            g.AddSector(S(39, "Old Cathedral", Sector.NEUTRAL, 9, true,
                new[] { ResourceNodeType.Stone, ResourceNodeType.FertileLand }, 5));

            for (int p = 0; p < 4; p++)
            {
                int b = p * WEDGE_SIZE;
                g.AddEdge(b + 7, ((p + 1) % 4) * WEDGE_SIZE + 8);
                g.AddEdge(b + 6, 36); g.AddEdge(b + 5, 37);
                // Opposite pairs share a deep-center objective each
                g.AddEdge(b + 7, p % 2 == 0 ? 38 : 39);
            }
            g.AddEdge(36, 37); g.AddEdge(36, 38); g.AddEdge(36, 39);
            g.AddEdge(37, 38); g.AddEdge(37, 39);

            return new MapFactory.MapInfo
            {
                Id = "the_frontier",
                DisplayName = "The Frontier (40 Sectors, 4 Players)",
                PlayerCount = 4,
                VPRequired = 7,
                Graph = g
            };
        }

        // --- Shared wedge template (9 sectors per player) --------------------
        // Coal 1 step from home, Iron 2, Gold 3 — identical for every player.
        private const int WEDGE_SIZE = 9;

        private static void AddWedge(SectorGraph g, int playerId, int b, string prefix)
        {
            g.AddSector(S(b + 0, $"{prefix} Hold", playerId, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 10));
            g.AddSector(S(b + 1, $"{prefix} Fields", playerId, 0, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.Stone,
                        ResourceNodeType.FertileLand, ResourceNodeType.FishingGround }, 8));
            g.AddSector(S(b + 2, $"{prefix} Coal Hills", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.Coal, ResourceNodeType.Stone }, 6));
            g.AddSector(S(b + 3, $"{prefix} Iron Hollow", Sector.NEUTRAL, 5, true,
                new[] { ResourceNodeType.Iron, ResourceNodeType.Coal }, 5));
            g.AddSector(S(b + 4, $"{prefix} Meadow", Sector.NEUTRAL, 2, false,
                new[] { ResourceNodeType.FertileLand, ResourceNodeType.WaterSource }, 7));
            g.AddSector(S(b + 5, $"{prefix} Fishing Lake", Sector.NEUTRAL, 3, false,
                new[] { ResourceNodeType.FishingGround, ResourceNodeType.FertileLand }, 6));
            g.AddSector(S(b + 6, $"{prefix} Gold Cliffs", Sector.NEUTRAL, 7, true,
                new[] { ResourceNodeType.Gold, ResourceNodeType.Stone }, 4));
            g.AddSector(S(b + 7, $"{prefix} Border Woods", Sector.NEUTRAL, 4, false,
                new[] { ResourceNodeType.Forest, ResourceNodeType.FertileLand }, 6));
            g.AddSector(S(b + 8, $"{prefix} Watch Hill", Sector.NEUTRAL, 6, true,
                new[] { ResourceNodeType.Stone, ResourceNodeType.Iron }, 5));

            // Two expansion paths from home: mining (2→3→6) and farming (1→4→5)
            g.AddEdge(b + 0, b + 1); g.AddEdge(b + 0, b + 2);
            g.AddEdge(b + 2, b + 3); g.AddEdge(b + 3, b + 6);
            g.AddEdge(b + 1, b + 4); g.AddEdge(b + 4, b + 5);
            g.AddEdge(b + 5, b + 6);
            // Flanks toward the neighbors
            g.AddEdge(b + 1, b + 7); g.AddEdge(b + 2, b + 8);
        }

        private static Sector S(int id, string name, int owner, int garrison,
            bool fortified, ResourceNodeType[] resources, int buildSlots)
        {
            return new Sector(id, name, owner, garrison, fortified,
                new List<ResourceNodeType>(resources), buildSlots);
        }
    }
}
