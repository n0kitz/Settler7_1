using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Static database of all production recipes and work yard → building type mappings.
    /// This is the authoritative source — AssetGenerator reads from here to create SOs.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class RecipeDatabase
    {
        /// <summary>
        /// A recipe definition: what a work yard produces.
        /// </summary>
        public class RecipeDef
        {
            public string WorkYardId;
            public string DisplayName;
            public BaseBuildingType ParentBuilding;
            public ResourceNodeType RequiredNode;
            public (ResourceType type, int amount)[] Inputs;
            public (ResourceType type, int amount)[] Outputs;
            public float CycleDuration;

            public RecipeDef(string id, string name, BaseBuildingType parent,
                ResourceNodeType node, (ResourceType, int)[] inputs,
                (ResourceType, int)[] outputs, float duration)
            {
                WorkYardId = id;
                DisplayName = name;
                ParentBuilding = parent;
                RequiredNode = node;
                Inputs = inputs;
                Outputs = outputs;
                CycleDuration = duration;
            }
        }

        private static List<RecipeDef> _recipes;

        /// <summary>All recipes in the game.</summary>
        public static IReadOnlyList<RecipeDef> All
        {
            get
            {
                if (_recipes == null)
                    BuildDatabase();
                return _recipes;
            }
        }

        private static Dictionary<string, RecipeDef> _byId;

        /// <summary>Look up a recipe by work yard ID.</summary>
        public static RecipeDef Get(string workYardId)
        {
            if (_byId == null)
                BuildDatabase();
            return _byId.TryGetValue(workYardId, out var r) ? r : null;
        }

        /// <summary>Get all work yard IDs for a building type.</summary>
        public static List<RecipeDef> GetForBuilding(BaseBuildingType type)
        {
            var result = new List<RecipeDef>();
            foreach (var r in All)
            {
                if (r.ParentBuilding == type)
                    result.Add(r);
            }
            return result;
        }

        private static void BuildDatabase()
        {
            _recipes = new List<RecipeDef>();
            _byId = new Dictionary<string, RecipeDef>();

            // ---- LODGE (3 Planks) ----
            Add("forester", "Forester", BaseBuildingType.Lodge,
                ResourceNodeType.Forest,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Wood, 1) }, 8f);

            Add("woodcutter", "Woodcutter", BaseBuildingType.Lodge,
                ResourceNodeType.Forest,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Wood, 1) }, 6f);

            Add("sawmill", "Sawmill", BaseBuildingType.Lodge,
                ResourceNodeType.None,
                new[] { (ResourceType.Wood, 1) },
                new[] { (ResourceType.Planks, 2) }, 8f);

            Add("fisher", "Fisher", BaseBuildingType.Lodge,
                ResourceNodeType.FishingGround,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Fish, 1) }, 10f);

            Add("hunter", "Hunter", BaseBuildingType.Lodge,
                ResourceNodeType.Forest,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Animal, 1) }, 12f);

            // ---- FARM (3 Planks) ----
            Add("well", "Well", BaseBuildingType.Farm,
                ResourceNodeType.WaterSource,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Water, 1) }, 6f);

            Add("grain_barn", "Grain Barn", BaseBuildingType.Farm,
                ResourceNodeType.FertileLand,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Grain, 1) }, 10f);

            Add("windmill", "Windmill", BaseBuildingType.Farm,
                ResourceNodeType.None,
                new[] { (ResourceType.Grain, 1) },
                new[] { (ResourceType.Flour, 1) }, 6f);

            Add("piggery", "Piggery", BaseBuildingType.Farm,
                ResourceNodeType.None,
                new[] { (ResourceType.Grain, 1) },
                new[] { (ResourceType.Animal, 1) }, 12f);

            Add("shepherd", "Shepherd", BaseBuildingType.Farm,
                ResourceNodeType.FertileLand,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Wool, 1) }, 10f);

            Add("stable", "Stable", BaseBuildingType.Farm,
                ResourceNodeType.FertileLand,
                new[] { (ResourceType.Grain, 1) },
                new[] { (ResourceType.Horses, 1) }, 15f);

            // ---- MOUNTAIN SHELTER (2P+1S) ----
            Add("quarry", "Quarry", BaseBuildingType.MountainShelter,
                ResourceNodeType.Stone,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Stone, 1) }, 8f);

            Add("coal_miner", "Coal Miner", BaseBuildingType.MountainShelter,
                ResourceNodeType.Coal,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.Coal, 1) }, 8f);

            Add("iron_miner", "Iron Miner", BaseBuildingType.MountainShelter,
                ResourceNodeType.Iron,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.IronOre, 1) }, 8f);

            Add("gold_miner", "Gold Miner", BaseBuildingType.MountainShelter,
                ResourceNodeType.Gold,
                new (ResourceType, int)[] { },
                new[] { (ResourceType.GoldOre, 1) }, 10f);

            Add("iron_smelter", "Iron Smelter", BaseBuildingType.MountainShelter,
                ResourceNodeType.None,
                new[] { (ResourceType.IronOre, 1), (ResourceType.Coal, 1) },
                new[] { (ResourceType.IronBars, 1) }, 8f);

            Add("coking_plant", "Coking Plant", BaseBuildingType.MountainShelter,
                ResourceNodeType.None,
                new[] { (ResourceType.Wood, 1) },
                new[] { (ResourceType.Coal, 1) }, 8f);

            Add("charcoal_kiln", "Charcoal Kiln", BaseBuildingType.MountainShelter,
                ResourceNodeType.Forest,
                new[] { (ResourceType.Wood, 1) },
                new[] { (ResourceType.Coal, 1) }, 12f);

            // ---- RESIDENCE (2P+1S, 4 pop) ----
            Add("bakery", "Bakery", BaseBuildingType.Residence,
                ResourceNodeType.None,
                new[] { (ResourceType.Flour, 1), (ResourceType.Water, 1) },
                new[] { (ResourceType.Bread, 1) }, 8f);

            Add("brewery", "Brewery", BaseBuildingType.Residence,
                ResourceNodeType.None,
                new[] { (ResourceType.Grain, 1), (ResourceType.Water, 1) },
                new[] { (ResourceType.Beer, 1) }, 10f);

            Add("paper_mill", "Paper Mill", BaseBuildingType.Residence,
                ResourceNodeType.None,
                new[] { (ResourceType.Wood, 1), (ResourceType.Water, 1) },
                new[] { (ResourceType.Paper, 1) }, 8f);

            Add("weaving_mill", "Weaving Mill", BaseBuildingType.Residence,
                ResourceNodeType.None,
                new[] { (ResourceType.Wool, 1) },
                new[] { (ResourceType.Cloth, 1) }, 8f);

            Add("wheelwright", "Wheelwright", BaseBuildingType.Residence,
                ResourceNodeType.None,
                new[] { (ResourceType.Planks, 1), (ResourceType.IronBars, 1) },
                new[] { (ResourceType.Wheels, 1) }, 10f);

            Add("toolmaker", "Toolmaker", BaseBuildingType.Residence,
                ResourceNodeType.None,
                new[] { (ResourceType.IronBars, 1), (ResourceType.Planks, 1) },
                new[] { (ResourceType.Tools, 1) }, 8f);

            // ---- NOBLE RESIDENCE (3P+2S, 5 pop) ----
            Add("butcher", "Butcher", BaseBuildingType.NobleResidence,
                ResourceNodeType.None,
                new[] { (ResourceType.Animal, 1) },
                new[] { (ResourceType.Sausages, 1) }, 8f);

            Add("blacksmith", "Blacksmith", BaseBuildingType.NobleResidence,
                ResourceNodeType.None,
                new[] { (ResourceType.IronBars, 1), (ResourceType.Coal, 1) },
                new[] { (ResourceType.Weapons, 1) }, 10f);

            Add("mint", "Mint", BaseBuildingType.NobleResidence,
                ResourceNodeType.None,
                new[] { (ResourceType.GoldOre, 1), (ResourceType.Coal, 1) },
                new[] { (ResourceType.Coins, 1) }, 8f);

            Add("goldsmith", "Goldsmith", BaseBuildingType.NobleResidence,
                ResourceNodeType.None,
                new[] { (ResourceType.GoldOre, 1) },
                new[] { (ResourceType.Jewelry, 1) }, 10f);

            Add("bookbinder", "Bookbinder", BaseBuildingType.NobleResidence,
                ResourceNodeType.None,
                new[] { (ResourceType.Paper, 1) },
                new[] { (ResourceType.Books, 1) }, 8f);

            Add("tailor", "Tailor", BaseBuildingType.NobleResidence,
                ResourceNodeType.None,
                new[] { (ResourceType.Cloth, 1) },
                new[] { (ResourceType.Garments, 1) }, 8f);
        }

        private static void Add(string id, string name, BaseBuildingType parent,
            ResourceNodeType node, (ResourceType, int)[] inputs,
            (ResourceType, int)[] outputs, float duration)
        {
            var def = new RecipeDef(id, name, parent, node, inputs, outputs, duration);
            _recipes.Add(def);
            _byId[id] = def;
        }
    }
}
