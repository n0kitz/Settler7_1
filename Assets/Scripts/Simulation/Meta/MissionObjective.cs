namespace Settlers.Simulation
{
    /// <summary>The type of condition that completes a mission objective.</summary>
    public enum MissionObjectiveType
    {
        ReachVPCount,    // Accumulate N victory points
        ConquerSectors,  // Own at least N sectors (excluding start)
        BuildBuilding,   // Place N buildings of a given type
        ProduceResource, // Accumulate N of a resource in storehouse
        DefendSector,    // Retain ownership of sector by map ID for N seconds (checked on win)
        SurviveTime,     // Reach N seconds of simulation time without losing
    }

    /// <summary>
    /// A single checkable objective within a campaign mission.
    /// Evaluated each tick by CampaignSystem against live GameState.
    /// Immutable data — created at mission startup.
    /// </summary>
    public sealed class MissionObjective
    {
        /// <summary>Human-readable description shown in the mission briefing.</summary>
        public readonly string Description;

        /// <summary>How this objective is evaluated.</summary>
        public readonly MissionObjectiveType Type;

        /// <summary>Target quantity (VP count, sector count, resource amount, seconds).</summary>
        public readonly int TargetAmount;

        /// <summary>
        /// Type-specific extra parameter.
        /// BuildBuilding → BaseBuildingType name.
        /// ProduceResource → ResourceType name.
        /// </summary>
        public readonly string TargetParam;

        /// <summary>Whether this objective has been met during the current mission.</summary>
        public bool IsComplete { get; private set; }

        public MissionObjective(string description, MissionObjectiveType type,
            int targetAmount, string targetParam = null)
        {
            Description = description;
            Type = type;
            TargetAmount = targetAmount;
            TargetParam = targetParam;
        }

        /// <summary>Mark this objective as complete.</summary>
        internal void Complete() => IsComplete = true;

        /// <summary>Clear completion — the static catalogue is shared, so a
        /// replayed mission must start with fresh objectives.</summary>
        internal void Reset() => IsComplete = false;
    }
}
