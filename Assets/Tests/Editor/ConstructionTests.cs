using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class ConstructionTests
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
        public void PlaceBuilding_ReturnsBuildingWithPlannedState()
        {
            var building = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            Assert.IsNotNull(building);
            Assert.AreEqual(BuildingState.Planned, building.State);
            Assert.AreEqual(0f, building.ConstructionProgress);
            Assert.AreEqual(BaseBuildingType.Lodge, building.Type);
        }

        [Test]
        public void PlaceBuilding_ReturnsNull_WhenSlotsAreFull()
        {
            var building = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 8, maxSlots: 8);

            Assert.IsNull(building);
        }

        [Test]
        public void PlaceBuilding_FiresBuildingPlacedEvent()
        {
            BuildingPlacedEvent? received = null;
            _eventBus.Subscribe<BuildingPlacedEvent>(e => received = e);

            _construction.PlaceBuilding(
                BaseBuildingType.Farm, sectorId: 2, ownerId: 0,
                maxWorkYards: 3, localX: 5f, localZ: 3f,
                currentBuildCount: 0, maxSlots: 6);

            Assert.IsNotNull(received);
            Assert.AreEqual(2, received.Value.SectorId);
            Assert.AreEqual(BaseBuildingType.Farm, received.Value.BuildingType);
        }

        [Test]
        public void Tick_AdvancesConstructionProgress()
        {
            var building = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            _construction.Tick(5f); // 5 seconds / 10 base = 50%

            Assert.AreEqual(BuildingState.UnderConstruction, building.State);
            Assert.AreEqual(0.5f, building.ConstructionProgress, 0.001f);
        }

        [Test]
        public void Tick_CompletesConstruction_AfterBaseTime()
        {
            var building = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            _construction.Tick(10f); // 10 seconds = 100%

            Assert.AreEqual(BuildingState.Complete, building.State);
            Assert.AreEqual(1f, building.ConstructionProgress);
        }

        [Test]
        public void Tick_FiresBuildingCompletedEvent_OnCompletion()
        {
            BuildingCompletedEvent? received = null;
            _eventBus.Subscribe<BuildingCompletedEvent>(e => received = e);

            _construction.PlaceBuilding(
                BaseBuildingType.Residence, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            _construction.Tick(10f);

            Assert.IsNotNull(received);
            Assert.AreEqual(0, received.Value.SectorId);
        }

        [Test]
        public void Constructor_OnlyBuildsOneAtATime()
        {
            // Player 0 has 1 constructor
            var b1 = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            var b2 = _construction.PlaceBuilding(
                BaseBuildingType.Farm, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 3f, localZ: 0f,
                currentBuildCount: 1, maxSlots: 8);

            _construction.Tick(5f);

            // First building should have progress, second should not
            Assert.AreEqual(0.5f, b1.ConstructionProgress, 0.001f);
            Assert.AreEqual(0f, b2.ConstructionProgress, 0.001f);
        }

        [Test]
        public void Constructor_MovesToNextBuilding_AfterCompletion()
        {
            var b1 = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            var b2 = _construction.PlaceBuilding(
                BaseBuildingType.Farm, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 3f, localZ: 0f,
                currentBuildCount: 1, maxSlots: 8);

            // Complete first building
            _construction.Tick(10f);
            Assert.AreEqual(BuildingState.Complete, b1.State);

            // Now constructor should work on second
            _construction.Tick(5f);
            Assert.AreEqual(0.5f, b2.ConstructionProgress, 0.001f);
        }

        [Test]
        public void MultipleConstructors_BuildInParallel()
        {
            _construction.SetConstructorCount(0, 2);

            var b1 = _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            var b2 = _construction.PlaceBuilding(
                BaseBuildingType.Farm, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 3f, localZ: 0f,
                currentBuildCount: 1, maxSlots: 8);

            _construction.Tick(5f);

            // Both should have progress with 2 constructors
            Assert.AreEqual(0.5f, b1.ConstructionProgress, 0.001f);
            Assert.AreEqual(0.5f, b2.ConstructionProgress, 0.001f);
        }

        [Test]
        public void GetBuildingsInSector_ReturnsCorrectBuildings()
        {
            _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            _construction.PlaceBuilding(
                BaseBuildingType.Farm, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 3f, localZ: 0f,
                currentBuildCount: 1, maxSlots: 8);

            _construction.PlaceBuilding(
                BaseBuildingType.Residence, sectorId: 1, ownerId: 1,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            Assert.AreEqual(2, _construction.GetBuildingsInSector(0).Count);
            Assert.AreEqual(1, _construction.GetBuildingsInSector(1).Count);
            Assert.AreEqual(0, _construction.GetBuildingsInSector(2).Count);
        }

        [Test]
        public void GetBuildingsByPlayer_ReturnsCorrectBuildings()
        {
            _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            _construction.PlaceBuilding(
                BaseBuildingType.Residence, sectorId: 1, ownerId: 1,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            Assert.AreEqual(1, _construction.GetBuildingsByPlayer(0).Count);
            Assert.AreEqual(1, _construction.GetBuildingsByPlayer(1).Count);
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

        [Test]
        public void GetQueuedCount_ReturnsCorrectCount()
        {
            _construction.PlaceBuilding(
                BaseBuildingType.Lodge, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 0f, localZ: 0f,
                currentBuildCount: 0, maxSlots: 8);

            _construction.PlaceBuilding(
                BaseBuildingType.Farm, sectorId: 0, ownerId: 0,
                maxWorkYards: 3, localX: 3f, localZ: 0f,
                currentBuildCount: 1, maxSlots: 8);

            Assert.AreEqual(2, _construction.GetQueuedCount(0));
            Assert.AreEqual(0, _construction.GetQueuedCount(1));
        }
    }
}
