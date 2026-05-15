namespace Settlers.Simulation
{
    /// <summary>
    /// Full configuration bundle for a skirmish or campaign mission.
    /// Combines starting resources, victory path constraints, and game length.
    /// Passed into GameState on construction.
    /// </summary>
    public sealed class GameRules
    {
        public readonly StartingProfile StartingResources;
        public readonly VictoryRuleSet VictoryPaths;

        /// <summary>
        /// When true, bribery conquest is disabled for all players.
        /// </summary>
        public readonly bool BriberyDisabled;

        /// <summary>
        /// When true, proselytism conquest is disabled for all players.
        /// </summary>
        public readonly bool ProselytismDisabled;

        /// <summary>
        /// When true, prestige unlocks are disabled at game start.
        /// Players must earn them in-game.
        /// </summary>
        public readonly bool PrestigeUnlocksDisabled;

        public GameRules(
            StartingProfile startingResources = null,
            VictoryRuleSet victoryPaths = null,
            bool briberyDisabled = false,
            bool proselytismDisabled = false,
            bool prestigeUnlocksDisabled = false)
        {
            StartingResources = startingResources ?? StartingProfile.Default;
            VictoryPaths = victoryPaths ?? VictoryRuleSet.Standard;
            BriberyDisabled = briberyDisabled;
            ProselytismDisabled = proselytismDisabled;
            PrestigeUnlocksDisabled = prestigeUnlocksDisabled;
        }

        public static readonly GameRules Default = new GameRules();

        public static readonly GameRules QuickPlay = new GameRules(
            startingResources: StartingProfile.Rich,
            victoryPaths: VictoryRuleSet.Standard);

        public static readonly GameRules ConquestMatch = new GameRules(
            startingResources: StartingProfile.Default,
            victoryPaths: VictoryRuleSet.ConquestOnly,
            briberyDisabled: true,
            proselytismDisabled: true);

        public static readonly GameRules PeacefulTrade = new GameRules(
            startingResources: StartingProfile.Rich,
            victoryPaths: VictoryRuleSet.TradeOnly,
            briberyDisabled: false,
            proselytismDisabled: false);
    }
}
