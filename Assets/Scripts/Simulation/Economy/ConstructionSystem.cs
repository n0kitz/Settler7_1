using System.Collections.Generic;
using System.Linq;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages building construction: assigns constructors, ticks progress.
    /// Each player starts with 1 constructor from their Castle.
    /// Constructors can be upgraded (1 plank) to have up to 3 total.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class ConstructionSystem
    {
        private readonly Dictionary<int, List<Building>> _buildingsBySector = new();
        private readonly Dictionary<int, List<Building>> _buildingsByPlayer = new();
        private readonly List<Building> _allBuildings = new();
        private readonly List<Building> _constructionQueue = new();
        private readonly Dictionary<int, int> _constructorCount = new(); // playerId → count
        private readonly Dictionary<int, int> _activeConstructions = new(); // playerId → active count
        private readonly EventBus _eventBus;
        private readonly float _baseConstructionTime;
        private TechEffects _techEffects;

        public ConstructionSystem(EventBus eventBus, float baseConstructionTime)
        {
            _eventBus = eventBus;
            _baseConstructionTime = baseConstructionTime;
        }

        /// <summary>Set the TechEffects reference for construction speed bonuses.</summary>
        public void SetTechEffects(TechEffects techEffects) => _techEffects = techEffects;

        /// <summary>All buildings in the simulation.</summary>
        public IReadOnlyList<Building> AllBuildings => _allBuildings;

        /// <summary>Set the number of constructors a player has.</summary>
        public void SetConstructorCount(int playerId, int count)
        {
            _constructorCount[playerId] = count;
            if (!_activeConstructions.ContainsKey(playerId))
                _activeConstructions[playerId] = 0;
        }

        /// <summary>Get how many constructors a player has.</summary>
        public int GetConstructorCount(int playerId)
        {
            return _constructorCount.TryGetValue(playerId, out int count) ? count : 0;
        }

        /// <summary>
        /// Place a new building in a sector. Returns the building if successful, null if slot full.
        /// </summary>
        public Building PlaceBuilding(BaseBuildingType type, int sectorId, int ownerId,
            int maxWorkYards, float localX, float localZ, int currentBuildCount, int maxSlots)
        {
            if (currentBuildCount >= maxSlots)
                return null;

            var building = new Building(type, sectorId, ownerId, maxWorkYards, localX, localZ);
            _allBuildings.Add(building);
            _constructionQueue.Add(building);

            if (!_buildingsBySector.TryGetValue(sectorId, out var sectorList))
            {
                sectorList = new List<Building>();
                _buildingsBySector[sectorId] = sectorList;
            }
            sectorList.Add(building);

            if (!_buildingsByPlayer.TryGetValue(ownerId, out var playerList))
            {
                playerList = new List<Building>();
                _buildingsByPlayer[ownerId] = playerList;
            }
            playerList.Add(building);

            _eventBus.Publish(new BuildingPlacedEvent(building.Id, sectorId, type));
            return building;
        }

        /// <summary>Get all buildings in a specific sector.</summary>
        public IReadOnlyList<Building> GetBuildingsInSector(int sectorId)
        {
            return _buildingsBySector.TryGetValue(sectorId, out var list)
                ? list
                : (IReadOnlyList<Building>)System.Array.Empty<Building>();
        }

        /// <summary>Get all buildings owned by a player.</summary>
        public IReadOnlyList<Building> GetBuildingsByPlayer(int playerId)
        {
            return _buildingsByPlayer.TryGetValue(playerId, out var list)
                ? list
                : (IReadOnlyList<Building>)System.Array.Empty<Building>();
        }

        /// <summary>Find a building by its ID.</summary>
        public Building GetBuilding(int buildingId)
        {
            for (int i = 0; i < _allBuildings.Count; i++)
            {
                if (_allBuildings[i].Id == buildingId)
                    return _allBuildings[i];
            }
            return null;
        }

        /// <summary>
        /// Tick construction progress for all buildings in the queue.
        /// Each constructor works on one building at a time (FIFO per player).
        /// </summary>
        public void Tick(float deltaTime)
        {
            // Reset active counts (snapshot keys to avoid modifying dictionary during iteration)
            foreach (var key in _activeConstructions.Keys.ToList())
                _activeConstructions[key] = 0;

            var completedBuildings = new List<Building>();

            for (int i = 0; i < _constructionQueue.Count; i++)
            {
                var building = _constructionQueue[i];
                int playerId = building.OwnerId;

                int maxConstructors = GetConstructorCount(playerId);
                if (!_activeConstructions.TryGetValue(playerId, out int active))
                    active = 0;

                if (active >= maxConstructors)
                    continue;

                _activeConstructions[playerId] = active + 1;

                // Progress = deltaTime / baseConstructionTime * techSpeedMultiplier
                float techMult = _techEffects?.GetConstructionSpeedMultiplier(playerId) ?? 1f;
                float progress = (deltaTime / _baseConstructionTime) * techMult;
                bool completed = building.AdvanceConstruction(progress);

                if (completed)
                    completedBuildings.Add(building);
            }

            // Remove completed buildings from queue and fire events
            for (int i = 0; i < completedBuildings.Count; i++)
            {
                _constructionQueue.Remove(completedBuildings[i]);
                _eventBus.Publish(new BuildingCompletedEvent(
                    completedBuildings[i].Id, completedBuildings[i].SectorId));
            }
        }

        /// <summary>
        /// Get how many constructors are currently active for a player after the last Tick.
        /// Used by UpgradeSystem to share the constructor pool.
        /// </summary>
        public int GetActiveConstructionCount(int playerId)
        {
            return _activeConstructions.TryGetValue(playerId, out int count) ? count : 0;
        }

        /// <summary>Get the number of buildings currently in the construction queue for a player.</summary>
        public int GetQueuedCount(int playerId)
        {
            int count = 0;
            for (int i = 0; i < _constructionQueue.Count; i++)
            {
                if (_constructionQueue[i].OwnerId == playerId)
                    count++;
            }
            return count;
        }

        /// <summary>Get the total number of buildings in a sector (all states).</summary>
        public int GetBuildingCountInSector(int sectorId)
        {
            return _buildingsBySector.TryGetValue(sectorId, out var list) ? list.Count : 0;
        }

        /// <summary>
        /// Restore a building directly from save data — bypasses slot check and construction
        /// queue for buildings that are already complete. Only adds to queue if still in progress.
        /// Fires BuildingPlacedEvent so presentation layer spawns the visual.
        /// </summary>
        public Building RestoreBuilding(BaseBuildingType type, int sectorId, int ownerId,
            int maxWorkYards, float localX, float localZ,
            BuildingState state, float progress, int upgradeLevel, FoodSetting foodSetting)
        {
            var building = new Building(type, sectorId, ownerId, maxWorkYards, localX, localZ);
            building.RestoreState(state, progress, upgradeLevel);
            building.SetFoodSetting(foodSetting);

            _allBuildings.Add(building);

            if (!_buildingsBySector.TryGetValue(sectorId, out var sectorList))
            {
                sectorList = new List<Building>();
                _buildingsBySector[sectorId] = sectorList;
            }
            sectorList.Add(building);

            if (!_buildingsByPlayer.TryGetValue(ownerId, out var playerList))
            {
                playerList = new List<Building>();
                _buildingsByPlayer[ownerId] = playerList;
            }
            playerList.Add(building);

            // Only queue buildings that are still being constructed/upgraded
            if (state == BuildingState.Planned || state == BuildingState.UnderConstruction
                || state == BuildingState.Upgrading)
                _constructionQueue.Add(building);

            _eventBus.Publish(new BuildingPlacedEvent(building.Id, sectorId, type));
            return building;
        }

        /// <summary>
        /// Remove all buildings in a sector owned by a specific player.
        /// Used on sector conquest (buildings destroyed, winner rebuilds).
        /// Returns the list of destroyed building IDs for event firing.
        /// </summary>
        public List<int> RemoveBuildingsInSector(int sectorId, int ownerId)
        {
            var destroyed = new List<int>();
            if (!_buildingsBySector.TryGetValue(sectorId, out var sectorList))
                return destroyed;

            for (int i = sectorList.Count - 1; i >= 0; i--)
            {
                var b = sectorList[i];
                if (b.OwnerId != ownerId) continue;
                sectorList.RemoveAt(i);
                _allBuildings.Remove(b);
                _constructionQueue.Remove(b);
                if (_buildingsByPlayer.TryGetValue(ownerId, out var playerList))
                    playerList.Remove(b);
                destroyed.Add(b.Id);
            }
            return destroyed;
        }
    }
}
