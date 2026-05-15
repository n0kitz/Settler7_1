namespace Settlers.Simulation
{
    /// <summary>
    /// Combined AI personality and difficulty. Passed to AIController on construction.
    /// Personality biases the chosen victory path; difficulty governs timing and thresholds.
    /// </summary>
    public sealed class AIBehaviorProfile
    {
        public readonly AIPersonality Personality;
        public readonly AIDifficulty Difficulty;

        /// <summary>Display name shown in the HUD for this AI opponent.</summary>
        public readonly string DisplayName;

        public AIBehaviorProfile(AIPersonality personality, AIDifficulty difficulty,
            string displayName = null)
        {
            Personality = personality;
            Difficulty = difficulty;
            DisplayName = displayName
                ?? $"Lord {personality.Type} ({difficulty.Level})";
        }

        public static readonly AIBehaviorProfile Default =
            new AIBehaviorProfile(AIPersonality.Builder, AIDifficulty.Normal, "Lord Heinrich");

        /// <summary>Create a profile from enum values.</summary>
        public static AIBehaviorProfile Create(
            AIPersonalityType personality, AIDifficultyLevel difficulty)
        {
            return new AIBehaviorProfile(
                AIPersonality.Get(personality),
                AIDifficulty.Get(difficulty));
        }
    }
}
