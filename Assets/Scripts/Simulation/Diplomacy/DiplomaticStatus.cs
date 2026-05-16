namespace Settlers.Simulation
{
    /// <summary>Pairwise diplomatic standing between two players.</summary>
    public enum DiplomaticStatus
    {
        /// <summary>Default: neutral co-existence, no treaties.</summary>
        Peace,
        /// <summary>Both sides agree not to attack each other.</summary>
        NonAggression,
        /// <summary>Formal alliance — treated as friendly.</summary>
        Alliance,
        /// <summary>Active state of war — all attacks permitted.</summary>
        War,
    }

    /// <summary>Helper queries on DiplomaticStatus.</summary>
    public static class DiplomaticStatusExtensions
    {
        /// <summary>Returns true if the attacker may launch a military assault on the target.</summary>
        public static bool AllowsAttack(this DiplomaticStatus status)
            => status == DiplomaticStatus.War || status == DiplomaticStatus.Peace;

        /// <summary>Returns a short display string.</summary>
        public static string ToDisplayString(this DiplomaticStatus status)
        {
            switch (status)
            {
                case DiplomaticStatus.NonAggression: return "Non-Aggression";
                case DiplomaticStatus.Alliance:      return "Alliance";
                case DiplomaticStatus.War:           return "WAR";
                default:                             return "Peace";
            }
        }
    }
}
