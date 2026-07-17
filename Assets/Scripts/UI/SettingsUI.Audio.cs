using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    public partial class SettingsUI
    {
        /// <summary>
        /// Builds the Audio section rows inside the given container.
        /// Returns the value labels so the caller can wire them to serialized fields.
        /// </summary>
        private static (TextMeshProUGUI musicPct, TextMeshProUGUI sfxPct,
                        TextMeshProUGUI muteLabel)
            CreateAudioSection(Transform container, TMP_FontAsset font, SettingsUI ui)
        {
            CreateSectionHeader(container, "ui.settings.audio", font, ui);

            var (_, musicPct) = CreateRowWithValue(container, "ui.settings.music_volume", font,
                ui, ui.OnMusicDown, ui.OnMusicUp, "30%");

            var (_, sfxPct) = CreateRowWithValue(container, "ui.settings.sfx_volume", font,
                ui, ui.OnSfxDown, ui.OnSfxUp, "60%");

            // Mute toggle row
            var muteRow = CreateRow(container, "ui.settings.mute_all", font, ui);
            var muteLabel = UIFactory.CreateLabel(muteRow, "MuteVal", "OFF", 16f, font);
            var muteLabelRect = muteLabel.GetComponent<RectTransform>();
            muteLabelRect.sizeDelta = new Vector2(50f, 30f);

            var muteBtn = CreateToggleButton(muteRow, font, ui, ui.OnToggleMute);
            muteBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(70f, 30f);

            return (musicPct, sfxPct, muteLabel);
        }

        /// <summary>The shared "Toggle" button used by mute/fullscreen/color-blind rows.</summary>
        private static Button CreateToggleButton(Transform row, TMP_FontAsset font,
            SettingsUI ui, UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIFactory.CreateButton(row, L.Get("ui.settings.toggle"), font,
                new Color(0.35f, 0.35f, 0.4f), onClick, new Vector2(70f, 30f), 14f);
            ui._chromeLabels.Add(
                (btn.GetComponentInChildren<TextMeshProUGUI>(), "ui.settings.toggle"));
            return btn;
        }

        private static Transform CreateRow(Transform parent, string labelKey,
            TMP_FontAsset font, SettingsUI ui)
        {
            var row = new GameObject($"Row_{labelKey.Substring(labelKey.LastIndexOf('.') + 1)}");
            row.transform.SetParent(parent, false);
            var rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 36f);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.padding = new RectOffset(0, 0, 2, 2);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 36f;

            var lbl = UIFactory.CreateLabel(row.transform, "Lbl", L.Get(labelKey), 16f, font);
            var lblRect = lbl.GetComponent<RectTransform>();
            lblRect.sizeDelta = new Vector2(150f, 30f);
            var lblLe = lbl.gameObject.AddComponent<LayoutElement>();
            lblLe.preferredWidth = 150f;
            lblLe.preferredHeight = 30f;
            lbl.color = UIColors.TEXT_LIGHT;
            ui._chromeLabels.Add((lbl, labelKey));

            return row.transform;
        }

        private static (Transform row, TextMeshProUGUI valLabel)
            CreateRowWithValue(Transform parent, string labelKey, TMP_FontAsset font,
                SettingsUI ui,
                UnityEngine.Events.UnityAction onDown,
                UnityEngine.Events.UnityAction onUp,
                string initial)
        {
            var row = CreateRow(parent, labelKey, font, ui);

            UIFactory.CreateButton(row, "-", font,
                new Color(0.3f, 0.3f, 0.35f), onDown, new Vector2(30f, 30f), 16f);

            var valLabel = UIFactory.CreateLabel(row, "Val", initial, 16f, font);
            var valRect = valLabel.GetComponent<RectTransform>();
            valRect.sizeDelta = new Vector2(50f, 30f);
            valLabel.alignment = TextAlignmentOptions.Center;
            var valLe = valLabel.gameObject.AddComponent<LayoutElement>();
            valLe.preferredWidth = 50f;
            valLe.preferredHeight = 30f;

            UIFactory.CreateButton(row, "+", font,
                new Color(0.3f, 0.3f, 0.35f), onUp, new Vector2(30f, 30f), 16f);

            return (row, valLabel);
        }

        private static void CreateSectionHeader(Transform parent, string titleKey,
            TMP_FontAsset font, SettingsUI ui)
        {
            var hdr = UIFactory.CreateLabel(parent,
                $"Hdr_{titleKey.Substring(titleKey.LastIndexOf('.') + 1)}",
                L.Get(titleKey), 15f, TMPro.FontStyles.Bold, font);
            hdr.color = UIColors.TEXT_HEADER_GOLD;
            var le = hdr.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 28f;
            ui._chromeLabels.Add((hdr, titleKey));
        }
    }
}
