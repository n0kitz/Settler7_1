using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class ConquestRewardTests
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
        public void ConquestEvent_PlacesStorehouse_InNewSector()
        {
            // Sector 2 is neutral, should not have a storehouse
            Assert.IsFalse(_state.Logistics.HasStorehouse(2));

            // Simulate conquest
            _state.Graph.GetSector(2).SetOwner(0);
            _state.Events.Publish(new SectorConqueredEvent(2, 0, Sector.NEUTRAL, ConquestMethod.Military));

            // Should now have a storehouse
            Assert.IsTrue(_state.Logistics.HasStorehouse(2));
            Assert.AreEqual(0, _state.Logistics.GetStorehouse(2).OwnerId);
        }

        [Test]
        public void ConquestEvent_AwardsPrestige()
        {
            int prestigeBefore = _state.Prestige.GetPoints(0);

            _state.Graph.GetSector(2).SetOwner(0);
            _state.Events.Publish(new SectorConqueredEvent(2, 0, Sector.NEUTRAL, ConquestMethod.Military));

            Assert.AreEqual(prestigeBefore + 1, _state.Prestige.GetPoints(0));
        }

        [Test]
        public void ConquestEvent_SpecialSector_AwardsVP()
        {
            // Sector 5 has vpRewardId
            int vpBefore = _state.Victory.GetVPCount(0);

            _state.Graph.GetSector(5).SetOwner(0);
            _state.Events.Publish(new SectorConqueredEvent(5, 0, Sector.NEUTRAL, ConquestMethod.Military));

            Assert.Greater(_state.Victory.GetVPCount(0), vpBefore);
        }

        [Test]
        public void StartingSectors_HaveStorehouses()
        {
            // Player 0 starts in sector 0, Player 1 in sector 1
            Assert.IsTrue(_state.Logistics.HasStorehouse(0));
            Assert.IsTrue(_state.Logistics.HasStorehouse(1));
        }
    }
}
