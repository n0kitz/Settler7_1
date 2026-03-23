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
        private readonly GameState _state;

        public SimulationRunner(GameState state)
        {
            _state = state;
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

            // 1. Construction — advance building progress
            _state.Construction.Tick(deltaTime);

            // 2. Upgrades — advance building upgrade progress
            _state.Upgrades.Tick(deltaTime);

            // 3. Population — auto-assign workers + tools to work yards
            _state.Population.Tick(deltaTime);

            // 4. Production — process recipe cycles with food boosting
            _state.Production.Tick(deltaTime);

            // 5. Logistics — advance carrier deliveries
            _state.Logistics.Tick(deltaTime);

            // 6. Army — training + movement
            _state.Army.Tick(deltaTime);

            // 7. Conquest — proselytism progress
            _state.Conquest.Tick(deltaTime);

            // 7b. Fortification — building fortifications
            _state.Fortification.Tick(deltaTime);

            // 8. Research — monastery tech research
            _state.Research.Tick(deltaTime);

            // 9. Trade — trader movement + claiming
            _state.Trade.Tick(deltaTime);

            // 10. Victory — evaluate VPs, manage countdown
            _state.Victory.Tick(deltaTime);

            // 11. AI — decision making for AI players
            foreach (var ai in _state.AIPlayers)
                ai.Tick(deltaTime);
        }
    }
}
