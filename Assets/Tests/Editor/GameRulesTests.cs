using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for GameRules, StartingProfile, and VictoryRuleSet.</summary>
    [TestFixture]
    public class GameRulesTests
    {
        [Test]
        public void StartingProfile_Default_HasPlanksAndTools()
        {
            var profile = StartingProfile.Default;
            Assert.IsTrue(profile.Resources.ContainsKey(ResourceType.Planks));
            Assert.IsTrue(profile.Resources.ContainsKey(ResourceType.Tools));
            Assert.Greater(profile.Resources[ResourceType.Planks], 0);
        }

        [Test]
        public void StartingProfile_Rich_HasMorePlanksThanDefault()
        {
            Assert.Greater(
                StartingProfile.Rich.Resources[ResourceType.Planks],
                StartingProfile.Default.Resources[ResourceType.Planks]);
        }

        [Test]
        public void StartingProfile_Lean_HasFewerPlanksThanDefault()
        {
            Assert.Less(
                StartingProfile.Lean.Resources[ResourceType.Planks],
                StartingProfile.Default.Resources[ResourceType.Planks]);
        }

        [Test]
        public void StartingProfile_Get_ReturnsCorrectType()
        {
            Assert.AreEqual(StartingProfileType.Rich,
                StartingProfile.Get(StartingProfileType.Rich).Type);
            Assert.AreEqual(StartingProfileType.Lean,
                StartingProfile.Get(StartingProfileType.Lean).Type);
            Assert.AreEqual(StartingProfileType.Default,
                StartingProfile.Get(StartingProfileType.Default).Type);
        }

        [Test]
        public void VictoryRuleSet_Standard_AllPathsEnabled()
        {
            var rules = VictoryRuleSet.Standard;
            Assert.IsTrue(rules.MilitaryVPsEnabled);
            Assert.IsTrue(rules.TechVPsEnabled);
            Assert.IsTrue(rules.TradeVPsEnabled);
            Assert.IsTrue(rules.PrestigeVPsEnabled);
        }

        [Test]
        public void VictoryRuleSet_ConquestOnly_OnlyMilitaryEnabled()
        {
            var rules = VictoryRuleSet.ConquestOnly;
            Assert.IsTrue(rules.MilitaryVPsEnabled);
            Assert.IsFalse(rules.TechVPsEnabled);
            Assert.IsFalse(rules.TradeVPsEnabled);
        }

        [Test]
        public void VictoryRuleSet_NoConquest_MilitaryDisabled()
        {
            var rules = VictoryRuleSet.NoConquest;
            Assert.IsFalse(rules.MilitaryVPsEnabled);
            Assert.IsTrue(rules.TechVPsEnabled);
            Assert.IsTrue(rules.TradeVPsEnabled);
        }

        [Test]
        public void GameRules_Default_UsesStandardVictoryAndDefaultResources()
        {
            var rules = GameRules.Default;
            Assert.AreEqual(StartingProfileType.Default, rules.StartingResources.Type);
            Assert.AreEqual(VictoryRuleSetType.Standard, rules.VictoryPaths.Type);
            Assert.IsFalse(rules.BriberyDisabled);
            Assert.IsFalse(rules.ProselytismDisabled);
        }

        [Test]
        public void GameRules_ConquestMatch_DisablesBriberyAndProselytism()
        {
            var rules = GameRules.ConquestMatch;
            Assert.IsTrue(rules.BriberyDisabled);
            Assert.IsTrue(rules.ProselytismDisabled);
            Assert.AreEqual(VictoryRuleSetType.ConquestOnly, rules.VictoryPaths.Type);
        }

        [Test]
        public void GameState_AppliesStartingProfileToAllPlayers()
        {
            Building.ResetIdCounter();
            Storehouse.ResetIdCounter();
            ArmySystem.ResetIdCounter();

            var graph = TestMapFactory.CreateSixSectorMap();
            var rules = new GameRules(StartingProfile.Rich);
            var state = new GameState(graph, playerCount: 2,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test", rules: rules);

            // Rich profile gives more planks than default (40 vs 20)
            int player0Planks = state.PlayerResources[0].Get(ResourceType.Planks);
            int player1Planks = state.PlayerResources[1].Get(ResourceType.Planks);
            Assert.AreEqual(StartingProfile.Rich.Resources[ResourceType.Planks], player0Planks);
            Assert.AreEqual(StartingProfile.Rich.Resources[ResourceType.Planks], player1Planks);
        }

        [Test]
        public void GameState_ExposesRules()
        {
            Building.ResetIdCounter();
            Storehouse.ResetIdCounter();
            ArmySystem.ResetIdCounter();

            var graph = TestMapFactory.CreateSixSectorMap();
            var rules = GameRules.QuickPlay;
            var state = new GameState(graph, playerCount: 1,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test", rules: rules);

            Assert.AreEqual(StartingProfileType.Rich, state.Rules.StartingResources.Type);
        }
    }
}
