using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Settlers.Simulation;
using Settlers.UI;

namespace Settlers.Presentation
{
    /// <summary>
    /// Drop this on any GameObject in an empty scene.
    /// Creates GameController, camera with SettlerCamera, light, and full UI hierarchy.
    /// Flow: MainMenu -> MapSelection -> Play.
    /// </summary>
    public partial class BootstrapScene : MonoBehaviour
    {
        private TMP_FontAsset _defaultFont;
        private MainMenuUI _mainMenu;
        private MapSelectionUI _mapSelect;
        private GameSetupUI _gameSetup;
        private SaveSlotUI _loadSlotUI;
        private UI.CampaignSelectionUI _campaignSelect;
        private UI.MissionBriefingUI _missionBriefing;
        private UI.SettingsUI _settingsUI;
        private UI.AchievementsPanel _achievementsPanel;
        private UI.AchievementToast _achievementToast;
        private Simulation.AchievementSystem _achievementSystem;
        private Simulation.PlayerStats _playerStats;
        private Simulation.DiplomacySystem _diplomacySystem;
        private UI.DiplomacyPanel _diplomacyPanel;
        private UI.PostGameSummaryUI _postGameSummary;
        private UI.HallOfFameUI _hallOfFame;
        private float _gameStartTime;
        private Simulation.CampaignProgress _campaignProgress;
        private Simulation.Mission _pendingMission;
        private MapEditorController _mapEditorController;

        // Last started game — replayed by the post-game "Play Again" button
        private string _lastMapId;
        private int _lastPlayerCount;
        private int _lastVPRequired;
        private Simulation.AIDifficultyLevel _lastDifficulty;
        private Simulation.AIPersonalityType _lastPersonality;
        private Simulation.GameRules _lastRules;

        /// <summary>Start a game and remember its settings for Play Again.</summary>
        private void StartTrackedGame(string mapId, int playerCount, int vpRequired,
            Simulation.AIDifficultyLevel difficulty = Simulation.AIDifficultyLevel.Normal,
            Simulation.AIPersonalityType personality = Simulation.AIPersonalityType.Builder,
            Simulation.GameRules rules = null)
        {
            if (GameController.Instance == null) return;
            _lastMapId = mapId;
            _lastPlayerCount = playerCount;
            _lastVPRequired = vpRequired;
            _lastDifficulty = difficulty;
            _lastPersonality = personality;
            _lastRules = rules;
            _gameStartTime = Time.realtimeSinceStartup;
            GameController.Instance.StartGame(mapId, playerCount, vpRequired,
                difficulty, personality, rules);
            // Every StartGame creates a fresh EventBus — subscriptions made at
            // bootstrap (or for the previous match) die with the old bus, so
            // re-wire or the post-game summary/toasts/VFX never fire again
            WireAchievements();
            WireDiplomacy();
            WireVFX();
        }

        private void OnPlayAgainClicked()
        {
            if (string.IsNullOrEmpty(_lastMapId)) { OnQuitToMenu(); return; }
            StartTrackedGame(_lastMapId, _lastPlayerCount, _lastVPRequired,
                _lastDifficulty, _lastPersonality, _lastRules);
        }

        private void Awake()
        {
            // Keep simulating when the editor/window loses focus
            Application.runInBackground = true;
            // Load the string table before any UI is built so L.Get resolves
            Simulation.L.SetLocale(Simulation.SettingsPersistence.Load().Language);
            _defaultFont = LoadDefaultTMPFont();
            CreateLight();
            CreateCamera();
            SetupAtmosphere();
            var canvas = CreateUI();
            CreateGameController();
        }

        private void CreateGameController()
        {
            if (GameController.Instance == null)
            {
                var gc = new GameObject("GameController");
                gc.AddComponent<GameController>();
                gc.AddComponent<SaveLoadController>();
            }

            if (AudioManager.Instance == null)
            {
                var audioGo = new GameObject("AudioManager");
                audioGo.AddComponent<AudioManager>();
            }
        }

        private void CreateCamera()
        {
            var camGo = Camera.main != null
                ? Camera.main.gameObject
                : new GameObject("Main Camera");

            if (camGo.GetComponent<Camera>() == null)
            {
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.15f, 0.18f, 0.12f);
            }
            camGo.tag = "MainCamera";

            if (camGo.GetComponent<SettlerCamera>() == null)
                camGo.AddComponent<SettlerCamera>();
        }

