using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    public partial class SettingsUI
    {
        private KeyBindings _keyBindings;

        private static readonly (string action, string label)[] REBINDABLE_ACTIONS =
        {
            ("toggle_quest",        "Quests"),
            ("toggle_tech",         "Technology"),
            ("toggle_trade",        "Trade Map"),
            ("toggle_army",         "Army"),
            ("toggle_tavern",       "Tavern"),
            ("toggle_prestige",     "Prestige"),
            ("toggle_diplomacy",    "Diplomacy"),
            ("toggle_achievements", "Achievements"),
        };

        /// <summary>Builds the Controls section with one row per rebindable action.</summary>
        private static void CreateControlsSection(Transform container,
            TMP_FontAsset font, SettingsUI ui)
        {
            CreateSectionHeader(container, "Controls", font);

            foreach (var (action, displayLabel) in REBINDABLE_ACTIONS)
            {
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

                var lbl = UIFactory.CreateLabel(row.transform, "Lbl", displayLabel, 14f, font);
                var lblLe = lbl.gameObject.AddComponent<LayoutElement>();
                lblLe.preferredWidth = 130f;
                lblLe.preferredHeight = 28f;
                lbl.color = UIColors.TEXT_LIGHT;

                var boundKey = ui._keyBindings?.Get(action) ?? "?";
                var keyLbl = UIFactory.CreateLabel(row.transform, "Key", boundKey, 14f, font);
                keyLbl.alignment = TextAlignmentOptions.Center;
                var keyLe = keyLbl.gameObject.AddComponent<LayoutElement>();
                keyLe.preferredWidth = 60f;
                keyLe.preferredHeight = 28f;

                var capturedAction = action;
                UIFactory.CreateButton(row.transform, "Reset", font,
                    new Color(0.3f, 0.3f, 0.35f),
                    () => ui.ResetKeyBind(capturedAction, keyLbl),
                    new Vector2(55f, 28f), 12f);
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
