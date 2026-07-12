using System;
using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// SaveSystem — restoring a parsed save into a fresh GameState.
    /// Serialization/parsing lives in SaveSystem.cs.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static partial class SaveSystem
    {
        /// <summary>
        /// Apply save data to a GameState. Call after creating a fresh state
        /// with the same map. ID counters must be reset before calling this.
        /// </summary>
        public static void ApplyToState(GameState state, Dictionary<string, string> data)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;

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

            // Prestige
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

            // Buildings — must be restored before work yards
            if (data.TryGetValue("buildingCount", out var bcStr) && int.TryParse(bcStr, out int bc))
            {
                for (int id = 0; id < bc; id++)
                {
                    if (!data.TryGetValue($"building.{id}", out var bStr)) continue;
                    var parts = bStr.Split('|');
                    if (parts.Length < 10) continue;

                    var bType = (BaseBuildingType)Enum.Parse(typeof(BaseBuildingType), parts[0]);
                    int sectorId   = int.Parse(parts[1]);
                    int ownerId    = int.Parse(parts[2]);
                    var bState     = (BuildingState)Enum.Parse(typeof(BuildingState), parts[3]);
                    float prog     = float.Parse(parts[4], inv);
                    int upLevel    = int.Parse(parts[5]);
                    int maxWY      = int.Parse(parts[6]);
                    float lx       = float.Parse(parts[7], inv);
                    float lz       = float.Parse(parts[8], inv);
                    var food       = (FoodSetting)Enum.Parse(typeof(FoodSetting), parts[9]);

                    state.Construction.RestoreBuilding(
                        bType, sectorId, ownerId, maxWY, lx, lz,
                        bState, prog, upLevel, food);
                }
            }

            // Work yards — buildings must already be restored above
            foreach (var kvp in data)
            {
                if (!kvp.Key.StartsWith("wy.")) continue;
                var parts = kvp.Value.Split('|');
                if (parts.Length < 10) continue;

                string typeId          = parts[0];
                int buildingId         = int.Parse(parts[1]);
                int sectorId           = int.Parse(parts[2]);
                int ownerId            = int.Parse(parts[3]);
                var reqNode = (ResourceNodeType)Enum.Parse(typeof(ResourceNodeType), parts[4]);
                float lx               = float.Parse(parts[5], inv);
                float lz               = float.Parse(parts[6], inv);
                bool hasWorker         = bool.Parse(parts[7]);
                bool hasTool           = bool.Parse(parts[8]);
                float cycleProgress    = float.Parse(parts[9], inv);

                var building = state.Construction.GetBuilding(buildingId);
                if (building == null) continue;

                var wy = new WorkYard(typeId, buildingId, sectorId, ownerId, reqNode, lx, lz);
                wy.RestoreState(hasWorker, hasTool, cycleProgress);
                building.AttachWorkYard(wy);
                state.Production.RegisterWorkYard(wy);
            }

            // Tech research
            for (int p = 0; p < state.PlayerCount; p++)
            {
                if (!data.TryGetValue($"techs.{p}", out var techStr)) continue;
                foreach (var techId in techStr.Split(','))
                {
                    if (!string.IsNullOrEmpty(techId))
                        state.Research.RestoreTech(p, techId.Trim());
                }
            }

            // Clerics
            for (int p = 0; p < state.PlayerCount; p++)
            {
                if (!data.TryGetValue($"clerics.{p}", out var clericStr)) continue;
                var parts = clericStr.Split(',');
                if (parts.Length != 3) continue;
                if (int.TryParse(parts[0], out int n))
                    state.Clerics.RestoreCount(p, ClericRank.Novice, n);
                if (int.TryParse(parts[1], out int b))
                    state.Clerics.RestoreCount(p, ClericRank.Brother, b);
                if (int.TryParse(parts[2], out int f))
                    state.Clerics.RestoreCount(p, ClericRank.Father, f);
            }

            // Trade outpost claims
            foreach (var kvp in data)
            {
                if (!kvp.Key.StartsWith("outpost.") || !kvp.Key.EndsWith(".claimedBy"))
                    continue;
                string outpostId = kvp.Key.Substring("outpost.".Length,
                    kvp.Key.Length - "outpost.".Length - ".claimedBy".Length);
                int claimOwner = int.Parse(kvp.Value);
                var outpost = state.TradeMapData.GetOutpost(outpostId);
                outpost?.TryClaim(claimOwner);
            }

            // Permanent VPs
            for (int p = 0; p < state.PlayerCount; p++)
            {
                if (!data.TryGetValue($"vp.{p}.permanent", out var vpStr)) continue;
                foreach (var vpId in vpStr.Split(','))
                {
                    if (!string.IsNullOrEmpty(vpId))
                        state.Victory.AwardPermanentVP(p, vpId.Trim());
                }
            }

            // Generals — restore with army composition
            foreach (var kvp in data)
            {
                if (!kvp.Key.StartsWith("general.")) continue;
                int genId = int.Parse(kvp.Key.Substring("general.".Length));
                var parts = kvp.Value.Split('|');
                if (parts.Length < 4) continue;

                int ownerId   = int.Parse(parts[0]);
                int sectorId  = int.Parse(parts[1]);
                bool isMoving = bool.Parse(parts[2]);
                var units = new Dictionary<UnitType, int>();
                if (parts[3].Length > 0)
                {
                    foreach (var entry in parts[3].Split(','))
                    {
                        var pair = entry.Split(':');
                        units[(UnitType)Enum.Parse(typeof(UnitType), pair[0])] =
                            int.Parse(pair[1]);
                    }
                }
                state.Army.RestoreGeneral(genId, ownerId, sectorId, isMoving, units);
            }

            // Training queue
            for (int i = 0; data.TryGetValue($"training.{i}", out var tStr); i++)
            {
                var parts = tStr.Split('|');
                if (parts.Length < 5) continue;
                state.Army.RestoreTrainingTask(
                    int.Parse(parts[0]), int.Parse(parts[1]),
                    (UnitType)Enum.Parse(typeof(UnitType), parts[2]),
                    float.Parse(parts[3], inv), float.Parse(parts[4], inv));
            }

            // Quests — completed first so active restores can't resurrect them
            if (data.TryGetValue("quest.completed", out var cqStr))
            {
                foreach (var id in cqStr.Split(','))
                {
                    if (!string.IsNullOrEmpty(id))
                        state.Quests.RestoreCompletedQuest(id.Trim());
                }
            }
            foreach (var kvp in data)
            {
                if (!kvp.Key.StartsWith("quest.active.")) continue;
                string questId = kvp.Key.Substring("quest.active.".Length);
                state.Quests.RestoreActiveQuest(int.Parse(kvp.Value), questId);
            }

            // Unresolved conquest reward choices
            foreach (var kvp in data)
            {
                if (!kvp.Key.StartsWith("reward.pending.")) continue;
                int sectorId = int.Parse(kvp.Key.Substring("reward.pending.".Length));
                state.ConquestRewards.RestorePending(int.Parse(kvp.Value), sectorId);
            }

            // Simulation time (apply last — doesn't affect anything above)
            if (data.TryGetValue("simTime", out var timeStr))
            {
                float time = float.Parse(timeStr, inv);
                state.AdvanceTime(time);
            }
        }
    }
}
