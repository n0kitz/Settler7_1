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
            var validMaps = new HashSet<string>(MapFactory.GetMapIds()) { "tutorial" };
            foreach (var m in CampaignSystem.AllMissions)
                Assert.IsTrue(validMaps.Contains(m.MapId),
                    $"Mission '{m.Id}' references unknown map '{m.MapId}'");
        }

        [Test]
        public void MissionChain_VisitsEveryMissionExactlyOnce()
        {
            // The arc must be a single unlock chain from the first mission to a finale.
            var visited = new HashSet<string>();
            var current = CampaignSystem.AllMissions[0];
            while (current != null)
            {
                Assert.IsTrue(visited.Add(current.Id),
                    $"Mission '{current.Id}' appears twice in the unlock chain");
                current = current.UnlocksNext != null
                    ? CampaignSystem.Find(current.UnlocksNext) : null;
            }
            Assert.AreEqual(CampaignSystem.AllMissions.Count, visited.Count,
                "Unlock chain does not reach every mission in the catalogue");
        }

        [Test]
        public void HearthAndHome_ObjectivesEvaluate_AndCompleteFiresOnce()
        {
            var info = MapFactory.CreateMap("highland_duel");
            var state = new GameState(info.Graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 5, mapId: "highland_duel");
            var campaign = new CampaignSystem(state);
            var mission = CampaignSystem.Find("c1_m2_hearth_and_home");
            campaign.SetActiveMission(mission);
            campaign.ApplyStartingResources();

            Assert.AreEqual(30, state.PlayerResources[0].Get(ResourceType.Planks),
                "Mission starting resources must be applied to player 0");

            int fired = 0;
            campaign.OnObjectivesComplete += _ => fired++;

            campaign.Tick();
            Assert.AreEqual(0, fired, "Objectives must not complete on a fresh state");

            // Satisfy all three objectives: 2 operational farms, 15 bread, 4 sectors
            state.Construction.RestoreBuilding(BaseBuildingType.Farm, 0, 0, 3, 0f, 0f,
                BuildingState.Complete, 1f, 0, FoodSetting.None);
            state.Construction.RestoreBuilding(BaseBuildingType.Farm, 1, 0, 3, 0f, 0f,
                BuildingState.Complete, 1f, 0, FoodSetting.None);
            state.PlayerResources[0].Set(ResourceType.Bread, 15);
            state.Graph.GetSector(4).SetOwner(0);
            state.Graph.GetSector(9).SetOwner(0);

            campaign.Tick();
            Assert.AreEqual(1, fired, "All objectives met — completion must fire");
            campaign.Tick();
            campaign.Tick();
            Assert.AreEqual(1, fired, "Completion must fire exactly once, not per tick");
        }

        [Test]
        public void SetActiveMission_ResetsSharedObjectives_ForReplay()
        {
            var mission = CampaignSystem.Find("c1_m1_first_steps");
            mission.Objectives[0].GetType(); // catalogue is static/shared

            var info = MapFactory.CreateMap("test_valley");
            var state = new GameState(info.Graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 2, mapId: "test_valley");
            var campaign = new CampaignSystem(state);
            campaign.SetActiveMission(mission);

            state.Victory.AwardPermanentVP(0, "test_vp_a");
            state.Victory.AwardPermanentVP(0, "test_vp_b");
            for (int i = 0; i < info.Graph.SectorCount && i < 3; i++)
                info.Graph.GetSector(i).SetOwner(0);
            campaign.Tick();
            Assert.IsTrue(mission.Objectives[0].IsComplete);

            // Replaying the mission must start with fresh objectives
            campaign.SetActiveMission(mission);
            Assert.IsFalse(mission.Objectives[0].IsComplete,
                "Static catalogue objectives must reset on SetActiveMission");
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
