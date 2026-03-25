using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settlers.UI
{
    public partial class SaveSlotUI
    {
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
