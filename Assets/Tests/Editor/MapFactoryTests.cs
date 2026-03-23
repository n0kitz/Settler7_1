using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class MapFactoryTests
    {
        [Test]
        public void TestValley_Has6Sectors()
        {
            var info = MapFactory.CreateMap("test_valley");
            Assert.AreEqual(6, info.Graph.SectorCount);
            Assert.AreEqual(2, info.PlayerCount);
            Assert.AreEqual(4, info.VPRequired);
        }

        [Test]
        public void TwinRivers_Has10Sectors()
        {
            var info = MapFactory.CreateMap("twin_rivers");
            Assert.AreEqual(10, info.Graph.SectorCount);
            Assert.AreEqual(2, info.PlayerCount);
            Assert.AreEqual(5, info.VPRequired);
        }

        [Test]
        public void MountainPass_Has12Sectors_3Players()
        {
            var info = MapFactory.CreateMap("mountain_pass");
            Assert.AreEqual(12, info.Graph.SectorCount);
            Assert.AreEqual(3, info.PlayerCount);
            Assert.AreEqual(5, info.VPRequired);
        }

        [Test]
        public void IslandChain_Has8Sectors()
        {
            var info = MapFactory.CreateMap("island_chain");
            Assert.AreEqual(8, info.Graph.SectorCount);
            Assert.AreEqual(2, info.PlayerCount);
            Assert.AreEqual(4, info.VPRequired);
        }

        [Test]
        public void AllMaps_HaveValidPlayerStarts()
        {
            foreach (var mapId in MapFactory.GetMapIds())
            {
                var info = MapFactory.CreateMap(mapId);
                int playerSectors = 0;
                for (int i = 0; i < info.Graph.SectorCount; i++)
                {
                    if (info.Graph.GetSector(i).IsPlayerOwned)
                        playerSectors++;
                }
                Assert.GreaterOrEqual(playerSectors, info.PlayerCount,
                    $"Map {mapId} has fewer player sectors than players");
            }
        }

        [Test]
        public void AllMaps_AreConnected()
        {
            foreach (var mapId in MapFactory.GetMapIds())
            {
                var info = MapFactory.CreateMap(mapId);
                // BFS from sector 0 should reach all sectors
                var visited = new System.Collections.Generic.HashSet<int> { 0 };
                var queue = new System.Collections.Generic.Queue<int>();
                queue.Enqueue(0);
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    foreach (int n in info.Graph.GetNeighbors(current))
                    {
                        if (visited.Add(n))
                            queue.Enqueue(n);
                    }
                }
                Assert.AreEqual(info.Graph.SectorCount, visited.Count,
                    $"Map {mapId} is not fully connected");
            }
        }

        [Test]
        public void GetMapIds_Returns4Maps()
        {
            Assert.AreEqual(4, MapFactory.GetMapIds().Length);
        }
    }
}
