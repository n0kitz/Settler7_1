using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class InputReservationTests
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
            _resources[0].Set(ResourceType.Wood, 5);
            _resources[0].Set(ResourceType.IronOre, 5);
            _resources[0].Set(ResourceType.Coal, 5);

            _construction = new ConstructionSystem(_eventBus, baseConstructionTime: 0.01f);
            _construction.SetConstructorCount(0, 10);
            _production = new ProductionSystem(_resources, _construction, _eventBus);
        }

        private Building CreateCompleteBuilding(BaseBuildingType type)
        {
            var b = _construction.PlaceBuilding(type, 0, 0, 3, 0, 0, 0, 99);
            _construction.Tick(1f);
            return b;
        }

        private WorkYard CreateOperationalWorkYard(Building building, string typeId)
        {
            var wy = new WorkYard(typeId, building.Id, 0, 0, ResourceNodeType.None, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);
            return wy;
        }

        [Test]
        public void TwoWorkYards_CompeteForSameInput_OnlyOneReserves()
        {
            // Both sawmills need Wood. Only 1 Wood available.
            _resources[0].Set(ResourceType.Wood, 1);

            var building1 = CreateCompleteBuilding(BaseBuildingType.Lodge);
            var building2 = CreateCompleteBuilding(BaseBuildingType.Lodge);

            var wy1 = CreateOperationalWorkYard(building1, "sawmill");
            var wy2 = CreateOperationalWorkYard(building2, "sawmill");

            _production.Tick(0.01f);

            // One should reserve, the other should not
            int reservedCount = (wy1.InputsReserved ? 1 : 0) + (wy2.InputsReserved ? 1 : 0);
            Assert.AreEqual(1, reservedCount,
                "Exactly one work yard should reserve the single available Wood");
            Assert.AreEqual(0, _resources[0].Get(ResourceType.Wood),
                "The Wood should be consumed by reservation");
        }

        [Test]
        public void ReservedInputs_ProduceOutput_OnCompletion()
        {
            _resources[0].Set(ResourceType.Wood, 3);

            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);
            var wy = CreateOperationalWorkYard(building, "sawmill");

            int planksBefore = _resources[0].Get(ResourceType.Planks);

            // Tick enough to complete the cycle (sawmill: 8s)
            _production.Tick(8f);

            // Should have consumed 1 wood and produced 2 planks
            Assert.AreEqual(planksBefore + 2, _resources[0].Get(ResourceType.Planks));
            Assert.IsFalse(wy.InputsReserved, "Reservation should be cleared after completion");
        }

        [Test]
        public void NoInputRecipe_SkipsReservation()
        {
            // Forester has no inputs
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);
            var wy = CreateOperationalWorkYard(building, "forester");

            int woodBefore = _resources[0].Get(ResourceType.Wood);
            _production.Tick(8f); // Full cycle

            // Should produce wood without reserving any inputs
            Assert.Greater(_resources[0].Get(ResourceType.Wood), woodBefore);
        }

        [Test]
        public void WorkYard_ResetCycle_ClearsReservation()
        {
            var wy = new WorkYard("test", 0, 0, 0, ResourceNodeType.None, 0, 0);
            wy.InputsReserved = true;
            wy.ResetCycle();

            Assert.IsFalse(wy.InputsReserved);
            Assert.AreEqual(0f, wy.CycleProgress);
        }
    }
}
