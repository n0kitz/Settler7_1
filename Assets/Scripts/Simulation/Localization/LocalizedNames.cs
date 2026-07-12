namespace Settlers.Simulation
{
    /// <summary>
    /// Localized display names for simulation enums and static databases.
    /// Key conventions: "ui.res." + enum, "ui.recipe." + work yard id,
    /// "ui.outpost." + outpost id, "ui.techname."/"ui.techdesc." + tech id,
    /// "ui.prestige.name."/"ui.prestige.desc." + unlock id.
    /// Falls back to the English DisplayName (the stable simulation identifier)
    /// when a key is missing — the simulation layer itself is never localized.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class LocalizedNames
    {
        /// <summary>Localized name of a resource type.</summary>
        public static string Resource(ResourceType type)
        {
            string key = ResourceKey(type);
            return L.Has(key) ? L.Get(key) : type.ToString();
        }

        /// <summary>The string-table key for a resource type.</summary>
        public static string ResourceKey(ResourceType type) =>
            "ui.res." + type.ToString().ToLowerInvariant();

        /// <summary>Localized name of a work yard recipe (e.g. "bakery" → "Bäckerei").</summary>
        public static string Recipe(string workYardId)
        {
            string key = RecipeKey(workYardId);
            if (L.Has(key)) return L.Get(key);
            return RecipeDatabase.Get(workYardId)?.DisplayName ?? workYardId;
        }

        /// <summary>The string-table key for a work yard recipe.</summary>
        public static string RecipeKey(string workYardId) => "ui.recipe." + workYardId;

        /// <summary>Localized name of a trade outpost.</summary>
        public static string Outpost(TradeOutpost outpost)
        {
            if (outpost == null) return "";
            string key = OutpostKey(outpost.Id);
            return L.Has(key) ? L.Get(key) : outpost.DisplayName;
        }

        /// <summary>The string-table key for a trade outpost.</summary>
        public static string OutpostKey(string outpostId) => "ui.outpost." + outpostId;

        /// <summary>Localized name of a technology.</summary>
        public static string Tech(string techId)
        {
            string key = TechNameKey(techId);
            if (L.Has(key)) return L.Get(key);
            return TechTree.Get(techId)?.DisplayName ?? techId;
        }

        /// <summary>Localized effect description of a technology.</summary>
        public static string TechDescription(string techId)
        {
            string key = TechDescKey(techId);
            if (L.Has(key)) return L.Get(key);
            return TechTree.Get(techId)?.Description ?? "";
        }

        /// <summary>The string-table key for a technology name.</summary>
        public static string TechNameKey(string techId) => "ui.techname." + techId;

        /// <summary>The string-table key for a technology description.</summary>
        public static string TechDescKey(string techId) => "ui.techdesc." + techId;

        /// <summary>Localized name of a prestige unlock.</summary>
        public static string Prestige(string unlockId)
        {
            if (unlockId == null) return "";
            string key = PrestigeNameKey(unlockId);
            if (L.Has(key)) return L.Get(key);
            return PrestigeDatabase.Get(unlockId)?.DisplayName ?? unlockId;
        }

        /// <summary>Localized description of a prestige unlock.</summary>
        public static string PrestigeDescription(string unlockId)
        {
            string key = PrestigeDescKey(unlockId);
            if (L.Has(key)) return L.Get(key);
            return PrestigeDatabase.Get(unlockId)?.Description ?? "";
        }

        /// <summary>The string-table key for a prestige unlock name.</summary>
        public static string PrestigeNameKey(string unlockId) => "ui.prestige.name." + unlockId;

        /// <summary>The string-table key for a prestige unlock description.</summary>
        public static string PrestigeDescKey(string unlockId) => "ui.prestige.desc." + unlockId;

        /// <summary>Localized mission title.</summary>
        public static string MissionTitle(Mission mission)
        {
            string key = $"ui.mission.{mission.Id}.title";
            return L.Has(key) ? L.Get(key) : mission.Title;
        }

        /// <summary>Localized mission briefing text.</summary>
        public static string MissionBriefing(Mission mission)
        {
            string key = $"ui.mission.{mission.Id}.briefing";
            return L.Has(key) ? L.Get(key) : mission.Briefing;
        }

        /// <summary>Localized description of a mission objective by index.</summary>
        public static string MissionObjective(Mission mission, int index)
        {
            string key = $"ui.mission.{mission.Id}.obj{index}";
            if (L.Has(key)) return L.Get(key);
            return index >= 0 && index < mission.Objectives.Length
                ? mission.Objectives[index].Description : "";
        }
    }
}
