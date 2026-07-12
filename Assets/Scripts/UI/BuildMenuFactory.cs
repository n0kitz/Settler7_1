using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Builds the three-tab build menu (§14.4) as an icon-tile grid in the
    /// S7 style: ornate olive/gold modal, icon tabs (house/shield/crown),
    /// close button, building tiles with procedural icons.
    /// </summary>
    public static class BuildMenuFactory
    {
        public static BuildMenu Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var (frameGo, contentGo) = UIFactory.CreateOrnatePanel(canvasTransform, "BuildMenu");
            var panelRect = frameGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(14f, 56f);
            panelRect.sizeDelta = new Vector2(280f, 300f);

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 10);
            layout.spacing = 6f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;

            var menu = frameGo.AddComponent<BuildMenu>();
            UIFactory.SetField(menu, "_panelRoot", frameGo);

            // Title row: BAUEN + close X
            CreateTitleRow(contentGo.transform, menu, font);

            // Icon tab bar (house / shield / crown)
            var tabBarGo = new GameObject("TabBar");
            tabBarGo.transform.SetParent(contentGo.transform, false);
            tabBarGo.AddComponent<RectTransform>();
            var tabLayout = tabBarGo.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 4f;
            tabLayout.childForceExpandWidth = false;
            tabLayout.childForceExpandHeight = false;
            tabLayout.childAlignment = TextAnchor.MiddleLeft;
            var tabBarElem = tabBarGo.AddComponent<LayoutElement>();
            tabBarElem.preferredHeight = 38f;

            var economyTab  = CreateTabContent(contentGo.transform, "EconomyTab");
            var specialsTab = CreateTabContent(contentGo.transform, "SpecialsTab");
            var empireTab   = CreateTabContent(contentGo.transform, "EmpireTab");

            CreateTabButton(tabBarGo.transform, menu, 0, IconFactory.HouseTab(), economyTab);
            CreateTabButton(tabBarGo.transform, menu, 1, IconFactory.ShieldTab(), specialsTab);
            CreateTabButton(tabBarGo.transform, menu, 2, IconFactory.CrownTab(), empireTab);

            // Tab 0 — Economy (house): base buildings
            AddBuildingTile(economyTab.transform, menu, BaseBuildingType.Lodge, font);
            AddBuildingTile(economyTab.transform, menu, BaseBuildingType.Farm, font);
            AddBuildingTile(economyTab.transform, menu, BaseBuildingType.MountainShelter, font);
            AddBuildingTile(economyTab.transform, menu, BaseBuildingType.Residence, font);

            // Tab 1 — Specials (shield): prestige-gated
            AddBuildingTile(specialsTab.transform, menu, BaseBuildingType.NobleResidence, font);

            // Tab 2 — Empire (crown): empire-wide prestige objects
            AddEmpireTile(empireTab.transform, menu, "mil_stronghold",
                "ui.build.empire.stronghold_active", font);
            AddEmpireTile(empireTab.transform, menu, "cul_church",
                "ui.build.empire.church_active", font);
            AddEmpireTile(empireTab.transform, menu, "cul_export_office",
                "ui.build.empire.export_active", font);

            // Cost text + feedback (below the grid)
            var costText = UIFactory.CreateLabel(contentGo.transform, "CostText", "", 12,
                FontStyles.Normal, font);
            costText.color = UIColors.TEXT_GOLD;
            UIFactory.SetField(menu, "_costText", costText);

            var feedbackText = UIFactory.CreateLabel(contentGo.transform, "FeedbackText", "", 13,
                FontStyles.Bold, font);
            feedbackText.color = UIColors.TEXT_RED_BRIGHT;
            feedbackText.gameObject.SetActive(false);
            UIFactory.SetField(menu, "_feedbackText", feedbackText);

            menu.SelectTab(0);
            return menu;
        }

        private static void CreateTitleRow(Transform parent, BuildMenu menu, TMP_FontAsset font)
        {
            var rowGo = new GameObject("TitleRow");
            rowGo.transform.SetParent(parent, false);
            rowGo.AddComponent<RectTransform>();
            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            var rowElem = rowGo.AddComponent<LayoutElement>();
            rowElem.preferredHeight = 24f;

            var title = UIFactory.CreateLabel(rowGo.transform, "Title",
                L.Get("ui.build.menu_title"), 16, FontStyles.Bold, font);
            title.color = UIColors.TEXT_HEADER_GOLD;
            var titleElem = title.gameObject.AddComponent<LayoutElement>();
            titleElem.flexibleWidth = 1f;
            menu._titleLabel = title;

            var closeBtn = UIFactory.CreateButton(rowGo.transform, "X", font,
                UIColors.BUTTON_RED, () => menu.Hide(), new Vector2(22f, 22f), 13f);
            var closeElem = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeElem.preferredWidth = 22f;
            closeElem.preferredHeight = 22f;
        }

        private static GameObject CreateTabContent(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(78f, 66f);
            grid.spacing = new Vector2(6f, 6f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            var elem = go.AddComponent<LayoutElement>();
            elem.preferredHeight = 142f;
            return go;
        }

        private static void CreateTabButton(Transform parent, BuildMenu menu, int index,
            Sprite icon, GameObject content)
        {
            var go = new GameObject($"Tab_{index}");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var elem = go.AddComponent<LayoutElement>();
            elem.preferredWidth = 44f;
            elem.preferredHeight = 36f;

            var bg = go.AddComponent<Image>();
            bg.color = BuildMenu.DEFAULT_BTN_COLOR;
            var btn = go.AddComponent<Button>();

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(26f, 26f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;

            menu.RegisterTab(bg, content);
            btn.onClick.AddListener(() => menu.SelectTab(index));
        }

        /// <summary>Icon tile: building sprite + small name label below.</summary>
        private static void AddBuildingTile(Transform parent, BuildMenu menu,
            BaseBuildingType type, TMP_FontAsset font)
        {
            var (btn, img, label) = CreateTile(parent, $"Tile_{type}",
                IconFactory.Building(type), TileName(type), font);
            menu.RegisterBuilding(btn, img, label, type);
        }

        private static void AddEmpireTile(Transform parent, BuildMenu menu,
            string unlockId, string activeHintKey, TMP_FontAsset font)
        {
            var (btn, img, label) = CreateTile(parent, $"Empire_{unlockId}",
                IconFactory.CrownTab(), LocalizedNames.Prestige(unlockId), font);
            menu.RegisterEmpire(btn, img, label, unlockId, activeHintKey);
        }

        private static (Button btn, Image img, TextMeshProUGUI label) CreateTile(
            Transform parent, string name, Sprite icon, string text, TMP_FontAsset font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var bg = go.AddComponent<Image>();
            bg.color = UIColors.TILE_BG;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.42f, 0.3f);
            colors.pressedColor = new Color(0.3f, 0.35f, 0.22f);
            btn.colors = colors;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -4f);
            iconRect.sizeDelta = new Vector2(38f, 38f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;

            var label = UIFactory.CreateLabel(go.transform, "Label", text, 9,
                FontStyles.Normal, font);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 2f);
            labelRect.sizeDelta = new Vector2(0f, 20f);
            label.alignment = TextAlignmentOptions.Center;

            return (btn, bg, label);
        }

        /// <summary>Short tile name from the localized button string ("Lodge (3 Planks)" → "Lodge").</summary>
        internal static string TileName(BaseBuildingType type)
        {
            string full = type switch
            {
                BaseBuildingType.Lodge => L.Get("ui.build.btn.lodge"),
                BaseBuildingType.Farm => L.Get("ui.build.btn.farm"),
                BaseBuildingType.MountainShelter => L.Get("ui.build.btn.mountain_shelter"),
                BaseBuildingType.Residence => L.Get("ui.build.btn.residence"),
                BaseBuildingType.NobleResidence => L.Get("ui.build.btn.noble_residence"),
                _ => type.ToString()
            };
            int paren = full.IndexOf(" (");
            return paren > 0 ? full.Substring(0, paren) : full;
        }
    }
}
