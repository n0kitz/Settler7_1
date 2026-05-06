using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages technology research at monasteries.
    /// Only one player can research a tech at a time (blocking).
    /// Once complete, the tech is permanently claimed — first-come-first-served.
    /// Clerics (Novice/Brother/Father) are required to research.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class ResearchSystem
    {
        private readonly HashSet<string> _globallyResearched = new();
        private readonly Dictionary<int, HashSet<string>> _playerTechs = new();
        private readonly List<ResearchTask> _activeTasks = new();
        private readonly Dictionary<string, int> _blockedTechs = new(); // techId → playerId blocking it
        private readonly EventBus _eventBus;

        public ResearchSystem(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        /// <summary>Active research tasks.</summary>
        public IReadOnlyList<ResearchTask> ActiveTasks => _activeTasks;

        /// <summary>Check if a tech has been researched globally (by any player).</summary>
        public bool IsResearchedGlobally(string techId) =>
            _globallyResearched.Contains(techId);

        /// <summary>Check if a specific player has a tech.</summary>
        public bool HasTech(int playerId, string techId) =>
            _playerTechs.TryGetValue(playerId, out var set) && set.Contains(techId);

        /// <summary>Get all techs a player has researched.</summary>
        public IReadOnlyCollection<string> GetPlayerTechs(int playerId) =>
            _playerTechs.TryGetValue(playerId, out var set)
                ? (IReadOnlyCollection<string>)set
                : System.Array.Empty<string>();

        /// <summary>Get how many techs a player has.</summary>
        public int GetTechCount(int playerId) =>
            _playerTechs.TryGetValue(playerId, out var set) ? set.Count : 0;

        /// <summary>Check if a tech is currently being researched (blocked).</summary>
        public bool IsBlocked(string techId) =>
            _blockedTechs.ContainsKey(techId);

        /// <summary>
        /// Directly mark a tech as researched for a player (used when loading saves).
        /// Does not fire events or check prerequisites.
        /// </summary>
        public void RestoreTech(int playerId, string techId)
        {
            _globallyResearched.Add(techId);
            if (!_playerTechs.TryGetValue(playerId, out var set))
            {
                set = new HashSet<string>();
                _playerTechs[playerId] = set;
            }
            set.Add(techId);
        }

        /// <summary>
        /// Start researching a tech. Returns true if research started.
        /// Fails if: tech already researched, already being researched by someone else,
        /// prerequisite not met, or already owned.
        /// </summary>
        public bool StartResearch(int playerId, string techId)
        {
            if (_globallyResearched.Contains(techId))
                return false;

            if (_blockedTechs.ContainsKey(techId))
                return false; // Someone else is researching it

            var techDef = TechTree.Get(techId);
            if (techDef == null)
                return false;

            // Check prerequisite
            if (techDef.PrerequisiteId != null && !HasTech(playerId, techDef.PrerequisiteId))
                return false;

            // Already have it
            if (HasTech(playerId, techId))
                return false;

            _blockedTechs[techId] = playerId;
            _activeTasks.Add(new ResearchTask(playerId, techId, techDef.ResearchTime));
            _eventBus.Publish(new ResearchStartedEvent(playerId, techId));
            return true;
        }

        /// <summary>Cancel active research (frees the tech for others).</summary>
        public bool CancelResearch(int playerId, string techId)
        {
            if (!_blockedTechs.TryGetValue(techId, out int blockerId))
                return false;
            if (blockerId != playerId)
                return false;

            _blockedTechs.Remove(techId);
            _activeTasks.RemoveAll(t => t.PlayerId == playerId && t.TechId == techId);
            return true;
        }

        /// <summary>Tick all active research tasks.</summary>
        public void Tick(float deltaTime)
        {
            var completed = new List<ResearchTask>();

            for (int i = 0; i < _activeTasks.Count; i++)
            {
                var task = _activeTasks[i];
                task.Progress += deltaTime / task.TotalTime;
                if (task.Progress >= 1f)
                {
                    task.Progress = 1f;
                    completed.Add(task);
                }
            }

            foreach (var task in completed)
            {
                _activeTasks.Remove(task);
                _blockedTechs.Remove(task.TechId);
                _globallyResearched.Add(task.TechId);

                if (!_playerTechs.TryGetValue(task.PlayerId, out var set))
                {
                    set = new HashSet<string>();
                    _playerTechs[task.PlayerId] = set;
                }
                set.Add(task.TechId);

                _eventBus.Publish(new TechResearchedEvent(task.PlayerId, task.TechId));
            }
        }
    }

    public class ResearchTask
    {
        public int PlayerId;
        public string TechId;
        public float TotalTime;
        public float Progress;

        public ResearchTask(int playerId, string techId, float totalTime)
        {
            PlayerId = playerId;
            TechId = techId;
            TotalTime = totalTime;
            Progress = 0f;
        }
    }

    // --- Technology Events ---

    public readonly struct ResearchStartedEvent
    {
        public readonly int PlayerId;
        public readonly string TechId;

        public ResearchStartedEvent(int playerId, string techId)
        {
            PlayerId = playerId;
            TechId = techId;
        }
    }

    public readonly struct TechResearchedEvent
    {
        public readonly int PlayerId;
        public readonly string TechId;

        public TechResearchedEvent(int playerId, string techId)
        {
            PlayerId = playerId;
            TechId = techId;
        }
    }
}
