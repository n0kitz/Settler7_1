using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    public partial class GameSetupUI
    {
        private readonly System.Collections.Generic.List<(TextMeshProUGUI label, string key)>
            _localeLabels = new();

        private void RegisterLocaleLabel(TextMeshProUGUI label, string key)
            => _localeLabels.Add((label, key));

        /// <summary>Re-resolves all baked strings after a locale switch (called on Show).</summary>
        private void RefreshLocaleTexts()
        {
            foreach (var (label, key) in _localeLabels)
                if (label != null) label.text = L.Get(key);
            if (_mapNameText != null && !string.IsNullOrEmpty(_mapId))
                _mapNameText.text = LocalizedNames.Map(_mapId, _mapDisplayFallback);
        }

        private static TextMeshProUGUI CreateSettingRow(Transform parent, GameSetupUI ui,
            string labelKey, TMP_FontAsset font, UnityEngine.Events.UnityAction onMinus,
            UnityEngine.Events.UnityAction onPlus)
        {
            string label = L.Get(labelKey);
            var rowGo = new GameObject($"Row_{labelKey.Replace("ui.setup.", "")}");
            rowGo.transform.SetParent(parent, false);

            var rowRect = rowGo.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 40f);

            var rowLayout = rowGo.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 40f;

            var hLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 8f;
            hLayout.childForceExpandHeight = true;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childForceExpandWidth = false;

            // Label
            var labelTmp = UIFactory.CreateLabel(rowGo.transform, "Label",
                label, 16, FontStyles.Normal, font);
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            var labelLE = labelTmp.gameObject.AddComponent<LayoutElement>();
            labelLE.preferredWidth = 160f;
            labelLE.flexibleWidth = 1f;
            ui.RegisterLocaleLabel(labelTmp, labelKey);

            // Minus button
            CreateSmallButton(rowGo.transform, "-", font, onMinus);

            // Value text
            var valueTmp = UIFactory.CreateLabel(rowGo.transform, "Value",
                "1", 18, FontStyles.Bold, font);
            valueTmp.alignment = TextAlignmentOptions.Center;
            var valueLE = valueTmp.gameObject.AddComponent<LayoutElement>();
            valueLE.preferredWidth = 50f;

            // Plus button
            CreateSmallButton(rowGo.transform, "+", font, onPlus);

            return valueTmp;
        }

        private static void CreateSmallButton(Transform parent, string label,
            TMP_FontAsset font, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject($"Btn_{label}");
            btnGo.transform.SetParent(parent, false);

            var le = btnGo.AddComponent<LayoutElement>();
            le.preferredWidth = 36f;
            le.preferredHeight = 36f;

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.35f, 0.9f);

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.45f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.25f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            var text = UIFactory.CreateLabel(btnGo.transform, "Label",
                label, 20, FontStyles.Bold, font);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateButton(Transform parent, GameSetupUI ui, string labelKey,
            TMP_FontAsset font, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIFactory.CreateButton(parent, L.Get(labelKey), font, bgColor, onClick);
            ui.RegisterLocaleLabel(btn.GetComponentInChildren<TextMeshProUGUI>(), labelKey);
        }
    }
}
