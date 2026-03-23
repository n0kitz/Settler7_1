using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>
    /// Integration tests verifying TechEffects multipliers are actually consumed
    /// by their target systems (construction, logistics, population, combat).
    /// </summary>
    [TestFixture]
    public class TechEffectsIntegrationTests
    {
        private EventBus _events;
        private ResearchSystem _research;
        private TechEffects _techEffects;

        [SetUp]
        public void Setup()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            Storehouse.ResetIdCounter();
            _events = new EventBus();
            _research = new ResearchSystem(_events);
            _techEffects = new TechEffects(_research);
        }

        // --- Construction Speed ---

        [Test]
        public void Architecture_DoublesConstructionSpeed_InSystem()
        {
            var construction = new ConstructionSystem(_events, baseConstructionTime: 10f);
            construction.SetConstructorCount(0, 1);
            construction.SetTechEffects(_techEffects);

            ResearchInstantly(0, "tech_masonry");
            ResearchInstantly(0, "tech_fortification_tech");
            ResearchInstantly(0, "tech_architecture");

            var building = construction.PlaceBuilding(
                BaseBuildingType.Lodge, 0, 0, 3, 0f, 0f, 0, 8);

            // With 2x speed, 5 seconds should complete (normally needs 10)
            construction.Tick(5f);
            Assert.AreEqual(BuildingState.Complete, building.State);
        }

        [Test]
        public void NoTech_ConstructionAtNormalSpeed()
        {
            var construction = new ConstructionSystem(_events, baseConstructionTime: 10f);
            construction.SetConstructorCount(0, 1);
            construction.SetTechEffects(_techEffects);

            var building = construction.PlaceBuilding(
                BaseBuildingType.Lodge, 0, 0, 3, 0f, 0f, 0, 8);

            construction.Tick(5f);
            Assert.AreEqual(BuildingState.UnderConstruction, building.State);
            Assert.AreEqual(0.5f, building.ConstructionProgress, 0.01f);
        }

        // --- Carrier Speed ---

        [Test]
        public void Logistics_TechReducesTravelTime()
        {
            var graph = TestMapFactory.CreateSixSectorMap();
            var logistics = new LogisticsSystem(graph, 3, _events);
            logistics.SetTechEffects(_techEffects);

            // Storehouse owned by player 0
            logistics.PlaceStorehouse(0, 0);
            logistics.PlaceStorehouse(1, 0);

            ResearchInstantly(0, "tech_carpentry");
            ResearchInstantly(0, "tech_woodworking");
            ResearchInstantly(0, "tech_logistics");

            logistics.RequestDelivery(0, 1, ResourceType.Planks, 1);

            // Normal: 3s per hop. With 1.5x speed, travel time = 3/1.5 = 2s
            logistics.Tick(2.1f);
            Assert.AreEqual(0, logistics.ActiveTasks.Count, "Carrier should arrive faster with logistics tech");
        }

        [Test]
        public void Logistics_NoTech_NormalTravelTime()
        {
            var graph = TestMapFactory.CreateSixSectorMap();
            var logistics = new LogisticsSystem(graph, 3, _events);
            logistics.SetTechEffects(_techEffects);

            logistics.PlaceStorehouse(0, 0);
            logistics.PlaceStorehouse(1, 0);

            logistics.RequestDelivery(0, 1, ResourceType.Planks, 1);

            // Normal: 3s per hop, should NOT be complete at 2s
            logistics.Tick(2f);
            Assert.AreEqual(1, logistics.ActiveTasks.Count, "Carrier should still be traveling without tech");
        }

        // --- Hygiene Population Bonus ---

        [Test]
        public void Hygiene_IncreasesLivingSpace_Residence()
        {
            var resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events)
            };
            var construction = new ConstructionSystem(_events, 0.1f);
            construction.SetConstructorCount(0, 10);
            var production = new ProductionSystem(resources, construction, _events);
            var population = new PopulationSystem(resources, construction, production, _events);
            population.SetTechEffects(_techEffects);

            // Build a Residence (base pop 4)
            var building = construction.PlaceBuilding(
                BaseBuildingType.Residence, 0, 0, 3, 0, 0, 0, 99);
            construction.Tick(1f);

            Assert.AreEqual(4, population.GetLivingSpace(0));

            // Research hygiene
            ResearchInstantly(0, "tech_fishing");
            ResearchInstantly(0, "tech_preservation");
            ResearchInstantly(0, "tech_hygiene");

            Assert.AreEqual(6, population.GetLivingSpace(0)); // 4 + 2 hygiene bonus
        }

        [Test]
        public void Hygiene_IncreasesLivingSpace_NobleResidence()
        {
            var resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events)
            };
            var construction = new ConstructionSystem(_events, 0.1f);
            construction.SetConstructorCount(0, 10);
            var production = new ProductionSystem(resources, construction, _events);
            var population = new PopulationSystem(resources, construction, production, _events);
            population.SetTechEffects(_techEffects);

            var building = construction.PlaceBuilding(
                BaseBuildingType.NobleResidence, 0, 0, 3, 0, 0, 0, 99);
            construction.Tick(1f);

            Assert.AreEqual(5, population.GetLivingSpace(0));

            ResearchInstantly(0, "tech_fishing");
            ResearchInstantly(0, "tech_preservation");
            ResearchInstantly(0, "tech_hygiene");

            Assert.AreEqual(9, population.GetLivingSpace(0)); // 5 + 4 hygiene bonus
        }

        [Test]
        public void Hygiene_NoBonus_ForLodge()
        {
            var resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events)
            };
            var construction = new ConstructionSystem(_events, 0.1f);
            construction.SetConstructorCount(0, 10);
            var production = new ProductionSystem(resources, construction, _events);
            var population = new PopulationSystem(resources, construction, production, _events);
            population.SetTechEffects(_techEffects);

            construction.PlaceBuilding(BaseBuildingType.Lodge, 0, 0, 3, 0, 0, 0, 99);
            construction.Tick(1f);

            ResearchInstantly(0, "tech_fishing");
            ResearchInstantly(0, "tech_preservation");
            ResearchInstantly(0, "tech_hygiene");

            Assert.AreEqual(1, population.GetLivingSpace(0)); // No bonus for Lodge
        }

        // --- Combat Attack Multiplier ---

        [Test]
        public void Cavalry_TechBoostsAttack_InCombat()
        {
            var graph = new SectorGraph();
            graph.AddSector(new Sector(0, "Home", 0, 0, false,
                new List<ResourceNodeType>(), 8));
            graph.AddSector(new Sector(1, "Neutral", -1, 5, false,
                new List<ResourceNodeType>(), 6));
            graph.AddEdge(0, 1);

            var combat = new CombatResolver(graph, _events);
            combat.SetTechEffects(_techEffects);

            ResearchInstantly(0, "tech_animal_husbandry");
            ResearchInstantly(0, "tech_breeding");
            ResearchInstantly(0, "tech_cavalry");

            var general = new General(0, 0, 0, 35);
            for (int i = 0; i < 5; i++)
                general.AddUnit(UnitType.Cavalier);

            var result = combat.ResolveCombat(general, 1);
            // 5 Cavaliers with 1.3x tech bonus should beat garrison of 5
            Assert.IsTrue(result.Victory);
        }

        private void ResearchInstantly(int playerId, string techId)
        {
            _research.StartResearch(playerId, techId);
            _research.Tick(1000f);
        }
    }
}
