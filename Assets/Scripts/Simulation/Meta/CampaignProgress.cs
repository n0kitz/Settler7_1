using System;
using System.Collections.Generic;
using System.IO;

namespace Settlers.Simulation
{
    /// <summary>
    /// Persists which campaign missions have been completed.
    /// Saved as a simple text file (one mission ID per line) next to the save games.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class CampaignProgress
    {
        private static readonly string SAVE_FILE = "campaign_progress.txt";

        private readonly HashSet<string> _completed = new();

        /// <summary>IDs of all completed missions.</summary>
        public IReadOnlyCollection<string> CompletedIds => _completed;

        /// <summary>True if the given mission has been beaten.</summary>
        public bool IsCompleted(string missionId) => _completed.Contains(missionId);

        /// <summary>Mark a mission as complete and persist to disk.</summary>
        public void MarkComplete(string missionId)
        {
            if (_completed.Add(missionId))
                Save();
        }

        /// <summary>Reset all progress (debug/new-game-plus).</summary>
        public void Reset()
        {
            _completed.Clear();
            Save();
        }

        // --- Persistence ---

        private static string GetSavePath()
        {
            // Mirror the pattern used by SaveSystem for locating the saves folder.
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Settlers7");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, SAVE_FILE);
        }

        private void Save()
        {
            try
            {
                File.WriteAllLines(GetSavePath(), _completed);
            }
            catch (Exception ex)
            {
                // Non-fatal — progress is still in memory for this session.
                _ = ex;
            }
        }

        /// <summary>Load progress from disk. Returns empty progress on failure.</summary>
        public static CampaignProgress Load()
        {
            var p = new CampaignProgress();
            try
            {
                string path = GetSavePath();
                if (File.Exists(path))
                    foreach (var line in File.ReadAllLines(path))
                        if (!string.IsNullOrWhiteSpace(line))
                            p._completed.Add(line.Trim());
            }
            catch { /* return empty progress */ }
            return p;
        }
    }
}
