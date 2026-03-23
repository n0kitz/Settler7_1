using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Map selection screen shown at game start.
    /// Displays available maps with details. Selecting a map starts the game.
    /// </summary>
    public class MapSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _mapListContainer;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _detailsText;

        private string _selectedMapId;

        /// <summary>Fired when a map is selected and confirmed.</summary>
        public event System.Action<string> OnMapSelected;

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

        private void OnMapClicked(string mapId)
        {
            _selectedMapId = mapId;
            var info = MapFactory.CreateMap(mapId);

            if (_detailsText != null)
            {
                _detailsText.text = $"<b>{info.DisplayName}</b>\n" +
                    $"Sectors: {info.Graph.SectorCount}\n" +
                    $"Players: {info.PlayerCount}\n" +
                    $"Victory Points: {info.VPRequired}";
            }
        }

        private void OnStartClicked()
        {
            if (string.IsNullOrEmpty(_selectedMapId))
                _selectedMapId = "test_valley";

            Hide();
            OnMapSelected?.Invoke(_selectedMapId);
        }

        /// <summary>Create the map selection UI programmatically.</summary>
        public static MapSelectionUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("MapSelectionUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.15f);
            panelRect.anchorMax = new Vector2(0.8f, 0.85f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

            // Title
            var titleText = CreateLabel(panelGo.transform, "Title",
                "Select Map", 24, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -12f);
            titleRect.sizeDelta = new Vector2(0f, 35f);
            titleText.alignment = TextAlignmentOptions.Center;

            // Map list container
            var listGo = new GameObject("MapList");
            listGo.transform.SetParent(panelGo.transform, false);
            var listRect = listGo.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.05f, 0.25f);
            listRect.anchorMax = new Vector2(0.5f, 0.9f);
            listRect.offsetMin = Vector2.zero;
            listRect.offsetMax = Vector2.zero;

            var listBg = listGo.AddComponent<Image>();
            listBg.color = new Color(0.12f, 0.12f, 0.14f, 0.8f);

            var listLayout = listGo.AddComponent<VerticalLayoutGroup>();
            listLayout.padding = new RectOffset(8, 8, 8, 8);
            listLayout.spacing = 6f;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;
            listLayout.childAlignment = TextAnchor.UpperLeft;

            // Details panel
            var detailsText = CreateLabel(panelGo.transform, "Details",
                "Select a map to see details", 14, FontStyles.Normal, font);
            var detailsRect = detailsText.GetComponent<RectTransform>();
            detailsRect.anchorMin = new Vector2(0.55f, 0.25f);
            detailsRect.anchorMax = new Vector2(0.95f, 0.9f);
            detailsRect.offsetMin = Vector2.zero;
            detailsRect.offsetMax = Vector2.zero;
            detailsText.alignment = TextAlignmentOptions.TopLeft;
            detailsText.richText = true;

            // Start button
            var startBtnGo = new GameObject("StartButton");
            startBtnGo.transform.SetParent(panelGo.transform, false);
            var startRect = startBtnGo.AddComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.3f, 0.05f);
            startRect.anchorMax = new Vector2(0.7f, 0.18f);
            startRect.offsetMin = Vector2.zero;
            startRect.offsetMax = Vector2.zero;

            var startBg = startBtnGo.AddComponent<Image>();
            startBg.color = new Color(0.2f, 0.5f, 0.25f, 0.9f);

            var startBtn = startBtnGo.AddComponent<Button>();
            var startColors = startBtn.colors;
            startColors.highlightedColor = new Color(0.3f, 0.6f, 0.35f);
            startColors.pressedColor = new Color(0.15f, 0.4f, 0.2f);
            startBtn.colors = startColors;

            var startLabel = CreateLabel(startBtnGo.transform, "Label",
                "Start Game", 18, FontStyles.Bold, font);
            var startLabelRect = startLabel.GetComponent<RectTransform>();
            startLabelRect.anchorMin = Vector2.zero;
            startLabelRect.anchorMax = Vector2.one;
            startLabelRect.offsetMin = Vector2.zero;
            startLabelRect.offsetMax = Vector2.zero;
            startLabel.alignment = TextAlignmentOptions.Center;

            // Component
            var ui = panelGo.AddComponent<MapSelectionUI>();
            SetField(ui, "_panelRoot", panelGo);
            SetField(ui, "_mapListContainer", listGo.transform);
            SetField(ui, "_titleText", titleText);
            SetField(ui, "_detailsText", detailsText);

            startBtn.onClick.AddListener(ui.OnStartClicked);

            // Populate map buttons
            foreach (var mapId in MapFactory.GetMapIds())
            {
                var info = MapFactory.CreateMap(mapId);
                CreateMapButton(listGo.transform, ui, mapId,
                    $"{info.DisplayName}", font);
            }

            return ui;
        }

        private static void CreateMapButton(Transform parent, MapSelectionUI ui,
            string mapId, string label, TMP_FontAsset font)
        {
            var btnGo = new GameObject($"Map_{mapId}");
            btnGo.transform.SetParent(parent, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 36f);

            var layoutElem = btnGo.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 36f;

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.22f, 0.25f, 0.9f);

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.4f, 0.35f);
            colors.pressedColor = new Color(0.2f, 0.35f, 0.25f);
            btn.colors = colors;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 0f);
            textRect.offsetMax = new Vector2(-10f, 0f);

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;
            if (font != null) tmp.font = font;

            string capturedId = mapId;
            btn.onClick.AddListener(() => ui.OnMapClicked(capturedId));
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name,
            string text, float fontSize, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, fontSize + 6f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;
            if (font != null) tmp.font = font;

            return tmp;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
