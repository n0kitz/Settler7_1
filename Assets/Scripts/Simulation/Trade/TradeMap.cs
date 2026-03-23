using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Trade outpost network. Outposts are graph nodes connected by trade routes.
    /// Players claim outposts by sending traders — first-come-first-served.
    /// Each outpost offers a specific exchange (e.g. 3 Planks → 1 Iron Bars).
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class TradeMap
    {
        private readonly List<TradeOutpost> _outposts = new();
        private readonly Dictionary<string, TradeOutpost> _byId = new();
        private readonly Dictionary<int, List<int>> _adjacency = new(); // outpost index → neighbors

        /// <summary>All trade outposts.</summary>
        public IReadOnlyList<TradeOutpost> AllOutposts => _outposts;

        /// <summary>Add a trade outpost to the map.</summary>
        public void AddOutpost(TradeOutpost outpost)
        {
            int idx = _outposts.Count;
            _outposts.Add(outpost);
            _byId[outpost.Id] = outpost;
            _adjacency[idx] = new List<int>();
        }

        /// <summary>Connect two outposts with a trade route.</summary>
        public void AddRoute(int outpostA, int outpostB)
        {
            if (!_adjacency[outpostA].Contains(outpostB))
                _adjacency[outpostA].Add(outpostB);
            if (!_adjacency[outpostB].Contains(outpostA))
                _adjacency[outpostB].Add(outpostA);
        }

        /// <summary>Get an outpost by ID.</summary>
        public TradeOutpost GetOutpost(string id) =>
            _byId.TryGetValue(id, out var op) ? op : null;

        /// <summary>Get outpost by index.</summary>
        public TradeOutpost GetOutpost(int index) =>
            index >= 0 && index < _outposts.Count ? _outposts[index] : null;

        /// <summary>Get outposts adjacent to this one.</summary>
        public IReadOnlyList<int> GetRoutes(int outpostIndex) =>
            _adjacency.TryGetValue(outpostIndex, out var list)
                ? list
                : (IReadOnlyList<int>)System.Array.Empty<int>();

        /// <summary>Get number of outposts claimed by a player.</summary>
        public int GetClaimedCount(int playerId)
        {
            int count = 0;
            foreach (var op in _outposts)
                if (op.ClaimedBy == playerId) count++;
            return count;
        }

        /// <summary>Get all outposts claimed by a player.</summary>
        public List<TradeOutpost> GetClaimedOutposts(int playerId)
        {
            var result = new List<TradeOutpost>();
            foreach (var op in _outposts)
                if (op.ClaimedBy == playerId) result.Add(op);
            return result;
        }
    }

    /// <summary>
    /// A trade outpost offering a resource exchange.
    /// First-come-first-served: once claimed, exclusive.
    /// </summary>
    public class TradeOutpost
    {
        public string Id { get; }
        public string DisplayName { get; }

        /// <summary>What the player gives to trade.</summary>
        public ResourceType InputResource { get; }
        public int InputAmount { get; }

        /// <summary>What the player receives.</summary>
        public ResourceType OutputResource { get; }
        public int OutputAmount { get; }

        /// <summary>Player who claimed this outpost (-1 = unclaimed).</summary>
        public int ClaimedBy { get; private set; }

        /// <summary>Is this a special outpost (grants permanent VP)?</summary>
        public bool IsSpecial { get; }

        public TradeOutpost(string id, string name,
            ResourceType inputRes, int inputAmt,
            ResourceType outputRes, int outputAmt,
            bool isSpecial = false)
        {
            Id = id;
            DisplayName = name;
            InputResource = inputRes;
            InputAmount = inputAmt;
            OutputResource = outputRes;
            OutputAmount = outputAmt;
            ClaimedBy = -1;
            IsSpecial = isSpecial;
        }

        public bool IsClaimed => ClaimedBy >= 0;

        public bool TryClaim(int playerId)
        {
            if (IsClaimed) return false;
            ClaimedBy = playerId;
            return true;
        }
    }

    /// <summary>
    /// Factory for creating trade maps. Returns map-specific trade networks.
    /// </summary>
    public static class TestTradeMapFactory
    {
        /// <summary>Create a trade map for the given map ID.</summary>
        public static TradeMap CreateForMap(string mapId)
        {
            return mapId switch
            {
                "large_valley" => CreateLargeValleyTradeMap(),
                _ => CreateTestTradeMap()
            };
        }

        public static TradeMap CreateTestTradeMap()
        {
            var map = new TradeMap();

            // 8 trade outposts with various exchanges
            map.AddOutpost(new TradeOutpost("trade_planks_iron",
                "Ironworks Exchange", ResourceType.Planks, 3, ResourceType.IronBars, 1));
            map.AddOutpost(new TradeOutpost("trade_grain_bread",
                "Baker's Exchange", ResourceType.Grain, 2, ResourceType.Bread, 1));
            map.AddOutpost(new TradeOutpost("trade_wool_cloth",
                "Weaver's Exchange", ResourceType.Wool, 2, ResourceType.Cloth, 1));
            map.AddOutpost(new TradeOutpost("trade_stone_tools",
                "Toolsmith Exchange", ResourceType.Stone, 3, ResourceType.Tools, 1));
            map.AddOutpost(new TradeOutpost("trade_gold_coins",
                "Mint Exchange", ResourceType.GoldOre, 1, ResourceType.Coins, 3));
            map.AddOutpost(new TradeOutpost("trade_iron_weapons",
                "Armory Exchange", ResourceType.IronBars, 2, ResourceType.Weapons, 1));
            map.AddOutpost(new TradeOutpost("trade_special_silk",
                "Silk Road", ResourceType.Cloth, 3, ResourceType.Jewelry, 2, isSpecial: true));
            map.AddOutpost(new TradeOutpost("trade_special_spice",
                "Spice Route", ResourceType.Coins, 5, ResourceType.Books, 2, isSpecial: true));

            // Connect them in a network
            map.AddRoute(0, 1);
            map.AddRoute(1, 2);
            map.AddRoute(2, 3);
            map.AddRoute(3, 4);
            map.AddRoute(4, 5);
            map.AddRoute(5, 6);
            map.AddRoute(6, 7);
            map.AddRoute(0, 3);
            map.AddRoute(1, 4);
            map.AddRoute(2, 5);

            return map;
        }

        /// <summary>Trade map for the large_valley 12-sector map — 12 outposts.</summary>
        public static TradeMap CreateLargeValleyTradeMap()
        {
            var map = new TradeMap();

            // Basic resource exchanges
            map.AddOutpost(new TradeOutpost("lv_planks_stone",
                "Mason's Exchange", ResourceType.Planks, 3, ResourceType.Stone, 2));
            map.AddOutpost(new TradeOutpost("lv_wood_tools",
                "Carpenter's Exchange", ResourceType.Wood, 4, ResourceType.Tools, 1));
            map.AddOutpost(new TradeOutpost("lv_grain_bread",
                "Baker's Exchange", ResourceType.Grain, 2, ResourceType.Bread, 1));
            map.AddOutpost(new TradeOutpost("lv_wool_cloth",
                "Weaver's Exchange", ResourceType.Wool, 2, ResourceType.Cloth, 1));
            map.AddOutpost(new TradeOutpost("lv_iron_weapons",
                "Armory Exchange", ResourceType.IronBars, 2, ResourceType.Weapons, 1));
            map.AddOutpost(new TradeOutpost("lv_gold_coins",
                "Mint Exchange", ResourceType.GoldOre, 1, ResourceType.Coins, 3));

            // Advanced exchanges
            map.AddOutpost(new TradeOutpost("lv_cloth_garments",
                "Tailor's Exchange", ResourceType.Cloth, 2, ResourceType.Garments, 1));
            map.AddOutpost(new TradeOutpost("lv_paper_books",
                "Scribe's Exchange", ResourceType.Paper, 2, ResourceType.Books, 1));
            map.AddOutpost(new TradeOutpost("lv_coins_horses",
                "Stable Exchange", ResourceType.Coins, 4, ResourceType.Horses, 1));
            map.AddOutpost(new TradeOutpost("lv_beer_coins",
                "Tavern Exchange", ResourceType.Beer, 3, ResourceType.Coins, 5));

            // Special outposts (grant permanent VP)
            map.AddOutpost(new TradeOutpost("lv_special_exotic",
                "Exotic Goods Route", ResourceType.Jewelry, 2, ResourceType.Coins, 10, isSpecial: true));
            map.AddOutpost(new TradeOutpost("lv_special_relics",
                "Relic Exchange", ResourceType.Books, 3, ResourceType.Jewelry, 2, isSpecial: true));

            // Network connections — more branching than small map
            map.AddRoute(0, 1);
            map.AddRoute(1, 2);
            map.AddRoute(2, 3);
            map.AddRoute(3, 4);
            map.AddRoute(4, 5);
            map.AddRoute(5, 6);
            map.AddRoute(6, 7);
            map.AddRoute(7, 8);
            map.AddRoute(8, 9);
            map.AddRoute(9, 10);
            map.AddRoute(10, 11);
            // Cross-links for multiple paths
            map.AddRoute(0, 3);
            map.AddRoute(1, 5);
            map.AddRoute(2, 6);
            map.AddRoute(4, 8);
            map.AddRoute(7, 10);

            return map;
        }
    }
}
