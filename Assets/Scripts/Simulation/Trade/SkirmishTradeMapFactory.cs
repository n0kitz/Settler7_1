namespace Settlers.Simulation
{
    /// <summary>
    /// Trade networks for the Sprint-7a skirmish maps. These are also the
    /// §14.9 source for the two trade-only luxuries: Spice and Wine have no
    /// production recipe — they arrive exclusively through these outposts.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class SkirmishTradeMapFactory
    {
        /// <summary>Highland Duel — 10 outposts, two-branch loch/ridge theme.</summary>
        public static TradeMap CreateHighlandDuelTradeMap()
        {
            var map = new TradeMap();

            map.AddOutpost(new TradeOutpost("hd_fish_bread",
                "Loch Fishery", ResourceType.Fish, 2, ResourceType.Bread, 1));
            map.AddOutpost(new TradeOutpost("hd_wool_cloth",
                "Crofters' Exchange", ResourceType.Wool, 2, ResourceType.Cloth, 1));
            map.AddOutpost(new TradeOutpost("hd_stone_tools",
                "Mason's Exchange", ResourceType.Stone, 3, ResourceType.Tools, 1));
            map.AddOutpost(new TradeOutpost("hd_planks_iron",
                "Smelter Exchange", ResourceType.Planks, 3, ResourceType.IronBars, 1));
            map.AddOutpost(new TradeOutpost("hd_fur_coins",
                "Fur Post", ResourceType.Fur, 2, ResourceType.Coins, 3));
            map.AddOutpost(new TradeOutpost("hd_leather_jewelry",
                "Leatherworks", ResourceType.Leather, 3, ResourceType.Jewelry, 1));
            map.AddOutpost(new TradeOutpost("hd_bread_spice",
                "Spice Caravan", ResourceType.Bread, 3, ResourceType.Spice, 2));
            map.AddOutpost(new TradeOutpost("hd_coins_wine",
                "Wine Cellar", ResourceType.Coins, 4, ResourceType.Wine, 2));
            map.AddOutpost(new TradeOutpost("hd_special_gold",
                "Highland Treasury", ResourceType.Jewelry, 2, ResourceType.Coins, 10,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("hd_special_relic",
                "Standing Stone Shrine", ResourceType.Books, 3, ResourceType.Jewelry, 2,
                isSpecial: true));

            for (int i = 0; i < 7; i++) map.AddRoute(i, i + 1);
            map.AddRoute(0, 3); map.AddRoute(2, 5);
            map.AddRoute(6, 8); map.AddRoute(7, 9);
            return map;
        }

        /// <summary>Golden Meadows — 12 outposts, market-fair theme.</summary>
        public static TradeMap CreateGoldenMeadowsTradeMap()
        {
            var map = new TradeMap();

            map.AddOutpost(new TradeOutpost("gm_grain_bread",
                "Fair Bakery", ResourceType.Grain, 2, ResourceType.Bread, 1));
            map.AddOutpost(new TradeOutpost("gm_wool_cloth",
                "Fair Loom", ResourceType.Wool, 2, ResourceType.Cloth, 1));
            map.AddOutpost(new TradeOutpost("gm_wood_tools",
                "Fair Workshop", ResourceType.Wood, 4, ResourceType.Tools, 1));
            map.AddOutpost(new TradeOutpost("gm_meat_coins",
                "Butchers' Row", ResourceType.Meat, 2, ResourceType.Coins, 4));
            map.AddOutpost(new TradeOutpost("gm_iron_weapons",
                "Armory Row", ResourceType.IronBars, 2, ResourceType.Weapons, 1));
            map.AddOutpost(new TradeOutpost("gm_gold_coins",
                "Fair Mint", ResourceType.GoldOre, 1, ResourceType.Coins, 3));
            map.AddOutpost(new TradeOutpost("gm_cloth_garments",
                "Tailors' Row", ResourceType.Cloth, 2, ResourceType.Garments, 1));
            map.AddOutpost(new TradeOutpost("gm_beer_coins",
                "Fair Tavern", ResourceType.Beer, 3, ResourceType.Coins, 5));
            map.AddOutpost(new TradeOutpost("gm_coins_spice",
                "Spice Merchants", ResourceType.Coins, 5, ResourceType.Spice, 2));
            map.AddOutpost(new TradeOutpost("gm_grain_wine",
                "Vineyard Cart", ResourceType.Grain, 4, ResourceType.Wine, 2));
            map.AddOutpost(new TradeOutpost("gm_special_crown",
                "Golden Pavilion", ResourceType.Jewelry, 2, ResourceType.Coins, 12,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("gm_special_relics",
                "Monastery Vault", ResourceType.Books, 3, ResourceType.Jewelry, 2,
                isSpecial: true));

            for (int i = 0; i < 9; i++) map.AddRoute(i, i + 1);
            map.AddRoute(0, 4); map.AddRoute(2, 6); map.AddRoute(5, 9);
            map.AddRoute(8, 10); map.AddRoute(9, 11);
            return map;
        }

        /// <summary>The Frontier — 14 outposts, expedition theme.</summary>
        public static TradeMap CreateTheFrontierTradeMap()
        {
            var map = new TradeMap();

            map.AddOutpost(new TradeOutpost("tf_planks_stone",
                "Border Quarry", ResourceType.Planks, 3, ResourceType.Stone, 2));
            map.AddOutpost(new TradeOutpost("tf_fish_bread",
                "River Post", ResourceType.Fish, 2, ResourceType.Bread, 1));
            map.AddOutpost(new TradeOutpost("tf_wood_tools",
                "Wagon Camp", ResourceType.Wood, 4, ResourceType.Tools, 1));
            map.AddOutpost(new TradeOutpost("tf_fur_coins",
                "Trappers' Post", ResourceType.Fur, 2, ResourceType.Coins, 3));
            map.AddOutpost(new TradeOutpost("tf_meat_coins",
                "Provision Depot", ResourceType.Meat, 2, ResourceType.Coins, 4));
            map.AddOutpost(new TradeOutpost("tf_iron_weapons",
                "Frontier Armory", ResourceType.IronBars, 2, ResourceType.Weapons, 1));
            map.AddOutpost(new TradeOutpost("tf_gold_coins",
                "Prospectors' Mint", ResourceType.GoldOre, 1, ResourceType.Coins, 3));
            map.AddOutpost(new TradeOutpost("tf_leather_garments",
                "Tannery Post", ResourceType.Leather, 2, ResourceType.Garments, 1));
            map.AddOutpost(new TradeOutpost("tf_coins_horses",
                "Horse Traders", ResourceType.Coins, 4, ResourceType.Horses, 1));
            map.AddOutpost(new TradeOutpost("tf_coins_spice",
                "Far-East Caravan", ResourceType.Coins, 5, ResourceType.Spice, 2));
            map.AddOutpost(new TradeOutpost("tf_coins_wine",
                "Old-World Cellars", ResourceType.Coins, 5, ResourceType.Wine, 2));
            map.AddOutpost(new TradeOutpost("tf_special_expedition",
                "Lost Expedition", ResourceType.Spice, 3, ResourceType.Jewelry, 2,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("tf_special_cathedral",
                "Cathedral Reliquary", ResourceType.Books, 3, ResourceType.Coins, 12,
                isSpecial: true));
            map.AddOutpost(new TradeOutpost("tf_special_citadel",
                "Citadel Commission", ResourceType.Weapons, 4, ResourceType.Coins, 10,
                isSpecial: true));

            for (int i = 0; i < 11; i++) map.AddRoute(i, i + 1);
            map.AddRoute(0, 4); map.AddRoute(2, 6); map.AddRoute(5, 9);
            map.AddRoute(9, 11); map.AddRoute(10, 12); map.AddRoute(8, 13);
            return map;
        }
    }
}
