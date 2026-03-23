using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Factory for programmatic creation of the TechTreeUI panel.
    /// </summary>
    public static class TechTreeUIFactory
    {
        public static TechTreeUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("TechTreeUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.06f, 0.08f, 0.1f, 0.95f);

            var titleText = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Technology Tree", 22, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(0f, 30f);
            titleText.alignment = TextAlignmentOptions.Center;

            var statusText = UIFactory.CreateLabel(panelGo.transform, "Status",
                "Technologies Researched: 0/18", 14, FontStyles.Normal, font);
            statusText.color = new Color(0.8f, 0.85f, 0.5f);
            var statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.anchoredPosition = new Vector2(0f, -40f);
            statusRect.sizeDelta = new Vector2(0f, 20f);
            statusText.alignment = TextAlignmentOptions.Center;

            var columnsRoot = new GameObject("Tiers");
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

            var tier1Col = CreateTierColumn(columnsRoot.transform, "Tier 1", font);
            var tier2Col = CreateTierColumn(columnsRoot.transform, "Tier 2", font);
            var tier3Col = CreateTierColumn(columnsRoot.transform, "Tier 3", font);

            var ui = panelGo.AddComponent<TechTreeUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);
            UIFactory.SetField(ui, "_titleText", titleText);
            UIFactory.SetField(ui, "_statusText", statusText);
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
                CreateTechNode(col, ui, tech, font);
            }

            var legendText = UIFactory.CreateLabel(panelGo.transform, "Legend",
                "<color=#4070A0>Available</color>  " +
                "<color=#40A050>Owned</color>  " +
                "<color=#A04040>Taken</color>  " +
                "<color=#808020>Blocked</color>  " +
                "<color=#708030>Researching</color>",
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

        private static GameObject CreateTierColumn(Transform parent, string label,
            TMP_FontAsset font)
        {
            var colGo = new GameObject($"Col_{label}");
            colGo.transform.SetParent(parent, false);
            colGo.AddComponent<RectTransform>();

            var colBg = colGo.AddComponent<Image>();
            colBg.color = new Color(0.1f, 0.12f, 0.14f, 0.8f);

            var colLayout = colGo.AddComponent<VerticalLayoutGroup>();
            colLayout.padding = new RectOffset(6, 6, 6, 6);
            colLayout.spacing = 4f;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = false;
            colLayout.childAlignment = TextAnchor.UpperCenter;

            var headerText = UIFactory.CreateLabel(colGo.transform, "Header", label, 16,
                FontStyles.Bold, font);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = new Color(0.7f, 0.85f, 0.95f);

            return colGo;
        }

        private static void CreateTechNode(Transform column, TechTreeUI ui,
            TechTree.TechDef def, TMP_FontAsset font)
        {
            var nodeGo = new GameObject($"Tech_{def.Id}");
            nodeGo.transform.SetParent(column, false);

            var nodeRect = nodeGo.AddComponent<RectTransform>();
            nodeRect.sizeDelta = new Vector2(0f, 50f);

            var layoutElem = nodeGo.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 50f;

            var nodeImg = nodeGo.AddComponent<Image>();
            nodeImg.color = TechTreeUI.LOCKED_COLOR;

            var btn = nodeGo.AddComponent<Button>();
            string capturedId = def.Id;
            btn.onClick.AddListener(() => ui.OnTechClicked(capturedId));

            var nameText = UIFactory.CreateLabel(nodeGo.transform, "Name", def.DisplayName, 13,
                FontStyles.Bold, font);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(6f, 0f);
            nameRect.offsetMax = new Vector2(-6f, -2f);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;

            string descContent = def.Description + $" ({def.ResearchTime:0}s)";
            if (def.PrerequisiteId != null)
            {
                var prereq = TechTree.Get(def.PrerequisiteId);
                if (prereq != null)
                    descContent += $"  Requires: {prereq.DisplayName}";
            }
            var descText = UIFactory.CreateLabel(nodeGo.transform, "Desc", descContent,
                10, FontStyles.Italic, font);
            descText.color = new Color(0.75f, 0.75f, 0.75f);
            var descRect = descText.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 0.5f);
            descRect.offsetMin = new Vector2(6f, 2f);
            descRect.offsetMax = new Vector2(-6f, 0f);
            descText.alignment = TextAlignmentOptions.MidlineLeft;

            ui._nodeImages[def.Id] = nodeImg;
            ui._nodeLabels[def.Id] = nameText;
        }
    }
}
