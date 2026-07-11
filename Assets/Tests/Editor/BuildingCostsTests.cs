using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class BuildingCostsTests
    {
        // Locks in the cost table documented in Enums.cs (BaseBuildingType comments)
        // and relied on by BuildMenu, AIEconomy, and BuildingPlacer — regressions here
        // silently unbalance every building's affordability.

        [Test]
        public void Get_Lodge_CostsThreePlanksNoStone()
        {
            BuildingCosts.Get(BaseBuildingType.Lodge, out int planks, out int stone);
            Assert.AreEqual(3, planks);
            Assert.AreEqual(0, stone);
        }

        [Test]
        public void Get_Farm_CostsThreePlanksNoStone()
        {
            BuildingCosts.Get(BaseBuildingType.Farm, out int planks, out int stone);
            Assert.AreEqual(3, planks);
            Assert.AreEqual(0, stone);
        }

        [Test]
        public void Get_MountainShelter_CostsTwoPlanksOneStone()
        {
            BuildingCosts.Get(BaseBuildingType.MountainShelter, out int planks, out int stone);
            Assert.AreEqual(2, planks);
            Assert.AreEqual(1, stone);
        }

        [Test]
        public void Get_Residence_CostsTwoPlanksOneStone()
        {
            BuildingCosts.Get(BaseBuildingType.Residence, out int planks, out int stone);
            Assert.AreEqual(2, planks);
            Assert.AreEqual(1, stone);
        }

        [Test]
        public void Get_NobleResidence_CostsThreePlanksTwoStone()
        {
            BuildingCosts.Get(BaseBuildingType.NobleResidence, out int planks, out int stone);
            Assert.AreEqual(3, planks);
            Assert.AreEqual(2, stone);
        }

        [Test]
        public void Get_NobleResidence_IsMostExpensiveTier()
        {
            BuildingCosts.Get(BaseBuildingType.Lodge, out int lodgePlanks, out int lodgeStone);
            BuildingCosts.Get(BaseBuildingType.NobleResidence, out int noblePlanks, out int nobleStone);

            Assert.GreaterOrEqual(noblePlanks + nobleStone, lodgePlanks + lodgeStone,
                "NobleResidence sits above Lodge in the building tier progression");
        }
    }
}
