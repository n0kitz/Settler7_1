using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    public class TradeMapUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _outpostsContainer;
        [SerializeField] private Transform _activeTradesContainer;

        internal static readonly Color SPECIAL_COLOR = new Color(0.6f, 0.5f, 0.2f, 0.9f);
        internal static readonly Color UNCLAIMED_COLOR = new Color(0.33f, 0.38f, 0.44f, 0.9f);
        private static readonly Color OWNED_COLOR = new Color(0.15f, 0.35f, 0.55f, 0.9f);
        private static readonly Color OTHER_COLOR = new Color(0.5f, 0.15f, 0.15f, 0.9f);

        internal readonly Dictionary<string, Image> _outpostImages = new();
        internal readonly Dictionary<string, TextMeshProUGUI> _outpostLabels = new();

        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.5f;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            RefreshNodes();
        }

        public void Hide() { if (_panelRoot != null) _panelRoot.SetActive(false); }
        public void Toggle() { if (IsVisible) Hide(); else Show(); }

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
                _statusText.text = $"Outposts: {claimed}/{total}  |  Traders en route: {active}";
                _statusText.color = UIColors.TEXT_GOLD;
            }

            foreach (var kvp in _outpostImages)
            {
                string opId = kvp.Key;
                var img = kvp.Value;
                if (img == null) continue;

                var outpost = trade.Map.GetOutpost(opId);
                if (outpost == null) continue;

                if (!outpost.IsClaimed)
                {
                    img.color = outpost.IsSpecial ? SPECIAL_COLOR : UNCLAIMED_COLOR;
                }
                else if (outpost.ClaimedBy == playerId)
                {
                    img.color = OWNED_COLOR;
                }
                else
                {
                    img.color = OTHER_COLOR;
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
