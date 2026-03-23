using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>A general leading an army.</summary>
    public class General
    {
        public int Id { get; }
        public int OwnerId { get; }
        public int SectorId { get; set; }
        public int MaxSoldiers { get; }
        public bool IsMoving { get; set; }

        private readonly Dictionary<UnitType, int> _units = new();

        public General(int id, int ownerId, int sectorId, int maxSoldiers)
        {
            Id = id;
            OwnerId = ownerId;
            SectorId = sectorId;
            MaxSoldiers = maxSoldiers;
        }

        public int TotalSoldiers
        {
            get
            {
                int total = 0;
                foreach (var kvp in _units) total += kvp.Value;
                return total;
            }
        }

        public int GetUnitCount(UnitType type) =>
            _units.TryGetValue(type, out int count) ? count : 0;

        public void AddUnit(UnitType type)
        {
            if (!_units.ContainsKey(type)) _units[type] = 0;
            _units[type]++;
        }

        public bool RemoveUnit(UnitType type)
        {
            if (!_units.TryGetValue(type, out int count) || count <= 0)
                return false;
            _units[type]--;
            if (_units[type] <= 0) _units.Remove(type);
            return true;
        }

        /// <summary>Check if this army has units that can breach fortifications.</summary>
        public bool CanBreachFortification()
        {
            foreach (var kvp in _units)
            {
                if (kvp.Value > 0 && UnitStats.CanBreachFortification(kvp.Key))
                    return true;
            }
            return false;
        }

        /// <summary>Total attack power of this army.</summary>
        public int TotalAttack
        {
            get
            {
                int total = 0;
                bool hasStandardBearer = GetUnitCount(UnitType.StandardBearer) > 0;
                foreach (var kvp in _units)
                    total += UnitStats.GetAttack(kvp.Key) * kvp.Value;
                if (hasStandardBearer) total = (int)(total * 1.15f);
                return total;
            }
        }

        /// <summary>Total defense power of this army.</summary>
        public int TotalDefense
        {
            get
            {
                int total = 0;
                foreach (var kvp in _units)
                    total += UnitStats.GetDefense(kvp.Key) * kvp.Value;
                return total;
            }
        }

        public IReadOnlyDictionary<UnitType, int> Units => _units;
    }

    public class TrainingTask
    {
        public int PlayerId;
        public int SectorId;
        public UnitType UnitType;
        public float TotalTime;
        public float Progress;

        public TrainingTask(int playerId, int sectorId, UnitType unitType, float totalTime)
        {
            PlayerId = playerId;
            SectorId = sectorId;
            UnitType = unitType;
            TotalTime = totalTime;
            Progress = 0f;
        }
    }

    public class ArmyMovement
    {
        public General General;
        public int TargetSectorId;
        public float TotalTime;
        public float Progress;
        public System.Collections.Generic.List<int> Path;

        public ArmyMovement(General general, int target, float totalTime, List<int> path)
        {
            General = general;
            TargetSectorId = target;
            TotalTime = totalTime;
            Progress = 0f;
            Path = path;
        }
    }

    // --- Military Events ---

    public readonly struct GeneralHiredEvent
    {
        public readonly int PlayerId;
        public readonly int GeneralId;
        public readonly int SectorId;

        public GeneralHiredEvent(int playerId, int generalId, int sectorId)
        {
            PlayerId = playerId;
            GeneralId = generalId;
            SectorId = sectorId;
        }
    }

    public readonly struct UnitTrainedEvent
    {
        public readonly int PlayerId;
        public readonly int SectorId;
        public readonly UnitType UnitType;

        public UnitTrainedEvent(int playerId, int sectorId, UnitType unitType)
        {
            PlayerId = playerId;
            SectorId = sectorId;
            UnitType = unitType;
        }
    }

    public readonly struct ArmyArrivedEvent
    {
        public readonly int PlayerId;
        public readonly int GeneralId;
        public readonly int SectorId;

        public ArmyArrivedEvent(int playerId, int generalId, int sectorId)
        {
            PlayerId = playerId;
            GeneralId = generalId;
            SectorId = sectorId;
        }
    }
}
