namespace Settlers.Simulation
{
    public enum AIPersonalityType { Builder, Warrior, Merchant }

    /// <summary>
    /// Behavior weights that bias an AI controller toward a particular victory path.
    /// Builder favors economy and tech; Warrior favors military; Merchant favors trade.
    /// </summary>
    public sealed class AIPersonality
    {
        public readonly AIPersonalityType Type;

        /// <summary>Score multiplier applied when evaluating the Technology path.</summary>
        public readonly float TechWeight;

        /// <summary>Score multiplier applied when evaluating the Military path.</summary>
        public readonly float MilitaryWeight;

        /// <summary>Score multiplier applied when evaluating the Trade path.</summary>
        public readonly float TradeWeight;

        /// <summary>Minimum operational buildings before leaving EarlyEconomy phase.</summary>
        public readonly int EarlyEconomyThreshold;

        private AIPersonality(AIPersonalityType type,
            float tech, float military, float trade, int earlyEconomy)
        {
            Type = type;
            TechWeight = tech;
            MilitaryWeight = military;
            TradeWeight = trade;
            EarlyEconomyThreshold = earlyEconomy;
        }

        public static readonly AIPersonality Builder =
            new AIPersonality(AIPersonalityType.Builder, 1.5f, 0.6f, 0.9f, 5);

        public static readonly AIPersonality Warrior =
            new AIPersonality(AIPersonalityType.Warrior, 0.6f, 2.0f, 0.5f, 3);

        public static readonly AIPersonality Merchant =
            new AIPersonality(AIPersonalityType.Merchant, 0.9f, 0.5f, 2.0f, 4);

        public static AIPersonality Get(AIPersonalityType type) => type switch
        {
            AIPersonalityType.Warrior  => Warrior,
            AIPersonalityType.Merchant => Merchant,
            _                          => Builder
        };
    }
}
