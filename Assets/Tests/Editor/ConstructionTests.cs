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
