using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Main overlay for the in-game map editor.
    /// Hosts the toolbar (top), sector property panel (right), and status bar (bottom).
    /// Binds to a MapEditorController in the Presentation layer.
    /// </summary>
    public class MapEditorUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _mapNameField;

        private TMP_FontAsset _font;

        public event Action OnPlaytest;
        public event Action OnSave;
        public event Action OnLoad;
        public event Action OnClose;
        public event Action<string> OnMapNameChanged;
        public event Action OnAddSector;
        public event Action OnDrawRoad;
        public event Action OnDeleteTool;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public void Show() => _panelRoot?.SetActive(true);
        public void Hide() => _panelRoot?.SetActive(false);

        /// <summary>Set the status bar message (e.g. validation errors or instructions).</summary>
        public void SetStatus(string message)
        {
            if (_statusText != null) _statusText.text = message;
        }

        public void SetMapName(string name)
        {
            if (_mapNameField != null) _mapNameField.text = name;
        }

        private void OnPlaytestClicked() => OnPlaytest?.Invoke();
        private void OnSaveClicked() => OnSave?.Invoke();
        private void OnLoadClicked() => OnLoad?.Invoke();
        private void OnCloseClicked() => OnClose?.Invoke();
        private void OnAddSectorClicked() => OnAddSector?.Invoke();
        private void OnDrawRoadClicked() => OnDrawRoad?.Invoke();
        private void OnDeleteClicked() => OnDeleteTool?.Invoke();

        public static MapEditorUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var root = new GameObject("MapEditorUI");
            root.transform.SetParent(canvasTransform, false);

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Semi-transparent full-screen backdrop
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.15f);

            var ui = root.AddComponent<MapEditorUI>();
            UIFactory.SetField(ui, "_panelRoot", root);
            UIFactory.SetField(ui, "_font", font);

            // Top toolbar
            var toolbar = CreateToolbar(root.transform, font, ui);

            // Status bar at bottom
            var statusBar = CreateStatusBar(root.transform, font, ui);

            root.SetActive(false);
            return ui;
        }

        private static GameObject CreateToolbar(Transform parent, TMP_FontAsset font, MapEditorUI ui)
        {
            var bar = new GameObject("Toolbar");
            bar.transform.SetParent(parent, false);

            var rect = bar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 48f);

            var bg = bar.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.1f, 0.12f, 0.95f);

            var layout = bar.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 6, 6);
            layout.spacing = 8f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleLeft;

            UIFactory.CreateButton(bar.transform, "+ Sector", font,
                UIColors.BUTTON_GREEN, ui.OnAddSectorClicked, new Vector2(90f, 36f), 14f);
            UIFactory.CreateButton(bar.transform, "~ Road", font,
                new Color(0.3f, 0.5f, 0.7f), ui.OnDrawRoadClicked, new Vector2(80f, 36f), 14f);
            UIFactory.CreateButton(bar.transform, "✕ Delete", font,
                UIColors.BUTTON_RED, ui.OnDeleteClicked, new Vector2(80f, 36f), 14f);

            // Separator
            var sep = new GameObject("Sep");
            sep.transform.SetParent(bar.transform, false);
            sep.AddComponent<LayoutElement>().flexibleWidth = 1f;

            UIFactory.CreateButton(bar.transform, "▶ Playtest", font,
                UIColors.BUTTON_GREEN, ui.OnPlaytestClicked, new Vector2(100f, 36f), 14f);
            UIFactory.CreateButton(bar.transform, "💾 Save", font,
                UIColors.BUTTON_BLUE, ui.OnSaveClicked, new Vector2(80f, 36f), 14f);
            UIFactory.CreateButton(bar.transform, "📂 Load", font,
                UIColors.BUTTON_BLUE, ui.OnLoadClicked, new Vector2(80f, 36f), 14f);
            UIFactory.CreateButton(bar.transform, "✕ Close", font,
                new Color(0.4f, 0.3f, 0.25f), ui.OnCloseClicked, new Vector2(80f, 36f), 14f);

            return bar;
        }

        private static GameObject CreateStatusBar(Transform parent, TMP_FontAsset font, MapEditorUI ui)
        {
            var bar = new GameObject("StatusBar");
            bar.transform.SetParent(parent, false);

            var rect = bar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 28f);

            bar.AddComponent<Image>().color = new Color(0.06f, 0.07f, 0.09f, 0.9f);

            var statusText = UIFactory.CreateLabel(bar.transform, "Status",
                "Click '+ Sector' to place a sector. Click '~ Road' then two sectors to connect them.",
                12f, FontStyles.Normal, font);
            var statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = Vector2.zero;
            statusRect.anchorMax = Vector2.one;
            statusRect.offsetMin = new Vector2(10f, 0f);
            statusRect.offsetMax = new Vector2(-10f, 0f);
            statusText.alignment = TextAlignmentOptions.MidlineLeft;
            statusText.color = new Color(0.7f, 0.7f, 0.65f);

            UIFactory.SetField(ui, "_statusText", statusText);

            return bar;
        }
    }
}
