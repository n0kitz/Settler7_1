using System;
using System.Collections.Generic;
using System.IO;

namespace Settlers.Simulation
{
    /// <summary>
    /// Persists which achievements have been unlocked across sessions.
    /// Format: one line per achievement: id|ISO8601-timestamp
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class AchievementProgress
    {
        private static readonly string FILE_NAME = "achievements.txt";
        private static Dictionary<string, DateTime> _unlocked;

        public static IReadOnlyDictionary<string, DateTime> Unlocked
        {
            get { Load(); return _unlocked; }
        }

        public static bool IsUnlocked(string id)
        {
            Load();
            return _unlocked.ContainsKey(id);
        }

        public static void MarkUnlocked(string id)
        {
            Load();
            if (_unlocked.ContainsKey(id)) return;
            _unlocked[id] = DateTime.UtcNow;
            Save();
        }

        public static void Reset()
        {
            _unlocked = new Dictionary<string, DateTime>();
            Save();
        }

        private static bool _loaded;
        private static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            _unlocked = new Dictionary<string, DateTime>();
            try
            {
                string path = GetPath();
                if (!File.Exists(path)) return;
                foreach (var raw in File.ReadAllLines(path))
                {
                    int sep = raw.IndexOf('|');
                    if (sep < 0) continue;
                    string achId = raw.Substring(0, sep).Trim();
                    string ts   = raw.Substring(sep + 1).Trim();
                    if (DateTime.TryParse(ts, out var dt))
                        _unlocked[achId] = dt;
                }
            }
            catch (Exception) { /* silently use empty dict */ }
        }

        private static void Save()
        {
            try
            {
                var lines = new List<string>();
                foreach (var kv in _unlocked)
                    lines.Add($"{kv.Key}|{kv.Value:O}");
                File.WriteAllLines(GetPath(), lines);
            }
            catch (Exception) { /* non-fatal */ }
        }

        private static string GetPath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Settlers7");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, FILE_NAME);
        }
    }
}
