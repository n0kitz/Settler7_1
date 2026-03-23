namespace Settlers.Simulation
{
    /// <summary>
    /// Calculates food boost multipliers for production.
    /// Pure C# — no UnityEngine references.
    ///
    /// Rules:
    /// - Lodge/Farm/MountainShelter/Residence: None=×1, Plain=×2, Fancy=×3 (×4 with eco_food_master)
    /// - Noble Residence: None=IDLE(×0), Plain=×1, Fancy=×2 (×3 with eco_food_master)
    /// - If food is toggled ON but unavailable: production HALTS (returns 0)
    /// </summary>
    public static class FoodBoostCalculator
    {
        /// <summary>
        /// Get the production multiplier for a building type and food setting.
        /// Returns 0 if production is halted (Noble Res with no food, or food toggled but unavailable).
        /// </summary>
        public static int GetMultiplier(BaseBuildingType type, FoodSetting setting,
            bool hasFoodMaster = false)
        {
            if (type == BaseBuildingType.NobleResidence)
            {
                return setting switch
                {
                    FoodSetting.None => 0,    // IDLE — Noble Residence requires food
                    FoodSetting.Plain => 1,   // Normal production
                    FoodSetting.Fancy => hasFoodMaster ? 3 : 2,
                    _ => 0
                };
            }

            return setting switch
            {
                FoodSetting.None => 1,    // Normal
                FoodSetting.Plain => 2,   // Doubled
                FoodSetting.Fancy => hasFoodMaster ? 4 : 3,
                _ => 1
            };
        }

        /// <summary>
        /// Check if production should halt because food is toggled on but unavailable.
        /// </summary>
        public static bool ShouldHalt(BaseBuildingType type, FoodSetting setting,
            bool hasFoodAvailable)
        {
            // No food selected — only halts Noble Residence
            if (setting == FoodSetting.None)
                return type == BaseBuildingType.NobleResidence;

            // Food selected but not available — HALT
            return !hasFoodAvailable;
        }

        /// <summary>
        /// Get the effective multiplier accounting for food availability.
        /// Returns 0 if halted.
        /// </summary>
        public static int GetEffectiveMultiplier(BaseBuildingType type, FoodSetting setting,
            bool hasFoodAvailable, bool hasFoodMaster = false)
        {
            if (ShouldHalt(type, setting, hasFoodAvailable))
                return 0;

            return GetMultiplier(type, setting, hasFoodMaster);
        }
    }
}
