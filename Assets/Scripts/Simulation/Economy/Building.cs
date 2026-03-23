using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Simulation data for a placed building instance.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class Building
    {
        private static int _nextId;
        private readonly List<WorkYard> _workYards = new();

        /// <summary>Unique building instance ID.</summary>
        public int Id { get; }

        /// <summary>The base building type (Lodge, Farm, etc.).</summary>
        public BaseBuildingType Type { get; }

        /// <summary>Which sector this building is in.</summary>
        public int SectorId { get; }

        /// <summary>Player who owns this building.</summary>
        public int OwnerId { get; }

        /// <summary>Current construction state.</summary>
        public BuildingState State { get; private set; }

        /// <summary>Construction progress (0.0 to 1.0).</summary>
        public float ConstructionProgress { get; private set; }

        /// <summary>Current upgrade level (0 = base, 1 = first upgrade, etc.).</summary>
        public int UpgradeLevel { get; private set; }

        /// <summary>Maximum work yards allowed (default 3, from GameConstants).</summary>
        public int MaxWorkYards { get; }

        /// <summary>Food setting for all work yards in this building.</summary>
        public FoodSetting FoodSetting { get; private set; }

        /// <summary>Local X/Z position within the sector for visual placement.</summary>
        public float LocalX { get; }
        public float LocalZ { get; }

        /// <summary>Attached work yards.</summary>
        public IReadOnlyList<WorkYard> WorkYards => _workYards;

        public Building(BaseBuildingType type, int sectorId, int ownerId,
            int maxWorkYards, float localX, float localZ)
        {
            Id = _nextId++;
            Type = type;
            SectorId = sectorId;
            OwnerId = ownerId;
            State = BuildingState.Planned;
            ConstructionProgress = 0f;
            UpgradeLevel = 0;
            MaxWorkYards = maxWorkYards;
            FoodSetting = FoodSetting.None;
            LocalX = localX;
            LocalZ = localZ;
        }

        /// <summary>Advance construction progress. Returns true if just completed.</summary>
        public bool AdvanceConstruction(float amount)
        {
            if (State != BuildingState.Planned && State != BuildingState.UnderConstruction)
                return false;

            State = BuildingState.UnderConstruction;
            ConstructionProgress += amount;

            if (ConstructionProgress >= 1f)
            {
                ConstructionProgress = 1f;
                State = BuildingState.Complete;
                return true;
            }
            return false;
        }

        /// <summary>Maximum upgrade level for this building type.</summary>
        public int MaxUpgradeLevel => Type switch
        {
            BaseBuildingType.Residence => 2,
            BaseBuildingType.NobleResidence => 2,
            _ => 0
        };

        /// <summary>Can this building be upgraded further?</summary>
        public bool CanUpgrade => IsOperational && UpgradeLevel < MaxUpgradeLevel;

        /// <summary>Returns true if the building is operational (complete, not upgrading).</summary>
        public bool IsOperational => State == BuildingState.Complete;

        /// <summary>Returns true if another work yard can be attached.</summary>
        public bool CanAttachWorkYard => _workYards.Count < MaxWorkYards && IsOperational;

        /// <summary>Attach a work yard to this building.</summary>
        public bool AttachWorkYard(WorkYard workYard)
        {
            if (!CanAttachWorkYard)
                return false;

            _workYards.Add(workYard);
            return true;
        }

        /// <summary>Set the food setting for this building's production.</summary>
        public void SetFoodSetting(FoodSetting setting)
        {
            FoodSetting = setting;
        }

        /// <summary>Start upgrading this building. Resets progress and sets state to Upgrading.</summary>
        public bool StartUpgrade()
        {
            if (!CanUpgrade) return false;
            State = BuildingState.Upgrading;
            ConstructionProgress = 0f;
            return true;
        }

        /// <summary>Advance upgrade progress. Returns true if upgrade just completed.</summary>
        public bool AdvanceUpgrade(float amount)
        {
            if (State != BuildingState.Upgrading) return false;
            ConstructionProgress += amount;
            if (ConstructionProgress >= 1f)
            {
                ConstructionProgress = 1f;
                UpgradeLevel++;
                State = BuildingState.Complete;
                return true;
            }
            return false;
        }

        /// <summary>Get the base population this building provides (including upgrades).</summary>
        public int GetBasePopulation()
        {
            int basePop = Type switch
            {
                BaseBuildingType.Lodge => 1,
                BaseBuildingType.Farm => 1,
                BaseBuildingType.MountainShelter => 1,
                BaseBuildingType.Residence => 4,
                BaseBuildingType.NobleResidence => 5,
                _ => 0
            };
            int upgradeBonus = Type switch
            {
                BaseBuildingType.Residence => 4 * UpgradeLevel,
                BaseBuildingType.NobleResidence => 5 * UpgradeLevel,
                _ => 0
            };
            return basePop + upgradeBonus;
        }

        /// <summary>Reset ID counter (for tests).</summary>
        public static void ResetIdCounter() => _nextId = 0;
    }
}
