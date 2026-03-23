using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Tracks resource amounts for a single player.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class PlayerResources
    {
        private readonly int _playerId;
        private readonly Dictionary<ResourceType, int> _amounts = new();
        private readonly EventBus _eventBus;

        public int PlayerId => _playerId;

        public PlayerResources(int playerId, EventBus eventBus)
        {
            _playerId = playerId;
            _eventBus = eventBus;
        }

        /// <summary>Get current amount of a resource.</summary>
        public int Get(ResourceType type)
        {
            return _amounts.TryGetValue(type, out int amount) ? amount : 0;
        }

        /// <summary>Add resources. Returns the new total.</summary>
        public int Add(ResourceType type, int amount)
        {
            if (amount <= 0) return Get(type);

            if (!_amounts.ContainsKey(type))
                _amounts[type] = 0;

            _amounts[type] += amount;
            _eventBus?.Publish(new ResourceChangedEvent(_playerId, type, _amounts[type]));
            return _amounts[type];
        }

        /// <summary>Try to spend resources. Returns true if successful (had enough).</summary>
        public bool TrySpend(ResourceType type, int amount)
        {
            if (amount <= 0) return true;
            if (Get(type) < amount) return false;

            _amounts[type] -= amount;
            _eventBus?.Publish(new ResourceChangedEvent(_playerId, type, _amounts[type]));
            return true;
        }

        /// <summary>Check if player has at least this much of a resource.</summary>
        public bool Has(ResourceType type, int amount)
        {
            return Get(type) >= amount;
        }

        /// <summary>Check if player can afford a building's costs.</summary>
        public bool CanAfford(int plankCost, int stoneCost)
        {
            return Has(ResourceType.Planks, plankCost) && Has(ResourceType.Stone, stoneCost);
        }

        /// <summary>Deduct building costs. Returns false if insufficient.</summary>
        public bool TrySpendBuildingCost(int plankCost, int stoneCost)
        {
            if (!CanAfford(plankCost, stoneCost))
                return false;

            TrySpend(ResourceType.Planks, plankCost);
            TrySpend(ResourceType.Stone, stoneCost);
            return true;
        }

        /// <summary>Set a resource to a specific value (for initialization/testing).</summary>
        public void Set(ResourceType type, int amount)
        {
            _amounts[type] = amount;
            _eventBus?.Publish(new ResourceChangedEvent(_playerId, type, amount));
        }
    }
}
