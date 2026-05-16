using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for ModManifest, ModLoader, CustomMapRegistry, ScenarioDefinition.</summary>
    [TestFixture]
    public class ModdingTests
    {
        private string _testModsRoot;

        [SetUp]
        public void SetUp()
        {
            _testModsRoot = Path.Combine(Path.GetTempPath(), "Settlers7TestMods_" + Guid.NewGuid());
            Directory.CreateDirectory(_testModsRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testModsRoot))
                Directory.Delete(_testModsRoot, recursive: true);
        }

        private string CreateTestMod(string modId, bool enabled = true,
            string name = null, string version = "1.0", string author = "Tester")
        {
            string modDir = Path.Combine(_testModsRoot, modId);
            Directory.CreateDirectory(modDir);
            File.WriteAllLines(Path.Combine(modDir, "manifest.ini"), new[]
            {
                $"ModId={modId}",
                $"Name={name ?? modId}",
                $"Author={author}",
                $"Version={version}",
                $"Description=Test mod {modId}",
                $"Enabled={enabled}",
            });
            return modDir;
        }

        // --- ModManifest ---

        [Test]
        public void ModManifest_Parse_ExtractsFields()
        {
            var lines = new[]
            {
                "ModId=cool_mod",
                "Name=Cool Mod",
                "Author=Alice",
                "Version=2.3",
                "Description=A cool mod",
                "Enabled=true",
            };
            var m = ModManifest.Parse("/fake/path", lines);
            Assert.AreEqual("cool_mod", m.ModId);
            Assert.AreEqual("Cool Mod", m.Name);
            Assert.AreEqual("Alice",    m.Author);
            Assert.AreEqual("2.3",      m.Version);
            Assert.IsTrue(m.Enabled);
        }

        [Test]
        public void ModManifest_Parse_DisabledMod()
        {
            var m = ModManifest.Parse("/path", new[] { "Enabled=false" });
            Assert.IsFalse(m.Enabled);
        }

        [Test]
        public void ModManifest_Parse_FallsBackToDirectoryName()
        {
            var m = ModManifest.Parse("/path/my_mod", new string[0]);
            Assert.AreEqual("my_mod", m.ModId);
        }

        // --- ModLoader (via test mods directory; we can't override GetModsRoot directly,
        //     so we test the parsing helpers instead) ---

        [Test]
        public void ModManifest_Parse_EmptyLines_DefaultsApplied()
        {
            var m = ModManifest.Parse("/x/some_mod", new string[0]);
            Assert.AreEqual("some_mod", m.ModId);
            Assert.AreEqual("1.0",      m.Version);
            Assert.IsTrue(m.Enabled);
        }

        // --- ScenarioDefinition ---

        [Test]
        public void ScenarioDefinition_ToGameRules_DefaultValues()
        {
            var sc = new ScenarioDefinition
            {
                StartingProfile = "Default",
                VictoryRules    = "Standard",
            };
            var rules = sc.ToGameRules();
            Assert.IsNotNull(rules);
        }

        [Test]
        public void ScenarioDefinition_ToGameRules_RichStart()
        {
            var sc = new ScenarioDefinition { StartingProfile = "Rich" };
            var rules = sc.ToGameRules();
            Assert.IsNotNull(rules);
            Assert.Greater(rules.StartingProfile.StartingPlanks,
                StartingProfile.Get(StartingProfileType.Default).StartingPlanks);
        }

        [Test]
        public void ScenarioDefinition_ToAIProfile_WarriorHard()
        {
            var sc = new ScenarioDefinition
            {
                AIPersonality = "Warrior",
                AIDifficulty  = "Hard",
            };
            var profile = sc.ToAIProfile();
            Assert.AreEqual(AIPersonalityType.Warrior, profile.PersonalityType);
            Assert.AreEqual(AIDifficultyLevel.Hard,    profile.DifficultyLevel);
        }

        [Test]
        public void ScenarioDefinition_Parse_UnknownEnum_FallsToDefault()
        {
            var sc = new ScenarioDefinition { AIPersonality = "NotARealPersonality" };
            var profile = sc.ToAIProfile();
            Assert.IsNotNull(profile, "Should not throw on unknown personality");
        }

        // --- CustomMapRegistry.LoadMap (without real mod dir) ---

        [Test]
        public void CustomMapRegistry_LoadMap_NotFound_ReturnsError()
        {
            var result = CustomMapRegistry.LoadMap("nonexistent_map", out string error);
            Assert.IsNull(result);
            Assert.IsNotNull(error);
            StringAssert.Contains("not found", error);
        }

        // --- CustomAchievementRegistry ---

        [Test]
        public void CustomAchievementRegistry_Reload_EmptyWhenNoMods()
        {
            // With no mods loaded, registry should be empty
            CustomAchievementRegistry.Reload();
            Assert.AreEqual(0, CustomAchievementRegistry.Achievements.Count);
        }
    }
}
