namespace Settlers.Simulation
{
    /// <summary>
    /// AI opponent: EarlyEconomy → PathSelection → Execution.
    /// Delegates economy to AIEconomy, handles path-specific strategy.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public partial class AIController
    {
        private readonly GameState _state;
        private readonly int _playerId;
        private readonly AIBehaviorProfile _profile;
        private float _decisionTimer;
        private float _stallTimer;
        private int _lastVPCount;
        private AIPhase _phase;
        private AIPath _chosenPath;
        private int _vpLeaderId = -1;
        private bool _leaderNearWin;

        public AIController(GameState state, int playerId,
            AIBehaviorProfile profile = null)
        {
            _state = state;
            _playerId = playerId;
            _profile = profile ?? AIBehaviorProfile.Default;
            _phase = AIPhase.EarlyEconomy;
            _chosenPath = AIPath.None;
            _decisionTimer = _profile.Difficulty.DecisionInterval;
        }

        public AIPhase Phase => _phase;
        public AIPath ChosenPath => _chosenPath;
        public int PlayerId => _playerId;
        public AIBehaviorProfile Profile => _profile;

        /// <summary>Opponent with the most VPs (victory race tracking).</summary>
        internal int VPLeaderId => _vpLeaderId;
        /// <summary>True when an opponent is within 1 VP of winning or counting down.</summary>
        internal bool LeaderNearWin => _leaderNearWin;

        public void Tick(float deltaTime)
        {
            _decisionTimer -= deltaTime;
            if (_decisionTimer > 0f) return;
            _decisionTimer = _profile.Difficulty.DecisionInterval;

            switch (_phase)
            {
                case AIPhase.EarlyEconomy: TickEarlyEconomy(); break;
                case AIPhase.PathSelection: TickPathSelection(); break;
                case AIPhase.Execution: TickExecution(); break;
            }
        }

        private void TickEarlyEconomy()
        {
            AIEconomy.BuildEconomy(_state, _playerId);
            AIEconomy.AttachWorkYards(_state, _playerId);

            int totalOp = 0;
            foreach (var b in _state.Construction.GetBuildingsByPlayer(_playerId))
                if (b.IsOperational) totalOp++;

            if (totalOp >= _profile.Personality.EarlyEconomyThreshold)
                _phase = AIPhase.PathSelection;
        }

        private void TickPathSelection()
        {
            int sectors = _state.Graph.GetSectorsOwnedBy(_playerId).Count;
            int coins = AIEconomy.GetResource(_state, _playerId, ResourceType.Coins);
            int weapons = AIEconomy.GetResource(_state, _playerId, ResourceType.Weapons);
            var p = _profile.Personality;

            // Base scores from resource situation, scaled by personality weights
            float milScore = (weapons + sectors) * p.MilitaryWeight;
            float tradeScore = coins * 0.5f * p.TradeWeight;
            float techScore = 3f * p.TechWeight; // always a viable option

            if (milScore >= tradeScore && milScore >= techScore)
                _chosenPath = AIPath.Military;
            else if (tradeScore >= techScore)
                _chosenPath = AIPath.Trade;
            else
                _chosenPath = AIPath.Technology;

            _phase = AIPhase.Execution;
        }

        private void TickExecution()
        {
            AssessVictoryRace();

            // Core economy — always runs
            AIEconomy.BuildEconomy(_state, _playerId);
            AIEconomy.AttachWorkYards(_state, _playerId);
            AIEconomy.ManageFood(_state, _playerId);
            AIEconomy.TryUpgradeBuildings(_state, _playerId);
            AIEconomy.ManageQuests(_state, _playerId);
            SpendPrestigeUnlocks();

            // Fortify owned sectors when possible
            TryFortify();

            // Path-specific actions
            switch (_chosenPath)
            {
                case AIPath.Military: TickMilitary(); break;
                case AIPath.Technology: TickTechnology(); break;
                case AIPath.Trade: TickTrade(); break;
            }

            // Victory race: an opponent is about to win — contest militarily
            // even off the Military path (attacking their sectors can strip
            // dynamic VPs like sector-count and break their countdown)
            if (_leaderNearWin && _chosenPath != AIPath.Military)
                TickMilitary();

            // Consider switching path if stalled
            ConsiderPathSwitch();

            // Opportunistic: try proselytism or bribery on adjacent neutral sectors
            TryProselytism();
            TryBribery();
        }

        /// <summary>
        /// Track the strongest opponent by VP count. "Near win" = within 1 VP
        /// of the requirement or already running the victory countdown.
        /// </summary>
        internal void AssessVictoryRace()
        {
            var victory = _state.Victory;
            int leaderId = -1;
            int leaderVPs = -1;
            for (int p = 0; p < _state.PlayerCount; p++)
            {
                if (p == _playerId) continue;
                int vps = victory.GetVPCount(p);
                if (vps > leaderVPs) { leaderVPs = vps; leaderId = p; }
            }

            _vpLeaderId = leaderId;
            _leaderNearWin = leaderVPs >= victory.VPRequired - 1
                || (victory.IsCountdownActive && victory.CountdownPlayerId != _playerId);
        }

        private void SpendPrestigeUnlocks()
        {
            if (_state.Prestige.GetUnspentLevels(_playerId) <= 0) return;

            var priorities = _chosenPath switch
            {
                AIPath.Military => new[]
                {
                    "mil_stronghold", "mil_pikeman", "mil_musketeer",
                    "eco_residence_upgrade", "eco_storehouse_lv2",
                    "mil_cavalier", "mil_fortification"
                },
                AIPath.Technology => new[]
                {
                    "cul_church", "cul_novice", "eco_residence_upgrade",
                    "eco_storehouse_lv2", "cul_brother", "cul_father"
                },
                AIPath.Trade => new[]
                {
                    "cul_export_office", "cul_hawker", "eco_residence_upgrade",
                    "eco_storehouse_lv2", "cul_salesman", "cul_merchant"
                },
                _ => new[] { "eco_residence_upgrade", "eco_storehouse_lv2" }
            };

            foreach (var id in priorities)
            {
                if (_state.Prestige.GetUnspentLevels(_playerId) <= 0) break;
                _state.Prestige.TryUnlock(_playerId, id);
            }
        }

    }

    public enum AIPhase { EarlyEconomy, PathSelection, Execution }
    public enum AIPath { None, Military, Technology, Trade }
}
