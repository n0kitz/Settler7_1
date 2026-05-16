using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Settlers.Simulation
{
    /// <summary>
    /// Saves and loads action logs to/from a simple text format.
    /// Each line: timestamp|playerId|actionType|payload
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class ReplaySerializer
    {
        private static readonly char SEP = '|';

        /// <summary>Serialize a list of ActionRecords to a multi-line string.</summary>
        public static string Serialize(IReadOnlyList<ActionRecord> records)
        {
            var sb = new StringBuilder();
            foreach (var r in records)
                sb.AppendLine(
                    $"{r.Timestamp.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)}" +
                    $"{SEP}{r.PlayerId}{SEP}{r.ActionType}{SEP}{r.Payload}");
            return sb.ToString();
        }

        /// <summary>Deserialize a multi-line string into ActionRecords.</summary>
        public static List<ActionRecord> Deserialize(string data)
        {
            var records = new List<ActionRecord>();
            if (string.IsNullOrWhiteSpace(data)) return records;

            foreach (var raw in data.Split('\n'))
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var parts = line.Split(SEP);
                if (parts.Length < 4) continue;

                if (!float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float ts))
                    continue;
                if (!int.TryParse(parts[1], out int pid)) continue;

                records.Add(new ActionRecord(ts, pid, parts[2], parts[3]));
            }
            return records;
        }

        /// <summary>Save a replay to AppData/Settlers7/Replays/latest.replay.</summary>
        public static void SaveLatest(IReadOnlyList<ActionRecord> records)
        {
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Settlers7", "Replays");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "latest.replay"),
                    Serialize(records));
            }
            catch (Exception) { }
        }

        /// <summary>Load the latest replay. Returns empty list if none found.</summary>
        public static List<ActionRecord> LoadLatest()
        {
            try
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Settlers7", "Replays", "latest.replay");
                if (!File.Exists(path)) return new List<ActionRecord>();
                return Deserialize(File.ReadAllText(path));
            }
            catch (Exception)
            {
                return new List<ActionRecord>();
            }
        }
    }
}
