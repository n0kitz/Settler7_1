using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    public partial class SettingsUI
    {
        [SerializeField] private TextMeshProUGUI _languageLabel;
        [SerializeField] private TextMeshProUGUI _colorBlindLabel;

        private static readonly string[] LOCALE_CODES   = { "en", "de", "fr" };
        private static readonly string[] LOCALE_DISPLAY = { "English", "Deutsch", "Français" };

        private int _localeIndex;

        /// <summary>Builds the Language + Accessibility section rows.</summary>
        private static (TextMeshProUGUI langLabel, TextMeshProUGUI cbLabel)
            CreateLanguageSection(Transform container, TMP_FontAsset font, SettingsUI ui)
        {
            CreateSectionHeader(container, "Language & Accessibility", font);

            var (_, langLabel) = CreateRowWithValue(container, "Language", font,
                ui.OnLanguagePrev, ui.OnLanguageNext, "English");

            // Color blind toggle row
            var cbRow = CreateRow(container, "Color Blind Mode", font);
            var cbLabel = UIFactory.CreateLabel(cbRow, "CbVal", "OFF", 16f, font);
            var cbRect = cbLabel.GetComponent<RectTransform>();
            cbRect.sizeDelta = new Vector2(50f, 30f);
            var cbLe = cbLabel.gameObject.AddComponent<LayoutElement>();
            cbLe.preferredWidth = 50f;
            cbLe.preferredHeight = 30f;
            UIFactory.CreateButton(cbRow, "Toggle", font,
                new Color(0.35f, 0.35f, 0.4f), ui.OnToggleColorBlind,
                new Vector2(70f, 30f), 14f);

            return (langLabel, cbLabel);
        }

        // --- Handlers ---

        private void OnLanguagePrev()
        {
            _localeIndex = (_localeIndex + LOCALE_CODES.Length - 1) % LOCALE_CODES.Length;
            _state.Language = LOCALE_CODES[_localeIndex];
            Refresh();
        }

        private void OnLanguageNext()
        {
            _localeIndex = (_localeIndex + 1) % LOCALE_CODES.Length;
            _state.Language = LOCALE_CODES[_localeIndex];
            Refresh();
        }

        private void OnToggleColorBlind()
        {
            _state.ColorBlindMode = !_state.ColorBlindMode;
            Refresh();
        }

        // --- Refresh extension ---

        private void RefreshLanguage()
        {
            _localeIndex = System.Array.IndexOf(LOCALE_CODES, _state.Language);
            if (_localeIndex < 0) _localeIndex = 0;

            if (_languageLabel != null)
                _languageLabel.text = LOCALE_DISPLAY[_localeIndex];
            if (_colorBlindLabel != null)
                _colorBlindLabel.text = _state.ColorBlindMode ? "ON" : "OFF";
        }

        // --- Apply extension ---

        private void ApplyLanguage()
        {
            L.SetLocale(_state.Language);
            UIColors.SetColorBlindMode(_state.ColorBlindMode);
        }
    }
}
