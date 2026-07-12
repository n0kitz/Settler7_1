using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// GameController — game lifecycle: start, initialize, tear down.
    /// The core bridge (fields, tick, public API) lives in GameController.cs.
    /// </summary>
    public partial class GameController
    {
        private int _playerCountOverride;
        private int _vpRequiredOverride;
        private AIDifficultyLevel _aiDifficulty = AIDifficultyLevel.Normal;
        private AIPersonalityType _aiPersonality = AIPersonalityType.Builder;
        private Simulation.GameRules _gameRules = Simulation.GameRules.Default;

        /// <summary>Set by Initialize() — minimal bootstrap done, controller ready.</summary>
        private bool _initialized;
        /// <summary>Set by InitializeGame() — real game is running. Blocks re-entry.</summary>
        private bool _gameRunning;

        /// <summary>
        /// Set the map ID and initialize the game.
        /// Called by MapSelectionUI after the player picks a map.
        /// </summary>
        public void SetMapId(string mapId)
        {
            _mapId = mapId;
            InitializeGame();
        }

        /// <summary>
        /// Start a game with custom player count and VP requirement.
        /// Called by GameSetupUI after the player configures settings.
        /// </summary>
        public void StartGame(string mapId, int playerCount, int vpRequired,
            AIDifficultyLevel difficulty = AIDifficultyLevel.Normal,
            AIPersonalityType personality = AIPersonalityType.Builder,
            Simulation.GameRules rules = null)
        {
            // A finished (or abandoned) game must be torn down first —
            // InitializeGame is _gameRunning-guarded and would silently no-op,
            // leaving the player in the OLD game (broke Play Again / re-New Game)
            if (_gameRunning) TeardownGame();
            _mapId = mapId;
            _playerCountOverride = playerCount;
            _vpRequiredOverride = vpRequired;
            _aiDifficulty = difficulty;
            _aiPersonality = personality;
            _gameRules = rules ?? Simulation.GameRules.Default;
            InitializeGame();
        }

        /// <summary>
        /// Accept a pre-built GameState and SimulationRunner from BootstrapScene.
        /// Skips MapFactory / visual spawning — used for minimal bootstrap.
        /// </summary>
        public void Initialize(GameState state, SimulationRunner runner)
        {
            if (_initialized) return;
            _initialized = true;
            State = state;
            _runner = runner;
        }

        /// <summary>
        /// Tear down the running game (simulation + spawned visuals) so a new
        /// StartGame can boot cleanly. Persistent UI panels survive — they read
        /// GameController.Instance.State and re-resolve on the next game.
        /// </summary>
        private void TeardownGame()
        {
            if (!_gameRunning) return;
            _gameRunning = false;

            if (_buildMenu != null)
                _buildMenu.OnBuildingSelected -= HandleBuildMenuSelection;

            if (_workerManager != null) Destroy(_workerManager);
            if (_carrierManager != null) Destroy(_carrierManager);
            if (_armyViewManager != null) Destroy(_armyViewManager);
            if (_clericManager != null) Destroy(_clericManager);
            _workerManager = null;
            _carrierManager = null;
            _armyViewManager = null;
            _clericManager = null;

            DestroyChildByName("MapRoot");
            DestroyChildByName("Roads");
            DestroyChildByName("Buildings");
            DestroyChildByName("Units");
            _buildingsRoot = null;
            _buildingViews.Clear();
            _sectorViews = null;
            _selectedSector = null;

            _runner = null;
            State = null;
            ActiveCampaign = null;
        }

        private void DestroyChildByName(string name)
        {
            var child = transform.Find(name);
            if (child != null) Destroy(child.gameObject);
        }

        private void InitializeGame()
        {
            if (_gameRunning) return;
            _gameRunning = true;
            _initialized = true;

            // Reset static ID counters so IDs are consistent across game sessions
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            Storehouse.ResetIdCounter();
            ArmySystem.ResetIdCounter();

            var mapInfo = MapFactory.CreateMap(_mapId);
            int playerCount = _playerCountOverride > 0 ? _playerCountOverride : mapInfo.PlayerCount;
            int vpRequired = _vpRequiredOverride > 0 ? _vpRequiredOverride : mapInfo.VPRequired;
            var vpThresholds = BuildVPThresholds();
            float countdown = _gameConstants != null
                ? _gameConstants.victoryCountdownSeconds : 180f;
            var profile = Simulation.AIBehaviorProfile.Create(_aiPersonality, _aiDifficulty);
            var aiProfiles = new Simulation.AIBehaviorProfile[playerCount - 1];
            for (int i = 0; i < aiProfiles.Length; i++)
                aiProfiles[i] = profile;

            State = new GameState(mapInfo.Graph, playerCount: playerCount,
                _constructionBaseTime, _carrierMaxItems, vpRequired, _mapId,
                countdown, vpThresholds, aiProfiles, _gameRules);
            _runner = new SimulationRunner(State);
            _runner.EnableAll();

            // Subscribe to simulation events for presentation layer
            State.Events.Subscribe<BuildingDestroyedEvent>(HandleBuildingDestroyed);
            State.Events.Subscribe<BuildingPlacedEvent>(HandleBuildingPlacedEvent);

            EnsureMaterial();
            EnsureBuildingMaterial();
            ComputeSectorPositions();
            SpawnSectors();
            SpawnRoads();
            RefreshAllOwnership();

            _buildingsRoot = new GameObject("Buildings").transform;
            _buildingsRoot.SetParent(transform);

            // Unit visual managers
            var unitsRoot = new GameObject("Units").transform;
            unitsRoot.SetParent(transform);

            _workerManager = gameObject.AddComponent<WorkerManager>();
            _workerManager.Initialize(unitsRoot, _buildingMaterial);

            _carrierManager = gameObject.AddComponent<CarrierManager>();
            _carrierManager.Initialize(unitsRoot, _buildingMaterial);

            _armyViewManager = gameObject.AddComponent<ArmyViewManager>();
            _armyViewManager.Initialize(unitsRoot, _buildingMaterial);

            _clericManager = gameObject.AddComponent<ClericManager>();
            _clericManager.Initialize(unitsRoot, _buildingMaterial);

            if (_buildMenu != null)
                _buildMenu.OnBuildingSelected += HandleBuildMenuSelection;

            // Tutorial: activate only on the tutorial map
            if (MapFactory.IsTutorial(_mapId))
            {
                _tutorialSystem = new TutorialSystem(State.Events);
                var tutorialUI = FindAnyObjectByType<UI.TutorialOverlayUI>();
                if (tutorialUI != null) tutorialUI.Bind(_tutorialSystem);
                _tutorialSystem.Activate();
            }
        }
    }
}
