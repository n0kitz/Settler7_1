using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Subscribes to EventBus events for player actions and appends ActionRecords
    /// to a growing log. Attach to a GameState's EventBus at match start.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public sealed class ActionRecorder
    {
        private readonly EventBus _bus;
        private readonly List<ActionRecord> _log = new List<ActionRecord>();
        private float _elapsed;

        public IReadOnlyList<ActionRecord> Log => _log;

        public ActionRecorder(EventBus bus)
        {
            _bus = bus;
            Subscribe();
        }

        /// <summary>Call each simulation tick to advance the internal clock.</summary>
        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
        }

        /// <summary>Returns a snapshot of the current log and resets it.</summary>
        public List<ActionRecord> TakeSnapshot()
        {
            var snapshot = new List<ActionRecord>(_log);
            _log.Clear();
            _elapsed = 0f;
            return snapshot;
        }

        private void Subscribe()
        {
            _bus.Subscribe<BuildingPlacedEvent>(e =>
                Record(e.SectorId < 0 ? -1 : 0, ActionRecord.PLACE_BUILDING,
                    $"sector={e.SectorId};type={e.BuildingType}"));

            _bus.Subscribe<TechResearchedEvent>(e =>
                Record(0, ActionRecord.RESEARCH_TECH, $"tech={e.TechId}"));

            _bus.Subscribe<ArmyArrivedEvent>(e =>
                Record(0, ActionRecord.MOVE_ARMY,
                    $"general={e.GeneralId};target={e.SectorId}"));

            _bus.Subscribe<GeneralHiredEvent>(e =>
                Record(e.OwnerId, ActionRecord.HIRE_GENERAL,
                    $"general={e.GeneralId}"));

            _bus.Subscribe<SectorConqueredEvent>(e =>
                Record(e.NewOwnerId, ActionRecord.CONQUER_SECTOR,
                    $"sector={e.SectorId};method={e.Method}"));

            _bus.Subscribe<FortificationBuiltEvent>(e =>
                Record(0, ActionRecord.FORTIFY_SECTOR,
                    $"sector={e.SectorId}"));

            _bus.Subscribe<OutpostClaimedEvent>(e =>
                Record(e.PlayerId, ActionRecord.CLAIM_OUTPOST,
                    $"outpost={e.OutpostId}"));
        }

        private void Record(int playerId, string actionType, string payload)
        {
            _log.Add(new ActionRecord(_elapsed, playerId, actionType, payload));
        }
    }
}
