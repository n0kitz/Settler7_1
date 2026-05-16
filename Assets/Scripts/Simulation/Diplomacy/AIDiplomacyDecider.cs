namespace Settlers.Simulation
{
    /// <summary>
    /// Decides whether an AI player accepts or rejects a diplomatic proposal.
    /// Uses AIPersonality weights and the relative power balance.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class AIDiplomacyDecider
    {
        private const int GIFT_ACCEPTANCE_THRESHOLD = 0; // AI always accepts gifts

        /// <summary>
        /// Returns true if the AI (toId) accepts the proposal from (fromId).
        /// powerBalance: positive = AI is stronger, negative = AI is weaker.
        /// </summary>
        public static bool ShouldAccept(DiplomaticAction action,
            AIPersonality personality, int powerBalance)
        {
            switch (action.Type)
            {
                case DiplomaticActionType.ProposeNonAggression:
                    return AcceptsNonAggression(personality, powerBalance);

                case DiplomaticActionType.ProposeAlliance:
                    return AcceptsAlliance(personality, powerBalance);

                case DiplomaticActionType.OfferGift:
                    return true; // always accept gifts

                case DiplomaticActionType.DemandTribute:
                    // Only accept if much weaker and the amount is small
                    return powerBalance < -5;

                case DiplomaticActionType.DeclareWar:
                    return true; // can't refuse war declaration

                default:
                    return false;
            }
        }

        private static bool AcceptsNonAggression(AIPersonality p, int powerBalance)
        {
            // Builders always prefer peace
            if (p.MilitaryWeight < 0.8f) return true;
            // Warriors only accept if significantly weaker
            if (p.MilitaryWeight >= 1.5f) return powerBalance < -3;
            // Balanced: accept if not strongly winning
            return powerBalance < 2;
        }

        private static bool AcceptsAlliance(AIPersonality p, int powerBalance)
        {
            // Merchants readily form alliances for trade stability
            if (p.TradeWeight >= 1.5f) return true;
            // Warriors rarely ally unless desperate
            if (p.MilitaryWeight >= 1.5f) return powerBalance < -5;
            // Builders ally when roughly even or weaker
            return powerBalance <= 1;
        }

        /// <summary>
        /// Estimate power balance from perspective of AI player (aiId).
        /// Positive = AI has more sectors.
        /// </summary>
        public static int EstimatePowerBalance(GameState state, int aiId, int otherId)
        {
            int aiSectors    = state.Graph.GetSectorsOwnedBy(aiId).Count;
            int otherSectors = state.Graph.GetSectorsOwnedBy(otherId).Count;
            return aiSectors - otherSectors;
        }
    }
}
