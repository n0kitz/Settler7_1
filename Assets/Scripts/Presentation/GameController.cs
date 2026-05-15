using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;
using Settlers.UI;

namespace Settlers.Presentation
{
    /// <summary>
    /// Bridge between simulation and presentation.
    /// Owns GameState + SimulationRunner. Spawns sector/building views.
    /// Handles sector selection, building placement, and simulation tick.
    /// Split into partial classes: GameController.SectorVisuals.cs for mesh/visuals.
    /// </summary>
    public partial class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        [Header("Sector Visuals")]
        [SerializeField] private float _sectorRadius = 8f;
        [SerializeField] private float _sectorSpacing = 18f;
        [SerializeField] private Material _sectorMaterial;

        [Header("Border")]
        [SerializeField] private float _borderWidth = 0.15f;
        [SerializeField] private float _borderHeight = 0.05f;

        [Header("Map")]
        [SerializeField] private string _mapId = "test_valley";

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
            AIPersonalityType personality = AIPersonalityType.Builder)
        {
            _mapId = mapId;
            _playerCountOverride = playerCount;
            _vpRequiredOverride = vpRequired;
            _aiDifficulty = difficulty;
            _aiPersonality = personality;
            InitializeGame();
        }

        private int _playerCountOverride;
        private int _vpRequiredOverride;
        private AIDifficultyLevel _aiDifficulty = AIDifficultyLevel.Normal;
        private AIPersonalityType _aiPersonality = AIPersonalityType.Builder;

        [Header("Game Constants")]
        [SerializeField] private Data.GameConstants _gameConstants;

        [Header("Construction")]
        [SerializeField] private float _constructionBaseTime = 10f;
        [SerializeField] private int _carrierMaxItems = 3;

        /// <summary>The full simulation state.</summary>
        public GameState State { get; private set; }

        /// <summary>Shortcut to the sector graph.</summary>
        public SectorGraph Graph => State?.Graph;

        /// <summary>Shortcut to the construction system.</summary>
        public ConstructionSystem Construction => State?.Construction;

        /// <summary>Shortcut to the event bus.</summary>
        public EventBus Events => State?.Events;

        private SimulationRunner _runner;
        private SectorView[] _sectorViews;
        private SectorView _selectedSector;
        private SectorPanel _sectorPanel;
        private BuildMenu _buildMenu;
        private BuildingPlacer _buildingPlacer;
        private PrestigeChartUI _prestigeChart;
        private TechTreeUI _techTreeUI;
        private TradeMapUI _tradeMapUI;
        private ArmyPanel _armyPanel;
        private TavernUI _tavernUI;
        private UI.QuestPanel _questPanel;
        private Camera _mainCamera;
        private Material _buildingMaterial;
        private WorkerManager _workerManager;
        private CarrierManager _carrierManager;
        private ArmyViewManager _armyViewManager;
        private ClericManager _clericManager;
        private TutorialSystem _tutorialSystem;

        private readonly Dictionary<int, BuildingView> _buildingViews = new();
        private Transform _buildingsRoot;

        private void Awake()
        {
            Instance = this;
            _mainCamera = Camera.main;
            _sectorPanel = FindAnyObjectByType<SectorPanel>();
            _buildMenu = FindAnyObjectByType<BuildMenu>();
            _prestigeChart = FindAnyObjectByType<PrestigeChartUI>();
            _techTreeUI = FindAnyObjectByType<TechTreeUI>();
            _tradeMapUI = FindAnyObjectByType<TradeMapUI>();
            _armyPanel = FindAnyObjectByType<ArmyPanel>();
            _tavernUI = FindAnyObjectByType<TavernUI>();
            _questPanel = FindAnyObjectByType<UI.QuestPanel>();

            _buildingPlacer = GetComponent<BuildingPlacer>();
            if (_buildingPlacer == null)
                _buildingPlacer = gameObject.AddComponent<BuildingPlacer>();
            _buildingPlacer.OnBuildingPlaced += HandleBuildingPlaced;
        }

        /// <summary>Set by Initialize() — minimal bootstrap done, controller ready.</summary>
        private bool _initialized;
        /// <summary>Set by InitializeGame() — real game is running. Blocks re-entry.</summary>
        private bool _gameRunning;

        private void Start()
        {
            // Don't auto-initialize — wait for MapSelectionUI to call SetMapId().
            // If no MapSelectionUI exists (e.g., testing), init with default map.
            if (FindAnyObjectByType<MapSelectionUI>() == null)
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
                countdown, vpThresholds, aiProfiles);
            _runner = new SimulationRunner(State);
            _runner.EnableAll();

            // Subscribe to simulation events for presentation layer
            State.Events.Subscribe<BuildingDestroyedEvent>(HandleBuildingDestroyed);

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

        private void Update()
        {
            if (_runner == null) return; // Not initialized yet (waiting for map selection)

            HandleClick();
            HandleKeyboardToggles();
            _runner.Tick(Time.deltaTime);
            UpdateBuildingViews();
            _workerManager?.Sync(State.Production.AllWorkYards);
            _carrierManager?.Sync(State.Logistics.ActiveTasks);
            _armyViewManager?.Sync(State.Army, State.PlayerCount);
            _clericManager?.Sync(State.Conquest.ProselytismTasks);
            SyncRoads();
        }

        private void OnDestroy()
        {
            if (_buildingPlacer != null)
                _buildingPlacer.OnBuildingPlaced -= HandleBuildingPlaced;
            if (_buildMenu != null)
                _buildMenu.OnBuildingSelected -= HandleBuildMenuSelection;
        }

        // BuildVPThresholds() → GameController.Buildings.cs
        // Sector click handling → GameController.Input.cs

        /// <summary>Get the number of buildings in a sector.</summary>
        public int GetBuildingCountInSector(int sectorId) =>
            Construction.GetBuildingCountInSector(sectorId);

        /// <summary>Get a player's resource inventory.</summary>
        public PlayerResources GetPlayerResources(int playerId) =>
            State?.PlayerResources.TryGetValue(playerId, out var res) == true ? res : null;

        /// <summary>Award prestige points to a player.</summary>
        public void AwardPrestige(int playerId, int points) =>
            State?.Prestige.AwardPoints(playerId, points);

        /// <summary>Try to upgrade a building.</summary>
        public bool TryUpgradeBuilding(int buildingId) =>
            State?.Upgrades.TryStartUpgrade(buildingId) ?? false;

        /// <summary>Try to build a fortification in a sector.</summary>
        public bool TryBuildFortification(int sectorId, int playerId) =>
            State?.Fortification.StartFortification(playerId, sectorId) ?? false;

        /// <summary>Try to build a paved road between two sectors (costs 5 stone).</summary>
        public bool TryBuildPavedRoad(int sectorA, int sectorB, int playerId)
        {
            if (State == null) return false;
            if (!State.Prestige.HasUnlock(playerId, "eco_paved_roads")) return false;
            var res = GetPlayerResources(playerId);
            if (res == null || !res.Has(ResourceType.Stone, 5)) return false;
            if (!State.Logistics.BuildPavedRoad(sectorA, sectorB)) return false;
            res.TrySpend(ResourceType.Stone, 5);
            return true;
        }
    }
}
