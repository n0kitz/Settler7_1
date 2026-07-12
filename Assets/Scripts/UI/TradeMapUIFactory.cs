using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Factory for the parchment trade map (§14.7): full-screen parchment with
    /// compass rose and lat/long grid, the player capital (castle, gold frame)
    /// in the center, and outpost cards connected by dotted routes. Nodes are
    /// rebuilt from the LIVE trade map every time the panel opens.
    /// </summary>
    public static class TradeMapUIFactory
    {
        private static readonly Color INK = new(0.30f, 0.20f, 0.10f);
        private static readonly Color INK_FADED = new(0.42f, 0.30f, 0.16f, 0.55f);
        private static readonly Color CARD_BG = new(0.74f, 0.65f, 0.47f);

        public static TradeMapUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var (frameGo, contentGo) = UIFactory.CreateOrnatePanel(canvasTransform, "TradeMapUI");
            var frameRect = frameGo.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.06f, 0.06f);
            frameRect.anchorMax = new Vector2(0.94f, 0.94f);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;

            var contentImg = contentGo.GetComponent<Image>();
            contentImg.sprite = MapArtFactory.Parchment();
            contentImg.color = Color.white;

            var titleText = UIFactory.CreateLabel(contentGo.transform, "Title",
                L.Get("ui.trade.title"), 26, FontStyles.Bold, font);
            titleText.color = INK;
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -6f);
            titleRect.sizeDelta = new Vector2(0f, 32f);
            titleText.alignment = TextAlignmentOptions.Center;

            var statusText = UIFactory.CreateLabel(contentGo.transform, "Status",
                "", 14, FontStyles.Normal, font);
            statusText.color = INK_FADED;
            var statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.anchoredPosition = new Vector2(0f, -38f);
            statusRect.sizeDelta = new Vector2(0f, 20f);
            statusText.alignment = TextAlignmentOptions.Center;

            // Map area: grid + compass + routes + nodes live here
            var mapAreaGo = new GameObject("MapArea");
            mapAreaGo.transform.SetParent(contentGo.transform, false);
            var mapRect = mapAreaGo.AddComponent<RectTransform>();
            mapRect.anchorMin = Vector2.zero;
            mapRect.anchorMax = Vector2.one;
            mapRect.offsetMin = new Vector2(14f, 26f);
            mapRect.offsetMax = new Vector2(-14f, -62f);

            MapArtFactory.CreateGrid(mapAreaGo.transform, 6, 4);

            var compassGo = new GameObject("Compass");
            compassGo.transform.SetParent(mapAreaGo.transform, false);
            var compassRect = compassGo.AddComponent<RectTransform>();
            compassRect.anchorMin = new Vector2(0f, 0f);
            compassRect.anchorMax = new Vector2(0f, 0f);
            compassRect.anchoredPosition = new Vector2(52f, 52f);
            compassRect.sizeDelta = new Vector2(72f, 72f);
            var compassImg = compassGo.AddComponent<Image>();
            compassImg.sprite = MapArtFactory.Compass();
            compassImg.color = new Color(1f, 1f, 1f, 0.55f);

            var routesGo = new GameObject("Routes");
            routesGo.transform.SetParent(mapAreaGo.transform, false);
            Stretch(routesGo.AddComponent<RectTransform>());

            var nodesGo = new GameObject("Nodes");
            nodesGo.transform.SetParent(mapAreaGo.transform, false);
            Stretch(nodesGo.AddComponent<RectTransform>());

            var capitalLabel = CreateCapitalNode(mapAreaGo.transform, font);

            var legendText = UIFactory.CreateLabel(contentGo.transform, "Legend",
                LegendText(), 12, FontStyles.Normal, font);
            legendText.color = INK;
            legendText.richText = true;
            var legendRect = legendText.GetComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0f, 0f);
            legendRect.anchorMax = new Vector2(1f, 0f);
            legendRect.pivot = new Vector2(0.5f, 0f);
            legendRect.anchoredPosition = new Vector2(0f, 5f);
            legendRect.sizeDelta = new Vector2(0f, 18f);
            legendText.alignment = TextAlignmentOptions.Center;

            var ui = frameGo.AddComponent<TradeMapUI>();
            UIFactory.SetField(ui, "_panelRoot", frameGo);
            UIFactory.SetField(ui, "_titleText", titleText);
            UIFactory.SetField(ui, "_statusText", statusText);
            ui.MapFont = font;
            ui.RoutesRoot = routesGo.transform;
            ui.NodesRoot = nodesGo.transform;
            ui.LegendLabel = legendText;
            ui.CapitalLabel = capitalLabel;

            frameGo.SetActive(false);
            return ui;
        }

        /// <summary>
        /// Clear and rebuild all outpost nodes + routes from the live trade map
        /// (falls back to the test map before a game exists).
        /// </summary>
        public static void RebuildNodes(TradeMapUI ui)
        {
            if (ui.NodesRoot == null || ui.RoutesRoot == null) return;

            ClearChildren(ui.RoutesRoot);
            ClearChildren(ui.NodesRoot);
            ui._outpostImages.Clear();
            ui._outpostLabels.Clear();

            var gc = Presentation.GameController.Instance;
            TradeMap map = gc != null && gc.State != null
                ? gc.State.Trade.Map
                : TestTradeMapFactory.CreateTestTradeMap();

            var outposts = map.AllOutposts;
            var center = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < outposts.Count; i++)
            {
                var anchor = SpiralAnchor(i, outposts.Count);
                MapArtFactory.CreateDottedRoute(ui.RoutesRoot, center, anchor, INK_FADED);
                CreateOutpostNode(ui.NodesRoot, ui, outposts[i], anchor, ui.MapFont);
            }
        }

        /// <summary>Deterministic golden-angle spiral position, normalized 0–1.</summary>
        private static Vector2 SpiralAnchor(int index, int count)
        {
            float angle = index * 2.399963f + 0.9f;
            float radius = 0.17f + 0.27f * Mathf.Sqrt((index + 1f) / count);
            float x = 0.5f + Mathf.Cos(angle) * radius * 0.92f;
            float y = 0.5f + Mathf.Sin(angle) * radius * 0.82f;
            return new Vector2(Mathf.Clamp(x, 0.10f, 0.90f), Mathf.Clamp(y, 0.12f, 0.88f));
        }

        /// <summary>Claim-state legend, re-resolved on every Show (locale can change).</summary>
        internal static string LegendText()
        {
            return "<color=#B08020>■</color> " + L.Get("ui.trade.legend.yours") + "   " +
                "<color=#9E2A1E>■</color> " + L.Get("ui.trade.legend.other") + "   " +
                "<color=#7A7266>■</color> " + L.Get("ui.trade.legend.unclaimed");
        }

        private static TextMeshProUGUI CreateCapitalNode(Transform mapArea, TMP_FontAsset font)
        {
            var nodeGo = new GameObject("Capital");
            nodeGo.transform.SetParent(mapArea, false);
            var rect = nodeGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(96f, 74f);
            nodeGo.AddComponent<Image>().color = TradeMapUI.OWNED_COLOR;

            var inner = new GameObject("Content");
            inner.transform.SetParent(nodeGo.transform, false);
            var innerRect = inner.AddComponent<RectTransform>();
            Stretch(innerRect);
            innerRect.offsetMin = new Vector2(2.5f, 2.5f);
            innerRect.offsetMax = new Vector2(-2.5f, -2.5f);
            inner.AddComponent<Image>().color = CARD_BG;

            var iconGo = new GameObject("CastleIcon");
            iconGo.transform.SetParent(inner.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -4f);
            iconRect.sizeDelta = new Vector2(40f, 40f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = IconFactory.Castle();

            var label = UIFactory.CreateLabel(inner.transform, "Name",
                L.Get("ui.trade.capital"), 12, FontStyles.Bold, font);
            label.color = INK;
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 2f);
            labelRect.sizeDelta = new Vector2(0f, 18f);
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }

        private static void CreateOutpostNode(Transform parent, TradeMapUI ui,
            TradeOutpost outpost, Vector2 anchor, TMP_FontAsset font)
        {
            var nodeGo = new GameObject($"Outpost_{outpost.Id}");
            nodeGo.transform.SetParent(parent, false);
            var rect = nodeGo.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(152f, 48f);

            var frameImg = nodeGo.AddComponent<Image>();
            frameImg.color = outpost.IsSpecial
                ? TradeMapUI.SPECIAL_COLOR : TradeMapUI.UNCLAIMED_COLOR;

            var btn = nodeGo.AddComponent<Button>();
            string capturedId = outpost.Id;
            btn.onClick.AddListener(() => ui.OnOutpostClicked(capturedId));

            var inner = new GameObject("Content");
            inner.transform.SetParent(nodeGo.transform, false);
            var innerRect = inner.AddComponent<RectTransform>();
            Stretch(innerRect);
            innerRect.offsetMin = new Vector2(2.5f, 2.5f);
            innerRect.offsetMax = new Vector2(-2.5f, -2.5f);
            inner.AddComponent<Image>().color = CARD_BG;

            float textLeft = 6f;
            if (outpost.IsSpecial)
            {
                var chestGo = new GameObject("Chest");
                chestGo.transform.SetParent(inner.transform, false);
                var chestRect = chestGo.AddComponent<RectTransform>();
                chestRect.anchorMin = new Vector2(0f, 0.5f);
                chestRect.anchorMax = new Vector2(0f, 0.5f);
                chestRect.anchoredPosition = new Vector2(13f, 0f);
                chestRect.sizeDelta = new Vector2(20f, 20f);
                chestGo.AddComponent<Image>().sprite = IconFactory.Chest();
                textLeft = 24f;
            }

            var nameText = UIFactory.CreateLabel(inner.transform, "Name",
                LocalizedNames.Outpost(outpost), 12, FontStyles.Bold, font);
            nameText.color = INK;
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(textLeft, 0f);
            nameRect.offsetMax = new Vector2(-4f, -1f);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;

            var exchangeText = UIFactory.CreateLabel(inner.transform, "Exchange",
                TradeMapUI.ExchangeText(outpost, 0), 11, FontStyles.Normal, font);
            exchangeText.color = INK;
            var exchangeRect = exchangeText.GetComponent<RectTransform>();
            exchangeRect.anchorMin = new Vector2(0f, 0f);
            exchangeRect.anchorMax = new Vector2(1f, 0.5f);
            exchangeRect.offsetMin = new Vector2(textLeft, 1f);
            exchangeRect.offsetMax = new Vector2(-4f, 0f);
            exchangeText.alignment = TextAlignmentOptions.MidlineLeft;

            ui._outpostImages[outpost.Id] = frameImg;
            ui._outpostLabels[outpost.Id] = exchangeText;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
                Object.Destroy(root.GetChild(i).gameObject);
        }
    }
}
