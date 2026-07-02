using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>
    /// AI victory-race awareness: the AI must notice when an opponent is
    /// about to win and mark them as the leader to contest.
    /// </summary>
    [TestFixture]
    public class AIVictoryRaceTests
    {
        private GameState _state;
        private AIController _ai;

        [SetUp]
        public void SetUp()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph = TestMapFactory.CreateSixSectorMap();
            _state = new GameState(graph, playerCount: 2,
                constructionBaseTime: 0.01f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test_valley");
            _ai = _state.AIPlayers[0]; // plays as player 1
        }

        [Test]
        public void FreshGame_NoLeaderNearWin()
        {
            _ai.AssessVictoryRace();

            Assert.IsFalse(_ai.LeaderNearWin);
        }

        [Test]
        public void OpponentOneVPFromWinning_IsFlaggedAsNearWin()
        {
            // vpRequired = 4 → 3 VPs = one away from winning
            _state.Victory.AwardPermanentVP(0, "test_vp_1");
            _state.Victory.AwardPermanentVP(0, "test_vp_2");
            _state.Victory.AwardPermanentVP(0, "test_vp_3");

            _ai.AssessVictoryRace();

            Assert.IsTrue(_ai.LeaderNearWin);
            Assert.AreEqual(0, _ai.VPLeaderId);
        }

        [Test]
        public void OpponentFarFromWinning_NotFlagged()
        {
            _state.Victory.AwardPermanentVP(0, "test_vp_1");

            _ai.AssessVictoryRace();

            Assert.IsFalse(_ai.LeaderNearWin);
            Assert.AreEqual(0, _ai.VPLeaderId);
        }

        [Test]
        public void OwnVPs_DoNotTriggerLeaderNearWin()
        {
            // The AI itself being close must not flag a leader to contest
            _state.Victory.AwardPermanentVP(1, "test_vp_1");
            _state.Victory.AwardPermanentVP(1, "test_vp_2");
            _state.Victory.AwardPermanentVP(1, "test_vp_3");

            _ai.AssessVictoryRace();

            Assert.IsFalse(_ai.LeaderNearWin);
        }
    }
}
