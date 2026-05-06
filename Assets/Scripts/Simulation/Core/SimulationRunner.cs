using System;

namespace Settlers.Simulation
{
    /// <summary>
    /// Tick-based simulation update loop.
    /// Calls all systems in the correct order each frame.
    /// Pure C# — no UnityEngine references.
    ///
    /// Update order:
    /// 1. ConstructionSystem
    /// 2. UpgradeSystem (building upgrades)
    /// 3. PopulationSystem (assign workers/tools)
    /// 4. ProductionSystem (recipe processing + food boosting)
    /// 5. LogisticsSystem (carrier dispatch)
    /// 6. ArmySystem (training + movement)
    /// 7. ConquestSystem (proselytism progress)
    /// 7b. FortificationSystem (building fortifications)
    /// 8. ResearchSystem (monastery research)
    /// 9. TradeSystem (trader movement)
    /// 10. VictorySystem (VP evaluation + countdown)
    /// 11. AIController (AI decisions)
    /// </summary>
    public class SimulationRunner
    {
        private const int SYSTEM_COUNT = 12;

        private readonly GameState _state;
        // Index → System:
        //  0  ConstructionSystem
        //  1  UpgradeSystem
        //  2  PopulationSystem
        //  3  ProductionSystem
        //  4  LogisticsSystem
        //  5  ArmySystem
        //  6  ConquestSystem
        //  7  FortificationSystem
        //  8  ResearchSystem
        //  9  TradeSystem
        // 10  VictorySystem
        // 11  AIController
        private readonly bool[] _enabledSystems = new bool[SYSTEM_COUNT];
        private int _tickCount;

        /// <summary>Total ticks since creation.</summary>
        public int TickCount => _tickCount;

        /// <summary>
        /// Callback invoked every 60 ticks with the current tick count.
        /// Presentation layer can wire this to Debug.Log.
        /// </summary>
        public Action<int> OnTickLog;

        public SimulationRunner(GameState state)
        {
            _state = state;
            // All systems disabled by default
        }

        /// <summary>Enable a system by index (0–11).</summary>
        public void EnableSystem(int index)
        {
            if (index >= 0 && index < SYSTEM_COUNT)
                _enabledSystems[index] = true;
        }

        /// <summary>Disable a system by index (0–11).</summary>
        public void DisableSystem(int index)
        {
            if (index >= 0 && index < SYSTEM_COUNT)
                _enabledSystems[index] = false;
        }

        /// <summary>Enable all systems.</summary>
        public void EnableAll()
        {
            for (int i = 0; i < SYSTEM_COUNT; i++)
                _enabledSystems[i] = true;
        }

        /// <summary>Disable all systems.</summary>
        public void DisableAll()
        {
            for (int i = 0; i < SYSTEM_COUNT; i++)
                _enabledSystems[i] = false;
        }

        /// <summary>
        /// Advance the simulation by deltaTime seconds.
        /// Call once per frame from GameController.Update().
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            _state.AdvanceTime(deltaTime);
            _tickCount++;

            if (_tickCount % 60 == 0)
                OnTickLog?.Invoke(_tickCount);

            // 1. Construction — advance building progress
            if (_enabledSystems[0])
                _state.Construction.Tick(deltaTime);

            // 2. Upgrades — advance building upgrade progress
            if (_enabledSystems[1])
                _state.Upgrades.Tick(deltaTime);

            // 3. Population — auto-assign workers + tools to work yards
            if (_enabledSystems[2])
                _state.Population.Tick(deltaTime);

            // 4. Production — process recipe cycles with food boosting
            if (_enabledSystems[3])
                _state.Production.Tick(deltaTime);

            // 5. Logistics — advance carrier deliveries
            if (_enabledSystems[4])
                _state.Logistics.Tick(deltaTime);

            // 6. Army — training + movement
            if (_enabledSystems[5])
                _state.Army.Tick(deltaTime);

            // 7. Conquest — proselytism progress
            if (_enabledSystems[6])
                _state.Conquest.Tick(deltaTime);

            // 7b. Fortification — building fortifications
            if (_enabledSystems[7])
                _state.Fortification.Tick(deltaTime);

            // 8. Research — monastery tech research
            if (_enabledSystems[8])
                _state.Research.Tick(deltaTime);

            // 9. Trade — trader movement + claiming
            if (_enabledSystems[9])
                _state.Trade.Tick(deltaTime);

            // 10. Victory — evaluate VPs, manage countdown
            if (_enabledSystems[10])
                _state.Victory.Tick(deltaTime);

            // 11. AI — decision making for AI players
            if (_enabledSystems[11])
            {
                foreach (var ai in _state.AIPlayers)
                    ai.Tick(deltaTime);
            }
        }
    }
}
