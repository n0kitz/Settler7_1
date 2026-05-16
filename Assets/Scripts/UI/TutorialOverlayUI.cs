using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Tutorial bubble overlay shown during the tutorial mission.
    /// Displays the current step's title, message, progress, and navigation buttons.
    /// Wires to TutorialSystem events; GameController creates and connects both.
    /// </summary>
    public class TutorialOverlayUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TextMeshProUGUI _nextButtonLabel;

        private TutorialSystem _tutorial;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        /// <summary>Connect to a live TutorialSystem and show first step.</summary>
        public void Bind(TutorialSystem tutorial)
        {
            _tutorial = tutorial;
            _tutorial.OnStepStarted += ShowStep;
            _tutorial.OnTutorialComplete += Hide;
            _tutorial.OnTutorialSkipped += Hide;
        }

        public void Show()
        {
            _panelRoot?.SetActive(true);
        }

        public void Hide()
        {
            _panelRoot?.SetActive(false);
        }

        private void ShowStep(TutorialStep step)
        {
            if (step == null) { Hide(); return; }
            Show();
            if (_titleText != null) _titleText.text = step.Title;
            if (_messageText != null) _messageText.text = step.Message;
            if (_progressText != null)
                _progressText.text = $"Step {_tutorial.CurrentStepIndex + 1} of {_tutorial.TotalSteps}";

            bool isLastStep = _tutorial.CurrentStepIndex == _tutorial.TotalSteps - 1;
            if (_nextButtonLabel != null)
                _nextButtonLabel.text = isLastStep ? "Finish" : "Next →";

            // Next button active only when condition is None (manual advance)
            bool showNext = step.Condition == TutorialConditionType.None;
            if (_nextButton != null) _nextButton.gameObject.SetActive(showNext);
        }

        private void OnNextClicked()
        {
            _tutorial?.Advance();
        }

        private void OnSkipClicked()
        {
            _tutorial?.Skip();
        }

        private void OnDestroy()
        {
            if (_tutorial != null)
            {
                _tutorial.OnStepStarted -= ShowStep;
                _tutorial.OnTutorialComplete -= Hide;
                _tutorial.OnTutorialSkipped -= Hide;
            }
        }

        // --- Factory ---

        /// <summary>Create the tutorial overlay programmatically.</summary>
        public static TutorialOverlayUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var root = new GameObject("TutorialOverlayUI");
            root.transform.SetParent(canvasTransform, false);

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(1f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(0f, 10f);
            rootRect.sizeDelta = new Vector2(-40f, 200f);

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.10f, 0.12f, 0.94f);

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 14, 14);
            layout.spacing = 8f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Progress + Title row
            var headerRow = new GameObject("HeaderRow");
            headerRow.transform.SetParent(root.transform, false);
            var headerLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 10f;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = false;
            headerRow.AddComponent<LayoutElement>().preferredHeight = 24f;

            var progressLabel = UIFactory.CreateLabel(headerRow.transform, "Progress",
                "Step 1 of 7", 13f, FontStyles.Normal, font);
            progressLabel.color = UIColors.TEXT_GRAY_DIM;
            var progressLE = progressLabel.GetComponent<LayoutElement>() ?? progressLabel.gameObject.AddComponent<LayoutElement>();
            progressLE.preferredWidth = 90f;

            var titleLabel = UIFactory.CreateLabel(headerRow.transform, "Title",
                "", 18f, FontStyles.Bold, font);
            titleLabel.color = UIColors.TEXT_HEADER_GOLD;
            var titleLE = titleLabel.GetComponent<LayoutElement>() ?? titleLabel.gameObject.AddComponent<LayoutElement>();
            titleLE.flexibleWidth = 1f;

            // Skip button (top-right)
            var skipBtn = UIFactory.CreateButton(headerRow.transform, "Skip Tutorial", font,
                new Color(0.3f, 0.3f, 0.3f), null, new Vector2(110f, 22f), 12f);
            skipBtn.GetComponent<LayoutElement>().preferredWidth = 110f;

            // Message
            var msgLabel = UIFactory.CreateLabel(root.transform, "Message",
                "", 15f, FontStyles.Normal, font);
            msgLabel.color = Color.white;
            msgLabel.textWrappingMode = TextWrappingModes.Normal;
            msgLabel.overflowMode = TextOverflowModes.Overflow;
            var msgLE = msgLabel.GetComponent<LayoutElement>() ?? msgLabel.gameObject.AddComponent<LayoutElement>();
            msgLE.preferredHeight = 90f;

            // Next button
            var nextBtn = UIFactory.CreateButton(root.transform, "Next →", font,
                UIColors.BUTTON_GREEN, null, new Vector2(120f, 36f), 16f);
            var nextBtnLE = nextBtn.GetComponent<LayoutElement>();
            if (nextBtnLE == null) nextBtnLE = nextBtn.gameObject.AddComponent<LayoutElement>();
            nextBtnLE.preferredWidth = 120f;

            // Wire component
            var ui = root.AddComponent<TutorialOverlayUI>();
            UIFactory.SetField(ui, "_panelRoot", root);
            UIFactory.SetField(ui, "_titleText", titleLabel);
            UIFactory.SetField(ui, "_messageText", msgLabel);
            UIFactory.SetField(ui, "_progressText", progressLabel);
            UIFactory.SetField(ui, "_nextButton", nextBtn);
            UIFactory.SetField(ui, "_nextButtonLabel", nextBtn.GetComponentInChildren<TextMeshProUGUI>());

            nextBtn.onClick.AddListener(ui.OnNextClicked);
            skipBtn.onClick.AddListener(ui.OnSkipClicked);

            root.SetActive(false);
            return ui;
        }
    }
}
