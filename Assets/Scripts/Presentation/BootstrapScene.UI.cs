using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Settlers.UI;

namespace Settlers.Presentation
{
    public partial class BootstrapScene
    {
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
            BuildMenuFactory.Create(canvasGo.transform, _defaultFont);

            // HUD (top bar)
            HUD.Create(canvasGo.transform, _defaultFont);

            // VP badge strip (below HUD)
            VPRingUI.Create(canvasGo.transform, _defaultFont);

            // Conquest reward modal (§14.3, Critical Rule #10)
            RewardModalUI.Create(canvasGo.transform, _defaultFont);

            // Bottom action bar (§14.8)
            ActionBarUI.Create(canvasGo.transform, _defaultFont);

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

            // Quest Panel (toggle with Q key)
            QuestPanel.Create(canvasGo.transform, _defaultFont);

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
            _gameSetup.OnStartGame += (map, players, vp, diff, pers, sp, vr) =>
                OnStartGame(map, players, vp, diff, pers, sp, vr);
            _gameSetup.OnBack += OnGameSetupBack;

            // Save Slot UI for main menu Load Game (separate from pause menu's instance)
            _loadSlotUI = SaveSlotUI.Create(canvasGo.transform, _defaultFont);
            _loadSlotUI.OnClosed += OnLoadSlotClosed;

            // Tutorial overlay (hidden until tutorial map starts)
            TutorialOverlayUI.Create(canvasGo.transform, _defaultFont);

            // Map Editor screens (hidden until "Map Editor" clicked)
            UI.MapEditorUI.Create(canvasGo.transform, _defaultFont);
            UI.SectorPropertyPanel.Create(canvasGo.transform, _defaultFont);

            // Settings (shown from Main Menu or Pause Menu, starts hidden)
            _settingsUI = UI.SettingsUI.Create(canvasGo.transform, _defaultFont);
            _settingsUI.Initialize();

            // Achievements panel (toggle with K key)
            _achievementsPanel = UI.AchievementsPanel.Create(canvasGo.transform, _defaultFont);
            _achievementToast  = UI.AchievementToast.Create(canvasGo.transform, _defaultFont);

            // Diplomacy panel (toggle with J key)
            _diplomacyPanel = UI.DiplomacyPanel.Create(canvasGo.transform, _defaultFont);

            // Post-game summary (shown on game over)
            _postGameSummary = UI.PostGameSummaryUI.Create(canvasGo.transform, _defaultFont);
            _postGameSummary.OnReturnToMenu += OnQuitToMenu;
            _postGameSummary.OnPlayAgain    += OnPlayAgainClicked;

            // Hall of Fame (shown from main menu)
            _hallOfFame = UI.HallOfFameUI.Create(canvasGo.transform, _defaultFont);

            // Achievement system (pure C# — lives outside MonoBehaviours)
            _achievementSystem = new Simulation.AchievementSystem();
            _playerStats       = new Simulation.PlayerStats();

            // Campaign screens
            _campaignSelect = CampaignSelectionUI.Create(canvasGo.transform, _defaultFont);
            _campaignSelect.OnMissionSelected += OnCampaignMissionSelected;
            _campaignSelect.OnBack += OnCampaignBack;

            _missionBriefing = MissionBriefingUI.Create(canvasGo.transform, _defaultFont);
            _missionBriefing.OnStart += OnMissionStart;
            _missionBriefing.OnBack += OnMissionBriefingBack;

            MissionCompleteUI.Create(canvasGo.transform, _defaultFont);

            // Main Menu (shown at startup, on top)
            _mainMenu = MainMenuUI.Create(canvasGo.transform, _defaultFont);
            _mainMenu.OnNewGame += OnNewGameClicked;
            _mainMenu.OnCampaign += OnCampaignClicked;
            _mainMenu.OnTutorial += OnTutorialClicked;
            _mainMenu.OnLoadGame += OnLoadGameClicked;
            _mainMenu.OnMapEditor += OnMapEditorClicked;
            _mainMenu.OnSettings += OnSettingsClicked;
            _mainMenu.OnAchievements += OnAchievementsClicked;
            _mainMenu.OnHallOfFame   += OnHallOfFameClicked;
            _mainMenu.Show();
            pauseMenu.OnSettings += OnSettingsClicked;
            pauseMenu.OnAchievements += OnAchievementsClicked;

            return canvasGo.transform;
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
            hotkeyHints.text = "[F]Food [U]Upgrade [W]Work [G]Army [C]Convert [N]Fortify";
            hotkeyHints.color = UI.UIColors.TEXT_GRAY_DIM;

            // Feedback text (bold, hidden by default)
            var feedbackText = CreateTMPLabel(panelGo.transform, "FeedbackText", 13, FontStyles.Bold);
            feedbackText.gameObject.SetActive(false);

            var panel = panelGo.AddComponent<SectorPanel>();
            UIFactory.SetField(panel, "_panelRoot", panelGo);
            UIFactory.SetField(panel, "_sectorName", nameText);
            UIFactory.SetField(panel, "_ownerText", ownerText);
            UIFactory.SetField(panel, "_garrisonText", garrisonText);
            UIFactory.SetField(panel, "_resourcesText", resourcesText);
            UIFactory.SetField(panel, "_buildSlotsText", slotsText);
            UIFactory.SetField(panel, "_fortifiedText", fortifiedText);
            UIFactory.SetField(panel, "_buildingsText", buildingsText);
            UIFactory.SetField(panel, "_hotkeyHints", hotkeyHints);
            UIFactory.SetField(panel, "_feedbackText", feedbackText);
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
    }
}
