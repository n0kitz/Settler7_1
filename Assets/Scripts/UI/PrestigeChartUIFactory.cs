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
            panelBg.color = UIColors.PANEL_BLUE_DARK;

            // Title
            var titleText = UIFactory.CreateLabel(panelGo.transform, "Title",
                L.Get("ui.prestige.title"), 22, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(0f, 30f);
            titleText.alignment = TextAlignmentOptions.Center;

            // Status line
            var statusText = UIFactory.CreateLabel(panelGo.transform, "Status",
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

            var ecoCol = CreateColumn(columnsRoot.transform,
                L.Get("ui.prestige.branch.economy"), font);
            var milCol = CreateColumn(columnsRoot.transform,
                L.Get("ui.prestige.branch.military"), font);
            var culCol = CreateColumn(columnsRoot.transform,
                L.Get("ui.prestige.branch.culture"), font);

            // Component
            var chart = panelGo.AddComponent<PrestigeChartUI>();
            UIFactory.SetField(chart, "_panelRoot", panelGo);
            UIFactory.SetField(chart, "_titleText", titleText);
            UIFactory.SetField(chart, "_statusText", statusText);
            UIFactory.SetField(chart, "_economyColumn", ecoCol.transform);
            UIFactory.SetField(chart, "_militaryColumn", milCol.transform);
            UIFactory.SetField(chart, "_cultureColumn", culCol.transform);
            chart.BranchHeaders[0] = ecoCol.transform.Find("Header").GetComponent<TextMeshProUGUI>();
            chart.BranchHeaders[1] = milCol.transform.Find("Header").GetComponent<TextMeshProUGUI>();
            chart.BranchHeaders[2] = culCol.transform.Find("Header").GetComponent<TextMeshProUGUI>();

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
            colBg.color = UIColors.PANEL_GRAY_MEDIUM;

            var colLayout = colGo.AddComponent<VerticalLayoutGroup>();
            colLayout.padding = new RectOffset(6, 6, 6, 6);
            colLayout.spacing = 4f;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = false;
            colLayout.childAlignment = TextAnchor.UpperCenter;

            var headerText = UIFactory.CreateLabel(colGo.transform, "Header", label, 16,
                FontStyles.Bold, font);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = UIColors.ACCENT_ORANGE;

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
            nodeImg.color = PrestigeChartUI.LOCKED_COLOR;

            var btn = nodeGo.AddComponent<Button>();
            string capturedId = def.Id;
            btn.onClick.AddListener(() => chart.HandleNodeClicked(capturedId));

            // Name text
            var nameText = UIFactory.CreateLabel(nodeGo.transform, "Name",
                LocalizedNames.Prestige(def.Id), 12, FontStyles.Bold, font);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(6f, 0f);
            nameRect.offsetMax = new Vector2(-6f, -2f);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;

            // Description text
            var descText = UIFactory.CreateLabel(nodeGo.transform, "Desc",
                string.Format(L.Get("ui.prestige.node_desc"), def.MinLevel,
                    LocalizedNames.PrestigeDescription(def.Id)),
                10, FontStyles.Normal, font);
            descText.color = new Color(0.7f, 0.7f, 0.7f);
            var descRect = descText.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 0.5f);
            descRect.offsetMin = new Vector2(6f, 2f);
            descRect.offsetMax = new Vector2(-6f, 0f);
            descText.alignment = TextAlignmentOptions.MidlineLeft;

            chart.RegisterNode(def.Id, nodeImg, btn);
            chart.RegisterNodeLabels(def.Id, nameText, descText);
        }

    }
}
