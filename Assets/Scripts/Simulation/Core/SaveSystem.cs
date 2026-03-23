using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Settlers.Simulation
{
    /// <summary>
    /// Serializes/deserializes GameState to a simple text-based save format.
    /// Uses a key-value line format for simplicity and human readability.
    /// Pure C# — no UnityEngine references (caller provides file path).
    /// </summary>
    public static class SaveSystem
    {
        private const string SAVE_VERSION = "1";

        /// <summary>Serialize the current game state to a string.</summary>
        public static string Serialize(GameState state)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"version={SAVE_VERSION}");
            sb.AppendLine($"simTime={state.SimulationTime:F2}");
            sb.AppendLine($"playerCount={state.PlayerCount}");

            // Sector ownership
            sb.AppendLine($"sectorCount={state.Graph.SectorCount}");
            for (int i = 0; i < state.Graph.SectorCount; i++)
            {
                var s = state.Graph.GetSector(i);
                sb.AppendLine($"sector.{i}.owner={s.OwnerId}");
                sb.AppendLine($"sector.{i}.garrison={s.GarrisonStrength}");
                sb.AppendLine($"sector.{i}.fortified={s.IsFortified}");
            }

            // Player resources
            for (int p = 0; p < state.PlayerCount; p++)
            {
                if (!state.PlayerResources.TryGetValue(p, out var res)) continue;
                foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
                {
                    int amount = res.Get(rt);
                    if (amount > 0)
                        sb.AppendLine($"res.{p}.{rt}={amount}");
                }
            }

            // Prestige
            for (int p = 0; p < state.PlayerCount; p++)
            {
                sb.AppendLine($"prestige.{p}.points={state.Prestige.GetPoints(p)}");
                var unlocks = state.Prestige.GetUnlocks(p);
                if (unlocks.Count > 0)
                    sb.AppendLine($"prestige.{p}.unlocks={string.Join(",", unlocks)}");
            }

            // Buildings
            sb.AppendLine($"buildingCount={state.Construction.AllBuildings.Count}");
            foreach (var b in state.Construction.AllBuildings)
            {
                sb.AppendLine($"building.{b.Id}={b.Type}|{b.SectorId}|{b.OwnerId}|" +
                    $"{b.State}|{b.ConstructionProgress:F3}|{b.UpgradeLevel}|" +
                    $"{b.MaxWorkYards}|{b.LocalX:F2}|{b.LocalZ:F2}|{b.FoodSetting}");
            }

            // Technologies
            for (int p = 0; p < state.PlayerCount; p++)
            {
                var techs = state.Research.GetPlayerTechs(p);
                if (techs.Count > 0)
                    sb.AppendLine($"techs.{p}={string.Join(",", techs)}");
            }

            // Trade outposts claimed
            foreach (var op in state.TradeMapData.AllOutposts)
            {
                if (op.IsClaimed)
                    sb.AppendLine($"outpost.{op.Id}.claimedBy={op.ClaimedBy}");
            }

            // Victory
            for (int p = 0; p < state.PlayerCount; p++)
            {
                int vpCount = state.Victory.GetVPCount(p);
                if (vpCount > 0)
                    sb.AppendLine($"vp.{p}.count={vpCount}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize save data into a dictionary for loading.
        /// Returns key-value pairs that can be applied to a fresh GameState.
        /// </summary>
        public static Dictionary<string, string> Deserialize(string saveData)
        {
            var data = new Dictionary<string, string>();
            var lines = saveData.Split('\n');
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string key = line.Substring(0, eq);
                string val = line.Substring(eq + 1);
                data[key] = val;
            }
            return data;
        }

        /// <summary>
        /// Apply save data to a GameState. Call after creating a fresh state
        /// with the same map.
        /// </summary>
        public static void ApplyToState(GameState state, Dictionary<string, string> data)
        {
            // Sector ownership
            for (int i = 0; i < state.Graph.SectorCount; i++)
            {
                if (data.TryGetValue($"sector.{i}.owner", out var ownerStr))
                    state.Graph.GetSector(i).SetOwner(int.Parse(ownerStr));
                if (data.TryGetValue($"sector.{i}.garrison", out var garStr))
                    state.Graph.GetSector(i).SetGarrison(int.Parse(garStr));
                if (data.TryGetValue($"sector.{i}.fortified", out var fortStr))
                    state.Graph.GetSector(i).SetFortified(bool.Parse(fortStr));
            }

            // Player resources
            for (int p = 0; p < state.PlayerCount; p++)
            {
                if (!state.PlayerResources.TryGetValue(p, out var res)) continue;
                foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
                {
                    if (data.TryGetValue($"res.{p}.{rt}", out var amtStr))
                        res.Set(rt, int.Parse(amtStr));
                }
            }

            // Prestige points
            for (int p = 0; p < state.PlayerCount; p++)
            {
                if (data.TryGetValue($"prestige.{p}.points", out var ptsStr))
                {
                    int pts = int.Parse(ptsStr);
                    int current = state.Prestige.GetPoints(p);
                    if (pts > current)
                        state.Prestige.AwardPoints(p, pts - current);
                }
                if (data.TryGetValue($"prestige.{p}.unlocks", out var unlockStr))
                {
                    foreach (var id in unlockStr.Split(','))
                    {
                        if (!string.IsNullOrEmpty(id))
                            state.Prestige.TryUnlock(p, id.Trim());
                    }
                }
            }

            // Simulation time
            if (data.TryGetValue("simTime", out var timeStr))
            {
                float time = float.Parse(timeStr,
                    System.Globalization.CultureInfo.InvariantCulture);
                state.AdvanceTime(time);
            }
        }

        /// <summary>Save to a file.</summary>
        public static void SaveToFile(GameState state, string filePath)
        {
            string data = Serialize(state);
            File.WriteAllText(filePath, data);
        }

        /// <summary>Load from a file and apply to a game state.</summary>
        public static void LoadFromFile(GameState state, string filePath)
        {
            if (!File.Exists(filePath)) return;
            string data = File.ReadAllText(filePath);
            var parsed = Deserialize(data);
            ApplyToState(state, parsed);
        }
    }
}
