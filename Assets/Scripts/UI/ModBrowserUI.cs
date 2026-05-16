using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Lists installed mods, shows their metadata, and lets the player enable/disable each.
    /// Opened from MainMenuUI "Mods" button. Starts hidden.
    /// </summary>
    public class ModBrowserUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform  _listContainer;
        [SerializeField] private TextMeshProUGUI _statusText;

        public void Show()
        {
            if (_panelRoot) _panelRoot.SetActive(true);
            Refresh();
        }

        public void Hide() { if (_panelRoot) _panelRoot.SetActive(false); }

        private void Refresh()
        {
            ModLoader.Reload();
            ClearList();

            var mods = ModLoader.Loaded;
            if (_statusText != null)
                _statusText.text = mods.Count == 0
                    ? $"No mods found in:\n{ModLoader.GetModsRoot()}"
                    : $"{mods.Count} mod(s) loaded";

            foreach (var mod in mods)
                AddModRow(mod);
        }

        private void ClearList()
        {
            if (_listContainer == null) return;
            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);
        }

        private void AddModRow(ModManifest mod)
        {
            var font = UIFactory.GetDefaultFont();
            var row = new GameObject($"ModRow_{mod.ModId}");
            row.transform.SetParent(_listContainer, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 60f);
            var bg = row.AddComponent<Image>();
            bg.color = UIColors.PANEL_GRAY_MEDIUM;
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding  = new RectOffset(8, 8, 6, 6);
            layout.spacing  = 8f;
            layout.childForceExpandWidth  = false;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleLeft;
            row.AddComponent<LayoutElement>().preferredHeight = 60f;

            // Info column
            var info = new GameObject("Info");
            info.transform.SetParent(row.transform, false);
            var infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            var infoLe = info.AddComponent<LayoutElement>();
            infoLe.flexibleWidth = 1f;

            var nameLabel = UIFactory.CreateLabel(info.transform, "Name",
                mod.Name, 14f, FontStyles.Bold, font);
            nameLabel.color = UIColors.TEXT_HEADER_GOLD;
            var metaLabel = UIFactory.CreateLabel(info.transform, "Meta",
                $"v{mod.Version} — {mod.Author}", 11f, font);
            metaLabel.color = UIColors.TEXT_GRAY_DIM;
            var descLabel = UIFactory.CreateLabel(info.transform, "Desc",
                mod.Description, 11f, font);
            descLabel.color = UIColors.TEXT_LIGHT;

            // Enable/Disable button
            var capturedId = mod.ModId;
            var captured   = mod;
            UIFactory.CreateButton(row.transform, mod.Enabled ? "Disable" : "Enable",
                font,
                mod.Enabled ? UIColors.BUTTON_RED : UIColors.BUTTON_GREEN,
                () => { ModLoader.SetEnabled(capturedId, !captured.Enabled); Refresh(); },
                new Vector2(80f, 36f), 13f);
        }

        private void OnCloseClicked() { Hide(); }

        public static ModBrowserUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("ModBrowserUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.1f);
            panelRect.anchorMax = new Vector2(0.8f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var bg = panelGo.AddComponent<Image>();
            bg.color = UIColors.PANEL_BLUE_DARK;

            var outerLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            outerLayout.padding = new RectOffset(14, 14, 14, 14);
            outerLayout.spacing = 10f;
            outerLayout.childForceExpandWidth = true;
            outerLayout.childForceExpandHeight = false;

            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Mods", 22f, FontStyles.Bold, font);
            title.color = UIColors.TEXT_HEADER_GOLD;
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;

            var status = UIFactory.CreateLabel(panelGo.transform, "Status", "", 12f, font);
            status.color = UIColors.TEXT_GRAY_DIM;
            status.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;

            var list = new GameObject("List");
            list.transform.SetParent(panelGo.transform, false);
            list.AddComponent<RectTransform>();
            var listLayout = list.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 6f;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;
            var listLe = list.AddComponent<LayoutElement>();
            listLe.flexibleHeight = 1f;

            var ui = panelGo.AddComponent<ModBrowserUI>();
            UIFactory.SetField(ui, "_panelRoot",     panelGo);
            UIFactory.SetField(ui, "_listContainer", list.transform);
            UIFactory.SetField(ui, "_statusText",    status);

            UIFactory.CreateButton(panelGo.transform, "Close", font,
                UIColors.BUTTON_RED, ui.OnCloseClicked, new Vector2(120f, 40f), 16f);

            panelGo.SetActive(false);
            return ui;
        }
    }
}
