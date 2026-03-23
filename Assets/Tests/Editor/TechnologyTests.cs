using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class TechnologyTests
    {
        private EventBus _events;
        private ResearchSystem _research;

        [SetUp]
        public void Setup()
        {
            _events = new EventBus();
            _research = new ResearchSystem(_events);
        }

        // --- Tech Tree Database ---

        [Test]
        public void TechTree_Has18Techs()
        {
            Assert.AreEqual(18, TechTree.All.Count);
        }

        [Test]
        public void TechTree_6PerTier()
        {
            Assert.AreEqual(6, TechTree.GetTier(TechTree.TechTier.Tier1).Count);
            Assert.AreEqual(6, TechTree.GetTier(TechTree.TechTier.Tier2).Count);
            Assert.AreEqual(6, TechTree.GetTier(TechTree.TechTier.Tier3).Count);
        }

        [Test]
        public void TechTree_GetById()
        {
            var tech = TechTree.Get("tech_plowing");
            Assert.IsNotNull(tech);
            Assert.AreEqual("Plowing", tech.DisplayName);
            Assert.AreEqual(TechTree.TechTier.Tier1, tech.Tier);
        }

        // --- Research ---

        [Test]
        public void StartResearch_Succeeds()
        {
            Assert.IsTrue(_research.StartResearch(0, "tech_plowing"));
            Assert.AreEqual(1, _research.ActiveTasks.Count);
        }

        [Test]
        public void StartResearch_BlocksTechForOthers()
        {
            _research.StartResearch(0, "tech_plowing");
            Assert.IsFalse(_research.StartResearch(1, "tech_plowing"));
            Assert.IsTrue(_research.IsBlocked("tech_plowing"));
        }

        [Test]
        public void Research_CompletesAfterTicking()
        {
            string completed = null;
            _events.Subscribe<TechResearchedEvent>(e => completed = e.TechId);
            _research.StartResearch(0, "tech_plowing");
            for (int i = 0; i < 600; i++) _research.Tick(0.1f);
            Assert.AreEqual("tech_plowing", completed);
            Assert.IsTrue(_research.HasTech(0, "tech_plowing"));
        }

        [Test]
        public void CompletedTech_GloballyLocked()
        {
            _research.StartResearch(0, "tech_plowing");
            for (int i = 0; i < 600; i++) _research.Tick(0.1f);
            Assert.IsTrue(_research.IsResearchedGlobally("tech_plowing"));
            Assert.IsFalse(_research.StartResearch(1, "tech_plowing"));
        }

        [Test]
        public void Tier2_RequiresTier1Prerequisite()
        {
            Assert.IsFalse(_research.StartResearch(0, "tech_crop_rotation"));
            // Research prerequisite first
            _research.StartResearch(0, "tech_plowing");
            for (int i = 0; i < 600; i++) _research.Tick(0.1f);
            Assert.IsTrue(_research.StartResearch(0, "tech_crop_rotation"));
        }

        [Test]
        public void CancelResearch_FreesTech()
        {
            _research.StartResearch(0, "tech_plowing");
            Assert.IsTrue(_research.CancelResearch(0, "tech_plowing"));
            Assert.IsFalse(_research.IsBlocked("tech_plowing"));
            Assert.IsTrue(_research.StartResearch(1, "tech_plowing"));
        }

        [Test]
        public void CancelResearch_OtherPlayerCannot()
        {
            _research.StartResearch(0, "tech_plowing");
            Assert.IsFalse(_research.CancelResearch(1, "tech_plowing"));
        }

        [Test]
        public void GetTechCount_Correct()
        {
            _research.StartResearch(0, "tech_plowing");
            for (int i = 0; i < 600; i++) _research.Tick(0.1f);
            _research.StartResearch(0, "tech_masonry");
            for (int i = 0; i < 600; i++) _research.Tick(0.1f);
            Assert.AreEqual(2, _research.GetTechCount(0));
        }

        [Test]
        public void DuplicateResearch_Fails()
        {
            _research.StartResearch(0, "tech_plowing");
            for (int i = 0; i < 600; i++) _research.Tick(0.1f);
            Assert.IsFalse(_research.StartResearch(0, "tech_plowing"));
        }

        [Test]
        public void ResearchStarted_FiresEvent()
        {
            string started = null;
            _events.Subscribe<ResearchStartedEvent>(e => started = e.TechId);
            _research.StartResearch(0, "tech_plowing");
            Assert.AreEqual("tech_plowing", started);
        }
    }
}
