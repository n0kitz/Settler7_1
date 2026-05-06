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

        private void Awake()
        {
            _defaultFont = LoadDefaultTMPFont();
            CreateLight();
            CreateCamera();
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
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void Start()
        {
            // Minimal valid SectorGraph: 1 sector, player 0 owns it
            var graph = new SectorGraph();
            graph.AddSector(new Sector(
                id: 0, name: "Home", ownerId: 0,
                garrisonStrength: 0, isFortified: false,
                resourceNodes: new List<ResourceNodeType>(),
                buildSlots: 4));

            // Construct GameState with real graph
            var state = new GameState(graph, playerCount: 1,
                constructionBaseTime: 10f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "bootstrap");

            // Create runner with all systems disabled
            var runner = new SimulationRunner(state);
            runner.DisableAll();
            runner.OnTickLog = tick => Debug.Log($"SimulationRunner tick #{tick}");

            // Initialize GameController with pre-built state
            GameController.Instance.Initialize(state, runner);

            Debug.Log("GameController initialized successfully");
        }

        private void OnNewGameClicked()
        {
            _mainMenu.Hide();
            _mapSelect.Show();
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

        private void OnStartGame(string mapId, int playerCount, int vpRequired)
        {
            if (GameController.Instance != null)
                GameController.Instance.StartGame(mapId, playerCount, vpRequired);
        }

        private void OnGameSetupBack()
        {
            _mapSelect.Show();
        }

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
