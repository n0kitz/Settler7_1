using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Presentation;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Replay playback controls: timeline bar, play/pause, speed, elapsed time.
    /// Toggle with the ReplayController. Hidden by default; shown when a replay loads.
    /// </summary>
    public class ReplayUI : MonoBehaviour
    {
        [SerializeField] private GameObject       _panelRoot;
        [SerializeField] private Image            _timelineFill;
        [SerializeField] private TextMeshProUGUI  _timeLabel;
        [SerializeField] private TextMeshProUGUI  _speedLabel;
        [SerializeField] private Button           _playPauseBtn;
        [SerializeField] private TextMeshProUGUI  _playPauseLabel;

        private ReplayController _rc;
        private float[] _speeds = { 1f, 2f, 4f, 8f };
        private int _speedIndex;

        public void Show() { if (_panelRoot) _panelRoot.SetActive(true); }
        public void Hide() { if (_panelRoot) _panelRoot.SetActive(false); }

        private void Start()
        {
            _rc = ReplayController.Instance;
            if (_rc == null) return;
            _rc.OnTick     += OnTick;
            _rc.OnComplete += OnComplete;
        }

        private void OnDestroy()
        {
            if (_rc != null)
            {
                _rc.OnTick     -= OnTick;
                _rc.OnComplete -= OnComplete;
            }
        }

        private void OnTick(float elapsed)
        {
            float total = _rc.TotalDuration;
            if (_timelineFill != null)
                _timelineFill.fillAmount = total > 0f ? elapsed / total : 0f;
            if (_timeLabel != null)
                _timeLabel.text = $"{FormatTime(elapsed)} / {FormatTime(total)}";
            if (_playPauseLabel != null)
                _playPauseLabel.text = _rc.IsPlaying ? "⏸" : "▶";
        }

        private void OnComplete()
        {
            if (_playPauseLabel != null) _playPauseLabel.text = "▶";
        }

        private void OnPlayPauseClicked() { _rc?.Toggle(); }

        private void OnSpeedClicked()
        {
            _speedIndex = (_speedIndex + 1) % _speeds.Length;
            if (_rc != null) _rc.PlaybackSpeed = _speeds[_speedIndex];
            if (_speedLabel != null) _speedLabel.text = $"{_speeds[_speedIndex]}×";
        }

        private void OnCloseClicked() { Hide(); }

        private static string FormatTime(float seconds)
        {
            int m = (int)(seconds / 60f);
            int s = (int)(seconds % 60f);
            return $"{m:D2}:{s:D2}";
        }

        public static ReplayUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("ReplayUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0f);
            panelRect.anchorMax = new Vector2(0.85f, 0f);
            panelRect.pivot     = new Vector2(0.5f,  0f);
            panelRect.anchoredPosition = new Vector2(0f, 10f);
            panelRect.sizeDelta = new Vector2(0f, 70f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);

            var layout = panelGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding  = new RectOffset(12, 12, 8, 8);
            layout.spacing  = 10f;
            layout.childForceExpandWidth  = false;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleLeft;

            var ui = panelGo.AddComponent<ReplayUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);

            // Play/Pause button
            var ppBtn = UIFactory.CreateButton(panelGo.transform, "▶", font,
                UIColors.BUTTON_GREEN, ui.OnPlayPauseClicked, new Vector2(48f, 48f), 20f);
            UIFactory.SetField(ui, "_playPauseBtn",   ppBtn);
            UIFactory.SetField(ui, "_playPauseLabel", ppBtn.GetComponentInChildren<TextMeshProUGUI>());

            // Timeline bar
            var barGo = new GameObject("Timeline");
            barGo.transform.SetParent(panelGo.transform, false);
            var barRect = barGo.AddComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(0f, 20f);
            var barLe = barGo.AddComponent<LayoutElement>();
            barLe.flexibleWidth  = 1f;
            barLe.preferredHeight = 20f;
            var barBg = barGo.AddComponent<Image>();
            barBg.color = new Color(0.2f, 0.2f, 0.25f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(barGo.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.pivot     = new Vector2(0f, 0.5f);
            fillRect.sizeDelta = new Vector2(0f, 0f);
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = UIColors.BUTTON_BLUE;
            fillImg.type  = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            UIFactory.SetField(ui, "_timelineFill", fillImg);

            // Time label
            var timeLabel = UIFactory.CreateLabel(panelGo.transform, "Time",
                "00:00 / 00:00", 14f, font);
            var timeLe = timeLabel.gameObject.AddComponent<LayoutElement>();
            timeLe.preferredWidth = 120f;
            UIFactory.SetField(ui, "_timeLabel", timeLabel);

            // Speed button
            UIFactory.CreateButton(panelGo.transform, "1×", font,
                UIColors.BUTTON_BLUE, ui.OnSpeedClicked, new Vector2(48f, 36f), 14f);
            var speedLabel = UIFactory.CreateLabel(panelGo.transform, "SpeedLbl", "1×", 14f, font);
            UIFactory.SetField(ui, "_speedLabel", speedLabel);

            // Close button
            UIFactory.CreateButton(panelGo.transform, "✕", font,
                UIColors.BUTTON_RED, ui.OnCloseClicked, new Vector2(36f, 36f), 16f);

            panelGo.SetActive(false);
            return ui;
        }
    }
}
