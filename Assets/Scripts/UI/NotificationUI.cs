using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Floating notification system. Shows brief messages that fade out.
    /// Subscribes to simulation events and displays relevant notifications.
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        [SerializeField] private Transform _container;

        private readonly List<NotificationEntry> _entries = new();
        private const float DISPLAY_TIME = 4f;
        private const float FADE_TIME = 1f;
        private const int MAX_VISIBLE = 6;

        // Category colors
        private static readonly Color COLOR_ECONOMY = new(1f, 0.95f, 0.7f);
        private static readonly Color COLOR_MILITARY = new(1f, 0.5f, 0.5f);
        private static readonly Color COLOR_PRESTIGE = new(0.9f, 0.7f, 0.9f);
        private static readonly Color COLOR_VICTORY = new(1f, 0.85f, 0.3f);
        private static readonly Color COLOR_TECH = new(0.5f, 0.8f, 1f);
        private static readonly Color COLOR_TRADE = new(0.5f, 1f, 0.7f);

        private struct NotificationEntry
        {
            public GameObject Go;
            public TextMeshProUGUI Text;
            public CanvasGroup Group;
            public float TimeLeft;
        }

        private void Start()
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.Events == null) return;

            // --- Existing subscriptions (all players) ---
            gc.Events.Subscribe<SectorConqueredEvent>(e =>
            {
                var sector = gc.State.Graph.GetSector(e.SectorId);
                string name = sector?.Name ?? $"sector {e.SectorId}";
                string method = e.Method switch
                {
                    ConquestMethod.Proselytism => " (proselytism)",
                    ConquestMethod.Bribery => " (bribery)",
                    _ => ""
                };
                string who = e.NewOwnerId == 0 ? "You conquered" : $"Player {e.NewOwnerId + 1} conquered";
                Show($"{who} {name}{method}!", COLOR_MILITARY);
            });
            gc.Events.Subscribe<TechResearchedEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show($"Research complete: {e.TechId}", COLOR_TECH);
            });
            gc.Events.Subscribe<VPChangedEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show(e.Gained ? $"VP gained: {e.VPId}" : $"VP lost: {e.VPId}", COLOR_VICTORY);
            });
            gc.Events.Subscribe<GameOverEvent>(e =>
                Show($"Player {e.WinnerId + 1} WINS!", COLOR_VICTORY));

            // --- Economy events (player 0 only) ---
            gc.Events.Subscribe<BuildingCompletedEvent>(e =>
            {
                var sector = gc.State.Graph.GetSector(e.SectorId);
                if (sector.OwnerId == 0)
                    Show($"Building complete in {sector.Name}", COLOR_ECONOMY);
            });
            gc.Events.Subscribe<BuildingUpgradedEvent>(e =>
            {
                var sector = gc.State.Graph.GetSector(e.SectorId);
                if (sector.OwnerId == 0)
                    Show($"Building upgraded to level {e.NewLevel}", COLOR_ECONOMY);
            });
            gc.Events.Subscribe<WorkYardAttachedEvent>(e =>
            {
                var building = gc.State.Construction.GetBuilding(e.BuildingId);
                if (building != null && building.OwnerId == 0)
                    Show($"Work yard attached: {e.WorkYardTypeId}", COLOR_ECONOMY);
            });

            // --- Tech events (player 0 only) ---
            gc.Events.Subscribe<ResearchStartedEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show($"Research started: {e.TechId}", COLOR_TECH);
            });

            // --- Military events (player 0 only) ---
            gc.Events.Subscribe<CombatResolvedEvent>(e =>
            {
                if (e.AttackerId == 0)
                {
                    string result = e.Victory ? "Victory" : "Defeat";
                    Show($"Battle in sector {e.SectorId}: {result} (lost {e.AttackerLosses})", COLOR_MILITARY);
                }
            });
            gc.Events.Subscribe<ArmyArrivedEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show($"Army arrived at sector {e.SectorId}", COLOR_MILITARY);
            });
            gc.Events.Subscribe<GeneralHiredEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show("General hired", COLOR_MILITARY);
            });
            gc.Events.Subscribe<UnitTrainedEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show($"Trained {e.UnitType}", COLOR_MILITARY);
            });
            gc.Events.Subscribe<FortificationBuiltEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show("Sector fortified", COLOR_MILITARY);
            });

            // --- Trade events (player 0 only) ---
            gc.Events.Subscribe<OutpostClaimedEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show($"Trade outpost claimed: {e.OutpostId}", COLOR_TRADE);
            });

            // --- Prestige events (player 0 only) ---
            gc.Events.Subscribe<PrestigeLevelUpEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show($"Prestige level {e.NewLevel}!", COLOR_PRESTIGE);
            });
            gc.Events.Subscribe<PrestigeUnlockEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show($"Unlocked: {e.UnlockId}", COLOR_PRESTIGE);
            });

            // --- Victory countdown events ---
            gc.Events.Subscribe<CountdownStartedEvent>(e =>
                Show("Victory countdown started! 3 minutes...", COLOR_VICTORY));
            gc.Events.Subscribe<CountdownCancelledEvent>(e =>
                Show("Victory countdown cancelled!", COLOR_VICTORY));

            // --- Quest events (player 0 only) ---
            gc.Events.Subscribe<QuestCompletedEvent>(e =>
            {
                if (e.PlayerId == 0)
                    Show($"Quest complete: {e.QuestId}", COLOR_VICTORY);
            });
        }

        private void Update()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                entry.TimeLeft -= Time.unscaledDeltaTime;
                _entries[i] = entry;

                if (entry.TimeLeft <= 0f)
                {
                    if (entry.Go != null) Destroy(entry.Go);
                    _entries.RemoveAt(i);
                }
                else if (entry.TimeLeft < FADE_TIME && entry.Group != null)
                {
                    entry.Group.alpha = entry.TimeLeft / FADE_TIME;
                }
            }
        }

        /// <summary>Show a notification message with default economy color.</summary>
        public void Show(string message)
        {
            Show(message, COLOR_ECONOMY);
        }

        /// <summary>Show a notification message with a specific color.</summary>
        public void Show(string message, Color color)
        {
            if (_container == null) return;

            // Remove oldest if at limit
            while (_entries.Count >= MAX_VISIBLE)
            {
                if (_entries[0].Go != null) Destroy(_entries[0].Go);
                _entries.RemoveAt(0);
            }

            var go = new GameObject("Notification");
            go.transform.SetParent(_container, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300f, 22f);

            var group = go.AddComponent<CanvasGroup>();

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.fontSize = 12;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            var font = UIFactory.GetDefaultFont();
            if (font != null) tmp.font = font;

            _entries.Add(new NotificationEntry
            {
                Go = go,
                Text = tmp,
                Group = group,
                TimeLeft = DISPLAY_TIME
            });
        }

        /// <summary>Create the notification UI.</summary>
        public static NotificationUI Create(Transform canvasTransform)
        {
            var panelGo = new GameObject("Notifications");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(10f, 10f);
            panelRect.sizeDelta = new Vector2(320f, 160f);

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.LowerLeft;

            var notif = panelGo.AddComponent<NotificationUI>();
            UIFactory.SetField(notif, "_container", panelGo.transform);

            return notif;
        }
    }
}
