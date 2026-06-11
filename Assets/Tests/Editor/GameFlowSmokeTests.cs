using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>
    /// Headless full-simulation smoke test: boots a real map with AI,
    /// runs every system through SimulationRunner for simulated minutes,
    /// and round-trips a save. Guards against integration breakage that
    /// unit tests of single systems cannot catch.
    /// </summary>
    [TestFixture]
    public class GameFlowSmokeTests
    {
        private GameState CreateRealGame(string mapId)
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            Storehouse.ResetIdCounter();
            ArmySystem.ResetIdCounter();

            var info = MapFactory.CreateMap(mapId);
            return new GameState(info.Graph, playerCount: info.PlayerCount,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: info.VPRequired, mapId: mapId);
        }

        [Test]
        public void FullSimulation_RunsTwoMinutes_WithoutErrors()
        {
            var state = CreateRealGame("test_valley");
            var runner = new SimulationRunner(state);
            runner.EnableAll();

            // 120 simulated seconds at 10 ticks/sec
            for (int i = 0; i < 1200; i++)
                runner.Tick(0.1f);

            Assert.AreEqual(1200, runner.TickCount);
            Assert.IsFalse(state.Victory.IsGameOver,
                "Game must not end within the first 2 minutes");
        }

        [Test]
        public void FullSimulation_AIBuildsEconomy()
        {
            var state = CreateRealGame("test_valley");
            // Give the AI enough to act on
            state.PlayerResources[1].Set(ResourceType.Planks, 50);
            state.PlayerResources[1].Set(ResourceType.Stone, 30);
            state.PlayerResources[1].Set(ResourceType.Tools, 10);

            var runner = new SimulationRunner(state);
            runner.EnableAll();

            for (int i = 0; i < 1200; i++)
                runner.Tick(0.1f);

            Assert.Greater(state.Construction.GetBuildingsByPlayer(1).Count, 0,
                "AI should place at least one building within 2 simulated minutes");
        }

        [Test]
        public void FullSimulation_SaveLoadRoundTrip_AfterActivity()
        {
            var state = CreateRealGame("test_valley");
            state.PlayerResources[1].Set(ResourceType.Planks, 50);

            var runner = new SimulationRunner(state);
            runner.EnableAll();
            for (int i = 0; i < 600; i++)
                runner.Tick(0.1f);

            string saved = SaveSystem.Serialize(state);
            var parsed = SaveSystem.Deserialize(saved);

            var state2 = CreateRealGame("test_valley");
            SaveSystem.ApplyToState(state2, parsed);

            Assert.AreEqual(state.Graph.GetSector(0).OwnerId,
                state2.Graph.GetSector(0).OwnerId);
            Assert.AreEqual(state.PlayerResources[0].Get(ResourceType.Planks),
                state2.PlayerResources[0].Get(ResourceType.Planks));
            Assert.AreEqual(state.Construction.AllBuildings.Count,
                state2.Construction.AllBuildings.Count);

            // Loaded state must keep simulating without errors
            var runner2 = new SimulationRunner(state2);
            runner2.EnableAll();
            for (int i = 0; i < 100; i++)
                runner2.Tick(0.1f);
        }

        [Test]
        public void FullSimulation_AllShippedMaps_BootAndTick()
        {
            foreach (var mapId in MapFactory.GetMapIds())
            {
                var state = CreateRealGame(mapId);
                var runner = new SimulationRunner(state);
                runner.EnableAll();

                for (int i = 0; i < 100; i++)
                    runner.Tick(0.1f);

                Assert.IsFalse(state.Victory.IsGameOver,
                    $"Map {mapId}: game ended within 10 seconds");
            }
        }
    }
}
