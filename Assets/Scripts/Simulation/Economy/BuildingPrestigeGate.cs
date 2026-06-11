namespace Settlers.Simulation
{
    /// <summary>
    /// Maps building types to the prestige unlock required to place them.
    /// Single source of truth for build gating — used by the build menu
    /// (gray silhouettes) and placement validation.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class BuildingPrestigeGate
    {
        /// <summary>
        /// The prestige unlock id required to build this type,
        /// or null if it is available from the start.
        /// </summary>
        public static string RequiredUnlock(BaseBuildingType type) => type switch
        {
            BaseBuildingType.NobleResidence => "eco_noble_residence",
            _ => null
        };

        /// <summary>True if the player may place this building type.</summary>
        public static bool IsUnlocked(PrestigeSystem prestige, int playerId,
            BaseBuildingType type)
        {
            string required = RequiredUnlock(type);
            return required == null || prestige.HasUnlock(playerId, required);
        }
    }
}
