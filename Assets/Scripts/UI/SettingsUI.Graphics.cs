using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settlers.UI
{
    public partial class SettingsUI
    {
        /// <summary>
        /// Builds the Graphics section rows inside the given container.
        /// Returns value labels for quality and fullscreen toggle.
        /// </summary>
        private static (TextMeshProUGUI qualityLabel, TextMeshProUGUI fullscreenLabel)
            CreateGraphicsSection(Transform container, TMP_FontAsset font, SettingsUI ui)
        {
            CreateSectionHeader(container, "Graphics", font);

            var (_, qualityLabel) = CreateRowWithValue(container, "Quality", font,
                ui.OnQualityPrev, ui.OnQualityNext, "High");

            // Fullscreen toggle row
            var fsRow = CreateRow(container, "Fullscreen", font);
            var fsLabel = UIFactory.CreateLabel(fsRow, "FsVal", "OFF", 16f, font);
            var fsRect = fsLabel.GetComponent<RectTransform>();
            fsRect.sizeDelta = new Vector2(50f, 30f);
            var fsLe = fsLabel.gameObject.AddComponent<LayoutElement>();
            fsLe.preferredWidth = 50f;
            fsLe.preferredHeight = 30f;

            UIFactory.CreateButton(fsRow, "Toggle", font,
                new Color(0.35f, 0.35f, 0.4f), ui.OnToggleFullscreen,
                new Vector2(70f, 30f), 14f);

            return (qualityLabel, fsLabel);
        }

        /// <summary>Build and return the SettingsUI MonoBehaviour.</summary>
        public static SettingsUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("SettingsUI");
            panelGo.transform.SetParent(canvasTransform, false);

            // Full-screen dim overlay
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var overlay = panelGo.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.65f);

            // Center card
            var card = new GameObject("Card");
            card.transform.SetParent(panelGo.transform, false);
            var cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.3f, 0.15f);
            cardRect.anchorMax = new Vector2(0.7f, 0.88f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            var cardBg = card.AddComponent<Image>();
            cardBg.color = UIColors.PANEL_BLUE_DARK;

            // Title
            var title = UIFactory.CreateLabel(card.transform, "Title", "Settings",
                24f, FontStyles.Bold, font);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -16f);
            titleRect.sizeDelta = new Vector2(0f, 34f);
            title.alignment = TextAlignmentOptions.Center;
            title.color = UIColors.TEXT_HEADER_GOLD;

            // Scrollable content area
            var content = new GameObject("Content");
            content.transform.SetParent(card.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.04f, 0.15f);
            contentRect.anchorMax = new Vector2(0.96f, 0.88f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.padding = new RectOffset(4, 4, 4, 4);
            content.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var ui = panelGo.AddComponent<SettingsUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);

            var (musicPct, sfxPct, muteLabel) = CreateAudioSection(content.transform, font, ui);
            var (qualLabel, fsLabel)           = CreateGraphicsSection(content.transform, font, ui);
            var (langLabel, cbLabel)           = CreateLanguageSection(content.transform, font, ui);
            CreateControlsSection(content.transform, font, ui);

            UIFactory.SetField(ui, "_musicPct",        musicPct);
            UIFactory.SetField(ui, "_sfxPct",          sfxPct);
            UIFactory.SetField(ui, "_muteLabel",       muteLabel);
            UIFactory.SetField(ui, "_qualityLabel",    qualLabel);
            UIFactory.SetField(ui, "_fullscreenLabel", fsLabel);
            UIFactory.SetField(ui, "_languageLabel",   langLabel);
            UIFactory.SetField(ui, "_colorBlindLabel", cbLabel);

            // Bottom buttons
            var btnRow = new GameObject("BtnRow");
            btnRow.transform.SetParent(card.transform, false);
            var btnRect = btnRow.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.05f, 0.02f);
            btnRect.anchorMax = new Vector2(0.95f, 0.13f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            var btnLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 12f;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = true;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;

            UIFactory.CreateButton(btnRow.transform, "Apply & Close", font,
                UIColors.BUTTON_GREEN, ui.OnApply);
            UIFactory.CreateButton(btnRow.transform, "Cancel", font,
                UIColors.BUTTON_RED, ui.OnClose);

            panelGo.SetActive(false);
            return ui;
        }
    }
}
