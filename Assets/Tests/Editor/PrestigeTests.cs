using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class PrestigeTests
    {
        private EventBus _events;
        private PrestigeSystem _prestige;

        [SetUp]
        public void Setup()
        {
            _events = new EventBus();
            _prestige = new PrestigeSystem(pointsPerLevel: 5, _events);
        }

        // --- Points & Levels ---

        [Test]
        public void NewPlayer_HasZeroPointsAndLevel()
        {
            Assert.AreEqual(0, _prestige.GetPoints(0));
            Assert.AreEqual(0, _prestige.GetLevel(0));
        }

        [Test]
        public void AwardPoints_IncreasesTotal()
        {
            _prestige.AwardPoints(0, 3);
            Assert.AreEqual(3, _prestige.GetPoints(0));
        }

        [Test]
        public void FivePoints_GrantsLevelOne()
        {
            _prestige.AwardPoints(0, 5);
            Assert.AreEqual(1, _prestige.GetLevel(0));
        }

        [Test]
        public void TenPoints_GrantsLevelTwo()
        {
            _prestige.AwardPoints(0, 10);
            Assert.AreEqual(2, _prestige.GetLevel(0));
        }

        [Test]
        public void FourPoints_StaysLevelZero()
        {
            _prestige.AwardPoints(0, 4);
            Assert.AreEqual(0, _prestige.GetLevel(0));
        }

        [Test]
        public void IncrementalPoints_AccumulateToLevel()
        {
            _prestige.AwardPoints(0, 2);
            _prestige.AwardPoints(0, 3);
            Assert.AreEqual(5, _prestige.GetPoints(0));
            Assert.AreEqual(1, _prestige.GetLevel(0));
        }

        // --- Level Up Events ---

        [Test]
        public void LevelUp_FiresEvent()
        {
            int firedLevel = -1;
            _events.Subscribe<PrestigeLevelUpEvent>(e => firedLevel = e.NewLevel);
            _prestige.AwardPoints(0, 5);
            Assert.AreEqual(1, firedLevel);
        }

        [Test]
        public void MultiplelevelsAtOnce_FiresEventWithCorrectGained()
        {
            int gained = 0;
            _events.Subscribe<PrestigeLevelUpEvent>(e => gained = e.LevelsGained);
            _prestige.AwardPoints(0, 15); // 3 levels at once
            Assert.AreEqual(3, gained);
        }

        [Test]
        public void NoLevelUp_DoesNotFireEvent()
        {
            bool fired = false;
            _events.Subscribe<PrestigeLevelUpEvent>(e => fired = true);
            _prestige.AwardPoints(0, 2);
            Assert.IsFalse(fired);
        }

        // --- Unspent Levels ---

        [Test]
        public void UnspentLevels_EqualsLevelMinusUnlocks()
        {
            _prestige.AwardPoints(0, 10); // Level 2
            Assert.AreEqual(2, _prestige.GetUnspentLevels(0));
            _prestige.TryUnlock(0, "eco_residence_upgrade"); // Costs 1 level
            Assert.AreEqual(1, _prestige.GetUnspentLevels(0));
        }

        // --- Unlocks ---

        [Test]
        public void TryUnlock_SucceedsWithSufficientLevel()
        {
            _prestige.AwardPoints(0, 5); // Level 1
            bool result = _prestige.TryUnlock(0, "eco_residence_upgrade"); // MinLevel 1
            Assert.IsTrue(result);
            Assert.IsTrue(_prestige.HasUnlock(0, "eco_residence_upgrade"));
        }

        [Test]
        public void TryUnlock_FailsWithInsufficientLevel()
        {
            _prestige.AwardPoints(0, 5); // Level 1
            bool result = _prestige.TryUnlock(0, "eco_paved_roads"); // MinLevel 2
            Assert.IsFalse(result);
        }

        [Test]
        public void TryUnlock_FailsWithNoUnspentLevels()
        {
            _prestige.AwardPoints(0, 5); // Level 1, 1 unspent
            _prestige.TryUnlock(0, "eco_residence_upgrade"); // Spends level
            bool result = _prestige.TryUnlock(0, "eco_storehouse_lv2");
            Assert.IsFalse(result);
        }

        [Test]
        public void TryUnlock_FailsIfPrerequisiteNotMet()
        {
            _prestige.AwardPoints(0, 10); // Level 2
            // eco_paved_roads requires eco_storehouse_lv2
            bool result = _prestige.TryUnlock(0, "eco_paved_roads");
            Assert.IsFalse(result);
        }

        [Test]
        public void TryUnlock_SucceedsAfterPrerequisiteMet()
        {
            _prestige.AwardPoints(0, 10); // Level 2
            _prestige.TryUnlock(0, "eco_storehouse_lv2"); // MinLevel 1, no prereq
            bool result = _prestige.TryUnlock(0, "eco_paved_roads"); // MinLevel 2, prereq met
            Assert.IsTrue(result);
        }

        [Test]
        public void TryUnlock_FailsOnDuplicate()
        {
            _prestige.AwardPoints(0, 10); // Level 2
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            bool result = _prestige.TryUnlock(0, "eco_residence_upgrade");
            Assert.IsFalse(result);
        }

        [Test]
        public void TryUnlock_FiresEvent()
        {
            string firedId = null;
            _events.Subscribe<PrestigeUnlockEvent>(e => firedId = e.UnlockId);
            _prestige.AwardPoints(0, 5);
            _prestige.TryUnlock(0, "mil_stronghold");
            Assert.AreEqual("mil_stronghold", firedId);
        }

        [Test]
        public void TryUnlock_InvalidId_ReturnsFalse()
        {
            _prestige.AwardPoints(0, 5);
            Assert.IsFalse(_prestige.TryUnlock(0, "nonexistent_unlock"));
        }

        // --- Get Unlocks ---

        [Test]
        public void GetUnlocks_ReturnsAllUnlocked()
        {
            _prestige.AwardPoints(0, 10);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            _prestige.TryUnlock(0, "cul_church");
            var unlocks = _prestige.GetUnlocks(0);
            Assert.AreEqual(2, unlocks.Count);
        }

        [Test]
        public void GetUnlocks_EmptyForNewPlayer()
        {
            var unlocks = _prestige.GetUnlocks(0);
            Assert.AreEqual(0, unlocks.Count);
        }

        // --- Database ---

        [Test]
        public void Database_Has24Unlocks()
        {
            Assert.AreEqual(24, PrestigeDatabase.All.Count);
        }

        [Test]
        public void Database_EachBranchHas8Unlocks()
        {
            Assert.AreEqual(8, PrestigeDatabase.GetBranch(
                PrestigeDatabase.PrestigeBranch.Economy).Count);
            Assert.AreEqual(8, PrestigeDatabase.GetBranch(
                PrestigeDatabase.PrestigeBranch.Military).Count);
            Assert.AreEqual(8, PrestigeDatabase.GetBranch(
                PrestigeDatabase.PrestigeBranch.Culture).Count);
        }

        [Test]
        public void Database_GetById_ReturnsCorrect()
        {
            var def = PrestigeDatabase.Get("mil_cannon");
            Assert.IsNotNull(def);
            Assert.AreEqual("Cannon Foundry", def.DisplayName);
            Assert.AreEqual(PrestigeDatabase.PrestigeBranch.Military, def.Branch);
            Assert.AreEqual(4, def.MinLevel);
        }

        [Test]
        public void Database_GetById_InvalidReturnsNull()
        {
            Assert.IsNull(PrestigeDatabase.Get("invalid_id"));
        }

        // --- Multi-player ---

        [Test]
        public void DifferentPlayers_IndependentPrestige()
        {
            _prestige.AwardPoints(0, 10);
            _prestige.AwardPoints(1, 5);
            Assert.AreEqual(2, _prestige.GetLevel(0));
            Assert.AreEqual(1, _prestige.GetLevel(1));
        }

        [Test]
        public void DifferentPlayers_IndependentUnlocks()
        {
            _prestige.AwardPoints(0, 5);
            _prestige.AwardPoints(1, 5);
            _prestige.TryUnlock(0, "eco_residence_upgrade");
            Assert.IsTrue(_prestige.HasUnlock(0, "eco_residence_upgrade"));
            Assert.IsFalse(_prestige.HasUnlock(1, "eco_residence_upgrade"));
        }
    }
}
