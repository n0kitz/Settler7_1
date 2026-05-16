using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Full post-game summary panel: winner, duration, player stats, score.
    /// Shown after game over. Offers "Play Again" and "Return to Menu" buttons.
    /// </summary>
    public class PostGameSummaryUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private TextMeshProUGUI _scoreText;

        public event System.Action OnReturnToMenu;
        public event System.Action OnPlayAgain;

        public void Show(MatchResult result)
        {
            if (_panelRoot == null) return;
            _panelRoot.SetActive(true);
            Populate(result);
        }

        public void Hide()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void Populate(MatchResult r)
        {
            bool playerWon = r.WinnerId == 0;
            string outcome = playerWon ? "VICTORY!" : "DEFEAT";
            string minutes = FormatDuration(r.DurationSeconds);

            if (_headerText != null)
            {
                _headerText.text  = outcome;
                _headerText.color = playerWon
                    ? new Color(0.3f, 0.9f, 0.4f)
                    : new Color(0.9f, 0.3f, 0.3f);
            }

            if (_statsText != null)
            {
                _statsText.text =
                    $"Map: {r.MapId}\n" +
                    $"Duration: {minutes}\n" +
                    $"Players: {r.PlayerCount}  •  VP to win: {r.VPRequired}\n\n" +
                    $"Buildings completed: {r.BuildingsBuilt}\n" +
                    $"Sectors conquered:   {r.SectorsConquered}\n" +
                    $"Technologies:        {r.TechsResearched}\n" +
                    $"Trades completed:    {r.TradesCompleted}";
            }

            if (_scoreText != null)
                _scoreText.text = $"Score: {r.Score:N0}";
        }

        private void OnReturnToMenuClicked() { Hide(); OnReturnToMenu?.Invoke(); }
        private void OnPlayAgainClicked()    { Hide(); OnPlayAgain?.Invoke(); }

        private static string FormatDuration(float seconds)
        {
            int m = (int)seconds / 60;
            int s = (int)seconds % 60;
            return $"{m}m {s:D2}s";
        }

        public static PostGameSummaryUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("PostGameSummaryUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.1f);
            rect.anchorMax = new Vector2(0.85f, 0.92f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panelGo.AddComponent<Image>().color = UIColors.PANEL_BLUE_DARK;

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.spacing = 12f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            var header = UIFactory.CreateLabel(panelGo.transform, "Header",
                "VICTORY!", 36f, FontStyles.Bold, font);
            header.alignment = TextAlignmentOptions.Center;
            var hLe = header.gameObject.AddComponent<LayoutElement>();
            hLe.preferredHeight = 50f;

            var stats = UIFactory.CreateLabel(panelGo.transform, "Stats",
                "", 16f, font);
            stats.textWrappingMode = TextWrappingModes.Normal;
            stats.color = UIColors.TEXT_LIGHT;
            var sLe = stats.gameObject.AddComponent<LayoutElement>();
            sLe.preferredHeight = 160f;

            var score = UIFactory.CreateLabel(panelGo.transform, "Score",
                "Score: 0", 22f, FontStyles.Bold, font);
            score.alignment = TextAlignmentOptions.Center;
            score.color = UIColors.TEXT_HEADER_GOLD;
            var scoreLe = score.gameObject.AddComponent<LayoutElement>();
            scoreLe.preferredHeight = 32f;

            // Spacer
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(panelGo.transform, false);
            var spacerLe = spacer.AddComponent<LayoutElement>();
            spacerLe.flexibleHeight = 1f;

            var ui = panelGo.AddComponent<PostGameSummaryUI>();
            UIFactory.SetField(ui, "_panelRoot",  panelGo);
            UIFactory.SetField(ui, "_headerText", header);
            UIFactory.SetField(ui, "_statsText",  stats);
            UIFactory.SetField(ui, "_scoreText",  score);

            // Button row
            var btnRow = new GameObject("BtnRow");
            btnRow.transform.SetParent(panelGo.transform, false);
            var btnLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 16f;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = false;
            var btnLe = btnRow.AddComponent<LayoutElement>();
            btnLe.preferredHeight = 46f;

            UIFactory.CreateButton(btnRow.transform, "Play Again", font,
                UIColors.BUTTON_GREEN, ui.OnPlayAgainClicked, null, 20f);
            UIFactory.CreateButton(btnRow.transform, "Return to Menu", font,
                UIColors.BUTTON_BLUE, ui.OnReturnToMenuClicked, null, 20f);

            panelGo.SetActive(false);
            return ui;
        }
    }
}
