using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Pre-mission briefing screen shown after selecting a mission.
    /// Displays title, briefing text, objectives list, and Start / Back buttons.
    /// </summary>
    public class MissionBriefingUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _briefingText;
        [SerializeField] private Transform _objectivesContainer;

        private TMP_FontAsset _font;
        private Mission _mission;

        public event Action<Mission> OnStart;
        public event Action OnBack;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public void Show(Mission mission)
        {
            _mission = mission;
            _panelRoot?.SetActive(true);

            if (_titleText != null) _titleText.text = mission.Title;
            if (_briefingText != null) _briefingText.text = mission.Briefing;

            // Populate objectives
            if (_objectivesContainer != null)
            {
                foreach (Transform child in _objectivesContainer) Destroy(child.gameObject);
                foreach (var obj in mission.Objectives)
                {
                    var lbl = UIFactory.CreateLabel(_objectivesContainer, "Obj",
                        "• " + obj.Description, 14f, FontStyles.Normal, _font);
                    lbl.color = new Color(0.8f, 0.9f, 0.7f);
                    lbl.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;
                }
            }
        }

        public void Hide() => _panelRoot?.SetActive(false);

        private void OnStartClicked() { Hide(); OnStart?.Invoke(_mission); }
        private void OnBackClicked() { Hide(); OnBack?.Invoke(); }

        public static MissionBriefingUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var root = new GameObject("MissionBriefingUI");
            root.transform.SetParent(canvasTransform, false);

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.15f, 0.1f);
            rootRect.anchorMax = new Vector2(0.85f, 0.9f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.10f, 0.97f);

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 24, 20);
            layout.spacing = 14f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var title = UIFactory.CreateLabel(root.transform, "Title", "", 24f, FontStyles.Bold, font);
            title.color = UIColors.TEXT_HEADER_GOLD;
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 34f;

            var briefing = UIFactory.CreateLabel(root.transform, "Briefing", "", 15f, FontStyles.Normal, font);
            briefing.color = Color.white;
            briefing.textWrappingMode = TextWrappingModes.Normal;
            briefing.overflowMode = TextOverflowModes.Overflow;
            briefing.gameObject.AddComponent<LayoutElement>().preferredHeight = 110f;

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
            btnLayout.spacing = 16f;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnRow.AddComponent<LayoutElement>().preferredHeight = 44f;

            UIFactory.CreateButton(btnRow.transform, "← Back", font,
                new Color(0.3f, 0.3f, 0.3f), null, new Vector2(120f, 44f), 16f);
            UIFactory.CreateButton(btnRow.transform, "Start Mission →", font,
                UIColors.BUTTON_GREEN, null, new Vector2(200f, 44f), 18f);

            var ui = root.AddComponent<MissionBriefingUI>();
            UIFactory.SetField(ui, "_panelRoot", root);
            UIFactory.SetField(ui, "_titleText", title);
            UIFactory.SetField(ui, "_briefingText", briefing);
            UIFactory.SetField(ui, "_objectivesContainer", objContainer.transform);
            UIFactory.SetField(ui, "_font", font);

            var btns = root.GetComponentsInChildren<Button>();
            btns[0].onClick.AddListener(ui.OnBackClicked);
            btns[1].onClick.AddListener(ui.OnStartClicked);

            root.SetActive(false);
            return ui;
        }
    }
}
