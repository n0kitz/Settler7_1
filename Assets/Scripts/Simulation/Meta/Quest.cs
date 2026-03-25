using System.Collections.Generic;

namespace Settlers.Simulation
{
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
}
