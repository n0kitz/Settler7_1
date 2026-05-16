using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Settlers.Simulation
{
    /// <summary>
    /// Saves and loads match history as a simple pipe-delimited flat file.
    /// Keeps the last 50 matches. Pure C# — no UnityEngine references.
    /// </summary>
    public static class MatchHistoryPersistence
    {
        private static readonly string FILE_NAME  = "match_history.txt";
        private const int MAX_ENTRIES = 50;

        public static List<MatchResult> Load()
        {
            var list = new List<MatchResult>();
            try
            {
                string path = GetPath();
                if (!File.Exists(path)) return list;

                foreach (var line in File.ReadAllLines(path))
                {
                    var r = ParseLine(line.Trim());
                    if (r != null) list.Add(r);
                }
            }
            catch (Exception) { /* return partial list on error */ }
            return list;
        }

        public static void Append(MatchResult result)
        {
            var list = Load();
            list.Add(result);
            if (list.Count > MAX_ENTRIES)
                list.RemoveRange(0, list.Count - MAX_ENTRIES);
            Save(list);
        }

        private static void Save(List<MatchResult> list)
        {
            try
            {
                var lines = new List<string>();
                foreach (var r in list)
                    lines.Add(FormatLine(r));
                File.WriteAllLines(GetPath(), lines);
            }
            catch (Exception) { /* non-fatal */ }
        }

        private static string FormatLine(MatchResult r)
        {
            var ci = CultureInfo.InvariantCulture;
            return string.Join("|", new[]
            {
                r.MapId,
                r.WinnerId.ToString(ci),
                r.DurationSeconds.ToString("F1", ci),
                r.PlayerCount.ToString(ci),
                r.VPRequired.ToString(ci),
                r.BuildingsBuilt.ToString(ci),
                r.SectorsConquered.ToString(ci),
                r.TechsResearched.ToString(ci),
                r.TradesCompleted.ToString(ci),
                r.Score.ToString(ci),
                r.PlayedAt.ToString("O"),
            });
        }

        private static MatchResult ParseLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            var f = line.Split('|');
            if (f.Length < 11) return null;
            try
            {
                var ci = CultureInfo.InvariantCulture;
                return new MatchResult(
                    f[0],
                    int.Parse(f[1], ci),
                    float.Parse(f[2], ci),
                    int.Parse(f[3], ci),
                    int.Parse(f[4], ci),
                    int.Parse(f[5], ci),
                    int.Parse(f[6], ci),
                    int.Parse(f[7], ci),
                    int.Parse(f[8], ci),
                    int.Parse(f[9], ci),
                    DateTime.Parse(f[10], null, System.Globalization.DateTimeStyles.RoundtripKind));
            }
            catch (Exception) { return null; }
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
