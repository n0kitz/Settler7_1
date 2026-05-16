using NUnit.Framework;
using System.IO;
using System;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for SettingsState and SettingsPersistence.</summary>
    [TestFixture]
    public class SettingsTests
    {
        // --- SettingsState ---

        [Test]
        public void Default_HasExpectedValues()
        {
            var s = SettingsState.Default;
            Assert.AreEqual(0.3f, s.MusicVolume, 0.001f);
            Assert.AreEqual(0.6f, s.SfxVolume,   0.001f);
            Assert.IsFalse(s.MasterMute);
            Assert.AreEqual(2, s.GraphicsQuality);
            Assert.IsFalse(s.Fullscreen);
        }

        [Test]
        public void Clone_IsDeepCopy()
        {
            var orig = SettingsState.Default;
            orig.MusicVolume = 0.8f;
            var clone = orig.Clone();
            clone.MusicVolume = 0.1f;

            Assert.AreEqual(0.8f, orig.MusicVolume, 0.001f,
                "Mutating clone must not change original");
        }

        [Test]
        public void Clone_CopiesAllFields()
        {
            var orig = new SettingsState
            {
                MusicVolume     = 0.5f,
                SfxVolume       = 0.4f,
                MasterMute      = true,
                GraphicsQuality = 3,
                Fullscreen      = true,
            };
            var clone = orig.Clone();

            Assert.AreEqual(orig.MusicVolume,     clone.MusicVolume,     0.001f);
            Assert.AreEqual(orig.SfxVolume,       clone.SfxVolume,       0.001f);
            Assert.AreEqual(orig.MasterMute,      clone.MasterMute);
            Assert.AreEqual(orig.GraphicsQuality, clone.GraphicsQuality);
            Assert.AreEqual(orig.Fullscreen,      clone.Fullscreen);
        }

        // --- SettingsPersistence ---

        private static string GetTestSettingsPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Settlers7", "settings.ini");
        }

        private void DeleteSettingsFile()
        {
            string path = GetTestSettingsPath();
            if (File.Exists(path)) File.Delete(path);
        }

        [Test]
        public void Load_WhenNoFile_ReturnsDefaults()
        {
            DeleteSettingsFile();
            var loaded = SettingsPersistence.Load();
            Assert.AreEqual(SettingsState.Default.MusicVolume, loaded.MusicVolume, 0.001f);
        }

        [Test]
        public void SaveAndLoad_RoundTrips_MusicVolume()
        {
            var state = SettingsState.Default;
            state.MusicVolume = 0.75f;
            SettingsPersistence.Save(state);

            var loaded = SettingsPersistence.Load();
            Assert.AreEqual(0.75f, loaded.MusicVolume, 0.001f);
        }

        [Test]
        public void SaveAndLoad_RoundTrips_SfxVolume()
        {
            var state = SettingsState.Default;
            state.SfxVolume = 0.2f;
            SettingsPersistence.Save(state);

            var loaded = SettingsPersistence.Load();
            Assert.AreEqual(0.2f, loaded.SfxVolume, 0.001f);
        }

        [Test]
        public void SaveAndLoad_RoundTrips_MasterMute()
        {
            var state = SettingsState.Default;
            state.MasterMute = true;
            SettingsPersistence.Save(state);

            var loaded = SettingsPersistence.Load();
            Assert.IsTrue(loaded.MasterMute);
        }

        [Test]
        public void SaveAndLoad_RoundTrips_GraphicsQuality()
        {
            var state = SettingsState.Default;
            state.GraphicsQuality = 1;
            SettingsPersistence.Save(state);

            var loaded = SettingsPersistence.Load();
            Assert.AreEqual(1, loaded.GraphicsQuality);
        }

        [Test]
        public void SaveAndLoad_RoundTrips_Fullscreen()
        {
            var state = SettingsState.Default;
            state.Fullscreen = true;
            SettingsPersistence.Save(state);

            var loaded = SettingsPersistence.Load();
            Assert.IsTrue(loaded.Fullscreen);
        }

        [Test]
        public void Load_ClampsVolumeToZeroOneRange()
        {
            string path = GetTestSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, new[]
            {
                "MusicVolume=2.5",
                "SfxVolume=-0.3",
            });

            var loaded = SettingsPersistence.Load();
            Assert.AreEqual(1f, loaded.MusicVolume, 0.001f);
            Assert.AreEqual(0f, loaded.SfxVolume,   0.001f);
        }

        [Test]
        public void Load_ClampsGraphicsQuality()
        {
            string path = GetTestSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, new[] { "GraphicsQuality=99" });

            var loaded = SettingsPersistence.Load();
            Assert.AreEqual(3, loaded.GraphicsQuality);
        }

        [Test]
        public void Load_IgnoresUnknownKeys()
        {
            string path = GetTestSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, new[]
            {
                "MusicVolume=0.5",
                "UnknownKey=badvalue",
            });

            var loaded = SettingsPersistence.Load();
            Assert.AreEqual(0.5f, loaded.MusicVolume, 0.001f);
        }
    }
}
