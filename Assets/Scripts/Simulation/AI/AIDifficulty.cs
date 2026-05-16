namespace Settlers.Simulation
{
    public enum AIDifficultyLevel { Easy, Normal, Hard }

    /// <summary>
    /// Timing and threshold settings that scale AI challenge.
    /// Easy reacts slowly and attacks late; Hard reacts fast with bonus starting resources.
    /// </summary>
    public sealed class AIDifficulty
    {
        public readonly AIDifficultyLevel Level;

        /// <summary>Seconds between AI decision ticks.</summary>
        public readonly float DecisionInterval;

        /// <summary>Seconds of stall before the AI reconsiders its victory path.</summary>
        public readonly float StallDuration;

        /// <summary>Minimum soldiers a general needs before attacking.</summary>
        public readonly int AttackThreshold;

        /// <summary>Bonus coins added to the AI's starting resources.</summary>
        public readonly int StartingBonusCoins;

        /// <summary>Bonus planks added to the AI's starting resources.</summary>
        public readonly int StartingBonusPlanks;

        private AIDifficulty(AIDifficultyLevel level,
            float interval, float stall, int attack, int coins, int planks)
        {
            Level = level;
            DecisionInterval = interval;
            StallDuration = stall;
            AttackThreshold = attack;
            StartingBonusCoins = coins;
            StartingBonusPlanks = planks;
        }

        public static readonly AIDifficulty Easy =
            new AIDifficulty(AIDifficultyLevel.Easy, 8f, 60f, 12, 0, 0);

        public static readonly AIDifficulty Normal =
            new AIDifficulty(AIDifficultyLevel.Normal, 5f, 30f, 8, 0, 0);

        public static readonly AIDifficulty Hard =
            new AIDifficulty(AIDifficultyLevel.Hard, 3f, 20f, 5, 10, 5);

        public static AIDifficulty Get(AIDifficultyLevel level) => level switch
        {
            AIDifficultyLevel.Easy => Easy,
            AIDifficultyLevel.Hard => Hard,
            _                      => Normal
        };
    }
}
