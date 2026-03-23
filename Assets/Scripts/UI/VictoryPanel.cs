using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;
using System.Collections.Generic;
using System.Linq;

namespace Settlers.UI
{
    /// <summary>
    /// VP tracker shown at all times + game over overlay with standings.
    /// Displays VP counts per player, countdown timer, and win/lose result.
    /// </summary>
    public class VictoryPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _vpText;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private GameObject _gameOverOverlay;
        [SerializeField] private TextMeshProUGUI _gameOverText;

        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.5f;
        private bool _gameOverShown;
        private TMP_FontAsset _font;

        /// <summary>Fired when the player clicks "Return to Menu" on the game over screen.</summary>
        public event System.Action OnReturnToMenu;

        private static readonly Color[] PLAYER_COLORS =
        {
            new Color(0.33f, 1f, 0.33f),  // green (player 0)
            new Color(1f, 0.33f, 0.33f),   // red
            new Color(0.33f, 0.55f, 1f),   // blue
            new Color(1f, 1f, 0.33f)        // yellow
        };

        private void Update()
        {
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = REFRESH_INTERVAL;
                Refresh();
            }
        }

        private void Refresh()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            var victory = gc.State.Victory;

            // VP tracker
            if (_vpText != null)
            {
                string text = $"VPs (need {victory.VPRequired}): ";
                for (int p = 0; p < gc.State.PlayerCount; p++)
                {
                    int vps = victory.GetVPCount(p);
                    text += p == 0
                        ? $"<color=#55FF55>You:{vps}</color>  "
                        : $"<color=#FF5555>P{p}:{vps}</color>  ";
                }
                _vpText.text = text;
            }

            // Countdown
            if (_countdownText != null)
            {
                if (victory.IsCountdownActive)
                {
                    int secs = (int)victory.CountdownRemaining;
                    string who = victory.CountdownPlayerId == 0 ? "YOU" : $"Player {victory.CountdownPlayerId}";
                    _countdownText.text = $"{who} wins in {secs}s!";
                    _countdownText.gameObject.SetActive(true);
                }
                else
                {
                    _countdownText.gameObject.SetActive(false);
                }
            }

            // Game over
            if (victory.IsGameOver && !_gameOverShown)
            {
                _gameOverShown = true;
                ShowGameOver(gc.State, victory.WinnerId);
            }
        }

        private void ShowGameOver(GameState state, int winnerId)
        {
            if (_gameOverOverlay != null) _gameOverOverlay.SetActive(true);

            // Header text
            if (_gameOverText != null)
            {
                _gameOverText.text = winnerId == 0
                    ? "VICTORY!"
                    : $"DEFEAT";
            }

            // Build standings below the header
            var overlayTransform = _gameOverOverlay.transform;

            // Match duration
            int totalSecs = (int)state.SimulationTime;
            int mins = totalSecs / 60;
            int secs = totalSecs % 60;
            var durationLabel = UIFactory.CreateLabel(overlayTransform, "Duration",
                $"Match Duration: {mins}:{secs:D2}", 16, _font);
            durationLabel.alignment = TextAlignmentOptions.Center;
            durationLabel.color = new Color(0.8f, 0.8f, 0.8f);
            var durRect = durationLabel.GetComponent<RectTransform>();
            durRect.anchorMin = new Vector2(0.3f, 0.62f);
            durRect.anchorMax = new Vector2(0.7f, 0.67f);
            durRect.offsetMin = Vector2.zero;
            durRect.offsetMax = Vector2.zero;

            // Gather standings
            var standings = BuildStandings(state);

            // Column header
            var headerLabel = UIFactory.CreateLabel(overlayTransform, "StandingsHeader",
                "Rank   Player          VP  Sectors  Army  Techs  Trade  Bldgs  Prstg", 13, FontStyles.Bold, _font);
            headerLabel.alignment = TextAlignmentOptions.Center;
            headerLabel.color = new Color(0.9f, 0.85f, 0.6f);
            headerLabel.enableWordWrapping = false;
            var hdrRect = headerLabel.GetComponent<RectTransform>();
            hdrRect.anchorMin = new Vector2(0.15f, 0.56f);
            hdrRect.anchorMax = new Vector2(0.85f, 0.60f);
            hdrRect.offsetMin = Vector2.zero;
            hdrRect.offsetMax = Vector2.zero;

            // Player rows
            float rowTop = 0.54f;
            const float ROW_HEIGHT = 0.045f;
            for (int i = 0; i < standings.Count; i++)
            {
                var s = standings[i];
                string rankStr = GetRankString(i + 1);
                string playerName = s.PlayerId == 0 ? "You" : $"Player {s.PlayerId}";
                string colorHex = ColorUtility.ToHtmlStringRGB(
                    s.PlayerId < PLAYER_COLORS.Length ? PLAYER_COLORS[s.PlayerId] : Color.white);

                string row = $"{rankStr}    <color=#{colorHex}>{playerName,-14}</color>  " +
                             $"{s.VP,3}    {s.Sectors,3}     {s.Army,3}    {s.Techs,3}    {s.Outposts,3}    {s.Buildings,3}    {s.Prestige,3}";

                float y = rowTop - i * ROW_HEIGHT;
                var rowLabel = UIFactory.CreateLabel(overlayTransform, $"Row_{i}",
                    row, 14, _font);
                rowLabel.alignment = TextAlignmentOptions.Center;
                rowLabel.richText = true;
                rowLabel.enableWordWrapping = false;
                rowLabel.color = s.PlayerId == winnerId
                    ? new Color(1f, 0.85f, 0.3f)
                    : Color.white;

                var rowRect = rowLabel.GetComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0.15f, y - ROW_HEIGHT);
                rowRect.anchorMax = new Vector2(0.85f, y);
                rowRect.offsetMin = Vector2.zero;
                rowRect.offsetMax = Vector2.zero;
            }

            // Player stats summary
            float statsY = rowTop - standings.Count * ROW_HEIGHT - 0.02f;
            var playerStats = standings.Find(s => s.PlayerId == 0);
            string statsStr = $"Your game: {playerStats.Buildings} buildings, {playerStats.Sectors} sectors, " +
                              $"{playerStats.Army} soldiers, {playerStats.Techs} techs, " +
                              $"{playerStats.Outposts} outposts, Prestige Lv{playerStats.Prestige}";
            var statsLabel = UIFactory.CreateLabel(overlayTransform, "PlayerStats", statsStr, 12, _font);
            statsLabel.alignment = TextAlignmentOptions.Center;
            statsLabel.color = new Color(0.7f, 0.85f, 0.7f);
            var statsRect = statsLabel.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.1f, statsY - 0.04f);
            statsRect.anchorMax = new Vector2(0.9f, statsY);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;

            // Return to Menu button
            float btnY = statsY - 0.06f;
            CreateReturnButton(overlayTransform, btnY);
        }

        private void CreateReturnButton(Transform parent, float yCenter)
        {
            var btnGo = new GameObject("Btn_ReturnToMenu");
            btnGo.transform.SetParent(parent, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.35f, yCenter - 0.025f);
            rect.anchorMax = new Vector2(0.65f, yCenter + 0.025f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bgColor = new Color(0.3f, 0.45f, 0.3f, 0.9f);
            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = bgColor;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;
            btn.onClick.AddListener(() => OnReturnToMenu?.Invoke());

            var text = UIFactory.CreateLabel(btnGo.transform, "Label",
                "Return to Menu", 18, FontStyles.Bold, _font);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;
        }

        private List<PlayerStanding> BuildStandings(GameState state)
        {
            var list = new List<PlayerStanding>();
            for (int p = 0; p < state.PlayerCount; p++)
            {
                int sectors = state.Graph.GetSectorsOwnedBy(p).Count;
                int army = state.Army.GetTotalArmySize(p);
                int techs = state.Research.GetPlayerTechs(p).Count;
                int outposts = 0;
                foreach (var op in state.TradeMapData.AllOutposts)
                    if (op.ClaimedBy == p) outposts++;

                int buildings = state.Construction.GetBuildingsByPlayer(p).Count;
                int prestige = state.Prestige.GetLevel(p);

                list.Add(new PlayerStanding
                {
                    PlayerId = p,
                    VP = state.Victory.GetVPCount(p),
                    Sectors = sectors,
                    Army = army,
                    Techs = techs,
                    Outposts = outposts,
                    Buildings = buildings,
                    Prestige = prestige
                });
            }

            list.Sort((a, b) =>
            {
                int cmp = b.VP.CompareTo(a.VP);
                return cmp != 0 ? cmp : b.Sectors.CompareTo(a.Sectors);
            });

            return list;
        }

        private static string GetRankString(int rank)
        {
            return rank switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                _ => $"{rank}th"
            };
        }

        private struct PlayerStanding
        {
            public int PlayerId;
            public int VP;
            public int Sectors;
            public int Army;
            public int Techs;
            public int Outposts;
            public int Buildings;
            public int Prestige;
        }

        /// <summary>Create the VP panel + game over overlay programmatically.</summary>
        public static VictoryPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            // VP tracker (bottom-right)
            var panelGo = new GameObject("VictoryPanel");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-10f, 10f);
            panelRect.sizeDelta = new Vector2(300f, 50f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.spacing = 2f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var vpText = CreateLabel(panelGo.transform, "VPText", "VPs: ...", 13, font);
            vpText.color = new Color(1f, 0.9f, 0.5f);
            vpText.richText = true;

            var countdownText = CreateLabel(panelGo.transform, "CountdownText", "", 14, font);
            countdownText.color = new Color(1f, 0.3f, 0.3f);
            countdownText.fontStyle = FontStyles.Bold;
            countdownText.gameObject.SetActive(false);

            // Game over overlay (centered, hidden)
            var overlayGo = new GameObject("GameOverOverlay");
            overlayGo.transform.SetParent(canvasTransform, false);

            var overlayRect = overlayGo.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var overlayBg = overlayGo.AddComponent<Image>();
            overlayBg.color = new Color(0f, 0f, 0f, 0.8f);

            var gameOverText = CreateLabel(overlayGo.transform, "GameOverText",
                "GAME OVER", 36, font);
            gameOverText.alignment = TextAlignmentOptions.Center;
            gameOverText.color = new Color(1f, 0.85f, 0.3f);
            var goRect = gameOverText.GetComponent<RectTransform>();
            goRect.anchorMin = new Vector2(0.2f, 0.68f);
            goRect.anchorMax = new Vector2(0.8f, 0.78f);
            goRect.offsetMin = Vector2.zero;
            goRect.offsetMax = Vector2.zero;

            overlayGo.SetActive(false);

            // Component
            var panel = panelGo.AddComponent<VictoryPanel>();
            SetField(panel, "_vpText", vpText);
            SetField(panel, "_countdownText", countdownText);
            SetField(panel, "_gameOverOverlay", overlayGo);
            SetField(panel, "_gameOverText", gameOverText);
            panel._font = font;

            return panel;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name,
            string text, float fontSize, TMP_FontAsset font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, fontSize + 6f);

            var layoutElem = go.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = fontSize + 6f;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            if (font != null) tmp.font = font;

            return tmp;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
