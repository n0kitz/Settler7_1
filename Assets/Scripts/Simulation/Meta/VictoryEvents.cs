namespace Settlers.Simulation
{
    public readonly struct VPChangedEvent
    {
        public readonly int PlayerId;
        public readonly string VPId;
        public readonly bool Gained;

        public VPChangedEvent(int playerId, string vpId, bool gained)
        {
            PlayerId = playerId;
            VPId = vpId;
            Gained = gained;
        }
    }

    public readonly struct CountdownStartedEvent
    {
        public readonly int PlayerId;
        public readonly float Duration;

        public CountdownStartedEvent(int playerId, float duration)
        {
            PlayerId = playerId;
            Duration = duration;
        }
    }

    public readonly struct CountdownCancelledEvent { }

    public readonly struct GameOverEvent
    {
        public readonly int WinnerId;
        public GameOverEvent(int winnerId) { WinnerId = winnerId; }
    }
}
