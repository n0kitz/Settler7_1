using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Shows the top 20 completed matches sorted by score.
    /// Accessible from the main menu via Hall of Fame button.
    /// </summary>
    public class HallOfFameUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform  _listContainer;

        public void Show()
        {
            if (_panelRoot == null) return;
            _panelRoot.SetActive(true);
            Rebuild();
        }

        public void Hide()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void Rebuild()
        {
            if (_listContainer == null) return;
            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);

            var history = MatchHistoryPersistence.Load();
            history.Sort((a, b) => b.Score.CompareTo(a.Score));

            int rank = 1;
            foreach (var r in history)
            {
                if (rank > 20) break;
                CreateRow(r, rank++);
            }

            if (history.Count == 0)
            {
                var empty = UIFactory.CreateLabel(_listContainer, "Empty",
                    "No matches recorded yet.", 16f, UIFactory.GetDefaultFont());
                empty.color = UIColors.TEXT_GRAY_DIM;
            }
        }

        private void CreateRow(MatchResult r, int rank)
        {
            var font = UIFactory.GetDefaultFont();

            var row = new GameObject($"Row_{rank}");
            row.transform.SetParent(_listContainer, false);
            var rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 36f);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(4, 4, 4, 4);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 36f;

            // Rank
            var rankLabel = UIFactory.CreateLabel(row.transform, "Rank",
                $"#{rank}", 14f, FontStyles.Bold, font);
            rankLabel.color = rank <= 3
                ? new Color(0.9f, 0.75f, 0.2f) : UIColors.TEXT_LIGHT;
            var rankLe = rankLabel.gameObject.AddComponent<LayoutElement>();
            rankLe.preferredWidth = 36f;

            // Map + outcome
            string outcome = r.WinnerId == 0 ? "Win" : "Loss";
            var infoLabel = UIFactory.CreateLabel(row.transform, "Info",
                $"{r.MapId}  {outcome}", 14f, font);
            infoLabel.color = r.WinnerId == 0
                ? new Color(0.4f, 0.9f, 0.4f) : new Color(0.9f, 0.4f, 0.4f);
            var infoLe = infoLabel.gameObject.AddComponent<LayoutElement>();
            infoLe.flexibleWidth = 1f;

            // Duration
            int mins = (int)r.DurationSeconds / 60;
            var durLabel = UIFactory.CreateLabel(row.transform, "Dur",
                $"{mins}m", 13f, font);
            durLabel.color = UIColors.TEXT_GRAY_DIM;
            durLabel.alignment = TextAlignmentOptions.MidlineRight;
            var durLe = durLabel.gameObject.AddComponent<LayoutElement>();
            durLe.preferredWidth = 40f;

            // Score
            var scoreLabel = UIFactory.CreateLabel(row.transform, "Score",
                r.Score.ToString("N0"), 14f, FontStyles.Bold, font);
            scoreLabel.color = UIColors.TEXT_HEADER_GOLD;
            scoreLabel.alignment = TextAlignmentOptions.MidlineRight;
            var scoreLe = scoreLabel.gameObject.AddComponent<LayoutElement>();
            scoreLe.preferredWidth = 70f;
        }

        public static HallOfFameUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("HallOfFameUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.95f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panelGo.AddComponent<Image>().color = UIColors.PANEL_BLUE_DARK;

            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Hall of Fame", 26f, FontStyles.Bold, font);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot     = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -14f);
            titleRect.sizeDelta = new Vector2(0f, 38f);
            title.alignment = TextAlignmentOptions.Center;
            title.color = UIColors.TEXT_HEADER_GOLD;

            var content = new GameObject("Content");
            content.transform.SetParent(panelGo.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.02f, 0.1f);
            contentRect.anchorMax = new Vector2(0.98f, 0.88f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            UIFactory.CreateButton(panelGo.transform, "Close", font,
                UIColors.BUTTON_RED, () => panelGo.SetActive(false),
                new Vector2(100f, 32f), 15f);
            var closeBtn = panelGo.GetComponentInChildren<Button>().GetComponent<RectTransform>();
            closeBtn.anchorMin = new Vector2(0.5f, 0f);
            closeBtn.anchorMax = new Vector2(0.5f, 0f);
            closeBtn.pivot     = new Vector2(0.5f, 0f);
            closeBtn.anchoredPosition = new Vector2(0f, 8f);

            var ui = panelGo.AddComponent<HallOfFameUI>();
            UIFactory.SetField(ui, "_panelRoot",    panelGo);
            UIFactory.SetField(ui, "_listContainer", (Transform)contentRect.transform);

            panelGo.SetActive(false);
            return ui;
        }
    }
}
