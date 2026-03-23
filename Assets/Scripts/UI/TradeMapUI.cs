using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Trade map overlay showing outpost network.
    /// Displays claimed/unclaimed outposts with exchange details.
    /// Click to send a trader or execute a trade.
    /// Toggle with R key.
    /// </summary>
    public class TradeMapUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _outpostsContainer;
        [SerializeField] private Transform _activeTradesContainer;

        private bool _isVisible;
        internal readonly Dictionary<string, Image> _outpostImages = new();
        internal readonly Dictionary<string, TextMeshProUGUI> _outpostLabels = new();
        private readonly List<GameObject> _dynamicElements = new();

        internal static readonly Color UNCLAIMED_COLOR = new Color(0.3f, 0.35f, 0.4f, 0.9f);
        internal static readonly Color CLAIMED_OWN_COLOR = new Color(0.2f, 0.55f, 0.35f, 0.9f);
        internal static readonly Color CLAIMED_OTHER_COLOR = new Color(0.55f, 0.25f, 0.2f, 0.9f);
        internal static readonly Color SPECIAL_COLOR = new Color(0.6f, 0.5f, 0.2f, 0.9f);

        public void Show()
        {
            _isVisible = true;
            if (_panelRoot != null) _panelRoot.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            _isVisible = false;
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (_isVisible) Hide(); else Show();
        }

        public bool IsVisible => _isVisible;

        private void Update()
        {
            if (_isVisible) Refresh();
        }

        private void Refresh()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            int playerId = 0;
            var trade = gc.State.Trade;
            var tradeMap = gc.State.TradeMapData;

            if (_statusText != null)
            {
                int claimed = tradeMap.GetClaimedCount(playerId);
                _statusText.text = $"Outposts Claimed: {claimed}/{tradeMap.AllOutposts.Count}";
            }

            foreach (var kvp in _outpostImages)
            {
                string outpostId = kvp.Key;
                var img = kvp.Value;
                var outpost = tradeMap.GetOutpost(outpostId);
                if (outpost == null) continue;

                if (outpost.ClaimedBy == playerId) img.color = CLAIMED_OWN_COLOR;
                else if (outpost.IsClaimed) img.color = CLAIMED_OTHER_COLOR;
                else if (outpost.IsSpecial) img.color = SPECIAL_COLOR;
                else img.color = UNCLAIMED_COLOR;

                var label = _outpostLabels.TryGetValue(outpostId, out var l) ? l : null;
                if (label != null)
                {
                    string status = outpost.ClaimedBy == playerId ? " [YOURS]"
                        : outpost.IsClaimed ? $" [P{outpost.ClaimedBy}]" : "";
                    label.text = $"{outpost.DisplayName}{status}";
                }
            }

            foreach (var go in _dynamicElements)
                if (go != null) Destroy(go);
            _dynamicElements.Clear();

            if (_activeTradesContainer != null)
            {
                foreach (var task in trade.ActiveTasks)
                {
                    if (task.PlayerId != playerId) continue;
                    int pct = (int)(task.Progress * 100);
                    var label = CreateDynamicLabel(_activeTradesContainer,
                        $"Trader → {task.OutpostId} [{pct}%]");
                    _dynamicElements.Add(label);
                }

                if (trade.ActiveTasks.Count == 0)
                {
                    var noTrades = CreateDynamicLabel(_activeTradesContainer, "No active traders.");
                    noTrades.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.6f);
                    _dynamicElements.Add(noTrades);
                }
            }
        }

        internal void OnOutpostClicked(string outpostId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            int playerId = 0;
            var tradeMap = gc.State.TradeMapData;
            var outpost = tradeMap.GetOutpost(outpostId);
            if (outpost == null) return;

            if (outpost.ClaimedBy == playerId)
            {
                bool traded = gc.State.Trade.ExecuteTrade(playerId, outpostId);
                Debug.Log(traded
                    ? $"Trade executed at {outpost.DisplayName}"
                    : $"Cannot trade at {outpost.DisplayName}: insufficient resources");
            }
            else if (!outpost.IsClaimed)
            {
                bool sent = gc.State.Trade.SendTrader(playerId, outpostId);
                Debug.Log(sent
                    ? $"Trader sent to claim {outpost.DisplayName}"
                    : "Cannot send trader (need Export Office prestige unlock)");
            }

            Refresh();
        }

        private GameObject CreateDynamicLabel(Transform parent, string text)
        {
            var go = new GameObject("DynLabel");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 18f);
            go.AddComponent<LayoutElement>().preferredHeight = 18f;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;
            return go;
        }

        /// <summary>Create the trade map UI programmatically.</summary>
        public static TradeMapUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return TradeMapUIFactory.Create(canvasTransform, font);
        }
    }
}
