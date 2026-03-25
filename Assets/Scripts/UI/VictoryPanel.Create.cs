using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settlers.UI
{
    public partial class VictoryPanel
    {
        /// <summary>Create the VP panel + game over overlay programmatically.</summary>
        public static VictoryPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            // VP tracker (bottom-right)
            var panelGo = new GameObject("VictoryPanel");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-10f, 10f);
            panelRect.sizeDelta = new Vector2(300f, 50f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = UIColors.PANEL_DARK;

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.spacing = 2f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var vpText = UIFactory.CreateLabel(panelGo.transform, "VPText", "VPs: ...", 13, font);
            vpText.color = new Color(1f, 0.9f, 0.5f);
            vpText.richText = true;

            var countdownText = UIFactory.CreateLabel(panelGo.transform, "CountdownText", "", 14, font);
            countdownText.color = UIColors.TEXT_RED_BRIGHT;
            countdownText.fontStyle = FontStyles.Bold;
            countdownText.gameObject.SetActive(false);

            // Game over overlay (centered, hidden)
            var overlayGo = new GameObject("GameOverOverlay");
            overlayGo.transform.SetParent(canvasTransform, false);

            var overlayRect = overlayGo.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var overlayBg = overlayGo.AddComponent<Image>();
            overlayBg.color = new Color(0f, 0f, 0f, 0.8f);

            var gameOverText = UIFactory.CreateLabel(overlayGo.transform, "GameOverText",
                "GAME OVER", 36, font);
            gameOverText.alignment = TextAlignmentOptions.Center;
            gameOverText.color = UIColors.HIGHLIGHT_GOLD;
            var goRect = gameOverText.GetComponent<RectTransform>();
            goRect.anchorMin = new Vector2(0.2f, 0.68f);
            goRect.anchorMax = new Vector2(0.8f, 0.78f);
            goRect.offsetMin = Vector2.zero;
            goRect.offsetMax = Vector2.zero;

            overlayGo.SetActive(false);

            // Component
            var panel = panelGo.AddComponent<VictoryPanel>();
            UIFactory.SetField(panel, "_vpText", vpText);
            UIFactory.SetField(panel, "_countdownText", countdownText);
            UIFactory.SetField(panel, "_gameOverOverlay", overlayGo);
            UIFactory.SetField(panel, "_gameOverText", gameOverText);
            panel._font = font;

            return panel;
        }

    }
}
