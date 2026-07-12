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
    /// Restoring a parsed save into a state lives in SaveSystem.Apply.cs.
    /// </summary>
    public static partial class SaveSystem
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

            // Clerics (recruited counts; occupation resets with active tasks)
            for (int p = 0; p < state.PlayerCount; p++)
            {
                int n = state.Clerics.GetCount(p, ClericRank.Novice);
                int b = state.Clerics.GetCount(p, ClericRank.Brother);
                int f = state.Clerics.GetCount(p, ClericRank.Father);
                if (n + b + f > 0)
                    sb.AppendLine($"clerics.{p}={n},{b},{f}");
            }

            // Trade outposts claimed
            foreach (var op in state.TradeMapData.AllOutposts)
            {
                if (op.IsClaimed)
                    sb.AppendLine($"outpost.{op.Id}.claimedBy={op.ClaimedBy}");
            }

            // Work yards (attached to buildings, save after buildings)
            foreach (var b in state.Construction.AllBuildings)
            {
                foreach (var wy in b.WorkYards)
                {
                    sb.AppendLine($"wy.{wy.Id}={wy.TypeId}|{wy.BuildingId}|{wy.SectorId}|" +
                        $"{wy.OwnerId}|{wy.RequiredResourceNode}|{wy.LocalX:F2}|{wy.LocalZ:F2}|" +
                        $"{wy.HasWorker}|{wy.HasTool}|{wy.CycleProgress:F4}");
                }
            }

            // Permanent VPs only — dynamic VPs are recalculated each tick
            for (int p = 0; p < state.PlayerCount; p++)
            {
                var allVPs = state.Victory.GetAllVPs(p);
                if (allVPs.Count > 0)
                    sb.AppendLine($"vp.{p}.permanent={string.Join(",", allVPs)}");
            }

            // Generals (with army composition)
            for (int p = 0; p < state.PlayerCount; p++)
            {
                foreach (var gen in state.Army.GetGenerals(p))
                {
                    var units = new List<string>();
                    foreach (UnitType ut in Enum.GetValues(typeof(UnitType)))
                    {
                        int count = gen.GetUnitCount(ut);
                        if (count > 0) units.Add($"{ut}:{count}");
                    }
                    sb.AppendLine($"general.{gen.Id}={gen.OwnerId}|{gen.SectorId}|" +
                        $"{gen.IsMoving}|{string.Join(",", units)}");
                }
            }

            // Training queue
            for (int i = 0; i < state.Army.TrainingQueue.Count; i++)
            {
                var t = state.Army.TrainingQueue[i];
                sb.AppendLine($"training.{i}={t.PlayerId}|{t.SectorId}|{t.UnitType}|" +
                    $"{t.TotalTime:F2}|{t.Progress:F4}");
            }

            // Quests — available quests are re-seeded from QuestDatabase on load
            for (int p = 0; p < state.PlayerCount; p++)
            {
                foreach (var quest in state.Quests.GetActiveQuests(p))
                    sb.AppendLine($"quest.active.{quest.Id}={p}");
            }
            if (state.Quests.CompletedQuestIds.Count > 0)
                sb.AppendLine($"quest.completed={string.Join(",", state.Quests.CompletedQuestIds)}");

            // Unresolved conquest reward choices
            foreach (var pending in state.ConquestRewards.Pending)
                sb.AppendLine($"reward.pending.{pending.SectorId}={pending.PlayerId}");

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
