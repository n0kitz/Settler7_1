using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>
    /// Cleric roster + §14.6 research gating: recruiting spends goods,
    /// research occupies clerics for its duration and releases them,
    /// and a ResearchSystem without a ClericSystem stays ungated.
    /// </summary>
    public class ClericSystemTests
    {
        private EventBus _events;
        private Dictionary<int, PlayerResources> _resources;
        private ClericSystem _clerics;
        private ResearchSystem _research;

        [SetUp]
        public void SetUp()
        {
            _events = new EventBus();
            _resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events),
                [1] = new PlayerResources(1, _events)
            };
            _clerics = new ClericSystem(_resources, _events);
            _research = new ResearchSystem(_events, _clerics);
        }

        private void FundNovices(int playerId, int count)
        {
            _resources[playerId].Add(ResourceType.Bread, count);
            _resources[playerId].Add(ResourceType.Coins, count * 2);
            for (int i = 0; i < count; i++)
                Assert.IsTrue(_clerics.Recruit(playerId, ClericRank.Novice));
        }

        [Test]
        public void Recruit_SpendsGoods_AndIncrementsCount()
        {
            _resources[0].Add(ResourceType.Bread, 1);
            _resources[0].Add(ResourceType.Coins, 2);

            Assert.IsTrue(_clerics.Recruit(0, ClericRank.Novice));
            Assert.AreEqual(1, _clerics.GetCount(0, ClericRank.Novice));
            Assert.AreEqual(0, _resources[0].Get(ResourceType.Bread));
            Assert.AreEqual(0, _resources[0].Get(ResourceType.Coins));
        }

        [Test]
        public void Recruit_Fails_WithoutGoods()
        {
            Assert.IsFalse(_clerics.Recruit(0, ClericRank.Novice));
            Assert.AreEqual(0, _clerics.GetCount(0, ClericRank.Novice));
        }

        [Test]
        public void Recruit_HigherRanks_UseTheirOwnCosts()
        {
            _resources[0].Add(ResourceType.Bread, 1);
            _resources[0].Add(ResourceType.Books, 1);
            _resources[0].Add(ResourceType.Coins, 3);

            Assert.IsFalse(_clerics.Recruit(0, ClericRank.Father)); // needs Garments
            Assert.IsTrue(_clerics.Recruit(0, ClericRank.Brother));
            Assert.AreEqual(1, _clerics.GetCount(0, ClericRank.Brother));
        }

        [Test]
        public void StartResearch_Fails_WithoutClerics()
        {
            Assert.IsFalse(_research.StartResearch(0, "tech_plowing"));
        }

        [Test]
        public void StartResearch_OccupiesClerics_AndCompletionReleasesThem()
        {
            FundNovices(0, 3); // tier 1 costs 3/0/0

            Assert.IsTrue(_research.StartResearch(0, "tech_plowing"));
            Assert.AreEqual(0, _clerics.GetAvailable(0, ClericRank.Novice));

            // A second tier-1 research cannot start with everyone occupied
            Assert.IsFalse(_research.StartResearch(0, "tech_masonry"));

            _research.Tick(1000f); // completes tech_plowing
            Assert.IsTrue(_research.HasTech(0, "tech_plowing"));
            Assert.AreEqual(3, _clerics.GetAvailable(0, ClericRank.Novice));
        }

        [Test]
        public void CancelResearch_ReleasesClerics()
        {
            FundNovices(0, 3);
            Assert.IsTrue(_research.StartResearch(0, "tech_plowing"));
            Assert.AreEqual(0, _clerics.GetAvailable(0, ClericRank.Novice));

            Assert.IsTrue(_research.CancelResearch(0, "tech_plowing"));
            Assert.AreEqual(3, _clerics.GetAvailable(0, ClericRank.Novice));
        }

        [Test]
        public void HasClericsFor_ReflectsAvailability()
        {
            var def = TechTree.Get("tech_plowing");
            Assert.IsFalse(_research.HasClericsFor(0, def));
            FundNovices(0, 3);
            Assert.IsTrue(_research.HasClericsFor(0, def));
        }

        [Test]
        public void ResearchSystem_WithoutClericSystem_IsUngated()
        {
            var ungated = new ResearchSystem(_events);
            Assert.IsTrue(ungated.StartResearch(0, "tech_masonry"));
        }

        [Test]
        public void TechDef_CostsScaleByTier()
        {
            var t1 = TechTree.Get("tech_plowing");
            var t2 = TechTree.Get("tech_crop_rotation");
            var t3 = TechTree.Get("tech_irrigation");
            Assert.AreEqual((3, 0, 0), (t1.CostNovices, t1.CostBrothers, t1.CostFathers));
            Assert.AreEqual((4, 2, 0), (t2.CostNovices, t2.CostBrothers, t2.CostFathers));
            Assert.AreEqual((5, 2, 1), (t3.CostNovices, t3.CostBrothers, t3.CostFathers));
        }

        [Test]
        public void SaveLoad_RoundTripsClericCounts()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph = TestMapFactory.CreateSixSectorMap();
            var state = new GameState(graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test_valley");
            state.PlayerResources[0].Add(ResourceType.Bread, 2);
            state.PlayerResources[0].Add(ResourceType.Coins, 4);
            Assert.IsTrue(state.Clerics.Recruit(0, ClericRank.Novice));
            Assert.IsTrue(state.Clerics.Recruit(0, ClericRank.Novice));

            string data = SaveSystem.Serialize(state);
            Assert.IsTrue(data.Contains("clerics.0=2,0,0"));

            var graph2 = TestMapFactory.CreateSixSectorMap();
            var state2 = new GameState(graph2, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test_valley");
            SaveSystem.ApplyToState(state2, SaveSystem.Deserialize(data));
            Assert.AreEqual(2, state2.Clerics.GetCount(0, ClericRank.Novice));
        }
    }
}
