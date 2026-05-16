using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settlers.UI
{
    /// <summary>
    /// Fading toast shown at the top of the screen when an achievement is unlocked.
    /// Auto-hides after a configurable duration.
    /// </summary>
    public class AchievementToast : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private const float SHOW_DURATION = 3.5f;
        private const float FADE_DURATION  = 0.4f;

        public void Show(string achievementName)
        {
            if (_panelRoot == null) return;
            StopAllCoroutines();
            if (_titleText != null) _titleText.text = "Achievement Unlocked!";
            if (_nameText  != null) _nameText.text  = achievementName;
            _panelRoot.SetActive(true);
            StartCoroutine(AutoHide());
        }

        private IEnumerator AutoHide()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(SHOW_DURATION);

            float elapsed = 0f;
            while (elapsed < FADE_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                if (_canvasGroup != null)
                    _canvasGroup.alpha = 1f - (elapsed / FADE_DURATION);
                yield return null;
            }
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        public static AchievementToast Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("AchievementToast");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.3f, 1f);
            panelRect.anchorMax = new Vector2(0.7f, 1f);
            panelRect.pivot     = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -10f);
            panelRect.sizeDelta = new Vector2(0f, 70f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.35f, 0.1f, 0.93f);

            var cg = panelGo.AddComponent<CanvasGroup>();

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 6, 6);
            layout.spacing = 2f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            var titleTmp = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Achievement Unlocked!", 13f, FontStyles.Bold, font);
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.9f, 0.85f, 0.4f);

            var nameTmp = UIFactory.CreateLabel(panelGo.transform, "Name",
                "", 18f, FontStyles.Bold, font);
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = Color.white;

            var toast = panelGo.AddComponent<AchievementToast>();
            UIFactory.SetField(toast, "_panelRoot", panelGo);
            UIFactory.SetField(toast, "_titleText",  titleTmp);
            UIFactory.SetField(toast, "_nameText",   nameTmp);
            UIFactory.SetField(toast, "_canvasGroup", cg);

            panelGo.SetActive(false);
            return toast;
        }
    }
}
