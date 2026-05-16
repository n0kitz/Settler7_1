using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for MatchResult, ScoreCalculator, and MatchHistoryPersistence.</summary>
    [TestFixture]
    public class PostGameTests
    {
        private static string GetHistoryPath() =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Settlers7", "match_history.txt");

        private void DeleteHistoryFile()
        {
            string path = GetHistoryPath();
            if (File.Exists(path)) File.Delete(path);
        }

        // --- ScoreCalculator ---

        [Test]
        public void Score_Winner_GetsVictoryBonus()
        {
            int score = ScoreCalculator.Calculate(true, 1200f, 3, 2);
            Assert.Greater(score, 1000); // victory bonus is 1000
        }

        [Test]
        public void Score_Loser_GetsNoVictoryBonus()
        {
            int winnerScore = ScoreCalculator.Calculate(true,  1200f, 3, 2);
            int loserScore  = ScoreCalculator.Calculate(false, 1200f, 3, 2);
            Assert.Greater(winnerScore, loserScore);
        }

        [Test]
        public void Score_FastVictory_GetsSpeedBonus()
        {
            int fastScore = ScoreCalculator.Calculate(true, 60f,   3, 2);
            int slowScore = ScoreCalculator.Calculate(true, 1200f, 3, 2);
            Assert.Greater(fastScore, slowScore);
        }

        [Test]
        public void Score_SectorsAndTechs_AreRewarded()
        {
            int high = ScoreCalculator.Calculate(false, 1200f, 10, 10);
            int low  = ScoreCalculator.Calculate(false, 1200f, 0,  0);
            Assert.Greater(high, low);
        }

        // --- MatchResult ---

        [Test]
        public void MatchResult_Constructor_StoresFields()
        {
            var r = new MatchResult("test_valley", 0, 300f, 2, 4,
                5, 3, 2, 1, 1200, DateTime.UtcNow);
            Assert.AreEqual("test_valley", r.MapId);
            Assert.AreEqual(0, r.WinnerId);
            Assert.AreEqual(300f, r.DurationSeconds, 0.01f);
            Assert.AreEqual(1200, r.Score);
        }

        [Test]
        public void MatchResult_From_ProducesNonNegativeScore()
        {
            var graph = TestMapFactory.CreateSixSectorMap();
            var state = new GameState(graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test");

            var stats = new PlayerStats();
            stats.Initialize(new EventBus()); // fresh bus, no events = zero counters

            var result = MatchResult.From(state, stats, 1, 600f);
            Assert.GreaterOrEqual(result.Score, 0);
        }

        // --- MatchHistoryPersistence ---

        [Test]
        public void Load_WhenNoFile_ReturnsEmptyList()
        {
            DeleteHistoryFile();
            var history = MatchHistoryPersistence.Load();
            Assert.AreEqual(0, history.Count);
        }

        [Test]
        public void Append_ThenLoad_PreservesScore()
        {
            DeleteHistoryFile();
            var r = new MatchResult("valley", 0, 300f, 2, 4, 5, 3, 2, 1, 999, DateTime.UtcNow);
            MatchHistoryPersistence.Append(r);

            var loaded = MatchHistoryPersistence.Load();
            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual(999, loaded[0].Score);
        }

        [Test]
        public void Append_ThenLoad_PreservesMapId()
        {
            DeleteHistoryFile();
            var r = new MatchResult("my_map", 0, 300f, 2, 4, 5, 3, 2, 1, 100, DateTime.UtcNow);
            MatchHistoryPersistence.Append(r);

            var loaded = MatchHistoryPersistence.Load();
            Assert.AreEqual("my_map", loaded[0].MapId);
        }

        [Test]
        public void Append_MultipleEntries_PreservesAll()
        {
            DeleteHistoryFile();
            for (int i = 0; i < 5; i++)
            {
                var r = new MatchResult("m" + i, 0, 300f, 2, 4, 0, 0, 0, 0,
                    i * 100, DateTime.UtcNow);
                MatchHistoryPersistence.Append(r);
            }
            var loaded = MatchHistoryPersistence.Load();
            Assert.AreEqual(5, loaded.Count);
        }

        [Test]
        public void Append_CapAt50Entries()
        {
            DeleteHistoryFile();
            for (int i = 0; i < 55; i++)
            {
                var r = new MatchResult("m" + i, 0, 300f, 2, 4, 0, 0, 0, 0,
                    i * 10, DateTime.UtcNow);
                MatchHistoryPersistence.Append(r);
            }
            var loaded = MatchHistoryPersistence.Load();
            Assert.AreEqual(50, loaded.Count);
        }
    }
}
