using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Shared technology tree. 18 technologies in 3 tiers.
    /// Technologies are first-come-first-served: once researched by one player,
    /// permanently locked for all others.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class TechTree
    {
        public enum TechTier { Tier1, Tier2, Tier3 }

        public class TechDef
        {
            public string Id;
            public string DisplayName;
            public string Description;
            public TechTier Tier;
            public string PrerequisiteId; // null = no prereq
            public float ResearchTime;    // seconds

            // §14.6 research cost: clerics assigned for the duration
            public int CostNovices;   // Geistliche
            public int CostBrothers;  // Mönche
            public int CostFathers;   // Prälaten

            public TechDef(string id, string name, string desc,
                TechTier tier, float researchTime, string prereq = null)
            {
                Id = id;
                DisplayName = name;
                Description = desc;
                Tier = tier;
                ResearchTime = researchTime;
                PrerequisiteId = prereq;

                // Cost scales with tier (§14.6 pattern: 3/0/0 → 4/2/0 → 5/2/1)
                switch (tier)
                {
                    case TechTier.Tier1:
                        CostNovices = 3; CostBrothers = 0; CostFathers = 0; break;
                    case TechTier.Tier2:
                        CostNovices = 4; CostBrothers = 2; CostFathers = 0; break;
                    default:
                        CostNovices = 5; CostBrothers = 2; CostFathers = 1; break;
                }
            }
        }

        private static List<TechDef> _all;
        private static Dictionary<string, TechDef> _byId;

        public static IReadOnlyList<TechDef> All
        {
            get
            {
                if (_all == null) Build();
                return _all;
            }
        }

        public static TechDef Get(string id)
        {
            if (_byId == null) Build();
            return _byId.TryGetValue(id, out var def) ? def : null;
        }

        public static List<TechDef> GetTier(TechTier tier)
        {
            var result = new List<TechDef>();
            foreach (var def in All)
                if (def.Tier == tier) result.Add(def);
            return result;
        }

        private static void Build()
        {
            _all = new List<TechDef>();
            _byId = new Dictionary<string, TechDef>();

            // --- TIER 1 (6 techs) ---
            Add("tech_plowing", "Plowing", "Grain barns produce +50% grain",
                TechTier.Tier1, 30f);
            Add("tech_masonry", "Masonry", "Stone quarries produce +50% stone",
                TechTier.Tier1, 30f);
            Add("tech_carpentry", "Carpentry", "Sawmills produce +50% planks",
                TechTier.Tier1, 30f);
            Add("tech_animal_husbandry", "Animal Husbandry", "Piggeries + shepherds produce +50%",
                TechTier.Tier1, 30f);
            Add("tech_smelting", "Smelting", "Iron smelters produce +50%",
                TechTier.Tier1, 30f);
            Add("tech_fishing", "Fishing Nets", "Fishers produce +50%",
                TechTier.Tier1, 30f);

            // --- TIER 2 (6 techs, each requires a Tier 1) ---
            Add("tech_crop_rotation", "Crop Rotation", "Grain barns produce ×2",
                TechTier.Tier2, 45f, "tech_plowing");
            Add("tech_fortification_tech", "Fortification Engineering", "Build fortifications 50% faster",
                TechTier.Tier2, 45f, "tech_masonry");
            Add("tech_woodworking", "Woodworking", "Wheelwrights produce +50%",
                TechTier.Tier2, 45f, "tech_carpentry");
            Add("tech_breeding", "Selective Breeding", "Stables produce horses +50%",
                TechTier.Tier2, 45f, "tech_animal_husbandry");
            Add("tech_steel", "Steel Working", "Weapons production +50%",
                TechTier.Tier2, 45f, "tech_smelting");
            Add("tech_preservation", "Food Preservation", "Fancy food multiplier +1",
                TechTier.Tier2, 45f, "tech_fishing");

            // --- TIER 3 (6 techs, each requires a Tier 2) ---
            Add("tech_irrigation", "Irrigation", "All farms produce ×2",
                TechTier.Tier3, 60f, "tech_crop_rotation");
            Add("tech_architecture", "Architecture", "Construction time -50%",
                TechTier.Tier3, 60f, "tech_fortification_tech");
            Add("tech_logistics", "Advanced Logistics", "Carriers move 50% faster",
                TechTier.Tier3, 60f, "tech_woodworking");
            Add("tech_cavalry", "Cavalry Tactics", "Cavaliers +30% attack",
                TechTier.Tier3, 60f, "tech_breeding");
            Add("tech_metallurgy", "Metallurgy", "All metal production ×2",
                TechTier.Tier3, 60f, "tech_steel");
            Add("tech_hygiene", "Hygiene", "Residence +2 pop, Noble +4 pop",
                TechTier.Tier3, 60f, "tech_preservation");
        }

        private static void Add(string id, string name, string desc,
            TechTier tier, float researchTime, string prereq = null)
        {
            var def = new TechDef(id, name, desc, tier, researchTime, prereq);
            _all.Add(def);
            _byId[id] = def;
        }
    }
}
