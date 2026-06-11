using System;
using System.Collections.Generic;
using System.Text;

namespace Settlers.Simulation
{
    /// <summary>
    /// Serializes and deserializes MapEditorState to a simple JSON-compatible format.
    /// Uses manual string building to avoid any UnityEngine dependency.
    /// </summary>
    public static class MapSerializer
    {
        /// <summary>Serialize the editor state to a JSON string.</summary>
        public static string Serialize(MapEditorState state)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"name\": \"{Esc(state.MapName)}\",");
            sb.AppendLine($"  \"description\": \"{Esc(state.MapDescription)}\",");
            sb.AppendLine($"  \"maxPlayers\": {state.MaxPlayers},");
            sb.AppendLine($"  \"defaultVP\": {state.DefaultVP},");
            sb.AppendLine("  \"sectors\": [");

            for (int i = 0; i < state.Sectors.Count; i++)
            {
                var s = state.Sectors[i];
                bool last = i == state.Sectors.Count - 1;
                sb.AppendLine($"    {{\"id\":{s.Id},\"name\":\"{Esc(s.Name)}\",\"owner\":{s.OwnerId}," +
                    $"\"garrison\":{s.GarrisonStrength},\"fortified\":{B(s.IsFortified)}," +
                    $"\"slots\":{s.BuildSlots},\"x\":{s.X:F2},\"y\":{s.Y:F2}}}" +
                    (last ? "" : ","));
            }

            sb.AppendLine("  ],");
            sb.AppendLine("  \"edges\": [");

            for (int i = 0; i < state.Edges.Count; i++)
            {
                var (a, b) = state.Edges[i];
                bool last = i == state.Edges.Count - 1;
                sb.AppendLine($"    [{a},{b}]" + (last ? "" : ","));
            }

            sb.AppendLine("  ]");
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Deserialize a JSON string produced by Serialize().
        /// Returns null and sets error if parsing fails.
        /// </summary>
        public static MapEditorState Deserialize(string json, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Empty JSON";
                return null;
            }

            try
            {
                var state = new MapEditorState();
                state.MapName = ExtractString(json, "name") ?? "Unnamed";
                state.MapDescription = ExtractString(json, "description") ?? "";
                state.MaxPlayers = ExtractInt(json, "maxPlayers", 2);
                state.DefaultVP = ExtractInt(json, "defaultVP", 4);

                // Sectors: parse simple {id, name, owner, garrison, fortified, slots, x, y}
                int si = json.IndexOf("\"sectors\"", StringComparison.Ordinal);
                int ei = json.IndexOf("\"edges\"", StringComparison.Ordinal);
                if (si >= 0 && ei > si)
                {
                    string sectorBlock = json.Substring(si, ei - si);
                    foreach (string obj in SplitObjects(sectorBlock))
                    {
                        var s = state.AddSector(
                            ExtractFloat(obj, "x", 0f),
                            ExtractFloat(obj, "y", 0f),
                            ExtractInt(obj, "owner", Sector.NEUTRAL));

                        s.Id = ExtractInt(obj, "id", s.Id);
                        s.Name = ExtractString(obj, "name") ?? s.Name;
                        s.GarrisonStrength = ExtractInt(obj, "garrison", 5);
                        s.IsFortified = ExtractBool(obj, "fortified");
                        s.BuildSlots = ExtractInt(obj, "slots", 4);
                    }
                }

                // Edges: parse [[a,b], ...]
                if (ei >= 0)
                {
                    string edgeBlock = json.Substring(ei);
                    foreach (string pair in SplitArrayPairs(edgeBlock))
                    {
                        var nums = pair.Trim('[', ']').Split(',');
                        if (nums.Length == 2 &&
                            int.TryParse(nums[0].Trim(), out int a) &&
                            int.TryParse(nums[1].Trim(), out int b))
                            state.AddEdge(a, b);
                    }
                }

                return state;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        private static string Esc(string s) => s?.Replace("\"", "\\\"") ?? "";
        private static string B(bool v) => v ? "true" : "false";

        /// <summary>Index of the value after "key": with optional whitespace, or -1.</summary>
        private static int FindValueStart(string json, string key)
        {
            string search = $"\"{key}\":";
            int start = json.IndexOf(search, StringComparison.Ordinal);
            if (start < 0) return -1;
            start += search.Length;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t'))
                start++;
            return start;
        }

        private static string ExtractString(string json, string key)
        {
            int start = FindValueStart(json, key);
            if (start < 0 || start >= json.Length || json[start] != '"') return null;
            start++;
            int end = json.IndexOf('"', start);
            return end < 0 ? null : json.Substring(start, end - start);
        }

        private static int ExtractInt(string json, string key, int def)
        {
            int start = FindValueStart(json, key);
            if (start < 0) return def;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
            return int.TryParse(json.Substring(start, end - start), out int v) ? v : def;
        }

        private static float ExtractFloat(string json, string key, float def)
        {
            int start = FindValueStart(json, key);
            if (start < 0) return def;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-'
                || json[end] == '.')) end++;
            return float.TryParse(json.Substring(start, end - start),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : def;
        }

        private static bool ExtractBool(string json, string key)
        {
            int start = FindValueStart(json, key);
            if (start < 0) return false;
            return json.Length > start + 3 && json.Substring(start, 4) == "true";
        }

        private static IEnumerable<string> SplitObjects(string block)
        {
            int depth = 0, start = -1;
            for (int i = 0; i < block.Length; i++)
            {
                if (block[i] == '{') { if (depth++ == 0) start = i; }
                else if (block[i] == '}' && --depth == 0 && start >= 0)
                    yield return block.Substring(start, i - start + 1);
            }
        }

        private static IEnumerable<string> SplitArrayPairs(string block)
        {
            // Track the innermost '[' so the outer array bracket never
            // swallows the first [a,b] pair
            int start = -1;
            for (int i = 0; i < block.Length; i++)
            {
                if (block[i] == '[') start = i;
                else if (block[i] == ']' && start >= 0)
                {
                    string sub = block.Substring(start, i - start + 1);
                    if (sub.Contains(",")) yield return sub;
                    start = -1;
                }
            }
        }
    }
}
