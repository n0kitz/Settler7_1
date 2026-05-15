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

        private string _mapId;
        private int _aiCount = 1;
        private int _maxAiCount = 1;
        private int _vpRequired = 4;
        private int _vpMin = 2;
        private int _vpMax = 10;
        private AIDifficultyLevel _difficulty = AIDifficultyLevel.Normal;
        private AIPersonalityType _personality = AIPersonalityType.Builder;

        /// <summary>Fired when Start Game is clicked. Args: mapId, totalPlayers, vpRequired, difficulty, personality.</summary>
        public event System.Action<string, int, int, AIDifficultyLevel, AIPersonalityType> OnStartGame;

        /// <summary>Fired when the player clicks Back.</summary>
        public event System.Action OnBack;

        public void Show()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
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
            _maxAiCount = maxPlayers - 1;
            _aiCount = _maxAiCount;
            _vpRequired = defaultVP;

            if (_mapNameText != null)
                _mapNameText.text = displayName;

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

        private void RefreshLabels()
        {
            if (_aiCountText != null) _aiCountText.text = $"{_aiCount}";
            if (_vpText != null) _vpText.text = $"{_vpRequired}";
            if (_difficultyText != null) _difficultyText.text = $"{_difficulty}";
            if (_personalityText != null) _personalityText.text = $"{_personality}";
        }

        private void OnStartClicked()
        {
            Hide();
            OnStartGame?.Invoke(_mapId, 1 + _aiCount, _vpRequired, _difficulty, _personality);
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
                "Game Setup", 28, FontStyles.Bold, font);
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

            // AI Opponents row
            var aiCountText = CreateSettingRow(settingsGo.transform, "AI Opponents", font,
                ui.OnAiMinus, ui.OnAiPlus);
            UIFactory.SetField(ui, "_aiCountText", aiCountText);

            // Victory Points row
            var vpText = CreateSettingRow(settingsGo.transform, "Victory Points", font,
                ui.OnVpMinus, ui.OnVpPlus);
            UIFactory.SetField(ui, "_vpText", vpText);

            // AI Difficulty row
            var diffText = CreateSettingRow(settingsGo.transform, "AI Difficulty", font,
                ui.OnDifficultyMinus, ui.OnDifficultyPlus);
            UIFactory.SetField(ui, "_difficultyText", diffText);

            // AI Style (personality) row
            var persText = CreateSettingRow(settingsGo.transform, "AI Style", font,
                ui.OnPersonalityMinus, ui.OnPersonalityPlus);
            UIFactory.SetField(ui, "_personalityText", persText);

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
            CreateButton(buttonsGo.transform, "Back", font,
                new Color(0.4f, 0.3f, 0.25f, 0.9f), ui.OnBackClicked);

            // Start Game button
            CreateButton(buttonsGo.transform, "Start Game", font,
                UIColors.BUTTON_GREEN, ui.OnStartClicked);

            panelGo.SetActive(false);
            return ui;
        }
    }
}
