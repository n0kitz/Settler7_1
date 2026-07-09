using UnityEngine;
using UnityEngine.UI;

namespace Settlers.UI
{
    /// <summary>
    /// Procedurally drawn art for full-screen panels: parchment + compass +
    /// dotted routes for the trade map (§14.7), dark stone + candlelight for
    /// the tech tree (§14.6). All original art, cached — no asset files.
    /// </summary>
    public static class MapArtFactory
    {
        private static Sprite _parchment;
        private static Sprite _compass;
        private static Sprite _stone;
        private static Sprite _glow;
        private static Sprite _disc;

        private static readonly Color INK = new(0.35f, 0.25f, 0.14f, 0.85f);

        /// <summary>Warm mottled parchment, 128×128, edge-darkened.</summary>
        public static Sprite Parchment()
        {
            if (_parchment != null) return _parchment;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var baseColor = new Color(0.80f, 0.71f, 0.52f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float mottle = Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 0.6f
                                 + Mathf.PerlinNoise(x * 0.17f + 40f, y * 0.17f + 40f) * 0.4f;
                    float shade = 0.86f + mottle * 0.20f;
                    int edgeDist = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    shade *= 0.76f + 0.24f * Mathf.Clamp01(edgeDist / 18f);
                    tex.SetPixel(x, y, new Color(
                        baseColor.r * shade, baseColor.g * shade, baseColor.b * shade, 1f));
                }
            }
            tex.Apply();
            _parchment = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            return _parchment;
        }

