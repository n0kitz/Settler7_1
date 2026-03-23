using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class SectorGraphTests
    {
        private SectorGraph _graph;

        [SetUp]
        public void SetUp()
        {
            _graph = TestMapFactory.CreateSixSectorMap();
        }

        [Test]
        public void SixSectorMap_HasSixSectors()
        {
            Assert.AreEqual(6, _graph.SectorCount);
        }

        [Test]
        public void Player0_OwnsSector0()
        {
            var sector = _graph.GetSector(0);
            Assert.AreEqual(0, sector.OwnerId);
            Assert.IsTrue(sector.IsPlayerOwned);
        }

        [Test]
        public void Player1_OwnsSector1()
        {
            var sector = _graph.GetSector(1);
            Assert.AreEqual(1, sector.OwnerId);
        }

        [Test]
        public void NeutralSectors_HaveGarrisons()
        {
            var sector2 = _graph.GetSector(2);
            Assert.IsTrue(sector2.IsNeutral);
            Assert.AreEqual(4, sector2.GarrisonStrength);

            var sector3 = _graph.GetSector(3);
            Assert.IsTrue(sector3.IsNeutral);
            Assert.IsTrue(sector3.IsFortified);
            Assert.AreEqual(8, sector3.GarrisonStrength);
        }

        [Test]
        public void Adjacency_Sector0HasThreeNeighbors()
        {
            var neighbors = _graph.GetNeighbors(0);
            Assert.AreEqual(3, neighbors.Count);
            Assert.IsTrue(_graph.AreAdjacent(0, 1));
            Assert.IsTrue(_graph.AreAdjacent(0, 2));
            Assert.IsTrue(_graph.AreAdjacent(0, 4));
        }

        [Test]
        public void Adjacency_NonAdjacentSectors()
        {
            // 0 and 3 are not directly connected
            Assert.IsFalse(_graph.AreAdjacent(0, 3));
            // 2 and 5 are not directly connected
            Assert.IsFalse(_graph.AreAdjacent(2, 5));
        }

        [Test]
        public void Adjacency_IsBidirectional()
        {
            Assert.IsTrue(_graph.AreAdjacent(0, 1));
            Assert.IsTrue(_graph.AreAdjacent(1, 0));
        }

        [Test]
        public void GetSectorsOwnedBy_ReturnsCorrectSectors()
        {
            var player0Sectors = _graph.GetSectorsOwnedBy(0);
            Assert.AreEqual(1, player0Sectors.Count);
            Assert.Contains(0, player0Sectors);

            var player1Sectors = _graph.GetSectorsOwnedBy(1);
            Assert.AreEqual(1, player1Sectors.Count);
            Assert.Contains(1, player1Sectors);
        }

        [Test]
        public void SetOwner_TransfersOwnership()
        {
            var sector = _graph.GetSector(2);
            Assert.IsTrue(sector.IsNeutral);

            sector.SetOwner(0);

            Assert.AreEqual(0, sector.OwnerId);
            Assert.IsTrue(sector.IsPlayerOwned);
            Assert.AreEqual(0, sector.GarrisonStrength); // Cleared on conquest

            var owned = _graph.GetSectorsOwnedBy(0);
            Assert.AreEqual(2, owned.Count);
        }

        [Test]
        public void FindPath_AdjacentSectors_DirectPath()
        {
            var path = _graph.FindPath(0, 1);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(0, path[0]);
            Assert.AreEqual(1, path[1]);
        }

        [Test]
        public void FindPath_NonAdjacentSectors_FindsShortestRoute()
        {
            // 0 → 3: must go through 1 (0→1→3)
            var path = _graph.FindPath(0, 3);
            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(0, path[0]);
            Assert.AreEqual(1, path[1]);
            Assert.AreEqual(3, path[2]);
        }

        [Test]
        public void FindPath_SameSourceAndTarget_ReturnsSingleNode()
        {
            var path = _graph.FindPath(2, 2);
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(2, path[0]);
        }

        [Test]
        public void CanReach_Player0_CanReachAdjacentNeutral()
        {
            Assert.IsTrue(_graph.CanReach(0, 2));
            Assert.IsTrue(_graph.CanReach(0, 4));
        }

        [Test]
        public void CanReach_Player0_CanReachThroughNeutralChain()
        {
            // Player 0 owns sector 0. Sector 4 is neutral, sector 5 is neutral.
            // Path: 0 → 4 → 5 (through neutral sectors)
            Assert.IsTrue(_graph.CanReach(0, 5));
        }

        [Test]
        public void HasResource_ReturnsCorrectResults()
        {
            var sector0 = _graph.GetSector(0);
            Assert.IsTrue(sector0.HasResource(ResourceNodeType.Forest));
            Assert.IsTrue(sector0.HasResource(ResourceNodeType.WaterSource));
            Assert.IsFalse(sector0.HasResource(ResourceNodeType.Gold));
        }
    }
}
