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
    public partial class SaveSlotUI : MonoBehaviour
    {
        public enum Mode { Save, Load }

        private const int SLOT_COUNT = 5;
        private const string METADATA_PREFIX = "#meta:";

        [SerializeField] private GameObject _panelRoot;

        private Mode _currentMode;
        private TextMeshProUGUI _titleLabel;
        private TextMeshProUGUI _closeLabel;
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
                _titleLabel.text = L.Get(mode == Mode.Save
                    ? "ui.pause_menu.save_game" : "ui.pause_menu.load_game");
            if (_closeLabel != null)
                _closeLabel.text = L.Get("ui.general.close");
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
            var btn = UIFactory.CreateButton(parent, L.Get("ui.general.close"), font,
                new Color(0.35f, 0.3f, 0.3f, 0.9f), ui.Hide,
                new Vector2(0f, 38f), 16f);
            ui._closeLabel = btn.GetComponentInChildren<TextMeshProUGUI>();

            var rect = btn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.35f, 0f);
            rect.anchorMax = new Vector2(0.65f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 12f);
        }
    }
}
