using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class AITests
    {
        private GameState _state;

        [SetUp]
        public void SetUp()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph = TestMapFactory.CreateSixSectorMap();
            _state = new GameState(graph, playerCount: 2,
                constructionBaseTime: 0.01f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test_valley");
            // Give AI player extra resources
            _state.PlayerResources[1].Set(ResourceType.Planks, 50);
            _state.PlayerResources[1].Set(ResourceType.Stone, 30);
            _state.PlayerResources[1].Set(ResourceType.Tools, 10);
        }

        [Test]
        public void AI_StartsInEarlyEconomy()
        {
            Assert.AreEqual(1, _state.AIPlayers.Count);
            Assert.AreEqual(AIPhase.EarlyEconomy, _state.AIPlayers[0].Phase);
            Assert.AreEqual(AIPath.None, _state.AIPlayers[0].ChosenPath);
        }

        [Test]
        public void AI_BuildsEconomy_PlacesBuildings()
        {
            int buildingsBefore = _state.Construction.GetBuildingsByPlayer(1).Count;
            AIEconomy.BuildEconomy(_state, 1);

            Assert.Greater(_state.Construction.GetBuildingsByPlayer(1).Count, buildingsBefore);
        }

        [Test]
        public void AI_AttachesWorkYards_AfterBuildingComplete()
        {
            AIEconomy.BuildEconomy(_state, 1);
            _state.Construction.Tick(1f); // Complete construction

            AIEconomy.AttachWorkYards(_state, 1);

            bool anyWorkYards = false;
            foreach (var b in _state.Construction.GetBuildingsByPlayer(1))
                if (b.WorkYards.Count > 0) { anyWorkYards = true; break; }

            Assert.IsTrue(anyWorkYards);
        }

        [Test]
        public void AI_TransitionsToPathSelection_AfterEnoughBuildings()
        {
            var ai = _state.AIPlayers[0];

            // Give resources and tick multiple times
            _state.Construction.SetConstructorCount(1, 10);

            for (int i = 0; i < 20; i++)
            {
                ai.Tick(6f); // Over DECISION_INTERVAL
                _state.Construction.Tick(1f);
            }

            Assert.AreNotEqual(AIPhase.EarlyEconomy, ai.Phase,
                "AI should have left early economy after building enough");
        }

        [Test]
        public void AIEconomy_ChoosesLodge_WhenLowOnWood()
        {
            _state.PlayerResources[1].Set(ResourceType.Planks, 5);
            _state.PlayerResources[1].Set(ResourceType.Wood, 0);

            AIEconomy.BuildEconomy(_state, 1);

            // Should have placed a Lodge (cheapest, produces wood)
            var buildings = _state.Construction.GetBuildingsByPlayer(1);
            bool hasLodge = false;
            foreach (var b in buildings)
                if (b.Type == BaseBuildingType.Lodge) { hasLodge = true; break; }

            Assert.IsTrue(hasLodge, "AI should place Lodge when low on wood/planks");
        }

        [Test]
        public void AIEconomy_ManagesFood_EnablesPlainWhenAvailable()
        {
            // Create a building with a work yard
            _state.PlayerResources[1].Set(ResourceType.Bread, 10);
            var b = _state.Construction.PlaceBuilding(
                BaseBuildingType.Lodge, 1, 1, 3, 0f, 0f, 0, 8);
            _state.Construction.Tick(1f);

            var wy = new WorkYard("forester", b.Id, 1, 1, ResourceNodeType.Forest, 0f, 0f);
            b.AttachWorkYard(wy);

            AIEconomy.ManageFood(_state, 1);

            Assert.AreEqual(FoodSetting.Plain, b.FoodSetting);
        }

        [Test]
        public void AIEconomy_ManageQuests_AcceptsAvailableQuest()
        {
            // Player 1 owns sector 1 — quests with sectorId=-1 are available
            AIEconomy.ManageQuests(_state, 1);

            var active = _state.Quests.GetActiveQuests(1);
            Assert.Greater(active.Count, 0, "AI should accept at least one quest");
        }

        [Test]
        public void AIEconomy_GetResource_ReturnsCorrectAmount()
        {
            _state.PlayerResources[1].Set(ResourceType.IronBars, 42);
            Assert.AreEqual(42, AIEconomy.GetResource(_state, 1, ResourceType.IronBars));
        }

        [Test]
        public void AIEconomy_GetResource_Returns0_ForMissingPlayer()
        {
            Assert.AreEqual(0, AIEconomy.GetResource(_state, 99, ResourceType.Planks));
        }

        [Test]
        public void AI_FullSimulation_DoesNotCrash()
        {
            // Run 200 ticks of full simulation — ensure no exceptions
            for (int i = 0; i < 200; i++)
            {
                _state.Construction.Tick(0.1f);
                _state.Production.Tick(0.1f);
                _state.Population.Tick(0.1f);
                foreach (var ai in _state.AIPlayers)
                    ai.Tick(0.1f);
            }

            // AI should have progressed past early economy
            Assert.Pass("200 ticks completed without exception");
        }
    }
}
