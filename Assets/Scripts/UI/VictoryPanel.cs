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
    public partial class VictoryPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _vpText;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private GameObject _gameOverOverlay;
        [SerializeField] private TextMeshProUGUI _gameOverText;

        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.5f;
        private bool _gameOverShown;
        private TMP_FontAsset _font;
        // Dynamically built game-over widgets — destroyed on restart (Play Again)
        private readonly List<GameObject> _overlayItems = new();

        /// <summary>Fired when the player clicks "Return to Menu" on the game over screen.</summary>
        public event System.Action OnReturnToMenu;

        private static readonly Color[] PLAYER_COLORS =
        {
            new Color(0.2f, 0.5f, 0.9f),   // blue (player 0 — matches SectorView)
            new Color(0.9f, 0.2f, 0.2f),   // red
            new Color(0.2f, 0.8f, 0.3f),   // green
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
                string text = string.Format(L.Get("ui.vp.tracker"), victory.VPRequired) + " ";
                for (int p = 0; p < gc.State.PlayerCount; p++)
                {
                    int vps = victory.GetVPCount(p);
                    string who = p == 0
                        ? L.Get("ui.vp.you") : string.Format(L.Get("ui.vp.player"), p);
                    text += p == 0
                        ? $"<color=#55FF55>{who}:{vps}</color>  "
                        : $"<color=#FF5555>{who}:{vps}</color>  ";
                }
                _vpText.text = text;
            }

            // Countdown
            if (_countdownText != null)
            {
                if (victory.IsCountdownActive)
                {
                    int secs = (int)victory.CountdownRemaining;
                    string who = victory.CountdownPlayerId == 0
                        ? L.Get("ui.vp.you")
                        : string.Format(L.Get("ui.vp.player"), victory.CountdownPlayerId);
                    _countdownText.text = string.Format(L.Get("ui.vp.countdown"), who, secs);
                    _countdownText.gameObject.SetActive(true);
                }
                else
                {
                    _countdownText.gameObject.SetActive(false);
                }
            }

            // Game over — PostGameSummaryUI owns the end screen; this overlay
            // is only the fallback when no summary actually made it on screen
            // (e.g. games started outside the bootstrap wiring). Both showing
            // at once was a double game-over screen. The summary shows
            // synchronously on GameOverEvent, i.e. before this 0.5s poll.
            if (victory.IsGameOver && !_gameOverShown)
            {
                _gameOverShown = true;
                var summary = FindFirstObjectByType<PostGameSummaryUI>(
                    FindObjectsInactive.Include);
                if (summary == null || !summary.IsVisible)
                    ShowGameOver(gc.State, victory.WinnerId);
            }
            else if (!victory.IsGameOver && _gameOverShown)
            {
                // A new game started (Play Again / new match) — reset overlay
                ResetGameOver();
            }
        }

        private void ResetGameOver()
        {
            _gameOverShown = false;
            foreach (var item in _overlayItems)
                if (item != null) Destroy(item);
            _overlayItems.Clear();
            if (_gameOverOverlay != null) _gameOverOverlay.SetActive(false);
        }

        private void ShowGameOver(GameState state, int winnerId)
        {
            if (_gameOverOverlay != null) _gameOverOverlay.SetActive(true);

            // Header text
            if (_gameOverText != null)
            {
                _gameOverText.text = winnerId == 0
                    ? L.Get("ui.endscreen.victory")
                    : L.Get("ui.endscreen.defeat");
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
            _overlayItems.Add(durationLabel.gameObject);

            // Gather standings
            var standings = BuildStandings(state);

            // Column header
            var headerLabel = UIFactory.CreateLabel(overlayTransform, "StandingsHeader",
                "Rank   Player          VP  Sectors  Army  Techs  Trade  Bldgs  Prstg", 13, FontStyles.Bold, _font);
            headerLabel.alignment = TextAlignmentOptions.Center;
            headerLabel.color = new Color(0.9f, 0.85f, 0.6f);
            headerLabel.textWrappingMode = TextWrappingModes.NoWrap;
            var hdrRect = headerLabel.GetComponent<RectTransform>();
            hdrRect.anchorMin = new Vector2(0.15f, 0.56f);
            hdrRect.anchorMax = new Vector2(0.85f, 0.60f);
            hdrRect.offsetMin = Vector2.zero;
            hdrRect.offsetMax = Vector2.zero;
            _overlayItems.Add(headerLabel.gameObject);

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
                rowLabel.textWrappingMode = TextWrappingModes.NoWrap;
                rowLabel.color = s.PlayerId == winnerId
                    ? UIColors.HIGHLIGHT_GOLD
                    : Color.white;

                var rowRect = rowLabel.GetComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0.15f, y - ROW_HEIGHT);
                rowRect.anchorMax = new Vector2(0.85f, y);
                rowRect.offsetMin = Vector2.zero;
                rowRect.offsetMax = Vector2.zero;
                _overlayItems.Add(rowLabel.gameObject);
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
            _overlayItems.Add(statsLabel.gameObject);

            // Return to Menu button
            float btnY = statsY - 0.06f;
            CreateReturnButton(overlayTransform, btnY);
        }

        private void CreateReturnButton(Transform parent, float yCenter)
        {
            var btn = UIFactory.CreateButton(parent, "Return to Menu", _font,
                new Color(0.3f, 0.45f, 0.3f, 0.9f),
                () => OnReturnToMenu?.Invoke());

            var rect = btn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.35f, yCenter - 0.025f);
            rect.anchorMax = new Vector2(0.65f, yCenter + 0.025f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _overlayItems.Add(btn.gameObject);
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
    }
}
