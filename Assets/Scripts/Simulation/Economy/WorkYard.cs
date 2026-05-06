namespace Settlers.Simulation
{
    /// <summary>
    /// Simulation data for a work yard attached to a building.
    /// Each work yard needs 1 settler + 1 tool to operate.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class WorkYard
    {
        private static int _nextId;

        /// <summary>Unique work yard instance ID.</summary>
        public int Id { get; }

        /// <summary>Definition ID (e.g. "forester", "sawmill") matching WorkYardDefinition.workYardId.</summary>
        public string TypeId { get; }

        /// <summary>The building this work yard is attached to.</summary>
        public int BuildingId { get; }

        /// <summary>The sector this work yard is in.</summary>
        public int SectorId { get; }

        /// <summary>Player who owns this work yard.</summary>
        public int OwnerId { get; }

        /// <summary>Whether a worker (settler) has been assigned.</summary>
        public bool HasWorker { get; private set; }

        /// <summary>Whether a tool has been consumed for this work yard.</summary>
        public bool HasTool { get; private set; }

        /// <summary>Current production cycle progress (0.0 to 1.0).</summary>
        public float CycleProgress { get; private set; }

        /// <summary>Whether this work yard has reserved its recipe inputs for the current cycle.</summary>
        public bool InputsReserved { get; set; }

        /// <summary>Required resource node type (None if no special requirement).</summary>
        public ResourceNodeType RequiredResourceNode { get; }

        /// <summary>Local X/Z offset from parent building for visual placement.</summary>
        public float LocalX { get; }
        public float LocalZ { get; }

        public WorkYard(string typeId, int buildingId, int sectorId, int ownerId,
            ResourceNodeType requiredResourceNode, float localX, float localZ)
        {
            Id = _nextId++;
            TypeId = typeId;
            BuildingId = buildingId;
            SectorId = sectorId;
            OwnerId = ownerId;
            RequiredResourceNode = requiredResourceNode;
            LocalX = localX;
            LocalZ = localZ;
        }

        /// <summary>Returns true if the work yard is fully staffed and equipped.</summary>
        public bool IsOperational => HasWorker && HasTool;

        /// <summary>Assign a worker to this work yard.</summary>
        public void AssignWorker() => HasWorker = true;

        /// <summary>Remove the worker from this work yard.</summary>
        public void RemoveWorker() => HasWorker = false;

        /// <summary>Provide a tool for this work yard.</summary>
        public void ProvideTool() => HasTool = true;

        /// <summary>Advance production cycle. Returns true if cycle completed.</summary>
        public bool AdvanceCycle(float amount)
        {
            if (!IsOperational)
                return false;

            CycleProgress += amount;
            if (CycleProgress >= 1f)
            {
                CycleProgress -= 1f;
                return true;
            }
            return false;
        }

        /// <summary>Reset cycle progress (e.g. when food runs out).</summary>
        public void ResetCycle()
        {
            CycleProgress = 0f;
            InputsReserved = false;
        }

        /// <summary>Restore work yard state from a save file.</summary>
        public void RestoreState(bool hasWorker, bool hasTool, float cycleProgress)
        {
            HasWorker = hasWorker;
            HasTool = hasTool;
            CycleProgress = cycleProgress;
        }

        /// <summary>Reset ID counter (for tests).</summary>
        public static void ResetIdCounter() => _nextId = 0;
    }
}
