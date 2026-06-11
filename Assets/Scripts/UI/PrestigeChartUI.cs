using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    public class PrestigeChartUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _economyColumn;
        [SerializeField] private Transform _militaryColumn;
        [SerializeField] private Transform _cultureColumn;

        internal static readonly Color LOCKED_COLOR = new Color(0.25f, 0.25f, 0.25f, 0.9f);
        private static readonly Color AVAILABLE_COLOR = new Color(0.2f, 0.45f, 0.25f, 0.9f);
        private static readonly Color OWNED_COLOR = new Color(0.15f, 0.35f, 0.55f, 0.9f);

        private readonly Dictionary<string, Image> _nodeImages = new();
        private readonly Dictionary<string, Button> _nodeButtons = new();

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

        public void RegisterNode(string id, Image img, Button btn)
        {
            _nodeImages[id] = img;
            _nodeButtons[id] = btn;
        }

        public void HandleNodeClicked(string id)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            int playerId = 0;
            var prestige = gc.State.Prestige;

            if (prestige.HasUnlock(playerId, id))
            {
                UpdateStatus(L.Get("ui.prestige.already_unlocked"), UIColors.TEXT_GOLD);
                return;
            }

            if (prestige.GetUnspentLevels(playerId) <= 0)
            {
                UpdateStatus(L.Get("ui.prestige.no_levels"), UIColors.TEXT_RED_BRIGHT);
                return;
            }

            bool success = prestige.TryUnlock(playerId, id);
            if (success)
            {
                var def = PrestigeDatabase.Get(id);
                string name = def != null ? def.DisplayName : id;
                UpdateStatus(string.Format(L.Get("ui.prestige.unlocked"), name),
                    UIColors.TEXT_GREEN_LIGHT);
            }
            else
            {
                UpdateStatus(L.Get("ui.prestige.cannot_unlock"), UIColors.TEXT_RED_BRIGHT);
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
            var prestige = gc.State.Prestige;

            if (_statusText != null)
            {
                int level = prestige.GetLevel(playerId);
                int unspent = prestige.GetUnspentLevels(playerId);
                int points = prestige.GetPoints(playerId);
                _statusText.text = $"Level: {level}  |  Unspent: {unspent}  |  Points: {points}";
            }

            foreach (var kvp in _nodeImages)
            {
                string nodeId = kvp.Key;
                var img = kvp.Value;
                if (img == null) continue;

                if (prestige.HasUnlock(playerId, nodeId))
                {
                    img.color = OWNED_COLOR;
                }
                else if (CanUnlock(playerId, nodeId, prestige))
                {
                    img.color = AVAILABLE_COLOR;
                }
                else
                {
                    img.color = LOCKED_COLOR;
                }
            }
        }

        private static bool CanUnlock(int playerId, string id, PrestigeSystem prestige)
        {
            if (prestige.GetUnspentLevels(playerId) <= 0) return false;
            var def = PrestigeDatabase.Get(id);
            if (def == null) return false;
            if (prestige.GetLevel(playerId) < def.MinLevel) return false;
            if (def.PrerequisiteId != null && !prestige.HasUnlock(playerId, def.PrerequisiteId))
                return false;
            return true;
        }

        private void UpdateStatus(string msg, Color color)
        {
            if (_statusText == null) return;
            _statusText.text = msg;
            _statusText.color = color;
        }

        public static PrestigeChartUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return PrestigeChartUIFactory.Create(canvasTransform, font);
        }
    }
}
