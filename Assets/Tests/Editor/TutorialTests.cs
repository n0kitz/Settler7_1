using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for TutorialSystem step progression and condition detection.</summary>
    [TestFixture]
    public class TutorialTests
    {
        private EventBus _bus;
        private TutorialSystem _tutorial;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _tutorial = new TutorialSystem(_bus);
        }

        [Test]
        public void Activate_SetsActiveAndFiresFirstStep()
        {
            TutorialStep received = null;
            _tutorial.OnStepStarted += s => received = s;

            _tutorial.Activate();

            Assert.IsTrue(_tutorial.IsActive);
            Assert.AreEqual(0, _tutorial.CurrentStepIndex);
            Assert.IsNotNull(received);
        }

        [Test]
        public void Advance_IncrementsStep()
        {
            _tutorial.Activate();
            int startIndex = _tutorial.CurrentStepIndex;

            _tutorial.Advance();

            Assert.AreEqual(startIndex + 1, _tutorial.CurrentStepIndex);
        }

        [Test]
        public void Advance_PastLastStep_FiresComplete()
        {
            bool completed = false;
            _tutorial.OnTutorialComplete += () => completed = true;
            _tutorial.Activate();

            while (!_tutorial.IsComplete)
                _tutorial.Advance();

            Assert.IsTrue(completed);
            Assert.IsFalse(_tutorial.IsActive);
        }

        [Test]
        public void Skip_DeactivatesAndFiresSkipped()
        {
            bool skipped = false;
            _tutorial.OnTutorialSkipped += () => skipped = true;
            _tutorial.Activate();

            _tutorial.Skip();

            Assert.IsFalse(_tutorial.IsActive);
            Assert.IsTrue(skipped);
        }

        [Test]
        public void BuildingPlaced_AutoAdvancesOnMatchingCondition()
        {
            _tutorial.Activate();
            // Step 0 expects PlaceBuilding (any building type by default)
            var step0 = _tutorial.CurrentStep;
            Assert.AreEqual(TutorialConditionType.PlaceBuilding, step0.Condition);

            _bus.Publish(new BuildingPlacedEvent(1, 0, BaseBuildingType.Lodge));

            Assert.AreEqual(1, _tutorial.CurrentStepIndex);
        }

        [Test]
        public void ConquerSector_AdvancesOnConquerStep()
        {
            _tutorial.Activate();
            // Skip to the conquer step (index 3)
            while (_tutorial.CurrentStep?.Condition != TutorialConditionType.ConquerSector
                   && !_tutorial.IsComplete)
                _tutorial.Advance();

            Assert.IsFalse(_tutorial.IsComplete, "Should find ConquerSector step");
            _bus.Publish(new SectorConqueredEvent(1, 0, Sector.NEUTRAL, ConquestMethod.Military));

            Assert.AreNotEqual(TutorialConditionType.ConquerSector,
                _tutorial.CurrentStep?.Condition ?? TutorialConditionType.None);
        }

        [Test]
        public void TutorialMapFactory_CreatesValidGraph()
        {
            var mapInfo = TutorialMapFactory.Create();
            Assert.AreEqual("tutorial", mapInfo.Id);
            Assert.AreEqual(1, mapInfo.PlayerCount);
            Assert.Greater(mapInfo.Graph.SectorCount, 0);
            // Player 0 must own at least one sector
            bool hasPlayerSector = false;
            for (int i = 0; i < mapInfo.Graph.SectorCount; i++)
                if (mapInfo.Graph.GetSector(i).OwnerId == 0) hasPlayerSector = true;
            Assert.IsTrue(hasPlayerSector);
        }
    }
}
