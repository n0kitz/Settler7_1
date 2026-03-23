using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class FoodBoostTests
    {
        [Test]
        public void Lodge_NoFood_Returns1()
        {
            Assert.AreEqual(1, FoodBoostCalculator.GetMultiplier(BaseBuildingType.Lodge, FoodSetting.None));
        }

        [Test]
        public void Lodge_PlainFood_Returns2()
        {
            Assert.AreEqual(2, FoodBoostCalculator.GetMultiplier(BaseBuildingType.Lodge, FoodSetting.Plain));
        }

        [Test]
        public void Lodge_FancyFood_Returns3()
        {
            Assert.AreEqual(3, FoodBoostCalculator.GetMultiplier(BaseBuildingType.Lodge, FoodSetting.Fancy));
        }

        [Test]
        public void Farm_PlainFood_Returns2()
        {
            Assert.AreEqual(2, FoodBoostCalculator.GetMultiplier(BaseBuildingType.Farm, FoodSetting.Plain));
        }

        [Test]
        public void MountainShelter_FancyFood_Returns3()
        {
            Assert.AreEqual(3, FoodBoostCalculator.GetMultiplier(BaseBuildingType.MountainShelter, FoodSetting.Fancy));
        }

        [Test]
        public void Residence_PlainFood_Returns2()
        {
            Assert.AreEqual(2, FoodBoostCalculator.GetMultiplier(BaseBuildingType.Residence, FoodSetting.Plain));
        }

        [Test]
        public void NobleResidence_NoFood_Returns0_IDLE()
        {
            Assert.AreEqual(0, FoodBoostCalculator.GetMultiplier(BaseBuildingType.NobleResidence, FoodSetting.None));
        }

        [Test]
        public void NobleResidence_PlainFood_Returns1()
        {
            Assert.AreEqual(1, FoodBoostCalculator.GetMultiplier(BaseBuildingType.NobleResidence, FoodSetting.Plain));
        }

        [Test]
        public void NobleResidence_FancyFood_Returns2()
        {
            Assert.AreEqual(2, FoodBoostCalculator.GetMultiplier(BaseBuildingType.NobleResidence, FoodSetting.Fancy));
        }

        // --- ShouldHalt tests ---

        [Test]
        public void NobleResidence_NoFoodSetting_ShouldHalt()
        {
            Assert.IsTrue(FoodBoostCalculator.ShouldHalt(
                BaseBuildingType.NobleResidence, FoodSetting.None, hasFoodAvailable: true));
        }

        [Test]
        public void Lodge_NoFoodSetting_ShouldNotHalt()
        {
            Assert.IsFalse(FoodBoostCalculator.ShouldHalt(
                BaseBuildingType.Lodge, FoodSetting.None, hasFoodAvailable: true));
        }

        [Test]
        public void Lodge_PlainFoodToggled_NoFoodAvailable_ShouldHalt()
        {
            Assert.IsTrue(FoodBoostCalculator.ShouldHalt(
                BaseBuildingType.Lodge, FoodSetting.Plain, hasFoodAvailable: false));
        }

        [Test]
        public void Lodge_PlainFoodToggled_FoodAvailable_ShouldNotHalt()
        {
            Assert.IsFalse(FoodBoostCalculator.ShouldHalt(
                BaseBuildingType.Lodge, FoodSetting.Plain, hasFoodAvailable: true));
        }

        // --- GetEffectiveMultiplier tests ---

        [Test]
        public void EffectiveMultiplier_FoodToggledButUnavailable_Returns0()
        {
            Assert.AreEqual(0, FoodBoostCalculator.GetEffectiveMultiplier(
                BaseBuildingType.Lodge, FoodSetting.Plain, hasFoodAvailable: false));
        }

        [Test]
        public void EffectiveMultiplier_FoodToggledAndAvailable_ReturnsNormal()
        {
            Assert.AreEqual(2, FoodBoostCalculator.GetEffectiveMultiplier(
                BaseBuildingType.Lodge, FoodSetting.Plain, hasFoodAvailable: true));
        }

        [Test]
        public void EffectiveMultiplier_NobleRes_PlainAvailable_Returns1()
        {
            Assert.AreEqual(1, FoodBoostCalculator.GetEffectiveMultiplier(
                BaseBuildingType.NobleResidence, FoodSetting.Plain, hasFoodAvailable: true));
        }

        // --- eco_food_master prestige unlock tests ---

        [Test]
        public void FoodMaster_Lodge_Fancy_Returns4()
        {
            Assert.AreEqual(4, FoodBoostCalculator.GetMultiplier(
                BaseBuildingType.Lodge, FoodSetting.Fancy, hasFoodMaster: true));
        }

        [Test]
        public void FoodMaster_Lodge_Plain_StillReturns2()
        {
            Assert.AreEqual(2, FoodBoostCalculator.GetMultiplier(
                BaseBuildingType.Lodge, FoodSetting.Plain, hasFoodMaster: true));
        }

        [Test]
        public void FoodMaster_NobleRes_Fancy_Returns3()
        {
            Assert.AreEqual(3, FoodBoostCalculator.GetMultiplier(
                BaseBuildingType.NobleResidence, FoodSetting.Fancy, hasFoodMaster: true));
        }

        [Test]
        public void FoodMaster_NobleRes_Plain_StillReturns1()
        {
            Assert.AreEqual(1, FoodBoostCalculator.GetMultiplier(
                BaseBuildingType.NobleResidence, FoodSetting.Plain, hasFoodMaster: true));
        }

        [Test]
        public void FoodMaster_Effective_FancyAvailable_Returns4()
        {
            Assert.AreEqual(4, FoodBoostCalculator.GetEffectiveMultiplier(
                BaseBuildingType.Residence, FoodSetting.Fancy,
                hasFoodAvailable: true, hasFoodMaster: true));
        }

        [Test]
        public void FoodMaster_Effective_FancyUnavailable_Returns0()
        {
            Assert.AreEqual(0, FoodBoostCalculator.GetEffectiveMultiplier(
                BaseBuildingType.Residence, FoodSetting.Fancy,
                hasFoodAvailable: false, hasFoodMaster: true));
        }
    }
}
