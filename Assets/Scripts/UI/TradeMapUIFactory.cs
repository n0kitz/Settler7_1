using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Factory for programmatic creation of the TradeMapUI panel.
    /// </summary>
    public static class TradeMapUIFactory
    {
        public static TradeMapUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("TradeMapUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.07f, 0.05f, 0.95f);

            var titleText = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Trade Map", 22, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(0f, 30f);
            titleText.alignment = TextAlignmentOptions.Center;

            var statusText = UIFactory.CreateLabel(panelGo.transform, "Status",
                "Outposts Claimed: 0/8", 14, FontStyles.Normal, font);
            statusText.color = new Color(0.9f, 0.85f, 0.6f);
            var statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.anchoredPosition = new Vector2(0f, -40f);
            statusRect.sizeDelta = new Vector2(0f, 20f);
            statusText.alignment = TextAlignmentOptions.Center;

            var columnsRoot = new GameObject("Columns");
            columnsRoot.transform.SetParent(panelGo.transform, false);
            var columnsRect = columnsRoot.AddComponent<RectTransform>();
            columnsRect.anchorMin = new Vector2(0f, 0f);
            columnsRect.anchorMax = new Vector2(1f, 1f);
            columnsRect.offsetMin = new Vector2(10f, 10f);
            columnsRect.offsetMax = new Vector2(-10f, -68f);

            var columnsLayout = columnsRoot.AddComponent<HorizontalLayoutGroup>();
            columnsLayout.spacing = 10f;
            columnsLayout.childForceExpandWidth = true;
            columnsLayout.childForceExpandHeight = true;
            columnsLayout.padding = new RectOffset(5, 5, 5, 5);

            var outpostsCol = CreateScrollColumn(columnsRoot.transform, "Outposts", font);
            var tradesCol = CreateScrollColumn(columnsRoot.transform, "Active Traders", font);

            var ui = panelGo.AddComponent<TradeMapUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);
            UIFactory.SetField(ui, "_titleText", titleText);
            UIFactory.SetField(ui, "_statusText", statusText);
            UIFactory.SetField(ui, "_outpostsContainer", outpostsCol.transform);
            UIFactory.SetField(ui, "_activeTradesContainer", tradesCol.transform);

            var tradeMap = TestTradeMapFactory.CreateTestTradeMap();
            foreach (var outpost in tradeMap.AllOutposts)
                CreateOutpostNode(outpostsCol.transform, ui, outpost, font);

            var legendText = UIFactory.CreateLabel(panelGo.transform, "Legend",
                "<color=#509060>Yours</color>  " +
                "<color=#A04535>Other</color>  " +
                "<color=#A08030>Special</color>  " +
                "<color=#556070>Unclaimed</color>  " +
                "Click to claim/trade",
                11, FontStyles.Normal, font);
            var legendRect = legendText.GetComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0f, 0f);
            legendRect.anchorMax = new Vector2(1f, 0f);
            legendRect.pivot = new Vector2(0.5f, 0f);
            legendRect.anchoredPosition = new Vector2(0f, 2f);
            legendRect.sizeDelta = new Vector2(0f, 16f);
            legendText.alignment = TextAlignmentOptions.Center;
            legendText.richText = true;

            panelGo.SetActive(false);
            return ui;
        }

        private static void CreateOutpostNode(Transform column, TradeMapUI ui,
            TradeOutpost outpost, TMP_FontAsset font)
        {
            var nodeGo = new GameObject($"Outpost_{outpost.Id}");
            nodeGo.transform.SetParent(column, false);
            nodeGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 52f);
            nodeGo.AddComponent<LayoutElement>().preferredHeight = 52f;

            var nodeImg = nodeGo.AddComponent<Image>();
            nodeImg.color = outpost.IsSpecial ? TradeMapUI.SPECIAL_COLOR : TradeMapUI.UNCLAIMED_COLOR;

            var btn = nodeGo.AddComponent<Button>();
            string capturedId = outpost.Id;
            btn.onClick.AddListener(() => ui.OnOutpostClicked(capturedId));

            var nameText = UIFactory.CreateLabel(nodeGo.transform, "Name", outpost.DisplayName, 13,
                FontStyles.Bold, font);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(6f, 0f);
            nameRect.offsetMax = new Vector2(-6f, -2f);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;

            string exchangeText = $"{outpost.InputAmount} {outpost.InputResource} → " +
                $"{outpost.OutputAmount} {outpost.OutputResource}";
            if (outpost.IsSpecial) exchangeText += " [SPECIAL]";
            var descText = UIFactory.CreateLabel(nodeGo.transform, "Exchange", exchangeText,
                11, FontStyles.Normal, font);
            descText.color = new Color(0.8f, 0.75f, 0.6f);
            var descRect = descText.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 0.5f);
            descRect.offsetMin = new Vector2(6f, 2f);
            descRect.offsetMax = new Vector2(-6f, 0f);
            descText.alignment = TextAlignmentOptions.MidlineLeft;

            ui._outpostImages[outpost.Id] = nodeImg;
            ui._outpostLabels[outpost.Id] = nameText;
        }

        private static GameObject CreateScrollColumn(Transform parent, string label,
            TMP_FontAsset font)
        {
            var colGo = new GameObject($"Col_{label}");
            colGo.transform.SetParent(parent, false);
            colGo.AddComponent<RectTransform>();

            var colBg = colGo.AddComponent<Image>();
            colBg.color = new Color(0.1f, 0.1f, 0.08f, 0.8f);

            var colLayout = colGo.AddComponent<VerticalLayoutGroup>();
            colLayout.padding = new RectOffset(6, 6, 6, 6);
            colLayout.spacing = 3f;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = false;
            colLayout.childAlignment = TextAnchor.UpperLeft;

            var headerText = UIFactory.CreateLabel(colGo.transform, "Header", label, 15,
                FontStyles.Bold, font);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = new Color(0.85f, 0.75f, 0.45f);

            return colGo;
        }
    }
}
