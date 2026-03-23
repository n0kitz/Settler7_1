using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Tracks settler count, living space, and tool allocation.
    /// Each work yard needs 1 settler + 1 tool to be operational.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class PopulationSystem
    {
        private readonly Dictionary<int, PlayerResources> _playerResources;
        private readonly ConstructionSystem _construction;
        private readonly ProductionSystem _production;
        private readonly EventBus _eventBus;
        private TechEffects _techEffects;

        public PopulationSystem(Dictionary<int, PlayerResources> playerResources,
            ConstructionSystem construction, ProductionSystem production,
            EventBus eventBus)
        {
            _playerResources = playerResources;
            _construction = construction;
            _production = production;
            _eventBus = eventBus;
        }

        /// <summary>Set the TechEffects reference for hygiene population bonuses.</summary>
        public void SetTechEffects(TechEffects techEffects) => _techEffects = techEffects;

        /// <summary>
        /// Get the total living space (max settlers) for a player.
        /// Each completed building provides population based on type + upgrade level.
        /// </summary>
        public int GetLivingSpace(int playerId)
        {
            int total = 0;
            var buildings = _construction.GetBuildingsByPlayer(playerId);
            for (int i = 0; i < buildings.Count; i++)
            {
                if (!buildings[i].IsOperational)
                    continue;
                total += buildings[i].GetBasePopulation();
                if (_techEffects != null)
                    total += _techEffects.GetHygieneBonus(playerId, buildings[i].Type);
            }
            return total;
        }

        /// <summary>
        /// Get the number of settlers currently employed (work yards with workers).
        /// </summary>
        public int GetEmployedCount(int playerId)
        {
            int count = 0;
            for (int i = 0; i < _production.AllWorkYards.Count; i++)
            {
                var wy = _production.AllWorkYards[i];
                if (wy.OwnerId == playerId && wy.HasWorker)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Get the number of available (unemployed) settlers.
        /// </summary>
        public int GetAvailableSettlers(int playerId)
        {
            return GetLivingSpace(playerId) - GetEmployedCount(playerId);
        }

        /// <summary>
        /// Try to assign workers and tools to unassigned work yards.
        /// Called each tick to auto-assign available settlers and tools.
        /// </summary>
        public void Tick(float deltaTime)
        {
            foreach (var kvp in _playerResources)
            {
                int playerId = kvp.Key;
                var resources = kvp.Value;

                int availableSettlers = GetAvailableSettlers(playerId);
                int availableTools = resources.Get(ResourceType.Tools);

                // Auto-assign workers and tools to unassigned work yards
                for (int i = 0; i < _production.AllWorkYards.Count; i++)
                {
                    var wy = _production.AllWorkYards[i];
                    if (wy.OwnerId != playerId)
                        continue;

                    // Assign worker if needed and available
                    if (!wy.HasWorker && availableSettlers > 0)
                    {
                        wy.AssignWorker();
                        availableSettlers--;
                    }

                    // Assign tool if needed and available
                    if (!wy.HasTool && availableTools > 0)
                    {
                        wy.ProvideTool();
                        resources.TrySpend(ResourceType.Tools, 1);
                        availableTools--;
                    }
                }
            }
        }
    }
}
