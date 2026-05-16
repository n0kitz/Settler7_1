namespace Settlers.Simulation
{
    /// <summary>
    /// Calculates a final match score from outcome data.
    /// Formula: victory bonus + speed bonus + conquest bonus + tech bonus.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class ScoreCalculator
    {
        private const int VICTORY_BONUS      = 1000;
        private const int SPEED_BONUS_MAX    = 500;   // awarded if won < 10 minutes
        private const int SPEED_THRESHOLD_S  = 600;   // 10 minutes in seconds
        private const int PER_SECTOR         = 50;
        private const int PER_TECH           = 30;

        /// <summary>
        /// Calculates final score. Loser scores a proportional fraction based on
        /// sectors and techs only (no victory or speed bonuses).
        /// </summary>
        public static int Calculate(bool playerWon, float durationSeconds,
            int sectorsConquered, int techsResearched)
        {
            int score = sectorsConquered * PER_SECTOR + techsResearched * PER_TECH;

            if (!playerWon) return score;

            score += VICTORY_BONUS;

            // Speed bonus: interpolated from 0 (≥10 min) to SPEED_BONUS_MAX (<1 min)
            if (durationSeconds < SPEED_THRESHOLD_S)
            {
                float ratio = 1f - (durationSeconds / SPEED_THRESHOLD_S);
                score += (int)(SPEED_BONUS_MAX * ratio);
            }

            return score;
        }
    }
}
