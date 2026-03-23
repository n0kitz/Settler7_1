using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;
using Settlers.Presentation;

namespace Settlers.UI
{
    /// <summary>
    /// Save/Load slot panel with 5 named slots. Can operate in Save or Load mode.
    /// Opened from PauseMenuUI (save/load) or MainMenuUI (load only).
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        public enum Mode { Save, Load }

        private const int SLOT_COUNT = 5;
        private const string METADATA_PREFIX = "#meta:";

        [SerializeField] private GameObject _panelRoot;

        private Mode _currentMode;
        private TextMeshProUGUI _titleLabel;
        private SlotEntry[] _slots;

        /// <summary>Fired after a successful load so callers can refresh visuals.</summary>
        public event Action OnLoadComplete;

        /// <summary>Fired when the panel is closed.</summary>
        public event Action OnClosed;

        public bool IsOpen => _panelRoot != null && _panelRoot.activeSelf;

        public void Show(Mode mode)
        {
            _currentMode = mode;
            if (_titleLabel != null)
                _titleLabel.text = mode == Mode.Save ? "Save Game" : "Load Game";
            RefreshSlots();
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
            OnClosed?.Invoke();
        }

        // --- Slot file paths ---

        private static string GetSlotPath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"save_slot_{slot + 1}.sav");
        }

        // --- Metadata ---

        private struct SlotMetadata
        {
            public bool Exists;
            public string MapId;
            public string SaveDate;
            public string PlayTime;
        }

        private static SlotMetadata ReadMetadata(int slot)
        {
            var path = GetSlotPath(slot);
            if (!File.Exists(path))
                return new SlotMetadata { Exists = false };

            var meta = new SlotMetadata { Exists = true, MapId = "Unknown", SaveDate = "Unknown", PlayTime = "0:00" };

            using var reader = new StreamReader(path);
            // Read first few lines looking for metadata comments
            for (int i = 0; i < 5; i++)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                if (!line.StartsWith(METADATA_PREFIX)) continue;

                var kv = line.Substring(METADATA_PREFIX.Length);
                int eq = kv.IndexOf('=');
                if (eq < 0) continue;
                string key = kv.Substring(0, eq);
                string val = kv.Substring(eq + 1);

                switch (key)
                {
                    case "map": meta.MapId = val; break;
                    case "date": meta.SaveDate = val; break;
                    case "playtime": meta.PlayTime = val; break;
                }
            }

            return meta;
        }

        private static void WriteMetadataHeader(StringBuilder sb, GameState state)
        {
            string mapId = state.MapId ?? "unknown";
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            int minutes = Mathf.FloorToInt(state.SimulationTime / 60f);
            int seconds = Mathf.FloorToInt(state.SimulationTime % 60f);
            string playTime = $"{minutes}:{seconds:D2}";

            sb.AppendLine($"{METADATA_PREFIX}map={mapId}");
            sb.AppendLine($"{METADATA_PREFIX}date={date}");
            sb.AppendLine($"{METADATA_PREFIX}playtime={playTime}");
        }

        /// <summary>Save game state to a slot, including metadata header.</summary>
        public static void SaveToSlot(GameState state, int slot)
        {
            var sb = new StringBuilder();
            WriteMetadataHeader(sb, state);
            sb.Append(SaveSystem.Serialize(state));
            File.WriteAllText(GetSlotPath(slot), sb.ToString());
            Debug.Log($"[Save] Saved to slot {slot + 1}: {GetSlotPath(slot)}");
        }

        /// <summary>Load game state from a slot.</summary>
        public static bool LoadFromSlot(GameState state, int slot)
        {
            var path = GetSlotPath(slot);
            if (!File.Exists(path)) return false;

            string data = File.ReadAllText(path);
            var parsed = SaveSystem.Deserialize(data);
            SaveSystem.ApplyToState(state, parsed);
            Debug.Log($"[Save] Loaded slot {slot + 1}");
            return true;
        }

        // --- Slot actions ---

        private void OnSlotSave(int slot)
        {
            var gc = GameController.Instance;
            if (gc == null || gc.State == null) return;
            SaveToSlot(gc.State, slot);
            RefreshSlots();
        }

        private void OnSlotLoad(int slot)
        {
            var gc = GameController.Instance;
            if (gc == null || gc.State == null) return;
            if (!LoadFromSlot(gc.State, slot)) return;
            gc.RefreshAllOwnership();
            Hide();
            OnLoadComplete?.Invoke();
        }

        private void OnSlotDelete(int slot)
        {
            var path = GetSlotPath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[Save] Deleted slot {slot + 1}");
            }
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            if (_slots == null) return;
            for (int i = 0; i < SLOT_COUNT; i++)
            {
                var meta = ReadMetadata(i);
                _slots[i].Refresh(meta, _currentMode);
            }
        }

        // --- Programmatic UI creation ---

        public static SaveSlotUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("SaveSlotUI");
            panelGo.transform.SetParent(canvasTransform, false);

            // Full-screen overlay
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0f, 0f, 0f, 0.7f);

            // Center box
            var boxGo = new GameObject("Box");
            boxGo.transform.SetParent(panelGo.transform, false);
            var boxRect = boxGo.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.2f, 0.1f);
            boxRect.anchorMax = new Vector2(0.8f, 0.9f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;

            var boxBg = boxGo.AddComponent<Image>();
            boxBg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

            // Title
            var titleText = UIFactory.CreateLabel(boxGo.transform, "Title",
                "Save Game", 28, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -16f);
            titleRect.sizeDelta = new Vector2(0f, 36f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.9f, 0.82f, 0.55f);

            // Slot container with vertical layout
            var slotContainer = new GameObject("Slots");
            slotContainer.transform.SetParent(boxGo.transform, false);
            var slotContainerRect = slotContainer.AddComponent<RectTransform>();
            slotContainerRect.anchorMin = new Vector2(0.05f, 0.12f);
            slotContainerRect.anchorMax = new Vector2(0.95f, 0.88f);
            slotContainerRect.offsetMin = Vector2.zero;
            slotContainerRect.offsetMax = Vector2.zero;

            var layout = slotContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.UpperCenter;

            // Create slot entries
            var ui = panelGo.AddComponent<SaveSlotUI>();
            UIFactory.SetField(ui, "_panelRoot", panelGo);
            ui._titleLabel = titleText;
            ui._slots = new SlotEntry[SLOT_COUNT];

            for (int i = 0; i < SLOT_COUNT; i++)
            {
                ui._slots[i] = SlotEntry.Create(slotContainer.transform, i, font, ui);
            }

            // Close button
            CreateCloseButton(boxGo.transform, font, ui);

            panelGo.SetActive(false);
            return ui;
        }

        private static void CreateCloseButton(Transform parent, TMP_FontAsset font, SaveSlotUI ui)
        {
            var btnGo = new GameObject("Btn_Close");
            btnGo.transform.SetParent(parent, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.35f, 0f);
            rect.anchorMax = new Vector2(0.65f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 12f);
            rect.sizeDelta = new Vector2(0f, 38f);

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = new Color(0.35f, 0.3f, 0.3f, 0.9f);

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.45f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.25f, 0.2f, 0.2f);
            btn.colors = colors;
            btn.onClick.AddListener(ui.Hide);

            var text = UIFactory.CreateLabel(btnGo.transform, "Label",
                "Close", 16, FontStyles.Bold, font);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;
        }

        // --- Individual slot row ---

        private class SlotEntry
        {
            public GameObject Root;
            public TextMeshProUGUI NameLabel;
            public TextMeshProUGUI InfoLabel;
            public Button ActionButton;
            public TextMeshProUGUI ActionLabel;
            public Button DeleteButton;
            public GameObject DeleteGo;
            public int SlotIndex;

            public void Refresh(SlotMetadata meta, Mode mode)
            {
                if (meta.Exists)
                {
                    NameLabel.text = $"Slot {SlotIndex + 1}";
                    InfoLabel.text = $"{meta.MapId}  |  {meta.SaveDate}  |  {meta.PlayTime}";
                    InfoLabel.color = new Color(0.7f, 0.7f, 0.65f);
                    ActionLabel.text = mode == Mode.Save ? "Overwrite" : "Load";
                    DeleteGo.SetActive(true);
                }
                else
                {
                    NameLabel.text = $"Slot {SlotIndex + 1}";
                    InfoLabel.text = "— Empty —";
                    InfoLabel.color = new Color(0.4f, 0.4f, 0.4f);
                    ActionLabel.text = mode == Mode.Save ? "Save" : "";
                    ActionButton.interactable = mode == Mode.Save;
                    DeleteGo.SetActive(false);
                }
            }

            public static SlotEntry Create(Transform parent, int index, TMP_FontAsset font, SaveSlotUI ui)
            {
                var entry = new SlotEntry { SlotIndex = index };

                var rowGo = new GameObject($"Slot_{index + 1}");
                rowGo.transform.SetParent(parent, false);
                entry.Root = rowGo;

                var rowRect = rowGo.AddComponent<RectTransform>();
                rowRect.sizeDelta = new Vector2(0f, 56f);

                var le = rowGo.AddComponent<LayoutElement>();
                le.preferredHeight = 56f;

                var rowBg = rowGo.AddComponent<Image>();
                rowBg.color = new Color(0.12f, 0.12f, 0.14f, 0.9f);

                // Slot name (left)
                var nameText = UIFactory.CreateLabel(rowGo.transform, "Name",
                    $"Slot {index + 1}", 18, FontStyles.Bold, font);
                var nameRect = nameText.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.5f);
                nameRect.anchorMax = new Vector2(0.2f, 0.5f);
                nameRect.pivot = new Vector2(0f, 0.5f);
                nameRect.anchoredPosition = new Vector2(12f, 0f);
                nameRect.sizeDelta = new Vector2(0f, 24f);
                nameText.alignment = TextAlignmentOptions.MidlineLeft;
                nameText.color = new Color(0.9f, 0.82f, 0.55f);
                entry.NameLabel = nameText;

                // Info text (center)
                var infoText = UIFactory.CreateLabel(rowGo.transform, "Info",
                    "— Empty —", 14, FontStyles.Normal, font);
                var infoRect = infoText.GetComponent<RectTransform>();
                infoRect.anchorMin = new Vector2(0.2f, 0.5f);
                infoRect.anchorMax = new Vector2(0.65f, 0.5f);
                infoRect.pivot = new Vector2(0f, 0.5f);
                infoRect.anchoredPosition = new Vector2(8f, 0f);
                infoRect.sizeDelta = new Vector2(0f, 20f);
                infoText.alignment = TextAlignmentOptions.MidlineLeft;
                infoText.color = new Color(0.4f, 0.4f, 0.4f);
                entry.InfoLabel = infoText;

                // Action button (Save/Load/Overwrite)
                var actionGo = new GameObject("Btn_Action");
                actionGo.transform.SetParent(rowGo.transform, false);
                var actionRect = actionGo.AddComponent<RectTransform>();
                actionRect.anchorMin = new Vector2(0.67f, 0.15f);
                actionRect.anchorMax = new Vector2(0.84f, 0.85f);
                actionRect.offsetMin = Vector2.zero;
                actionRect.offsetMax = Vector2.zero;

                var actionBg = actionGo.AddComponent<Image>();
                actionBg.color = new Color(0.2f, 0.4f, 0.5f, 0.9f);

                entry.ActionButton = actionGo.AddComponent<Button>();
                var ac = entry.ActionButton.colors;
                ac.highlightedColor = new Color(0.25f, 0.5f, 0.6f);
                ac.pressedColor = new Color(0.15f, 0.3f, 0.4f);
                ac.disabledColor = new Color(0.15f, 0.15f, 0.17f);
                entry.ActionButton.colors = ac;

                int capturedIndex = index;
                entry.ActionButton.onClick.AddListener(() =>
                {
                    if (ui._currentMode == Mode.Save)
                        ui.OnSlotSave(capturedIndex);
                    else
                        ui.OnSlotLoad(capturedIndex);
                });

                entry.ActionLabel = UIFactory.CreateLabel(actionGo.transform, "Label",
                    "Save", 14, FontStyles.Bold, font);
                var alRect = entry.ActionLabel.GetComponent<RectTransform>();
                alRect.anchorMin = Vector2.zero;
                alRect.anchorMax = Vector2.one;
                alRect.offsetMin = Vector2.zero;
                alRect.offsetMax = Vector2.zero;
                entry.ActionLabel.alignment = TextAlignmentOptions.Center;

                // Delete button
                var deleteGo = new GameObject("Btn_Delete");
                deleteGo.transform.SetParent(rowGo.transform, false);
                entry.DeleteGo = deleteGo;

                var delRect = deleteGo.AddComponent<RectTransform>();
                delRect.anchorMin = new Vector2(0.86f, 0.15f);
                delRect.anchorMax = new Vector2(0.97f, 0.85f);
                delRect.offsetMin = Vector2.zero;
                delRect.offsetMax = Vector2.zero;

                var delBg = deleteGo.AddComponent<Image>();
                delBg.color = new Color(0.5f, 0.2f, 0.2f, 0.9f);

                entry.DeleteButton = deleteGo.AddComponent<Button>();
                var dc = entry.DeleteButton.colors;
                dc.highlightedColor = new Color(0.6f, 0.25f, 0.25f);
                dc.pressedColor = new Color(0.4f, 0.15f, 0.15f);
                entry.DeleteButton.colors = dc;
                entry.DeleteButton.onClick.AddListener(() => ui.OnSlotDelete(capturedIndex));

                var delLabel = UIFactory.CreateLabel(deleteGo.transform, "Label",
                    "X", 16, FontStyles.Bold, font);
                var dlRect = delLabel.GetComponent<RectTransform>();
                dlRect.anchorMin = Vector2.zero;
                dlRect.anchorMax = Vector2.one;
                dlRect.offsetMin = Vector2.zero;
                dlRect.offsetMax = Vector2.zero;
                delLabel.alignment = TextAlignmentOptions.Center;
                delLabel.color = new Color(1f, 0.7f, 0.7f);

                deleteGo.SetActive(false);

                return entry;
            }
        }
    }
}
