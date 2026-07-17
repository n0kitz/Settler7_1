using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Main menu shown on startup. Provides New Game, Load Game, and Quit buttons.
    /// New Game flow: MainMenu -> MapSelectionUI -> Play.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _versionText;

        private readonly List<(TextMeshProUGUI label, string key)> _buttonLabels = new();

        /// <summary>Fired when the player clicks New Game.</summary>
        public event System.Action OnNewGame;

        /// <summary>Fired when the player clicks Campaign.</summary>
        public event System.Action OnCampaign;

        /// <summary>Fired when the player clicks Tutorial.</summary>
        public event System.Action OnTutorial;

        /// <summary>Fired when the player clicks Load Game.</summary>
        public event System.Action OnLoadGame;

        /// <summary>Fired when the player clicks Map Editor.</summary>
        public event System.Action OnMapEditor;

        /// <summary>Fired when the player clicks Settings.</summary>
        public event System.Action OnSettings;

        /// <summary>Fired when the player clicks Achievements.</summary>
        public event System.Action OnAchievements;

        /// <summary>Fired when the player clicks Hall of Fame.</summary>
        public event System.Action OnHallOfFame;

        public void Show()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
            RefreshLocaleTexts();
        }

        private void RefreshLocaleTexts()
        {
            foreach (var (label, key) in _buttonLabels)
                if (label != null) label.text = L.Get(key);
        }

        public void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        private void OnNewGameClicked()
        {
            Hide();
            OnNewGame?.Invoke();
        }

        private void OnCampaignClicked()
        {
            Hide();
            OnCampaign?.Invoke();
        }

        private void OnTutorialClicked()
        {
            Hide();
            OnTutorial?.Invoke();
        }

        private void OnLoadGameClicked()
        {
            Hide();
            OnLoadGame?.Invoke();
        }

        private void OnMapEditorClicked()
        {
            Hide();
            OnMapEditor?.Invoke();
        }

        private void OnSettingsClicked()     => OnSettings?.Invoke();
        private void OnAchievementsClicked() => OnAchievements?.Invoke();
        private void OnHallOfFameClicked()   => OnHallOfFame?.Invoke();

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Create the main menu UI programmatically.</summary>
        public static MainMenuUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("MainMenuUI");
            panelGo.transform.SetParent(canvasTransform, false);

            // Full-screen dark background
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.06f, 0.08f, 0.97f);

            // Title
            var titleText = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Die Siedler VII", 42, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -60f);
            titleRect.sizeDelta = new Vector2(500f, 50f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = UIColors.TEXT_HEADER_GOLD;

            // Subtitle
            var subtitleText = UIFactory.CreateLabel(panelGo.transform, "Subtitle",
                "Paths to a Kingdom", 18, FontStyles.Italic, font);
            var subRect = subtitleText.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 1f);
            subRect.anchorMax = new Vector2(0.5f, 1f);
            subRect.pivot = new Vector2(0.5f, 1f);
            subRect.anchoredPosition = new Vector2(0f, -115f);
            subRect.sizeDelta = new Vector2(400f, 28f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.color = new Color(0.7f, 0.7f, 0.65f);

            // Button container (centered)
            var buttonContainer = new GameObject("Buttons");
            buttonContainer.transform.SetParent(panelGo.transform, false);
            var btnContainerRect = buttonContainer.AddComponent<RectTransform>();
            btnContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnContainerRect.pivot = new Vector2(0.5f, 0.5f);
            btnContainerRect.anchoredPosition = new Vector2(0f, -20f);
            btnContainerRect.sizeDelta = new Vector2(280f, 200f);

            var layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.padding = new RectOffset(0, 0, 0, 0);

            var ui = panelGo.AddComponent<MainMenuUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);
            UIFactory.SetField(ui, "_titleText", titleText);

            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.new_game", font,
                UIColors.BUTTON_GREEN, ui.OnNewGameClicked);
            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.campaign", font,
                new Color(0.6f, 0.4f, 0.1f), ui.OnCampaignClicked);
            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.tutorial", font,
                new Color(0.3f, 0.5f, 0.7f), ui.OnTutorialClicked);
            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.load_game", font,
                UIColors.BUTTON_BLUE, ui.OnLoadGameClicked);
            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.map_editor", font,
                new Color(0.35f, 0.28f, 0.5f), ui.OnMapEditorClicked);
            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.achievements", font,
                new Color(0.4f, 0.3f, 0.1f), ui.OnAchievementsClicked);
            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.hall_of_fame", font,
                new Color(0.5f, 0.38f, 0.08f), ui.OnHallOfFameClicked);
            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.settings", font,
                new Color(0.28f, 0.28f, 0.35f), ui.OnSettingsClicked);
            CreateMenuButton(buttonContainer.transform, ui, "ui.menu.quit", font,
                UIColors.BUTTON_RED, ui.OnQuitClicked);

            // Version text (bottom)
            var versionText = UIFactory.CreateLabel(panelGo.transform, "Version",
                "v1.0-rc", 12, FontStyles.Normal, font);
            var verRect = versionText.GetComponent<RectTransform>();
            verRect.anchorMin = new Vector2(0.5f, 0f);
            verRect.anchorMax = new Vector2(0.5f, 0f);
            verRect.pivot = new Vector2(0.5f, 0f);
            verRect.anchoredPosition = new Vector2(0f, 15f);
            verRect.sizeDelta = new Vector2(300f, 20f);
            versionText.alignment = TextAlignmentOptions.Center;
            versionText.color = new Color(0.4f, 0.4f, 0.4f);
            UIFactory.SetField(ui, "_versionText", versionText);

            return ui;
        }

        private static void CreateMenuButton(Transform parent, MainMenuUI ui, string key,
            TMP_FontAsset font, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIFactory.CreateButton(parent, L.Get(key), font, color, onClick,
                new Vector2(280f, 48f), 20f);
            ui._buttonLabels.Add((btn.GetComponentInChildren<TextMeshProUGUI>(), key));
        }
    }
}
