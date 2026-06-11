using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for BuildingPrestigeGate — build menu / placement gating.</summary>
    [TestFixture]
    public class BuildingGateTests
    {
        [Test]
        public void BaseBuildings_RequireNoUnlock()
        {
            Assert.IsNull(BuildingPrestigeGate.RequiredUnlock(BaseBuildingType.Lodge));
            Assert.IsNull(BuildingPrestigeGate.RequiredUnlock(BaseBuildingType.Farm));
            Assert.IsNull(BuildingPrestigeGate.RequiredUnlock(BaseBuildingType.MountainShelter));
            Assert.IsNull(BuildingPrestigeGate.RequiredUnlock(BaseBuildingType.Residence));
        }

        [Test]
        public void NobleResidence_RequiresPrestigeUnlock()
        {
            Assert.AreEqual("eco_noble_residence",
                BuildingPrestigeGate.RequiredUnlock(BaseBuildingType.NobleResidence));
        }

        [Test]
        public void RequiredUnlocks_ExistInPrestigeDatabase()
        {
            foreach (BaseBuildingType type in System.Enum.GetValues(typeof(BaseBuildingType)))
            {
                string required = BuildingPrestigeGate.RequiredUnlock(type);
                if (required == null) continue;
                Assert.IsNotNull(PrestigeDatabase.Get(required),
                    $"Gate for {type} references unknown prestige id '{required}'");
            }
        }

        [Test]
        public void IsUnlocked_FollowsPrestigeState()
        {
            var events = new EventBus();
            var prestige = new PrestigeSystem(5, events);

            Assert.IsTrue(BuildingPrestigeGate.IsUnlocked(prestige, 0, BaseBuildingType.Lodge),
                "Ungated building must always be unlocked");
            Assert.IsFalse(
                BuildingPrestigeGate.IsUnlocked(prestige, 0, BaseBuildingType.NobleResidence),
                "NobleResidence must be locked without prestige unlock");

            // eco_noble_residence needs level 2 + prereq eco_residence_upgrade
            prestige.AwardPoints(0, 15);
            Assert.IsTrue(prestige.TryUnlock(0, "eco_residence_upgrade"));
            Assert.IsTrue(prestige.TryUnlock(0, "eco_noble_residence"));

            Assert.IsTrue(
                BuildingPrestigeGate.IsUnlocked(prestige, 0, BaseBuildingType.NobleResidence));
        }
    }
}
