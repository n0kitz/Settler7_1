using NUnit.Framework;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for campaign mission catalogue, progress persistence, and objective evaluation.</summary>
    [TestFixture]
    public class CampaignTests
    {
        [Test]
        public void AllMissions_HasAtLeastSixMissions()
        {
            Assert.GreaterOrEqual(CampaignSystem.AllMissions.Count, 6);
        }

        [Test]
        public void AllMissions_FirstMissionHasNoPrerequisite()
        {
            // The first mission must always be unlockable from a fresh save.
            var fresh = new CampaignProgress();
            var unlocked = CampaignSystem.GetUnlocked(fresh);
            Assert.IsTrue(unlocked.Contains(CampaignSystem.AllMissions[0]));
        }

        [Test]
        public void GetUnlocked_UnlocksNextAfterCompletion()
        {
            var first = CampaignSystem.AllMissions[0];
            Assert.IsNotNull(first.UnlocksNext, "First mission should unlock a next mission");

            var progress = new CampaignProgress();
            progress.MarkComplete(first.Id);

            var unlocked = CampaignSystem.GetUnlocked(progress);
            var nextMission = CampaignSystem.Find(first.UnlocksNext);
            Assert.IsNotNull(nextMission);
            Assert.IsTrue(unlocked.Contains(nextMission));
        }

        [Test]
        public void CampaignProgress_MarkComplete_Persists()
        {
            var progress = new CampaignProgress();
            progress.MarkComplete("c1_m1_first_steps");
            Assert.IsTrue(progress.IsCompleted("c1_m1_first_steps"));
        }

        [Test]
        public void CampaignProgress_Reset_ClearsAll()
        {
            var progress = new CampaignProgress();
            progress.MarkComplete("c1_m1_first_steps");
            progress.Reset();
            Assert.IsFalse(progress.IsCompleted("c1_m1_first_steps"));
        }

        [Test]
        public void MissionObjective_InitiallyNotComplete()
        {
            var obj = new MissionObjective("Test", MissionObjectiveType.ReachVPCount, 5);
            Assert.IsFalse(obj.IsComplete);
        }

        [Test]
        public void AllMissions_EachHasValidMapId()
        {
            var validMaps = new HashSet<string>
                { "tutorial", "test_valley", "twin_rivers", "mountain_pass",
                  "island_chain", "large_valley", "crown_war", "empire" };
            foreach (var m in CampaignSystem.AllMissions)
                Assert.IsTrue(validMaps.Contains(m.MapId),
                    $"Mission '{m.Id}' references unknown map '{m.MapId}'");
        }

        [Test]
        public void Find_ReturnsCorrectMission()
        {
            var found = CampaignSystem.Find("c1_m1_first_steps");
            Assert.IsNotNull(found);
            Assert.AreEqual("c1_m1_first_steps", found.Id);
        }

        [Test]
        public void Find_UnknownId_ReturnsNull()
        {
            Assert.IsNull(CampaignSystem.Find("does_not_exist"));
        }
    }
}
