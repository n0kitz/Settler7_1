using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>
    /// Lightweight tests for Phase 10 VFX data-layer logic.
    /// The actual MonoBehaviours require PlayMode so we test pure-data helpers.
    /// </summary>
    [TestFixture]
    public class VFXTests
    {
        // --- ScoreCalculator edge cases (used by PostGameSummaryUI) ---

        [Test]
        public void ScoreCalc_ZeroStats_Winner_StillGetsVictoryBonus()
        {
            int score = ScoreCalculator.Calculate(true, 700f, 0, 0);
            Assert.AreEqual(1000, score, "Victory bonus 1000 expected with no stats");
        }

        [Test]
        public void ScoreCalc_InstantVictory_GetsMaxSpeedBonus()
        {
            int score = ScoreCalculator.Calculate(true, 0f, 0, 0);
            Assert.AreEqual(1000 + 500, score,
                "1s victory should give max speed bonus (500)");
        }

        [Test]
        public void ScoreCalc_SlowVictory_GetsNoSpeedBonus()
        {
            int score = ScoreCalculator.Calculate(true, 601f, 0, 0);
            Assert.AreEqual(1000, score,
                "Victory >= 10 min gets no speed bonus");
        }

        [Test]
        public void ScoreCalc_Sectors_AddCorrectly()
        {
            int score = ScoreCalculator.Calculate(false, 700f, 5, 0);
            Assert.AreEqual(5 * 50, score);
        }

        [Test]
        public void ScoreCalc_Techs_AddCorrectly()
        {
            int score = ScoreCalculator.Calculate(false, 700f, 0, 3);
            Assert.AreEqual(3 * 30, score);
        }

        // --- DiplomaticStatus helpers (also reachable from UI, tested here) ---

        [Test]
        public void Status_ToDisplayString_War()
        {
            Assert.AreEqual("WAR", DiplomaticStatus.War.ToDisplayString());
        }

        [Test]
        public void Status_ToDisplayString_NonAggression()
        {
            Assert.AreEqual("Non-Aggression",
                DiplomaticStatus.NonAggression.ToDisplayString());
        }

        [Test]
        public void Status_ToDisplayString_Peace()
        {
            Assert.AreEqual("Peace", DiplomaticStatus.Peace.ToDisplayString());
        }

        // --- AchievementSystem notification integration ---

        [Test]
        public void AchievementToast_TriggeredOn_UnlockedEvent()
        {
            AchievementProgress.Reset();
            var sys = new AchievementSystem();
            var bus = new EventBus();
            sys.Initialize(bus);

            string toastName = null;
            bus.Subscribe<AchievementUnlockedEvent>(e => toastName = e.Name);

            bus.Publish(new BuildingCompletedEvent(1, 0));
            Assert.IsNotNull(toastName,
                "Achievement unlocked event should fire for first building");
        }

        // --- MatchResult + History integration ---

        [Test]
        public void MatchHistory_SortByScore_HighestFirst()
        {
            MatchHistoryPersistence_DeleteFile();
            for (int i = 0; i < 3; i++)
            {
                var r = new MatchResult("m", 0, 300f, 2, 4, 0, 0, 0, 0,
                    i * 100, System.DateTime.UtcNow);
                MatchHistoryPersistence.Append(r);
            }
            var history = MatchHistoryPersistence.Load();
            history.Sort((a, b) => b.Score.CompareTo(a.Score));
            Assert.Greater(history[0].Score, history[1].Score);
            Assert.Greater(history[1].Score, history[2].Score);
        }

        private static void MatchHistoryPersistence_DeleteFile()
        {
            string path = System.IO.Path.Combine(
                System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData),
                "Settlers7", "match_history.txt");
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
