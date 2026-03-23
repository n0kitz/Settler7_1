using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Applies technology bonuses to production, construction, logistics, and military.
    /// Subscribes to TechResearchedEvent and caches per-player multipliers.
    /// Other systems query TechEffects for their active bonuses.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class TechEffects
    {
        private readonly ResearchSystem _research;

        public TechEffects(ResearchSystem research)
        {
            _research = research;
        }

        /// <summary>
        /// Get production speed multiplier for a specific work yard type.
        /// Returns 1.0 for no bonus, 1.5 for +50%, 2.0 for x2, etc.
        /// </summary>
        public float GetProductionMultiplier(int playerId, string workYardId)
        {
            float mult = 1f;

            switch (workYardId)
            {
                case "grain_barn":
                    if (_research.HasTech(playerId, "tech_plowing")) mult *= 1.5f;
                    if (_research.HasTech(playerId, "tech_crop_rotation")) mult *= 2f / 1.5f;
                    if (_research.HasTech(playerId, "tech_irrigation")) mult *= 2f;
                    break;

                case "quarry":
                    if (_research.HasTech(playerId, "tech_masonry")) mult *= 1.5f;
                    break;

                case "sawmill":
                    if (_research.HasTech(playerId, "tech_carpentry")) mult *= 1.5f;
                    break;

                case "piggery":
                case "shepherd":
                    if (_research.HasTech(playerId, "tech_animal_husbandry")) mult *= 1.5f;
                    break;

                case "iron_smelter":
                    if (_research.HasTech(playerId, "tech_smelting")) mult *= 1.5f;
                    if (_research.HasTech(playerId, "tech_metallurgy")) mult *= 2f / 1.5f;
                    break;

                case "gold_miner":
                    if (_research.HasTech(playerId, "tech_metallurgy")) mult *= 2f;
                    break;

                case "fisher":
                    if (_research.HasTech(playerId, "tech_fishing")) mult *= 1.5f;
                    break;

                case "wheelwright":
                    if (_research.HasTech(playerId, "tech_woodworking")) mult *= 1.5f;
                    break;

                case "stable":
                    if (_research.HasTech(playerId, "tech_breeding")) mult *= 1.5f;
                    break;

                case "blacksmith":
                    if (_research.HasTech(playerId, "tech_steel")) mult *= 1.5f;
                    break;

                // Irrigation: all Farm building work yards get x2
                case "windmill":
                    if (_research.HasTech(playerId, "tech_irrigation")) mult *= 2f;
                    break;
            }

            return mult;
        }

        /// <summary>
        /// Get construction speed multiplier (1.0 = normal, 2.0 = 50% faster).
        /// tech_architecture: Construction time -50% (= speed x2).
        /// </summary>
        public float GetConstructionSpeedMultiplier(int playerId)
        {
            return _research.HasTech(playerId, "tech_architecture") ? 2f : 1f;
        }

        /// <summary>
        /// Get fortification build speed multiplier.
        /// tech_fortification_tech: Build fortifications 50% faster (= speed x2).
        /// </summary>
        public float GetFortificationSpeedMultiplier(int playerId)
        {
            return _research.HasTech(playerId, "tech_fortification_tech") ? 2f : 1f;
        }

        /// <summary>
        /// Get carrier movement speed multiplier.
        /// tech_logistics: Carriers move 50% faster (= speed x1.5).
        /// </summary>
        public float GetCarrierSpeedMultiplier(int playerId)
        {
            return _research.HasTech(playerId, "tech_logistics") ? 1.5f : 1f;
        }

        /// <summary>
        /// Get attack multiplier for a specific unit type.
        /// tech_cavalry: Cavaliers +30% attack.
        /// </summary>
        public float GetUnitAttackMultiplier(int playerId, UnitType unitType)
        {
            if (unitType == UnitType.Cavalier &&
                _research.HasTech(playerId, "tech_cavalry"))
                return 1.3f;
            return 1f;
        }

        /// <summary>
        /// Get fancy food multiplier bonus.
        /// tech_preservation: Fancy food multiplier +1.
        /// </summary>
        public int GetFancyFoodBonus(int playerId)
        {
            return _research.HasTech(playerId, "tech_preservation") ? 1 : 0;
        }

        /// <summary>
        /// Get population bonus from Hygiene tech.
        /// tech_hygiene: Residence +2 pop, Noble +4 pop.
        /// </summary>
        public int GetHygieneBonus(int playerId, BaseBuildingType buildingType)
        {
            if (!_research.HasTech(playerId, "tech_hygiene")) return 0;
            return buildingType switch
            {
                BaseBuildingType.Residence => 2,
                BaseBuildingType.NobleResidence => 4,
                _ => 0
            };
        }
    }
}
