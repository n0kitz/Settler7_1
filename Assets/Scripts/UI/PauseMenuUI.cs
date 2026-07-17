using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

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
        private SettingsUI _settingsUI;
        private TextMeshProUGUI _titleLabel;
        private readonly List<(TextMeshProUGUI label, string key)> _buttonLabels = new();

        /// <summary>Fired when the player clicks Settings.</summary>
        public event System.Action OnSettings;

        /// <summary>Fired when the player clicks Achievements.</summary>
        public event System.Action OnAchievements;

        /// <summary>Fired when the player clicks Quit to Menu.</summary>
        public event System.Action OnQuitToMenu;

        public void Show()
        {
            _isOpen = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
            RefreshLocaleTexts();
        }

        private void RefreshLocaleTexts()
        {
            if (_titleLabel != null) _titleLabel.text = L.Get("ui.pause_menu.title");
            foreach (var (label, key) in _buttonLabels)
                if (label != null) label.text = L.Get(key);
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

        private void OnSettingsClicked()      => OnSettings?.Invoke();
        private void OnAchievementsClicked()  => OnAchievements?.Invoke();

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
            boxBg.color = UIColors.PANEL_BLUE_DARK;

            // Title
            var titleText = UIFactory.CreateLabel(boxGo.transform, "Title",
                L.Get("ui.pause_menu.title"), 28, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(0f, 36f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = UIColors.TEXT_HEADER_GOLD;

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
            ui._titleLabel = titleText;

            CreateMenuButton(btnContainer.transform, ui, "ui.pause_menu.resume", font,
                UIColors.BUTTON_GREEN, ui.OnResumeClicked);
            CreateMenuButton(btnContainer.transform, ui, "ui.pause_menu.save_game", font,
                UIColors.BUTTON_BLUE, ui.OnSaveGameClicked);
            CreateMenuButton(btnContainer.transform, ui, "ui.pause_menu.load_game", font,
                UIColors.BUTTON_BLUE, ui.OnLoadGameClicked);
            CreateMenuButton(btnContainer.transform, ui, "ui.menu.achievements", font,
                new Color(0.4f, 0.3f, 0.1f), ui.OnAchievementsClicked);
            CreateMenuButton(btnContainer.transform, ui, "ui.pause_menu.settings", font,
                new Color(0.28f, 0.28f, 0.35f), ui.OnSettingsClicked);
            CreateMenuButton(btnContainer.transform, ui, "ui.pause_menu.quit_to_menu", font,
                UIColors.BUTTON_RED, ui.OnQuitToMenuClicked);

            // Create the save slot panel (shared by save and load)
            ui._saveSlotUI = SaveSlotUI.Create(canvasTransform, font);

            panelGo.SetActive(false);
            return ui;
        }

        private static void CreateMenuButton(Transform parent, PauseMenuUI ui, string key,
            TMP_FontAsset font, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIFactory.CreateButton(parent, L.Get(key), font, color, onClick);
            ui._buttonLabels.Add((btn.GetComponentInChildren<TextMeshProUGUI>(), key));
        }
    }
}
