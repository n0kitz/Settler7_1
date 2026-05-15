using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Campaign mission selection screen.
    /// Lists all unlocked missions grouped by chapter.
    /// </summary>
    public class CampaignSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _listContainer;

        public event Action<Mission> OnMissionSelected;
        public event Action OnBack;

        private CampaignProgress _progress;
        private TMP_FontAsset _font;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public void Show(CampaignProgress progress)
        {
            _progress = progress;
            _panelRoot?.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            _panelRoot?.SetActive(false);
        }

        private void Refresh()
        {
            if (_listContainer == null) return;
            foreach (Transform child in _listContainer) Destroy(child.gameObject);

            var unlocked = CampaignSystem.GetUnlocked(_progress);
            int lastChapter = -1;

            foreach (var mission in CampaignSystem.AllMissions)
            {
                bool isUnlocked = unlocked.Contains(mission);
                bool isComplete = _progress.IsCompleted(mission.Id);

                if (mission.Chapter != lastChapter)
                {
                    lastChapter = mission.Chapter;
                    var chapterLabel = UIFactory.CreateLabel(_listContainer, $"Chapter{lastChapter}",
                        $"— Chapter {lastChapter + 1} —", 15f, FontStyles.Bold, _font);
                    chapterLabel.color = UIColors.TEXT_HEADER_GOLD;
                    chapterLabel.alignment = TextAlignmentOptions.Center;
                    var le = chapterLabel.gameObject.AddComponent<LayoutElement>();
                    le.preferredHeight = 28f;
                }

                string statusSuffix = isComplete ? " ✓" : (!isUnlocked ? " 🔒" : "");
                var btn = UIFactory.CreateButton(_listContainer, mission.Title + statusSuffix, _font,
                    isUnlocked ? UIColors.BUTTON_BLUE : new Color(0.25f, 0.25f, 0.25f),
                    null, new Vector2(0f, 44f), 17f);

                if (isUnlocked)
                {
                    var captured = mission;
                    btn.onClick.AddListener(() => OnMissionSelected?.Invoke(captured));
                }
                else
                {
                    btn.interactable = false;
                }
            }
        }

        private void OnBackClicked() => OnBack?.Invoke();

        /// <summary>Create the campaign selection UI programmatically.</summary>
        public static CampaignSelectionUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var root = new GameObject("CampaignSelectionUI");
            root.transform.SetParent(canvasTransform, false);

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.06f, 0.08f, 0.97f);

            var mainLayout = root.AddComponent<VerticalLayoutGroup>();
            mainLayout.padding = new RectOffset(60, 60, 40, 30);
            mainLayout.spacing = 16f;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;

            var title = UIFactory.CreateLabel(root.transform, "Title",
                "Campaign — Paths to a Kingdom", 30f, FontStyles.Bold, font);
            title.color = UIColors.TEXT_HEADER_GOLD;
            title.alignment = TextAlignmentOptions.Center;
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;

            // Scroll view for mission list
            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(root.transform, false);
            var scrollLE = scrollGo.AddComponent<LayoutElement>();
            scrollLE.flexibleHeight = 1f;
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            var scrollImage = scrollGo.AddComponent<Image>();
            scrollImage.color = new Color(0.07f, 0.09f, 0.11f, 0.8f);

            var content = new GameObject("Content");
            content.transform.SetParent(scrollGo.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(20, 20, 10, 10);
            contentLayout.spacing = 8f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            UIFactory.CreateButton(root.transform, "← Back", font,
                new Color(0.3f, 0.3f, 0.3f), null, new Vector2(140f, 40f), 16f)
                .GetComponent<LayoutElement>().preferredHeight = 40f;

            var ui = root.AddComponent<CampaignSelectionUI>();
            UIFactory.SetField(ui, "_panelRoot", root);
            UIFactory.SetField(ui, "_listContainer", content.transform);
            UIFactory.SetField(ui, "_font", font);

            // Wire back button (last button created)
            var backBtn = root.GetComponentsInChildren<Button>()[0];
            backBtn.onClick.AddListener(ui.OnBackClicked);

            root.SetActive(false);
            return ui;
        }
    }
}
