using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Processes all work yard production cycles.
    /// Handles recipe input consumption, output production, and food boosting.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class ProductionSystem
    {
        private readonly List<WorkYard> _allWorkYards = new();
        private readonly Dictionary<int, PlayerResources> _playerResources;
        private readonly ConstructionSystem _construction;
        private readonly EventBus _eventBus;
        private TechEffects _techEffects;
        private PrestigeSystem _prestige;

        public ProductionSystem(Dictionary<int, PlayerResources> playerResources,
            ConstructionSystem construction, EventBus eventBus)
        {
            _playerResources = playerResources;
            _construction = construction;
            _eventBus = eventBus;
        }

        /// <summary>Wire TechEffects for production bonuses (called after GameState init).</summary>
        public void SetTechEffects(TechEffects techEffects) => _techEffects = techEffects;

        /// <summary>Wire PrestigeSystem for eco_food_master check.</summary>
        public void SetPrestige(PrestigeSystem prestige) => _prestige = prestige;

        /// <summary>All registered work yards.</summary>
        public IReadOnlyList<WorkYard> AllWorkYards => _allWorkYards;

        /// <summary>Register a work yard for production ticking.</summary>
        public void RegisterWorkYard(WorkYard workYard)
        {
            _allWorkYards.Add(workYard);
        }

        /// <summary>
        /// Tick all active work yards. For each operational work yard:
        /// 1. Check food boost status
        /// 2. Advance cycle by (deltaTime / cycleDuration) * multiplier
        /// 3. On cycle complete: consume inputs, produce outputs
        /// </summary>
        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _allWorkYards.Count; i++)
            {
                var wy = _allWorkYards[i];
                if (!wy.IsOperational)
                    continue;

                var recipe = RecipeDatabase.Get(wy.TypeId);
                if (recipe == null)
                    continue;

                // Get the parent building for food boost
                var building = _construction.GetBuilding(wy.BuildingId);
                if (building == null || !building.IsOperational)
                    continue;

                // Calculate food boost multiplier
                int multiplier = GetEffectiveMultiplier(building, wy.OwnerId);
                if (multiplier <= 0)
                {
                    wy.ResetCycle();
                    continue;
                }

                // Reserve inputs at cycle start (prevents mid-cycle theft)
                if (!wy.InputsReserved)
                {
                    if (!ConsumeInputs(wy.OwnerId, recipe))
                        continue; // Can't start — inputs unavailable
                    wy.InputsReserved = true;
                }

                // Apply tech bonus to production speed
                float techMult = _techEffects?.GetProductionMultiplier(wy.OwnerId, wy.TypeId) ?? 1f;

                // Advance cycle: progress = (dt / cycleDuration) * foodMult * techMult
                float progress = (deltaTime / recipe.CycleDuration) * multiplier * techMult;
                bool completed = wy.AdvanceCycle(progress);

                if (completed)
                {
                    // Inputs were already consumed at reservation
                    wy.InputsReserved = false;
                    ProduceOutputs(wy.OwnerId, recipe);
                }
            }
        }

        private int GetEffectiveMultiplier(Building building, int playerId)
        {
            var setting = building.FoodSetting;

            // Check if food is actually available
            bool hasFoodAvailable = true;
            if (setting == FoodSetting.Plain)
            {
                hasFoodAvailable = HasPlainFood(playerId);
            }
            else if (setting == FoodSetting.Fancy)
            {
                hasFoodAvailable = HasFancyFood(playerId);
            }

            bool hasFoodMaster = _prestige?.HasUnlock(playerId, "eco_food_master") ?? false;
            return FoodBoostCalculator.GetEffectiveMultiplier(
                building.Type, setting, hasFoodAvailable, hasFoodMaster);
        }

        private bool HasPlainFood(int playerId)
        {
            if (!_playerResources.TryGetValue(playerId, out var res))
                return false;
            return res.Get(ResourceType.Bread) > 0 || res.Get(ResourceType.Fish) > 0;
        }

        private bool HasFancyFood(int playerId)
        {
            if (!_playerResources.TryGetValue(playerId, out var res))
                return false;
            return res.Get(ResourceType.Sausages) > 0;
        }

        private bool ConsumeInputs(int playerId, RecipeDatabase.RecipeDef recipe)
        {
            if (!_playerResources.TryGetValue(playerId, out var res))
                return false;

            for (int i = 0; i < recipe.Inputs.Length; i++)
            {
                if (!res.TrySpend(recipe.Inputs[i].type, recipe.Inputs[i].amount))
                    return false;
            }
            return true;
        }

        private void ProduceOutputs(int playerId, RecipeDatabase.RecipeDef recipe)
        {
            if (!_playerResources.TryGetValue(playerId, out var res))
                return;

            for (int i = 0; i < recipe.Outputs.Length; i++)
            {
                res.Add(recipe.Outputs[i].type, recipe.Outputs[i].amount);
            }

            _eventBus.Publish(new ProductionCompleteEvent(
                playerId, recipe.WorkYardId, recipe.Outputs));
        }
    }

    /// <summary>Fired when a production cycle completes.</summary>
    public readonly struct ProductionCompleteEvent
    {
        public readonly int PlayerId;
        public readonly string WorkYardId;
        public readonly (ResourceType type, int amount)[] Outputs;

        public ProductionCompleteEvent(int playerId, string workYardId,
            (ResourceType type, int amount)[] outputs)
        {
            PlayerId = playerId;
            WorkYardId = workYardId;
            Outputs = outputs;
        }
    }
}
