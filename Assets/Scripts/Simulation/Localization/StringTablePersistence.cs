using System;
using System.Collections.Generic;
using System.IO;

namespace Settlers.Simulation
{
    /// <summary>
    /// Loads locale CSV files from Resources/Localization/ at runtime.
    /// Format: key,value  (one entry per line, # = comment, no headers)
    /// Falls back to the embedded English table if the file is absent.
    /// Pure C# — loads from disk, not UnityEngine.Resources.
    /// </summary>
    public static class StringTablePersistence
    {
        private static readonly string LOCALIZATION_FOLDER = "Assets/Resources/Localization";

        /// <summary>Load a string table for the given locale (e.g., "en", "de").</summary>
        public static Dictionary<string, string> Load(string locale)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string path = Path.Combine(LOCALIZATION_FOLDER, $"StringTable.{locale}.csv");

            try
            {
                if (!File.Exists(path)) return result;

                foreach (var rawLine in File.ReadAllLines(path))
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                    int comma = line.IndexOf(',');
                    if (comma < 0) continue;

                    string key = line.Substring(0, comma).Trim();
                    string val = line.Substring(comma + 1).Trim()
                        .Replace("\\n", "\n");

                    if (!string.IsNullOrEmpty(key))
                        result[key] = val;
                }
            }
            catch (Exception) { /* return empty — will fall back to key */ }

            return result;
        }

        /// <summary>Save a string table to disk (used by locale editor tools).</summary>
        public static void Save(string locale, Dictionary<string, string> strings)
        {
            try
            {
                Directory.CreateDirectory(LOCALIZATION_FOLDER);
                var lines = new List<string> { "# Settlers 7 — Locale: " + locale };
                foreach (var kv in strings)
                    lines.Add($"{kv.Key},{kv.Value.Replace("\n", "\\n")}");
                File.WriteAllLines(
                    Path.Combine(LOCALIZATION_FOLDER, $"StringTable.{locale}.csv"), lines);
            }
            catch (Exception) { }
        }
    }
}
