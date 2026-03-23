using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Tracks all Victory Points (dynamic + permanent), evaluates conditions each tick,
    /// manages the 3-minute countdown when a player reaches the required VP count.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class VictorySystem
    {
        private readonly GameState _state;
        private readonly EventBus _eventBus;
        private readonly int _vpRequired;
        private readonly float _countdownDuration;

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

        public VictorySystem(GameState state, int vpRequired, float countdownDuration)
        {
            _state = state;
            _eventBus = state.Events;
            _vpRequired = vpRequired;
            _countdownDuration = countdownDuration;

            // Subscribe to combat events for Pacifist + Generalissimo tracking
            _eventBus.Subscribe<CombatResolvedEvent>(OnCombatResolved);
        }

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

        /// <summary>VP count required to win.</summary>
        public int VPRequired => _vpRequired;

        /// <summary>Is the game over?</summary>
        public bool IsGameOver => _gameOver;

        /// <summary>Winner player ID (-1 if no winner yet).</summary>
        public int WinnerId => _winnerId;

        /// <summary>Is a countdown active?</summary>
        public bool IsCountdownActive => _countdownPlayerId >= 0;

        /// <summary>Countdown player ID (-1 if none).</summary>
        public int CountdownPlayerId => _countdownPlayerId;

        /// <summary>Remaining countdown time in seconds.</summary>
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
            for (int p = 0; p < _state.PlayerCount; p++)
            {
                EvaluatePlayerDynamicVPs(p);
            }
        }

        private void EvaluatePlayerDynamicVPs(int playerId)
        {
            // Field Marshal: ≥20 army
            EvalDynamic(playerId, "vp_field_marshal",
                _state.Army.GetTotalArmySize(playerId) >= 20);

            // Metropolis: ≥25 workers (employed)
            EvalDynamic(playerId, "vp_metropolis",
                _state.Population.GetEmployedCount(playerId) >= 25);

            // Emperor: ≥3 sectors
            EvalDynamic(playerId, "vp_emperor",
                _state.Graph.GetSectorsOwnedBy(playerId).Count >= 3);

            // Banker: ≥25 coins
            var res = _state.PlayerResources.TryGetValue(playerId, out var r) ? r : null;
            EvalDynamic(playerId, "vp_banker",
                res != null && res.Get(ResourceType.Coins) >= 25);

            // Sun King: ≥5 prestige level
            EvalDynamic(playerId, "vp_sun_king",
                _state.Prestige.GetLevel(playerId) >= 5);

            // Trading Company: ≥5 outposts
            EvalDynamic(playerId, "vp_trading_company",
                _state.TradeMapData.GetClaimedCount(playerId) >= 5);

            // Fountain of Knowledge: ≥3 techs
            EvalDynamic(playerId, "vp_fountain",
                _state.Research.GetTechCount(playerId) >= 3);

            // Pacifist: ≥10 minutes without attacking
            float lastAtk = GetLastAttackTime(playerId);
            float timeSinceAttack = lastAtk < 0
                ? _state.SimulationTime // never attacked
                : _state.SimulationTime - lastAtk;
            EvalDynamic(playerId, "vp_pacifist",
                _state.SimulationTime >= 600f && timeSinceAttack >= 600f);

            // Economist: ≥75% work yards staffed
            EvalDynamic(playerId, "vp_economist",
                GetStaffingPercent(playerId) >= 75);

            // Generalissimo: ≥20 kills
            EvalDynamic(playerId, "vp_generalissimo",
                GetKillCount(playerId) >= 20);
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
