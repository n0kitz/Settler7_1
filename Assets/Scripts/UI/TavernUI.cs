using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Tavern panel: exchange Beer→Coins, Coins→Tools, hire generals.
    /// Toggle with V key.
    /// </summary>
    public class TavernUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _inventoryText;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        private TMP_FontAsset _font;
        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.5f;
        private float _feedbackClearTimer;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            RefreshNow();
        }

        public void Hide() { if (_panelRoot != null) _panelRoot.SetActive(false); }
        public void Toggle() { if (IsVisible) Hide(); else Show(); }

        private void Update()
        {
            if (!IsVisible) return;
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = REFRESH_INTERVAL;
                RefreshNow();
            }

            if (_feedbackClearTimer > 0f)
            {
                _feedbackClearTimer -= Time.deltaTime;
                if (_feedbackClearTimer <= 0f && _feedbackText != null)
                    _feedbackText.gameObject.SetActive(false);
            }
        }

        private void RefreshNow()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            if (_inventoryText != null)
            {
                var res = gc.State.PlayerResources.ContainsKey(0)
                    ? gc.State.PlayerResources[0] : null;
                if (res != null)
                {
                    int beer = res.Get(ResourceType.Beer);
                    int coins = res.Get(ResourceType.Coins);
                    int tools = res.Get(ResourceType.Tools);
                    int generals = gc.State.Army.GetGenerals(0).Count;
                    _inventoryText.text =
                        $"Beer: {beer}    Coins: {coins}    Tools: {tools}    Generals: {generals}/5";
                }
            }
        }

        private void ShowFeedback(string msg, Color color)
        {
            if (_feedbackText == null) return;
            _feedbackText.text = msg;
            _feedbackText.color = color;
            _feedbackText.gameObject.SetActive(true);
            _feedbackClearTimer = 3f;
        }

        private void ExchangeBeer(int beerCount)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            var res = gc.State.PlayerResources.ContainsKey(0)
                ? gc.State.PlayerResources[0] : null;
            if (res == null) return;

            int available = res.Get(ResourceType.Beer);
            if (available < beerCount)
            {
                ShowFeedback($"Not enough Beer! ({available}/{beerCount})",
                    UIColors.TEXT_RED_BRIGHT);
                return;
            }

            int coins = gc.State.Tavern.ExchangeBeerForCoins(0, beerCount);
            ShowFeedback($"Exchanged {beerCount} Beer → {coins} Coins!",
                UIColors.TEXT_GREEN_LIGHT);
            RefreshNow();
        }

        private void ExchangeCoins(int toolCount)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            var res = gc.State.PlayerResources.ContainsKey(0)
                ? gc.State.PlayerResources[0] : null;
            if (res == null) return;

            int coinCost = toolCount * 5;
            int available = res.Get(ResourceType.Coins);
            if (available < coinCost)
            {
                ShowFeedback($"Not enough Coins! ({available}/{coinCost})",
                    UIColors.TEXT_RED_BRIGHT);
                return;
            }

            int tools = gc.State.Tavern.ExchangeCoinsForTools(0, toolCount);
            ShowFeedback($"Exchanged {coinCost} Coins → {tools} Tools!",
                UIColors.TEXT_GREEN_LIGHT);
            RefreshNow();
        }

        private void HireGeneral()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            var res = gc.State.PlayerResources.ContainsKey(0)
                ? gc.State.PlayerResources[0] : null;
            if (res == null) return;

            int coins = res.Get(ResourceType.Coins);
            if (coins < 10)
            {
                ShowFeedback($"Not enough Coins! ({coins}/10)", UIColors.TEXT_RED_BRIGHT);
                return;
            }

            var sectors = gc.State.Graph.GetSectorsOwnedBy(0);
            if (sectors.Count == 0)
            {
                ShowFeedback("No owned sectors!", UIColors.TEXT_RED_BRIGHT);
                return;
            }

            var gen = gc.State.Tavern.HireGeneral(0, sectors[0].Id);
            if (gen == null)
            {
                ShowFeedback("Cannot hire — max generals reached or missing prestige unlock.",
                    UIColors.TEXT_RED_BRIGHT);
                return;
            }

            ShowFeedback($"Hired General #{gen.Id} in Sector {gen.SectorId}! (-10 Coins)",
                UIColors.TEXT_GREEN_LIGHT);
            RefreshNow();
        }

        public static TavernUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("TavernUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.15f);
            rect.anchorMax = new Vector2(0.8f, 0.85f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.04f, 0.95f);

            // Title
            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Tavern  [V]", 22, FontStyles.Bold, font);
            title.alignment = TextAlignmentOptions.Center;
            title.color = UIColors.TEXT_HEADER_GOLD;
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(0f, 30f);

            // Inventory line
            var inventoryText = UIFactory.CreateLabel(panelGo.transform, "Inventory",
                "Beer: 0    Coins: 0    Tools: 0    Generals: 0/5",
                14, FontStyles.Normal, font);
            inventoryText.color = UIColors.TEXT_GOLD;
            inventoryText.alignment = TextAlignmentOptions.Center;
            var invRect = inventoryText.GetComponent<RectTransform>();
            invRect.anchorMin = new Vector2(0f, 1f);
            invRect.anchorMax = new Vector2(1f, 1f);
            invRect.pivot = new Vector2(0.5f, 1f);
            invRect.anchoredPosition = new Vector2(0f, -42f);
            invRect.sizeDelta = new Vector2(0f, 20f);

            // Content area with vertical layout
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(panelGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.1f, 0.08f);
            contentRect.anchorMax = new Vector2(0.9f, 0.88f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8f;
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childAlignment = TextAnchor.UpperCenter;

            var panel = panelGo.AddComponent<TavernUI>();
            panel._font = font;

            // === Beer → Coins section ===
            var beerHeader = UIFactory.CreateLabel(contentGo.transform, "BeerHeader",
                "Beer → Coins  (1 Beer = 3 Coins)", 15, FontStyles.Bold, font);
            beerHeader.color = UIColors.ACCENT_ORANGE;
            beerHeader.alignment = TextAlignmentOptions.Center;

            UIFactory.CreateButton(contentGo.transform, "Exchange 1 Beer → 3 Coins", font,
                UIColors.BUTTON_GREEN, () => panel.ExchangeBeer(1),
                new Vector2(0f, 38f), 13f);

            UIFactory.CreateButton(contentGo.transform, "Exchange 5 Beer → 15 Coins", font,
                UIColors.BUTTON_GREEN, () => panel.ExchangeBeer(5),
                new Vector2(0f, 38f), 13f);

            // Spacer
            CreateSpacer(contentGo.transform, 6f);

            // === Coins → Tools section ===
            var toolsHeader = UIFactory.CreateLabel(contentGo.transform, "ToolsHeader",
                "Coins → Tools  (5 Coins = 1 Tool)", 15, FontStyles.Bold, font);
            toolsHeader.color = UIColors.ACCENT_ORANGE;
            toolsHeader.alignment = TextAlignmentOptions.Center;

            UIFactory.CreateButton(contentGo.transform, "Buy 1 Tool (5 Coins)", font,
                UIColors.BUTTON_BLUE, () => panel.ExchangeCoins(1),
                new Vector2(0f, 38f), 13f);

            UIFactory.CreateButton(contentGo.transform, "Buy 3 Tools (15 Coins)", font,
                UIColors.BUTTON_BLUE, () => panel.ExchangeCoins(3),
                new Vector2(0f, 38f), 13f);

            // Spacer
            CreateSpacer(contentGo.transform, 6f);

            // === Hire General section ===
            var hireHeader = UIFactory.CreateLabel(contentGo.transform, "HireHeader",
                "Hire General  (10 Coins)", 15, FontStyles.Bold, font);
            hireHeader.color = UIColors.ACCENT_ORANGE;
            hireHeader.alignment = TextAlignmentOptions.Center;

            UIFactory.CreateButton(contentGo.transform, "Hire General (10 Coins)", font,
                new Color(0.5f, 0.35f, 0.15f, 0.9f), () => panel.HireGeneral(),
                new Vector2(0f, 42f), 14f);

            // Spacer
            CreateSpacer(contentGo.transform, 6f);

            // Feedback text
            var feedbackText = UIFactory.CreateLabel(contentGo.transform, "Feedback",
                "", 14, FontStyles.Bold, font);
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.gameObject.SetActive(false);

            UIFactory.SetField(panel, "_panelRoot", panelGo);
            UIFactory.SetField(panel, "_inventoryText", inventoryText);
            UIFactory.SetField(panel, "_feedbackText", feedbackText);

            panelGo.SetActive(false);
            return panel;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);
            spacer.AddComponent<RectTransform>();
            var le = spacer.AddComponent<LayoutElement>();
            le.preferredHeight = height;
        }
    }
}
