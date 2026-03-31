using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>Tracks VPs (dynamic + permanent), evaluates conditions, manages countdown.</summary>
    public class VictorySystem
    {
        private readonly GameState _state;
        private readonly EventBus _eventBus;
        private readonly int _vpRequired;
        private readonly float _countdownDuration;
        private readonly VPThresholds _thresholds;

        // Dynamic VPs: can be gained and lost
        private readonly Dictionary<int, HashSet<string>> _dynamicVPs = new();
        // Permanent VPs: once gained, kept forever
        private readonly Dictionary<int, HashSet<string>> _permanentVPs = new();

        // Countdown state
        private int _countdownPlayerId = -1;
        private float _countdownRemaining;
        private bool _gameOver;
        private int _winnerId = -1;

        // Per-player attack tracking for Pacifist VP
        private readonly Dictionary<int, float> _lastAttackTime = new();
        // Per-player kill counter for Generalissimo VP
        private readonly Dictionary<int, int> _killCount = new();

        public VictorySystem(GameState state, int vpRequired, float countdownDuration,
            VPThresholds thresholds = null)
        {
            _state = state;
            _eventBus = state.Events;
            _vpRequired = vpRequired;
            _countdownDuration = countdownDuration;
            _thresholds = thresholds ?? new VPThresholds();

            // Subscribe to combat events for Pacifist + Generalissimo tracking
            _eventBus.Subscribe<CombatResolvedEvent>(OnCombatResolved);
        }

        /// <summary>Active thresholds (for UI display).</summary>
        public VPThresholds Thresholds => _thresholds;

        private void OnCombatResolved(CombatResolvedEvent evt)
        {
            // Track last attack time (for Pacifist VP — any attack disqualifies)
            _lastAttackTime[evt.AttackerId] = _state.SimulationTime;

            // Track kills (for Generalissimo VP)
            if (!_killCount.ContainsKey(evt.AttackerId))
                _killCount[evt.AttackerId] = 0;
            _killCount[evt.AttackerId] += evt.DefenderLosses;
        }

        /// <summary>Get kill count for a player.</summary>
        public int GetKillCount(int playerId) =>
            _killCount.TryGetValue(playerId, out var c) ? c : 0;

        /// <summary>Get last attack time for a player (-1 if never attacked).</summary>
        public float GetLastAttackTime(int playerId) =>
            _lastAttackTime.TryGetValue(playerId, out var t) ? t : -1f;

        public int VPRequired => _vpRequired;
        public bool IsGameOver => _gameOver;
        public int WinnerId => _winnerId;
        public bool IsCountdownActive => _countdownPlayerId >= 0;
        public int CountdownPlayerId => _countdownPlayerId;
        public float CountdownRemaining => _countdownRemaining;

        /// <summary>Get total VP count for a player.</summary>
        public int GetVPCount(int playerId)
        {
            int count = 0;
            if (_dynamicVPs.TryGetValue(playerId, out var dyn))
                count += dyn.Count;
            if (_permanentVPs.TryGetValue(playerId, out var perm))
                count += perm.Count;
            return count;
        }

        /// <summary>Get all VP IDs held by a player (dynamic + permanent).</summary>
        public List<string> GetAllVPs(int playerId)
        {
            var result = new List<string>();
            if (_dynamicVPs.TryGetValue(playerId, out var dyn))
                result.AddRange(dyn);
            if (_permanentVPs.TryGetValue(playerId, out var perm))
                result.AddRange(perm);
            return result;
        }

        /// <summary>Check if a player holds a specific VP.</summary>
        public bool HasVP(int playerId, string vpId)
        {
            if (_dynamicVPs.TryGetValue(playerId, out var dyn) && dyn.Contains(vpId))
                return true;
            if (_permanentVPs.TryGetValue(playerId, out var perm) && perm.Contains(vpId))
                return true;
            return false;
        }

        /// <summary>Award a permanent VP (cannot be lost).</summary>
        public void AwardPermanentVP(int playerId, string vpId)
        {
            if (!_permanentVPs.TryGetValue(playerId, out var set))
            {
                set = new HashSet<string>();
                _permanentVPs[playerId] = set;
            }
            if (set.Add(vpId))
                _eventBus.Publish(new VPChangedEvent(playerId, vpId, true));
        }

        /// <summary>Tick: evaluate dynamic VPs and manage countdown.</summary>
        public void Tick(float deltaTime)
        {
            if (_gameOver) return;

            EvaluateDynamicVPs();
            ManageCountdown(deltaTime);
        }

        private void EvaluateDynamicVPs()
        {
            var t = _thresholds;
            int n = _state.PlayerCount;

            // Competitive VPs: highest value above threshold wins (ties keep current holder)
            EvalCompetitive("vp_field_marshal", t.FieldMarshalArmy, p => _state.Army.GetTotalArmySize(p));
            EvalCompetitive("vp_metropolis", t.MetropolisWorkers, p => _state.Population.GetEmployedCount(p));
            EvalCompetitive("vp_emperor", t.EmperorSectors, p => _state.Graph.GetSectorsOwnedBy(p).Count);
            EvalCompetitive("vp_banker", t.BankerCoins, p => GetCoins(p));
            EvalCompetitive("vp_sun_king", t.SunKingPrestige, p => _state.Prestige.GetLevel(p));
            EvalCompetitive("vp_trading_company", t.TradingCompanyOutposts, p => _state.TradeMapData.GetClaimedCount(p));
            EvalCompetitive("vp_fountain", t.FountainTechs, p => _state.Research.GetTechCount(p));
            EvalCompetitive("vp_generalissimo", t.GeneralissimoKills, p => GetKillCount(p));

            // Non-competitive VPs: any player meeting the condition holds it
            for (int p = 0; p < n; p++)
            {
                float lastAtk = GetLastAttackTime(p);
                float timeSinceAttack = lastAtk < 0
                    ? _state.SimulationTime
                    : _state.SimulationTime - lastAtk;
                EvalDynamic(p, "vp_pacifist",
                    _state.SimulationTime >= t.PacifistSeconds && timeSinceAttack >= t.PacifistSeconds);

                EvalDynamic(p, "vp_economist",
                    GetStaffingPercent(p) >= t.EconomistStaffPercent);
            }
        }

        /// <summary>Highest value above threshold wins. Ties preserve current holder.</summary>
        private void EvalCompetitive(string vpId, int threshold, System.Func<int, int> getValue)
        {
            int bestPlayer = -1;
            int bestValue = threshold - 1;

            // Find current holder (for tie-breaking)
            int currentHolder = -1;
            for (int p = 0; p < _state.PlayerCount; p++)
            {
                if (_dynamicVPs.TryGetValue(p, out var s) && s.Contains(vpId))
                    currentHolder = p;
            }

            for (int p = 0; p < _state.PlayerCount; p++)
            {
                int val = getValue(p);
                if (val >= threshold)
                {
                    if (val > bestValue || (val == bestValue && p == currentHolder))
                    {
                        bestValue = val;
                        bestPlayer = p;
                    }
                }
            }

            for (int p = 0; p < _state.PlayerCount; p++)
                EvalDynamic(p, vpId, p == bestPlayer);
        }

        private void EvalDynamic(int playerId, string vpId, bool condition)
        {
            if (!_dynamicVPs.TryGetValue(playerId, out var set))
            {
                set = new HashSet<string>();
                _dynamicVPs[playerId] = set;
            }

            bool hadIt = set.Contains(vpId);
            if (condition && !hadIt)
            {
                // Check if another player holds it — steal it
                for (int other = 0; other < _state.PlayerCount; other++)
                {
                    if (other == playerId) continue;
                    if (_dynamicVPs.TryGetValue(other, out var otherSet) &&
                        otherSet.Remove(vpId))
                    {
                        _eventBus.Publish(new VPChangedEvent(other, vpId, false));
                    }
                }
                set.Add(vpId);
                _eventBus.Publish(new VPChangedEvent(playerId, vpId, true));
            }
            else if (!condition && hadIt)
            {
                set.Remove(vpId);
                _eventBus.Publish(new VPChangedEvent(playerId, vpId, false));
            }
        }

        private void ManageCountdown(float deltaTime)
        {
            // Check if any player meets VP requirement (highest VP count wins ties)
            int leader = -1;
            int leaderVPs = 0;
            for (int p = 0; p < _state.PlayerCount; p++)
            {
                int vps = GetVPCount(p);
                if (vps >= _vpRequired && vps > leaderVPs)
                {
                    leader = p;
                    leaderVPs = vps;
                }
            }

            if (leader >= 0)
            {
                if (_countdownPlayerId < 0)
                {
                    // Start countdown
                    _countdownPlayerId = leader;
                    _countdownRemaining = _countdownDuration;
                    _eventBus.Publish(new CountdownStartedEvent(leader, _countdownDuration));
                }
                else if (_countdownPlayerId == leader)
                {
                    // Continue countdown
                    _countdownRemaining -= deltaTime;
                    if (_countdownRemaining <= 0f)
                    {
                        // Victory!
                        _gameOver = true;
                        _winnerId = leader;
                        _eventBus.Publish(new GameOverEvent(leader));
                    }
                }
                else
                {
                    // Different player — restart countdown
                    _countdownPlayerId = leader;
                    _countdownRemaining = _countdownDuration;
                    _eventBus.Publish(new CountdownStartedEvent(leader, _countdownDuration));
                }
            }
            else if (_countdownPlayerId >= 0)
            {
                // Leader lost VPs — cancel countdown
                _countdownPlayerId = -1;
                _countdownRemaining = 0f;
                _eventBus.Publish(new CountdownCancelledEvent());
            }
        }

        private int GetCoins(int playerId)
        {
            return _state.PlayerResources.TryGetValue(playerId, out var r)
                ? r.Get(ResourceType.Coins) : 0;
        }

        private int GetStaffingPercent(int playerId)
        {
            var buildings = _state.Construction.GetBuildingsByPlayer(playerId);
            int totalWY = 0;
            int staffedWY = 0;
            foreach (var b in buildings)
            {
                if (!b.IsOperational) continue;
                foreach (var wy in b.WorkYards)
                {
                    totalWY++;
                    if (wy.IsOperational) staffedWY++;
                }
            }
            return totalWY > 0 ? (staffedWY * 100) / totalWY : 0;
        }
    }
}
