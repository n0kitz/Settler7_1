using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Post-mission result screen shown when a campaign mission ends.
    /// Displayed on both victory and defeat.
    /// </summary>
    public class MissionCompleteUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _headlineText;
        [SerializeField] private TextMeshProUGUI _summaryText;
        [SerializeField] private Transform _objectivesContainer;

        private TMP_FontAsset _font;

        public event Action OnContinue;
        public event Action OnRetry;
        public event Action OnMenu;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        /// <summary>Show the result of the mission.</summary>
        public void Show(Mission mission, bool victory, float elapsedSeconds)
        {
            _panelRoot?.SetActive(true);

            if (_headlineText != null)
                _headlineText.text = victory
                    ? L.Get("ui.mission.complete") : L.Get("ui.mission.failed");
            if (_headlineText != null)
                _headlineText.color = victory ? UIColors.BUTTON_GREEN : UIColors.BUTTON_RED;

            int minutes = (int)(elapsedSeconds / 60);
            int seconds = (int)(elapsedSeconds % 60);
            if (_summaryText != null)
                _summaryText.text = $"{LocalizedNames.MissionTitle(mission)}\n" +
                    $"{L.Get("ui.mission.time")}: {minutes}:{seconds:D2}";
            RefreshChrome();

            if (_objectivesContainer != null)
            {
                foreach (Transform child in _objectivesContainer) Destroy(child.gameObject);
                for (int i = 0; i < mission.Objectives.Length; i++)
                {
                    var obj = mission.Objectives[i];
                    string icon = obj.IsComplete ? "✓" : "✗";
                    Color color = obj.IsComplete ? UIColors.BUTTON_GREEN : UIColors.BUTTON_RED;
                    var lbl = UIFactory.CreateLabel(_objectivesContainer, "Obj",
                        $"{icon} {LocalizedNames.MissionObjective(mission, i)}",
                        14f, FontStyles.Normal, _font);
                    lbl.color = color;
                    lbl.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;
                }
            }
        }

        /// <summary>Re-resolve creation-time baked labels (locale can change).</summary>
        private void RefreshChrome()
        {
            if (_objHeader != null) _objHeader.text = L.Get("ui.mission.objectives");
            if (_menuLabel != null) _menuLabel.text = L.Get("ui.mission.menu");
            if (_retryLabel != null) _retryLabel.text = L.Get("ui.mission.retry");
            if (_continueLabel != null) _continueLabel.text = L.Get("ui.mission.continue");
        }

        private TextMeshProUGUI _objHeader;
        private TextMeshProUGUI _menuLabel;
        private TextMeshProUGUI _retryLabel;
        private TextMeshProUGUI _continueLabel;

        public void Hide() => _panelRoot?.SetActive(false);

        private void OnContinueClicked() { Hide(); OnContinue?.Invoke(); }
        private void OnRetryClicked() { Hide(); OnRetry?.Invoke(); }
        private void OnMenuClicked() { Hide(); OnMenu?.Invoke(); }

        public static MissionCompleteUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var root = new GameObject("MissionCompleteUI");
            root.transform.SetParent(canvasTransform, false);

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.2f, 0.15f);
            rootRect.anchorMax = new Vector2(0.8f, 0.85f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.06f, 0.08f, 0.97f);

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 20);
            layout.spacing = 14f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var headline = UIFactory.CreateLabel(root.transform, "Headline",
                "Mission Complete!", 32f, FontStyles.Bold, font);
            headline.alignment = TextAlignmentOptions.Center;
            headline.gameObject.AddComponent<LayoutElement>().preferredHeight = 44f;

            var summary = UIFactory.CreateLabel(root.transform, "Summary",
                "", 16f, FontStyles.Normal, font);
            summary.color = Color.white;
            summary.alignment = TextAlignmentOptions.Center;
            summary.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;

            var objHeader = UIFactory.CreateLabel(root.transform, "ObjHeader",
                "Objectives:", 15f, FontStyles.Bold, font);
            objHeader.color = UIColors.TEXT_HEADER_GOLD;
            objHeader.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            var objContainer = new GameObject("Objectives");
            objContainer.transform.SetParent(root.transform, false);
            var objLayout = objContainer.AddComponent<VerticalLayoutGroup>();
            objLayout.spacing = 4f;
            objLayout.childForceExpandWidth = true;
            objLayout.childForceExpandHeight = false;
            objContainer.AddComponent<LayoutElement>().flexibleHeight = 1f;

            // Button row
            var btnRow = new GameObject("BtnRow");
            btnRow.transform.SetParent(root.transform, false);
            var btnLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 12f;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnRow.AddComponent<LayoutElement>().preferredHeight = 44f;

            UIFactory.CreateButton(btnRow.transform, "Main Menu", font,
                new Color(0.3f, 0.3f, 0.3f), null, new Vector2(140f, 44f), 15f);
            UIFactory.CreateButton(btnRow.transform, "Retry", font,
                UIColors.BUTTON_BLUE, null, new Vector2(110f, 44f), 15f);
            UIFactory.CreateButton(btnRow.transform, "Continue →", font,
                UIColors.BUTTON_GREEN, null, new Vector2(140f, 44f), 15f);

            var ui = root.AddComponent<MissionCompleteUI>();
            UIFactory.SetField(ui, "_panelRoot", root);
            UIFactory.SetField(ui, "_headlineText", headline);
            UIFactory.SetField(ui, "_summaryText", summary);
            UIFactory.SetField(ui, "_objectivesContainer", objContainer.transform);
            UIFactory.SetField(ui, "_font", font);

            var btns = root.GetComponentsInChildren<Button>();
            btns[0].onClick.AddListener(ui.OnMenuClicked);
            btns[1].onClick.AddListener(ui.OnRetryClicked);
            btns[2].onClick.AddListener(ui.OnContinueClicked);

            ui._objHeader = objHeader;
            ui._menuLabel = btns[0].GetComponentInChildren<TextMeshProUGUI>();
            ui._retryLabel = btns[1].GetComponentInChildren<TextMeshProUGUI>();
            ui._continueLabel = btns[2].GetComponentInChildren<TextMeshProUGUI>();

            root.SetActive(false);
            return ui;
        }
    }
}
