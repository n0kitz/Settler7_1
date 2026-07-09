using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Parchment trade map panel (§14.7). Outpost card frames follow claim
    /// state: gold = yours, red = other player, gray = unclaimed. Nodes are
    /// rebuilt from the live trade map whenever the panel opens.
    /// </summary>
    public class TradeMapUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusText;

        internal static readonly Color SPECIAL_COLOR = new Color(0.62f, 0.50f, 0.18f);
        internal static readonly Color UNCLAIMED_COLOR = new Color(0.48f, 0.45f, 0.40f);
        internal static readonly Color OWNED_COLOR = new Color(0.82f, 0.64f, 0.20f);
        private static readonly Color OTHER_COLOR = new Color(0.62f, 0.16f, 0.12f);

        internal readonly Dictionary<string, Image> _outpostImages = new();
        internal readonly Dictionary<string, TextMeshProUGUI> _outpostLabels = new();

        /// <summary>Set by the factory; used when rebuilding nodes on Show.</summary>
        internal TMP_FontAsset MapFont;
        internal Transform RoutesRoot;
        internal Transform NodesRoot;

        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.5f;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            if (_titleText != null) _titleText.text = L.Get("ui.trade.title");
            TradeMapUIFactory.RebuildNodes(this);
            RefreshNodes();
        }

        public void Hide() { if (_panelRoot != null) _panelRoot.SetActive(false); }
        public void Toggle() { if (IsVisible) Hide(); else Show(); }

        /// <summary>Node card exchange line: "3 Fisch → 6 Münzen  [2]".</summary>
        internal static string ExchangeText(TradeOutpost outpost, int traders)
        {
            return $"{outpost.InputAmount} {LocalizedNames.Resource(outpost.InputResource)}" +
                $" → {outpost.OutputAmount} {LocalizedNames.Resource(outpost.OutputResource)}" +
                $"  [{traders}]";
        }

        public void OnOutpostClicked(string outpostId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            int playerId = 0;
            var trade = gc.State.Trade;
            var outpost = trade.Map.GetOutpost(outpostId);
            if (outpost == null) return;

            if (!outpost.IsClaimed)
            {
                bool sent = trade.SendTrader(playerId, outpostId);
                if (sent)
                {
                    UpdateStatus($"Trader sent to {outpost.DisplayName}!", UIColors.TEXT_GREEN_LIGHT);
                }
                else
                {
                    UpdateStatus("Cannot send trader — need Export Office prestige unlock.",
                        UIColors.TEXT_RED_BRIGHT);
                }
            }
            else if (outpost.ClaimedBy == playerId)
            {
                bool traded = trade.ExecuteTrade(playerId, outpostId);
                if (traded)
                {
                    UpdateStatus(
                        $"Traded {outpost.InputAmount} {outpost.InputResource} → {outpost.OutputAmount} {outpost.OutputResource}!",
                        UIColors.TEXT_GREEN_LIGHT);
                }
                else
                {
                    UpdateStatus(
                        $"Not enough {outpost.InputResource}! (need {outpost.InputAmount})",
                        UIColors.TEXT_RED_BRIGHT);
                }
            }
            else
            {
                UpdateStatus("This outpost belongs to another player!", UIColors.TEXT_RED_BRIGHT);
            }

            RefreshNodes();
        }

        private void Update()
        {
            if (!IsVisible) return;
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = REFRESH_INTERVAL;
                RefreshNodes();
            }
        }

        private void RefreshNodes()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            int playerId = 0;
            var trade = gc.State.Trade;

            if (_statusText != null)
            {
                int claimed = trade.Map.GetClaimedCount(playerId);
                int total = trade.Map.AllOutposts.Count;
                int active = 0;
                foreach (var task in trade.ActiveTasks)
                    if (task.PlayerId == playerId) active++;
                _statusText.text = string.Format(L.Get("ui.trade.status"),
                    claimed, total, active);
            }

            foreach (var kvp in _outpostImages)
            {
                string opId = kvp.Key;
                var img = kvp.Value;
                if (img == null) continue;

                var outpost = trade.Map.GetOutpost(opId);
                if (outpost == null) continue;

                if (!outpost.IsClaimed)
                    img.color = outpost.IsSpecial ? SPECIAL_COLOR : UNCLAIMED_COLOR;
                else if (outpost.ClaimedBy == playerId)
                    img.color = OWNED_COLOR;
                else
                    img.color = OTHER_COLOR;

                // Keep the exchange line's trader count current
                if (_outpostLabels.TryGetValue(opId, out var label) && label != null)
                {
                    int enRoute = 0;
                    foreach (var task in trade.ActiveTasks)
                        if (task.OutpostId == opId) enRoute++;
                    label.text = ExchangeText(outpost, enRoute);
                }
            }
        }

        private void UpdateStatus(string msg, Color color)
        {
            if (_statusText == null) return;
            _statusText.text = msg;
            _statusText.color = color;
        }

        public static TradeMapUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return TradeMapUIFactory.Create(canvasTransform, font);
        }
    }
}
