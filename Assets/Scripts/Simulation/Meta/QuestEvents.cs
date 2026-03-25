namespace Settlers.Simulation
{
    public readonly struct QuestAcceptedEvent
    {
        public readonly int PlayerId;
        public readonly string QuestId;
        public QuestAcceptedEvent(int playerId, string questId)
        { PlayerId = playerId; QuestId = questId; }
    }

    public readonly struct QuestCompletedEvent
    {
        public readonly int PlayerId;
        public readonly string QuestId;
        public QuestCompletedEvent(int playerId, string questId)
        { PlayerId = playerId; QuestId = questId; }
    }
}
