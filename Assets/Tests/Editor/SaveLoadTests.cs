using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class SaveLoadTests
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

        [Test]
        public void Serialize_ContainsVersion()
        {
            string data = SaveSystem.Serialize(_state);
            Assert.IsTrue(data.Contains("version=1"));
        }

        [Test]
        public void Serialize_ContainsPlayerCount()
        {
            string data = SaveSystem.Serialize(_state);
            Assert.IsTrue(data.Contains("playerCount=2"));
        }

        [Test]
        public void Serialize_ContainsSectorOwnership()
        {
            string data = SaveSystem.Serialize(_state);
            Assert.IsTrue(data.Contains("sector.0.owner=0"));
            Assert.IsTrue(data.Contains("sector.1.owner=1"));
        }

        [Test]
        public void Serialize_ContainsResources()
        {
            _state.PlayerResources[0].Set(ResourceType.Planks, 42);
            string data = SaveSystem.Serialize(_state);
            Assert.IsTrue(data.Contains("res.0.Planks=42"));
        }

        [Test]
        public void Deserialize_ParsesKeyValuePairs()
        {
            string data = "version=1\nplayerCount=2\nres.0.Planks=42";
            var parsed = SaveSystem.Deserialize(data);

            Assert.AreEqual("1", parsed["version"]);
            Assert.AreEqual("2", parsed["playerCount"]);
            Assert.AreEqual("42", parsed["res.0.Planks"]);
        }

        [Test]
        public void Deserialize_IgnoresEmptyLines()
        {
            string data = "version=1\n\n\nplayerCount=2\n";
            var parsed = SaveSystem.Deserialize(data);
            Assert.AreEqual(2, parsed.Count);
        }

        [Test]
        public void RoundTrip_PreservesResources()
        {
            _state.PlayerResources[0].Set(ResourceType.Planks, 99);
            _state.PlayerResources[0].Set(ResourceType.IronBars, 15);
            _state.PlayerResources[1].Set(ResourceType.Coins, 33);

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            // Create fresh state and apply
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph2 = TestMapFactory.CreateSixSectorMap();
            var state2 = new GameState(graph2, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test_valley");

            SaveSystem.ApplyToState(state2, parsed);

            Assert.AreEqual(99, state2.PlayerResources[0].Get(ResourceType.Planks));
            Assert.AreEqual(15, state2.PlayerResources[0].Get(ResourceType.IronBars));
            Assert.AreEqual(33, state2.PlayerResources[1].Get(ResourceType.Coins));
        }

        [Test]
        public void RoundTrip_PreservesSectorOwnership()
        {
            // Conquer sector 2
            _state.Graph.GetSector(2).SetOwner(0);

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph2 = TestMapFactory.CreateSixSectorMap();
            var state2 = new GameState(graph2, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3);

            SaveSystem.ApplyToState(state2, parsed);

            Assert.AreEqual(0, state2.Graph.GetSector(2).OwnerId);
        }

        [Test]
        public void RoundTrip_PreservesPrestigePoints()
        {
            _state.Prestige.AwardPoints(0, 10);

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph2 = TestMapFactory.CreateSixSectorMap();
            var state2 = new GameState(graph2, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3);

            SaveSystem.ApplyToState(state2, parsed);

            Assert.AreEqual(10, state2.Prestige.GetPoints(0));
        }

        [Test]
        public void RoundTrip_PreservesSimulationTime()
        {
            _state.AdvanceTime(123.45f);

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph2 = TestMapFactory.CreateSixSectorMap();
            var state2 = new GameState(graph2, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3);

            SaveSystem.ApplyToState(state2, parsed);

            Assert.AreEqual(123.45f, state2.SimulationTime, 0.1f);
        }

        [Test]
        public void RoundTrip_PreservesFortification()
        {
            // Sector 3 starts fortified
            Assert.IsTrue(_state.Graph.GetSector(3).IsFortified);

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph2 = TestMapFactory.CreateSixSectorMap();
            var state2 = new GameState(graph2, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3);

            SaveSystem.ApplyToState(state2, parsed);

            Assert.IsTrue(state2.Graph.GetSector(3).IsFortified);
        }

        [Test]
        public void Serialize_EmptyState_DoesNotCrash()
        {
            string data = SaveSystem.Serialize(_state);
            Assert.IsNotNull(data);
            Assert.IsTrue(data.Length > 0);
        }

        // ---- NEW: army round-trips ----

        private GameState CreateFreshState()
        {
            ArmySystem.ResetIdCounter();
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph = TestMapFactory.CreateSixSectorMap();
            return new GameState(graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test_valley");
        }

        [Test]
        public void RoundTrip_PreservesGenerals()
        {
            // Give player 0 a general with 3 footsoldiers in sector 0
            var gen = _state.Army.HireGeneral(0, 0);
            gen.AddUnit(UnitType.Pikeman);
            gen.AddUnit(UnitType.Pikeman);
            gen.AddUnit(UnitType.Pikeman);

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            var state2 = CreateFreshState();
            SaveSystem.ApplyToState(state2, parsed);

            var generals2 = state2.Army.GetGenerals(0);
            Assert.AreEqual(1, generals2.Count);
            Assert.AreEqual(0, generals2[0].SectorId);
            Assert.AreEqual(3, generals2[0].GetUnitCount(UnitType.Pikeman));
        }

        [Test]
        public void RoundTrip_PreservesTrainingQueue()
        {
            // Player 0 has prestige unlock for basic units — skip prestige check via restore
            _state.Army.RestoreTrainingTask(0, 0, UnitType.Pikeman, 10f, 0.5f);

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            var state2 = CreateFreshState();
            SaveSystem.ApplyToState(state2, parsed);

            var queue2 = state2.Army.TrainingQueue;
            Assert.AreEqual(1, queue2.Count);
            Assert.AreEqual(UnitType.Pikeman, queue2[0].UnitType);
            Assert.AreEqual(0.5f, queue2[0].Progress, 0.001f);
        }

        [Test]
        public void RoundTrip_PreservesActiveQuests()
        {
            // Accept quest_lumber_baron as player 0 — but sector 0 must be owned by 0
            // Sector 0 already owned by 0 in TestMapFactory
            bool accepted = _state.Quests.AcceptQuest(0, "quest_lumber_baron");
            Assert.IsTrue(accepted);

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            var state2 = CreateFreshState();
            SaveSystem.ApplyToState(state2, parsed);

            var active = state2.Quests.GetActiveQuests(0);
            Assert.AreEqual(1, active.Count);
            Assert.AreEqual("quest_lumber_baron", active[0].Id);
        }

        [Test]
        public void RoundTrip_PreservesCompletedQuests()
        {
            _state.Quests.RestoreCompletedQuest("quest_iron_will");

            string saved = SaveSystem.Serialize(_state);
            var parsed = SaveSystem.Deserialize(saved);

            var state2 = CreateFreshState();
            SaveSystem.ApplyToState(state2, parsed);

            Assert.IsTrue(state2.Quests.IsCompleted("quest_iron_will"));
            // Should no longer appear in available
            bool found = false;
            foreach (var q in state2.Quests.AvailableQuests)
                if (q.Id == "quest_iron_will") { found = true; break; }
            Assert.IsFalse(found);
        }

        [Test]
        public void RoundTrip_GeneralIdCounterNoCollision()
        {
            // Restore a general with high ID, then hire for a different player
            // (no prestige restriction on first general per player) — IDs must not collide
            var units = new System.Collections.Generic.Dictionary<UnitType, int>();
            _state.Army.RestoreGeneral(99, 0, 0, false, units);
            var gen2 = _state.Army.HireGeneral(1, 1); // player 1's first general — sector 1
            Assert.IsNotNull(gen2);
            Assert.AreNotEqual(99, gen2.Id);
            Assert.IsTrue(gen2.Id >= 100, $"Expected ID >= 100 but got {gen2.Id}");
        }
    }
}
