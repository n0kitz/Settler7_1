using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages building fortifications in owned sectors.
    /// Requires prestige unlock "mil_fortification" and stone resources.
    /// Tech "tech_fortification_tech" gives 50% faster build speed.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class FortificationSystem
    {
        private readonly SectorGraph _graph;
        private readonly PrestigeSystem _prestige;
        private readonly Dictionary<int, PlayerResources> _resources;
        private TechEffects _techEffects;
        private readonly EventBus _eventBus;
        private readonly List<FortificationTask> _tasks = new();

        private const int STONE_COST = 10;
        private const float BASE_BUILD_TIME = 30f;

        public FortificationSystem(SectorGraph graph, PrestigeSystem prestige,
            Dictionary<int, PlayerResources> resources, EventBus eventBus)
        {
            _graph = graph;
            _prestige = prestige;
            _resources = resources;
            _eventBus = eventBus;
        }

        /// <summary>Set the TechEffects reference (called after TechEffects is created).</summary>
        public void SetTechEffects(TechEffects techEffects) => _techEffects = techEffects;

        /// <summary>Active fortification construction tasks.</summary>
        public IReadOnlyList<FortificationTask> ActiveTasks => _tasks;

        /// <summary>
        /// Start building a fortification in a sector.
        /// Requires: player owns sector, not already fortified, prestige unlock, 10 stone.
        /// </summary>
        public bool StartFortification(int playerId, int sectorId)
        {
            var sector = _graph.GetSector(sectorId);
            if (sector.OwnerId != playerId) return false;
            if (sector.IsFortified) return false;
            if (!_prestige.HasUnlock(playerId, "mil_fortification")) return false;

            // Check not already building
            foreach (var t in _tasks)
                if (t.SectorId == sectorId) return false;

            if (!_resources.TryGetValue(playerId, out var res)) return false;
            if (!res.TrySpend(ResourceType.Stone, STONE_COST)) return false;

            float speedMult = _techEffects?.GetFortificationSpeedMultiplier(playerId) ?? 1f;
            float buildTime = BASE_BUILD_TIME / speedMult;

            _tasks.Add(new FortificationTask(playerId, sectorId, buildTime));
            return true;
        }

        /// <summary>Tick fortification construction progress.</summary>
        public void Tick(float deltaTime)
        {
            var completed = new List<FortificationTask>();

            for (int i = 0; i < _tasks.Count; i++)
            {
                var task = _tasks[i];
                task.Progress += deltaTime / task.TotalTime;
                if (task.Progress >= 1f)
                {
                    task.Progress = 1f;
                    completed.Add(task);
                }
            }

            foreach (var task in completed)
            {
                _tasks.Remove(task);
                var sector = _graph.GetSector(task.SectorId);
                sector.SetFortified(true);
                _eventBus.Publish(new FortificationBuiltEvent(task.PlayerId, task.SectorId));
            }
        }
    }

    public class FortificationTask
    {
        public int PlayerId;
        public int SectorId;
        public float TotalTime;
        public float Progress;

        public FortificationTask(int playerId, int sectorId, float totalTime)
        {
            PlayerId = playerId;
            SectorId = sectorId;
            TotalTime = totalTime;
            Progress = 0f;
        }
    }

    public readonly struct FortificationBuiltEvent
    {
        public readonly int PlayerId;
        public readonly int SectorId;

        public FortificationBuiltEvent(int playerId, int sectorId)
        {
            PlayerId = playerId;
            SectorId = sectorId;
        }
    }
}
