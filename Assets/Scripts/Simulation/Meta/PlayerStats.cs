namespace Settlers.Simulation
{
    /// <summary>
    /// Accumulates per-game statistics for player 0 by subscribing to EventBus events.
    /// Reset between games via Reset(). Pure C# — no UnityEngine references.
    /// </summary>
    public class PlayerStats
    {
        public int BuildingsBuilt      { get; private set; }
        public int SectorsConquered    { get; private set; }
        public int TechsResearched     { get; private set; }
        public int VPsGained           { get; private set; }
        public int OutpostsClaimed     { get; private set; }
        public int TradesCompleted     { get; private set; }
        public int QuestsCompleted     { get; private set; }
        public int GeneralsHired       { get; private set; }

        public void Initialize(EventBus bus)
        {
            bus.Subscribe<BuildingCompletedEvent>(_ => BuildingsBuilt++);
            bus.Subscribe<SectorConqueredEvent>(e =>
                { if (e.NewOwnerId == 0) SectorsConquered++; });
            bus.Subscribe<TechResearchedEvent>(e =>
                { if (e.PlayerId == 0) TechsResearched++; });
            bus.Subscribe<VPChangedEvent>(e =>
                { if (e.PlayerId == 0 && e.Gained) VPsGained++; });
            bus.Subscribe<OutpostClaimedEvent>(e =>
                { if (e.PlayerId == 0) OutpostsClaimed++; });
            bus.Subscribe<TradeExecutedEvent>(e =>
                { if (e.PlayerId == 0) TradesCompleted++; });
            bus.Subscribe<QuestCompletedEvent>(e =>
                { if (e.PlayerId == 0) QuestsCompleted++; });
            bus.Subscribe<GeneralHiredEvent>(e =>
                { if (e.PlayerId == 0) GeneralsHired++; });
        }

        public void Reset()
        {
            BuildingsBuilt   = 0;
            SectorsConquered = 0;
            TechsResearched  = 0;
            VPsGained        = 0;
            OutpostsClaimed  = 0;
            TradesCompleted  = 0;
            QuestsCompleted  = 0;
            GeneralsHired    = 0;
        }
    }
}
