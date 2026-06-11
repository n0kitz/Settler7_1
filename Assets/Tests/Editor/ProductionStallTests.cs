using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for ProductionStalledEvent (carrier "goods missing" feedback).</summary>
    [TestFixture]
    public class ProductionStallTests
    {
        private EventBus _eventBus;
        private Dictionary<int, PlayerResources> _resources;
        private ConstructionSystem _construction;
        private ProductionSystem _production;

        [SetUp]
        public void SetUp()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            _eventBus = new EventBus();
            _resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _eventBus)
            };
            _resources[0].Set(ResourceType.Planks, 50);
            _resources[0].Set(ResourceType.Stone, 50);

            _construction = new ConstructionSystem(_eventBus, baseConstructionTime: 0.1f);
            _construction.SetConstructorCount(0, 10);
            _production = new ProductionSystem(_resources, _construction, _eventBus);
        }

        private WorkYard CreateSawmill()
        {
            // Sawmill needs Wood input — player has none in these tests
            var b = _construction.PlaceBuilding(BaseBuildingType.Lodge, 0, 0, 3, 0, 0, 0, 99);
            _construction.Tick(1f);
            var wy = new WorkYard("sawmill", b.Id, 0, 0, ResourceNodeType.None, 0, 0);
            b.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);
            return wy;
        }

        [Test]
        public void Stall_FiresEvent_WhenInputsMissing()
        {
            var wy = CreateSawmill();
            int fired = 0;
            int sectorId = -1;
            _eventBus.Subscribe<ProductionStalledEvent>(e =>
            {
                fired++;
                sectorId = e.SectorId;
            });

            _production.Tick(1f);

            Assert.AreEqual(1, fired);
            Assert.AreEqual(wy.SectorId, sectorId);
        }

        [Test]
        public void Stall_FiresOnlyOnce_WhileStillStalled()
        {
            CreateSawmill();
            int fired = 0;
            _eventBus.Subscribe<ProductionStalledEvent>(e => fired++);

            _production.Tick(1f);
            _production.Tick(1f);
            _production.Tick(1f);

            Assert.AreEqual(1, fired, "Stall event must not fire every tick");
        }

        [Test]
        public void Stall_RefiresAfterRecoveryAndNewStall()
        {
            CreateSawmill();
            int fired = 0;
            _eventBus.Subscribe<ProductionStalledEvent>(e => fired++);

            _production.Tick(1f);                       // stall #1
            _resources[0].Set(ResourceType.Wood, 1);    // recover (1 cycle worth)
            _production.Tick(8f);                       // full sawmill cycle runs
            _production.Tick(1f);                       // wood exhausted → stall #2

            Assert.AreEqual(2, fired);
        }

        [Test]
        public void NoStall_WhenInputsAvailable()
        {
            CreateSawmill();
            _resources[0].Set(ResourceType.Wood, 10);
            int fired = 0;
            _eventBus.Subscribe<ProductionStalledEvent>(e => fired++);

            _production.Tick(8f);

            Assert.AreEqual(0, fired);
        }
    }
}
