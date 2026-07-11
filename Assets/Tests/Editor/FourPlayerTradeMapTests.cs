using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class FourPlayerTradeMapTests
    {
        [Test]
        public void CrownWarTradeMap_Has14Outposts()
        {
            var map = FourPlayerTradeMapFactory.CreateCrownWarTradeMap();
            Assert.AreEqual(14, map.AllOutposts.Count);
        }

        [Test]
        public void EmpireTradeMap_Has18Outposts()
        {
            var map = FourPlayerTradeMapFactory.CreateEmpireTradeMap();
            Assert.AreEqual(18, map.AllOutposts.Count);
        }

        [Test]
        public void CrownWarTradeMap_OutpostIdsAreUnique()
        {
            var map = FourPlayerTradeMapFactory.CreateCrownWarTradeMap();
            var seen = new HashSet<string>();
            foreach (var outpost in map.AllOutposts)
                Assert.IsTrue(seen.Add(outpost.Id), $"Duplicate outpost id: {outpost.Id}");
        }

        [Test]
        public void EmpireTradeMap_OutpostIdsAreUnique()
        {
            var map = FourPlayerTradeMapFactory.CreateEmpireTradeMap();
            var seen = new HashSet<string>();
            foreach (var outpost in map.AllOutposts)
                Assert.IsTrue(seen.Add(outpost.Id), $"Duplicate outpost id: {outpost.Id}");
        }

        [Test]
        public void CrownWarTradeMap_AllOutpostsReachableFromStart()
        {
            AssertFullyConnected(FourPlayerTradeMapFactory.CreateCrownWarTradeMap());
        }

        [Test]
        public void EmpireTradeMap_AllOutpostsReachableFromStart()
        {
            AssertFullyConnected(FourPlayerTradeMapFactory.CreateEmpireTradeMap());
        }

        [Test]
        public void TradeMap_CreateForMap_DispatchesCrownWarAndEmpire()
        {
            var crownWar = TestTradeMapFactory.CreateForMap("crown_war");
            var empire = TestTradeMapFactory.CreateForMap("empire");

            Assert.AreEqual(14, crownWar.AllOutposts.Count,
                "CreateForMap(\"crown_war\") must route to FourPlayerTradeMapFactory");
            Assert.AreEqual(18, empire.AllOutposts.Count,
                "CreateForMap(\"empire\") must route to FourPlayerTradeMapFactory");
        }

        private static void AssertFullyConnected(TradeMap map)
        {
            var visited = new HashSet<int> { 0 };
            var queue = new Queue<int>();
            queue.Enqueue(0);
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                foreach (int n in map.GetRoutes(current))
                {
                    if (visited.Add(n))
                        queue.Enqueue(n);
                }
            }
            Assert.AreEqual(map.AllOutposts.Count, visited.Count,
                "Trade map must be fully connected — every outpost reachable from outpost 0");
        }
    }
}
