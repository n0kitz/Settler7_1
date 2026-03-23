using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Minimap showing sector ownership at a glance.
    /// Renders as a Canvas overlay with colored dots per sector.
    /// Clicking a dot focuses the camera on that sector.
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] private RectTransform _mapRoot;
        [SerializeField] private float _mapScale = 2f;

        private Image[] _sectorDots;
        private TextMeshProUGUI[] _sectorLabels;
        private bool _initialized;

        private static readonly Color[] PLAYER_COLORS = {
            new Color(0.2f, 0.5f, 0.9f),
            new Color(0.9f, 0.3f, 0.2f),
            new Color(0.2f, 0.8f, 0.3f),
            new Color(0.9f, 0.8f, 0.2f)
        };
        private static readonly Color NEUTRAL_COLOR = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        private static readonly Color UNOWNED_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        private void LateUpdate()
        {
            if (!_initialized) TryInitialize();
            if (!_initialized) return;
            Refresh();
        }

        private void TryInitialize()
        {
            var gc = GameController.Instance;
            if (gc == null || gc.Graph == null || _mapRoot == null) return;

            int count = gc.Graph.SectorCount;
            _sectorDots = new Image[count];
            _sectorLabels = new TextMeshProUGUI[count];

            for (int i = 0; i < count; i++)
            {
                var sector = gc.Graph.GetSector(i);
                var worldPos = gc.GetSectorPosition(i);

                // Create dot
                var dotGo = new GameObject($"Dot_{i}");
                dotGo.transform.SetParent(_mapRoot, false);

                var dotRect = dotGo.AddComponent<RectTransform>();
                dotRect.sizeDelta = new Vector2(16f, 16f);
                dotRect.anchoredPosition = new Vector2(worldPos.x * _mapScale, worldPos.z * _mapScale);

                var dotImg = dotGo.AddComponent<Image>();
                _sectorDots[i] = dotImg;

                // Click handler
                var btn = dotGo.AddComponent<Button>();
                int capturedId = i;
                btn.onClick.AddListener(() => OnDotClicked(capturedId));

                // Label
                var labelGo = new GameObject($"Label_{i}");
                labelGo.transform.SetParent(dotGo.transform, false);

                var labelRect = labelGo.AddComponent<RectTransform>();
                labelRect.sizeDelta = new Vector2(60f, 14f);
                labelRect.anchoredPosition = new Vector2(0f, -12f);

                var tmp = labelGo.AddComponent<TextMeshProUGUI>();
                tmp.text = sector.Name;
                tmp.fontSize = 7;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.overflowMode = TextOverflowModes.Truncate;
                var font = UI.UIFactory.GetDefaultFont();
                if (font != null) tmp.font = font;
                _sectorLabels[i] = tmp;
            }

            _initialized = true;
        }

        private void Refresh()
        {
            var gc = GameController.Instance;
            if (gc?.Graph == null) return;

            for (int i = 0; i < _sectorDots.Length; i++)
            {
                var sector = gc.Graph.GetSector(i);
                _sectorDots[i].color = GetSectorColor(sector);
            }
        }

        private Color GetSectorColor(Sector sector)
        {
            if (sector.IsPlayerOwned && sector.OwnerId < PLAYER_COLORS.Length)
                return PLAYER_COLORS[sector.OwnerId];
            if (sector.IsNeutral) return NEUTRAL_COLOR;
            return UNOWNED_COLOR;
        }

        private void OnDotClicked(int sectorId)
        {
            var gc = GameController.Instance;
            if (gc == null) return;

            var pos = gc.GetSectorPosition(sectorId);
            var cam = FindAnyObjectByType<SettlerCamera>();
            cam?.FocusOn(pos);
        }

        /// <summary>Create the minimap UI programmatically.</summary>
        public static MinimapController Create(Transform canvasTransform)
        {
            var panelGo = new GameObject("Minimap");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(10f, -10f);
            panelRect.sizeDelta = new Vector2(180f, 160f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.08f, 0.8f);

            // Map content area (centered in panel)
            var mapRoot = new GameObject("MapRoot");
            mapRoot.transform.SetParent(panelGo.transform, false);
            var mapRect = mapRoot.AddComponent<RectTransform>();
            mapRect.anchorMin = Vector2.zero;
            mapRect.anchorMax = Vector2.one;
            mapRect.offsetMin = new Vector2(10f, 10f);
            mapRect.offsetMax = new Vector2(-10f, -10f);

            var controller = panelGo.AddComponent<MinimapController>();
            var field = typeof(MinimapController).GetField("_mapRoot",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(controller, mapRect);

            return controller;
        }
    }
}
