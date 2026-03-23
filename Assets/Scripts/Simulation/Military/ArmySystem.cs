using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages armies, generals, unit training, and army movement.
    /// Each general leads up to 35 soldiers. Max 5 generals per player.
    /// Requires prestige unlock: mil_stronghold for the building,
    /// then individual unit unlock per type.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class ArmySystem
    {
        private readonly Dictionary<int, List<General>> _generalsByPlayer = new();
        private readonly List<TrainingTask> _trainingQueue = new();
        private readonly List<ArmyMovement> _movements = new();
        private readonly PrestigeSystem _prestige;
        private readonly Dictionary<int, PlayerResources> _resources;
        private readonly SectorGraph _graph;
        private readonly EventBus _eventBus;
        private readonly int _maxSoldiersPerGeneral;
        private readonly int _maxGenerals;

        private static int _nextGeneralId;

        public ArmySystem(PrestigeSystem prestige,
            Dictionary<int, PlayerResources> resources,
            SectorGraph graph, EventBus eventBus,
            int maxSoldiersPerGeneral = 35, int maxGenerals = 5)
        {
            _prestige = prestige;
            _resources = resources;
            _graph = graph;
            _eventBus = eventBus;
            _maxSoldiersPerGeneral = maxSoldiersPerGeneral;
            _maxGenerals = maxGenerals;
        }

        /// <summary>Get all generals for a player.</summary>
        public IReadOnlyList<General> GetGenerals(int playerId)
        {
            return _generalsByPlayer.TryGetValue(playerId, out var list)
                ? list
                : (IReadOnlyList<General>)System.Array.Empty<General>();
        }

        /// <summary>Get total army size for a player.</summary>
        public int GetTotalArmySize(int playerId)
        {
            int total = 0;
            foreach (var gen in GetGenerals(playerId))
                total += gen.TotalSoldiers;
            return total;
        }

        /// <summary>Active training tasks.</summary>
        public IReadOnlyList<TrainingTask> TrainingQueue => _trainingQueue;

        /// <summary>Active army movements.</summary>
        public IReadOnlyList<ArmyMovement> Movements => _movements;

        /// <summary>Hire a new general in a sector (requires Tavern or stronghold).</summary>
        public General HireGeneral(int playerId, int sectorId)
        {
            if (!_generalsByPlayer.TryGetValue(playerId, out var list))
            {
                list = new List<General>();
                _generalsByPlayer[playerId] = list;
            }

            if (list.Count >= _maxGenerals)
                return null;

            // Second general requires prestige unlock
            if (list.Count >= 1 && !_prestige.HasUnlock(playerId, "mil_second_general"))
                return null;

            var gen = new General(_nextGeneralId++, playerId, sectorId, _maxSoldiersPerGeneral);
            list.Add(gen);
            _eventBus.Publish(new GeneralHiredEvent(playerId, gen.Id, sectorId));
            return gen;
        }

        /// <summary>Queue training of a unit at a stronghold in a sector.</summary>
        public bool TrainUnit(int playerId, int sectorId, UnitType unitType)
        {
            // Check prestige unlock
            string unlockId = UnitStats.GetRequiredUnlock(unitType);
            if (unlockId != null && !_prestige.HasUnlock(playerId, unlockId))
                return false;

            // Check resource cost
            UnitStats.GetTrainingCost(unitType, out var resType, out int cost);
            if (!_resources.TryGetValue(playerId, out var res))
                return false;
            if (!res.TrySpend(resType, cost))
                return false;

            float time = UnitStats.GetTrainingTime(unitType);
            _trainingQueue.Add(new TrainingTask(playerId, sectorId, unitType, time));
            return true;
        }

        /// <summary>Assign a trained unit to a general.</summary>
        public bool AssignUnit(General general, UnitType unitType)
        {
            if (general.TotalSoldiers >= general.MaxSoldiers)
                return false;
            general.AddUnit(unitType);
            return true;
        }

        /// <summary>Move a general's army to a target sector.</summary>
        public bool MoveArmy(General general, int targetSectorId)
        {
            if (general.IsMoving) return false;

            var path = _graph.FindPath(general.SectorId, targetSectorId);
            if (path.Count == 0) return false;

            float travelTime = path.Count * 4f; // 4 seconds per sector hop for armies
            var movement = new ArmyMovement(general, targetSectorId, travelTime, path);
            _movements.Add(movement);
            general.IsMoving = true;
            return true;
        }

        /// <summary>Tick training + army movement.</summary>
        public void Tick(float deltaTime)
        {
            TickTraining(deltaTime);
            TickMovement(deltaTime);
        }

        private void TickTraining(float deltaTime)
        {
            var completed = new List<TrainingTask>();
            for (int i = 0; i < _trainingQueue.Count; i++)
            {
                var task = _trainingQueue[i];
                task.Progress += deltaTime / task.TotalTime;
                if (task.Progress >= 1f)
                {
                    task.Progress = 1f;
                    completed.Add(task);
                }
            }

            foreach (var task in completed)
            {
                _trainingQueue.Remove(task);
                _eventBus.Publish(new UnitTrainedEvent(
                    task.PlayerId, task.SectorId, task.UnitType));
            }
        }

        private void TickMovement(float deltaTime)
        {
            var arrived = new List<ArmyMovement>();
            for (int i = 0; i < _movements.Count; i++)
            {
                var mov = _movements[i];
                mov.Progress += deltaTime / mov.TotalTime;
                if (mov.Progress >= 1f)
                {
                    mov.Progress = 1f;
                    arrived.Add(mov);
                }
            }

            foreach (var mov in arrived)
            {
                _movements.Remove(mov);
                mov.General.SectorId = mov.TargetSectorId;
                mov.General.IsMoving = false;
                _eventBus.Publish(new ArmyArrivedEvent(
                    mov.General.OwnerId, mov.General.Id, mov.TargetSectorId));
            }
        }

        /// <summary>Reset ID counter (for tests).</summary>
        public static void ResetIdCounter() => _nextGeneralId = 0;
    }
}
