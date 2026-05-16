using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class ProselytismTests
    {
        private GameState _state;

        [SetUp]
        public void SetUp()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph = TestMapFactory.CreateSixSectorMap();
            _state = new GameState(graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test_valley");
        }

        // ---- Cleric count requirements ----

        [Test]
        public void StartProselytism_UnfortifiedSector_RequiresSixClerics()
        {
            // Sector 2 is neutral, unfortified
            bool tooFew = _state.Conquest.StartProselytism(0, 2, clericCount: 5);
            Assert.IsFalse(tooFew, "5 clerics must be insufficient for unfortified sector");

            bool enough = _state.Conquest.StartProselytism(0, 2, clericCount: 6);
            Assert.IsTrue(enough, "6 clerics must start proselytism on unfortified sector");
        }

        [Test]
        public void StartProselytism_FortifiedSector_RequiresTwelveClerics()
        {
            // Sector 3 is neutral, fortified
            bool tooFew = _state.Conquest.StartProselytism(0, 3, clericCount: 6);
            Assert.IsFalse(tooFew, "6 clerics must be insufficient for fortified sector");

            bool enough = _state.Conquest.StartProselytism(0, 3, clericCount: 12);
            Assert.IsTrue(enough, "12 clerics must start proselytism on fortified sector");
        }

        [Test]
        public void StartProselytism_PlayerOwnedSector_Fails()
        {
            // Sector 0 owned by player 0 — cannot proselytize own or enemy sector
            bool result = _state.Conquest.StartProselytism(1, 0, clericCount: 6);
            Assert.IsFalse(result, "Cannot proselytize a player-owned sector");
        }

        [Test]
        public void StartProselytism_CreatesActiveTask()
        {
            _state.Conquest.StartProselytism(0, 2, clericCount: 6);
            Assert.AreEqual(1, _state.Conquest.ProselytismTasks.Count);
            Assert.AreEqual(2, _state.Conquest.ProselytismTasks[0].SectorId);
            Assert.AreEqual(0, _state.Conquest.ProselytismTasks[0].PlayerId);
        }

        // ---- Tick advancement ----

        [Test]
        public void Tick_AdvancesProgress()
        {
            _state.Conquest.StartProselytism(0, 2, clericCount: 6);
            float durationBefore = _state.Conquest.ProselytismTasks[0].Duration;

            _state.Conquest.Tick(10f);

            float progress = _state.Conquest.ProselytismTasks[0].Progress;
            Assert.Greater(progress, 0f, "Progress must advance after Tick");
            Assert.Less(progress, 1f, "Task must not be complete after partial tick");
        }

        [Test]
        public void Tick_CompletesConversion_FlipsSectorOwner()
        {
            // Sector 4: neutral, unfortified — player 0 sends 6 clerics, duration 30s
            _state.Conquest.StartProselytism(0, 4, clericCount: 6);

            _state.Conquest.Tick(30f); // Full duration

            Assert.AreEqual(0, _state.Graph.GetSector(4).OwnerId,
                "Sector must be owned by player 0 after proselytism completes");
            Assert.AreEqual(0, _state.Conquest.ProselytismTasks.Count,
                "Completed task must be removed from queue");
        }

        [Test]
        public void Tick_FortifiedSector_CompletesWithTwelveClerics()
        {
            // Sector 3: neutral, fortified — requires 12 clerics
            _state.Conquest.StartProselytism(0, 3, clericCount: 12);

            _state.Conquest.Tick(30f);

            Assert.AreEqual(0, _state.Graph.GetSector(3).OwnerId,
                "Fortified sector must convert after 30s with 12 clerics");
        }

        [Test]
        public void Tick_CompletesConversion_AwardsPrestige()
        {
            int prestigeBefore = _state.Prestige.GetPoints(0);

            _state.Conquest.StartProselytism(0, 4, clericCount: 6);
            _state.Conquest.Tick(30f);

            Assert.AreEqual(prestigeBefore + 1, _state.Prestige.GetPoints(0),
                "Proselytism conquest must award 1 prestige point");
        }

        [Test]
        public void Tick_CompletesConversion_PublishesSectorConqueredEvent()
        {
            bool eventFired = false;
            ConquestMethod capturedMethod = ConquestMethod.Military;

            _state.Events.Subscribe<SectorConqueredEvent>(evt =>
            {
                if (evt.SectorId == 2)
                {
                    eventFired = true;
                    capturedMethod = evt.Method;
                }
            });

            _state.Conquest.StartProselytism(0, 2, clericCount: 6);
            _state.Conquest.Tick(30f);

            Assert.IsTrue(eventFired, "SectorConqueredEvent must fire on proselytism completion");
            Assert.AreEqual(ConquestMethod.Proselytism, capturedMethod,
                "Event method must be Proselytism");
        }

        [Test]
        public void Tick_IncrementalTicks_CompleteAfterFullDuration()
        {
            // 3 ticks of 10s each = 30s total = complete
            _state.Conquest.StartProselytism(0, 2, clericCount: 6);

            _state.Conquest.Tick(10f);
            Assert.AreEqual(1, _state.Conquest.ProselytismTasks.Count, "Still active after 10s");

            _state.Conquest.Tick(10f);
            Assert.AreEqual(1, _state.Conquest.ProselytismTasks.Count, "Still active after 20s");

            _state.Conquest.Tick(10f);
            Assert.AreEqual(0, _state.Conquest.ProselytismTasks.Count, "Complete after 30s");
            Assert.AreEqual(0, _state.Graph.GetSector(2).OwnerId);
        }

        [Test]
        public void Tick_SectorTakenByOther_DoesNotOverwrite()
        {
            // Player 0 starts proselytism on sector 2
            _state.Conquest.StartProselytism(0, 2, clericCount: 6);

            // Before tick completes, player 1 conquers sector 2 militarily
            _state.Graph.GetSector(2).SetOwner(1);

            // Tick to completion
            _state.Conquest.Tick(30f);

            // Sector 2 must still be player 1's — proselytism must not overwrite
            Assert.AreEqual(1, _state.Graph.GetSector(2).OwnerId,
                "Proselytism must not overwrite sector already taken by another player");
        }
    }
}
