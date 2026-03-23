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

        public static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
