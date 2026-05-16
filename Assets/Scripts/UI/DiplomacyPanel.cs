using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Diplomacy relations matrix and action panel.
    /// Toggle with J key. Shows status per AI opponent and player action buttons.
    /// </summary>
    public class DiplomacyPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform  _rowContainer;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        private DiplomacySystem _diplomacy;
        private TMP_FontAsset   _font;

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current.jKey.wasPressedThisFrame) Toggle();
        }

        public void Bind(DiplomacySystem diplomacy, TMP_FontAsset font)
        {
            _diplomacy = diplomacy;
            _font      = font;
        }

        public void Toggle()
        {
            if (_panelRoot == null) return;
            bool next = !_panelRoot.activeSelf;
            _panelRoot.SetActive(next);
            if (next) Rebuild();
        }

        public void Show()  { if (_panelRoot != null) { _panelRoot.SetActive(true); Rebuild(); } }
        public void Hide()  { if (_panelRoot != null)   _panelRoot.SetActive(false); }

        private void Rebuild()
        {
            if (_rowContainer == null) return;
            for (int i = _rowContainer.childCount - 1; i >= 0; i--)
                Destroy(_rowContainer.GetChild(i).gameObject);

            var gc = Presentation.GameController.Instance;
            if (gc?.State == null) return;

            for (int p = 1; p < gc.State.PlayerCount; p++)
                CreateOpponentRow(p);
        }

        private void CreateOpponentRow(int opponentId)
        {
            var gc     = Presentation.GameController.Instance;
            var status = _diplomacy?.GetStatus(0, opponentId) ?? DiplomaticStatus.Peace;

            var row = new GameObject($"Row_P{opponentId}");
            row.transform.SetParent(_rowContainer, false);
            var rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 52f);
            var layout = row.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(4, 4, 4, 4);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 52f;

            var infoLine = UIFactory.CreateLabel(row.transform, "Info",
                $"Player {opponentId}: <b>{status.ToDisplayString()}</b>", 14f, _font);
            infoLine.color = StatusColor(status);

            var btnRow = new GameObject("BtnRow");
            btnRow.transform.SetParent(row.transform, false);
            var btnLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 6f;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childForceExpandHeight = false;

            int capturedId = opponentId;
            CreateSmallBtn(btnRow.transform, "Non-Aggression",
                new Color(0.2f, 0.5f, 0.2f),
                () => SendAction(capturedId, DiplomaticActionType.ProposeNonAggression));
            CreateSmallBtn(btnRow.transform, "Alliance",
                new Color(0.2f, 0.3f, 0.6f),
                () => SendAction(capturedId, DiplomaticActionType.ProposeAlliance));
            CreateSmallBtn(btnRow.transform, "Gift 50g",
                new Color(0.5f, 0.4f, 0.1f),
                () => SendAction(capturedId, DiplomaticActionType.OfferGift));
            CreateSmallBtn(btnRow.transform, "War",
                new Color(0.6f, 0.1f, 0.1f),
                () => SendAction(capturedId, DiplomaticActionType.DeclareWar));
        }

        private void SendAction(int toId, DiplomaticActionType type)
        {
            if (_diplomacy == null) return;
            var action = new DiplomaticAction(0, toId, type);
            bool accepted = _diplomacy.ProcessAction(action);
            if (_feedbackText != null)
                _feedbackText.text = accepted ? $"Proposal accepted!" : "Proposal rejected.";
            Rebuild();
        }

        private static void CreateSmallBtn(Transform parent, string label,
            Color color, UnityEngine.Events.UnityAction onClick)
        {
            UIFactory.CreateButton(parent, label, UIFactory.GetDefaultFont(),
                color, onClick, new Vector2(90f, 24f), 11f);
        }

        private static Color StatusColor(DiplomaticStatus s)
        {
            switch (s)
            {
                case DiplomaticStatus.War:           return new Color(0.9f, 0.3f, 0.3f);
                case DiplomaticStatus.Alliance:      return new Color(0.4f, 0.7f, 1f);
                case DiplomaticStatus.NonAggression: return new Color(0.4f, 0.9f, 0.4f);
                default:                             return UIColors.TEXT_LIGHT;
            }
        }

        public static DiplomacyPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("DiplomacyPanel");
            panelGo.transform.SetParent(canvasTransform, false);

            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.08f);
            rect.anchorMax = new Vector2(0.42f, 0.92f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panelGo.AddComponent<Image>().color = UIColors.PANEL_BLUE_DARK;

            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Diplomacy [J]", 20f, FontStyles.Bold, font);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot     = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -10f);
            titleRect.sizeDelta = new Vector2(0f, 30f);
            title.alignment = TextAlignmentOptions.Center;
            title.color = UIColors.TEXT_HEADER_GOLD;

            // Content area
            var content = new GameObject("Content");
            content.transform.SetParent(panelGo.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.02f, 0.15f);
            contentRect.anchorMax = new Vector2(0.98f, 0.85f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            content.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var feedback = UIFactory.CreateLabel(panelGo.transform, "Feedback",
                "", 13f, FontStyles.Italic, font);
            var feedbackRect = feedback.GetComponent<RectTransform>();
            feedbackRect.anchorMin = new Vector2(0.02f, 0.07f);
            feedbackRect.anchorMax = new Vector2(0.98f, 0.14f);
            feedbackRect.offsetMin = Vector2.zero;
            feedbackRect.offsetMax = Vector2.zero;
            feedback.color = UIColors.TEXT_HEADER_GOLD;

            UIFactory.CreateButton(panelGo.transform, "Close", font,
                UIColors.BUTTON_RED, () => panelGo.SetActive(false),
                new Vector2(80f, 28f), 14f)
                .GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var closeRect = panelGo.GetComponentInChildren<Button>()
                .GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot     = new Vector2(0.5f, 0f);
            closeRect.anchoredPosition = new Vector2(0f, 4f);

            var panel = panelGo.AddComponent<DiplomacyPanel>();
            UIFactory.SetField(panel, "_panelRoot",    panelGo);
            UIFactory.SetField(panel, "_rowContainer", (Transform)contentRect.transform);
            UIFactory.SetField(panel, "_feedbackText", feedback);
            panel._font = font;

            panelGo.SetActive(false);
            return panel;
        }
    }
}
