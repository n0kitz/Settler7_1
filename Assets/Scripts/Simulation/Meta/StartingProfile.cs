using System.Collections.Generic;

namespace Settlers.Simulation
{
    public enum StartingProfileType { Default, Rich, Lean }

    /// <summary>
    /// Named preset of starting resources given to every player at game start.
    /// Lean reduces the initial stockpile; Rich provides a comfortable head-start.
    /// </summary>
    public sealed class StartingProfile
    {
        public readonly StartingProfileType Type;
        public readonly IReadOnlyDictionary<ResourceType, int> Resources;

        private StartingProfile(StartingProfileType type,
            Dictionary<ResourceType, int> resources)
        {
            Type = type;
            Resources = resources;
        }

        public static readonly StartingProfile Default = new StartingProfile(
            StartingProfileType.Default, new Dictionary<ResourceType, int>
            {
                { ResourceType.Planks, 20 },
                { ResourceType.Stone,  10 },
                { ResourceType.Tools,   5 },
            });

        public static readonly StartingProfile Rich = new StartingProfile(
            StartingProfileType.Rich, new Dictionary<ResourceType, int>
            {
                { ResourceType.Planks, 40 },
                { ResourceType.Stone,  25 },
                { ResourceType.Tools,  12 },
                { ResourceType.Coins,  10 },
                { ResourceType.Wood,   15 },
            });

        public static readonly StartingProfile Lean = new StartingProfile(
            StartingProfileType.Lean, new Dictionary<ResourceType, int>
            {
                { ResourceType.Planks, 10 },
                { ResourceType.Stone,   5 },
                { ResourceType.Tools,   2 },
            });

        public static StartingProfile Get(StartingProfileType type) => type switch
        {
            StartingProfileType.Rich => Rich,
            StartingProfileType.Lean => Lean,
            _                        => Default
        };
    }
}
