using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Sidebar panel in the map editor that shows and edits the selected sector's properties.
    /// </summary>
    public class SectorPropertyPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _sectorIdText;
        [SerializeField] private TextMeshProUGUI _ownerText;
        [SerializeField] private TextMeshProUGUI _garrisonText;
        [SerializeField] private TextMeshProUGUI _slotsText;

        public event Action<int> OnOwnerChanged;    // new OwnerId
        public event Action<int> OnGarrisonChanged; // delta (+1 / -1)
        public event Action<int> OnSlotsChanged;    // delta (+1 / -1)
        public event Action OnDeleteSector;

        private MapEditorState.EditorSector _selected;

        public void Show() => _panelRoot?.SetActive(true);
        public void Hide() => _panelRoot?.SetActive(false);

        public void Bind(MapEditorState.EditorSector sector)
        {
            _selected = sector;
            Refresh();
            Show();
        }

        public void Refresh()
        {
            if (_selected == null) return;
            if (_sectorIdText != null)
                _sectorIdText.text = $"ID: {_selected.Id}   Name: {_selected.Name}";
            if (_ownerText != null)
                _ownerText.text = _selected.OwnerId < 0
                    ? "Neutral" : $"Player {_selected.OwnerId}";
            if (_garrisonText != null)
                _garrisonText.text = $"{_selected.GarrisonStrength}";
            if (_slotsText != null)
                _slotsText.text = $"{_selected.BuildSlots}";
        }

        private void OnOwnerMinusClicked()
        {
            if (_selected == null) return;
            _selected.OwnerId = _selected.OwnerId <= -2 ? 3 : _selected.OwnerId - 1;
            Refresh();
            OnOwnerChanged?.Invoke(_selected.OwnerId);
        }

        private void OnOwnerPlusClicked()
        {
            if (_selected == null) return;
            _selected.OwnerId = _selected.OwnerId >= 3 ? -2 : _selected.OwnerId + 1;
            Refresh();
            OnOwnerChanged?.Invoke(_selected.OwnerId);
        }

        private void OnGarrisonMinusClicked() { OnGarrisonChanged?.Invoke(-1); Refresh(); }
        private void OnGarrisonPlusClicked() { OnGarrisonChanged?.Invoke(1); Refresh(); }
        private void OnSlotsMinusClicked() { OnSlotsChanged?.Invoke(-1); Refresh(); }
        private void OnSlotsPlusClicked() { OnSlotsChanged?.Invoke(1); Refresh(); }
        private void OnDeleteClicked() => OnDeleteSector?.Invoke();

        public static SectorPropertyPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var root = new GameObject("SectorPropertyPanel");
            root.transform.SetParent(canvasTransform, false);

            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-10f, 0f);
            rect.sizeDelta = new Vector2(220f, 340f);

            root.AddComponent<Image>().color = new Color(0.08f, 0.1f, 0.12f, 0.95f);

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 8f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var header = UIFactory.CreateLabel(root.transform, "Header",
                "Sector Properties", 16f, FontStyles.Bold, font);
            header.color = UIColors.TEXT_HEADER_GOLD;
            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            var idText = UIFactory.CreateLabel(root.transform, "ID", "ID: —", 12f, FontStyles.Normal, font);
            idText.gameObject.AddComponent<LayoutElement>().preferredHeight = 18f;

            var ui = root.AddComponent<SectorPropertyPanel>();
            UIFactory.SetField(ui, "_panelRoot", root);
            UIFactory.SetField(ui, "_headerText", header);
            UIFactory.SetField(ui, "_sectorIdText", idText);

            var ownerText = AddSpinnerRow(root.transform, "Owner", font,
                ui.OnOwnerMinusClicked, ui.OnOwnerPlusClicked);
            UIFactory.SetField(ui, "_ownerText", ownerText);

            var garrisonText = AddSpinnerRow(root.transform, "Garrison", font,
                ui.OnGarrisonMinusClicked, ui.OnGarrisonPlusClicked);
            UIFactory.SetField(ui, "_garrisonText", garrisonText);

            var slotsText = AddSpinnerRow(root.transform, "Build Slots", font,
                ui.OnSlotsMinusClicked, ui.OnSlotsPlusClicked);
            UIFactory.SetField(ui, "_slotsText", slotsText);

            UIFactory.CreateButton(root.transform, "Delete Sector", font,
                UIColors.BUTTON_RED, ui.OnDeleteClicked, new Vector2(196f, 36f), 13f);

            root.SetActive(false);
            return ui;
        }

        private static TextMeshProUGUI AddSpinnerRow(Transform parent, string label,
            TMP_FontAsset font, UnityEngine.Events.UnityAction onMinus,
            UnityEngine.Events.UnityAction onPlus)
        {
            var row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 4f;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childForceExpandWidth = false;
            row.AddComponent<LayoutElement>().preferredHeight = 32f;

            var lbl = UIFactory.CreateLabel(row.transform, "Lbl", label, 13f, FontStyles.Normal, font);
            lbl.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            CreateTinyButton(row.transform, "-", font, onMinus);
            var val = UIFactory.CreateLabel(row.transform, "Val", "—", 14f, FontStyles.Bold, font);
            val.alignment = TextAlignmentOptions.Center;
            val.gameObject.AddComponent<LayoutElement>().preferredWidth = 40f;
            CreateTinyButton(row.transform, "+", font, onPlus);

            return val;
        }

        private static void CreateTinyButton(Transform parent, string label,
            TMP_FontAsset font, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject($"Btn{label}");
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredWidth = 28f;
            go.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);
            var txt = UIFactory.CreateLabel(go.transform, "T", label, 16f, FontStyles.Bold, font);
            var r = txt.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            txt.alignment = TextAlignmentOptions.Center;
        }
    }
}
