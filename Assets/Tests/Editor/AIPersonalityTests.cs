using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for AI personality, difficulty, and behavior profile.</summary>
    [TestFixture]
    public class AIPersonalityTests
    {
        [Test]
        public void AIPersonality_Get_Builder_ReturnsBuilderInstance()
        {
            var p = AIPersonality.Get(AIPersonalityType.Builder);
            Assert.AreEqual(AIPersonalityType.Builder, p.Type);
        }

        [Test]
        public void AIPersonality_Get_Warrior_HasHighMilitaryWeight()
        {
            var warrior = AIPersonality.Get(AIPersonalityType.Warrior);
            Assert.Greater(warrior.MilitaryWeight, warrior.TechWeight);
            Assert.Greater(warrior.MilitaryWeight, warrior.TradeWeight);
        }

        [Test]
        public void AIPersonality_Get_Merchant_HasHighTradeWeight()
        {
            var merchant = AIPersonality.Get(AIPersonalityType.Merchant);
            Assert.Greater(merchant.TradeWeight, merchant.MilitaryWeight);
        }

        [Test]
        public void AIPersonality_Builder_HasHighestEarlyEconomyThreshold()
        {
            Assert.Greater(
                AIPersonality.Builder.EarlyEconomyThreshold,
                AIPersonality.Warrior.EarlyEconomyThreshold);
        }

        [Test]
        public void AIDifficulty_Hard_HasShorterDecisionInterval()
        {
            Assert.Less(AIDifficulty.Hard.DecisionInterval, AIDifficulty.Easy.DecisionInterval);
        }

        [Test]
        public void AIDifficulty_Hard_HasLowerAttackThreshold()
        {
            Assert.Less(AIDifficulty.Hard.AttackThreshold, AIDifficulty.Easy.AttackThreshold);
        }

        [Test]
        public void AIDifficulty_Hard_HasBonusStartingResources()
        {
            Assert.Greater(AIDifficulty.Hard.StartingBonusCoins, 0);
            Assert.Greater(AIDifficulty.Hard.StartingBonusPlanks, 0);
        }

        [Test]
        public void AIDifficulty_Easy_HasNoStartingBonus()
        {
            Assert.AreEqual(0, AIDifficulty.Easy.StartingBonusCoins);
            Assert.AreEqual(0, AIDifficulty.Easy.StartingBonusPlanks);
        }

        [Test]
        public void AIDifficulty_Get_ReturnsCorrectLevel()
        {
            Assert.AreEqual(AIDifficultyLevel.Easy,
                AIDifficulty.Get(AIDifficultyLevel.Easy).Level);
            Assert.AreEqual(AIDifficultyLevel.Hard,
                AIDifficulty.Get(AIDifficultyLevel.Hard).Level);
            Assert.AreEqual(AIDifficultyLevel.Normal,
                AIDifficulty.Get(AIDifficultyLevel.Normal).Level);
        }

        [Test]
        public void AIBehaviorProfile_Create_UsesCorrectPersonalityAndDifficulty()
        {
            var profile = AIBehaviorProfile.Create(
                AIPersonalityType.Warrior, AIDifficultyLevel.Hard);

            Assert.AreEqual(AIPersonalityType.Warrior, profile.Personality.Type);
            Assert.AreEqual(AIDifficultyLevel.Hard, profile.Difficulty.Level);
        }

        [Test]
        public void AIBehaviorProfile_Default_IsNormalBuilder()
        {
            var def = AIBehaviorProfile.Default;
            Assert.AreEqual(AIPersonalityType.Builder, def.Personality.Type);
            Assert.AreEqual(AIDifficultyLevel.Normal, def.Difficulty.Level);
        }

        [Test]
        public void AIBehaviorProfile_DisplayName_IsSet()
        {
            var profile = new AIBehaviorProfile(
                AIPersonality.Warrior, AIDifficulty.Hard, "Lord Viktor");
            Assert.AreEqual("Lord Viktor", profile.DisplayName);
        }

        [Test]
        public void AIController_UsesProfileDecisionInterval()
        {
            var easyProfile = AIBehaviorProfile.Create(
                AIPersonalityType.Builder, AIDifficultyLevel.Easy);
            var hardProfile = AIBehaviorProfile.Create(
                AIPersonalityType.Builder, AIDifficultyLevel.Hard);

            var graph = TestMapFactory.CreateSixSectorMap();
            var state = new GameState(graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test");

            var easyAI = new AIController(state, 1, easyProfile);
            var hardAI = new AIController(state, 1, hardProfile);

            Assert.AreEqual(AIPersonalityType.Builder, easyAI.Profile.Personality.Type);
            Assert.Less(hardAI.Profile.Difficulty.DecisionInterval,
                        easyAI.Profile.Difficulty.DecisionInterval);
        }
    }
}
