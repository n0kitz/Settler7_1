using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class BuildingAndWorkYardTests
    {
        private EventBus _eventBus;
        private ConstructionSystem _construction;

        [SetUp]
        public void SetUp()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            _eventBus = new EventBus();
            _construction = new ConstructionSystem(_eventBus, baseConstructionTime: 10f);
            _construction.SetConstructorCount(0, 1);
            _construction.SetConstructorCount(1, 1);
        }

        [Test]
        public void Building_CanAttachWorkYard_WhenComplete()
        {
            var building = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            _construction.Tick(10f); // Complete it

            Assert.IsTrue(building.IsOperational);
            Assert.IsTrue(building.CanAttachWorkYard);

            var workYard = new WorkYard("forester", building.Id, 0, 0,
                ResourceNodeType.Forest, 2f, 0f);
            bool attached = building.AttachWorkYard(workYard);

            Assert.IsTrue(attached);
            Assert.AreEqual(1, building.WorkYards.Count);
        }

        [Test]
        public void Building_CannotAttachWorkYard_WhenNotComplete()
        {
            var building = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            Assert.IsFalse(building.IsOperational);
            Assert.IsFalse(building.CanAttachWorkYard);
        }

        [Test]
        public void Building_CannotExceedMaxWorkYards()
        {
            var building = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 2, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            _construction.Tick(10f); // Complete it

            building.AttachWorkYard(new WorkYard("forester", building.Id, 0, 0,
                ResourceNodeType.Forest, 2f, 0f));
            building.AttachWorkYard(new WorkYard("woodcutter", building.Id, 0, 0,
                ResourceNodeType.Forest, -2f, 0f));

            bool thirdAttached = building.AttachWorkYard(
                new WorkYard("sawmill", building.Id, 0, 0,
                    ResourceNodeType.None, 0f, 2f));

            Assert.IsFalse(thirdAttached);
            Assert.AreEqual(2, building.WorkYards.Count);
        }

        [Test]
        public void WorkYard_IsNotOperational_WithoutWorkerOrTool()
        {
            var wy = new WorkYard("forester", 0, 0, 0,
                ResourceNodeType.Forest, 0f, 0f);

            Assert.IsFalse(wy.IsOperational);

            wy.AssignWorker();
            Assert.IsFalse(wy.IsOperational); // Still needs tool

            wy.ProvideTool();
            Assert.IsTrue(wy.IsOperational);
        }

        [Test]
        public void WorkYard_CycleDoesNotAdvance_WhenNotOperational()
        {
            var wy = new WorkYard("forester", 0, 0, 0,
                ResourceNodeType.Forest, 0f, 0f);

            bool completed = wy.AdvanceCycle(0.5f);

            Assert.IsFalse(completed);
            Assert.AreEqual(0f, wy.CycleProgress);
        }

        [Test]
        public void WorkYard_CycleCompletes_WhenOperational()
        {
            var wy = new WorkYard("forester", 0, 0, 0,
                ResourceNodeType.Forest, 0f, 0f);
            wy.AssignWorker();
            wy.ProvideTool();

            bool completed = wy.AdvanceCycle(1f);

            Assert.IsTrue(completed);
            Assert.AreEqual(0f, wy.CycleProgress); // Resets after completion
        }

        [Test]
        public void EventBus_SubscribeAndPublish()
        {
            var bus = new EventBus();
            BuildingPlacedEvent? received = null;

            bus.Subscribe<BuildingPlacedEvent>(e => received = e);
            bus.Publish(new BuildingPlacedEvent(1, 2, BaseBuildingType.Farm));

            Assert.IsNotNull(received);
            Assert.AreEqual(1, received.Value.BuildingId);
            Assert.AreEqual(2, received.Value.SectorId);
        }

        [Test]
        public void EventBus_Unsubscribe_StopsReceiving()
        {
            var bus = new EventBus();
            int callCount = 0;
            void Handler(BuildingPlacedEvent e) => callCount++;

            bus.Subscribe<BuildingPlacedEvent>(Handler);
            bus.Publish(new BuildingPlacedEvent(0, 0, BaseBuildingType.Lodge));
            Assert.AreEqual(1, callCount);

            bus.Unsubscribe<BuildingPlacedEvent>(Handler);
            bus.Publish(new BuildingPlacedEvent(0, 0, BaseBuildingType.Lodge));
            Assert.AreEqual(1, callCount); // Should not increment
        }

        [Test]
        public void Building_GetBasePopulation_ReturnsCorrectValues()
        {
            Assert.AreEqual(1, new Building(BaseBuildingType.Lodge, 0, 0, 3, 0, 0).GetBasePopulation());
            Assert.AreEqual(1, new Building(BaseBuildingType.Farm, 0, 0, 3, 0, 0).GetBasePopulation());
            Assert.AreEqual(1, new Building(BaseBuildingType.MountainShelter, 0, 0, 3, 0, 0).GetBasePopulation());
            Assert.AreEqual(4, new Building(BaseBuildingType.Residence, 0, 0, 3, 0, 0).GetBasePopulation());
            Assert.AreEqual(5, new Building(BaseBuildingType.NobleResidence, 0, 0, 3, 0, 0).GetBasePopulation());
        }

        [Test]
        public void Building_FoodSetting_DefaultsToNone()
        {
            var building = new Building(BaseBuildingType.Residence, 0, 0, 3, 0, 0);
            Assert.AreEqual(FoodSetting.None, building.FoodSetting);

            building.SetFoodSetting(FoodSetting.Plain);
            Assert.AreEqual(FoodSetting.Plain, building.FoodSetting);
        }
    }
}
