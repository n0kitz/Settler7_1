using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class LargeMapTests
    {
        [Test]
        public void LargeMap_Has12Sectors()
        {
            var graph = LargeMapFactory.CreateTwelveSectorMap();
            Assert.AreEqual(12, graph.SectorCount);
        }

        [Test]
        public void LargeMap_Has3PlayerStarts()
        {
            var graph = LargeMapFactory.CreateTwelveSectorMap();
            Assert.AreEqual(0, graph.GetSector(0).OwnerId);  // P0
            Assert.AreEqual(1, graph.GetSector(2).OwnerId);  // P1
            Assert.AreEqual(2, graph.GetSector(10).OwnerId); // P2
        }

        [Test]
        public void LargeMap_DragonsPeak_HasVPReward()
        {
            var graph = LargeMapFactory.CreateTwelveSectorMap();
            Assert.AreEqual("vp_special_sector_dragons_peak",
                graph.GetSector(11).VPRewardId);
        }

        [Test]
        public void LargeMap_FortifiedSectors_HaveHighGarrison()
        {
            var graph = LargeMapFactory.CreateTwelveSectorMap();
            // Sector 4 (Iron Hills) and 9 (Gold River) and 11 (Dragon's Peak) are fortified
            Assert.IsTrue(graph.GetSector(4).IsFortified);
            Assert.IsTrue(graph.GetSector(9).IsFortified);
            Assert.IsTrue(graph.GetSector(11).IsFortified);
        }

        [Test]
        public void LargeMap_AllSectorsConnected()
        {
            var graph = LargeMapFactory.CreateTwelveSectorMap();
            // Every sector should have at least 1 neighbor
            for (int i = 0; i < graph.SectorCount; i++)
            {
                var neighbors = graph.GetNeighbors(i);
                Assert.Greater(neighbors.Count, 0,
                    $"Sector {i} ({graph.GetSector(i).Name}) has no neighbors");
            }
        }

        [Test]
        public void MapFactory_CreateLargeValley_ReturnsCorrectInfo()
        {
            var info = MapFactory.CreateMap("large_valley");
            Assert.AreEqual("large_valley", info.Id);
            Assert.AreEqual(3, info.PlayerCount);
            Assert.AreEqual(5, info.VPRequired);
            Assert.AreEqual(12, info.Graph.SectorCount);
        }

        [Test]
        public void MapFactory_AllMapsCreate_WithoutErrors()
        {
            foreach (var mapId in MapFactory.GetMapIds())
            {
                var info = MapFactory.CreateMap(mapId);
                Assert.IsNotNull(info.Graph, $"Map {mapId} has null graph");
                Assert.Greater(info.Graph.SectorCount, 0, $"Map {mapId} has no sectors");
                Assert.Greater(info.PlayerCount, 0, $"Map {mapId} has no players");
                Assert.Greater(info.VPRequired, 0, $"Map {mapId} has no VP requirement");
            }
        }

        [Test]
        public void TradeMap_LargeValley_Has12Outposts()
        {
            var tradeMap = TestTradeMapFactory.CreateForMap("large_valley");
            Assert.AreEqual(12, tradeMap.AllOutposts.Count);
        }

        [Test]
        public void TradeMap_LargeValley_HasSpecialOutposts()
        {
            var tradeMap = TestTradeMapFactory.CreateForMap("large_valley");
            int specialCount = 0;
            foreach (var op in tradeMap.AllOutposts)
                if (op.IsSpecial) specialCount++;
            Assert.AreEqual(2, specialCount);
        }

        [Test]
        public void QuestDatabase_LargeValley_HasMapSpecificQuests()
        {
            var quests = QuestDatabase.GetQuestsForMap("large_valley");
            // Should have universal (4) + large_valley specific (6)
            Assert.AreEqual(10, quests.Count);

            // Verify a map-specific quest exists
            bool hasGoldRush = false;
            foreach (var q in quests)
                if (q.Id == "quest_gold_rush") { hasGoldRush = true; break; }
            Assert.IsTrue(hasGoldRush, "Large valley should have Gold Rush quest");
        }

        [Test]
        public void QuestDatabase_TestValley_HasDefaultQuests()
        {
            var quests = QuestDatabase.GetQuestsForMap("test_valley");
            // Should have universal (4) + test_valley specific (2)
            Assert.AreEqual(6, quests.Count);
        }

        [Test]
        public void GameState_LargeValley_InitializesCorrectly()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph = LargeMapFactory.CreateTwelveSectorMap();
            var state = new GameState(graph, playerCount: 3,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 5, mapId: "large_valley");

            Assert.AreEqual(3, state.PlayerCount);
            Assert.IsTrue(state.Logistics.HasStorehouse(0));  // P0 start
            Assert.IsTrue(state.Logistics.HasStorehouse(2));  // P1 start
            Assert.IsTrue(state.Logistics.HasStorehouse(10)); // P2 start
            Assert.AreEqual(2, state.AIPlayers.Count); // P1 and P2 are AI
        }
    }
}
