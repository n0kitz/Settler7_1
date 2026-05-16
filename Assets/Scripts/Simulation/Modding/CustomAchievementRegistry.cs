using System;
using System.Collections.Generic;
using System.IO;

namespace Settlers.Simulation
{
    /// <summary>
    /// Discovers *.achievement.json files in loaded mods and provides them
    /// to AchievementSystem alongside built-in achievements.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class CustomAchievementRegistry
    {
        public sealed class CustomAchievementEntry
        {
            public string Id;
            public string DisplayName;
            public string Description;
            public string ConditionTag; // matches AchievementConditionType tag
            public int    ConditionValue;
            public string ModId;
        }

        private static readonly List<CustomAchievementEntry> _achievements =
            new List<CustomAchievementEntry>();

        public static IReadOnlyList<CustomAchievementEntry> Achievements => _achievements;

        public static void Reload()
        {
            _achievements.Clear();
            foreach (var mod in ModLoader.Loaded)
            {
                string dir = Path.Combine(mod.RootPath, "Achievements");
                if (!Directory.Exists(dir)) continue;

                foreach (var file in Directory.GetFiles(dir, "*.achievement.json"))
                {
                    try
                    {
                        var entry = ParseFile(file, mod.ModId);
                        if (entry != null) _achievements.Add(entry);
                    }
                    catch (Exception) { /* skip malformed */ }
                }
            }
        }

        private static CustomAchievementEntry ParseFile(string path, string modId)
        {
            var entry = new CustomAchievementEntry { ModId = modId };
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                // Minimal JSON key extraction: "key": "value"
                int colon = line.IndexOf(':');
                if (colon < 0) continue;
                string key = line.Substring(0, colon).Trim().Trim('"');
                string val = line.Substring(colon + 1).Trim().Trim(',').Trim('"');
                switch (key)
                {
                    case "id":             entry.Id             = val; break;
                    case "displayName":    entry.DisplayName    = val; break;
                    case "description":    entry.Description    = val; break;
                    case "conditionTag":   entry.ConditionTag   = val; break;
                    case "conditionValue":
                        int.TryParse(val, out entry.ConditionValue); break;
                }
            }
            return string.IsNullOrEmpty(entry.Id) ? null : entry;
        }
    }
}
