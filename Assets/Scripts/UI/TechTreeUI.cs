using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Technology tree overlay showing 18 techs in 3 tiers.
    /// Clicking a tech starts research (if prerequisites met).
    /// Toggle with T key.
    /// </summary>
    public class TechTreeUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _tier1Container;
        [SerializeField] private Transform _tier2Container;
        [SerializeField] private Transform _tier3Container;

        private bool _isVisible;
        internal readonly Dictionary<string, Image> _nodeImages = new();
        internal readonly Dictionary<string, TextMeshProUGUI> _nodeLabels = new();

        internal static readonly Color AVAILABLE_COLOR = new Color(0.25f, 0.4f, 0.55f, 0.9f);
        internal static readonly Color RESEARCHED_OWN_COLOR = new Color(0.2f, 0.6f, 0.3f, 0.9f);
        internal static readonly Color RESEARCHED_OTHER_COLOR = new Color(0.5f, 0.2f, 0.2f, 0.9f);
        internal static readonly Color BLOCKED_COLOR = new Color(0.5f, 0.45f, 0.15f, 0.9f);
        internal static readonly Color LOCKED_COLOR = new Color(0.25f, 0.25f, 0.25f, 0.9f);
        internal static readonly Color RESEARCHING_COLOR = new Color(0.4f, 0.5f, 0.2f, 0.9f);

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
            var research = gc.State.Research;

            if (_statusText != null)
                _statusText.text = $"Technologies Researched: {research.GetTechCount(playerId)}/18";

            string activeResearchId = null;
            float activeProgress = 0f;
            foreach (var task in research.ActiveTasks)
            {
                if (task.PlayerId == playerId)
                {
                    activeResearchId = task.TechId;
                    activeProgress = task.Progress;
                    break;
                }
            }

            foreach (var kvp in _nodeImages)
            {
                string techId = kvp.Key;
                var img = kvp.Value;
                var label = _nodeLabels.TryGetValue(techId, out var l) ? l : null;
                var techDef = TechTree.Get(techId);
                if (techDef == null) continue;

                string displayText = techDef.DisplayName;

                if (research.HasTech(playerId, techId))
                    img.color = RESEARCHED_OWN_COLOR;
                else if (research.IsResearchedGlobally(techId))
                { img.color = RESEARCHED_OTHER_COLOR; displayText += " [TAKEN]"; }
                else if (techId == activeResearchId)
                { img.color = RESEARCHING_COLOR; displayText += $" [{(int)(activeProgress * 100)}%]"; }
                else if (research.IsBlocked(techId))
                { img.color = BLOCKED_COLOR; displayText += " [BLOCKED]"; }
                else if (CanResearch(research, playerId, techId))
                    img.color = AVAILABLE_COLOR;
                else
                    img.color = LOCKED_COLOR;

                if (label != null) label.text = displayText;
            }
        }

        private bool CanResearch(ResearchSystem research, int playerId, string techId)
        {
            if (research.IsResearchedGlobally(techId)) return false;
            if (research.IsBlocked(techId)) return false;
            if (research.HasTech(playerId, techId)) return false;
            var techDef = TechTree.Get(techId);
            if (techDef == null) return false;
            if (techDef.PrerequisiteId != null && !research.HasTech(playerId, techDef.PrerequisiteId))
                return false;
            return true;
        }

        internal void OnTechClicked(string techId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;
            bool started = gc.State.Research.StartResearch(0, techId);
            if (started)
            {
                var def = TechTree.Get(techId);
                Debug.Log($"Started researching: {def?.DisplayName ?? techId}");
            }
            Refresh();
        }

        /// <summary>Create the tech tree UI programmatically.</summary>
        public static TechTreeUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return TechTreeUIFactory.Create(canvasTransform, font);
        }
    }
}
