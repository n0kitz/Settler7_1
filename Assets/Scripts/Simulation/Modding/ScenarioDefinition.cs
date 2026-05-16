namespace Settlers.Simulation
{
    /// <summary>
    /// A self-contained challenge: bundles a map ID, AI roster, victory rules, and
    /// starting profile. Loaded from *.scenario.json files in mod Scenarios/ folders.
    /// Pure C# data class — no UnityEngine references.
    /// </summary>
    public sealed class ScenarioDefinition
    {
        public string ScenarioId        { get; set; } = "";
        public string DisplayName       { get; set; } = "";
        public string Description       { get; set; } = "";
        public string MapId             { get; set; } = "";
        public int    PlayerCount       { get; set; } = 2;
        public int    VPRequired        { get; set; } = 4;
        public string StartingProfile   { get; set; } = "Default";
        public string VictoryRules      { get; set; } = "Standard";
        public string AIPersonality     { get; set; } = "Builder";
        public string AIDifficulty      { get; set; } = "Normal";
        public bool   IsModScenario     { get; set; } = false;
        public string ModId             { get; set; } = "";

        /// <summary>Resolve this scenario into a GameRules instance.</summary>
        public GameRules ToGameRules()
        {
            var profile = Simulation.StartingProfile.Get(
                ParseEnum(StartingProfile, StartingProfileType.Default));
            var rules = Simulation.VictoryRuleSet.Get(
                ParseEnum(VictoryRules, VictoryRuleSetType.Standard));
            return new GameRules(profile, rules);
        }

        /// <summary>Resolve the AI profile for this scenario.</summary>
        public AIBehaviorProfile ToAIProfile()
        {
            return AIBehaviorProfile.Create(
                ParseEnum(AIPersonality, AIPersonalityType.Builder),
                ParseEnum(AIDifficulty,  AIDifficultyLevel.Normal));
        }

        private static T ParseEnum<T>(string val, T fallback) where T : struct
        {
            return System.Enum.TryParse<T>(val, ignoreCase: true, out var result)
                ? result
                : fallback;
        }

        public override string ToString() => DisplayName;
    }
}
