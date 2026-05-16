using System;
using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Tracks achievement conditions by subscribing to EventBus events.
    /// Call Initialize(bus) once per game session.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class AchievementSystem
    {
        private readonly List<Achievement> _achievements = new List<Achievement>();
        private readonly Dictionary<AchievementConditionType, int> _counters =
            new Dictionary<AchievementConditionType, int>();
        private EventBus _events;

        public IReadOnlyList<Achievement> All => _achievements;

        public AchievementSystem()
        {
            BuildDefinitions();
            foreach (AchievementConditionType ct in
                Enum.GetValues(typeof(AchievementConditionType)))
                _counters[ct] = 0;

            // Restore previously unlocked achievements from disk
            foreach (var ach in _achievements)
                if (AchievementProgress.IsUnlocked(ach.Id))
                {
                    ach.IsUnlocked = true;
                    ach.UnlockedAt = AchievementProgress.Unlocked[ach.Id];
                }
        }

        /// <summary>Subscribe to the given event bus. Call after each new game starts.</summary>
        public void Initialize(EventBus bus)
        {
            _events = bus;
            bus.Subscribe<BuildingCompletedEvent>(_ =>
                Increment(AchievementConditionType.BuildingCompleted));
            bus.Subscribe<SectorConqueredEvent>(e =>
                { if (e.NewOwnerId == 0) Increment(AchievementConditionType.SectorConquered); });
            bus.Subscribe<TechResearchedEvent>(e =>
                { if (e.PlayerId == 0) Increment(AchievementConditionType.TechResearched); });
            bus.Subscribe<VPChangedEvent>(e =>
                { if (e.PlayerId == 0 && e.Gained) Increment(AchievementConditionType.VPGained); });
            bus.Subscribe<PrestigeLevelUpEvent>(e =>
                { if (e.PlayerId == 0) IncrementTo(AchievementConditionType.PrestigeLevelReached, e.NewLevel); });
            bus.Subscribe<OutpostClaimedEvent>(e =>
                { if (e.PlayerId == 0) Increment(AchievementConditionType.OutpostClaimed); });
            bus.Subscribe<TradeExecutedEvent>(e =>
                { if (e.PlayerId == 0) Increment(AchievementConditionType.TradeExecuted); });
            bus.Subscribe<GeneralHiredEvent>(e =>
                { if (e.PlayerId == 0) Increment(AchievementConditionType.GeneralHired); });
            bus.Subscribe<QuestCompletedEvent>(e =>
                { if (e.PlayerId == 0) Increment(AchievementConditionType.QuestCompleted); });
        }

        /// <summary>Call when the player completes a tutorial session.</summary>
        public void NotifyTutorialCompleted() =>
            Increment(AchievementConditionType.TutorialCompleted);

        /// <summary>Call when the player finishes a campaign mission.</summary>
        public void NotifyCampaignMissionCompleted() =>
            Increment(AchievementConditionType.CampaignMissionCompleted);

        private void Increment(AchievementConditionType type)
        {
            _counters[type]++;
            CheckUnlocks(type);
        }

        private void IncrementTo(AchievementConditionType type, int value)
        {
            if (value > _counters[type]) _counters[type] = value;
            CheckUnlocks(type);
        }

        private void CheckUnlocks(AchievementConditionType type)
        {
            foreach (var ach in _achievements)
                if (!ach.IsUnlocked && ach.Condition == type &&
                    _counters[type] >= ach.Threshold)
                    Unlock(ach);
        }

        private void Unlock(Achievement ach)
        {
            ach.IsUnlocked = true;
            ach.UnlockedAt = DateTime.UtcNow;
            AchievementProgress.MarkUnlocked(ach.Id);
            _events?.Publish(new AchievementUnlockedEvent(ach.Id, ach.Name));
        }

        private void BuildDefinitions()
        {
            Add("first_building",   "Builder",          "Complete your first building",
                AchievementConditionType.BuildingCompleted, 1);
            Add("ten_buildings",    "Constructor",      "Complete 10 buildings",
                AchievementConditionType.BuildingCompleted, 10);
            Add("first_conquest",   "Conqueror",        "Conquer your first sector",
                AchievementConditionType.SectorConquered, 1);
            Add("five_conquests",   "Warlord",          "Conquer 5 sectors",
                AchievementConditionType.SectorConquered, 5);
            Add("first_tech",       "Scholar",          "Research your first technology",
                AchievementConditionType.TechResearched, 1);
            Add("five_techs",       "Academician",      "Research 5 technologies",
                AchievementConditionType.TechResearched, 5);
            Add("first_vp",         "Point Scorer",     "Earn your first victory point",
                AchievementConditionType.VPGained, 1);
            Add("three_vps",        "Royal Candidate",  "Hold 3 victory points at once",
                AchievementConditionType.VPGained, 3);
            Add("prestige_5",       "Noble",            "Reach prestige level 5",
                AchievementConditionType.PrestigeLevelReached, 5);
            Add("first_outpost",    "Merchant Prince",  "Claim your first trade outpost",
                AchievementConditionType.OutpostClaimed, 1);
            Add("first_trade",      "Trader",           "Complete your first trade",
                AchievementConditionType.TradeExecuted, 1);
            Add("ten_trades",       "Mercator",         "Complete 10 trades",
                AchievementConditionType.TradeExecuted, 10);
            Add("first_general",    "Commander",        "Hire your first general",
                AchievementConditionType.GeneralHired, 1);
            Add("tutorial_done",    "Apprentice",       "Complete the tutorial",
                AchievementConditionType.TutorialCompleted, 1);
            Add("campaign_ch1",     "Campaign Hero",    "Complete a campaign mission",
                AchievementConditionType.CampaignMissionCompleted, 1);
        }

        private void Add(string id, string name, string desc,
            AchievementConditionType cond, int threshold)
            => _achievements.Add(new Achievement(id, name, desc, cond, threshold));
    }
}
