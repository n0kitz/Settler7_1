using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Tavern panel: beer→coins, coins→tools, hire general.
    /// Toggle with V key. Auto-refreshes inventory on show.
    /// </summary>
    public class TavernUI : MonoBehaviour
    {
        private TextMeshProUGUI _beerText;
        private TextMeshProUGUI _coinsText;
        private TextMeshProUGUI _toolsText;
        private TextMeshProUGUI _generalsText;
        private TextMeshProUGUI _feedbackText;
        private TMP_FontAsset _font;

        public bool IsVisible { get; private set; }

        private float _feedbackTimer;
        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.5f;

        // ---- Lifecycle --------------------------------------------------

        private void Update()
        {
            if (!IsVisible) return;
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = REFRESH_INTERVAL;
                RefreshInventory();
            }
            if (_feedbackTimer > 0f)
            {
                _feedbackTimer -= Time.deltaTime;
                if (_feedbackTimer <= 0f && _feedbackText != null)
                    _feedbackText.text = string.Empty;
            }
        }

        // ---- Public API -------------------------------------------------

        public void Show()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            _refreshTimer = 0f;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            IsVisible = false;
        }

        public void Toggle() { if (IsVisible) Hide(); else Show(); }

        /// <summary>Factory entry point called by BootstrapScene.UI.</summary>
        public static TavernUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var go = new GameObject("TavernUI");
            go.transform.SetParent(canvasTransform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.25f, 0.2f);
            rect.anchorMax = new Vector2(0.75f, 0.8f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.05f, 0.02f, 0.96f);

            var ui = go.AddComponent<TavernUI>();
            ui._font = font;
            ui.BuildLayout(go.transform);

            go.SetActive(false);
            return ui;
        }

        // ---- Layout -----------------------------------------------------

        private void BuildLayout(Transform root)
        {
            // Title
            var title = UIFactory.CreateLabel(root, "Title", "Tavern  [V]",
                22, FontStyles.Bold, _font);
            title.color = UIColors.TEXT_HEADER_GOLD;
            title.alignment = TextAlignmentOptions.Center;
            PositionRect(title.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(0f, 30f));

            // Inventory row
            var invRoot = CreateHBox(root, "Inventory", new Vector2(10f, -46f), new Vector2(-10f, -66f));
            _beerText = AddInvLabel(invRoot.transform, "BeerLbl", "Beer: 0");
            _coinsText = AddInvLabel(invRoot.transform, "CoinsLbl", "Coins: 0");
            _toolsText = AddInvLabel(invRoot.transform, "ToolsLbl", "Tools: 0");
            _generalsText = AddInvLabel(invRoot.transform, "GenLbl", "Generals: 0/5");

            // Separator
            var sep = UIFactory.CreateLabel(root, "Sep", "─────── Exchanges ───────",
                11, FontStyles.Normal, _font);
            sep.color = UIColors.TEXT_GRAY_DIM;
            sep.alignment = TextAlignmentOptions.Center;
            PositionRect(sep.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -74f), new Vector2(0f, 18f));

            // Beer→Coins section
            AddSectionLabel(root, "BeerHeader", "Beer  →  Coins  (1 Beer = 3 Coins)", -96f);
            var beerBtns = CreateHBox(root, "BeerBtns", new Vector2(20f, -118f), new Vector2(-20f, -86f));
            AddBtn(beerBtns.transform, "×1 Beer", () => ExchangeBeer(1));
            AddBtn(beerBtns.transform, "×5 Beer", () => ExchangeBeer(5));

            // Coins→Tools section
            AddSectionLabel(root, "ToolsHeader", "Coins  →  Tools  (5 Coins = 1 Tool)", -136f);
            var toolBtns = CreateHBox(root, "ToolBtns", new Vector2(20f, -158f), new Vector2(-20f, -126f));
            AddBtn(toolBtns.transform, "×1 Tool", () => BuyTools(1));
            AddBtn(toolBtns.transform, "×3 Tools", () => BuyTools(3));

            // Hire General section
            AddSectionLabel(root, "GenHeader", "Hire General  (10 Coins)", -176f);
            var genBtn = UIFactory.CreateButton(root, "Hire General", _font,
                UIColors.BUTTON_BLUE, HireGeneral, new Vector2(160f, 34f), 13f);
            var genBtnRect = genBtn.GetComponent<RectTransform>();
            genBtnRect.anchorMin = new Vector2(0.5f, 1f);
            genBtnRect.anchorMax = new Vector2(0.5f, 1f);
            genBtnRect.pivot = new Vector2(0.5f, 1f);
            genBtnRect.anchoredPosition = new Vector2(0f, -198f);

            // Feedback text
            _feedbackText = UIFactory.CreateLabel(root, "Feedback", string.Empty,
                12, FontStyles.Bold, _font);
            _feedbackText.color = UIColors.ACCENT_ORANGE;
            _feedbackText.alignment = TextAlignmentOptions.Center;
            PositionRect(_feedbackText.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -238f), new Vector2(0f, 22f));
        }

        // ---- Button actions ---------------------------------------------

        private void ExchangeBeer(int amount)
        {
            var state = Presentation.GameController.Instance?.State;
            if (state == null) return;
            int gained = state.Tavern.ExchangeBeerForCoins(0, amount);
            ShowFeedback(gained > 0
                ? $"Exchanged {amount} Beer → {gained} Coins."
                : "Not enough Beer.");
        }

        private void BuyTools(int amount)
        {
            var state = Presentation.GameController.Instance?.State;
            if (state == null) return;
            int gained = state.Tavern.ExchangeCoinsForTools(0, amount);
            ShowFeedback(gained > 0
                ? $"Bought {gained} Tool(s)."
                : $"Need {amount * 5} Coins.");
        }

        private void HireGeneral()
        {
            var state = Presentation.GameController.Instance?.State;
            if (state == null) return;
            int sectorId = FindOwnedSector(state);
            if (sectorId < 0) { ShowFeedback("No owned sector."); return; }
            var gen = state.Tavern.HireGeneral(0, sectorId);
            ShowFeedback(gen != null
                ? $"General #{gen.Id} hired in sector {sectorId}."
                : "Cannot hire: need 10 Coins or max generals reached.");
        }

        // ---- Refresh ----------------------------------------------------

        private void RefreshInventory()
        {
            var state = Presentation.GameController.Instance?.State;
            if (state == null) return;
            var res = state.PlayerResources.TryGetValue(0, out var r) ? r : null;
            if (res == null) return;

            if (_beerText != null) _beerText.text = $"Beer: {res.Get(ResourceType.Beer)}";
            if (_coinsText != null) _coinsText.text = $"Coins: {res.Get(ResourceType.Coins)}";
            if (_toolsText != null) _toolsText.text = $"Tools: {res.Get(ResourceType.Tools)}";
            if (_generalsText != null)
            {
                int gens = state.Army.GetGenerals(0).Count;
                _generalsText.text = $"Generals: {gens}/5";
            }
        }

        // ---- UI helpers -------------------------------------------------

        private void ShowFeedback(string msg)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = msg;
                _feedbackText.color = msg.Contains("Not enough") || msg.Contains("Cannot") || msg.Contains("Need")
                    ? UIColors.ACCENT_ORANGE : UIColors.TEXT_GOLD;
            }
            _feedbackTimer = 3f;
            RefreshInventory();
        }

        private TextMeshProUGUI AddInvLabel(Transform parent, string name, string text)
        {
            var lbl = UIFactory.CreateLabel(parent, name, text, 13, FontStyles.Normal, _font);
            lbl.color = UIColors.TEXT_GOLD;
            var le = lbl.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            return lbl;
        }

        private void AddSectionLabel(Transform root, string name, string text, float yOffset)
        {
            var lbl = UIFactory.CreateLabel(root, name, text, 12, FontStyles.Bold, _font);
            lbl.color = UIColors.TEXT_HEADER_GOLD;
            lbl.alignment = TextAlignmentOptions.Center;
            PositionRect(lbl.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, yOffset), new Vector2(0f, 18f));
        }

        private void AddBtn(Transform parent, string label, System.Action action)
        {
            var btn = UIFactory.CreateButton(parent, label, _font,
                UIColors.BUTTON_BLUE, action, new Vector2(0f, 30f), 12f);
            var le = btn.GetComponent<LayoutElement>();
            if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 30f;
            le.flexibleWidth = 1f;
        }

        private GameObject CreateHBox(Transform root, string name,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(offsetMin.x, offsetMax.y);
            rect.offsetMax = new Vector2(offsetMax.x, offsetMin.y);
            var hbox = go.AddComponent<HorizontalLayoutGroup>();
            hbox.spacing = 8f;
            hbox.childForceExpandWidth = true;
            hbox.childForceExpandHeight = true;
            hbox.padding = new RectOffset(4, 4, 0, 0);
            return go;
        }

        private static void PositionRect(RectTransform rt,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        private static int FindOwnedSector(GameState state)
        {
            foreach (var s in state.Graph.AllSectors)
                if (s.OwnerId == 0) return s.Id;
            return -1;
        }
    }
}
