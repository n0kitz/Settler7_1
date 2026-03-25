namespace Settlers.Simulation
{
    /// <summary>
    /// Trade map factories for 4-player maps (crown_war, empire).
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class FourPlayerTradeMapFactory
    {
        /// <summary>
        /// Trade map for Crown War (18 sectors). 14 outposts.
        /// Tier 1: basic exchanges near start.
        /// Tier 2: advanced exchanges mid-distance.
        /// Tier 3: special VP outposts deep in the network.
        /// </summary>
        public static TradeMap CreateCrownWarTradeMap()
        {
            var map = new TradeMap();

            // Tier 1 — basic (3-4 steps from start)
            map.AddOutpost(new TradeOutpost("cw_planks_stone",
                "Mason's Exchange", ResourceType.Planks, 3, ResourceType.Stone, 2));
            map.AddOutpost(new TradeOutpost("cw_grain_bread",
                "Baker's Exchange", ResourceType.Grain, 2, ResourceType.Bread, 1));
            map.AddOutpost(new TradeOutpost("cw_wood_tools",
                "Carpenter's Exchange", ResourceType.Wood, 4, ResourceType.Tools, 1));
            map.AddOutpost(new TradeOutpost("cw_wool_cloth",
                "Weaver's Exchange", ResourceType.Wool, 2, ResourceType.Cloth, 1));

            // Tier 2 — advanced (5-7 steps)
            map.AddOutpost(new TradeOutpost("cw_iron_weapons",
                "Armory Exchange", ResourceType.IronBars, 2, ResourceType.Weapons, 1));
            map.AddOutpost(new TradeOutpost("cw_gold_coins",
                "Mint Exchange", ResourceType.GoldOre, 1, ResourceType.Coins, 3));
            map.AddOutpost(new TradeOutpost("cw_cloth_garments",
                "Tailor's Exchange", ResourceType.Cloth, 2, ResourceType.Garments, 1));
            map.AddOutpost(new TradeOutpost("cw_paper_books",
                "Scribe's Exchange", ResourceType.Paper, 2, ResourceType.Books, 1));
            map.AddOutpost(new TradeOutpost("cw_coins_horses",
                "Stable Exchange", ResourceType.Coins, 4, ResourceType.Horses, 1));
            map.AddOutpost(new TradeOutpost("cw_beer_coins",
                "Tavern Exchange", ResourceType.Beer, 3, ResourceType.Coins, 5));

            // Tier 3 — special VP outposts (8+ steps)
            map.AddOutpost(new TradeOutpost("cw_jewelry_trade",
                "Crown Jeweler", ResourceType.Jewelry, 2, ResourceType.Coins, 10,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("cw_relics_trade",
                "Relic Exchange", ResourceType.Books, 3, ResourceType.Jewelry, 2,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("cw_exotic_trade",
                "Exotic Goods", ResourceType.Garments, 3, ResourceType.GoldOre, 2,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("cw_royal_trade",
                "Royal Commission", ResourceType.Weapons, 3, ResourceType.Coins, 8,
                isSpecial: false));

            // Network — 4 branches (one per player direction) converging at center
            map.AddRoute(0, 1);  // planks—grain
            map.AddRoute(1, 2);  // grain—tools
            map.AddRoute(2, 3);  // tools—cloth
            map.AddRoute(3, 0);  // cloth—planks (ring)
            map.AddRoute(0, 4);  // planks—weapons
            map.AddRoute(1, 5);  // grain—coins
            map.AddRoute(2, 6);  // tools—garments
            map.AddRoute(3, 7);  // cloth—books
            map.AddRoute(4, 8);  // weapons—horses
            map.AddRoute(5, 9);  // coins—beer
            map.AddRoute(6, 10); // garments—jeweler
            map.AddRoute(7, 11); // books—relics
            map.AddRoute(8, 12); // horses—exotic
            map.AddRoute(9, 13); // beer—royal
            map.AddRoute(10, 12); // jeweler—exotic
            map.AddRoute(11, 13); // relics—royal
            map.AddRoute(4, 5);  // cross-link mid
            map.AddRoute(6, 7);  // cross-link mid

            return map;
        }

        /// <summary>
        /// Trade map for Empire (24 sectors). 18 outposts.
        /// Larger network with more branching and 4 special VP outposts.
        /// </summary>
        public static TradeMap CreateEmpireTradeMap()
        {
            var map = new TradeMap();

            // Tier 1 — basic (3-4 steps)
            map.AddOutpost(new TradeOutpost("emp_planks_stone",
                "Quarry Exchange", ResourceType.Planks, 3, ResourceType.Stone, 2));
            map.AddOutpost(new TradeOutpost("emp_grain_bread",
                "Bakery Exchange", ResourceType.Grain, 2, ResourceType.Bread, 1));
            map.AddOutpost(new TradeOutpost("emp_wood_tools",
                "Workshop Exchange", ResourceType.Wood, 4, ResourceType.Tools, 1));
            map.AddOutpost(new TradeOutpost("emp_wool_cloth",
                "Loom Exchange", ResourceType.Wool, 2, ResourceType.Cloth, 1));
            map.AddOutpost(new TradeOutpost("emp_fish_bread",
                "Harbor Exchange", ResourceType.Fish, 2, ResourceType.Bread, 1));

            // Tier 2 — advanced (5-7 steps)
            map.AddOutpost(new TradeOutpost("emp_iron_weapons",
                "Imperial Armory", ResourceType.IronBars, 2, ResourceType.Weapons, 1));
            map.AddOutpost(new TradeOutpost("emp_gold_coins",
                "Imperial Mint", ResourceType.GoldOre, 1, ResourceType.Coins, 3));
            map.AddOutpost(new TradeOutpost("emp_cloth_garments",
                "Imperial Tailor", ResourceType.Cloth, 2, ResourceType.Garments, 1));
            map.AddOutpost(new TradeOutpost("emp_paper_books",
                "Imperial Library", ResourceType.Paper, 2, ResourceType.Books, 1));
            map.AddOutpost(new TradeOutpost("emp_coins_horses",
                "Imperial Stables", ResourceType.Coins, 4, ResourceType.Horses, 1));
            map.AddOutpost(new TradeOutpost("emp_beer_coins",
                "Grand Tavern", ResourceType.Beer, 3, ResourceType.Coins, 5));
            map.AddOutpost(new TradeOutpost("emp_coal_iron",
                "Smelter Exchange", ResourceType.Coal, 2, ResourceType.IronBars, 1));
            map.AddOutpost(new TradeOutpost("emp_sausage_coins",
                "Butcher's Exchange", ResourceType.Sausages, 2, ResourceType.Coins, 4));

            // Tier 3 — special VP outposts (8+ steps)
            map.AddOutpost(new TradeOutpost("emp_special_silk",
                "Silk Road", ResourceType.Garments, 3, ResourceType.Jewelry, 2,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("emp_special_relics",
                "Ancient Relics", ResourceType.Books, 3, ResourceType.Coins, 12,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("emp_special_crown",
                "Crown Commission", ResourceType.Jewelry, 3, ResourceType.Coins, 15,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("emp_special_war",
                "War Council", ResourceType.Weapons, 4, ResourceType.Horses, 2,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("emp_luxury_trade",
                "Luxury Exchange", ResourceType.Coins, 8, ResourceType.Jewelry, 1,
                isSpecial: false));

            // Network — larger web with 4 quadrant branches + central hub
            // Tier 1 ring
            map.AddRoute(0, 1);
            map.AddRoute(1, 2);
            map.AddRoute(2, 3);
            map.AddRoute(3, 4);
            map.AddRoute(4, 0);
            // Tier 1 to Tier 2
            map.AddRoute(0, 5);
            map.AddRoute(1, 6);
            map.AddRoute(2, 7);
            map.AddRoute(3, 8);
            map.AddRoute(4, 9);
            map.AddRoute(0, 11);
            map.AddRoute(2, 12);
            // Tier 2 internal
            map.AddRoute(5, 6);
            map.AddRoute(7, 8);
            map.AddRoute(9, 10);
            map.AddRoute(10, 11);
            map.AddRoute(11, 12);
            // Tier 2 to Tier 3
            map.AddRoute(5, 13);
            map.AddRoute(6, 14);
            map.AddRoute(8, 15);
            map.AddRoute(9, 16);
            map.AddRoute(10, 17);
            // Tier 3 internal
            map.AddRoute(13, 14);
            map.AddRoute(15, 16);
            map.AddRoute(14, 15);
            map.AddRoute(13, 17);
            map.AddRoute(16, 17);

            return map;
        }
    }
}
