using System;
using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Owns the campaign mission catalogue and evaluates mission objectives each tick.
    /// Create it from the mission-launch flow when starting a campaign mission.
    /// Fires C# events consumed by the UI layer. The mission catalogue lives in
    /// CampaignSystem.Missions.cs.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public partial class CampaignSystem
    {
        // --- Events for presentation ---

        /// <summary>Fired when all special objectives for the active mission are met.</summary>
        public event Action<Mission> OnObjectivesComplete;

        // --- State ---

        private readonly GameState _state;
        private Mission _activeMission;
        private bool _completeFired;

        public Mission ActiveMission => _activeMission;

        // --- Static catalogue ---

        /// <summary>All campaign missions in play order.</summary>
        public static IReadOnlyList<Mission> AllMissions { get; } = BuildCatalogue();

        public CampaignSystem(GameState state)
        {
            _state = state;
        }

        /// <summary>
        /// Set which mission is currently being played. Objectives are reset —
        /// the catalogue is static, so a replayed mission would otherwise start
        /// with objectives already marked complete from the previous run.
        /// </summary>
        public void SetActiveMission(Mission mission)
        {
            _activeMission = mission;
            _completeFired = false;
            if (mission == null) return;
            foreach (var obj in mission.Objectives)
                obj.Reset();
        }

        /// <summary>
        /// Apply the active mission's starting-resource overrides to player 0.
        /// Call once right after the mission's game state is created.
        /// </summary>
        public void ApplyStartingResources()
        {
            if (_activeMission?.StartingResources == null) return;
            if (!_state.PlayerResources.TryGetValue(0, out var res)) return;
            foreach (var kv in _activeMission.StartingResources)
                res.Set(kv.Key, kv.Value);
        }

        /// <summary>Find a mission by ID. Returns null if not found.</summary>
        public static Mission Find(string id)
        {
            foreach (var m in AllMissions)
                if (m.Id == id) return m;
            return null;
        }

        /// <summary>
        /// Determine which missions are playable given current progress.
        /// The first mission is always unlocked. Subsequent ones unlock when their predecessor completes.
        /// </summary>
        public static List<Mission> GetUnlocked(CampaignProgress progress)
        {
            var result = new List<Mission>();
            foreach (var m in AllMissions)
            {
                bool isFirst = m == AllMissions[0];
                bool prevDone = false;
                foreach (var other in AllMissions)
                    if (other.UnlocksNext == m.Id && progress.IsCompleted(other.Id))
                        prevDone = true;
                if (isFirst || prevDone) result.Add(m);
            }
            return result;
        }

        /// <summary>Called each game tick — checks if special objectives are satisfied.</summary>
        public void Tick()
        {
            if (_activeMission == null || _completeFired) return;
            bool allDone = true;
            foreach (var obj in _activeMission.Objectives)
            {
                if (obj.IsComplete) continue;
                if (EvaluateObjective(obj)) obj.Complete();
                if (!obj.IsComplete) allDone = false;
            }
            if (allDone && _activeMission.Objectives.Length > 0)
            {
                _completeFired = true; // fire once, not every tick
                OnObjectivesComplete?.Invoke(_activeMission);
            }
        }

        private bool EvaluateObjective(MissionObjective obj)
        {
            var res = _state.PlayerResources.TryGetValue(0, out var playerRes) ? playerRes : null;
            switch (obj.Type)
            {
                case MissionObjectiveType.ReachVPCount:
                    return _state.Victory.GetVPCount(0) >= obj.TargetAmount;

                case MissionObjectiveType.ConquerSectors:
                    int owned = 0;
                    for (int i = 0; i < _state.Graph.SectorCount; i++)
                        if (_state.Graph.GetSector(i).OwnerId == 0) owned++;
                    return owned >= obj.TargetAmount;

                case MissionObjectiveType.ProduceResource:
                    if (res == null || obj.TargetParam == null) return false;
                    if (!Enum.TryParse<ResourceType>(obj.TargetParam, out var rt)) return false;
                    return res.Get(rt) >= obj.TargetAmount;

                case MissionObjectiveType.SurviveTime:
                    return _state.SimulationTime >= obj.TargetAmount;

                case MissionObjectiveType.BuildBuilding:
                    if (obj.TargetParam == null) return false;
                    if (!Enum.TryParse<BaseBuildingType>(obj.TargetParam, out var bt))
                        return false;
                    int built = 0;
                    foreach (var b in _state.Construction.GetBuildingsByPlayer(0))
                        if (b.Type == bt && b.IsOperational) built++;
                    return built >= obj.TargetAmount;

                case MissionObjectiveType.DefendSector:
                    // TargetParam = sector id; hold it until TargetAmount seconds pass
                    if (obj.TargetParam == null || !int.TryParse(obj.TargetParam, out var sid))
                        return false;
                    if (sid < 0 || sid >= _state.Graph.SectorCount) return false;
                    return _state.SimulationTime >= obj.TargetAmount
                        && _state.Graph.GetSector(sid).OwnerId == 0;

                default:
                    return false;
            }
        }

        // Mission catalogue (BuildCatalogue) → CampaignSystem.Missions.cs
    }
}
