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

    /// <summary>A quest definition.</summary>
    public class Quest
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public int SectorId;  // -1 = no sector requirement
        public int AcceptedBy; // -1 = not accepted
        public List<QuestObjective> Objectives;
        public List<QuestReward> Rewards;

        public Quest(string id, string name, string desc, int sectorId)
        {
            Id = id;
            DisplayName = name;
            Description = desc;
            SectorId = sectorId;
            AcceptedBy = -1;
            Objectives = new List<QuestObjective>();
            Rewards = new List<QuestReward>();
        }
    }

    public enum QuestObjectiveType
    {
        DeliverResource,
        OwnSectors,
        HaveArmy,
        HavePrestigeLevel,
        ResearchTech
    }

    public class QuestObjective
    {
        public QuestObjectiveType Type;
        public ResourceType ResourceType;
        public int Amount;
        public string TechId;

        public static QuestObjective Deliver(ResourceType res, int amount) =>
            new() { Type = QuestObjectiveType.DeliverResource, ResourceType = res, Amount = amount };

        public static QuestObjective OwnSectors(int count) =>
            new() { Type = QuestObjectiveType.OwnSectors, Amount = count };

        public static QuestObjective Army(int size) =>
            new() { Type = QuestObjectiveType.HaveArmy, Amount = size };

        public static QuestObjective Prestige(int level) =>
            new() { Type = QuestObjectiveType.HavePrestigeLevel, Amount = level };

        public static QuestObjective Tech(string techId) =>
            new() { Type = QuestObjectiveType.ResearchTech, TechId = techId, Amount = 1 };
    }

    public enum QuestRewardType { Resource, PrestigePoints, VictoryPoint }

    public class QuestReward
    {
        public QuestRewardType Type;
        public ResourceType ResourceType;
        public int Amount;
        public string VPId;

        public static QuestReward Resource(ResourceType res, int amount) =>
            new() { Type = QuestRewardType.Resource, ResourceType = res, Amount = amount };

        public static QuestReward Prestige(int points) =>
            new() { Type = QuestRewardType.PrestigePoints, Amount = points };

        public static QuestReward VP(string vpId) =>
            new() { Type = QuestRewardType.VictoryPoint, VPId = vpId };
    }

    /// <summary>Predefined quests for the test maps.</summary>
    public static class QuestDatabase
    {
        public static List<Quest> GetQuestsForMap(string mapId)
        {
            var quests = new List<Quest>();
            AddUniversalQuests(quests);

            switch (mapId)
            {
                case "large_valley":
                    AddLargeValleyQuests(quests);
                    break;
                default:
                    AddTestValleyQuests(quests);
                    break;
            }

            return quests;
        }

        private static void AddUniversalQuests(List<Quest> quests)
        {
            var q1 = new Quest("quest_lumber_baron", "Lumber Baron",
                "Deliver 20 Planks to prove your logging prowess.", -1);
            q1.Objectives.Add(QuestObjective.Deliver(ResourceType.Planks, 20));
            q1.Rewards.Add(QuestReward.Prestige(3));
            q1.Rewards.Add(QuestReward.Resource(ResourceType.Tools, 5));
            quests.Add(q1);

            var q2 = new Quest("quest_iron_will", "Iron Will",
                "Deliver 10 Iron Bars to the forge.", -1);
            q2.Objectives.Add(QuestObjective.Deliver(ResourceType.IronBars, 10));
            q2.Rewards.Add(QuestReward.Resource(ResourceType.Weapons, 5));
            q2.Rewards.Add(QuestReward.Prestige(2));
            quests.Add(q2);

            var q3 = new Quest("quest_expansionist", "Expansionist",
                "Control 4 sectors to demonstrate your reach.", -1);
            q3.Objectives.Add(QuestObjective.OwnSectors(4));
            q3.Rewards.Add(QuestReward.VP("vp_quest_expansionist"));
            quests.Add(q3);

            var q4 = new Quest("quest_scholar", "Scholar",
                "Research any technology.", -1);
            q4.Objectives.Add(QuestObjective.Tech("tech_plowing"));
            q4.Rewards.Add(QuestReward.Prestige(2));
            q4.Rewards.Add(QuestReward.Resource(ResourceType.Books, 3));
            quests.Add(q4);
        }

        private static void AddTestValleyQuests(List<Quest> quests)
        {
            var q5 = new Quest("quest_bread_basket", "Bread Basket",
                "Deliver 15 Bread to feed the hungry.", -1);
            q5.Objectives.Add(QuestObjective.Deliver(ResourceType.Bread, 15));
            q5.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 20));
            quests.Add(q5);

            var q6 = new Quest("quest_warlord", "Warlord",
                "Raise an army of 15 soldiers.", -1);
            q6.Objectives.Add(QuestObjective.Army(15));
            q6.Rewards.Add(QuestReward.VP("vp_quest_warlord"));
            quests.Add(q6);
        }

        private static void AddLargeValleyQuests(List<Quest> quests)
        {
            // Map-specific quests tied to sectors
            var q1 = new Quest("quest_gold_rush", "Gold Rush",
                "Deliver 10 Gold Ore from the Dragon's Peak mines.", 11);
            q1.Objectives.Add(QuestObjective.Deliver(ResourceType.GoldOre, 10));
            q1.Rewards.Add(QuestReward.VP("vp_quest_gold_rush"));
            q1.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 30));
            quests.Add(q1);

            var q2 = new Quest("quest_iron_lord", "Iron Lord",
                "Control the Iron Hills and deliver 15 Iron Bars.", 4);
            q2.Objectives.Add(QuestObjective.Deliver(ResourceType.IronBars, 15));
            q2.Rewards.Add(QuestReward.Prestige(5));
            q2.Rewards.Add(QuestReward.Resource(ResourceType.Weapons, 8));
            quests.Add(q2);

            var q3 = new Quest("quest_breadbasket_lv", "Breadbasket of the Valley",
                "Deliver 20 Bread from the Southern Farmlands.", 8);
            q3.Objectives.Add(QuestObjective.Deliver(ResourceType.Bread, 20));
            q3.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 25));
            q3.Rewards.Add(QuestReward.Prestige(3));
            quests.Add(q3);

            var q4 = new Quest("quest_conqueror", "Conqueror",
                "Control 6 sectors to dominate the valley.", -1);
            q4.Objectives.Add(QuestObjective.OwnSectors(6));
            q4.Rewards.Add(QuestReward.VP("vp_quest_conqueror"));
            quests.Add(q4);

            var q5 = new Quest("quest_master_trader", "Master Trader",
                "Deliver 20 Coins to prove your trade mastery.", -1);
            q5.Objectives.Add(QuestObjective.Deliver(ResourceType.Coins, 20));
            q5.Rewards.Add(QuestReward.Prestige(4));
            q5.Rewards.Add(QuestReward.Resource(ResourceType.Jewelry, 5));
            quests.Add(q5);

            var q6 = new Quest("quest_grand_army", "Grand Army",
                "Raise an army of 25 soldiers.", -1);
            q6.Objectives.Add(QuestObjective.Army(25));
            q6.Rewards.Add(QuestReward.VP("vp_quest_grand_army"));
            quests.Add(q6);
        }
    }

    // --- Quest Events ---

    public readonly struct QuestAcceptedEvent
    {
        public readonly int PlayerId;
        public readonly string QuestId;
        public QuestAcceptedEvent(int playerId, string questId)
        { PlayerId = playerId; QuestId = questId; }
    }

    public readonly struct QuestCompletedEvent
    {
        public readonly int PlayerId;
        public readonly string QuestId;
        public QuestCompletedEvent(int playerId, string questId)
        { PlayerId = playerId; QuestId = questId; }
    }
}
