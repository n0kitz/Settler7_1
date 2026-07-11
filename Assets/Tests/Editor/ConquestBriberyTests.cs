using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class ConquestBriberyTests
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

        private void GrantBriberyCost(int playerId, int sectorId)
        {
            var sector = _state.Graph.GetSector(sectorId);
            ConquestSystem.GetBriberyCost(sector.GarrisonStrength,
                out int coins, out int garments, out int jewelry);
            _state.PlayerResources[playerId].Set(ResourceType.Coins, coins);
            _state.PlayerResources[playerId].Set(ResourceType.Garments, garments);
            _state.PlayerResources[playerId].Set(ResourceType.Jewelry, jewelry);
        }

        // ---- Cost formula ----

        [Test]
        public void GetBriberyCost_ScalesWithGarrisonStrength()
        {
            ConquestSystem.GetBriberyCost(0, out int coinsWeak, out int garmentsWeak, out int jewelryWeak);
            ConquestSystem.GetBriberyCost(8, out int coinsStrong, out int garmentsStrong, out int jewelryStrong);

            Assert.Greater(coinsStrong, coinsWeak, "Stronger garrison must cost more coins");
            Assert.Greater(garmentsStrong, garmentsWeak, "Stronger garrison must cost more garments");
            Assert.Greater(jewelryStrong, jewelryWeak, "Stronger garrison must cost more jewelry");
        }

        [Test]
        public void GetBriberyCost_KnownGarrisonValues()
        {
            // coins = 8 + garrison*3, garments = 2 + garrison, jewelry = 1 + garrison/2
            ConquestSystem.GetBriberyCost(4, out int coins, out int garments, out int jewelry);
            Assert.AreEqual(20, coins);
            Assert.AreEqual(6, garments);
            Assert.AreEqual(3, jewelry);
        }

        // ---- TryBribe: eligibility ----

        [Test]
        public void TryBribe_PlayerOwnedSector_Fails()
        {
            GrantBriberyCost(1, 0);
            bool result = _state.Conquest.TryBribe(1, 0);
            Assert.IsFalse(result, "Cannot bribe a player-owned sector");
        }

        [Test]
        public void TryBribe_NeutralSector_InsufficientResources_Fails()
        {
            // Sector 2 requires coins/garments/jewelry, grant nothing
            bool result = _state.Conquest.TryBribe(0, 2);
            Assert.IsFalse(result, "Bribery must fail without sufficient resources");
            Assert.AreEqual(Sector.NEUTRAL, _state.Graph.GetSector(2).OwnerId,
                "Failed bribery must not change sector ownership");
        }

        [Test]
        public void TryBribe_PartialResources_Fails()
        {
            var sector = _state.Graph.GetSector(2);
            ConquestSystem.GetBriberyCost(sector.GarrisonStrength,
                out int coins, out int garments, out int jewelry);

            // Grant coins and garments but withhold jewelry
            _state.PlayerResources[0].Set(ResourceType.Coins, coins);
            _state.PlayerResources[0].Set(ResourceType.Garments, garments);
            _state.PlayerResources[0].Set(ResourceType.Jewelry, jewelry - 1);

            bool result = _state.Conquest.TryBribe(0, 2);
            Assert.IsFalse(result, "Bribery must fail if any single resource is short");
        }

        // ---- TryBribe: success path ----

        [Test]
        public void TryBribe_SufficientResources_Succeeds()
        {
            GrantBriberyCost(0, 2);
            bool result = _state.Conquest.TryBribe(0, 2);
            Assert.IsTrue(result, "Bribery must succeed with sufficient resources");
            Assert.AreEqual(0, _state.Graph.GetSector(2).OwnerId,
                "Sector must transfer to briber immediately (no delay, unlike proselytism)");
        }

        [Test]
        public void TryBribe_Success_SpendsExactResources()
        {
            var sector = _state.Graph.GetSector(2);
            ConquestSystem.GetBriberyCost(sector.GarrisonStrength,
                out int coins, out int garments, out int jewelry);
            GrantBriberyCost(0, 2);

            _state.Conquest.TryBribe(0, 2);

            Assert.AreEqual(0, _state.PlayerResources[0].Get(ResourceType.Coins));
            Assert.AreEqual(0, _state.PlayerResources[0].Get(ResourceType.Garments));
            Assert.AreEqual(0, _state.PlayerResources[0].Get(ResourceType.Jewelry));
        }

        [Test]
        public void TryBribe_Success_AwardsPrestige()
        {
            int prestigeBefore = _state.Prestige.GetPoints(0);
            GrantBriberyCost(0, 2);

            _state.Conquest.TryBribe(0, 2);

            Assert.AreEqual(prestigeBefore + 1, _state.Prestige.GetPoints(0),
                "Bribery conquest must award 1 prestige point, same as other conquest methods");
        }

        [Test]
        public void TryBribe_Success_PublishesSectorConqueredEventWithBriberyMethod()
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

            GrantBriberyCost(0, 2);
            _state.Conquest.TryBribe(0, 2);

            Assert.IsTrue(eventFired, "SectorConqueredEvent must fire on successful bribery");
            Assert.AreEqual(ConquestMethod.Bribery, capturedMethod);
        }

        [Test]
        public void TryBribe_FortifiedSector_CostsMoreThanUnfortified()
        {
            // Sector 3 (fortified, garrison 8) vs sector 4 (unfortified, garrison 4)
            var fortified = _state.Graph.GetSector(3);
            var unfortified = _state.Graph.GetSector(4);

            ConquestSystem.GetBriberyCost(fortified.GarrisonStrength,
                out int coinsF, out _, out _);
            ConquestSystem.GetBriberyCost(unfortified.GarrisonStrength,
                out int coinsU, out _, out _);

            Assert.Greater(coinsF, coinsU,
                "Fortified sector's higher garrison strength must raise bribery cost");
        }
    }
}
