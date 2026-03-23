using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class LogisticsTests
    {
        private SectorGraph _graph;
        private EventBus _eventBus;
        private LogisticsSystem _logistics;

        [SetUp]
        public void SetUp()
        {
            Storehouse.ResetIdCounter();
            _graph = TestMapFactory.CreateSixSectorMap();
            _eventBus = new EventBus();
            _logistics = new LogisticsSystem(_graph, carrierMaxItems: 3, _eventBus);
        }

        [Test]
        public void PlaceStorehouse_CreatesStorehouseInSector()
        {
            var sh = _logistics.PlaceStorehouse(0, 0);
            Assert.IsNotNull(sh);
            Assert.AreEqual(0, sh.SectorId);
            Assert.AreEqual(0, sh.OwnerId);
            Assert.IsTrue(_logistics.HasStorehouse(0));
        }

        [Test]
        public void PlaceStorehouse_SecondCall_ReturnsSame()
        {
            var sh1 = _logistics.PlaceStorehouse(0, 0);
            var sh2 = _logistics.PlaceStorehouse(0, 0);
            Assert.AreSame(sh1, sh2);
        }

        [Test]
        public void Storehouse_DefaultLevel1_Has2Carriers()
        {
            var sh = _logistics.PlaceStorehouse(0, 0);
            Assert.AreEqual(1, sh.Level);
            Assert.AreEqual(2, sh.CarrierCount);
        }

        [Test]
        public void Storehouse_Upgrade_IncreasesCarriers()
        {
            var sh = _logistics.PlaceStorehouse(0, 0);
            Assert.IsTrue(sh.Upgrade());
            Assert.AreEqual(2, sh.Level);
            Assert.AreEqual(3, sh.CarrierCount);
        }

        [Test]
        public void Storehouse_CannotUpgradePastLevel3()
        {
            var sh = _logistics.PlaceStorehouse(0, 0);
            sh.Upgrade(); // 2
            sh.Upgrade(); // 3
            Assert.IsFalse(sh.Upgrade()); // Can't go to 4
            Assert.AreEqual(3, sh.Level);
        }

        [Test]
        public void Storehouse_DispatchAndReturn_Carrier()
        {
            var sh = _logistics.PlaceStorehouse(0, 0);
            int initial = sh.IdleCarriers;

            Assert.IsTrue(sh.DispatchCarrier());
            Assert.AreEqual(initial - 1, sh.IdleCarriers);

            sh.ReturnCarrier();
            Assert.AreEqual(initial, sh.IdleCarriers);
        }

        [Test]
        public void Storehouse_CannotDispatch_WhenNoIdle()
        {
            var sh = _logistics.PlaceStorehouse(0, 0);
            // Dispatch all carriers
            while (sh.IdleCarriers > 0)
                sh.DispatchCarrier();

            Assert.IsFalse(sh.DispatchCarrier());
        }

        [Test]
        public void RequestDelivery_DispatchesCarrier()
        {
            _logistics.PlaceStorehouse(0, 0);
            _logistics.PlaceStorehouse(1, 1);

            bool dispatched = _logistics.RequestDelivery(0, 1, ResourceType.Planks, 3);
            Assert.IsTrue(dispatched);
            Assert.AreEqual(1, _logistics.ActiveTasks.Count);
        }

        [Test]
        public void RequestDelivery_ClampsTo_MaxItems()
        {
            _logistics.PlaceStorehouse(0, 0);
            _logistics.PlaceStorehouse(1, 1);

            _logistics.RequestDelivery(0, 1, ResourceType.Planks, 10);
            Assert.AreEqual(3, _logistics.ActiveTasks[0].Amount); // Clamped to 3
        }

        [Test]
        public void RequestDelivery_Fails_WithoutStorehouse()
        {
            bool dispatched = _logistics.RequestDelivery(0, 1, ResourceType.Planks, 1);
            Assert.IsFalse(dispatched);
        }

        [Test]
        public void Tick_CompletesDelivery_AfterTravelTime()
        {
            _logistics.PlaceStorehouse(0, 0);
            _logistics.PlaceStorehouse(1, 1);

            _logistics.RequestDelivery(0, 1, ResourceType.Planks, 2);

            // Path from 0→1 is 2 sectors = 2*3s = 6s travel
            _logistics.Tick(6f);

            Assert.AreEqual(0, _logistics.ActiveTasks.Count);
        }

        [Test]
        public void Tick_FiresCarrierDeliveryEvent_OnComplete()
        {
            _logistics.PlaceStorehouse(0, 0);
            _logistics.PlaceStorehouse(1, 1);

            CarrierDeliveryEvent? received = null;
            _eventBus.Subscribe<CarrierDeliveryEvent>(e => received = e);

            _logistics.RequestDelivery(0, 1, ResourceType.Stone, 1);
            _logistics.Tick(10f); // Enough to complete

            Assert.IsNotNull(received);
            Assert.AreEqual(0, received.Value.FromSectorId);
            Assert.AreEqual(1, received.Value.ToSectorId);
            Assert.AreEqual(ResourceType.Stone, received.Value.ResourceType);
        }

        [Test]
        public void Tick_ReturnsCarrier_AfterDelivery()
        {
            var sh = _logistics.PlaceStorehouse(0, 0);
            _logistics.PlaceStorehouse(1, 1);

            int idleBefore = sh.IdleCarriers;
            _logistics.RequestDelivery(0, 1, ResourceType.Planks, 1);
            Assert.AreEqual(idleBefore - 1, sh.IdleCarriers);

            _logistics.Tick(10f); // Complete
            Assert.AreEqual(idleBefore, sh.IdleCarriers);
        }

        [Test]
        public void PopulationSystem_AssignsWorkersAndTools()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var eventBus = new EventBus();
            var resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, eventBus)
            };
            resources[0].Set(ResourceType.Tools, 3);

            var construction = new ConstructionSystem(eventBus, 0.1f);
            construction.SetConstructorCount(0, 10);
            var production = new ProductionSystem(resources, construction, eventBus);
            var population = new PopulationSystem(resources, construction, production, eventBus);

            // Create a building with population
            var building = construction.PlaceBuilding(BaseBuildingType.Residence, 0, 0, 3, 0, 0, 0, 99);
            construction.Tick(1f); // Complete

            // Attach a work yard
            var wy = new WorkYard("toolmaker", building.Id, 0, 0, ResourceNodeType.None, 0, 0);
            building.AttachWorkYard(wy);
            production.RegisterWorkYard(wy);

            Assert.IsFalse(wy.HasWorker);
            Assert.IsFalse(wy.HasTool);

            population.Tick(0.1f);

            Assert.IsTrue(wy.HasWorker);
            Assert.IsTrue(wy.HasTool);
            Assert.AreEqual(2, resources[0].Get(ResourceType.Tools)); // 3 - 1 consumed
        }
    }
}
