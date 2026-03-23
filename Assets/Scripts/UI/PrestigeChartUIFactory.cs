using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Factory that builds the PrestigeChartUI panel programmatically.
    /// Extracted from PrestigeChartUI to keep both files under 300 lines.
    /// </summary>
    public static class PrestigeChartUIFactory
    {
        private static readonly Color LOCKED_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.9f);

        /// <summary>Create the prestige chart UI programmatically.</summary>
        public static PrestigeChartUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("PrestigeChart");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

            // Title
            var titleText = CreateLabel(panelGo.transform, "Title",
                "Prestige Unlocks", 22, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(0f, 30f);
            titleText.alignment = TextAlignmentOptions.Center;

            // Status line
            var statusText = CreateLabel(panelGo.transform, "Status",
                "Points: 0  Level: 0  Unspent: 0", 15, FontStyles.Normal, font);
            statusText.color = new Color(0.9f, 0.85f, 0.5f);
            var statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.anchoredPosition = new Vector2(0f, -40f);
            statusRect.sizeDelta = new Vector2(0f, 22f);
            statusText.alignment = TextAlignmentOptions.Center;

            // Three columns
            var columnsRoot = new GameObject("Columns");
            columnsRoot.transform.SetParent(panelGo.transform, false);
            var columnsRect = columnsRoot.AddComponent<RectTransform>();
            columnsRect.anchorMin = new Vector2(0f, 0f);
            columnsRect.anchorMax = new Vector2(1f, 1f);
            columnsRect.offsetMin = new Vector2(10f, 10f);
            columnsRect.offsetMax = new Vector2(-10f, -70f);

            var columnsLayout = columnsRoot.AddComponent<HorizontalLayoutGroup>();
            columnsLayout.spacing = 10f;
            columnsLayout.childForceExpandWidth = true;
            columnsLayout.childForceExpandHeight = true;
            columnsLayout.padding = new RectOffset(5, 5, 5, 5);

            var ecoCol = CreateColumn(columnsRoot.transform, "Economy", font);
            var milCol = CreateColumn(columnsRoot.transform, "Military", font);
            var culCol = CreateColumn(columnsRoot.transform, "Culture", font);

            // Component
            var chart = panelGo.AddComponent<PrestigeChartUI>();
            SetField(chart, "_panelRoot", panelGo);
            SetField(chart, "_titleText", titleText);
            SetField(chart, "_statusText", statusText);
            SetField(chart, "_economyColumn", ecoCol.transform);
            SetField(chart, "_militaryColumn", milCol.transform);
            SetField(chart, "_cultureColumn", culCol.transform);

            // Populate unlock nodes
            foreach (var def in PrestigeDatabase.All)
            {
                Transform col = def.Branch switch
                {
                    PrestigeDatabase.PrestigeBranch.Economy => ecoCol.transform,
                    PrestigeDatabase.PrestigeBranch.Military => milCol.transform,
                    PrestigeDatabase.PrestigeBranch.Culture => culCol.transform,
                    _ => ecoCol.transform
                };
                CreateNode(col, chart, def, font);
            }

            panelGo.SetActive(false);
            return chart;
        }

        private static GameObject CreateColumn(Transform parent, string label,
            TMP_FontAsset font)
        {
            var colGo = new GameObject($"Col_{label}");
            colGo.transform.SetParent(parent, false);

            colGo.AddComponent<RectTransform>();

            var colBg = colGo.AddComponent<Image>();
            colBg.color = new Color(0.12f, 0.12f, 0.15f, 0.8f);

            var colLayout = colGo.AddComponent<VerticalLayoutGroup>();
            colLayout.padding = new RectOffset(6, 6, 6, 6);
            colLayout.spacing = 4f;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = false;
            colLayout.childAlignment = TextAnchor.UpperCenter;

            var headerText = CreateLabel(colGo.transform, "Header", label, 16,
                FontStyles.Bold, font);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = new Color(0.9f, 0.8f, 0.4f);

            return colGo;
        }

        private static void CreateNode(Transform column, PrestigeChartUI chart,
            PrestigeDatabase.PrestigeUnlockDef def, TMP_FontAsset font)
        {
            var nodeGo = new GameObject($"Node_{def.Id}");
            nodeGo.transform.SetParent(column, false);

            var nodeRect = nodeGo.AddComponent<RectTransform>();
            nodeRect.sizeDelta = new Vector2(0f, 44f);

            var layoutElem = nodeGo.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 44f;

            var nodeImg = nodeGo.AddComponent<Image>();
            nodeImg.color = LOCKED_COLOR;

            var btn = nodeGo.AddComponent<Button>();
            string capturedId = def.Id;
            btn.onClick.AddListener(() => chart.HandleNodeClicked(capturedId));

            // Name text
            var nameText = CreateLabel(nodeGo.transform, "Name", def.DisplayName, 12,
                FontStyles.Bold, font);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(6f, 0f);
            nameRect.offsetMax = new Vector2(-6f, -2f);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;

            // Description text
            var descText = CreateLabel(nodeGo.transform, "Desc", $"Lv{def.MinLevel}: {def.Description}",
                10, FontStyles.Normal, font);
            descText.color = new Color(0.7f, 0.7f, 0.7f);
            var descRect = descText.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 0.5f);
            descRect.offsetMin = new Vector2(6f, 2f);
            descRect.offsetMax = new Vector2(-6f, 0f);
            descText.alignment = TextAlignmentOptions.MidlineLeft;

            chart.RegisterNode(def.Id, nodeImg, btn);
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
