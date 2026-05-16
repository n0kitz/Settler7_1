using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Static database of all prestige unlocks organized in 3 branches.
    /// Branch 1: Economy (building upgrades, storehouse, roads)
    /// Branch 2: Military (stronghold, unit upgrades, fortifications)
    /// Branch 3: Culture (church, trade, prestige objects)
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class PrestigeDatabase
    {
        public enum PrestigeBranch { Economy, Military, Culture }

        public class PrestigeUnlockDef
        {
            public string Id;
            public string DisplayName;
            public string Description;
            public PrestigeBranch Branch;
            public int MinLevel;
            public string PrerequisiteId; // null = no prerequisite

            public PrestigeUnlockDef(string id, string name, string desc,
                PrestigeBranch branch, int minLevel, string prereq = null)
            {
                Id = id;
                DisplayName = name;
                Description = desc;
                Branch = branch;
                MinLevel = minLevel;
                PrerequisiteId = prereq;
            }
        }

        private static List<PrestigeUnlockDef> _all;
        private static Dictionary<string, PrestigeUnlockDef> _byId;

        public static IReadOnlyList<PrestigeUnlockDef> All
        {
            get
            {
                if (_all == null) Build();
                return _all;
            }
        }

        public static PrestigeUnlockDef Get(string id)
        {
            if (_byId == null) Build();
            return _byId.TryGetValue(id, out var def) ? def : null;
        }

        public static List<PrestigeUnlockDef> GetBranch(PrestigeBranch branch)
        {
            var result = new List<PrestigeUnlockDef>();
            foreach (var def in All)
            {
                if (def.Branch == branch)
                    result.Add(def);
            }
            return result;
        }

        private static void Build()
        {
            _all = new List<PrestigeUnlockDef>();
            _byId = new Dictionary<string, PrestigeUnlockDef>();

            // --- ECONOMY BRANCH ---
            Add("eco_residence_upgrade", "Residence Upgrade", "Unlock Residence upgrades (+4 pop each level)",
                PrestigeBranch.Economy, 1);
            Add("eco_storehouse_lv2", "Storehouse Level 2", "Upgrade storehouses to level 2 (+1 carrier)",
                PrestigeBranch.Economy, 1);
            Add("eco_paved_roads", "Paved Roads", "Build paved roads (faster carrier movement, costs stone)",
                PrestigeBranch.Economy, 2, "eco_storehouse_lv2");
            Add("eco_noble_residence", "Noble Residence Unlock", "Unlock Noble Residence building type",
                PrestigeBranch.Economy, 2, "eco_residence_upgrade");
            Add("eco_noble_upgrade", "Noble Upgrade", "Unlock Noble Residence upgrades (+5 pop each level)",
                PrestigeBranch.Economy, 3, "eco_noble_residence");
            Add("eco_storehouse_lv3", "Storehouse Level 3", "Upgrade storehouses to level 3 (+1 carrier)",
                PrestigeBranch.Economy, 3, "eco_paved_roads");
            Add("eco_geologist", "Geologist", "Unlock geologist mode for miners (higher yield)",
                PrestigeBranch.Economy, 4, "eco_noble_upgrade");
            Add("eco_food_master", "Food Master", "Fancy food provides ×4 instead of ×3",
                PrestigeBranch.Economy, 5, "eco_geologist");

            // --- MILITARY BRANCH ---
            Add("mil_stronghold", "Stronghold", "Unlock Stronghold building (military units)",
                PrestigeBranch.Military, 1);
            Add("mil_pikeman", "Pikeman Training", "Train pikemen at the Stronghold",
                PrestigeBranch.Military, 1, "mil_stronghold");
            Add("mil_musketeer", "Musketeer Training", "Train musketeers (required for fortified sectors)",
                PrestigeBranch.Military, 2, "mil_pikeman");
            Add("mil_cavalier", "Cavalier Training", "Train cavaliers (fast, strong)",
                PrestigeBranch.Military, 3, "mil_musketeer");
            Add("mil_cannon", "Cannon Foundry", "Build cannons (siege, breach fortifications)",
                PrestigeBranch.Military, 4, "mil_cavalier");
            Add("mil_fortification", "Fortification", "Build fortifications in your sectors",
                PrestigeBranch.Military, 1);
            Add("mil_second_general", "Second General", "Hire a second general",
                PrestigeBranch.Military, 3, "mil_fortification");
            Add("mil_standard_bearer", "Standard Bearer", "Train standard bearers (army morale bonus)",
                PrestigeBranch.Military, 5, "mil_cannon");

            // --- CULTURE BRANCH ---
            Add("cul_church", "Church", "Unlock Church building (technology research)",
                PrestigeBranch.Culture, 1);
            Add("cul_export_office", "Export Office", "Unlock Export Office (trade routes)",
                PrestigeBranch.Culture, 1);
            Add("cul_novice", "Novice Training", "Train novices at the Church",
                PrestigeBranch.Culture, 2, "cul_church");
            Add("cul_hawker", "Hawker Training", "Train hawkers at the Export Office",
                PrestigeBranch.Culture, 2, "cul_export_office");
            Add("cul_brother", "Brother Training", "Train brothers (advanced clerics)",
                PrestigeBranch.Culture, 3, "cul_novice");
            Add("cul_salesman", "Salesman Training", "Train salesmen (advanced traders)",
                PrestigeBranch.Culture, 3, "cul_hawker");
            Add("cul_father", "Father Training", "Train fathers (master clerics)",
                PrestigeBranch.Culture, 4, "cul_brother");
            Add("cul_merchant", "Merchant Training", "Train merchants (master traders)",
                PrestigeBranch.Culture, 4, "cul_salesman");
        }

        private static void Add(string id, string name, string desc,
            PrestigeBranch branch, int minLevel, string prereq = null)
        {
            var def = new PrestigeUnlockDef(id, name, desc, branch, minLevel, prereq);
            _all.Add(def);
            _byId[id] = def;
        }
    }
}
