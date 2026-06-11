namespace Settlers.Simulation
{
    /// <summary>
    /// Localized display names for simulation enums.
    /// Key convention: "ui.res." + lowercase enum name (e.g. ui.res.ironbars).
    /// Falls back to the enum name when a key is missing.
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
    }
}
