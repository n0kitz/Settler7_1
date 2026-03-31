using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Static database of VP display names and descriptions.
    /// Used by UI to show what each VP means and its threshold.
    /// </summary>
    public static class VPDatabase
    {
        public class VPDef
        {
            public string Id;
            public string DisplayName;
            public string Description;
            public bool IsDynamic;

            public VPDef(string id, string name, string desc, bool isDynamic)
            {
                Id = id;
                DisplayName = name;
                Description = desc;
                IsDynamic = isDynamic;
            }
        }

        private static Dictionary<string, VPDef> _byId;
        private static List<VPDef> _all;

        public static IReadOnlyList<VPDef> All
        {
            get
            {
                if (_all == null) Build();
                return _all;
            }
        }

        public static VPDef Get(string id)
        {
            if (_byId == null) Build();
            return _byId.TryGetValue(id, out var def) ? def : null;
        }

        private static void Build()
        {
            _all = new List<VPDef>();
            _byId = new Dictionary<string, VPDef>();

            // Dynamic VPs (can be stolen)
            Add("vp_field_marshal", "Field Marshal", "Largest army", true);
            Add("vp_metropolis", "Metropolis", "Most employed workers", true);
            Add("vp_emperor", "Emperor", "Most sectors controlled", true);
            Add("vp_banker", "Banker", "Largest coin reserve", true);
            Add("vp_sun_king", "Sun King", "Highest prestige level", true);
            Add("vp_trading_company", "Trading Company", "Most trade outposts", true);
            Add("vp_fountain", "Fountain of Knowledge", "Most technologies", true);
            Add("vp_pacifist", "Pacifist", "No attacks for 10 minutes", true);
            Add("vp_economist", "Economist", "High work yard staffing", true);
            Add("vp_generalissimo", "Generalissimo", "Most enemy kills", true);

            // Permanent VPs
            Add("vp_genius", "Genius", "First to complete all Tier 3 technologies", false);
        }

        private static void Add(string id, string name, string desc, bool isDynamic)
        {
            var def = new VPDef(id, name, desc, isDynamic);
            _all.Add(def);
            _byId[id] = def;
        }
    }

    public readonly struct VPChangedEvent
    {
        public readonly int PlayerId;
        public readonly string VPId;
        public readonly bool Gained;

        public VPChangedEvent(int playerId, string vpId, bool gained)
        {
            PlayerId = playerId;
            VPId = vpId;
            Gained = gained;
        }
    }

    public readonly struct CountdownStartedEvent
    {
        public readonly int PlayerId;
        public readonly float Duration;

        public CountdownStartedEvent(int playerId, float duration)
        {
            PlayerId = playerId;
            Duration = duration;
        }
    }

    public readonly struct CountdownCancelledEvent { }

    public readonly struct GameOverEvent
    {
        public readonly int WinnerId;
        public GameOverEvent(int winnerId) { WinnerId = winnerId; }
    }
}
