using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Factory for the ÜBERSICHT stats panel (§14.1): good selector on the
    /// left, the four verified columns (ERFORDERT / PRODUZIERT VON /
    /// ERBRINGT / VERBRAUCHT VON) on the right.
    /// </summary>
    public static class StatsOverviewUIFactory
    {
        public static StatsOverviewUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var (frameGo, contentGo) = UIFactory.CreateOrnatePanel(canvasTransform,
                "StatsOverviewUI");
            var frameRect = frameGo.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.12f, 0.10f);
            frameRect.anchorMax = new Vector2(0.88f, 0.90f);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;

            var titleText = UIFactory.CreateLabel(contentGo.transform, "Title",
                L.Get("ui.stats.title"), 26, FontStyles.Bold, font);
            titleText.color = UIColors.TEXT_HEADER_GOLD;
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -6f);
            titleRect.sizeDelta = new Vector2(0f, 32f);
            titleText.alignment = TextAlignmentOptions.Center;

            var subtitleText = UIFactory.CreateLabel(contentGo.transform, "Subtitle",
                "", 15, FontStyles.Normal, font);
            subtitleText.color = UIColors.TEXT_GOLD;
            var subtitleRect = subtitleText.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0f, 1f);
            subtitleRect.anchorMax = new Vector2(1f, 1f);
            subtitleRect.pivot = new Vector2(0.5f, 1f);
            subtitleRect.anchoredPosition = new Vector2(0f, -38f);
            subtitleRect.sizeDelta = new Vector2(0f, 22f);
            subtitleText.alignment = TextAlignmentOptions.Center;

            var ui = frameGo.AddComponent<StatsOverviewUI>();
            ui.PanelFont = font;

            CreateSelector(contentGo.transform, ui, font);

            var columnsRoot = new GameObject("Columns");
            columnsRoot.transform.SetParent(contentGo.transform, false);
            var columnsRect = columnsRoot.AddComponent<RectTransform>();
            columnsRect.anchorMin = new Vector2(0f, 0f);
            columnsRect.anchorMax = new Vector2(1f, 1f);
            columnsRect.offsetMin = new Vector2(190f, 12f);
            columnsRect.offsetMax = new Vector2(-12f, -66f);

            var columnsLayout = columnsRoot.AddComponent<HorizontalLayoutGroup>();
            columnsLayout.spacing = 8f;
            columnsLayout.childForceExpandWidth = true;
            columnsLayout.childForceExpandHeight = true;

            var requiresCol = CreateColumn(columnsRoot.transform,
                L.Get("ui.stats.col.requires"), font);
            var producedByCol = CreateColumn(columnsRoot.transform,
                L.Get("ui.stats.col.produced_by"), font);
            var yieldsCol = CreateColumn(columnsRoot.transform,
                L.Get("ui.stats.col.yields"), font);
            var consumedByCol = CreateColumn(columnsRoot.transform,
                L.Get("ui.stats.col.consumed_by"), font);

            UIFactory.SetField(ui, "_panelRoot", frameGo);
            UIFactory.SetField(ui, "_titleText", titleText);
            UIFactory.SetField(ui, "_subtitleText", subtitleText);
            UIFactory.SetField(ui, "_requiresCol", requiresCol.transform);
            UIFactory.SetField(ui, "_producedByCol", producedByCol.transform);
            UIFactory.SetField(ui, "_yieldsCol", yieldsCol.transform);
            UIFactory.SetField(ui, "_consumedByCol", consumedByCol.transform);

            frameGo.SetActive(false);
            return ui;
        }

        /// <summary>Good selector: every resource that appears in a recipe.</summary>
        private static void CreateSelector(Transform content, StatsOverviewUI ui,
            TMP_FontAsset font)
        {
            var selectorGo = new GameObject("Selector");
            selectorGo.transform.SetParent(content, false);
            var selectorRect = selectorGo.AddComponent<RectTransform>();
            selectorRect.anchorMin = new Vector2(0f, 0f);
            selectorRect.anchorMax = new Vector2(0f, 1f);
            selectorRect.pivot = new Vector2(0f, 0.5f);
            selectorRect.anchoredPosition = new Vector2(12f, 0f);
            selectorRect.sizeDelta = new Vector2(170f, -78f);
            selectorGo.AddComponent<Image>().color = UIColors.PANEL_OLIVE_LIGHT;

            var grid = selectorGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(80f, 22f);
            grid.spacing = new Vector2(3f, 3f);
            grid.padding = new RectOffset(3, 3, 3, 3);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            foreach (var resource in GatherRecipeResources())
            {
                var btnGo = new GameObject($"Res_{resource}");
                btnGo.transform.SetParent(selectorGo.transform, false);
                btnGo.AddComponent<RectTransform>();
                var img = btnGo.AddComponent<Image>();
                img.color = UIColors.TILE_BG;

                var captured = resource;
                btnGo.AddComponent<Button>().onClick
                    .AddListener(() => ui.SelectResource(captured));

                var label = UIFactory.CreateLabel(btnGo.transform, "Label",
                    LocalizedNames.Resource(resource), 11, font);
                var labelRect = label.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(4f, 0f);
                labelRect.offsetMax = new Vector2(-2f, 0f);
                label.alignment = TextAlignmentOptions.MidlineLeft;

                ui._selectorImages[resource] = img;
            }
        }

        private static GameObject CreateColumn(Transform parent, string header,
            TMP_FontAsset font)
        {
            var colGo = new GameObject($"Col_{header}");
            colGo.transform.SetParent(parent, false);
            colGo.AddComponent<RectTransform>();
            colGo.AddComponent<Image>().color = UIColors.TILE_BG;

            var layout = colGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 2f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;

            var headerText = UIFactory.CreateLabel(colGo.transform, "Header", header, 13,
                FontStyles.Bold, font);
            headerText.color = UIColors.ACCENT_ORANGE;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.enableAutoSizing = true;   // long verified strings must not truncate
            headerText.fontSizeMin = 8f;
            headerText.fontSizeMax = 13f;
            headerText.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;

            return colGo;
        }

        /// <summary>All resources referenced by any recipe, in enum order.</summary>
        private static List<ResourceType> GatherRecipeResources()
        {
            var set = new HashSet<ResourceType>();
            foreach (var recipe in RecipeDatabase.All)
            {
                foreach (var (type, _) in recipe.Inputs) set.Add(type);
                foreach (var (type, _) in recipe.Outputs) set.Add(type);
            }
            var result = new List<ResourceType>(set);
            result.Sort();
            return result;
        }
    }
}
