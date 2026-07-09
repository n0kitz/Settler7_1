using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Factory for the monastery research panel (§14.6): dark stone background,
    /// candle holders at the sides, one card per technology in three tier
    /// columns. Each card carries a status gem that TechTreeUI recolors.
    /// </summary>
    public static class TechTreeUIFactory
    {
        private static readonly Color CARD_FRAME = new(0.09f, 0.085f, 0.08f);
        private static readonly Color CARD_BG = new(0.27f, 0.26f, 0.245f);
        private static readonly Color CARD_NAME = new(0.92f, 0.85f, 0.66f);
        private static readonly Color CARD_DESC = new(0.72f, 0.70f, 0.66f);
        private static readonly Color TIER_HEADER = new(0.85f, 0.75f, 0.50f);

        public static TechTreeUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var (frameGo, contentGo) = UIFactory.CreateOrnatePanel(canvasTransform, "TechTreeUI");
            var frameRect = frameGo.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.08f, 0.06f);
            frameRect.anchorMax = new Vector2(0.92f, 0.94f);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;

            var contentImg = contentGo.GetComponent<Image>();
            contentImg.sprite = MapArtFactory.Stone();
            contentImg.color = Color.white;

            var titleText = UIFactory.CreateLabel(contentGo.transform, "Title",
                L.Get("ui.tech.title"), 26, FontStyles.Bold, font);
            titleText.color = CARD_NAME;
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -6f);
            titleRect.sizeDelta = new Vector2(0f, 32f);
            titleText.alignment = TextAlignmentOptions.Center;

            var statusText = UIFactory.CreateLabel(contentGo.transform, "Status",
                "", 14, FontStyles.Normal, font);
            statusText.color = UIColors.TEXT_GOLD;
            var statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.anchoredPosition = new Vector2(0f, -38f);
            statusRect.sizeDelta = new Vector2(0f, 20f);
            statusText.alignment = TextAlignmentOptions.Center;

            // Candle holders on the side walls (§14.6)
            MapArtFactory.CreateCandle(contentGo.transform, new Vector2(0.035f, 0.28f));
            MapArtFactory.CreateCandle(contentGo.transform, new Vector2(0.035f, 0.72f));
            MapArtFactory.CreateCandle(contentGo.transform, new Vector2(0.965f, 0.28f));
            MapArtFactory.CreateCandle(contentGo.transform, new Vector2(0.965f, 0.72f));

            var columnsRoot = new GameObject("Tiers");
            columnsRoot.transform.SetParent(contentGo.transform, false);
            var columnsRect = columnsRoot.AddComponent<RectTransform>();
            columnsRect.anchorMin = new Vector2(0f, 0f);
            columnsRect.anchorMax = new Vector2(1f, 1f);
            columnsRect.offsetMin = new Vector2(58f, 28f);
            columnsRect.offsetMax = new Vector2(-58f, -92f);

            var columnsLayout = columnsRoot.AddComponent<HorizontalLayoutGroup>();
            columnsLayout.spacing = 12f;
            columnsLayout.childForceExpandWidth = true;
            columnsLayout.childForceExpandHeight = true;
            columnsLayout.padding = new RectOffset(5, 5, 5, 5);

            var tier1Col = CreateTierColumn(columnsRoot.transform, L.Get("ui.tech.tier1"), font);
            var tier2Col = CreateTierColumn(columnsRoot.transform, L.Get("ui.tech.tier2"), font);
            var tier3Col = CreateTierColumn(columnsRoot.transform, L.Get("ui.tech.tier3"), font);

            var ui = frameGo.AddComponent<TechTreeUI>();
            UIFactory.SetField(ui, "_panelRoot", frameGo);
            UIFactory.SetField(ui, "_titleText", titleText);
            UIFactory.SetField(ui, "_statusText", statusText);

            CreateClericBar(contentGo.transform, ui, font);
            UIFactory.SetField(ui, "_tier1Container", tier1Col.transform);
            UIFactory.SetField(ui, "_tier2Container", tier2Col.transform);
            UIFactory.SetField(ui, "_tier3Container", tier3Col.transform);

            foreach (var tech in TechTree.All)
            {
                Transform col = tech.Tier switch
                {
                    TechTree.TechTier.Tier1 => tier1Col.transform,
                    TechTree.TechTier.Tier2 => tier2Col.transform,
                    TechTree.TechTier.Tier3 => tier3Col.transform,
                    _ => tier1Col.transform
                };
                CreateTechCard(col, ui, tech, font);
            }

            var legendText = UIFactory.CreateLabel(contentGo.transform, "Legend",
                "<color=#59BF66>■</color> " + L.Get("ui.tech.legend.available") + "  " +
                "<color=#F2CC4D>■</color> " + L.Get("ui.tech.legend.owned") + "  " +
                "<color=#CC3326>■</color> " + L.Get("ui.tech.legend.taken") + "  " +
                "<color=#6699E5>■</color> " + L.Get("ui.tech.legend.researching") + "  " +
                "<color=#737373>■</color> " + L.Get("ui.tech.legend.locked"),
                11, FontStyles.Normal, font);
            legendText.color = CARD_DESC;
            legendText.richText = true;
            var legendRect = legendText.GetComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0f, 0f);
            legendRect.anchorMax = new Vector2(1f, 0f);
            legendRect.pivot = new Vector2(0.5f, 0f);
            legendRect.anchoredPosition = new Vector2(0f, 6f);
            legendRect.sizeDelta = new Vector2(0f, 16f);
            legendText.alignment = TextAlignmentOptions.Center;

            frameGo.SetActive(false);
            return ui;
        }

        /// <summary>
        /// Cleric roster row (§14.6): "Geistliche 0/0 [+]  Mönche 0/0 [+]
        /// Prälaten 0/0 [+]" — counts refreshed by TechTreeUI, [+] recruits.
        /// </summary>
        private static void CreateClericBar(Transform content, TechTreeUI ui,
            TMP_FontAsset font)
        {
            var barGo = new GameObject("ClericBar");
            barGo.transform.SetParent(content, false);
            var barRect = barGo.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.anchoredPosition = new Vector2(0f, -60f);
            barRect.sizeDelta = new Vector2(0f, 26f);

            var layout = barGo.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 10f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int rank = 0; rank < 3; rank++)
            {
                var label = UIFactory.CreateLabel(barGo.transform, $"ClericCount_{rank}",
                    "", 13, FontStyles.Bold, font);
                label.color = CARD_NAME;
                label.alignment = TextAlignmentOptions.MidlineRight;
                label.gameObject.AddComponent<LayoutElement>().preferredWidth = 130f;
                ui._clericLabels[rank] = label;

                int captured = rank;
                UIFactory.CreateButton(barGo.transform, "+", font,
                    UIColors.BUTTON_GREEN, () => ui.OnRecruitClicked(captured),
                    new Vector2(24f, 20f), 14f);
            }
        }

        private static GameObject CreateTierColumn(Transform parent, string label,
            TMP_FontAsset font)
        {
            var colGo = new GameObject($"Col_{label}");
            colGo.transform.SetParent(parent, false);
            colGo.AddComponent<RectTransform>();

            var colBg = colGo.AddComponent<Image>();
            colBg.color = new Color(0f, 0f, 0f, 0.28f);

            var colLayout = colGo.AddComponent<VerticalLayoutGroup>();
            colLayout.padding = new RectOffset(7, 7, 7, 7);
            colLayout.spacing = 6f;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = false;
            colLayout.childAlignment = TextAnchor.UpperCenter;

            var headerText = UIFactory.CreateLabel(colGo.transform, "Header", label, 16,
                FontStyles.Bold, font);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = TIER_HEADER;

            return colGo;
        }

        private static void CreateTechCard(Transform column, TechTreeUI ui,
            TechTree.TechDef def, TMP_FontAsset font)
        {
            var cardGo = new GameObject($"Tech_{def.Id}");
            cardGo.transform.SetParent(column, false);
            cardGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 56f);
            cardGo.AddComponent<LayoutElement>().preferredHeight = 56f;
            cardGo.AddComponent<Image>().color = CARD_FRAME;

            var btn = cardGo.AddComponent<Button>();
            string capturedId = def.Id;
            btn.onClick.AddListener(() => ui.OnTechClicked(capturedId));

            var inner = new GameObject("Face");
            inner.transform.SetParent(cardGo.transform, false);
            var innerRect = inner.AddComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(2f, 2f);
            innerRect.offsetMax = new Vector2(-2f, -2f);
            inner.AddComponent<Image>().color = CARD_BG;

            var nameText = UIFactory.CreateLabel(inner.transform, "Name", def.DisplayName, 13,
                FontStyles.Bold, font);
            nameText.color = CARD_NAME;
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(8f, 0f);
            nameRect.offsetMax = new Vector2(-24f, -2f);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;

            string descContent = $"[{def.CostNovices}/{def.CostBrothers}/{def.CostFathers}]  " +
                $"{def.Description}  ({def.ResearchTime:0}s)";
            if (def.PrerequisiteId != null)
            {
                var prereq = TechTree.Get(def.PrerequisiteId);
                if (prereq != null)
                    descContent += $"  ← {prereq.DisplayName}";
            }
            var descText = UIFactory.CreateLabel(inner.transform, "Desc", descContent,
                10, FontStyles.Italic, font);
            descText.color = CARD_DESC;
            var descRect = descText.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 0.5f);
            descRect.offsetMin = new Vector2(8f, 2f);
            descRect.offsetMax = new Vector2(-8f, 0f);
            descText.alignment = TextAlignmentOptions.MidlineLeft;

            // Status gem (wax seal) — recolored by TechTreeUI.RefreshNodes
            var gemGo = new GameObject("Gem");
            gemGo.transform.SetParent(inner.transform, false);
            var gemRect = gemGo.AddComponent<RectTransform>();
            gemRect.anchorMin = new Vector2(1f, 1f);
            gemRect.anchorMax = new Vector2(1f, 1f);
            gemRect.anchoredPosition = new Vector2(-12f, -12f);
            gemRect.sizeDelta = new Vector2(13f, 13f);
            var gemImg = gemGo.AddComponent<Image>();
            gemImg.sprite = MapArtFactory.Disc();
            gemImg.color = TechTreeUI.LOCKED_COLOR;

            ui._nodeImages[def.Id] = gemImg;
            ui._nodeLabels[def.Id] = nameText;
        }
    }
}
