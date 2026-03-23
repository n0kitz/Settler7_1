using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Visual prestige unlock tree with 3 branches (Economy, Military, Culture).
    /// Toggle with P key. Clicking an unlock node spends a level to unlock it.
    /// </summary>
    public class PrestigeChartUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _economyColumn;
        [SerializeField] private Transform _militaryColumn;
        [SerializeField] private Transform _cultureColumn;

        private bool _isVisible;
        private readonly Dictionary<string, Image> _nodeImages = new();
        private readonly Dictionary<string, Button> _nodeButtons = new();

        private static readonly Color LOCKED_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        private static readonly Color AVAILABLE_COLOR = new Color(0.3f, 0.45f, 0.6f, 0.9f);
        private static readonly Color UNLOCKED_COLOR = new Color(0.2f, 0.6f, 0.3f, 0.9f);

        public void Show()
        {
            _isVisible = true;
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            _isVisible = false;
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (_isVisible) Hide();
            else Show();
        }

        public bool IsVisible => _isVisible;

        private void Update()
        {
            if (!_isVisible) return;
            Refresh();
        }

        private void Refresh()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            var prestige = gc.State.Prestige;
            int playerId = 0;

            if (_statusText != null)
            {
                int pts = prestige.GetPoints(playerId);
                int lvl = prestige.GetLevel(playerId);
                int unspent = prestige.GetUnspentLevels(playerId);
                _statusText.text = $"Points: {pts}  Level: {lvl}  Unspent: {unspent}";
            }

            // Update node colors
            foreach (var kvp in _nodeImages)
            {
                string unlockId = kvp.Key;
                var img = kvp.Value;

                if (prestige.HasUnlock(playerId, unlockId))
                {
                    img.color = UNLOCKED_COLOR;
                }
                else if (CanUnlock(prestige, playerId, unlockId))
                {
                    img.color = AVAILABLE_COLOR;
                }
                else
                {
                    img.color = LOCKED_COLOR;
                }
            }
        }

        private bool CanUnlock(PrestigeSystem prestige, int playerId, string unlockId)
        {
            if (prestige.GetUnspentLevels(playerId) <= 0) return false;
            var def = PrestigeDatabase.Get(unlockId);
            if (def == null) return false;
            if (prestige.GetLevel(playerId) < def.MinLevel) return false;
            if (def.PrerequisiteId != null && !prestige.HasUnlock(playerId, def.PrerequisiteId))
                return false;
            return !prestige.HasUnlock(playerId, unlockId);
        }

        /// <summary>Called by PrestigeChartUIFactory when a node is clicked.</summary>
        internal void HandleNodeClicked(string unlockId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            bool success = gc.State.Prestige.TryUnlock(0, unlockId);
            if (success)
            {
                var def = PrestigeDatabase.Get(unlockId);
                Debug.Log($"Unlocked: {def?.DisplayName ?? unlockId}");
            }
            Refresh();
        }

        /// <summary>Called by PrestigeChartUIFactory to register node visuals.</summary>
        internal void RegisterNode(string unlockId, Image img, Button btn)
        {
            _nodeImages[unlockId] = img;
            _nodeButtons[unlockId] = btn;
        }

        /// <summary>Create the prestige chart UI programmatically.</summary>
        public static PrestigeChartUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return PrestigeChartUIFactory.Create(canvasTransform, font);
        }
    }
}
