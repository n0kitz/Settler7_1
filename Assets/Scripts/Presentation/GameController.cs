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
    /// Split into partials: .Lifecycle (start/teardown/init), .SectorVisuals
    /// (mesh/visuals), .Buildings, .Input.
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

        // SetMapId / StartGame / Initialize / TeardownGame / InitializeGame
        // → GameController.Lifecycle.cs

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

        /// <summary>Active campaign mission evaluation, null in skirmish games.
        /// Set by the bootstrap mission flow, ticked alongside the simulation.</summary>
        public CampaignSystem ActiveCampaign { get; set; }

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
        private BootstrapScene _bootstrap;
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
            // Include inactive — panels deactivate their own GameObject when hidden
            _sectorPanel = FindAnyObjectByType<SectorPanel>(FindObjectsInactive.Include);
            _buildMenu = FindAnyObjectByType<BuildMenu>(FindObjectsInactive.Include);
            _prestigeChart = FindAnyObjectByType<PrestigeChartUI>(FindObjectsInactive.Include);
            _techTreeUI = FindAnyObjectByType<TechTreeUI>(FindObjectsInactive.Include);
            _tradeMapUI = FindAnyObjectByType<TradeMapUI>(FindObjectsInactive.Include);
            _armyPanel = FindAnyObjectByType<ArmyPanel>(FindObjectsInactive.Include);
            _tavernUI = FindAnyObjectByType<TavernUI>(FindObjectsInactive.Include);
            _questPanel = FindAnyObjectByType<UI.QuestPanel>(FindObjectsInactive.Include);
            _bootstrap = FindAnyObjectByType<BootstrapScene>();

            _buildingPlacer = GetComponent<BuildingPlacer>();
            if (_buildingPlacer == null)
                _buildingPlacer = gameObject.AddComponent<BuildingPlacer>();
            _buildingPlacer.OnBuildingPlaced += HandleBuildingPlaced;
        }

        private void Start()
        {
            // Don't auto-initialize when any menu flow exists — otherwise a
            // default game silently starts under the main menu and blocks the
            // player's actual map choice (StartGame is _gameRunning-guarded).
            // Auto-init only in bare test scenes with no menu UI at all.
            bool hasMenuFlow = FindAnyObjectByType<MapSelectionUI>() != null
                || FindAnyObjectByType<UI.GameSetupUI>() != null
                || FindAnyObjectByType<UI.MainMenuUI>() != null;
            if (!hasMenuFlow)
                InitializeGame();
        }

        private void Update()
        {
            if (_runner == null) return; // Not initialized yet (waiting for map selection)

            HandleClick();
            HandleKeyboardToggles();
            _runner.Tick(Time.deltaTime);
            ActiveCampaign?.Tick();
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
