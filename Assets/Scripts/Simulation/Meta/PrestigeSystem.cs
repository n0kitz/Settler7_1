using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Tracks prestige points, levels, and unlocks for each player.
    /// Prestige is earned by conquering sectors (+1 each).
    /// Every 5 points = 1 level. Levels unlock items from 3 branches.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class PrestigeSystem
    {
        private readonly Dictionary<int, int> _points = new();   // playerId → points
        private readonly Dictionary<int, int> _levels = new();   // playerId → level
        private readonly Dictionary<int, HashSet<string>> _unlocks = new(); // playerId → unlock IDs
        private readonly int _pointsPerLevel;
        private readonly EventBus _eventBus;

        public PrestigeSystem(int pointsPerLevel, EventBus eventBus)
        {
            _pointsPerLevel = pointsPerLevel;
            _eventBus = eventBus;
        }

        /// <summary>Get prestige points for a player.</summary>
        public int GetPoints(int playerId)
        {
            return _points.TryGetValue(playerId, out int pts) ? pts : 0;
        }

        /// <summary>Get prestige level for a player.</summary>
        public int GetLevel(int playerId)
        {
            return _levels.TryGetValue(playerId, out int lvl) ? lvl : 0;
        }

        /// <summary>Get number of unused level-ups (levels not yet spent on unlocks).</summary>
        public int GetUnspentLevels(int playerId)
        {
            int level = GetLevel(playerId);
            int unlockCount = _unlocks.TryGetValue(playerId, out var set) ? set.Count : 0;
            return level - unlockCount;
        }

        /// <summary>
        /// Award prestige points. Recalculates level.
        /// Returns the new total points.
        /// </summary>
        public int AwardPoints(int playerId, int amount)
        {
            if (!_points.ContainsKey(playerId))
            {
                _points[playerId] = 0;
                _levels[playerId] = 0;
            }

            _points[playerId] += amount;
            int newLevel = _points[playerId] / _pointsPerLevel;
            int oldLevel = _levels[playerId];
            _levels[playerId] = newLevel;

            if (newLevel > oldLevel)
            {
                _eventBus.Publish(new PrestigeLevelUpEvent(playerId, newLevel, newLevel - oldLevel));
            }

            return _points[playerId];
        }

        /// <summary>
        /// Try to unlock a prestige item. Costs 1 level.
        /// Returns true if successful.
        /// </summary>
        public bool TryUnlock(int playerId, string unlockId)
        {
            if (GetUnspentLevels(playerId) <= 0)
                return false;

            var unlock = PrestigeDatabase.Get(unlockId);
            if (unlock == null)
                return false;

            // Check prerequisites
            if (unlock.PrerequisiteId != null)
            {
                if (!HasUnlock(playerId, unlock.PrerequisiteId))
                    return false;
            }

            // Check minimum level
            if (GetLevel(playerId) < unlock.MinLevel)
                return false;

            if (!_unlocks.ContainsKey(playerId))
                _unlocks[playerId] = new HashSet<string>();

            if (!_unlocks[playerId].Add(unlockId))
                return false; // Already unlocked

            _eventBus.Publish(new PrestigeUnlockEvent(playerId, unlockId));
            return true;
        }

        /// <summary>Check if a player has a specific unlock.</summary>
        public bool HasUnlock(int playerId, string unlockId)
        {
            return _unlocks.TryGetValue(playerId, out var set) && set.Contains(unlockId);
        }

        /// <summary>Get all unlock IDs for a player.</summary>
        public IReadOnlyCollection<string> GetUnlocks(int playerId)
        {
            return _unlocks.TryGetValue(playerId, out var set)
                ? (IReadOnlyCollection<string>)set
                : System.Array.Empty<string>();
        }
    }

    /// <summary>Fired when a player gains a prestige level.</summary>
    public readonly struct PrestigeLevelUpEvent
    {
        public readonly int PlayerId;
        public readonly int NewLevel;
        public readonly int LevelsGained;

        public PrestigeLevelUpEvent(int playerId, int newLevel, int levelsGained)
        {
            PlayerId = playerId;
            NewLevel = newLevel;
            LevelsGained = levelsGained;
        }
    }

    /// <summary>Fired when a player unlocks a prestige item.</summary>
    public readonly struct PrestigeUnlockEvent
    {
        public readonly int PlayerId;
        public readonly string UnlockId;

        public PrestigeUnlockEvent(int playerId, string unlockId)
        {
            PlayerId = playerId;
            UnlockId = unlockId;
        }
    }
}
