using System;
using System.Collections.Generic;
using System.IO;

namespace Settlers.Simulation
{
    /// <summary>
    /// Discovers *.map.json files inside each loaded mod's Maps/ subfolder and
    /// makes them available to MapSelectionUI alongside built-in maps.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class CustomMapRegistry
    {
        public sealed class CustomMapEntry
        {
            public string MapId;       // filename without extension
            public string DisplayName; // from first line comment or filename
            public string FilePath;
            public string ModId;
        }

        private static readonly List<CustomMapEntry> _maps = new List<CustomMapEntry>();

        public static IReadOnlyList<CustomMapEntry> Maps => _maps;

        /// <summary>Scan all loaded mods for *.map.json files.</summary>
        public static void Reload()
        {
            _maps.Clear();
            foreach (var mod in ModLoader.Loaded)
            {
                string mapsDir = Path.Combine(mod.RootPath, "Maps");
                if (!Directory.Exists(mapsDir)) continue;

                foreach (var file in Directory.GetFiles(mapsDir, "*.map.json"))
                {
                    string mapId = Path.GetFileNameWithoutExtension(file)
                        .Replace(".map", "");
                    string displayName = ExtractDisplayName(file, mapId);
                    _maps.Add(new CustomMapEntry
                    {
                        MapId       = mapId,
                        DisplayName = displayName,
                        FilePath    = file,
                        ModId       = mod.ModId,
                    });
                }
            }
        }

        /// <summary>Load and deserialize a custom map to MapEditorState.</summary>
        public static MapEditorState LoadMap(string mapId, out string error)
        {
            foreach (var entry in _maps)
            {
                if (entry.MapId == mapId)
                {
                    try
                    {
                        string json = File.ReadAllText(entry.FilePath);
                        return MapSerializer.Deserialize(json, out error);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                        return null;
                    }
                }
            }
            error = $"Map '{mapId}' not found in mod registry";
            return null;
        }

        private static string ExtractDisplayName(string filePath, string fallback)
        {
            try
            {
                var firstLine = File.ReadLines(filePath).GetEnumerator();
                if (firstLine.MoveNext())
                {
                    var line = firstLine.Current?.Trim();
                    if (!string.IsNullOrEmpty(line) && line.StartsWith("//"))
                        return line.Substring(2).Trim();
                }
            }
            catch (Exception) { }
            return fallback;
        }
    }
}
