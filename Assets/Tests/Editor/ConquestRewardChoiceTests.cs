using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for the conquest reward choice (§14.3, Critical Rule #10).</summary>
    [TestFixture]
    public class ConquestRewardChoiceTests
    {
        private EventBus _events;
        private Dictionary<int, PlayerResources> _resources;
        private ConquestRewardSystem _rewards;

        [SetUp]
        public void SetUp()
        {
            _events = new EventBus();
            _resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events),
                [1] = new PlayerResources(1, _events)
            };
            _rewards = new ConquestRewardSystem(_resources, _events);
        }

        private void Conquer(int sectorId, int newOwner, int prevOwner)
        {
            _events.Publish(new SectorConqueredEvent(
                sectorId, newOwner, prevOwner, ConquestMethod.Military));
        }

        [Test]
        public void NeutralConquest_CreatesPendingReward_WithFourPackages()
        {
            Conquer(3, 0, Sector.NEUTRAL);

            var pending = _rewards.GetPendingFor(0);
            Assert.IsNotNull(pending, "Neutral conquest must offer a reward choice");
            Assert.AreEqual(4, pending.Packages.Length, "§14.3: exactly 4 packages");
            Assert.AreEqual("ui.reward.population", pending.Packages[0].TitleKey,
                "First package is the population reward");
        }

        [Test]
        public void EnemyConquest_GrantsNoRewardChoice()
        {
            Conquer(3, 0, prevOwner: 1);
            Assert.IsNull(_rewards.GetPendingFor(0),
                "Only NEUTRAL conquests grant reward packages");
        }

        [Test]
        public void ChooseReward_AppliesGoods_ExactlyOnce()
        {
            Conquer(3, 0, Sector.NEUTRAL);
            int planksBefore = _resources[0].Get(ResourceType.Planks);

            bool first = _rewards.ChooseReward(0, 3, 1); // Planks + Stone package
            bool second = _rewards.ChooseReward(0, 3, 1);

            Assert.IsTrue(first);
            Assert.IsFalse(second, "Reward must be claimable only once");
            Assert.AreEqual(planksBefore + 10, _resources[0].Get(ResourceType.Planks));
            Assert.AreEqual(5, _resources[0].Get(ResourceType.Stone));
            Assert.IsNull(_rewards.GetPendingFor(0));
        }

        [Test]
        public void HumanReward_IsNeverAutoGranted()
        {
            Conquer(3, 0, Sector.NEUTRAL);

            // Critical Rule #10: still pending until the player chooses
            Assert.IsNotNull(_rewards.GetPendingFor(0));
            Assert.AreEqual(0, _resources[0].Get(ResourceType.Planks),
                "No package contents may be granted before the choice");
        }

        [Test]
        public void AIConquest_AutoChoosesImmediately()
        {
            Conquer(5, 1, Sector.NEUTRAL);

            Assert.IsNull(_rewards.GetPendingFor(1),
                "AI must resolve its reward immediately");
            Assert.Greater(_resources[1].Get(ResourceType.Planks), 0,
                "AI auto-pick applies the package goods");
        }

        [Test]
        public void InvalidPackageIndex_Rejected()
        {
            Conquer(3, 0, Sector.NEUTRAL);
            Assert.IsFalse(_rewards.ChooseReward(0, 3, 99));
            Assert.IsFalse(_rewards.ChooseReward(0, 3, -1));
            Assert.IsNotNull(_rewards.GetPendingFor(0), "Pending must survive bad input");
        }

        [Test]
        public void ChosenEvent_FiresWithPackageIndex()
        {
            int firedIndex = -1;
            _events.Subscribe<ConquestRewardChosenEvent>(e => firedIndex = e.PackageIndex);

            Conquer(3, 0, Sector.NEUTRAL);
            _rewards.ChooseReward(0, 3, 2);

            Assert.AreEqual(2, firedIndex);
        }

        [Test]
        public void RestorePending_RecreatesChoice_WithoutDuplicates()
        {
            _rewards.RestorePending(0, 7);
            _rewards.RestorePending(0, 7);

            Assert.IsNotNull(_rewards.GetPendingFor(0));
            Assert.AreEqual(1, _rewards.Pending.Count);
        }
    }
}
