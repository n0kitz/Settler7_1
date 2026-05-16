using NUnit.Framework;
using System.IO;
using System;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for Achievement, AchievementSystem, AchievementProgress, and PlayerStats.</summary>
    [TestFixture]
    public class AchievementTests
    {
        // --- Achievement ---

        [Test]
        public void Achievement_DefaultsToLocked()
        {
            var ach = new Achievement("test", "Test", "Desc",
                AchievementConditionType.BuildingCompleted);
            Assert.IsFalse(ach.IsUnlocked);
            Assert.IsNull(ach.UnlockedAt);
        }

        [Test]
        public void Achievement_StoresFieldsCorrectly()
        {
            var ach = new Achievement("id1", "Name", "Desc",
                AchievementConditionType.TechResearched, 5);
            Assert.AreEqual("id1", ach.Id);
            Assert.AreEqual(5, ach.Threshold);
            Assert.AreEqual(AchievementConditionType.TechResearched, ach.Condition);
        }

        // --- AchievementSystem ---

        [Test]
        public void AchievementSystem_HasFifteenAchievements()
        {
            var sys = new AchievementSystem();
            Assert.AreEqual(15, sys.All.Count);
        }

        [Test]
        public void AchievementSystem_BuildingCompleted_UnlocksFirstBuilding()
        {
            AchievementProgress.Reset();
            var sys = new AchievementSystem();
            var bus = new EventBus();
            sys.Initialize(bus);

            bus.Publish(new BuildingCompletedEvent(1, 0));

            var ach = FindById(sys, "first_building");
            Assert.IsNotNull(ach);
            Assert.IsTrue(ach.IsUnlocked);
        }

        [Test]
        public void AchievementSystem_RequiresThreshold_TenBuildings()
        {
            AchievementProgress.Reset();
            var sys = new AchievementSystem();
            var bus = new EventBus();
            sys.Initialize(bus);

            for (int i = 0; i < 9; i++)
                bus.Publish(new BuildingCompletedEvent(i, 0));

            Assert.IsFalse(FindById(sys, "ten_buildings").IsUnlocked);

            bus.Publish(new BuildingCompletedEvent(9, 0));
            Assert.IsTrue(FindById(sys, "ten_buildings").IsUnlocked);
        }

        [Test]
        public void AchievementSystem_SectorConquered_OnlyCountsPlayer0()
        {
            AchievementProgress.Reset();
            var sys = new AchievementSystem();
            var bus = new EventBus();
            sys.Initialize(bus);

            // AI player conquers sector — should NOT count for player 0
            bus.Publish(new SectorConqueredEvent(1, 1, -1, ConquestMethod.Military));

            Assert.IsFalse(FindById(sys, "first_conquest").IsUnlocked);

            // Player 0 conquers sector — should count
            bus.Publish(new SectorConqueredEvent(2, 0, -1, ConquestMethod.Military));
            Assert.IsTrue(FindById(sys, "first_conquest").IsUnlocked);
        }

        [Test]
        public void AchievementSystem_PublishesUnlockedEvent()
        {
            AchievementProgress.Reset();
            var sys = new AchievementSystem();
            var bus = new EventBus();
            sys.Initialize(bus);

            string unlockedId = null;
            bus.Subscribe<AchievementUnlockedEvent>(e => unlockedId = e.Id);

            bus.Publish(new BuildingCompletedEvent(1, 0));

            Assert.AreEqual("first_building", unlockedId);
        }

        [Test]
        public void AchievementSystem_DoesNotUnlockTwice()
        {
            AchievementProgress.Reset();
            var sys = new AchievementSystem();
            var bus = new EventBus();
            sys.Initialize(bus);

            int unlockCount = 0;
            bus.Subscribe<AchievementUnlockedEvent>(_ => unlockCount++);

            bus.Publish(new BuildingCompletedEvent(1, 0));
            bus.Publish(new BuildingCompletedEvent(2, 0));

            Assert.AreEqual(1, unlockCount);
        }

        [Test]
        public void AchievementSystem_PrestigeUsesMaxValue()
        {
            AchievementProgress.Reset();
            var sys = new AchievementSystem();
            var bus = new EventBus();
            sys.Initialize(bus);

            bus.Publish(new PrestigeLevelUpEvent(0, 5, 5));

            Assert.IsTrue(FindById(sys, "prestige_5").IsUnlocked);
        }

        // --- AchievementProgress ---

        [Test]
        public void AchievementProgress_MarkAndCheck()
        {
            AchievementProgress.Reset();
            Assert.IsFalse(AchievementProgress.IsUnlocked("test_ach"));
            AchievementProgress.MarkUnlocked("test_ach");
            Assert.IsTrue(AchievementProgress.IsUnlocked("test_ach"));
        }

        [Test]
        public void AchievementProgress_Reset_ClearsAll()
        {
            AchievementProgress.MarkUnlocked("something");
            AchievementProgress.Reset();
            Assert.IsFalse(AchievementProgress.IsUnlocked("something"));
        }

        // --- PlayerStats ---

        [Test]
        public void PlayerStats_CountsBuildingsBuilt()
        {
            var stats = new PlayerStats();
            var bus   = new EventBus();
            stats.Initialize(bus);

            bus.Publish(new BuildingCompletedEvent(1, 0));
            bus.Publish(new BuildingCompletedEvent(2, 0));

            Assert.AreEqual(2, stats.BuildingsBuilt);
        }

        [Test]
        public void PlayerStats_CountsSectorsConquered_OnlyPlayer0()
        {
            var stats = new PlayerStats();
            var bus   = new EventBus();
            stats.Initialize(bus);

            bus.Publish(new SectorConqueredEvent(1, 1, -1, ConquestMethod.Military));
            bus.Publish(new SectorConqueredEvent(2, 0, -1, ConquestMethod.Military));

            Assert.AreEqual(1, stats.SectorsConquered);
        }

        [Test]
        public void PlayerStats_Reset_ClearsCounters()
        {
            var stats = new PlayerStats();
            var bus   = new EventBus();
            stats.Initialize(bus);
            bus.Publish(new BuildingCompletedEvent(1, 0));

            stats.Reset();

            Assert.AreEqual(0, stats.BuildingsBuilt);
        }

        private static Achievement FindById(AchievementSystem sys, string id)
        {
            foreach (var a in sys.All)
                if (a.Id == id) return a;
            return null;
        }
    }
}
