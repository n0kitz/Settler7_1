using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    public class TechTreeUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _tier1Container;
        [SerializeField] private Transform _tier2Container;
        [SerializeField] private Transform _tier3Container;

        internal static readonly Color LOCKED_COLOR = new Color(0.25f, 0.25f, 0.25f, 0.9f);
        private static readonly Color AVAILABLE_COLOR = new Color(0.2f, 0.45f, 0.25f, 0.9f);
        private static readonly Color OWNED_COLOR = new Color(0.15f, 0.35f, 0.55f, 0.9f);
        private static readonly Color TAKEN_COLOR = new Color(0.5f, 0.15f, 0.15f, 0.9f);
        private static readonly Color RESEARCHING_COLOR = new Color(0.5f, 0.45f, 0.1f, 0.9f);

        internal readonly Dictionary<string, Image> _nodeImages = new();
        internal readonly Dictionary<string, TextMeshProUGUI> _nodeLabels = new();

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

        public void OnTechClicked(string techId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            int playerId = 0;
            var research = gc.State.Research;

            if (research.HasTech(playerId, techId))
            {
                UpdateStatus("Already researched.", UIColors.TEXT_GOLD);
                return;
            }

            if (research.IsResearchedGlobally(techId))
            {
                UpdateStatus("Already taken by another player!", UIColors.TEXT_RED_BRIGHT);
                return;
            }

            if (research.IsBlocked(techId))
            {
                UpdateStatus("Currently being researched by another player!", UIColors.TEXT_RED_BRIGHT);
                return;
            }

            bool success = research.StartResearch(playerId, techId);
            if (success)
            {
                var def = TechTree.Get(techId);
                string name = def != null ? def.DisplayName : techId;
                UpdateStatus($"Researching: {name}...", UIColors.TEXT_GREEN_LIGHT);
            }
            else
            {
                UpdateStatus("Cannot research — prerequisites not met.", UIColors.TEXT_RED_BRIGHT);
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
            var research = gc.State.Research;

            if (_statusText != null)
            {
                int count = research.GetTechCount(playerId);
                int active = 0;
                foreach (var task in research.ActiveTasks)
                    if (task.PlayerId == playerId) active++;
                _statusText.text = $"Techs: {count}/18  |  Researching: {active}";
                _statusText.color = UIColors.TEXT_GOLD;
            }

            foreach (var kvp in _nodeImages)
            {
                string techId = kvp.Key;
                var img = kvp.Value;
                if (img == null) continue;

                if (research.HasTech(playerId, techId))
                {
                    img.color = OWNED_COLOR;
                }
                else if (research.IsResearchedGlobally(techId))
                {
                    img.color = TAKEN_COLOR;
                }
                else if (research.IsBlocked(techId))
                {
                    img.color = RESEARCHING_COLOR;
                }
                else if (CanResearch(playerId, techId, research))
                {
                    img.color = AVAILABLE_COLOR;
                }
                else
                {
                    img.color = LOCKED_COLOR;
                }
            }
        }

        private static bool CanResearch(int playerId, string techId, ResearchSystem research)
        {
            var def = TechTree.Get(techId);
            if (def == null) return false;
            if (def.PrerequisiteId != null && !research.HasTech(playerId, def.PrerequisiteId))
                return false;
            return true;
        }

        private void UpdateStatus(string msg, Color color)
        {
            if (_statusText == null) return;
            _statusText.text = msg;
            _statusText.color = color;
        }

        public static TechTreeUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return TechTreeUIFactory.Create(canvasTransform, font);
        }
    }
}
