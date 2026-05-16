using System;
using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Owns the campaign mission catalogue and evaluates mission objectives each tick.
    /// Create it from GameController when starting a campaign mission.
    /// Fires C# events consumed by the UI layer.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class CampaignSystem
    {
        // --- Events for presentation ---

        /// <summary>Fired when all special objectives for the active mission are met.</summary>
        public event Action<Mission> OnObjectivesComplete;

        // --- State ---

        private readonly GameState _state;
        private Mission _activeMission;

        public Mission ActiveMission => _activeMission;

        // --- Static catalogue ---

        /// <summary>All campaign missions in play order.</summary>
        public static IReadOnlyList<Mission> AllMissions { get; } = BuildCatalogue();

        public CampaignSystem(GameState state)
        {
            _state = state;
        }

        /// <summary>Set which mission is currently being played.</summary>
        public void SetActiveMission(Mission mission)
        {
            _activeMission = mission;
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
            if (_activeMission == null) return;
            bool allDone = true;
            foreach (var obj in _activeMission.Objectives)
            {
                if (obj.IsComplete) continue;
                if (EvaluateObjective(obj)) obj.Complete();
                if (!obj.IsComplete) allDone = false;
            }
            if (allDone && _activeMission.Objectives.Length > 0)
                OnObjectivesComplete?.Invoke(_activeMission);
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

                default:
                    return false;
            }
        }

        // --- Mission catalogue ---

        private static List<Mission> BuildCatalogue() => new List<Mission>
        {
            // Chapter 1: Learning the Basics
            new Mission(
                id: "c1_m1_first_steps",
                title: "First Steps",
                briefing: "My lord, our people have settled in the valley. We must build our economy " +
                          "quickly — rival lords eye our lands. Establish a Lodge and Farm, then " +
                          "expand to the neutral sectors nearby.",
                mapId: "test_valley",
                playerCount: 1,
                vpRequired: 2,
                objectives: new[]
                {
                    new MissionObjective("Reach 2 Victory Points", MissionObjectiveType.ReachVPCount, 2),
                    new MissionObjective("Own at least 3 sectors", MissionObjectiveType.ConquerSectors, 3),
                },
                unlocksNext: "c1_m2_iron_fist",
                chapter: 0),

            new Mission(
                id: "c1_m2_iron_fist",
                title: "Iron Fist",
                briefing: "The Iron Ridge holds resources vital to our military ambitions. " +
                          "An eastern lord has garrisoned the ridge with soldiers. " +
                          "Train your army and crush his forces. The iron belongs to us!",
                mapId: "twin_rivers",
                playerCount: 2,
                vpRequired: 4,
                objectives: new[]
                {
                    new MissionObjective("Conquer 5 sectors", MissionObjectiveType.ConquerSectors, 5),
                    new MissionObjective("Stockpile 20 Iron Bars", MissionObjectiveType.ProduceResource, 20,
                        nameof(ResourceType.IronBars)),
                },
                unlocksNext: "c1_m3_mountain_lord",
                chapter: 0),

            new Mission(
                id: "c1_m3_mountain_lord",
                title: "Lord of the Mountain",
                briefing: "Three lords vie for the Central Stronghold. He who holds it commands the pass " +
                          "and wins prestige across the realm. Take the stronghold by any means necessary.",
                mapId: "mountain_pass",
                playerCount: 3,
                vpRequired: 5,
                objectives: new[]
                {
                    new MissionObjective("Reach 5 Victory Points", MissionObjectiveType.ReachVPCount, 5),
                    new MissionObjective("Own the Central Stronghold (7 sectors)", MissionObjectiveType.ConquerSectors, 7),
                },
                unlocksNext: "c2_m1_sea_roads",
                chapter: 0),

            // Chapter 2: Trade & Technology
            new Mission(
                id: "c2_m1_sea_roads",
                title: "Sea Roads",
                briefing: "The island chain is a vital trade corridor. Control its outposts and your " +
                          "merchants will profit handsomely. But a rival fleet already claims the eastern isle!",
                mapId: "island_chain",
                playerCount: 2,
                vpRequired: 4,
                objectives: new[]
                {
                    new MissionObjective("Reach 4 Victory Points", MissionObjectiveType.ReachVPCount, 4),
                    new MissionObjective("Accumulate 30 Coins", MissionObjectiveType.ProduceResource, 30,
                        nameof(ResourceType.Coins)),
                },
                unlocksNext: "c2_m2_scholars_duel",
                chapter: 1),

            new Mission(
                id: "c2_m2_scholars_duel",
                title: "The Scholars' Duel",
                briefing: "Three great academies stand empty in the large valley. " +
                          "He who researches the most technologies will be known as the wisest lord. " +
                          "Your rivals have sent their clerics — do not let them claim every monastery!",
                mapId: "large_valley",
                playerCount: 3,
                vpRequired: 5,
                objectives: new[]
                {
                    new MissionObjective("Reach 5 Victory Points", MissionObjectiveType.ReachVPCount, 5),
                },
                unlocksNext: "c3_m1_crown_war",
                chapter: 1),

            // Chapter 3: Conquest
            new Mission(
                id: "c3_m1_crown_war",
                title: "The Crown War",
                briefing: "Four noble houses stand at the brink of all-out war. Only one can wear the " +
                          "crown. Forge your empire through blood, trade, or wisdom — but win you must. " +
                          "The realm watches.",
                mapId: "crown_war",
                playerCount: 4,
                vpRequired: 5,
                objectives: new[]
                {
                    new MissionObjective("Reach 5 Victory Points", MissionObjectiveType.ReachVPCount, 5),
                },
                unlocksNext: "c3_m2_empire",
                chapter: 2),

            new Mission(
                id: "c3_m2_empire",
                title: "Empire",
                briefing: "This is the final battle for dominion over the entire realm. " +
                          "24 sectors, 4 lords, and only one throne. Conquer, trade, or out-think your " +
                          "rivals. The empire is yours to claim — if you are worthy.",
                mapId: "empire",
                playerCount: 4,
                vpRequired: 6,
                objectives: new[]
                {
                    new MissionObjective("Reach 6 Victory Points", MissionObjectiveType.ReachVPCount, 6),
                    new MissionObjective("Survive 600 seconds", MissionObjectiveType.SurviveTime, 600),
                },
                chapter: 2),
        };
    }
}
