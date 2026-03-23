using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class QuestTests
    {
        private GameState _state;
        private QuestSystem _quests;

        [SetUp]
        public void Setup()
        {
            Building.ResetIdCounter();
            Storehouse.ResetIdCounter();
            ArmySystem.ResetIdCounter();
            var graph = TestMapFactory.CreateSixSectorMap();
            _state = new GameState(graph, 2, 10f, 3);
            _quests = _state.Quests;
        }

        [Test]
        public void QuestDatabase_HasQuests()
        {
            var quests = QuestDatabase.GetQuestsForMap("test_valley");
            Assert.Greater(quests.Count, 0);
        }

        [Test]
        public void AvailableQuests_PopulatedOnStart()
        {
            Assert.Greater(_quests.AvailableQuests.Count, 0);
        }

        [Test]
        public void AcceptQuest_Succeeds()
        {
            // Player 0 owns sector 0
            bool result = _quests.AcceptQuest(0, "quest_lumber_baron");
            Assert.IsTrue(result);
            var active = _quests.GetActiveQuests(0);
            Assert.AreEqual(1, active.Count);
        }

        [Test]
        public void AcceptQuest_FiresEvent()
        {
            string accepted = null;
            _state.Events.Subscribe<QuestAcceptedEvent>(e => accepted = e.QuestId);
            _quests.AcceptQuest(0, "quest_lumber_baron");
            Assert.AreEqual("quest_lumber_baron", accepted);
        }

        [Test]
        public void CompleteQuest_DeliverResource()
        {
            _state.PlayerResources[0].Set(ResourceType.Planks, 30);
            _quests.AcceptQuest(0, "quest_lumber_baron");

            bool result = _quests.TryCompleteQuest(0, "quest_lumber_baron");
            Assert.IsTrue(result);
            Assert.IsTrue(_quests.IsCompleted("quest_lumber_baron"));
            // Should have spent 20 planks
            Assert.AreEqual(10, _state.PlayerResources[0].Get(ResourceType.Planks));
        }

        [Test]
        public void CompleteQuest_GrantsReward()
        {
            _state.PlayerResources[0].Set(ResourceType.Planks, 30);
            int toolsBefore = _state.PlayerResources[0].Get(ResourceType.Tools);
            int prestigeBefore = _state.Prestige.GetPoints(0);

            _quests.AcceptQuest(0, "quest_lumber_baron");
            _quests.TryCompleteQuest(0, "quest_lumber_baron");

            Assert.AreEqual(toolsBefore + 5, _state.PlayerResources[0].Get(ResourceType.Tools));
            Assert.AreEqual(prestigeBefore + 3, _state.Prestige.GetPoints(0));
        }

        [Test]
        public void CompleteQuest_FailsWithInsufficientResources()
        {
            _state.PlayerResources[0].Set(ResourceType.Planks, 5); // Need 20
            _quests.AcceptQuest(0, "quest_lumber_baron");
            Assert.IsFalse(_quests.TryCompleteQuest(0, "quest_lumber_baron"));
        }

        [Test]
        public void CompleteQuest_FiresEvent()
        {
            string completed = null;
            _state.Events.Subscribe<QuestCompletedEvent>(e => completed = e.QuestId);
            _state.PlayerResources[0].Set(ResourceType.Planks, 30);
            _quests.AcceptQuest(0, "quest_lumber_baron");
            _quests.TryCompleteQuest(0, "quest_lumber_baron");
            Assert.AreEqual("quest_lumber_baron", completed);
        }

        [Test]
        public void Quest_CannotBeAcceptedTwice()
        {
            _quests.AcceptQuest(0, "quest_lumber_baron");
            Assert.IsFalse(_quests.AcceptQuest(0, "quest_lumber_baron"));
        }

        [Test]
        public void CompletedQuest_CannotBeReaccepted()
        {
            _state.PlayerResources[0].Set(ResourceType.Planks, 30);
            _quests.AcceptQuest(0, "quest_lumber_baron");
            _quests.TryCompleteQuest(0, "quest_lumber_baron");
            Assert.IsFalse(_quests.AcceptQuest(0, "quest_lumber_baron"));
        }

        [Test]
        public void Quest_VPReward()
        {
            // Expansionist quest: own 4 sectors
            _state.Graph.GetSector(2).SetOwner(0);
            _state.Graph.GetSector(4).SetOwner(0);
            _state.Graph.GetSector(5).SetOwner(0);
            // Player 0 now owns sectors 0,2,4,5 = 4

            _quests.AcceptQuest(0, "quest_expansionist");
            bool result = _quests.TryCompleteQuest(0, "quest_expansionist");
            Assert.IsTrue(result);
            Assert.IsTrue(_state.Victory.HasVP(0, "vp_quest_expansionist"));
        }
    }
}
