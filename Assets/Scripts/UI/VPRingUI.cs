using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Victory point badge strip (§14.2) below the HUD bar.
    /// One badge per VP: green = held by player, red = held by an enemy,
    /// gray = unheld. Variable permanent VPs (special sectors / trade
    /// outposts) are discovered at runtime and appended.
    /// </summary>
    public class VPRingUI : MonoBehaviour
    {
        private struct Badge
        {
            public Image Dot;
            public TextMeshProUGUI Label;
            public string VpId;
        }

        private readonly List<Badge> _badges = new();
        private readonly HashSet<string> _knownIds = new();
        private Transform _row;
        private TMP_FontAsset _font;
        private float _refreshTimer;

        private static readonly Color HELD_BY_PLAYER = new(0.3f, 0.8f, 0.35f);
        private static readonly Color HELD_BY_ENEMY  = new(0.85f, 0.25f, 0.25f);
        private static readonly Color UNHELD         = new(0.5f, 0.5f, 0.5f, 0.6f);

        /// <summary>Fixed VPs always shown, §14.2 order (tech → military → trade → economy).</summary>
        private static readonly string[] FIXED_VPS =
        {
            "vp_genius", "vp_fountain", "vp_generalissimo", "vp_sun_king",
            "vp_trading_company", "vp_banker", "vp_emperor", "vp_metropolis",
            "vp_field_marshal", "vp_pacifist", "vp_economist"
        };

        private void Update()
        {
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer > 0f) return;
            _refreshTimer = 0.5f;
            Refresh();
        }

        private void Refresh()
        {
            var gc = Presentation.GameController.Instance;
            var state = gc?.State;
            if (state == null) return;

            // Discover variable permanent VPs (special sectors, trade outposts)
            for (int p = 0; p < state.PlayerCount; p++)
            {
                foreach (var vpId in state.Victory.GetAllVPs(p))
                {
                    if (!_knownIds.Contains(vpId))
                        AddBadge(vpId);
                }
            }

            foreach (var badge in _badges)
            {
                int holder = -1;
                for (int p = 0; p < state.PlayerCount; p++)
                {
                    if (state.Victory.HasVP(p, badge.VpId)) { holder = p; break; }
                }

                Color c = holder < 0 ? UNHELD
                    : holder == 0 ? HELD_BY_PLAYER : HELD_BY_ENEMY;
                badge.Dot.color = c;
                badge.Label.color = holder < 0 ? UIColors.TEXT_GRAY_DIM : c;
                // Re-resolve so a language switch takes effect live
                badge.Label.text = DisplayName(badge.VpId);
            }
        }

        private void AddBadge(string vpId)
        {
            _knownIds.Add(vpId);

            var go = new GameObject($"VP_{vpId}");
            go.transform.SetParent(_row, false);
            go.AddComponent<RectTransform>();

            var rowLayout = go.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 3f;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.padding = new RectOffset(4, 6, 2, 2);

            var bg = go.AddComponent<Image>();
            bg.color = UIColors.PANEL_GRAY_MEDIUM;

            var dotGo = new GameObject("Dot");
            dotGo.transform.SetParent(go.transform, false);
            var dotRect = dotGo.AddComponent<RectTransform>();
            dotRect.sizeDelta = new Vector2(10f, 10f);
            var dotElem = dotGo.AddComponent<LayoutElement>();
            dotElem.preferredWidth = 10f;
            dotElem.preferredHeight = 10f;
            var dot = dotGo.AddComponent<Image>();
            dot.color = UNHELD;

            var label = UIFactory.CreateLabel(go.transform, "Label",
                DisplayName(vpId), 10, FontStyles.Normal, _font);
            label.color = UIColors.TEXT_GRAY_DIM;

            _badges.Add(new Badge { Dot = dot, Label = label, VpId = vpId });
        }

        /// <summary>Localized VP name; variable ids map to their category name.</summary>
        private static string DisplayName(string vpId)
        {
            if (vpId.StartsWith("vp_special_outpost_"))
                return L.Get("ui.vp.special_outpost");

            string key = $"ui.vp.{vpId}";
            if (L.Has(key)) return L.Get(key);

            // Unknown permanent VP (special sector reward etc.)
            return L.Get("ui.vp.special_sector");
        }

        /// <summary>Create the VP strip below the HUD (programmatic bootstrap).</summary>
        public static VPRingUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var rootGo = new GameObject("VPRing");
            rootGo.transform.SetParent(canvasTransform, false);

            var rect = rootGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -164f);
            rect.sizeDelta = new Vector2(1100f, 22f);

            var layout = rootGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var ring = rootGo.AddComponent<VPRingUI>();
            ring._row = rootGo.transform;
            ring._font = font;

            foreach (var vpId in FIXED_VPS)
                ring.AddBadge(vpId);

            return ring;
        }
    }
}
