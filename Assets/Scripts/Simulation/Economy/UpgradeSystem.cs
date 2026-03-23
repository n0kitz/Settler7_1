using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages building upgrades (Residence, Noble Residence).
    /// Upgrades use the construction queue — they require a constructor
    /// and take the same base time as new construction.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class UpgradeSystem
    {
        private readonly ConstructionSystem _construction;
        private readonly Dictionary<int, PlayerResources> _playerResources;
        private readonly PrestigeSystem _prestige;
        private readonly EventBus _eventBus;
        private readonly float _baseUpgradeTime;
        private readonly List<Building> _upgradeQueue = new();

        public UpgradeSystem(ConstructionSystem construction,
            Dictionary<int, PlayerResources> playerResources,
            PrestigeSystem prestige, EventBus eventBus, float baseUpgradeTime)
        {
            _construction = construction;
            _playerResources = playerResources;
            _prestige = prestige;
            _eventBus = eventBus;
            _baseUpgradeTime = baseUpgradeTime;
        }

        /// <summary>Buildings currently being upgraded.</summary>
        public IReadOnlyList<Building> UpgradeQueue => _upgradeQueue;

        /// <summary>
        /// Try to start upgrading a building. Checks prestige unlock, resources, and upgrade eligibility.
        /// Returns true if upgrade was started.
        /// </summary>
        public bool TryStartUpgrade(int buildingId)
        {
            var building = _construction.GetBuilding(buildingId);
            if (building == null || !building.CanUpgrade)
                return false;

            int playerId = building.OwnerId;

            // Check prestige unlock requirement
            if (!IsUpgradeUnlocked(building, playerId))
                return false;

            // Check resource cost
            GetUpgradeCost(building.Type, building.UpgradeLevel + 1,
                out int plankCost, out int stoneCost);
            if (!_playerResources.TryGetValue(playerId, out var res))
                return false;
            if (!res.TrySpendBuildingCost(plankCost, stoneCost))
                return false;

            // Start the upgrade
            building.StartUpgrade();
            _upgradeQueue.Add(building);
            return true;
        }

        /// <summary>
        /// Tick upgrade progress. Uses available constructors (shared with construction queue).
        /// </summary>
        public void Tick(float deltaTime)
        {
            var completed = new List<Building>();

            for (int i = 0; i < _upgradeQueue.Count; i++)
            {
                var building = _upgradeQueue[i];
                float progress = deltaTime / _baseUpgradeTime;
                bool done = building.AdvanceUpgrade(progress);

                if (done)
                    completed.Add(building);
            }

            for (int i = 0; i < completed.Count; i++)
            {
                var b = completed[i];
                _upgradeQueue.Remove(b);
                _eventBus.Publish(new BuildingUpgradedEvent(
                    b.Id, b.SectorId, b.UpgradeLevel));
            }
        }

        /// <summary>Check if the player has the prestige unlock for this upgrade.</summary>
        private bool IsUpgradeUnlocked(Building building, int playerId)
        {
            return building.Type switch
            {
                BaseBuildingType.Residence =>
                    _prestige.HasUnlock(playerId, "eco_residence_upgrade"),
                BaseBuildingType.NobleResidence =>
                    _prestige.HasUnlock(playerId, "eco_noble_upgrade"),
                _ => false
            };
        }

        /// <summary>Get the resource cost for an upgrade level.</summary>
        public static void GetUpgradeCost(BaseBuildingType type, int targetLevel,
            out int planks, out int stone)
        {
            // Each upgrade costs the same as building (scaled by level)
            switch (type)
            {
                case BaseBuildingType.Residence:
                    planks = 2 * targetLevel;
                    stone = 1 * targetLevel;
                    break;
                case BaseBuildingType.NobleResidence:
                    planks = 3 * targetLevel;
                    stone = 2 * targetLevel;
                    break;
                default:
                    planks = 0;
                    stone = 0;
                    break;
            }
        }
    }
}