        /// <summary>Ink compass rose, 64×64: ring, cross spokes, north point.</summary>
        public static Sprite Compass()
        {
            if (_compass != null) return _compass;

            const int size = 64;
            const int c = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    int dx = Mathf.Abs(x - c);
                    int dy = Mathf.Abs(y - c);

                    bool ring = dist >= 24f && dist <= 26f;
                    bool mainSpokes = (dx <= 1 && dy <= 28) || (dy <= 1 && dx <= 28);
                    bool diagonals = Mathf.Abs(dx - dy) <= 1 && dist <= 19f;
                    bool northTip = y - c >= 22 && dx <= (30 - (y - c));

                    tex.SetPixel(x, y,
                        ring || mainSpokes || diagonals || northTip ? INK : clear);
                }
            }
            tex.Apply();
            _compass = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            return _compass;
        }

        /// <summary>Dark mottled stone wall, 128×128, edge-darkened (§14.6).</summary>
        public static Sprite Stone()
        {
            if (_stone != null) return _stone;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var baseColor = new Color(0.16f, 0.155f, 0.15f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float mottle = Mathf.PerlinNoise(x * 0.05f + 80f, y * 0.05f) * 0.6f
                                 + Mathf.PerlinNoise(x * 0.21f, y * 0.21f + 80f) * 0.4f;
                    // Coarse block seams every 32px
                    bool seam = y % 32 < 2 || (x + (y / 32 % 2) * 16) % 32 < 2;
                    float shade = (seam ? 0.55f : 0.85f) + mottle * 0.35f;
                    int edgeDist = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    shade *= 0.70f + 0.30f * Mathf.Clamp01(edgeDist / 22f);
                    tex.SetPixel(x, y, new Color(
                        baseColor.r * shade, baseColor.g * shade, baseColor.b * shade, 1f));
                }
            }
            tex.Apply();
            _stone = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            return _stone;
        }

        /// <summary>Soft radial warm glow, 64×64, for candlelight (§14.6).</summary>
        public static Sprite Glow()
        {
            if (_glow != null) return _glow;

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                    float a = Mathf.Clamp01(1f - dist / 30f);
                    a *= a; // soft falloff
                    tex.SetPixel(x, y, new Color(1f, 0.82f, 0.45f, a * 0.6f));
                }
            }
            tex.Apply();
            _glow = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            return _glow;
        }

        /// <summary>Filled circle, 16×16 — status gems / wax seals on cards.</summary>
        public static Sprite Disc()
        {
            if (_disc != null) return _disc;

            const int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                    float a = Mathf.Clamp01(7.5f - dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            _disc = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            return _disc;
        }

        /// <summary>
        /// A lit candle with holder and warm glow, anchored to a normalized
        /// position on the parent panel (§14.6 side candle holders).
        /// </summary>
        public static void CreateCandle(Transform parent, Vector2 anchor)
        {
            var glowGo = new GameObject("CandleGlow");
            glowGo.transform.SetParent(parent, false);
            var glowRect = glowGo.AddComponent<RectTransform>();
            glowRect.anchorMin = anchor;
            glowRect.anchorMax = anchor;
            glowRect.anchoredPosition = new Vector2(0f, 26f);
            glowRect.sizeDelta = new Vector2(110f, 110f);
            var glowImg = glowGo.AddComponent<Image>();
            glowImg.sprite = Glow();
            glowImg.raycastTarget = false;

            var waxGo = new GameObject("Candle");
            waxGo.transform.SetParent(parent, false);
            var waxRect = waxGo.AddComponent<RectTransform>();
            waxRect.anchorMin = anchor;
            waxRect.anchorMax = anchor;
            waxRect.anchoredPosition = new Vector2(0f, 10f);
            waxRect.sizeDelta = new Vector2(10f, 34f);
            var waxImg = waxGo.AddComponent<Image>();
            waxImg.color = new Color(0.90f, 0.86f, 0.74f);
            waxImg.raycastTarget = false;

            var flameGo = new GameObject("Flame");
            flameGo.transform.SetParent(parent, false);
            var flameRect = flameGo.AddComponent<RectTransform>();
            flameRect.anchorMin = anchor;
            flameRect.anchorMax = anchor;
            flameRect.anchoredPosition = new Vector2(0f, 32f);
            flameRect.sizeDelta = new Vector2(9f, 13f);
            var flameImg = flameGo.AddComponent<Image>();
            flameImg.sprite = Disc();
            flameImg.color = new Color(1f, 0.72f, 0.20f);
            flameImg.raycastTarget = false;

            var holderGo = new GameObject("Holder");
            holderGo.transform.SetParent(parent, false);
            var holderRect = holderGo.AddComponent<RectTransform>();
            holderRect.anchorMin = anchor;
            holderRect.anchorMax = anchor;
            holderRect.anchoredPosition = new Vector2(0f, -9f);
            holderRect.sizeDelta = new Vector2(26f, 5f);
            var holderImg = holderGo.AddComponent<Image>();
            holderImg.color = new Color(0.45f, 0.36f, 0.16f);
            holderImg.raycastTarget = false;
        }

        /// <summary>
        /// Dotted route between two normalized anchor points (0–1 within the
        /// parent rect) — aspect-independent stand-in for §14.7's dashed lines.
        /// </summary>
        public static void CreateDottedRoute(Transform parent, Vector2 fromAnchor,
            Vector2 toAnchor, Color color, int dots = 14)
        {
            for (int i = 1; i < dots; i++)
            {
                var a = Vector2.Lerp(fromAnchor, toAnchor, i / (float)dots);
                var dotGo = new GameObject("RouteDot");
                dotGo.transform.SetParent(parent, false);
                var rect = dotGo.AddComponent<RectTransform>();
                rect.anchorMin = a;
                rect.anchorMax = a;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(4f, 4f);
                dotGo.AddComponent<Image>().color = color;
            }
        }

        /// <summary>Faint lat/long grid lines over a map rect (§14.7).</summary>
        public static void CreateGrid(Transform parent, int columns, int rows)
        {
            var gridColor = new Color(0.35f, 0.25f, 0.14f, 0.13f);
            for (int i = 1; i < columns; i++)
            {
                float x = i / (float)columns;
                CreateGridLine(parent, new Vector2(x, 0f), new Vector2(x, 1f),
                    new Vector2(1.5f, 0f), gridColor);
            }
            for (int i = 1; i < rows; i++)
            {
                float y = i / (float)rows;
                CreateGridLine(parent, new Vector2(0f, y), new Vector2(1f, y),
                    new Vector2(0f, 1.5f), gridColor);
            }
        }

        private static void CreateGridLine(Transform parent, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 thickness, Color color)
        {
            var lineGo = new GameObject("GridLine");
            lineGo.transform.SetParent(parent, false);
            var rect = lineGo.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = thickness;
            lineGo.AddComponent<Image>().color = color;
        }
    }
}
