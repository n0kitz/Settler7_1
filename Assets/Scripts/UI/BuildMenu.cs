using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Build menu panel showing available building types.
    /// Click a button to enter placement mode for that building.
    /// </summary>
    public class BuildMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        private bool _isVisible;
        private Image _selectedButtonImage;
        private float _affordabilityTimer;
        private static readonly Color SELECTED_COLOR = new Color(0.2f, 0.5f, 0.2f, 0.9f);
        private static readonly Color DEFAULT_BTN_COLOR = new Color(0.25f, 0.25f, 0.25f, 0.9f);
        private static readonly Color DIMMED_BTN_COLOR = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        private static readonly Color UNAFFORDABLE_TEXT = new Color(1f, 0.4f, 0.4f);

        private readonly List<(Button btn, Image img, TextMeshProUGUI costText, BaseBuildingType type)>
            _buttonEntries = new();

        /// <summary>Fired when a building type is selected from the menu.</summary>
        public event System.Action<BaseBuildingType> OnBuildingSelected;

        private void Start()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            Hide();
        }

        private void Update()
        {
            if (!_isVisible) return;

            _affordabilityTimer -= Time.deltaTime;
            if (_affordabilityTimer <= 0f)
            {
                _affordabilityTimer = 1f;
                RefreshAffordability();
            }
        }

        /// <summary>Show the build menu.</summary>
        public void Show()
        {
            _isVisible = true;
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
            UpdateCostText(null);
            RefreshAffordability();
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

        private Coroutine _feedbackCoroutine;

        private System.Collections.IEnumerator HideFeedbackAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_feedbackText != null)
                _feedbackText.gameObject.SetActive(false);
        }

        /// <summary>Check affordability per button and tint accordingly.</summary>
        private void RefreshAffordability()
        {
            var gc = Presentation.GameController.Instance;
            var resources = gc?.GetPlayerResources(0);
            if (resources == null) return;

            foreach (var (btn, img, costText, type) in _buttonEntries)
            {
                BuildingCosts.Get(type, out int plankCost, out int stoneCost);
                bool affordable = resources.CanAfford(plankCost, stoneCost);

                if (costText != null)
                    costText.color = affordable ? Color.white : UNAFFORDABLE_TEXT;

                // Don't override selected button color
                if (img != null && img != _selectedButtonImage)
                    img.color = affordable ? DEFAULT_BTN_COLOR : DIMMED_BTN_COLOR;
            }
        }

        /// <summary>
        /// Create the build menu UI programmatically (for bootstrap scene).
        /// </summary>
        public static BuildMenu Create(Transform canvasTransform, TMP_FontAsset font)
        {
            // Panel root
            var panelGo = new GameObject("BuildMenu");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(10f, 10f);
            panelRect.sizeDelta = new Vector2(320f, 260f);

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            // Vertical layout
            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 4f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;

            // Title
            UIFactory.CreateLabel(panelGo.transform, "Title", "Build Menu", 18,
                FontStyles.Bold, font);

            // Button container
            var containerGo = new GameObject("ButtonContainer");
            containerGo.transform.SetParent(panelGo.transform, false);
            var containerRect = containerGo.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(300f, 140f);

            var containerLayout = containerGo.AddComponent<VerticalLayoutGroup>();
            containerLayout.spacing = 3f;
            containerLayout.childForceExpandWidth = true;
            containerLayout.childForceExpandHeight = false;

            var containerLayoutElem = containerGo.AddComponent<LayoutElement>();
            containerLayoutElem.preferredHeight = 180f;

            // Cost text
            var costText = UIFactory.CreateLabel(panelGo.transform, "CostText", "", 13,
                FontStyles.Normal, font);
            costText.color = new Color(0.8f, 0.8f, 0.6f);

            // Feedback text (hidden by default)
            var feedbackText = UIFactory.CreateLabel(panelGo.transform, "FeedbackText", "", 14,
                FontStyles.Bold, font);
            feedbackText.color = new Color(1f, 0.3f, 0.3f);
            feedbackText.gameObject.SetActive(false);

            // BuildMenu component
            var menu = panelGo.AddComponent<BuildMenu>();
            UIFactory.SetField(menu, "_panelRoot", panelGo);
            UIFactory.SetField(menu, "_buttonContainer", containerGo.transform);
            UIFactory.SetField(menu, "_titleText", costText); // unused but kept for compat
            UIFactory.SetField(menu, "_costText", costText);
            UIFactory.SetField(menu, "_feedbackText", feedbackText);

            // Create building buttons with section headers
            CreateSectionHeader(containerGo.transform, "Basic", font);
            CreateBuildingButton(containerGo.transform, menu, BaseBuildingType.Lodge,
                "Lodge (3 Planks)", font);
            CreateBuildingButton(containerGo.transform, menu, BaseBuildingType.Farm,
                "Farm (3 Planks)", font);
            CreateSectionHeader(containerGo.transform, "Advanced", font);
            CreateBuildingButton(containerGo.transform, menu, BaseBuildingType.MountainShelter,
                "Mountain Shelter (2P+1S)", font);
            CreateBuildingButton(containerGo.transform, menu, BaseBuildingType.Residence,
                "Residence (2P+1S)", font);
            CreateBuildingButton(containerGo.transform, menu, BaseBuildingType.NobleResidence,
                "Noble Residence (3P+2S)", font);

            return menu;
        }

        private static void CreateSectionHeader(Transform parent, string label, TMP_FontAsset font)
        {
            var headerText = UIFactory.CreateLabel(parent, $"Header_{label}", label, 11,
                FontStyles.Bold, font);
            headerText.color = new Color(0.6f, 0.6f, 0.6f);
            var le = headerText.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 16f;
        }

        private static void CreateBuildingButton(Transform parent, BuildMenu menu,
            BaseBuildingType type, string label, TMP_FontAsset font)
        {
            var btnGo = new GameObject($"Btn_{type}");
            btnGo.transform.SetParent(parent, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280f, 24f);

            var layoutElem = btnGo.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 24f;

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = new Color(0.25f, 0.25f, 0.25f, 0.9f);

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.45f, 0.35f);
            colors.pressedColor = new Color(0.2f, 0.4f, 0.2f);
            btn.colors = colors;

            // Button text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 0f);
            textRect.offsetMax = new Vector2(-8f, 0f);

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;
            if (font != null) tmp.font = font;

            // Track for affordability
            menu._buttonEntries.Add((btn, btnImage, tmp, type));

            // Click handler
            btn.onClick.AddListener(() =>
            {
                if (menu._selectedButtonImage != null)
                    menu._selectedButtonImage.color = DEFAULT_BTN_COLOR;
                menu._selectedButtonImage = btnImage;
                btnImage.color = SELECTED_COLOR;

                menu.OnBuildingSelected?.Invoke(type);
                menu.UpdateCostText(type);
                menu.Hide();
            });
        }

        private void UpdateCostText(BaseBuildingType? type)
        {
            if (_costText == null) return;

            if (type == null)
            {
                _costText.text = "Select a building to place";
                return;
            }

            _costText.text = type.Value switch
            {
                BaseBuildingType.Lodge => "Cost: 3 Planks | Pop: +1",
                BaseBuildingType.Farm => "Cost: 3 Planks | Pop: +1",
                BaseBuildingType.MountainShelter => "Cost: 2 Planks, 1 Stone | Pop: +1",
                BaseBuildingType.Residence => "Cost: 2 Planks, 1 Stone | Pop: +4",
                BaseBuildingType.NobleResidence => "Cost: 3 Planks, 2 Stone | Pop: +5",
                _ => ""
            };
        }
    }
}
