using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Factory that builds the ArmyPanel UI programmatically.
    /// </summary>
    public static class ArmyPanelFactory
    {
        public static ArmyPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("ArmyPanel");
            panelGo.transform.SetParent(canvasTransform, false);

            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.12f, 0.1f);
            rect.anchorMax = new Vector2(0.88f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.06f, 0.08f, 0.95f);

            // Title
            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Army Overview  [M]", 22, FontStyles.Bold, font);
            title.alignment = TextAlignmentOptions.Center;
            title.color = UIColors.TEXT_HEADER_GOLD;
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(0f, 30f);

            // Status line
            var statusText = UIFactory.CreateLabel(panelGo.transform, "Status",
                "Generals: 0/5  |  Total Soldiers: 0", 14, FontStyles.Normal, font);
            statusText.color = UIColors.TEXT_GOLD;
            statusText.alignment = TextAlignmentOptions.Center;
            var statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.anchoredPosition = new Vector2(0f, -42f);
            statusRect.sizeDelta = new Vector2(0f, 20f);

            // Two-column layout
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

            var generalsCol = CreateColumn(columnsRoot.transform, "Generals", font);
            var trainingCol = CreateColumn(columnsRoot.transform, "Training", font);

            var panel = panelGo.AddComponent<ArmyPanel>();
            panel._font = font;

            CreateTrainButton(trainingCol.transform, panel, UnitType.Pikeman,
                "Pikeman (1 Weapon, 8s)", font);
            CreateTrainButton(trainingCol.transform, panel, UnitType.Musketeer,
                "Musketeer (2 Weapons, 12s)", font);
            CreateTrainButton(trainingCol.transform, panel, UnitType.Cavalier,
                "Cavalier (1 Horse, 15s)", font);
            CreateTrainButton(trainingCol.transform, panel, UnitType.Cannon,
                "Cannon (3 Iron, 20s)", font);
            CreateTrainButton(trainingCol.transform, panel, UnitType.StandardBearer,
                "Std Bearer (1 Cloth, 10s)", font);

            var queueHeader = UIFactory.CreateLabel(trainingCol.transform, "QueueHeader",
                "Training Queue:", 13, FontStyles.Bold, font);
            queueHeader.color = UIColors.ACCENT_ORANGE;
            queueHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 20f);

            var queueText = UIFactory.CreateLabel(trainingCol.transform, "QueueText",
                "No units training.", 12, FontStyles.Normal, font);
            queueText.color = UIColors.TEXT_GRAY_DIM;

            UIFactory.SetField(panel, "_panelRoot", panelGo);
            UIFactory.SetField(panel, "_statusText", statusText);
            UIFactory.SetField(panel, "_generalsContainer", generalsCol.transform);
            UIFactory.SetField(panel, "_trainingContainer", trainingCol.transform);
            UIFactory.SetField(panel, "_trainingQueueText", queueText);

            panelGo.SetActive(false);
            return panel;
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

        private static void CreateTrainButton(Transform parent, ArmyPanel panel,
            UnitType unitType, string label, TMP_FontAsset font)
        {
            var btn = UIFactory.CreateButton(parent, label, font,
                UIColors.BUTTON_BLUE,
                () => panel.TrainUnit(unitType),
                new Vector2(0f, 34f), 12f);
            btn.GetComponent<LayoutElement>().preferredHeight = 34f;
        }
    }
}
