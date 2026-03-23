using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Settlers.UI;

namespace Settlers.Presentation
{
    /// <summary>
    /// Drop this on any GameObject in an empty scene.
    /// Creates GameController, camera with SettlerCamera, light, and full UI hierarchy.
    /// Flow: MainMenu -> MapSelection -> Play.
    /// </summary>
    public class BootstrapScene : MonoBehaviour
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

        private Transform CreateUI()
        {
            // EventSystem is REQUIRED for all Unity UI click/input handling
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                // Use the new Input System UI module (project uses InputSystem package)
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            var canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Sector Panel (right side)
            CreateSectorPanel(canvasGo.transform);

            // Build Menu (bottom-left)
            BuildMenu.Create(canvasGo.transform, _defaultFont);

            // HUD (top bar)
            HUD.Create(canvasGo.transform, _defaultFont);

            // Prestige Chart (toggle with P key)
            PrestigeChartUI.Create(canvasGo.transform, _defaultFont);

            // Tech Tree (toggle with T key)
            TechTreeUI.Create(canvasGo.transform, _defaultFont);

            // Trade Map (toggle with R key)
            TradeMapUI.Create(canvasGo.transform, _defaultFont);

            // Army Panel (toggle with M key)
            ArmyPanel.Create(canvasGo.transform, _defaultFont);

            // Tavern (toggle with V key)
            TavernUI.Create(canvasGo.transform, _defaultFont);

            // Victory Panel (bottom-right VP tracker + game over overlay)
            var victoryPanel = VictoryPanel.Create(canvasGo.transform, _defaultFont);
            victoryPanel.OnReturnToMenu += OnQuitToMenu;

            // Notifications (bottom-left, fading messages)
            NotificationUI.Create(canvasGo.transform);

            // Minimap (top-left corner)
            MinimapController.Create(canvasGo.transform);

            // Pause Menu (ESC to toggle, starts hidden)
            var pauseMenu = PauseMenuUI.Create(canvasGo.transform, _defaultFont);
            pauseMenu.OnQuitToMenu += OnQuitToMenu;

            // Map Selection (hidden until New Game clicked)
            _mapSelect = MapSelectionUI.Create(canvasGo.transform, _defaultFont);
            _mapSelect.OnMapSelected += OnMapSelected;
            _mapSelect.Hide();

            // Game Setup (hidden until map selected)
            _gameSetup = GameSetupUI.Create(canvasGo.transform, _defaultFont);
            _gameSetup.OnStartGame += OnStartGame;
            _gameSetup.OnBack += OnGameSetupBack;

            // Save Slot UI for main menu Load Game (separate from pause menu's instance)
            _loadSlotUI = SaveSlotUI.Create(canvasGo.transform, _defaultFont);
            _loadSlotUI.OnClosed += OnLoadSlotClosed;

            // Main Menu (shown at startup, on top)
            _mainMenu = MainMenuUI.Create(canvasGo.transform, _defaultFont);
            _mainMenu.OnNewGame += OnNewGameClicked;
            _mainMenu.OnLoadGame += OnLoadGameClicked;
            _mainMenu.Show();

            return canvasGo.transform;
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

        private void CreateSectorPanel(Transform canvasTransform)
        {
            var panelGo = new GameObject("SectorPanel");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0.5f);
            panelRect.anchorMax = new Vector2(1f, 0.5f);
            panelRect.pivot = new Vector2(1f, 0.5f);
            panelRect.anchoredPosition = new Vector2(-10f, 0f);
            panelRect.sizeDelta = new Vector2(250f, 450f);

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 6f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;

            var nameText = CreateTMPLabel(panelGo.transform, "NameText", 20, FontStyles.Bold);
            var ownerText = CreateTMPLabel(panelGo.transform, "OwnerText", 16, FontStyles.Normal);
            var garrisonText = CreateTMPLabel(panelGo.transform, "GarrisonText", 16, FontStyles.Normal);
            var resourcesText = CreateTMPLabel(panelGo.transform, "ResourcesText", 14, FontStyles.Normal);
            var slotsText = CreateTMPLabel(panelGo.transform, "SlotsText", 14, FontStyles.Normal);
            var fortifiedText = CreateTMPLabel(panelGo.transform, "FortifiedText", 14, FontStyles.Bold);
            var buildingsText = CreateTMPLabel(panelGo.transform, "BuildingsText", 11, FontStyles.Normal);
            buildingsText.color = new Color(0.8f, 0.9f, 0.8f);

            // Hotkey hints (small, gray, at bottom)
            var hotkeyHints = CreateTMPLabel(panelGo.transform, "HotkeyHints", 10, FontStyles.Normal);
            hotkeyHints.text = "[F]Food [U]Upgrade [W]Work [G]Army [C]Convert";
            hotkeyHints.color = new Color(0.6f, 0.6f, 0.6f);

            // Feedback text (bold, hidden by default)
            var feedbackText = CreateTMPLabel(panelGo.transform, "FeedbackText", 13, FontStyles.Bold);
            feedbackText.gameObject.SetActive(false);

            var panel = panelGo.AddComponent<SectorPanel>();
            SetField(panel, "_panelRoot", panelGo);
            SetField(panel, "_sectorName", nameText);
            SetField(panel, "_ownerText", ownerText);
            SetField(panel, "_garrisonText", garrisonText);
            SetField(panel, "_resourcesText", resourcesText);
            SetField(panel, "_buildSlotsText", slotsText);
            SetField(panel, "_fortifiedText", fortifiedText);
            SetField(panel, "_buildingsText", buildingsText);
            SetField(panel, "_hotkeyHints", hotkeyHints);
            SetField(panel, "_feedbackText", feedbackText);
        }

        private TextMeshProUGUI CreateTMPLabel(Transform parent, string name,
            float fontSize, FontStyles style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(226f, fontSize + 10f);

            var layoutElem = go.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = fontSize + 10f;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;

            var font = _defaultFont != null ? _defaultFont : UI.UIFactory.GetDefaultFont();
            if (font != null)
                tmp.font = font;

            return tmp;
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

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
