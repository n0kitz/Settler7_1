using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// BELOHNUNGEN modal (§14.3, Critical Rule #10): appears when the player
    /// has an unresolved conquest reward and offers exactly one choice of
    /// four packages. Never auto-grants.
    /// </summary>
    public class RewardModalUI : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Transform _packageContainer;
        private TMP_FontAsset _font;
        private PendingConquestReward _shown;
        private float _pollTimer;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        private void Update()
        {
            _pollTimer -= Time.deltaTime;
            if (_pollTimer > 0f) return;
            _pollTimer = 0.3f;

            var rewards = Presentation.GameController.Instance?.State?.ConquestRewards;
            if (rewards == null) return;

            var pending = rewards.GetPendingFor(0);
            if (pending != null && !IsVisible)
                Show(pending);
            else if (pending == null && IsVisible)
                Hide();
        }

        private void Show(PendingConquestReward pending)
        {
            _shown = pending;
            RebuildPackages(pending);
            _panelRoot.SetActive(true);
        }

        private void Hide()
        {
            _shown = null;
            _panelRoot.SetActive(false);
        }

        private void RebuildPackages(PendingConquestReward pending)
        {
            for (int i = _packageContainer.childCount - 1; i >= 0; i--)
                Destroy(_packageContainer.GetChild(i).gameObject);

            for (int i = 0; i < pending.Packages.Length; i++)
            {
                int index = i;
                var package = pending.Packages[i];
                string label = $"{L.Get(package.TitleKey)}\n{GoodsText(package)}";

                var btn = UIFactory.CreateButton(_packageContainer, label, _font,
                    UIColors.BUTTON_BLUE, () => OnPackageClicked(index),
                    new Vector2(0f, 52f), 13f);
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 52f;
            }
        }

        private static string GoodsText(ConquestRewardPackage package)
        {
            var parts = new string[package.Goods.Length];
            for (int i = 0; i < package.Goods.Length; i++)
                parts[i] = $"{package.Goods[i].amount}× " +
                    LocalizedNames.Resource(package.Goods[i].type);
            return string.Join("   ", parts);
        }

        private void OnPackageClicked(int index)
        {
            if (_shown == null) return;
            var rewards = Presentation.GameController.Instance?.State?.ConquestRewards;
            rewards?.ChooseReward(0, _shown.SectorId, index);
            Hide();
        }

        /// <summary>Create the modal programmatically (hidden by default).</summary>
        public static RewardModalUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            // Always-active wrapper holds the component so Update() keeps
            // polling for pending rewards while the visual panel is hidden
            var rootGo = new GameObject("RewardModal");
            rootGo.transform.SetParent(canvasTransform, false);
            var rootRect = rootGo.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(rootGo.transform, false);

            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(380f, 330f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = UIColors.PANEL_BLUE_DARK;

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 8f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                L.Get("ui.reward.title"), 20, FontStyles.Bold, font);
            title.color = UIColors.TEXT_HEADER_GOLD;
            title.alignment = TextAlignmentOptions.Center;

            var hint = UIFactory.CreateLabel(panelGo.transform, "Hint",
                L.Get("ui.reward.choose_hint"), 12, FontStyles.Normal, font);
            hint.color = UIColors.TEXT_LIGHT;
            hint.alignment = TextAlignmentOptions.Center;

            var containerGo = new GameObject("Packages");
            containerGo.transform.SetParent(panelGo.transform, false);
            containerGo.AddComponent<RectTransform>();
            var containerLayout = containerGo.AddComponent<VerticalLayoutGroup>();
            containerLayout.spacing = 6f;
            containerLayout.childForceExpandWidth = true;
            containerLayout.childForceExpandHeight = false;

            var modal = rootGo.AddComponent<RewardModalUI>();
            modal._panelRoot = panelGo;
            modal._packageContainer = containerGo.transform;
            modal._font = font;

            panelGo.SetActive(false);
            return modal;
        }
    }
}
