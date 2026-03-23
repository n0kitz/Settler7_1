using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Tavern exchange panel: beer→coins, coins→tools, hire general.
    /// Toggle with V key.
    /// </summary>
    public class TavernUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _beerText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _toolsText;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        private bool _isVisible;
        private float _feedbackTimer;

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
            if (!_isVisible) return;
            Refresh();

            if (_feedbackTimer > 0f)
            {
                _feedbackTimer -= Time.deltaTime;
                if (_feedbackTimer <= 0f && _feedbackText != null)
                    _feedbackText.text = "";
            }
        }

        private void Refresh()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            var res = gc.GetPlayerResources(0);
            if (res == null) return;

            if (_beerText != null)
                _beerText.text = $"Beer: {res.Get(ResourceType.Beer)}  (1 Beer → 3 Coins)";
            if (_coinsText != null)
                _coinsText.text = $"Coins: {res.Get(ResourceType.Coins)}  (5 Coins → 1 Tool)";
            if (_toolsText != null)
                _toolsText.text = $"Tools: {res.Get(ResourceType.Tools)}";
        }

        private void ShowFeedback(string msg)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = msg;
                _feedbackTimer = 3f;
            }
        }

        private void OnBeerToCoins()
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State == null) return;
            int coins = gc.State.Tavern.ExchangeBeerForCoins(0, 1);
            ShowFeedback(coins > 0 ? $"+{coins} Coins" : "No beer available!");
        }

        private void OnBeerToCoins5()
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State == null) return;
            int coins = gc.State.Tavern.ExchangeBeerForCoins(0, 5);
            ShowFeedback(coins > 0 ? $"+{coins} Coins" : "Not enough beer!");
        }

        private void OnCoinsToTools()
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State == null) return;
            int tools = gc.State.Tavern.ExchangeCoinsForTools(0, 1);
            ShowFeedback(tools > 0 ? "+1 Tool" : "Need 5 coins!");
        }

        private void OnHireGeneral()
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State == null) return;
            var sectors = gc.State.Graph.GetSectorsOwnedBy(0);
            if (sectors.Count == 0) return;
            var gen = gc.State.Tavern.HireGeneral(0, sectors[0]);
            ShowFeedback(gen != null ? $"General #{gen.Id} hired!" : "Cannot hire (need 10 coins or max generals)");
        }

        /// <summary>Create the tavern UI programmatically.</summary>
        public static TavernUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("TavernUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.25f, 0.2f);
            panelRect.anchorMax = new Vector2(0.75f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.08f, 0.05f, 0.95f);

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 12);
            layout.spacing = 8f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            // Title
            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Tavern", 22, FontStyles.Bold, font);
            title.alignment = TextAlignmentOptions.Center;
            title.color = new Color(0.9f, 0.8f, 0.5f);
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);

            // Beer line
            var beerText = UIFactory.CreateLabel(panelGo.transform, "BeerText",
                "Beer: 0", 14, font);
            beerText.color = new Color(0.9f, 0.8f, 0.4f);

            // Beer→Coins buttons
            var beerBtns = CreateButtonRow(panelGo.transform, font);
            var btn1 = CreateActionButton(beerBtns.transform, "1 Beer → 3 Coins", font);
            var btn5 = CreateActionButton(beerBtns.transform, "5 Beer → 15 Coins", font);

            // Coins line
            var coinsText = UIFactory.CreateLabel(panelGo.transform, "CoinsText",
                "Coins: 0", 14, font);
            coinsText.color = new Color(0.9f, 0.85f, 0.3f);

            // Coins→Tools button
            var toolBtns = CreateButtonRow(panelGo.transform, font);
            var btnTool = CreateActionButton(toolBtns.transform, "5 Coins → 1 Tool", font);

            // Tools line
            var toolsText = UIFactory.CreateLabel(panelGo.transform, "ToolsText",
                "Tools: 0", 14, font);
            toolsText.color = new Color(0.7f, 0.7f, 0.9f);

            // Separator
            UIFactory.CreateLabel(panelGo.transform, "Sep", "--- Military ---", 12, font)
                .color = new Color(0.5f, 0.5f, 0.5f);

            // Hire General button
            var genBtns = CreateButtonRow(panelGo.transform, font);
            var btnGen = CreateActionButton(genBtns.transform, "Hire General (10 Coins)", font);

            // Feedback
            var feedbackText = UIFactory.CreateLabel(panelGo.transform, "Feedback",
                "", 13, FontStyles.Italic, font);
            feedbackText.color = new Color(0.4f, 0.9f, 0.4f);

            // Component
            var ui = panelGo.AddComponent<TavernUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);
            UIFactory.SetField(ui, "_beerText", beerText);
            UIFactory.SetField(ui, "_coinsText", coinsText);
            UIFactory.SetField(ui, "_toolsText", toolsText);
            UIFactory.SetField(ui, "_feedbackText", feedbackText);

            // Wire buttons
            btn1.GetComponent<Button>().onClick.AddListener(() => ui.OnBeerToCoins());
            btn5.GetComponent<Button>().onClick.AddListener(() => ui.OnBeerToCoins5());
            btnTool.GetComponent<Button>().onClick.AddListener(() => ui.OnCoinsToTools());
            btnGen.GetComponent<Button>().onClick.AddListener(() => ui.OnHireGeneral());

            panelGo.SetActive(false);
            return ui;
        }

        private static GameObject CreateButtonRow(Transform parent, TMP_FontAsset font)
        {
            var row = new GameObject("ButtonRow");
            row.transform.SetParent(parent, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 32f);
            row.AddComponent<LayoutElement>().preferredHeight = 32f;
            var hl = row.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 8f;
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = true;
            return row;
        }

        private static GameObject CreateActionButton(Transform parent, string label,
            TMP_FontAsset font)
        {
            var btnGo = new GameObject($"Btn_{label}");
            btnGo.transform.SetParent(parent, false);
            btnGo.AddComponent<RectTransform>();

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.2f, 0.25f, 0.3f, 0.9f);

            btnGo.AddComponent<Button>();

            var text = UIFactory.CreateLabel(btnGo.transform, "Label", label, 12, font);
            text.alignment = TextAlignmentOptions.Center;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btnGo;
        }
    }
}
