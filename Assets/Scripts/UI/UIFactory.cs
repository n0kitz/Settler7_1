using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settlers.UI
{
    /// <summary>
    /// Shared UI construction helpers used by all programmatic UI factories.
    /// </summary>
    public static class UIFactory
    {
        private static TMP_FontAsset _cachedFont;

        /// <summary>
        /// Returns a usable TMP font asset. Caches the result.
        /// Tries Resources load first, then TMP_Settings default.
        /// </summary>
        public static TMP_FontAsset GetDefaultFont()
        {
            if (_cachedFont != null) return _cachedFont;
            _cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (_cachedFont == null)
                _cachedFont = TMP_Settings.defaultFontAsset;
            return _cachedFont;
        }

        public static TextMeshProUGUI CreateLabel(Transform parent, string name,
            string text, float fontSize, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, fontSize + 6f);

            var resolvedFont = font != null ? font : GetDefaultFont();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;
            if (resolvedFont != null) tmp.font = resolvedFont;

            return tmp;
        }

        public static TextMeshProUGUI CreateLabel(Transform parent, string name,
            string text, float fontSize, TMP_FontAsset font)
        {
            return CreateLabel(parent, name, text, fontSize, FontStyles.Normal, font);
        }

        /// <summary>
        /// Creates a standard button with Image, Button (highlight ×1.2, pressed ×0.8),
        /// LayoutElement, and a centered bold label. Returns the Button component.
        /// </summary>
        public static Button CreateButton(Transform parent, string label, TMP_FontAsset font,
            Color bgColor, UnityEngine.Events.UnityAction onClick,
            Vector2? sizeDelta = null, float fontSize = 18f)
        {
            var size = sizeDelta ?? new Vector2(0f, 44f);

            var btnGo = new GameObject($"Btn_{label.Replace(" ", "")}");
            btnGo.transform.SetParent(parent, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            var le = btnGo.AddComponent<LayoutElement>();
            le.preferredHeight = size.y;
            if (size.x > 0f) le.preferredWidth = size.x;

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = bgColor;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(onClick);
            // Every factory button clicks audibly (ui_click stays silent if absent)
            btn.onClick.AddListener(() =>
                Presentation.AudioManager.Instance?.PlayUIClick());

            var text = CreateLabel(btnGo.transform, "Label", label, fontSize,
                FontStyles.Bold, font);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        public static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        /// <summary>
        /// Ornate panel: gold border frame with inset olive content area.
        /// Returns (frame root, content). Caller positions the frame rect
        /// and adds layout components to the content.
        /// </summary>
        public static (GameObject frame, GameObject content) CreateOrnatePanel(
            Transform parent, string name)
        {
            var frameGo = new GameObject(name);
            frameGo.transform.SetParent(parent, false);
            frameGo.AddComponent<RectTransform>();
            var frameBg = frameGo.AddComponent<Image>();
            frameBg.color = UIColors.BORDER_GOLD;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(frameGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(3f, 3f);
            contentRect.offsetMax = new Vector2(-3f, -3f);
            var contentBg = contentGo.AddComponent<Image>();
            contentBg.color = UIColors.PANEL_OLIVE;

            return (frameGo, contentGo);
        }
    }
}
