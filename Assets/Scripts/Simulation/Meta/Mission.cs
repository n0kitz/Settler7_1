using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Definition of a single campaign mission.
    /// Contains map config, starting overrides, objectives, and narrative text.
    /// Immutable — created from CampaignSystem.AllMissions.
    /// </summary>
    public sealed class Mission
    {
        /// <summary>Unique string identifier used for save/unlock tracking.</summary>
        public readonly string Id;

        /// <summary>Display name shown in the mission selection screen.</summary>
        public readonly string Title;

        /// <summary>Pre-mission story text shown in the briefing screen.</summary>
        public readonly string Briefing;

        /// <summary>Map ID passed to MapFactory.CreateMap().</summary>
        public readonly string MapId;

        /// <summary>Total player slots (including AI opponents).</summary>
        public readonly int PlayerCount;

        /// <summary>VPs needed to trigger the 3-minute countdown.</summary>
        public readonly int VPRequired;

        /// <summary>Special objectives beyond the standard VP win condition.</summary>
        public readonly MissionObjective[] Objectives;

        /// <summary>
        /// Starting resource overrides for player 0.
        /// Null entries fall back to the GameState defaults.
        /// </summary>
        public readonly Dictionary<ResourceType, int> StartingResources;

        /// <summary>Mission ID that becomes available after this mission is completed.</summary>
        public readonly string UnlocksNext;

        /// <summary>Chapter index (0-based) for grouping in the UI.</summary>
        public readonly int Chapter;

        public Mission(string id, string title, string briefing, string mapId,
            int playerCount, int vpRequired, MissionObjective[] objectives,
            Dictionary<ResourceType, int> startingResources = null,
            string unlocksNext = null, int chapter = 0)
        {
            Id = id;
            Title = title;
            Briefing = briefing;
            MapId = mapId;
            PlayerCount = playerCount;
            VPRequired = vpRequired;
            Objectives = objectives ?? System.Array.Empty<MissionObjective>();
            StartingResources = startingResources;
            UnlocksNext = unlocksNext;
            Chapter = chapter;
        }
    }
}
