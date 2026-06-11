using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Bottom action bar (§14.8): one button per game panel, mirroring the
    /// keyboard toggles (B/P/T/R/M/V/Q). Labels re-resolve periodically so a
    /// language switch takes effect live.
    /// </summary>
    public class ActionBarUI : MonoBehaviour
    {
        private struct Entry
        {
            public TextMeshProUGUI Label;
            public string LabelKey;
            public string Hotkey;
        }

        private readonly List<Entry> _entries = new();
        private float _refreshTimer;

        private void Update()
        {
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer > 0f) return;
            _refreshTimer = 1f;

            foreach (var e in _entries)
                e.Label.text = $"{L.Get(e.LabelKey)} [{e.Hotkey}]";

            var prestige = Presentation.GameController.Instance?.State?.Prestige;
            if (_prestigeLevelText != null && prestige != null)
                _prestigeLevelText.text = prestige.GetLevel(0).ToString();
        }

        // --- Panel toggles (same lookup pattern as GameController.Input) ---

        private static void Toggle<T>() where T : Component
        {
            // Include inactive — most panels deactivate their own GameObject
            var panel = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            panel?.SendMessage("Toggle", SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>Create the bottom action bar (S7 style: ornate centered
        /// bar with the prestige star level in the middle).</summary>
        public static ActionBarUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var (frameGo, contentGo) = UIFactory.CreateOrnatePanel(canvasTransform, "ActionBar");

            var rect = frameGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 4f);
            rect.sizeDelta = new Vector2(820f, 40f);

            var layout = contentGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.spacing = 5f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var bar = frameGo.AddComponent<ActionBarUI>();

            AddButton(bar, contentGo.transform, "ui.build.title", "B", font,
                () => Toggle<BuildMenu>());
            AddButton(bar, contentGo.transform, "ui.settings.controls.toggle_prestige", "P", font,
                () => Toggle<PrestigeChartUI>());
            AddButton(bar, contentGo.transform, "ui.settings.controls.toggle_tech", "T", font,
                () => Toggle<TechTreeUI>());

            // Center piece: crown icon + prestige level (like the S7 star)
            CreatePrestigeCenter(bar, contentGo.transform, font);

            AddButton(bar, contentGo.transform, "ui.settings.controls.toggle_trade", "R", font,
                () => Toggle<TradeMapUI>());
            AddButton(bar, contentGo.transform, "ui.settings.controls.toggle_army", "M", font,
                () => Toggle<ArmyPanel>());
            AddButton(bar, contentGo.transform, "ui.settings.controls.toggle_tavern", "V", font,
                () => Toggle<TavernUI>());
            AddButton(bar, contentGo.transform, "ui.settings.controls.toggle_quest", "Q", font,
                () => Toggle<QuestPanel>());

            return bar;
        }

        private TextMeshProUGUI _prestigeLevelText;

        private static void CreatePrestigeCenter(ActionBarUI bar, Transform parent,
            TMP_FontAsset font)
        {
            var go = new GameObject("PrestigeCenter");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var elem = go.AddComponent<LayoutElement>();
            elem.preferredWidth = 56f;
            elem.flexibleWidth = 0f;

            var iconGo = new GameObject("Crown");
            iconGo.transform.SetParent(go.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(-12f, 0f);
            iconRect.sizeDelta = new Vector2(24f, 24f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = IconFactory.CrownTab();
            iconImg.preserveAspect = true;

            var level = UIFactory.CreateLabel(go.transform, "Level", "0", 16,
                FontStyles.Bold, font);
            var levelRect = level.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.5f, 0.5f);
            levelRect.anchorMax = new Vector2(0.5f, 0.5f);
            levelRect.anchoredPosition = new Vector2(12f, 0f);
            levelRect.sizeDelta = new Vector2(28f, 24f);
            level.color = UIColors.HIGHLIGHT_GOLD;
            level.alignment = TextAlignmentOptions.Center;

            bar._prestigeLevelText = level;
        }

        private static void AddButton(ActionBarUI bar, Transform parent,
            string labelKey, string hotkey, TMP_FontAsset font,
            UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIFactory.CreateButton(parent,
                $"{L.Get(labelKey)} [{hotkey}]", font,
                UIColors.BUTTON_BLUE, onClick, new Vector2(0f, 26f), 12f);

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                bar._entries.Add(new Entry
                {
                    Label = label,
                    LabelKey = labelKey,
                    Hotkey = hotkey
                });
        }
    }
}