        private void CreateLight()
        {
            if (FindAnyObjectByType<Light>() != null)
                return;

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.75f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void Start()
        {
            var graph = new SectorGraph();
            graph.AddSector(new Sector(
                id: 0, name: "Home", ownerId: 0,
                garrisonStrength: 0, isFortified: false,
                resourceNodes: new List<ResourceNodeType>(),
                buildSlots: 4));

            var state = new GameState(graph, playerCount: 1,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "bootstrap");

            var runner = new SimulationRunner(state);
            runner.DisableAll();
            runner.OnTickLog = tick => Debug.Log($"SimulationRunner tick #{tick}");

            GameController.Instance.Initialize(state, runner);
            WireAchievements();
            WireDiplomacy();
            WireVFX();
            Debug.Log("GameController initialized successfully");
        }

        private void WireAchievements()
        {
            var bus = GameController.Instance?.Events;
            if (bus == null) return;
            _achievementSystem?.Initialize(bus);
            _playerStats?.Initialize(bus);
            _achievementsPanel?.Bind(_achievementSystem, _playerStats);
            bus.Subscribe<AchievementUnlockedEvent>(e =>
                _achievementToast?.Show(e.Name));
            bus.Subscribe<GameOverEvent>(e => OnGameOver(e.WinnerId));
            _gameStartTime = Time.realtimeSinceStartup;
        }

        private void WireDiplomacy()
        {
            var state = GameController.Instance?.State;
            if (state == null) return;
            _diplomacySystem = new Simulation.DiplomacySystem(state);
            _diplomacyPanel?.Bind(_diplomacySystem, _defaultFont);
        }

        private void OnNewGameClicked()
        {
            _mainMenu.Hide();
            _mapSelect.Show();
        }

        private void OnTutorialClicked()
        {
            _mainMenu.Hide();
            // Start tutorial map directly — no map/setup screens
            StartTrackedGame("tutorial", playerCount: 1, vpRequired: 3);
        }

        private void OnCampaignClicked()
        {
            _mainMenu.Hide();
            _campaignProgress ??= Simulation.CampaignProgress.Load();
            _campaignSelect?.Show(_campaignProgress);
        }

        private void OnCampaignMissionSelected(Simulation.Mission mission)
        {
            _pendingMission = mission;
            _campaignSelect?.Hide();
            _missionBriefing?.Show(mission);
        }

        private void OnMissionStart(Simulation.Mission mission)
        {
            StartTrackedGame(mission.MapId, mission.PlayerCount, mission.VPRequired);
        }

        private void OnMissionBriefingBack()
        {
            _missionBriefing?.Hide();
            _campaignSelect?.Show(_campaignProgress);
        }

        private void OnCampaignBack()
        {
            _campaignSelect?.Hide();
            _mainMenu.Show();
        }

        private void OnMapEditorClicked()
        {
            _mainMenu.Hide();
            if (_mapEditorController == null)
            {
                var go = new GameObject("MapEditorController");
                _mapEditorController = go.AddComponent<MapEditorController>();
            }
            var editorUI = FindAnyObjectByType<UI.MapEditorUI>();
            var propPanel = FindAnyObjectByType<UI.SectorPropertyPanel>();
            _mapEditorController.OnEditorClosed += OnMapEditorClosed;
            _mapEditorController.OnPlaytestRequested += OnMapEditorPlaytest;
            _mapEditorController.Activate(editorUI, propPanel);
        }

        private void OnMapEditorClosed()
        {
            if (_mapEditorController != null)
            {
                _mapEditorController.OnEditorClosed -= OnMapEditorClosed;
                _mapEditorController.OnPlaytestRequested -= OnMapEditorPlaytest;
            }
            _mainMenu.Show();
        }

        private void OnMapEditorPlaytest(Simulation.MapEditorState editorState)
        {
            if (GameController.Instance == null) return;
            var graph = editorState.ToSectorGraph();
            var rules = Simulation.GameRules.Default;
            // Inline-launch the editor map as a skirmish
            GameController.Instance.StartGame("editor_custom",
                editorState.MaxPlayers, editorState.DefaultVP,
                Simulation.AIDifficultyLevel.Normal,
                Simulation.AIPersonalityType.Builder, rules);
        }

        private void OnSettingsClicked() => _settingsUI?.Show();

        private void OnAchievementsClicked() => _achievementsPanel?.Show();
        private void OnHallOfFameClicked()   => _hallOfFame?.Show();

        private void OnGameOver(int winnerId)
        {
            var state = GameController.Instance?.State;
            if (state == null || _playerStats == null) return;
            float duration = Time.realtimeSinceStartup - _gameStartTime;
            var result = Simulation.MatchResult.From(state, _playerStats, winnerId, duration);
            Simulation.MatchHistoryPersistence.Append(result);
            _postGameSummary?.Show(result);
        }

        private void OnLoadGameClicked()
        {
            _mainMenu.Hide();

            // Need a game initialized before loading — start with default map
            if (GameController.Instance != null && GameController.Instance.State == null)
                GameController.Instance.SetMapId("test_valley");

            _loadSlotUI.Show(SaveSlotUI.Mode.Load);
        }

        private void OnLoadSlotClosed()
        {
            // If closed without loading, return to main menu
            _mainMenu.Show();
        }

        private void OnQuitToMenu()
        {
            _mapSelect.Hide();
            _gameSetup.Hide();
            _mainMenu.Show();
        }

        private void OnMapSelected(string mapId)
        {
            _mapSelect.Hide();
            var mapInfo = Simulation.MapFactory.CreateMap(mapId);
            _gameSetup.SetMap(mapId, mapInfo.DisplayName, mapInfo.PlayerCount, mapInfo.VPRequired);
            _gameSetup.Show();
        }

        private void OnStartGame(string mapId, int playerCount, int vpRequired,
            Simulation.AIDifficultyLevel difficulty, Simulation.AIPersonalityType personality,
            Simulation.StartingProfileType startingProfile, Simulation.VictoryRuleSetType victoryRules)
        {
            var rules = new Simulation.GameRules(
                Simulation.StartingProfile.Get(startingProfile),
                Simulation.VictoryRuleSet.Get(victoryRules));
            StartTrackedGame(mapId, playerCount, vpRequired,
                difficulty, personality, rules);
        }

        private void OnGameSetupBack() => _mapSelect.Show();

        private TMP_FontAsset LoadDefaultTMPFont()
        {
            // Try direct load first — most reliable, works before TMP_Settings initializes
            var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
                return font;

            // Fallback: TMP default font (may be null if TMP hasn't initialized yet)
            if (TMP_Settings.defaultFontAsset != null)
                return TMP_Settings.defaultFontAsset;

            Debug.LogWarning("[BootstrapScene] No TMP font asset found. " +
                "Import TMP Essential Resources via Window > TextMeshPro > Import TMP Essential Resources.");
            return null;
        }

    }
}
