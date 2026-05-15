namespace Settlers.Simulation
{
    public enum VictoryRuleSetType
    {
        Standard,       // All VP paths active
        ConquestOnly,   // Only military VPs count
        TradeOnly,      // Only trade VPs count
        TechOnly,       // Only technology VPs count
        NoConquest,     // Military conquest VPs disabled
    }

    /// <summary>
    /// Determines which VP paths are active during a match.
    /// VictorySystem checks IsPathAllowed before awarding path-specific VPs.
    /// </summary>
    public sealed class VictoryRuleSet
    {
        public readonly VictoryRuleSetType Type;

        public readonly bool MilitaryVPsEnabled;
        public readonly bool TechVPsEnabled;
        public readonly bool TradeVPsEnabled;
        public readonly bool PrestigeVPsEnabled;

        private VictoryRuleSet(VictoryRuleSetType type,
            bool military, bool tech, bool trade, bool prestige)
        {
            Type = type;
            MilitaryVPsEnabled = military;
            TechVPsEnabled = tech;
            TradeVPsEnabled = trade;
            PrestigeVPsEnabled = prestige;
        }

        public static readonly VictoryRuleSet Standard =
            new VictoryRuleSet(VictoryRuleSetType.Standard, true, true, true, true);

        public static readonly VictoryRuleSet ConquestOnly =
            new VictoryRuleSet(VictoryRuleSetType.ConquestOnly, true, false, false, false);

        public static readonly VictoryRuleSet TradeOnly =
            new VictoryRuleSet(VictoryRuleSetType.TradeOnly, false, false, true, false);

        public static readonly VictoryRuleSet TechOnly =
            new VictoryRuleSet(VictoryRuleSetType.TechOnly, false, true, false, false);

        public static readonly VictoryRuleSet NoConquest =
            new VictoryRuleSet(VictoryRuleSetType.NoConquest, false, true, true, true);

        public static VictoryRuleSet Get(VictoryRuleSetType type) => type switch
        {
            VictoryRuleSetType.ConquestOnly => ConquestOnly,
            VictoryRuleSetType.TradeOnly    => TradeOnly,
            VictoryRuleSetType.TechOnly     => TechOnly,
            VictoryRuleSetType.NoConquest   => NoConquest,
            _                               => Standard
        };
    }
}
