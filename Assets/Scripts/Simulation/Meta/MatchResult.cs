using System;

namespace Settlers.Simulation
{
    /// <summary>Immutable record of a completed match's outcome and stats.</summary>
    public sealed class MatchResult
    {
        public readonly string MapId;
        public readonly int    WinnerId;
        public readonly float  DurationSeconds;
        public readonly int    PlayerCount;
        public readonly int    VPRequired;
        public readonly int    BuildingsBuilt;
        public readonly int    SectorsConquered;
        public readonly int    TechsResearched;
        public readonly int    TradesCompleted;
        public readonly DateTime PlayedAt;
        public readonly int    Score;

        public MatchResult(string mapId, int winnerId, float duration,
            int playerCount, int vpRequired,
            int buildingsBuilt, int sectorsConquered,
            int techsResearched, int tradesCompleted,
            int score, DateTime playedAt)
        {
            MapId            = mapId;
            WinnerId         = winnerId;
            DurationSeconds  = duration;
            PlayerCount      = playerCount;
            VPRequired       = vpRequired;
            BuildingsBuilt   = buildingsBuilt;
            SectorsConquered = sectorsConquered;
            TechsResearched  = techsResearched;
            TradesCompleted  = tradesCompleted;
            Score            = score;
            PlayedAt         = playedAt;
        }

        /// <summary>Build a MatchResult from live game state + accumulated stats.</summary>
        public static MatchResult From(GameState state, PlayerStats stats,
            int winnerId, float durationSeconds)
        {
            int score = ScoreCalculator.Calculate(
                winnerId == 0, durationSeconds,
                stats.SectorsConquered, stats.TechsResearched);

            return new MatchResult(
                state.MapId, winnerId, durationSeconds,
                state.PlayerCount, state.Victory.VPRequired,
                stats.BuildingsBuilt, stats.SectorsConquered,
                stats.TechsResearched, stats.TradesCompleted,
                score, DateTime.UtcNow);
        }
    }
}
