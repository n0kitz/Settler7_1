using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>Cleric ranks (§14.6): Geistliche / Mönche / Prälaten.</summary>
    public enum ClericRank { Novice = 0, Brother = 1, Father = 2 }

    /// <summary>
    /// Church cleric roster. Players recruit clerics for goods; research
    /// occupies them for its duration and releases them on completion or
    /// cancel. Pure C# — no UnityEngine references.
    /// </summary>
    public class ClericSystem
    {
        private const int RANK_COUNT = 3;

        private static readonly (ResourceType type, int amount)[][] RECRUIT_COSTS =
        {
            new[] { (ResourceType.Bread, 1), (ResourceType.Coins, 2) },                          // Novice
            new[] { (ResourceType.Bread, 1), (ResourceType.Books, 1), (ResourceType.Coins, 3) }, // Brother
            new[] { (ResourceType.Books, 1), (ResourceType.Garments, 1), (ResourceType.Coins, 5) } // Father
        };

        private readonly Dictionary<int, int[]> _counts = new();
        private readonly Dictionary<int, int[]> _occupied = new();
        private readonly Dictionary<int, PlayerResources> _resources;
        private readonly EventBus _eventBus;

        public ClericSystem(Dictionary<int, PlayerResources> resources, EventBus eventBus)
        {
            _resources = resources;
            _eventBus = eventBus;
        }

        /// <summary>Goods required to recruit one cleric of the given rank.</summary>
        public static IReadOnlyList<(ResourceType type, int amount)> GetRecruitCost(
            ClericRank rank) => RECRUIT_COSTS[(int)rank];

        /// <summary>Total recruited clerics of a rank (including occupied).</summary>
        public int GetCount(int playerId, ClericRank rank) =>
            _counts.TryGetValue(playerId, out var c) ? c[(int)rank] : 0;

        /// <summary>Clerics of a rank not currently assigned to research.</summary>
        public int GetAvailable(int playerId, ClericRank rank) =>
            GetCount(playerId, rank) - GetOccupied(playerId, rank);

        /// <summary>Clerics of a rank currently assigned to research.</summary>
        public int GetOccupied(int playerId, ClericRank rank) =>
            _occupied.TryGetValue(playerId, out var o) ? o[(int)rank] : 0;

        /// <summary>
        /// Recruit one cleric, spending the rank's goods. Returns false when
        /// the player cannot afford it.
        /// </summary>
        public bool Recruit(int playerId, ClericRank rank)
        {
            if (!_resources.TryGetValue(playerId, out var res))
                return false;

            var cost = RECRUIT_COSTS[(int)rank];
            for (int i = 0; i < cost.Length; i++)
                if (!res.Has(cost[i].type, cost[i].amount))
                    return false;

            for (int i = 0; i < cost.Length; i++)
                res.TrySpend(cost[i].type, cost[i].amount);

            var counts = GetOrCreate(_counts, playerId);
            counts[(int)rank]++;
            _eventBus?.Publish(new ClericRecruitedEvent(playerId, rank, counts[(int)rank]));
            return true;
        }

        /// <summary>True if the player has that many unoccupied clerics per rank.</summary>
        public bool HasAvailable(int playerId, int novices, int brothers, int fathers) =>
            GetAvailable(playerId, ClericRank.Novice) >= novices &&
            GetAvailable(playerId, ClericRank.Brother) >= brothers &&
            GetAvailable(playerId, ClericRank.Father) >= fathers;

        /// <summary>
        /// Assign clerics to a research task. All-or-nothing; returns false
        /// when not enough are available.
        /// </summary>
        public bool TryOccupy(int playerId, int novices, int brothers, int fathers)
        {
            if (!HasAvailable(playerId, novices, brothers, fathers))
                return false;
            var occupied = GetOrCreate(_occupied, playerId);
            occupied[(int)ClericRank.Novice] += novices;
            occupied[(int)ClericRank.Brother] += brothers;
            occupied[(int)ClericRank.Father] += fathers;
            return true;
        }

        /// <summary>Release clerics when research completes or is cancelled.</summary>
        public void Release(int playerId, int novices, int brothers, int fathers)
        {
            if (!_occupied.TryGetValue(playerId, out var occupied)) return;
            occupied[(int)ClericRank.Novice] =
                System.Math.Max(0, occupied[(int)ClericRank.Novice] - novices);
            occupied[(int)ClericRank.Brother] =
                System.Math.Max(0, occupied[(int)ClericRank.Brother] - brothers);
            occupied[(int)ClericRank.Father] =
                System.Math.Max(0, occupied[(int)ClericRank.Father] - fathers);
        }

        /// <summary>
        /// Restore a recruited count when loading a save. Occupied counts are
        /// not persisted (active research tasks are not persisted either).
        /// </summary>
        public void RestoreCount(int playerId, ClericRank rank, int count)
        {
            GetOrCreate(_counts, playerId)[(int)rank] = count;
        }

        private static int[] GetOrCreate(Dictionary<int, int[]> map, int playerId)
        {
            if (!map.TryGetValue(playerId, out var arr))
            {
                arr = new int[RANK_COUNT];
                map[playerId] = arr;
            }
            return arr;
        }
    }

    public readonly struct ClericRecruitedEvent
    {
        public readonly int PlayerId;
        public readonly ClericRank Rank;
        public readonly int NewCount;

        public ClericRecruitedEvent(int playerId, ClericRank rank, int newCount)
        {
            PlayerId = playerId;
            Rank = rank;
            NewCount = newCount;
        }
    }
}
