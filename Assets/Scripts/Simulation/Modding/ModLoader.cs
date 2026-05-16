using System;
using System.Collections.Generic;
using System.IO;

namespace Settlers.Simulation
{
    /// <summary>
    /// Scans ~/AppData/Roaming/Settlers7/Mods/ and loads all enabled mod manifests.
    /// Individual subsystem registries (CustomMapRegistry, etc.) then read from
    /// each ModManifest.RootPath to discover content files.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class ModLoader
    {
        private static readonly string MODS_FOLDER = "Mods";
        private static readonly string MANIFEST_FILE = "manifest.ini";

        private static List<ModManifest> _loaded = new List<ModManifest>();

        public static IReadOnlyList<ModManifest> Loaded => _loaded;

        /// <summary>Scan the Mods folder and reload all enabled manifests.</summary>
        public static void Reload()
        {
            _loaded = new List<ModManifest>();
            string modsRoot = GetModsRoot();
            if (!Directory.Exists(modsRoot)) return;

            foreach (var modDir in Directory.GetDirectories(modsRoot))
            {
                string manifestPath = Path.Combine(modDir, MANIFEST_FILE);
                try
                {
                    var lines = File.Exists(manifestPath)
                        ? File.ReadAllLines(manifestPath)
                        : Array.Empty<string>();
                    var manifest = ModManifest.Parse(modDir, lines);
                    if (manifest.Enabled)
                        _loaded.Add(manifest);
                }
                catch (Exception) { /* skip malformed mods */ }
            }
        }

        /// <summary>Enable or disable a mod by ID and persist the change.</summary>
        public static void SetEnabled(string modId, bool enabled)
        {
            string manifestPath = Path.Combine(GetModsRoot(), modId, MANIFEST_FILE);
            if (!File.Exists(manifestPath)) return;

            var lines = new List<string>(File.ReadAllLines(manifestPath));
            bool found = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].TrimStart().StartsWith("Enabled="))
                {
                    lines[i] = $"Enabled={enabled}";
                    found = true;
                    break;
                }
            }
            if (!found) lines.Add($"Enabled={enabled}");
            File.WriteAllLines(manifestPath, lines);
        }

        public static string GetModsRoot()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Settlers7", MODS_FOLDER);
        }
    }
}
