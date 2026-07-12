using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Build menu with the Settlers 7 three-tab structure (§14.4):
    /// Economy (house), Specials (shield, prestige-gated), Empire (crown).
    /// Locked entries render as gray silhouettes — never hidden (Critical Rule #9).
    /// Constructed by <see cref="BuildMenuFactory"/>.
    /// </summary>
    public class BuildMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        private struct BuildEntry
        {
            public Image Img;
            public TextMeshProUGUI Label;
            public BaseBuildingType Type;
        }

        private struct EmpireEntry
        {
            public Image Img;
            public TextMeshProUGUI Label;
            public string UnlockId;
        }

        private readonly List<BuildEntry> _buildEntries = new();
        private readonly List<EmpireEntry> _empireEntries = new();
        private readonly List<(Image img, GameObject content)> _tabs = new();

        private bool _isVisible;
        private Image _selectedButtonImage;
        private float _refreshTimer;
        private Coroutine _feedbackCoroutine;

        internal static readonly Color SELECTED_COLOR    = new(0.2f, 0.5f, 0.2f, 0.9f);
        internal static readonly Color DEFAULT_BTN_COLOR = new(0.25f, 0.25f, 0.25f, 0.9f);
        internal static readonly Color DIMMED_BTN_COLOR  = new(0.2f, 0.2f, 0.2f, 0.5f);
        internal static readonly Color LOCKED_BTN_COLOR  = new(0.15f, 0.15f, 0.15f, 0.6f);
        private static readonly Color UNAFFORDABLE_TEXT  = new(1f, 0.4f, 0.4f);

        /// <summary>Fired when a building type is selected from the menu.</summary>
        public event System.Action<BaseBuildingType> OnBuildingSelected;

        private void Start() => Hide();

        private void Update()
        {
            if (!_isVisible) return;

            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = 0.2f;
                RefreshEntries();
            }
        }

        /// <summary>Show the build menu.</summary>
        public void Show()
        {
            _isVisible = true;
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
            RefreshLocaleTexts();
            UpdateCostText(null);
            RefreshEntries();
        }

        /// <summary>Re-resolve creation-time baked tile labels (locale can change).</summary>
        private void RefreshLocaleTexts()
        {
            foreach (var entry in _buildEntries)
                if (entry.Label != null)
                    entry.Label.text = BuildMenuFactory.TileName(entry.Type);
            foreach (var entry in _empireEntries)
                if (entry.Label != null)
                    entry.Label.text = LocalizedNames.Prestige(entry.UnlockId);
        }

        /// <summary>Hide the build menu.</summary>
        public void Hide()
        {
            _isVisible = false;
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        /// <summary>Toggle visibility.</summary>
        public void Toggle()
        {
            if (_isVisible) Hide();
            else Show();
        }

        /// <summary>Whether the menu is currently visible.</summary>
        public bool IsVisible => _isVisible;

        /// <summary>Show a temporary feedback message (e.g. "Not enough resources").</summary>
        public void ShowFeedback(string message)
        {
            if (_feedbackText == null) return;
            _feedbackText.text = message;
            _feedbackText.gameObject.SetActive(true);
            if (_feedbackCoroutine != null)
                StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay(2f));
        }

        private System.Collections.IEnumerator HideFeedbackAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_feedbackText != null)
                _feedbackText.gameObject.SetActive(false);
        }

        /// <summary>Switch the visible tab (0 = Economy, 1 = Specials, 2 = Empire).</summary>
        public void SelectTab(int index)
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].content.SetActive(i == index);
                _tabs[i].img.color = i == index ? UIColors.BUTTON_BLUE : DEFAULT_BTN_COLOR;
            }
        }

        // --- Registration (called by BuildMenuFactory) ---

        internal void RegisterTab(Image tabButton, GameObject content) =>
            _tabs.Add((tabButton, content));

        internal void RegisterBuilding(Button btn, Image img, TextMeshProUGUI label,
            BaseBuildingType type)
        {
            _buildEntries.Add(new BuildEntry { Img = img, Label = label, Type = type });
            btn.onClick.AddListener(() => HandleBuildingClicked(type, img));
        }

        internal void RegisterEmpire(Button btn, Image img, TextMeshProUGUI label,
            string unlockId, string activeHintKey)
        {
            _empireEntries.Add(new EmpireEntry { Img = img, Label = label, UnlockId = unlockId });
            btn.onClick.AddListener(() => HandleEmpireClicked(unlockId, activeHintKey));
        }

        // --- Click handling ---

        private void HandleBuildingClicked(BaseBuildingType type, Image img)
        {
            var prestige = Presentation.GameController.Instance?.State?.Prestige;
            if (prestige != null && !BuildingPrestigeGate.IsUnlocked(prestige, 0, type))
            {
                ShowFeedback(string.Format(L.Get("ui.build.locked_feedback"),
                    LocalizedNames.Prestige(BuildingPrestigeGate.RequiredUnlock(type))));
                return;
            }

            if (_selectedButtonImage != null)
                _selectedButtonImage.color = DEFAULT_BTN_COLOR;
            _selectedButtonImage = img;
            img.color = SELECTED_COLOR;

            OnBuildingSelected?.Invoke(type);
            UpdateCostText(type);
            Hide();
        }

        private void HandleEmpireClicked(string unlockId, string activeHintKey)
        {
            var prestige = Presentation.GameController.Instance?.State?.Prestige;
            bool unlocked = prestige != null && prestige.HasUnlock(0, unlockId);
            ShowFeedback(L.Get(unlocked ? activeHintKey : "ui.build.empire.locked_hint"));
        }

        // --- State refresh ---

        /// <summary>Refresh lock state and affordability tint on all entries.</summary>
        private void RefreshEntries()
        {
            var gc = Presentation.GameController.Instance;
            var resources = gc?.GetPlayerResources(0);
            var prestige = gc?.State?.Prestige;
            if (resources == null) return;

            foreach (var e in _buildEntries)
            {
                if (prestige != null && !BuildingPrestigeGate.IsUnlocked(prestige, 0, e.Type))
                {
                    // Critical Rule #9: locked = gray silhouette, still visible
                    e.Img.color = LOCKED_BTN_COLOR;
                    e.Label.color = UIColors.TEXT_GRAY_DIM;
                    continue;
                }

                BuildingCosts.Get(e.Type, out int plankCost, out int stoneCost);
                bool affordable = resources.CanAfford(plankCost, stoneCost);
                e.Label.color = affordable ? Color.white : UNAFFORDABLE_TEXT;
                if (e.Img != _selectedButtonImage)
                    e.Img.color = affordable ? DEFAULT_BTN_COLOR : DIMMED_BTN_COLOR;
            }

            foreach (var e in _empireEntries)
            {
                bool unlocked = prestige != null && prestige.HasUnlock(0, e.UnlockId);
                e.Img.color = unlocked ? UIColors.BUTTON_BLUE : LOCKED_BTN_COLOR;
                e.Label.color = unlocked ? UIColors.TEXT_GOLD : UIColors.TEXT_GRAY_DIM;
            }
        }

        internal void UpdateCostText(BaseBuildingType? type)
        {
            if (_costText == null) return;

            if (type == null)
            {
                _costText.text = L.Get("ui.build.select_prompt");
                return;
            }

            _costText.text = type.Value switch
            {
                BaseBuildingType.Lodge => L.Get("ui.build.cost.lodge"),
                BaseBuildingType.Farm => L.Get("ui.build.cost.farm"),
                BaseBuildingType.MountainShelter => L.Get("ui.build.cost.mountain_shelter"),
                BaseBuildingType.Residence => L.Get("ui.build.cost.residence"),
                BaseBuildingType.NobleResidence => L.Get("ui.build.cost.noble_residence"),
                _ => ""
            };
        }
    }
}
