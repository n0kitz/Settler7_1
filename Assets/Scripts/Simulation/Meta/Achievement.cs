using System;

namespace Settlers.Simulation
{
    /// <summary>Describes a single achievement and its unlock state.</summary>
    public sealed class Achievement
    {
        public string Id;
        public string Name;
        public string Description;
        public AchievementConditionType Condition;
        public int Threshold;        // how many events needed (1 = first-time)
        public bool IsUnlocked;
        public DateTime? UnlockedAt;

        public Achievement(string id, string name, string desc,
            AchievementConditionType cond, int threshold = 1)
        {
            Id = id;
            Name = name;
            Description = desc;
            Condition = cond;
            Threshold = threshold;
        }
    }

    /// <summary>Fired when the player earns a new achievement.</summary>
    public readonly struct AchievementUnlockedEvent
    {
        public readonly string Id;
        public readonly string Name;

        public AchievementUnlockedEvent(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
