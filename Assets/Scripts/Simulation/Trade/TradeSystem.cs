using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages trade operations: sending traders to outposts, claiming,
    /// and executing exchanges.
    /// Trader types: Hawker (Tier 1), Salesman (Tier 2), Merchant (Tier 3).
    /// Higher tiers can trade at better rates or access further outposts.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class TradeSystem
    {
        private readonly TradeMap _tradeMap;
        private readonly Dictionary<int, PlayerResources> _resources;
        private readonly PrestigeSystem _prestige;
        private readonly EventBus _eventBus;
        private readonly List<TraderTask> _activeTasks = new();

        public TradeSystem(TradeMap tradeMap,
            Dictionary<int, PlayerResources> resources,
            PrestigeSystem prestige, EventBus eventBus)
        {
            _tradeMap = tradeMap;
            _resources = resources;
            _prestige = prestige;
            _eventBus = eventBus;
        }

        /// <summary>Active trader tasks (claiming + trading).</summary>
        public IReadOnlyList<TraderTask> ActiveTasks => _activeTasks;

        /// <summary>The underlying trade map.</summary>
        public TradeMap Map => _tradeMap;

        /// <summary>
        /// Send a trader to claim an outpost. Returns true if task started.
        /// </summary>
        public bool SendTrader(int playerId, string outpostId)
        {
            var outpost = _tradeMap.GetOutpost(outpostId);
            if (outpost == null || outpost.IsClaimed)
                return false;

            // Need Export Office prestige unlock
            if (!_prestige.HasUnlock(playerId, "cul_export_office"))
                return false;

            float travelTime = 15f; // Base travel time
            _activeTasks.Add(new TraderTask(playerId, outpostId,
                TraderTaskType.Claim, travelTime));
            return true;
        }

        /// <summary>
        /// Execute a trade at a claimed outpost. Spends input, grants output.
        /// </summary>
        public bool ExecuteTrade(int playerId, string outpostId)
        {
            var outpost = _tradeMap.GetOutpost(outpostId);
            if (outpost == null || outpost.ClaimedBy != playerId)
                return false;

            if (!_resources.TryGetValue(playerId, out var res))
                return false;

            if (!res.Has(outpost.InputResource, outpost.InputAmount))
                return false;

            res.TrySpend(outpost.InputResource, outpost.InputAmount);
            res.Add(outpost.OutputResource, outpost.OutputAmount);

            _eventBus.Publish(new TradeExecutedEvent(
                playerId, outpostId,
                outpost.InputResource, outpost.InputAmount,
                outpost.OutputResource, outpost.OutputAmount));
            return true;
        }

        /// <summary>Tick active trader tasks.</summary>
        public void Tick(float deltaTime)
        {
            var completed = new List<TraderTask>();

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

                if (task.TaskType == TraderTaskType.Claim)
                {
                    var outpost = _tradeMap.GetOutpost(task.OutpostId);
                    if (outpost != null && outpost.TryClaim(task.PlayerId))
                    {
                        _eventBus.Publish(new OutpostClaimedEvent(
                            task.PlayerId, task.OutpostId, outpost.IsSpecial));
                    }
                }
            }
        }
    }

    public enum TraderTaskType { Claim, Trade }

    public class TraderTask
    {
        public int PlayerId;
        public string OutpostId;
        public TraderTaskType TaskType;
        public float TotalTime;
        public float Progress;

        public TraderTask(int playerId, string outpostId,
            TraderTaskType taskType, float totalTime)
        {
            PlayerId = playerId;
            OutpostId = outpostId;
            TaskType = taskType;
            TotalTime = totalTime;
            Progress = 0f;
        }
    }

    // --- Trade Events ---

    public readonly struct OutpostClaimedEvent
    {
        public readonly int PlayerId;
        public readonly string OutpostId;
        public readonly bool IsSpecial;

        public OutpostClaimedEvent(int playerId, string outpostId, bool isSpecial)
        {
            PlayerId = playerId;
            OutpostId = outpostId;
            IsSpecial = isSpecial;
        }
    }

    public readonly struct TradeExecutedEvent
    {
        public readonly int PlayerId;
        public readonly string OutpostId;
        public readonly ResourceType InputResource;
        public readonly int InputAmount;
        public readonly ResourceType OutputResource;
        public readonly int OutputAmount;

        public TradeExecutedEvent(int playerId, string outpostId,
            ResourceType inputRes, int inputAmt,
            ResourceType outputRes, int outputAmt)
        {
            PlayerId = playerId;
            OutpostId = outpostId;
            InputResource = inputRes;
            InputAmount = inputAmt;
            OutputResource = outputRes;
            OutputAmount = outputAmt;
        }
    }
}
