namespace Settlers.Simulation
{
    /// <summary>
    /// AI opponent: EarlyEconomy → PathSelection → Execution.
    /// Delegates economy to AIEconomy, handles path-specific strategy.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class AIController
    {
        private readonly GameState _state;
        private readonly int _playerId;
        private float _decisionTimer;
        private AIPhase _phase;
        private AIPath _chosenPath;

        private const float DECISION_INTERVAL = 5f;

        public AIController(GameState state, int playerId)
        {
            _state = state;
            _playerId = playerId;
            _phase = AIPhase.EarlyEconomy;
            _chosenPath = AIPath.None;
        }

        public AIPhase Phase => _phase;
        public AIPath ChosenPath => _chosenPath;
        public int PlayerId => _playerId;

        public void Tick(float deltaTime)
        {
            _decisionTimer -= deltaTime;
            if (_decisionTimer > 0f) return;
            _decisionTimer = DECISION_INTERVAL;

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

            if (totalOp >= 4)
                _phase = AIPhase.PathSelection;
        }

        private void TickPathSelection()
        {
            int sectors = _state.Graph.GetSectorsOwnedBy(_playerId).Count;
            int coins = AIEconomy.GetResource(_state, _playerId, ResourceType.Coins);
            int weapons = AIEconomy.GetResource(_state, _playerId, ResourceType.Weapons);

            // Smarter path selection based on resources and map state
            if (weapons >= 3 || sectors >= 2)
                _chosenPath = AIPath.Military;
            else if (coins >= 10)
                _chosenPath = AIPath.Trade;
            else
                _chosenPath = AIPath.Technology;

            _phase = AIPhase.Execution;
        }

        private void TickExecution()
        {
            // Core economy — always runs
            AIEconomy.BuildEconomy(_state, _playerId);
            AIEconomy.AttachWorkYards(_state, _playerId);
            AIEconomy.ManageFood(_state, _playerId);
            AIEconomy.TryUpgradeBuildings(_state, _playerId);
            AIEconomy.ManageQuests(_state, _playerId);
            SpendPrestigeUnlocks();

            // Path-specific actions
            switch (_chosenPath)
            {
                case AIPath.Military: TickMilitary(); break;
                case AIPath.Technology: TickTechnology(); break;
                case AIPath.Trade: TickTrade(); break;
            }

            // Opportunistic: try proselytism on adjacent neutral sectors
            TryProselytism();
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

        private void TickMilitary()
        {
            var generals = _state.Army.GetGenerals(_playerId);
            var sectors = _state.Graph.GetSectorsOwnedBy(_playerId);
            if (sectors.Count == 0) return;

            // Hire generals if possible
            if (generals.Count == 0)
                _state.Army.HireGeneral(_playerId, sectors[0]);

            // Train units in home sector
            if (_state.Prestige.HasUnlock(_playerId, "mil_pikeman") &&
                AIEconomy.GetResource(_state, _playerId, ResourceType.Weapons) >= 1)
                _state.Army.TrainUnit(_playerId, sectors[0], UnitType.Pikeman);

            // Also train musketeers if unlocked (needed for fortified sectors)
            if (_state.Prestige.HasUnlock(_playerId, "mil_musketeer") &&
                AIEconomy.GetResource(_state, _playerId, ResourceType.Weapons) >= 1)
                _state.Army.TrainUnit(_playerId, sectors[0], UnitType.Musketeer);

            // Assign soldiers to generals
            foreach (var gen in generals)
                if (gen.TotalSoldiers < gen.MaxSoldiers)
                    _state.Army.AssignUnit(gen, UnitType.Pikeman);

            // Attack when ready
            if (generals.Count > 0 && generals[0].TotalSoldiers >= 8 && !generals[0].IsMoving)
            {
                int target = FindAttackTarget(generals[0]);
                if (target >= 0)
                    _state.Army.MoveArmy(generals[0], target);
            }
        }

        private int FindAttackTarget(General gen)
        {
            // First check for weak neutral sectors nearby
            int bestTarget = -1;
            int bestScore = int.MinValue;

            foreach (int n in _state.Graph.GetNeighbors(gen.SectorId))
            {
                var sector = _state.Graph.GetSector(n);
                if (sector.OwnerId == _playerId) continue;

                int score = 0;
                if (sector.IsNeutral)
                {
                    score = 100 - sector.GarrisonStrength * 5;
                    if (sector.IsFortified) score -= 30;
                    if (sector.VPRewardId != null) score += 50;
                }
                else
                {
                    // Enemy sector — only attack if strong advantage
                    score = gen.TotalAttack - sector.GarrisonStrength * 10 - 20;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = n;
                }
            }

            return bestScore > 0 ? bestTarget : -1;
        }

        private void TickTechnology()
        {
            bool anyActive = false;
            foreach (var task in _state.Research.ActiveTasks)
                if (task.PlayerId == _playerId) { anyActive = true; break; }

            if (!anyActive)
            {
                // Prioritize lower tiers first
                foreach (var tech in TechTree.All)
                {
                    if (_state.Research.HasTech(_playerId, tech.Id)) continue;
                    if (_state.Research.StartResearch(_playerId, tech.Id)) break;
                }
            }
        }

        private void TickTrade()
        {
            if (!_state.Prestige.HasUnlock(_playerId, "cul_export_office")) return;

            // Claim unclaimed outposts
            foreach (var outpost in _state.TradeMapData.AllOutposts)
            {
                if (!outpost.IsClaimed)
                {
                    _state.Trade.SendTrader(_playerId, outpost.Id);
                    break;
                }
            }

            // Execute trades on claimed outposts when we have the input resource
            foreach (var outpost in _state.TradeMapData.AllOutposts)
            {
                if (outpost.ClaimedBy != _playerId) continue;
                int have = AIEconomy.GetResource(_state, _playerId, outpost.InputResource);
                if (have >= outpost.InputAmount)
                    _state.Trade.ExecuteTrade(_playerId, outpost.Id);
            }
        }

        /// <summary>Opportunistic proselytism on adjacent neutral sectors.</summary>
        private void TryProselytism()
        {
            var sectors = _state.Graph.GetSectorsOwnedBy(_playerId);
            foreach (int owned in sectors)
            {
                foreach (int n in _state.Graph.GetNeighbors(owned))
                {
                    var neighbor = _state.Graph.GetSector(n);
                    if (!neighbor.IsNeutral) continue;
                    if (neighbor.IsFortified) continue; // skip fortified for proselytism

                    int clericCount = 6;
                    if (_state.Conquest.StartProselytism(_playerId, n, clericCount))
                        return; // One at a time
                }
            }
        }
    }

    public enum AIPhase { EarlyEconomy, PathSelection, Execution }
    public enum AIPath { None, Military, Technology, Trade }
}
