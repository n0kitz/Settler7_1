using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Top-bar HUD showing resources, population, and construction queue.
    /// Reads from GameState via GameController — never modifies simulation.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _resourcesText;
        [SerializeField] private TextMeshProUGUI _populationText;
        [SerializeField] private TextMeshProUGUI _constructionText;
        [SerializeField] private TextMeshProUGUI _prestigeText;
        [SerializeField] private TextMeshProUGUI _vpText;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _countdownText;

        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.25f;

        private bool _countdownActive;
        private float _countdownEndTime;

        private void Start()
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.Events == null) return;

            gc.Events.Subscribe<CountdownStartedEvent>(e =>
            {
                _countdownActive = true;
                _countdownEndTime = Time.time + e.Duration;
                if (_countdownText != null) _countdownText.gameObject.SetActive(true);
            });
            gc.Events.Subscribe<CountdownCancelledEvent>(e =>
            {
                _countdownActive = false;
                if (_countdownText != null) _countdownText.gameObject.SetActive(false);
            });
            gc.Events.Subscribe<GameOverEvent>(e =>
            {
                _countdownActive = false;
                if (_countdownText != null) _countdownText.gameObject.SetActive(false);
            });
        }

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

            var state = gc.State;
            var res = state.PlayerResources.TryGetValue(0, out var r) ? r : null;
            if (res == null) return;

            // Resources — §14.8 order first (coins/weapons/tools/food),
            // production detail on the second line. Names re-resolved each
            // refresh so a language switch takes effect live.
            if (_resourcesText != null)
            {
                int food = res.Get(ResourceType.Bread) + res.Get(ResourceType.Fish)
                    + res.Get(ResourceType.Sausages);
                string foodPart = food == 0
                    ? $"<color=#FF6666>{L.Get("ui.hud.food")}:{food}</color>"
                    : $"{L.Get("ui.hud.food")}:{food}";

                _resourcesText.richText = true;
                _resourcesText.text =
                    $"{FR(ResourceType.Coins, res, 3)}   " +
                    $"{FR(ResourceType.Weapons, res, 2)}   " +
                    $"{FR(ResourceType.Tools, res, 1)}   " +
                    $"{foodPart}\n" +
                    $"{FR(ResourceType.Wood, res, 0)}  " +
                    $"{FR(ResourceType.Planks, res, 2)}  " +
                    $"{FR(ResourceType.Stone, res, 1)}  " +
                    $"{FR(ResourceType.IronBars, res, 0)}  " +
                    $"{FR(ResourceType.Coal, res, 0)}";
            }

            // Population
            if (_populationText != null)
            {
                int livingSpace = state.Population.GetLivingSpace(0);
                int employed = state.Population.GetEmployedCount(0);
                int available = livingSpace - employed;
                _populationText.text =
                    $"{L.Get("ui.hud.pop")}: {employed}/{livingSpace}  " +
                    $"{L.Get("ui.hud.idle")}: {available}";
            }

            // Construction queue
            if (_constructionText != null)
            {
                int queued = state.Construction.GetQueuedCount(0);
                if (queued > 0)
                {
                    _constructionText.text =
                        string.Format(L.Get("ui.hud.building_progress"), queued);
                    _constructionText.gameObject.SetActive(true);
                }
                else
                {
                    _constructionText.gameObject.SetActive(false);
                }
            }

            // Prestige
            if (_prestigeText != null)
            {
                var prestige = state.Prestige;
                int pts = prestige.GetPoints(0);
                int lvl = prestige.GetLevel(0);
                int unspent = prestige.GetUnspentLevels(0);
                _prestigeText.text =
                    string.Format(L.Get("ui.hud.prestige_line"), pts, lvl, unspent);
            }

            // Victory Points
            if (_vpText != null)
            {
                int vps = state.Victory.GetVPCount(0);
                int required = state.Victory.VPRequired;
                _vpText.text = $"VP: {vps}/{required}";
            }

            // Game speed
            if (_speedText != null)
            {
                string speed = Time.timeScale switch
                {
                    0f => L.Get("ui.hud.paused"),
                    1f => "1x",
                    2f => "2x",
                    4f => "4x",
                    _ => $"{Time.timeScale:F1}x"
                };
                _speedText.text = speed;
            }

            // Countdown timer
            if (_countdownText != null && _countdownActive)
            {
                float remaining = _countdownEndTime - Time.time;
                if (remaining < 0f) remaining = 0f;
                int mins = (int)remaining / 60;
                int secs = (int)remaining % 60;
                _countdownText.text =
                    $"{L.Get("ui.hud.victory_in")}: {mins}:{secs:D2}";
                _countdownText.color = remaining < 30f
                    ? UIColors.TEXT_RED_BRIGHT
                    : UIColors.HIGHLIGHT_GOLD;
            }
        }

        /// <summary>Format a resource with localized name and shortage coloring.</summary>
        private static string FR(ResourceType type, PlayerResources res, int threshold)
        {
            string name = LocalizedNames.Resource(type);
            int amount = res.Get(type);
            if (amount == 0)
                return $"<color=#FF6666>{name}:{amount}</color>";
            if (amount <= threshold)
                return $"<color=#FFCC44>{name}:{amount}</color>";
            return $"{name}:{amount}";
        }

        /// <summary>
        /// Create the HUD UI programmatically (for bootstrap scene).
        /// </summary>
        public static HUD Create(Transform canvasTransform, TMP_FontAsset font)
        {
            // HUD root — compact centered top bar in the S7 style (§14.8)
            var (frameGo, contentGo) = UIFactory.CreateOrnatePanel(canvasTransform, "HUD");
            var hudGo = contentGo;

            var hudRect = frameGo.GetComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0.5f, 1f);
            hudRect.anchorMax = new Vector2(0.5f, 1f);
            hudRect.pivot = new Vector2(0.5f, 1f);
            hudRect.anchoredPosition = new Vector2(0f, -4f);
            hudRect.sizeDelta = new Vector2(680f, 150f);

            var layout = hudGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 4, 4);
            layout.spacing = 2f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            // Resource line
            var resourcesText = UIFactory.CreateLabel(hudGo.transform, "ResourcesText", "", 13, font);
            resourcesText.color = UIColors.TEXT_GOLD;
            resourcesText.richText = true;
            resourcesText.alignment = TextAlignmentOptions.Center;

            // Population line
            var popText = UIFactory.CreateLabel(hudGo.transform, "PopulationText", "", 13, font);
            popText.color = UIColors.TEXT_GREEN_LIGHT;
            popText.alignment = TextAlignmentOptions.Center;

            // Construction line
            var constText = UIFactory.CreateLabel(hudGo.transform, "ConstructionText", "", 12, font);
            constText.color = new Color(0.7f, 0.7f, 0.9f);
            constText.gameObject.SetActive(false);

            // Prestige line
            var prestigeText = UIFactory.CreateLabel(hudGo.transform, "PrestigeText", "", 12, font);
            prestigeText.color = new Color(0.9f, 0.7f, 0.9f);

            // VP line
            var vpText = UIFactory.CreateLabel(hudGo.transform, "VPText", "", 13, font);
            vpText.color = UIColors.HIGHLIGHT_GOLD;

            // Countdown timer (hidden by default)
            var countdownText = UIFactory.CreateLabel(hudGo.transform, "CountdownText", "", 14, font);
            countdownText.color = UIColors.HIGHLIGHT_GOLD;
            countdownText.fontStyle = FontStyles.Bold;
            countdownText.gameObject.SetActive(false);

            // Speed indicator
            var speedText = UIFactory.CreateLabel(hudGo.transform, "SpeedText", "1x", 11, font);
            speedText.color = new Color(0.7f, 0.7f, 0.7f);

            // Keyboard shortcuts hint
            var shortcutsText = UIFactory.CreateLabel(hudGo.transform, "ShortcutsText",
                "[B]uild [P]restige [T]ech [R]ade [M]ilitary [V]Tavern [F]ood [U]pgrade [1-3]Speed [Space]Pause", 9, font);
            shortcutsText.color = new Color(0.5f, 0.5f, 0.5f);

            // HUD component
            var hud = hudGo.AddComponent<HUD>();
            UIFactory.SetField(hud, "_resourcesText", resourcesText);
            UIFactory.SetField(hud, "_populationText", popText);
            UIFactory.SetField(hud, "_constructionText", constText);
            UIFactory.SetField(hud, "_prestigeText", prestigeText);
            UIFactory.SetField(hud, "_vpText", vpText);
            UIFactory.SetField(hud, "_speedText", speedText);
            UIFactory.SetField(hud, "_countdownText", countdownText);

            return hud;
        }
    }
}
