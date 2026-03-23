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

        /// <summary>Fired when the player clicks Load Game.</summary>
        public event System.Action OnLoadGame;

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

        private void OnLoadGameClicked()
        {
            Hide();
            OnLoadGame?.Invoke();
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
            titleText.color = new Color(0.9f, 0.82f, 0.55f);

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
            CreateMenuButton(buttonContainer.transform, "New Game", font,
                new Color(0.2f, 0.5f, 0.25f, 0.9f), ui.OnNewGameClicked);

            // Load Game button
            CreateMenuButton(buttonContainer.transform, "Load Game", font,
                new Color(0.25f, 0.35f, 0.5f, 0.9f), ui.OnLoadGameClicked);

            // Quit button
            CreateMenuButton(buttonContainer.transform, "Quit", font,
                new Color(0.5f, 0.2f, 0.2f, 0.9f), ui.OnQuitClicked);

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

        private static void CreateMenuButton(Transform parent, string label,
            TMP_FontAsset font, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject($"Btn_{label.Replace(" ", "")}");
            btnGo.transform.SetParent(parent, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280f, 48f);

            var layoutElem = btnGo.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 48f;
            layoutElem.preferredWidth = 280f;

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = bgColor;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            var text = UIFactory.CreateLabel(btnGo.transform, "Label",
                label, 20, FontStyles.Bold, font);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;
        }
    }
}
