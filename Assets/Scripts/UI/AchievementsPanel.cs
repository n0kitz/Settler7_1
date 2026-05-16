using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Achievement gallery and current-game statistics panel.
    /// Toggle with K key. Shows all achievements (locked/unlocked) and PlayerStats.
    /// </summary>
    public class AchievementsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform  _listContainer;
        [SerializeField] private TextMeshProUGUI _statsText;

        private AchievementSystem _achievements;
        private PlayerStats       _stats;
        private bool _dirty = true;

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current.kKey.wasPressedThisFrame) Toggle();
        }

        public void Bind(AchievementSystem achievements, PlayerStats stats)
        {
            _achievements = achievements;
            _stats        = stats;
            _dirty        = true;
        }

        public void Toggle()
        {
            if (_panelRoot == null) return;
            bool next = !_panelRoot.activeSelf;
            _panelRoot.SetActive(next);
            if (next && _dirty) Rebuild();
        }

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            if (_dirty) Rebuild();
        }

        public void Hide()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void Rebuild()
        {
            _dirty = false;
            if (_achievements == null || _listContainer == null) return;

            // Clear existing rows
            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);

            foreach (var ach in _achievements.All)
                CreateRow(ach);

            if (_statsText != null && _stats != null)
                _statsText.text = FormatStats();
        }

        private void CreateRow(Achievement ach)
        {
            var font = UIFactory.GetDefaultFont();
            var row = new GameObject("Row");
            row.transform.SetParent(_listContainer, false);
            var rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 40f);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(4, 4, 4, 4);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 40f;

            var icon = UIFactory.CreateLabel(row.transform, "Icon",
                ach.IsUnlocked ? "★" : "☆", 18f, font);
            icon.color = ach.IsUnlocked ? new Color(0.9f, 0.8f, 0.2f) : UIColors.TEXT_GRAY_DIM;
            var iconLe = icon.gameObject.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 24f;

            var info = UIFactory.CreateLabel(row.transform, "Info",
                $"<b>{ach.Name}</b>\n{ach.Description}", 12f, font);
            info.color = ach.IsUnlocked ? UIColors.TEXT_LIGHT : UIColors.TEXT_GRAY_DIM;
            info.textWrappingMode = TMPro.TextWrappingModes.Normal;
            var infoLe = info.gameObject.AddComponent<LayoutElement>();
            infoLe.flexibleWidth = 1f;

            if (ach.IsUnlocked && ach.UnlockedAt.HasValue)
            {
                var date = UIFactory.CreateLabel(row.transform, "Date",
                    ach.UnlockedAt.Value.ToLocalTime().ToString("MMM d"), 10f, font);
                date.color = UIColors.TEXT_GRAY_DIM;
                date.alignment = TMPro.TextAlignmentOptions.MidlineRight;
                var dateLe = date.gameObject.AddComponent<LayoutElement>();
                dateLe.preferredWidth = 54f;
            }
        }

        private string FormatStats()
        {
            return $"This session:\n" +
                   $"Buildings built: {_stats.BuildingsBuilt}\n" +
                   $"Sectors conquered: {_stats.SectorsConquered}\n" +
                   $"Techs researched: {_stats.TechsResearched}\n" +
                   $"VPs gained: {_stats.VPsGained}\n" +
                   $"Trades completed: {_stats.TradesCompleted}\n" +
                   $"Outposts claimed: {_stats.OutpostsClaimed}";
        }

        public static AchievementsPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("AchievementsPanel");
            panelGo.transform.SetParent(canvasTransform, false);

            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.95f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var bg = panelGo.AddComponent<Image>();
            bg.color = UIColors.PANEL_BLUE_DARK;

            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Achievements", 22f, FontStyles.Bold, font);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot     = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -12f);
            titleRect.sizeDelta = new Vector2(0f, 34f);
            title.alignment = TextAlignmentOptions.Center;
            title.color = UIColors.TEXT_HEADER_GOLD;

            // Achievement list (left side)
            var listScroll = new GameObject("ListArea");
            listScroll.transform.SetParent(panelGo.transform, false);
            var scrollRect = listScroll.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.01f, 0.25f);
            scrollRect.anchorMax = new Vector2(0.6f,  0.9f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            var sr = listScroll.AddComponent<ScrollRect>();
            sr.horizontal = false;

            var content = new GameObject("Content");
            content.transform.SetParent(listScroll.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot     = new Vector2(0f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 0f);
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = contentRect;

            // Stats panel (right side)
            var statsArea = new GameObject("StatsArea");
            statsArea.transform.SetParent(panelGo.transform, false);
            var statsRect = statsArea.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.62f, 0.25f);
            statsRect.anchorMax = new Vector2(0.99f, 0.9f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            statsArea.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);

            var statsLabel = UIFactory.CreateLabel(statsArea.transform, "StatsText",
                "", 14f, font);
            var statsLabelRect = statsLabel.GetComponent<RectTransform>();
            statsLabelRect.anchorMin = Vector2.zero;
            statsLabelRect.anchorMax = Vector2.one;
            statsLabelRect.offsetMin = new Vector2(8f, 8f);
            statsLabelRect.offsetMax = new Vector2(-8f, -8f);
            statsLabel.textWrappingMode = TMPro.TextWrappingModes.Normal;
            statsLabel.verticalAlignment = TMPro.VerticalAlignmentOptions.Top;

            // Close button
            UIFactory.CreateButton(panelGo.transform, "Close [K]", font,
                UIColors.BUTTON_RED, () => {
                    panelGo.SetActive(false);
                }, new Vector2(140f, 36f), 16f).GetComponent<RectTransform>()
                .anchoredPosition = Vector2.zero;

            var closeRect = panelGo.GetComponentInChildren<UnityEngine.UI.Button>()
                .GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot     = new Vector2(0.5f, 0f);
            closeRect.anchoredPosition = new Vector2(0f, 10f);

            var panel = panelGo.AddComponent<AchievementsPanel>();
            UIFactory.SetField(panel, "_panelRoot",     panelGo);
            UIFactory.SetField(panel, "_listContainer", (Transform)contentRect.transform);
            UIFactory.SetField(panel, "_statsText",     statsLabel);

            panelGo.SetActive(false);
            return panel;
        }
    }
}
