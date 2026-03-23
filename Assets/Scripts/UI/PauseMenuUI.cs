using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace Settlers.UI
{
    /// <summary>
    /// Pause menu toggled with ESC. Pauses the game and shows Resume / Quit to Menu buttons.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;

        private bool _isOpen;
        private float _previousTimeScale = 1f;
        private SaveSlotUI _saveSlotUI;

        /// <summary>Fired when the player clicks Quit to Menu.</summary>
        public event System.Action OnQuitToMenu;

        public void Show()
        {
            _isOpen = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
        }

        public void Hide()
        {
            _isOpen = false;
            Time.timeScale = _previousTimeScale;
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (_isOpen) Hide();
            else Show();
        }

        public bool IsOpen => _isOpen;

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

            // GameController already consumed ESC to close a panel this frame
            if (Presentation.GameController.EscConsumedThisFrame) return;

            // If save slot panel is open, close it instead of toggling pause menu
            if (_saveSlotUI != null && _saveSlotUI.IsOpen)
            {
                _saveSlotUI.Hide();
                return;
            }

            Toggle();
        }

        private void OnResumeClicked()
        {
            Hide();
        }

        private void OnSaveGameClicked()
        {
            if (_saveSlotUI != null)
                _saveSlotUI.Show(SaveSlotUI.Mode.Save);
        }

        private void OnLoadGameClicked()
        {
            if (_saveSlotUI != null)
                _saveSlotUI.Show(SaveSlotUI.Mode.Load);
        }

        private void OnQuitToMenuClicked()
        {
            Hide();
            OnQuitToMenu?.Invoke();
        }

        /// <summary>Create the pause menu UI programmatically.</summary>
        public static PauseMenuUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("PauseMenuUI");
            panelGo.transform.SetParent(canvasTransform, false);

            // Semi-transparent full-screen overlay
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0f, 0f, 0f, 0.6f);

            // Center box
            var boxGo = new GameObject("Box");
            boxGo.transform.SetParent(panelGo.transform, false);
            var boxRect = boxGo.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.35f, 0.2f);
            boxRect.anchorMax = new Vector2(0.65f, 0.8f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;

            var boxBg = boxGo.AddComponent<Image>();
            boxBg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

            // Title
            var titleText = UIFactory.CreateLabel(boxGo.transform, "Title",
                "Paused", 28, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(0f, 36f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.9f, 0.82f, 0.55f);

            // Button container
            var btnContainer = new GameObject("Buttons");
            btnContainer.transform.SetParent(boxGo.transform, false);
            var btnContainerRect = btnContainer.AddComponent<RectTransform>();
            btnContainerRect.anchorMin = new Vector2(0.15f, 0.15f);
            btnContainerRect.anchorMax = new Vector2(0.85f, 0.7f);
            btnContainerRect.offsetMin = Vector2.zero;
            btnContainerRect.offsetMax = Vector2.zero;

            var layout = btnContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            var ui = panelGo.AddComponent<PauseMenuUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);

            // Resume button
            CreateButton(btnContainer.transform, "Resume", font,
                new Color(0.2f, 0.5f, 0.25f, 0.9f), ui.OnResumeClicked);

            // Save Game button
            CreateButton(btnContainer.transform, "Save Game", font,
                new Color(0.25f, 0.35f, 0.5f, 0.9f), ui.OnSaveGameClicked);

            // Load Game button
            CreateButton(btnContainer.transform, "Load Game", font,
                new Color(0.25f, 0.35f, 0.5f, 0.9f), ui.OnLoadGameClicked);

            // Quit to Menu button
            CreateButton(btnContainer.transform, "Quit to Menu", font,
                new Color(0.5f, 0.2f, 0.2f, 0.9f), ui.OnQuitToMenuClicked);

            // Create the save slot panel (shared by save and load)
            ui._saveSlotUI = SaveSlotUI.Create(canvasTransform, font);

            panelGo.SetActive(false);
            return ui;
        }

        private static void CreateButton(Transform parent, string label,
            TMP_FontAsset font, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject($"Btn_{label.Replace(" ", "")}");
            btnGo.transform.SetParent(parent, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 44f);

            var le = btnGo.AddComponent<LayoutElement>();
            le.preferredHeight = 44f;

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = bgColor;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            var text = UIFactory.CreateLabel(btnGo.transform, "Label",
                label, 18, FontStyles.Bold, font);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;
        }
    }
}
