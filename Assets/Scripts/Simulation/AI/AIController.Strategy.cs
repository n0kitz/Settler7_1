namespace Settlers.Simulation
{
    public partial class AIController
    {
        private void TickMilitary()
        {
            var generals = _state.Army.GetGenerals(_playerId);
            var sectors = _state.Graph.GetSectorsOwnedBy(_playerId);
            if (sectors.Count == 0) return;

            // Hire generals — 2nd when economy supports it
            if (generals.Count == 0)
                _state.Army.HireGeneral(_playerId, sectors[0]);
            else if (generals.Count == 1 &&
                     AIEconomy.GetResource(_state, _playerId, ResourceType.Coins) >= 20 &&
                     _state.Prestige.HasUnlock(_playerId, "mil_second_general"))
                _state.Army.HireGeneral(_playerId, sectors[0]);

            // Train units in home sector
            if (_state.Prestige.HasUnlock(_playerId, "mil_pikeman") &&
                AIEconomy.GetResource(_state, _playerId, ResourceType.Weapons) >= 1)
                _state.Army.TrainUnit(_playerId, sectors[0], UnitType.Pikeman);

            if (_state.Prestige.HasUnlock(_playerId, "mil_musketeer") &&
                AIEconomy.GetResource(_state, _playerId, ResourceType.Weapons) >= 1)
                _state.Army.TrainUnit(_playerId, sectors[0], UnitType.Musketeer);

            // Assign soldiers to generals
            foreach (var gen in generals)
                if (gen.TotalSoldiers < gen.MaxSoldiers)
                    _state.Army.AssignUnit(gen, UnitType.Pikeman);

            // Attack when ready — each general acts independently
            foreach (var gen in generals)
            {
                if (gen.TotalSoldiers >= _profile.Difficulty.AttackThreshold && !gen.IsMoving)
                {
                    int target = FindAttackTarget(gen);
                    if (target >= 0)
                        _state.Army.MoveArmy(gen, target);
                }
            }
        }

        private int FindAttackTarget(General gen)
        {
            int bestTarget = -1;
            int bestScore = int.MinValue;

            foreach (int n in _state.Graph.GetNeighbors(gen.SectorId))
            {
                var sector = _state.Graph.GetSector(n);
                if (sector.OwnerId == _playerId) continue;

                int score;
                if (sector.IsNeutral)
                {
                    score = 100 - sector.GarrisonStrength * 5;
                    if (sector.IsFortified) score -= 30;
                    if (sector.VPRewardId != null) score += 50;
                }
                else
                {
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

            foreach (var outpost in _state.TradeMapData.AllOutposts)
            {
                if (!outpost.IsClaimed)
                {
                    _state.Trade.SendTrader(_playerId, outpost.Id);
                    break;
                }
            }

            foreach (var outpost in _state.TradeMapData.AllOutposts)
            {
                if (outpost.ClaimedBy != _playerId) continue;
                int have = AIEconomy.GetResource(_state, _playerId, outpost.InputResource);
                if (have >= outpost.InputAmount)
                    _state.Trade.ExecuteTrade(_playerId, outpost.Id);
            }
        }

        private void TryProselytism()
        {
            foreach (int owned in _state.Graph.GetSectorsOwnedBy(_playerId))
            {
                foreach (int n in _state.Graph.GetNeighbors(owned))
                {
                    var neighbor = _state.Graph.GetSector(n);
                    if (!neighbor.IsNeutral) continue;
                    if (neighbor.IsFortified) continue;

                    if (_state.Conquest.StartProselytism(_playerId, n, 6))
                        return;
                }
            }
        }

        private void TryFortify()
        {
            if (!_state.Prestige.HasUnlock(_playerId, "mil_fortification")) return;
            int stone = AIEconomy.GetResource(_state, _playerId, ResourceType.Stone);
            if (stone < 15) return;

            foreach (int sectorId in _state.Graph.GetSectorsOwnedBy(_playerId))
            {
                if (_state.Fortification.StartFortification(_playerId, sectorId))
                    return;
            }
        }

        private void TryBribery()
        {
            int coins = AIEconomy.GetResource(_state, _playerId, ResourceType.Coins);
            int garments = AIEconomy.GetResource(_state, _playerId, ResourceType.Garments);
            int jewelry = AIEconomy.GetResource(_state, _playerId, ResourceType.Jewelry);
            if (coins < 15 || garments < 3 || jewelry < 2) return;

            foreach (int owned in _state.Graph.GetSectorsOwnedBy(_playerId))
            {
                foreach (int n in _state.Graph.GetNeighbors(owned))
                {
                    var neighbor = _state.Graph.GetSector(n);
                    if (!neighbor.IsNeutral) continue;
                    if (_state.Conquest.TryBribe(_playerId, n))
                        return;
                }
            }
        }

        private void ConsiderPathSwitch()
        {
            _stallTimer += _profile.Difficulty.DecisionInterval;
            if (_stallTimer < _profile.Difficulty.StallDuration) return;

            int vps = _state.Victory.GetVPCount(_playerId);
            if (vps > _lastVPCount)
            {
                _lastVPCount = vps;
                _stallTimer = 0f;
                return;
            }

            _stallTimer = 0f;
            switch (_chosenPath)
            {
                case AIPath.Military when AIEconomy.GetResource(_state, _playerId, ResourceType.Weapons) < 2:
                    _chosenPath = AIPath.Trade;
                    break;
                case AIPath.Technology when _state.Research.GetPlayerTechs(_playerId).Count == 0:
                    _chosenPath = AIPath.Military;
                    break;
                case AIPath.Trade when AIEconomy.GetResource(_state, _playerId, ResourceType.Coins) < 5:
                    _chosenPath = AIPath.Military;
                    break;
            }
        }
    }
}
