using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        public void StartGame(string mapId, int playerCount, int vpRequired)
        {
            _mapId = mapId;
            _playerCountOverride = playerCount;
            _vpRequiredOverride = vpRequired;
            InitializeGame();
        }

        private int _playerCountOverride;
        private int _vpRequiredOverride;

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
        private Camera _mainCamera;
        private Material _buildingMaterial;
        private WorkerManager _workerManager;
        private CarrierManager _carrierManager;
        private ArmyViewManager _armyViewManager;

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

            _buildingPlacer = GetComponent<BuildingPlacer>();
            if (_buildingPlacer == null)
                _buildingPlacer = gameObject.AddComponent<BuildingPlacer>();
            _buildingPlacer.OnBuildingPlaced += HandleBuildingPlaced;
        }

        private bool _initialized;

        private void Start()
        {
            // Don't auto-initialize — wait for MapSelectionUI to call SetMapId().
            // If no MapSelectionUI exists (e.g., testing), init with default map.
            if (FindAnyObjectByType<MapSelectionUI>() == null)
                InitializeGame();
        }

        private void InitializeGame()
        {
            if (_initialized) return;
            _initialized = true;

            var mapInfo = MapFactory.CreateMap(_mapId);
            int playerCount = _playerCountOverride > 0 ? _playerCountOverride : mapInfo.PlayerCount;
            int vpRequired = _vpRequiredOverride > 0 ? _vpRequiredOverride : mapInfo.VPRequired;
            State = new GameState(mapInfo.Graph, playerCount: playerCount,
                _constructionBaseTime, _carrierMaxItems, vpRequired, _mapId);
            _runner = new SimulationRunner(State);

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

            if (_buildMenu != null)
                _buildMenu.OnBuildingSelected += HandleBuildMenuSelection;
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
            SyncRoads();
        }

        private void OnDestroy()
        {
            if (_buildingPlacer != null)
                _buildingPlacer.OnBuildingPlaced -= HandleBuildingPlaced;
            if (_buildMenu != null)
                _buildMenu.OnBuildingSelected -= HandleBuildMenuSelection;
        }

        // Building placement + work yards → GameController.Buildings.cs

        /// <summary>Get the number of buildings in a sector.</summary>
        public int GetBuildingCountInSector(int sectorId)
        {
            return Construction.GetBuildingCountInSector(sectorId);
        }

        /// <summary>Get a player's resource inventory.</summary>
        public PlayerResources GetPlayerResources(int playerId)
        {
            return State?.PlayerResources.TryGetValue(playerId, out var res) == true ? res : null;
        }

        // --- Sector Selection ---

        private void HandleClick()
        {
            if (_buildingPlacer != null && _buildingPlacer.IsPlacing) return;
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            // Don't raycast into the 3D world when clicking on UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                var view = hit.collider.GetComponentInParent<SectorView>();
                if (view != null) SelectSector(view);
            }
        }

        private void SelectSector(SectorView view)
        {
            if (_selectedSector != null) _selectedSector.Deselect();
            _selectedSector = view;
            _selectedSector.Select();

            var sector = Graph.GetSector(view.SectorId);
            int buildingCount = Construction.GetBuildingCountInSector(view.SectorId);
            Debug.Log($"Selected: {sector.Name} (ID:{sector.Id}, " +
                $"Owner:{sector.OwnerId}, Buildings:{buildingCount}/{sector.BuildSlots})");

            if (_sectorPanel != null)
                _sectorPanel.ShowSector(view.SectorId);
        }

        // --- Public API ---

        /// <summary>Award prestige points to a player.</summary>
        public void AwardPrestige(int playerId, int points)
        {
            State?.Prestige.AwardPoints(playerId, points);
        }

        /// <summary>Try to upgrade a building.</summary>
        public bool TryUpgradeBuilding(int buildingId)
        {
            return State?.Upgrades.TryStartUpgrade(buildingId) ?? false;
        }

        /// <summary>Try to build a fortification in a sector (costs 10 stone, prestige-gated).</summary>
        public bool TryBuildFortification(int sectorId, int playerId)
        {
            return State?.Fortification.StartFortification(playerId, sectorId) ?? false;
        }

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
