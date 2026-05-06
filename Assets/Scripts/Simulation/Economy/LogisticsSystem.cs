using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages storehouse relay logistics. All goods flow:
    /// Producer → nearest Storehouse → Carrier relay → destination Storehouse → Consumer.
    /// Carriers transport max 3 items per trip.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class LogisticsSystem
    {
        private readonly Dictionary<int, Storehouse> _storehousesBySector = new();
        private readonly List<Storehouse> _allStorehouses = new();
        private readonly List<CarrierTask> _activeTasks = new();
        private readonly HashSet<long> _pavedEdges = new();
        private readonly SectorGraph _graph;
        private readonly int _carrierMaxItems;
        private readonly EventBus _eventBus;
        private TechEffects _techEffects;

        private const float NORMAL_TRAVEL_TIME = 3f;  // seconds per sector hop
        private const float PAVED_TRAVEL_TIME = 1.5f;  // paved road = 2x faster

        public LogisticsSystem(SectorGraph graph, int carrierMaxItems, EventBus eventBus)
        {
            _graph = graph;
            _carrierMaxItems = carrierMaxItems;
            _eventBus = eventBus;
        }

        /// <summary>Set the TechEffects reference for carrier speed bonuses.</summary>
        public void SetTechEffects(TechEffects techEffects) => _techEffects = techEffects;

        /// <summary>All storehouses.</summary>
        public IReadOnlyList<Storehouse> AllStorehouses => _allStorehouses;

        /// <summary>All active carrier tasks.</summary>
        public IReadOnlyList<CarrierTask> ActiveTasks => _activeTasks;

        /// <summary>Place a storehouse in a sector for a player. No-op if one already exists.</summary>
        public Storehouse PlaceStorehouse(int sectorId, int ownerId)
        {
            if (_storehousesBySector.ContainsKey(sectorId))
                return _storehousesBySector[sectorId]; // Already has one

            var sh = new Storehouse(sectorId, ownerId);
            _storehousesBySector[sectorId] = sh;
            _allStorehouses.Add(sh);
            return sh;
        }

        /// <summary>
        /// Replace the storehouse in a sector with a new one for the given owner.
        /// Used on sector conquest to transfer storehouse ownership.
        /// </summary>
        public Storehouse ReplaceStorehouse(int sectorId, int newOwnerId)
        {
            if (_storehousesBySector.TryGetValue(sectorId, out var old))
                _allStorehouses.Remove(old);

            var sh = new Storehouse(sectorId, newOwnerId);
            _storehousesBySector[sectorId] = sh;
            _allStorehouses.Add(sh);
            return sh;
        }

        /// <summary>Get the storehouse in a sector (null if none).</summary>
        public Storehouse GetStorehouse(int sectorId)
        {
            return _storehousesBySector.TryGetValue(sectorId, out var sh) ? sh : null;
        }

        /// <summary>Check if a sector has a storehouse.</summary>
        public bool HasStorehouse(int sectorId)
        {
            return _storehousesBySector.ContainsKey(sectorId);
        }

        /// <summary>
        /// Request a delivery between sectors. Creates a carrier task if a carrier is available.
        /// Returns true if a carrier was dispatched.
        /// </summary>
        public bool RequestDelivery(int fromSectorId, int toSectorId,
            ResourceType resourceType, int amount)
        {
            var fromSh = GetStorehouse(fromSectorId);
            if (fromSh == null || !fromSh.DispatchCarrier())
                return false;

            int clampedAmount = System.Math.Min(amount, _carrierMaxItems);

            var path = _graph.FindPath(fromSectorId, toSectorId);
            if (path.Count == 0)
            {
                fromSh.ReturnCarrier();
                return false;
            }

            float travelTime = CalculateTravelTime(path);
            float speedMult = _techEffects?.GetCarrierSpeedMultiplier(fromSh.OwnerId) ?? 1f;
            travelTime /= speedMult;

            var task = new CarrierTask(
                fromSh.Id, fromSectorId, toSectorId,
                resourceType, clampedAmount,
                travelTime, path);
            _activeTasks.Add(task);

            return true;
        }

        /// <summary>
        /// Tick all active carrier tasks. Advance travel, complete deliveries.
        /// </summary>
        public void Tick(float deltaTime)
        {
            var completed = new List<CarrierTask>();

            for (int i = 0; i < _activeTasks.Count; i++)
            {
                var task = _activeTasks[i];
                task.Progress += deltaTime / task.TotalTravelTime;

                if (task.Progress >= 1f)
                {
                    task.Progress = 1f;
                    completed.Add(task);
                }
            }

            for (int i = 0; i < completed.Count; i++)
            {
                var task = completed[i];
                _activeTasks.Remove(task);

                // Return carrier to origin storehouse
                var fromSh = GetStorehouseById(task.StorehouseId);
                fromSh?.ReturnCarrier();

                _eventBus.Publish(new CarrierDeliveryEvent(
                    task.FromSectorId, task.ToSectorId,
                    task.ResourceType, task.Amount));
            }
        }

        /// <summary>Build a paved road between two adjacent sectors. Costs stone (handled by caller).</summary>
        public bool BuildPavedRoad(int sectorA, int sectorB)
        {
            if (!_graph.AreAdjacent(sectorA, sectorB))
                return false;
            long key = MakeEdgeKey(sectorA, sectorB);
            return _pavedEdges.Add(key);
        }

        /// <summary>Check if an edge between two sectors has a paved road.</summary>
        public bool IsPaved(int sectorA, int sectorB)
        {
            return _pavedEdges.Contains(MakeEdgeKey(sectorA, sectorB));
        }

        /// <summary>Get the total number of paved roads.</summary>
        public int PavedRoadCount => _pavedEdges.Count;

        private float CalculateTravelTime(List<int> path)
        {
            float total = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                total += IsPaved(path[i], path[i + 1])
                    ? PAVED_TRAVEL_TIME
                    : NORMAL_TRAVEL_TIME;
            }
            // Minimum 1 hop even for same-sector
            return total > 0f ? total : NORMAL_TRAVEL_TIME;
        }

        private static long MakeEdgeKey(int a, int b)
        {
            return System.Math.Min(a, b) * 1000L + System.Math.Max(a, b);
        }

        private Storehouse GetStorehouseById(int id)
        {
            for (int i = 0; i < _allStorehouses.Count; i++)
            {
                if (_allStorehouses[i].Id == id)
                    return _allStorehouses[i];
            }
            return null;
        }
    }

    /// <summary>
    /// An in-progress carrier delivery task.
    /// </summary>
    public class CarrierTask
    {
        public int StorehouseId;
        public int FromSectorId;
        public int ToSectorId;
        public ResourceType ResourceType;
        public int Amount;
        public float TotalTravelTime;
        public float Progress; // 0 to 1
        public List<int> Path;

        public CarrierTask(int storehouseId, int from, int to,
            ResourceType resourceType, int amount,
            float travelTime, List<int> path)
        {
            StorehouseId = storehouseId;
            FromSectorId = from;
            ToSectorId = to;
            ResourceType = resourceType;
            Amount = amount;
            TotalTravelTime = travelTime;
            Progress = 0f;
            Path = path;
        }
    }

    /// <summary>Fired when a carrier completes a delivery.</summary>
    public readonly struct CarrierDeliveryEvent
    {
        public readonly int FromSectorId;
        public readonly int ToSectorId;
        public readonly ResourceType ResourceType;
        public readonly int Amount;

        public CarrierDeliveryEvent(int from, int to, ResourceType type, int amount)
        {
            FromSectorId = from;
            ToSectorId = to;
            ResourceType = type;
            Amount = amount;
        }
    }
}
