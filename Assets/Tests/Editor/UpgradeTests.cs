using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class UpgradeTests
    {
        private EventBus _events;
        private PrestigeSystem _prestige;
        private ConstructionSystem _construction;
        private Dictionary<int, PlayerResources> _resources;
        private UpgradeSystem _upgrades;

        [SetUp]
        public void Setup()
        {
            Building.ResetIdCounter();
            _events = new EventBus();
            _prestige = new PrestigeSystem(5, _events);
            _construction = new ConstructionSystem(_events, 10f);
            _construction.SetConstructorCount(0, 1);
            _resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events)
            };
            _resources[0].Set(ResourceType.Planks, 50);
            _resources[0].Set(ResourceType.Stone, 50);
            _upgrades = new UpgradeSystem(_construction, _resources, _prestige, _events, 10f);
        }

        private Building PlaceAndComplete(BaseBuildingType type)
        {
            var b = _construction.PlaceBuilding(type, 0, 0, 3, 0, 0, 0, 10);
            // Complete construction instantly
            for (int i = 0; i < 200; i++)
                _construction.Tick(0.1f);
            Assert.AreEqual(BuildingState.Complete, b.State);
            return b;
        }

        [Test]
        public void Residence_CanUpgrade_AfterPrestigeUnlock()
        {
            var b = PlaceAndComplete(BaseBuildingType.Residence);
            _prestige.AwardPoints(0, 5);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            bool result = _upgrades.TryStartUpgrade(b.Id);
            Assert.IsTrue(result);
            Assert.AreEqual(BuildingState.Upgrading, b.State);
        }

        [Test]
        public void Residence_CannotUpgrade_WithoutPrestigeUnlock()
        {
            var b = PlaceAndComplete(BaseBuildingType.Residence);
            Assert.IsFalse(_upgrades.TryStartUpgrade(b.Id));
        }

        [Test]
        public void Upgrade_CompletesAfterTicking()
        {
            var b = PlaceAndComplete(BaseBuildingType.Residence);
            _prestige.AwardPoints(0, 5);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            _upgrades.TryStartUpgrade(b.Id);

            // Tick until upgrade completes
            for (int i = 0; i < 200; i++)
                _upgrades.Tick(0.1f);

            Assert.AreEqual(BuildingState.Complete, b.State);
            Assert.AreEqual(1, b.UpgradeLevel);
        }

        [Test]
        public void Upgrade_IncreasesPopulation_Residence()
        {
            var b = PlaceAndComplete(BaseBuildingType.Residence);
            Assert.AreEqual(4, b.GetBasePopulation()); // Base

            _prestige.AwardPoints(0, 5);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            _upgrades.TryStartUpgrade(b.Id);
            for (int i = 0; i < 200; i++)
                _upgrades.Tick(0.1f);

            Assert.AreEqual(8, b.GetBasePopulation()); // 4 base + 4 upgrade
        }

        [Test]
        public void Upgrade_IncreasesPopulation_NobleResidence()
        {
            var b = PlaceAndComplete(BaseBuildingType.NobleResidence);
            Assert.AreEqual(5, b.GetBasePopulation());

            // Need eco_noble_residence then eco_noble_upgrade
            _prestige.AwardPoints(0, 20);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            _prestige.TryUnlock(0, "eco_noble_residence");
            _prestige.TryUnlock(0, "eco_noble_upgrade");
            _upgrades.TryStartUpgrade(b.Id);
            for (int i = 0; i < 200; i++)
                _upgrades.Tick(0.1f);

            Assert.AreEqual(10, b.GetBasePopulation()); // 5 base + 5 upgrade
        }

        [Test]
        public void Upgrade_CostsResources()
        {
            var b = PlaceAndComplete(BaseBuildingType.Residence);
            int planksBefore = _resources[0].Get(ResourceType.Planks);
            int stoneBefore = _resources[0].Get(ResourceType.Stone);

            _prestige.AwardPoints(0, 5);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            _upgrades.TryStartUpgrade(b.Id);

            // Level 1 upgrade: 2*1 planks, 1*1 stone
            Assert.AreEqual(planksBefore - 2, _resources[0].Get(ResourceType.Planks));
            Assert.AreEqual(stoneBefore - 1, _resources[0].Get(ResourceType.Stone));
        }

        [Test]
        public void Upgrade_FailsWithInsufficientResources()
        {
            var b = PlaceAndComplete(BaseBuildingType.Residence);
            _resources[0].Set(ResourceType.Planks, 0);
            _resources[0].Set(ResourceType.Stone, 0);

            _prestige.AwardPoints(0, 5);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            Assert.IsFalse(_upgrades.TryStartUpgrade(b.Id));
        }

        [Test]
        public void Upgrade_FiresEvent()
        {
            int firedLevel = -1;
            _events.Subscribe<BuildingUpgradedEvent>(e => firedLevel = e.NewLevel);

            var b = PlaceAndComplete(BaseBuildingType.Residence);
            _prestige.AwardPoints(0, 5);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            _upgrades.TryStartUpgrade(b.Id);
            for (int i = 0; i < 200; i++)
                _upgrades.Tick(0.1f);

            Assert.AreEqual(1, firedLevel);
        }

        [Test]
        public void MaxUpgradeLevel_PreventsFurtherUpgrade()
        {
            var b = PlaceAndComplete(BaseBuildingType.Residence);
            _prestige.AwardPoints(0, 20);
            _prestige.TryUnlock(0, "eco_residence_upgrade");

            // Upgrade twice (max for Residence)
            for (int up = 0; up < 2; up++)
            {
                _upgrades.TryStartUpgrade(b.Id);
                for (int i = 0; i < 200; i++)
                    _upgrades.Tick(0.1f);
            }

            Assert.AreEqual(2, b.UpgradeLevel);
            Assert.IsFalse(b.CanUpgrade);
            Assert.IsFalse(_upgrades.TryStartUpgrade(b.Id));
        }

        [Test]
        public void Lodge_CannotUpgrade()
        {
            var b = PlaceAndComplete(BaseBuildingType.Lodge);
            Assert.AreEqual(0, b.MaxUpgradeLevel);
            Assert.IsFalse(b.CanUpgrade);
        }
    }

    [TestFixture]
    public class PavedRoadTests
    {
        private SectorGraph _graph;
        private LogisticsSystem _logistics;
        private EventBus _events;

        [SetUp]
        public void Setup()
        {
            Storehouse.ResetIdCounter();
            _events = new EventBus();
            _graph = TestMapFactory.CreateSixSectorMap();
            _logistics = new LogisticsSystem(_graph, 3, _events);
            _logistics.PlaceStorehouse(0, 0);
            _logistics.PlaceStorehouse(1, 1);
        }

        [Test]
        public void BuildPavedRoad_BetweenAdjacentSectors()
        {
            Assert.IsTrue(_logistics.BuildPavedRoad(0, 1));
            Assert.IsTrue(_logistics.IsPaved(0, 1));
            Assert.IsTrue(_logistics.IsPaved(1, 0)); // Bidirectional
        }

        [Test]
        public void BuildPavedRoad_FailsForNonAdjacent()
        {
            // Sectors 0 and 3 are not adjacent in the test map
            Assert.IsFalse(_logistics.BuildPavedRoad(0, 3));
        }

        [Test]
        public void BuildPavedRoad_DuplicateReturnsFalse()
        {
            _logistics.BuildPavedRoad(0, 1);
            Assert.IsFalse(_logistics.BuildPavedRoad(0, 1));
        }

        [Test]
        public void PavedRoad_ReducesTravelTime()
        {
            // Normal delivery: path count * 3s
            // Paved delivery: path count * 1.5s
            _logistics.BuildPavedRoad(0, 1);
            bool dispatched = _logistics.RequestDelivery(0, 1, ResourceType.Planks, 1);
            Assert.IsTrue(dispatched);

            var task = _logistics.ActiveTasks[0];
            // Path from 0→1 is 2 sectors, 1 hop. Paved = 1.5s
            Assert.AreEqual(1.5f, task.TotalTravelTime, 0.01f);
        }

        [Test]
        public void UnpavedRoad_NormalTravelTime()
        {
            bool dispatched = _logistics.RequestDelivery(0, 1, ResourceType.Planks, 1);
            Assert.IsTrue(dispatched);

            var task = _logistics.ActiveTasks[0];
            // Path from 0→1 is 2 sectors, 1 hop. Normal = 3s
            Assert.AreEqual(3f, task.TotalTravelTime, 0.01f);
        }

        [Test]
        public void PavedRoadCount_TracksCorrectly()
        {
            Assert.AreEqual(0, _logistics.PavedRoadCount);
            _logistics.BuildPavedRoad(0, 1);
            Assert.AreEqual(1, _logistics.PavedRoadCount);
        }
    }
}
