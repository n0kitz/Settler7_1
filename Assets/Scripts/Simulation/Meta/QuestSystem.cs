using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages quests found at event locations in sectors.
    /// Each quest has objectives and rewards (resources, VPs, prestige).
    /// Quests are per-map and can only be completed once.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class QuestSystem
    {
        private readonly GameState _state;
        private readonly EventBus _eventBus;
        private readonly List<Quest> _availableQuests = new();
        private readonly HashSet<string> _completedQuests = new();
        private readonly Dictionary<string, Quest> _activeQuests = new(); // questId → active

        public QuestSystem(GameState state)
        {
            _state = state;
            _eventBus = state.Events;
        }

        /// <summary>Available (not yet accepted or completed) quests.</summary>
        public IReadOnlyList<Quest> AvailableQuests => _availableQuests;

        /// <summary>Get active quests for a player.</summary>
        public List<Quest> GetActiveQuests(int playerId)
        {
            var result = new List<Quest>();
            foreach (var kvp in _activeQuests)
                if (kvp.Value.AcceptedBy == playerId) result.Add(kvp.Value);
            return result;
        }

        /// <summary>Check if a quest is completed.</summary>
        public bool IsCompleted(string questId) => _completedQuests.Contains(questId);

        /// <summary>Register a quest as available.</summary>
        public void AddQuest(Quest quest)
        {
            _availableQuests.Add(quest);
        }

        /// <summary>Accept a quest (player must own the sector it's in).</summary>
        public bool AcceptQuest(int playerId, string questId)
        {
            Quest quest = null;
            for (int i = 0; i < _availableQuests.Count; i++)
            {
                if (_availableQuests[i].Id == questId)
                {
                    quest = _availableQuests[i];
                    break;
                }
            }
            if (quest == null) return false;
            if (_completedQuests.Contains(questId)) return false;
            if (_activeQuests.ContainsKey(questId)) return false;

            // Player must own the sector
            if (quest.SectorId >= 0)
            {
                var sector = _state.Graph.GetSector(quest.SectorId);
                if (sector.OwnerId != playerId) return false;
            }

            quest.AcceptedBy = playerId;
            _availableQuests.Remove(quest);
            _activeQuests[questId] = quest;
            _eventBus.Publish(new QuestAcceptedEvent(playerId, questId));
            return true;
        }

        /// <summary>
        /// Try to complete an active quest. Checks all objectives.
        /// Awards rewards on success.
        /// </summary>
        public bool TryCompleteQuest(int playerId, string questId)
        {
            if (!_activeQuests.TryGetValue(questId, out var quest))
                return false;
            if (quest.AcceptedBy != playerId)
                return false;

            // Check all objectives
            foreach (var obj in quest.Objectives)
            {
                if (!CheckObjective(playerId, obj))
                    return false;
            }

            // Consume objective resources
            foreach (var obj in quest.Objectives)
            {
                if (obj.Type == QuestObjectiveType.DeliverResource)
                {
                    var res = _state.PlayerResources[playerId];
                    res.TrySpend(obj.ResourceType, obj.Amount);
                }
            }

            // Award rewards
            foreach (var reward in quest.Rewards)
                AwardReward(playerId, reward);

            _activeQuests.Remove(questId);
            _completedQuests.Add(questId);
            _eventBus.Publish(new QuestCompletedEvent(playerId, questId));
            return true;
        }

        private bool CheckObjective(int playerId, QuestObjective obj)
        {
            switch (obj.Type)
            {
                case QuestObjectiveType.DeliverResource:
                    return _state.PlayerResources[playerId].Has(obj.ResourceType, obj.Amount);

                case QuestObjectiveType.OwnSectors:
                    return _state.Graph.GetSectorsOwnedBy(playerId).Count >= obj.Amount;

                case QuestObjectiveType.HaveArmy:
                    return _state.Army.GetTotalArmySize(playerId) >= obj.Amount;

                case QuestObjectiveType.HavePrestigeLevel:
                    return _state.Prestige.GetLevel(playerId) >= obj.Amount;

                case QuestObjectiveType.ResearchTech:
                    return obj.TechId != null && _state.Research.HasTech(playerId, obj.TechId);

                default:
                    return false;
            }
        }

        private void AwardReward(int playerId, QuestReward reward)
        {
            switch (reward.Type)
            {
                case QuestRewardType.Resource:
                    _state.PlayerResources[playerId].Add(reward.ResourceType, reward.Amount);
                    break;
                case QuestRewardType.PrestigePoints:
                    _state.Prestige.AwardPoints(playerId, reward.Amount);
                    break;
                case QuestRewardType.VictoryPoint:
                    _state.Victory.AwardPermanentVP(playerId, reward.VPId);
                    break;
            }
        }
    }
}
