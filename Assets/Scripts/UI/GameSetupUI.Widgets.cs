using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settlers.UI
{
    public partial class GameSetupUI
    {
        private static TextMeshProUGUI CreateSettingRow(Transform parent, string label,
            TMP_FontAsset font, UnityEngine.Events.UnityAction onMinus,
            UnityEngine.Events.UnityAction onPlus)
        {
            var rowGo = new GameObject($"Row_{label.Replace(" ", "")}");
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

        private static void CreateButton(Transform parent, string label,
            TMP_FontAsset font, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            UIFactory.CreateButton(parent, label, font, bgColor, onClick);
        }
    }
}
