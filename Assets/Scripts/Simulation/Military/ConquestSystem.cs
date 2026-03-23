using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Handles all three sector conquest methods:
    /// 1. Military — army attacks sector outpost
    /// 2. Proselytism — clerics convert neutral sectors
    /// 3. Bribery — spend coins+garments+jewelry on neutral sectors
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class ConquestSystem
    {
        private readonly SectorGraph _graph;
        private readonly ArmySystem _army;
        private readonly CombatResolver _combat;
        private readonly Dictionary<int, PlayerResources> _resources;
        private readonly EventBus _eventBus;
        private readonly List<ProselytismTask> _proselytismTasks = new();

        public ConquestSystem(SectorGraph graph, ArmySystem army,
            CombatResolver combat, Dictionary<int, PlayerResources> resources,
            EventBus eventBus)
        {
            _graph = graph;
            _army = army;
            _combat = combat;
            _resources = resources;
            _eventBus = eventBus;

            // Auto-resolve combat when army arrives at enemy/neutral sector
            _eventBus.Subscribe<ArmyArrivedEvent>(OnArmyArrived);
        }

        /// <summary>Active proselytism tasks.</summary>
        public IReadOnlyList<ProselytismTask> ProselytismTasks => _proselytismTasks;

        private void OnArmyArrived(ArmyArrivedEvent evt)
        {
            var generals = _army.GetGenerals(evt.PlayerId);
            General gen = null;
            foreach (var g in generals)
            {
                if (g.Id == evt.GeneralId) { gen = g; break; }
            }
            if (gen == null) return;

            var sector = _graph.GetSector(evt.SectorId);
            if (sector.OwnerId != evt.PlayerId && sector.OwnerId != Sector.UNOWNED)
            {
                _combat.ResolveCombat(gen, evt.SectorId);
            }
        }

        /// <summary>
        /// Attempt to conquer a neutral sector via bribery.
        /// Cost scales with garrison: base coins + garments + jewelry.
        /// </summary>
        public bool TryBribe(int playerId, int sectorId)
        {
            var sector = _graph.GetSector(sectorId);
            if (!sector.IsNeutral) return false;

            if (!_resources.TryGetValue(playerId, out var res))
                return false;

            GetBriberyCost(sector.GarrisonStrength,
                out int coins, out int garments, out int jewelry);

            if (!res.Has(ResourceType.Coins, coins) ||
                !res.Has(ResourceType.Garments, garments) ||
                !res.Has(ResourceType.Jewelry, jewelry))
                return false;

            res.TrySpend(ResourceType.Coins, coins);
            res.TrySpend(ResourceType.Garments, garments);
            res.TrySpend(ResourceType.Jewelry, jewelry);

            int prevOwner = sector.OwnerId;
            sector.SetOwner(playerId);
            _eventBus.Publish(new SectorConqueredEvent(
                sectorId, playerId, prevOwner, ConquestMethod.Bribery));
            return true;
        }

        /// <summary>
        /// Start proselytism on a neutral sector.
        /// Requires clerics (Novices/Brothers/Fathers): ~6 for unfortified, ~12 for fortified.
        /// </summary>
        public bool StartProselytism(int playerId, int sectorId, int clericCount)
        {
            var sector = _graph.GetSector(sectorId);
            if (!sector.IsNeutral) return false;

            int required = sector.IsFortified ? 12 : 6;
            if (clericCount < required) return false;

            // Proselytism takes 30 seconds base
            float duration = 30f;
            _proselytismTasks.Add(new ProselytismTask(
                playerId, sectorId, clericCount, duration));
            return true;
        }

        /// <summary>Tick proselytism tasks.</summary>
        public void Tick(float deltaTime)
        {
            var completed = new List<ProselytismTask>();
            for (int i = 0; i < _proselytismTasks.Count; i++)
            {
                var task = _proselytismTasks[i];
                task.Progress += deltaTime / task.Duration;
                if (task.Progress >= 1f)
                {
                    task.Progress = 1f;
                    completed.Add(task);
                }
            }

            foreach (var task in completed)
            {
                _proselytismTasks.Remove(task);
                var sector = _graph.GetSector(task.SectorId);
                if (sector.IsNeutral) // Still neutral (not taken by someone else)
                {
                    int prevOwner = sector.OwnerId;
                    sector.SetOwner(task.PlayerId);
                    _eventBus.Publish(new SectorConqueredEvent(
                        task.SectorId, task.PlayerId, prevOwner,
                        ConquestMethod.Proselytism));
                }
            }
        }

        /// <summary>Calculate bribery cost based on garrison strength.</summary>
        public static void GetBriberyCost(int garrisonStrength,
            out int coins, out int garments, out int jewelry)
        {
            // Scales: weak garrison ~11 coins, strong ~94 coins
            coins = 8 + garrisonStrength * 3;
            garments = 2 + garrisonStrength;
            jewelry = 1 + garrisonStrength / 2;
        }
    }

    public class ProselytismTask
    {
        public int PlayerId;
        public int SectorId;
        public int ClericCount;
        public float Duration;
        public float Progress;

        public ProselytismTask(int playerId, int sectorId, int clericCount, float duration)
        {
            PlayerId = playerId;
            SectorId = sectorId;
            ClericCount = clericCount;
            Duration = duration;
            Progress = 0f;
        }
    }
}
