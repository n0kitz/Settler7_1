using System;
using System.IO;
using System.Globalization;

namespace Settlers.Simulation
{
    /// <summary>
    /// Saves and loads SettingsState as a simple key=value text file.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class SettingsPersistence
    {
        private static readonly string FILE_NAME = "settings.ini";

        public static void Save(SettingsState state)
        {
            try
            {
                var lines = new[]
                {
                    $"MusicVolume={state.MusicVolume.ToString("F3", CultureInfo.InvariantCulture)}",
                    $"SfxVolume={state.SfxVolume.ToString("F3", CultureInfo.InvariantCulture)}",
                    $"MasterMute={state.MasterMute}",
                    $"GraphicsQuality={state.GraphicsQuality}",
                    $"Fullscreen={state.Fullscreen}",
                };
                File.WriteAllLines(GetPath(), lines);
            }
            catch (Exception) { /* non-fatal */ }
        }

        public static SettingsState Load()
        {
            var state = SettingsState.Default;
            try
            {
                string path = GetPath();
                if (!File.Exists(path)) return state;

                foreach (var raw in File.ReadAllLines(path))
                {
                    var line = raw.Trim();
                    int sep = line.IndexOf('=');
                    if (sep < 0) continue;
                    string key = line.Substring(0, sep).Trim();
                    string val = line.Substring(sep + 1).Trim();
                    ApplyLine(state, key, val);
                }
            }
            catch (Exception) { /* return defaults on any parse error */ }
            return state;
        }

        private static void ApplyLine(SettingsState s, string key, string val)
        {
            switch (key)
            {
                case "MusicVolume":
                    if (float.TryParse(val, NumberStyles.Float,
                            CultureInfo.InvariantCulture, out float mv))
                        s.MusicVolume = Clamp01(mv);
                    break;
                case "SfxVolume":
                    if (float.TryParse(val, NumberStyles.Float,
                            CultureInfo.InvariantCulture, out float sv))
                        s.SfxVolume = Clamp01(sv);
                    break;
                case "MasterMute":
                    s.MasterMute = string.Equals(val, "True",
                        StringComparison.OrdinalIgnoreCase);
                    break;
                case "GraphicsQuality":
                    if (int.TryParse(val, out int gq))
                        s.GraphicsQuality = Clamp(gq, 0, 3);
                    break;
                case "Fullscreen":
                    s.Fullscreen = string.Equals(val, "True",
                        StringComparison.OrdinalIgnoreCase);
                    break;
            }
        }

        private static string GetPath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Settlers7");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, FILE_NAME);
        }

        private static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
        private static int Clamp(int v, int min, int max) => v < min ? min : v > max ? max : v;
    }
}
