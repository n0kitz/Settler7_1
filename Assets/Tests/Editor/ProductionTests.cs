using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class ProductionTests
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
            _resources[0].Set(ResourceType.Tools, 10);
            _resources[0].Set(ResourceType.Wood, 10);
            _resources[0].Set(ResourceType.IronOre, 10);
            _resources[0].Set(ResourceType.Coal, 10);

            _construction = new ConstructionSystem(_eventBus, baseConstructionTime: 0.1f);
            _construction.SetConstructorCount(0, 10);
            _production = new ProductionSystem(_resources, _construction, _eventBus);
        }

        private Building CreateCompleteBuilding(BaseBuildingType type)
        {
            var b = _construction.PlaceBuilding(type, 0, 0, 3, 0, 0, 0, 99);
            _construction.Tick(1f); // Complete instantly (0.1s base)
            Assert.AreEqual(BuildingState.Complete, b.State);
            return b;
        }

        [Test]
        public void RecipeDatabase_Has30Recipes()
        {
            Assert.AreEqual(30, RecipeDatabase.All.Count);
        }

        [Test]
        public void RecipeDatabase_GetForBuilding_Lodge_Returns5()
        {
            var lodgeRecipes = RecipeDatabase.GetForBuilding(BaseBuildingType.Lodge);
            Assert.AreEqual(5, lodgeRecipes.Count);
        }

        [Test]
        public void RecipeDatabase_GetForBuilding_NobleResidence_Returns6()
        {
            var nobleRecipes = RecipeDatabase.GetForBuilding(BaseBuildingType.NobleResidence);
            Assert.AreEqual(6, nobleRecipes.Count);
        }

        [Test]
        public void RecipeDatabase_LookupById_Forester()
        {
            var recipe = RecipeDatabase.Get("forester");
            Assert.IsNotNull(recipe);
            Assert.AreEqual("Forester", recipe.DisplayName);
            Assert.AreEqual(BaseBuildingType.Lodge, recipe.ParentBuilding);
            Assert.AreEqual(ResourceNodeType.Forest, recipe.RequiredNode);
            Assert.AreEqual(0, recipe.Inputs.Length);
            Assert.AreEqual(1, recipe.Outputs.Length);
            Assert.AreEqual(ResourceType.Wood, recipe.Outputs[0].type);
        }

        [Test]
        public void RecipeDatabase_IronSmelter_Requires_IronOre_And_Coal()
        {
            var recipe = RecipeDatabase.Get("iron_smelter");
            Assert.IsNotNull(recipe);
            Assert.AreEqual(2, recipe.Inputs.Length);
            Assert.AreEqual(ResourceType.IronOre, recipe.Inputs[0].type);
            Assert.AreEqual(ResourceType.Coal, recipe.Inputs[1].type);
            Assert.AreEqual(ResourceType.IronBars, recipe.Outputs[0].type);
        }

        [Test]
        public void Production_OperationalWorkYard_ProducesOutput()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);

            // Sawmill: Wood → 2 Planks, 8s cycle
            var wy = new WorkYard("sawmill", building.Id, 0, 0, ResourceNodeType.None, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            int planksBefore = _resources[0].Get(ResourceType.Planks);
            int woodBefore = _resources[0].Get(ResourceType.Wood);

            // Tick for full cycle (8 seconds)
            _production.Tick(8f);

            Assert.AreEqual(woodBefore - 1, _resources[0].Get(ResourceType.Wood));
            Assert.AreEqual(planksBefore + 2, _resources[0].Get(ResourceType.Planks));
        }

        [Test]
        public void Production_NoWorker_DoesNotProduce()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);

            var wy = new WorkYard("forester", building.Id, 0, 0, ResourceNodeType.Forest, 0, 0);
            building.AttachWorkYard(wy);
            // No worker assigned
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            int woodBefore = _resources[0].Get(ResourceType.Wood);
            _production.Tick(20f);

            Assert.AreEqual(woodBefore, _resources[0].Get(ResourceType.Wood));
        }

        [Test]
        public void Production_NoTool_DoesNotProduce()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);

            var wy = new WorkYard("forester", building.Id, 0, 0, ResourceNodeType.Forest, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            // No tool
            _production.RegisterWorkYard(wy);

            int woodBefore = _resources[0].Get(ResourceType.Wood);
            _production.Tick(20f);

            Assert.AreEqual(woodBefore, _resources[0].Get(ResourceType.Wood));
        }

        [Test]
        public void Production_MissingInputs_DoesNotAdvanceCycle()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.MountainShelter);

            // Iron Smelter needs IronOre + Coal
            _resources[0].Set(ResourceType.IronOre, 0);
            _resources[0].Set(ResourceType.Coal, 0);

            var wy = new WorkYard("iron_smelter", building.Id, 0, 0, ResourceNodeType.None, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            int ironBarsBefore = _resources[0].Get(ResourceType.IronBars);
            _production.Tick(20f);

            Assert.AreEqual(ironBarsBefore, _resources[0].Get(ResourceType.IronBars));
        }

        [Test]
        public void Production_FoodBoost_Plain_DoublesSpeed()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);
            building.SetFoodSetting(FoodSetting.Plain);

            // Give player some bread for food boost
            _resources[0].Set(ResourceType.Bread, 10);

            // Forester: no inputs, produces Wood, 8s cycle
            var wy = new WorkYard("forester", building.Id, 0, 0, ResourceNodeType.Forest, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            int woodBefore = _resources[0].Get(ResourceType.Wood);

            // With x2 boost, 8s cycle should complete in 4s of real time
            _production.Tick(4f);

            Assert.AreEqual(woodBefore + 1, _resources[0].Get(ResourceType.Wood));
        }

        [Test]
        public void Production_NobleRes_NoFood_Halts()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.NobleResidence);
            // FoodSetting.None = IDLE for Noble Residence

            var wy = new WorkYard("butcher", building.Id, 0, 0, ResourceNodeType.None, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            _resources[0].Set(ResourceType.Animal, 10);
            int sausagesBefore = _resources[0].Get(ResourceType.Sausages);

            _production.Tick(50f); // Even a long tick should produce nothing

            Assert.AreEqual(sausagesBefore, _resources[0].Get(ResourceType.Sausages));
        }

        [Test]
        public void Production_FoodToggled_NoFoodAvailable_Halts()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);
            building.SetFoodSetting(FoodSetting.Plain);

            // No bread or fish available
            _resources[0].Set(ResourceType.Bread, 0);
            _resources[0].Set(ResourceType.Fish, 0);

            var wy = new WorkYard("forester", building.Id, 0, 0, ResourceNodeType.Forest, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            int woodBefore = _resources[0].Get(ResourceType.Wood);
            _production.Tick(20f);

            Assert.AreEqual(woodBefore, _resources[0].Get(ResourceType.Wood));
        }

        [Test]
        public void Production_FiresProductionCompleteEvent()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);

            var wy = new WorkYard("forester", building.Id, 0, 0, ResourceNodeType.Forest, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            ProductionCompleteEvent? received = null;
            _eventBus.Subscribe<ProductionCompleteEvent>(e => received = e);

            _production.Tick(8f); // Full cycle

            Assert.IsNotNull(received);
            Assert.AreEqual("forester", received.Value.WorkYardId);
            Assert.AreEqual(0, received.Value.PlayerId);
        }

        // --- Input Reservation Tests ---

        [Test]
        public void Production_InputsReservedAtCycleStart()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);

            // Sawmill: Wood → 2 Planks
            var wy = new WorkYard("sawmill", building.Id, 0, 0, ResourceNodeType.None, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            int woodBefore = _resources[0].Get(ResourceType.Wood);

            // Tick just a tiny bit — should consume input immediately
            _production.Tick(0.1f);

            // Wood consumed at cycle start (reserved)
            Assert.AreEqual(woodBefore - 1, _resources[0].Get(ResourceType.Wood));
            Assert.IsTrue(wy.InputsReserved);
        }

        [Test]
        public void Production_InputsNotReReservedMidCycle()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);

            var wy = new WorkYard("sawmill", building.Id, 0, 0, ResourceNodeType.None, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            _resources[0].Set(ResourceType.Wood, 2);

            // First tick reserves 1 wood
            _production.Tick(0.1f);
            Assert.AreEqual(1, _resources[0].Get(ResourceType.Wood));

            // Second tick should NOT consume another wood (already reserved)
            _production.Tick(0.1f);
            Assert.AreEqual(1, _resources[0].Get(ResourceType.Wood));
        }

        [Test]
        public void Production_InputsReleasedOnHalt()
        {
            var building = CreateCompleteBuilding(BaseBuildingType.Lodge);
            building.SetFoodSetting(FoodSetting.Plain);
            _resources[0].Set(ResourceType.Bread, 10);

            var wy = new WorkYard("sawmill", building.Id, 0, 0, ResourceNodeType.None, 0, 0);
            building.AttachWorkYard(wy);
            wy.AssignWorker();
            wy.ProvideTool();
            _production.RegisterWorkYard(wy);

            // Start cycle (reserves inputs)
            _production.Tick(0.1f);
            Assert.IsTrue(wy.InputsReserved);

            // Remove food — should halt and reset
            _resources[0].Set(ResourceType.Bread, 0);
            _resources[0].Set(ResourceType.Fish, 0);
            _production.Tick(0.1f);

            // Reservation cleared on halt
            Assert.IsFalse(wy.InputsReserved);
        }
    }
}
