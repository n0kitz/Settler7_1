using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Game setup screen shown after map selection.
    /// Lets the player configure AI count, difficulty, personality, and VP before starting.
    /// </summary>
    public partial class GameSetupUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _mapNameText;
        [SerializeField] private TextMeshProUGUI _aiCountText;
        [SerializeField] private TextMeshProUGUI _vpText;
        [SerializeField] private TextMeshProUGUI _difficultyText;
        [SerializeField] private TextMeshProUGUI _personalityText;
        [SerializeField] private TextMeshProUGUI _startingProfileText;
        [SerializeField] private TextMeshProUGUI _victoryRulesText;

        private string _mapId;
        private string _mapDisplayFallback = "";
        private int _aiCount = 1;
        private int _maxAiCount = 1;
        private int _vpRequired = 4;
        private int _vpMin = 2;
        private int _vpMax = 10;
        private AIDifficultyLevel _difficulty = AIDifficultyLevel.Normal;
        private AIPersonalityType _personality = AIPersonalityType.Builder;
        private StartingProfileType _startingProfile = StartingProfileType.Default;
        private VictoryRuleSetType _victoryRules = VictoryRuleSetType.Standard;

        /// <summary>Fired when Start Game is clicked.</summary>
        public event System.Action<string, int, int, AIDifficultyLevel, AIPersonalityType,
            StartingProfileType, VictoryRuleSetType> OnStartGame;

        /// <summary>Fired when the player clicks Back.</summary>
        public event System.Action OnBack;

        public void Show()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
            RefreshLocaleTexts();
            RefreshLabels();
        }

        public void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        /// <summary>Configure the setup screen for a specific map.</summary>
        public void SetMap(string mapId, string displayName, int maxPlayers, int defaultVP)
        {
            _mapId = mapId;
            _mapDisplayFallback = displayName;
            _maxAiCount = maxPlayers - 1;
            _aiCount = _maxAiCount;
            _vpRequired = defaultVP;

            if (_mapNameText != null)
                _mapNameText.text = LocalizedNames.Map(mapId, displayName);

            RefreshLabels();
        }

        private void OnAiMinus()
        {
            _aiCount = Mathf.Max(1, _aiCount - 1);
            RefreshLabels();
        }

        private void OnAiPlus()
        {
            _aiCount = Mathf.Min(_maxAiCount, _aiCount + 1);
            RefreshLabels();
        }

        private void OnVpMinus()
        {
            _vpRequired = Mathf.Max(_vpMin, _vpRequired - 1);
            RefreshLabels();
        }

        private void OnVpPlus()
        {
            _vpRequired = Mathf.Min(_vpMax, _vpRequired + 1);
            RefreshLabels();
        }

        private void OnDifficultyMinus()
        {
            _difficulty = _difficulty == AIDifficultyLevel.Easy
                ? AIDifficultyLevel.Hard : _difficulty - 1;
            RefreshLabels();
        }

        private void OnDifficultyPlus()
        {
            _difficulty = _difficulty == AIDifficultyLevel.Hard
                ? AIDifficultyLevel.Easy : _difficulty + 1;
            RefreshLabels();
        }

        private void OnPersonalityMinus()
        {
            _personality = _personality == AIPersonalityType.Builder
                ? AIPersonalityType.Merchant : _personality - 1;
            RefreshLabels();
        }

        private void OnPersonalityPlus()
        {
            _personality = _personality == AIPersonalityType.Merchant
                ? AIPersonalityType.Builder : _personality + 1;
            RefreshLabels();
        }

        private void OnStartingProfileMinus()
        {
            _startingProfile = _startingProfile == StartingProfileType.Default
                ? StartingProfileType.Lean : _startingProfile - 1;
            RefreshLabels();
        }

        private void OnStartingProfilePlus()
        {
            _startingProfile = _startingProfile == StartingProfileType.Lean
                ? StartingProfileType.Default : _startingProfile + 1;
            RefreshLabels();
        }

        private void OnVictoryRulesMinus()
        {
            _victoryRules = _victoryRules == VictoryRuleSetType.Standard
                ? VictoryRuleSetType.NoConquest : _victoryRules - 1;
            RefreshLabels();
        }

        private void OnVictoryRulesPlus()
        {
            _victoryRules = _victoryRules == VictoryRuleSetType.NoConquest
                ? VictoryRuleSetType.Standard : _victoryRules + 1;
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (_aiCountText != null) _aiCountText.text = $"{_aiCount}";
            if (_vpText != null) _vpText.text = $"{_vpRequired}";
            if (_difficultyText != null)
                _difficultyText.text = LocalizedNames.Difficulty(_difficulty);
            if (_personalityText != null)
                _personalityText.text = LocalizedNames.Personality(_personality);
            if (_startingProfileText != null)
                _startingProfileText.text = LocalizedNames.StartingProfile(_startingProfile);
            if (_victoryRulesText != null)
                _victoryRulesText.text = LocalizedNames.VictoryRules(_victoryRules);
        }

        private void OnStartClicked()
        {
            Hide();
            OnStartGame?.Invoke(_mapId, 1 + _aiCount, _vpRequired,
                _difficulty, _personality, _startingProfile, _victoryRules);
        }

        private void OnBackClicked()
        {
            Hide();
            OnBack?.Invoke();
        }

        /// <summary>Create the game setup UI programmatically.</summary>
        public static GameSetupUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("GameSetupUI");
            panelGo.transform.SetParent(canvasTransform, false);

            // Centered panel
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.25f, 0.2f);
            panelRect.anchorMax = new Vector2(0.75f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.06f, 0.07f, 0.09f, 0.96f);

            // Title
            var titleText = UIFactory.CreateLabel(panelGo.transform, "Title",
                L.Get("ui.setup.title"), 28, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -16f);
            titleRect.sizeDelta = new Vector2(0f, 36f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = UIColors.TEXT_HEADER_GOLD;

            // Map name
            var mapNameText = UIFactory.CreateLabel(panelGo.transform, "MapName",
                "", 18, FontStyles.Italic, font);
            var mapRect = mapNameText.GetComponent<RectTransform>();
            mapRect.anchorMin = new Vector2(0f, 1f);
            mapRect.anchorMax = new Vector2(1f, 1f);
            mapRect.pivot = new Vector2(0.5f, 1f);
            mapRect.anchoredPosition = new Vector2(0f, -56f);
            mapRect.sizeDelta = new Vector2(0f, 24f);
            mapNameText.alignment = TextAlignmentOptions.Center;
            mapNameText.color = new Color(0.7f, 0.7f, 0.65f);

            // Settings container
            var settingsGo = new GameObject("Settings");
            settingsGo.transform.SetParent(panelGo.transform, false);
            var settingsRect = settingsGo.AddComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(0.15f, 0.35f);
            settingsRect.anchorMax = new Vector2(0.85f, 0.75f);
            settingsRect.offsetMin = Vector2.zero;
            settingsRect.offsetMax = Vector2.zero;

            var settingsLayout = settingsGo.AddComponent<VerticalLayoutGroup>();
            settingsLayout.spacing = 16f;
            settingsLayout.childForceExpandWidth = true;
            settingsLayout.childForceExpandHeight = false;
            settingsLayout.childAlignment = TextAnchor.UpperCenter;

            var ui = panelGo.AddComponent<GameSetupUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);
            UIFactory.SetField(ui, "_mapNameText", mapNameText);
            ui.RegisterLocaleLabel(titleText, "ui.setup.title");

            var aiCountText = CreateSettingRow(settingsGo.transform, ui,
                "ui.setup.ai_opponents", font, ui.OnAiMinus, ui.OnAiPlus);
            UIFactory.SetField(ui, "_aiCountText", aiCountText);

            var vpText = CreateSettingRow(settingsGo.transform, ui,
                "ui.setup.victory_points", font, ui.OnVpMinus, ui.OnVpPlus);
            UIFactory.SetField(ui, "_vpText", vpText);

            var diffText = CreateSettingRow(settingsGo.transform, ui,
                "ui.setup.difficulty", font, ui.OnDifficultyMinus, ui.OnDifficultyPlus);
            UIFactory.SetField(ui, "_difficultyText", diffText);

            var persText = CreateSettingRow(settingsGo.transform, ui,
                "ui.setup.ai_style", font, ui.OnPersonalityMinus, ui.OnPersonalityPlus);
            UIFactory.SetField(ui, "_personalityText", persText);

            var spText = CreateSettingRow(settingsGo.transform, ui,
                "ui.setup.resources", font, ui.OnStartingProfileMinus, ui.OnStartingProfilePlus);
            UIFactory.SetField(ui, "_startingProfileText", spText);

            var vrText = CreateSettingRow(settingsGo.transform, ui,
                "ui.setup.victory_rules", font, ui.OnVictoryRulesMinus, ui.OnVictoryRulesPlus);
            UIFactory.SetField(ui, "_victoryRulesText", vrText);

            // Buttons container
            var buttonsGo = new GameObject("Buttons");
            buttonsGo.transform.SetParent(panelGo.transform, false);
            var buttonsRect = buttonsGo.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.15f, 0.08f);
            buttonsRect.anchorMax = new Vector2(0.85f, 0.25f);
            buttonsRect.offsetMin = Vector2.zero;
            buttonsRect.offsetMax = Vector2.zero;

            var buttonsLayout = buttonsGo.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 20f;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.childForceExpandHeight = true;
            buttonsLayout.childAlignment = TextAnchor.MiddleCenter;

            // Back button
            CreateButton(buttonsGo.transform, ui, "ui.setup.back", font,
                new Color(0.4f, 0.3f, 0.25f, 0.9f), ui.OnBackClicked);

            // Start Game button
            CreateButton(buttonsGo.transform, ui, "ui.setup.start", font,
                UIColors.BUTTON_GREEN, ui.OnStartClicked);

            panelGo.SetActive(false);
            return ui;
        }
    }
}
