using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

            // New Game button
            UIFactory.CreateButton(buttonContainer.transform, "New Game", font,
                UIColors.BUTTON_GREEN, ui.OnNewGameClicked,
                new Vector2(280f, 48f), 20f);

            // Campaign button
            UIFactory.CreateButton(buttonContainer.transform, "Campaign", font,
                new Color(0.6f, 0.4f, 0.1f), ui.OnCampaignClicked,
                new Vector2(280f, 48f), 20f);

            // Tutorial button
            UIFactory.CreateButton(buttonContainer.transform, "Tutorial", font,
                new Color(0.3f, 0.5f, 0.7f), ui.OnTutorialClicked,
                new Vector2(280f, 48f), 20f);

            // Load Game button
            UIFactory.CreateButton(buttonContainer.transform, "Load Game", font,
                UIColors.BUTTON_BLUE, ui.OnLoadGameClicked,
                new Vector2(280f, 48f), 20f);

            // Map Editor button
            UIFactory.CreateButton(buttonContainer.transform, "Map Editor", font,
                new Color(0.35f, 0.28f, 0.5f), ui.OnMapEditorClicked,
                new Vector2(280f, 48f), 20f);

            // Quit button
            UIFactory.CreateButton(buttonContainer.transform, "Quit", font,
                UIColors.BUTTON_RED, ui.OnQuitClicked,
                new Vector2(280f, 48f), 20f);

            // Version text (bottom)
            var versionText = UIFactory.CreateLabel(panelGo.transform, "Version",
                "v0.12 — Phase 12", 12, FontStyles.Normal, font);
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

    }
}
