namespace Settlers.Simulation
{
    /// <summary>Types of diplomatic proposals a player can send.</summary>
    public enum DiplomaticActionType
    {
        ProposeNonAggression,
        ProposeAlliance,
        OfferGift,         // transfers _GIFT_COINS coins from sender to receiver
        DemandTribute,     // AI-only: demands coins from target
        DeclareWar,        // unilateral; always succeeds
    }

    /// <summary>A discrete diplomatic action from one player to another.</summary>
    public readonly struct DiplomaticAction
    {
        public readonly int FromPlayerId;
        public readonly int ToPlayerId;
        public readonly DiplomaticActionType Type;
        public readonly int Amount;   // coins for OfferGift / DemandTribute

        public DiplomaticAction(int from, int to,
            DiplomaticActionType type, int amount = 0)
        {
            FromPlayerId = from;
            ToPlayerId   = to;
            Type         = type;
            Amount       = amount;
        }
    }

    /// <summary>Fired when the diplomatic standing between two players changes.</summary>
    public readonly struct DiplomaticStatusChangedEvent
    {
        public readonly int PlayerA;
        public readonly int PlayerB;
        public readonly DiplomaticStatus OldStatus;
        public readonly DiplomaticStatus NewStatus;

        public DiplomaticStatusChangedEvent(int a, int b,
            DiplomaticStatus oldS, DiplomaticStatus newS)
        {
            PlayerA   = a;
            PlayerB   = b;
            OldStatus = oldS;
            NewStatus = newS;
        }
    }
}
