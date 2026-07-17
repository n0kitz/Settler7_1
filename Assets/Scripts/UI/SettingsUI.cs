using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Settings panel reachable from both MainMenuUI and PauseMenuUI.
    /// Houses audio volume controls, graphics quality, and fullscreen toggle.
    /// Partial: SettingsUI.Audio.cs and SettingsUI.Graphics.cs hold the section builders.
    /// </summary>
    public partial class SettingsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;

        // Audio controls
        [SerializeField] private TextMeshProUGUI _musicPct;
        [SerializeField] private TextMeshProUGUI _sfxPct;
        [SerializeField] private TextMeshProUGUI _muteLabel;

        // Graphics controls
        [SerializeField] private TextMeshProUGUI _qualityLabel;
        [SerializeField] private TextMeshProUGUI _fullscreenLabel;

        private SettingsState _state;
        private static readonly string[] QUALITY_CODES = { "low", "medium", "high", "ultra" };
        private readonly List<(TextMeshProUGUI label, string key)> _chromeLabels = new();

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            RefreshLocaleTexts();
            if (_state != null) Refresh();
        }

        private void RefreshLocaleTexts()
        {
            foreach (var (label, key) in _chromeLabels)
                if (label != null) label.text = L.Get(key);
        }

        public void Hide()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        public bool IsOpen =>
            _panelRoot != null && _panelRoot.activeSelf;

        /// <summary>Load persisted settings and refresh the UI.</summary>
        public void Initialize()
        {
            _state = SettingsPersistence.Load();
            LoadKeyBindings();
            Refresh();
            ApplyToEngine();
        }

        private void Refresh()
        {
            if (_musicPct != null)
                _musicPct.text = Pct(_state.MusicVolume);
            if (_sfxPct != null)
                _sfxPct.text = Pct(_state.SfxVolume);
            if (_muteLabel != null)
                _muteLabel.text = OnOff(_state.MasterMute);
            if (_qualityLabel != null)
                _qualityLabel.text =
                    L.Get("ui.settings.quality." + QUALITY_CODES[_state.GraphicsQuality]);
            if (_fullscreenLabel != null)
                _fullscreenLabel.text = OnOff(_state.Fullscreen);
            RefreshLanguage();
        }

        private void Apply()
        {
            SettingsPersistence.Save(_state);
            SaveKeyBindings();
            ApplyToEngine();
        }

        private void ApplyToEngine()
        {
            var audio = Presentation.AudioManager.Instance;
            if (audio != null)
            {
                audio.SetMusicVolume(_state.MasterMute ? 0f : _state.MusicVolume);
                audio.SetSFXVolume(_state.MasterMute ? 0f : _state.SfxVolume);
            }
            QualitySettings.SetQualityLevel(_state.GraphicsQuality, true);
            Screen.fullScreen = _state.Fullscreen;
            ApplyLanguage();
        }

        // --- Handlers ---

        private void OnMusicDown()  { _state.MusicVolume = Step(_state.MusicVolume, -0.1f); Refresh(); }
        private void OnMusicUp()    { _state.MusicVolume = Step(_state.MusicVolume,  0.1f); Refresh(); }
        private void OnSfxDown()    { _state.SfxVolume   = Step(_state.SfxVolume,   -0.1f); Refresh(); }
        private void OnSfxUp()      { _state.SfxVolume   = Step(_state.SfxVolume,    0.1f); Refresh(); }
        private void OnToggleMute() { _state.MasterMute  = !_state.MasterMute; Refresh(); }
        private void OnQualityPrev() { _state.GraphicsQuality = (_state.GraphicsQuality + 3) % 4; Refresh(); }
        private void OnQualityNext() { _state.GraphicsQuality = (_state.GraphicsQuality + 1) % 4; Refresh(); }
        private void OnToggleFullscreen() { _state.Fullscreen = !_state.Fullscreen; Refresh(); }

        private void OnApply() { Apply(); Hide(); }
        private void OnClose() { Hide(); }

        private static float Step(float v, float delta)
        {
            float result = v + delta;
            return result < 0f ? 0f : result > 1f ? 1f : Mathf.Round(result * 10f) / 10f;
        }

        private static string Pct(float v) => $"{Mathf.RoundToInt(v * 100f)}%";

        private static string OnOff(bool v) => L.Get(v ? "ui.general.on" : "ui.general.off");
    }
}
