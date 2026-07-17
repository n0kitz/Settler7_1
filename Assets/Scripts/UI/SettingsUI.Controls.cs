using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    public partial class SettingsUI
    {
        private KeyBindings _keyBindings;

        private static readonly string[] REBINDABLE_ACTIONS =
        {
            "toggle_quest",
            "toggle_tech",
            "toggle_trade",
            "toggle_army",
            "toggle_tavern",
            "toggle_prestige",
            "toggle_diplomacy",
            "toggle_achievements",
        };

        /// <summary>Builds the Controls section with one row per rebindable action.</summary>
        private static void CreateControlsSection(Transform container,
            TMP_FontAsset font, SettingsUI ui)
        {
            CreateSectionHeader(container, "ui.settings.controls", font, ui);

            foreach (var action in REBINDABLE_ACTIONS)
            {
                string labelKey = "ui.settings.controls." + action;
                var row = new GameObject($"CtrlRow_{action}");
                row.transform.SetParent(container, false);
                row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 32f);
                var layout = row.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 6f;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.padding = new RectOffset(0, 0, 2, 2);
                var le = row.AddComponent<LayoutElement>();
                le.preferredHeight = 32f;

                var lbl = UIFactory.CreateLabel(row.transform, "Lbl",
                    L.Get(labelKey), 14f, font);
                var lblLe = lbl.gameObject.AddComponent<LayoutElement>();
                lblLe.preferredWidth = 130f;
                lblLe.preferredHeight = 28f;
                lbl.color = UIColors.TEXT_LIGHT;
                ui._chromeLabels.Add((lbl, labelKey));

                var boundKey = ui._keyBindings?.Get(action) ?? "?";
                var keyLbl = UIFactory.CreateLabel(row.transform, "Key", boundKey, 14f, font);
                keyLbl.alignment = TextAlignmentOptions.Center;
                var keyLe = keyLbl.gameObject.AddComponent<LayoutElement>();
                keyLe.preferredWidth = 60f;
                keyLe.preferredHeight = 28f;

                var capturedAction = action;
                var resetBtn = UIFactory.CreateButton(row.transform,
                    L.Get("ui.settings.controls.reset"), font,
                    new Color(0.3f, 0.3f, 0.35f),
                    () => ui.ResetKeyBind(capturedAction, keyLbl),
                    new Vector2(55f, 28f), 12f);
                ui._chromeLabels.Add((resetBtn.GetComponentInChildren<TextMeshProUGUI>(),
                    "ui.settings.controls.reset"));
            }
        }

        private void ResetKeyBind(string action, TextMeshProUGUI label)
        {
            if (_keyBindings == null) return;
            _keyBindings.ResetAction(action);
            label.text = _keyBindings.Get(action) ?? "?";
        }

        private void SaveKeyBindings()
        {
            if (_keyBindings != null)
                KeyBindingsPersistence.Save(_keyBindings);
        }

        private void LoadKeyBindings()
        {
            _keyBindings = KeyBindingsPersistence.Load();
        }
    }
}
