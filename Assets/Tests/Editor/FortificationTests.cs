using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class FortificationTests
    {
        private SectorGraph _graph;
        private EventBus _events;
        private PrestigeSystem _prestige;
        private Dictionary<int, PlayerResources> _resources;
        private TechEffects _techEffects;
        private ResearchSystem _research;
        private FortificationSystem _fortification;

        [SetUp]
        public void Setup()
        {
            _graph = new SectorGraph();
            _graph.AddSector(new Sector(0, "Home", 0, 0, false,
                new List<ResourceNodeType> { ResourceNodeType.Forest }, 8));
            _graph.AddSector(new Sector(1, "Enemy", 1, 0, false,
                new List<ResourceNodeType>(), 6));
            _graph.AddEdge(0, 1);

            _events = new EventBus();
            _prestige = new PrestigeSystem(5, _events);
            _resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events),
                [1] = new PlayerResources(1, _events)
            };
            _resources[0].Set(ResourceType.Stone, 50);
            _research = new ResearchSystem(_events);
            _techEffects = new TechEffects(_research);
            _fortification = new FortificationSystem(_graph, _prestige, _resources, _events);
            _fortification.SetTechEffects(_techEffects);
        }

        [Test]
        public void CannotBuild_WithoutPrestigeUnlock()
        {
            bool result = _fortification.StartFortification(0, 0);
            Assert.IsFalse(result);
        }

        [Test]
        public void CanBuild_WithPrestigeUnlock()
        {
            UnlockFortification();
            bool result = _fortification.StartFortification(0, 0);
            Assert.IsTrue(result);
            Assert.AreEqual(1, _fortification.ActiveTasks.Count);
        }

        [Test]
        public void Build_CostsStone()
        {
            UnlockFortification();
            int before = _resources[0].Get(ResourceType.Stone);
            _fortification.StartFortification(0, 0);
            Assert.AreEqual(before - 10, _resources[0].Get(ResourceType.Stone));
        }

        [Test]
        public void Build_CompletesAfterTime()
        {
            UnlockFortification();
            _fortification.StartFortification(0, 0);

            Assert.IsFalse(_graph.GetSector(0).IsFortified);

            // Tick past build time (30 seconds)
            _fortification.Tick(31f);

            Assert.IsTrue(_graph.GetSector(0).IsFortified);
            Assert.AreEqual(0, _fortification.ActiveTasks.Count);
        }

        [Test]
        public void CannotBuild_OnEnemySector()
        {
            UnlockFortification();
            bool result = _fortification.StartFortification(0, 1);
            Assert.IsFalse(result);
        }

        [Test]
        public void CannotBuild_AlreadyFortified()
        {
            UnlockFortification();
            _graph.GetSector(0).SetFortified(true);
            bool result = _fortification.StartFortification(0, 0);
            Assert.IsFalse(result);
        }

        [Test]
        public void CannotBuild_AlreadyBuilding()
        {
            UnlockFortification();
            _fortification.StartFortification(0, 0);
            bool result = _fortification.StartFortification(0, 0);
            Assert.IsFalse(result);
        }

        [Test]
        public void CannotBuild_InsufficientStone()
        {
            UnlockFortification();
            _resources[0].Set(ResourceType.Stone, 5);
            bool result = _fortification.StartFortification(0, 0);
            Assert.IsFalse(result);
        }

        [Test]
        public void FortificationTech_BuildsFaster()
        {
            UnlockFortification();
            // Research fortification tech for 2x speed
            _research.StartResearch(0, "tech_masonry");
            _research.Tick(1000f);
            _research.StartResearch(0, "tech_fortification_tech");
            _research.Tick(1000f);

            _fortification.StartFortification(0, 0);

            // Should complete in 15s instead of 30s (2x speed)
            _fortification.Tick(14f);
            Assert.IsFalse(_graph.GetSector(0).IsFortified);

            _fortification.Tick(2f);
            Assert.IsTrue(_graph.GetSector(0).IsFortified);
        }

        [Test]
        public void FortificationBuilt_PublishesEvent()
        {
            UnlockFortification();
            bool eventFired = false;
            _events.Subscribe<FortificationBuiltEvent>(e =>
            {
                eventFired = true;
                Assert.AreEqual(0, e.PlayerId);
                Assert.AreEqual(0, e.SectorId);
            });

            _fortification.StartFortification(0, 0);
            _fortification.Tick(31f);

            Assert.IsTrue(eventFired);
        }

        private void UnlockFortification()
        {
            _prestige.AwardPoints(0, 5); // Level 1
            _prestige.TryUnlock(0, "mil_fortification");
        }
    }
}
