using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>
    /// Storehouse relay (Critical Rule #4, Sprint 7c): production outside the
    /// home sector travels by carrier and is credited on DELIVERY — never twice.
    /// Home-sector production and busy carriers credit immediately instead.
    /// </summary>
    [TestFixture]
    public class StorehouseRelayTests
    {
        private GameState _state;

        [SetUp]
        public void Setup()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            Storehouse.ResetIdCounter();
            var info = MapFactory.CreateMap("twin_rivers");
            _state = new GameState(info.Graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 5, mapId: "twin_rivers");
        }

        private WorkYard AttachForester(int sectorId)
        {
            var building = _state.Construction.RestoreBuilding(
                BaseBuildingType.Lodge, sectorId, 0, 3, 0f, 0f,
                BuildingState.Complete, 1f, 0, FoodSetting.None);
            var wy = new WorkYard("forester", building.Id, sectorId, 0,
                ResourceNodeType.Forest, 0f, 0f);
            wy.RestoreState(true, true, 0f);
            building.AttachWorkYard(wy);
            _state.Production.RegisterWorkYard(wy);
            return wy;
        }

        [Test]
        public void Production_OutsideHomeSector_TravelsByCarrier_CreditsOnce()
        {
            // Player 0's home is sector 0 (lowest owned id); produce in sector 1
            AttachForester(1);
            int woodBefore = _state.PlayerResources[0].Get(ResourceType.Wood);

            _state.Production.Tick(10f); // forester cycle = 8s → one completion

            Assert.AreEqual(woodBefore, _state.PlayerResources[0].Get(ResourceType.Wood),
                "Routed goods must NOT be credited at production time");
            Assert.AreEqual(1, _state.Logistics.ActiveTasks.Count,
                "A carrier task must carry the goods toward the home storehouse");

            _state.Logistics.Tick(99999f); // complete the delivery

            Assert.AreEqual(woodBefore + 1, _state.PlayerResources[0].Get(ResourceType.Wood),
                "Delivered goods must be credited exactly once");
            Assert.AreEqual(0, _state.Logistics.ActiveTasks.Count);
        }

        [Test]
        public void Production_InHomeSector_CreditsImmediately_NoCarrier()
        {
            AttachForester(0); // home sector itself
            int woodBefore = _state.PlayerResources[0].Get(ResourceType.Wood);

            _state.Production.Tick(10f);

            Assert.AreEqual(woodBefore + 1, _state.PlayerResources[0].Get(ResourceType.Wood),
                "Home-sector production credits immediately");
            Assert.AreEqual(0, _state.Logistics.ActiveTasks.Count);
        }

        [Test]
        public void Production_WhenCarriersBusy_FallsBackToImmediateCredit()
        {
            // Occupy ALL of sector 1's carriers first (level 1 = Level+1 = 2)
            int busy = 0;
            while (_state.Logistics.RequestDelivery(1, 0, ResourceType.Stone, 1))
                busy++;
            Assert.Greater(busy, 0, "Setup: at least one carrier must exist");

            AttachForester(1);
            int woodBefore = _state.PlayerResources[0].Get(ResourceType.Wood);

            _state.Production.Tick(10f);

            Assert.AreEqual(woodBefore + 1, _state.PlayerResources[0].Get(ResourceType.Wood),
                "With no free carrier the goods must not be lost — immediate credit");
            Assert.AreEqual(busy, _state.Logistics.ActiveTasks.Count,
                "Only the manual tasks are active — production added none");
        }
    }
}
