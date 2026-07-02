using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Terrain look per sector (§14.10): classifies a sector's ground from its
    /// simulation resource nodes and provides cached procedural ground textures.
    /// Fertile land reads as warm grass, infertile as sandy red-tan, deposits as
    /// rocky ground — ownership is shown by walls and border rings, not ground tint.
    /// </summary>
    public static class TerrainStyle
    {
        public enum GroundKind { Fertile, Forest, Rocky, Sand }

        private const int TEXTURE_SIZE = 64;
        private const float NOISE_SCALE = 5.5f;
        private const float SPECKLE_STRENGTH = 0.06f;
        private const float UV_TILING = 3f;

        // Two-tone palette per ground kind — noise blends between A and B
        // Deliberately darker than target look — sun + ambient + fog brighten in scene
        private static readonly Color FERTILE_A = new(0.28f, 0.46f, 0.15f);
        private static readonly Color FERTILE_B = new(0.38f, 0.55f, 0.20f);
        private static readonly Color FOREST_A  = new(0.20f, 0.35f, 0.13f);
        private static readonly Color FOREST_B  = new(0.28f, 0.43f, 0.17f);
        private static readonly Color ROCKY_A   = new(0.38f, 0.34f, 0.29f);
        private static readonly Color ROCKY_B   = new(0.48f, 0.43f, 0.36f);
        private static readonly Color SAND_A    = new(0.58f, 0.41f, 0.24f);
        private static readonly Color SAND_B    = new(0.68f, 0.51f, 0.32f);

        private static readonly Dictionary<GroundKind, Texture2D> _textureCache = new();

        /// <summary>Ground look for a sector, by §14.10 priority: fertile grass
        /// over forest floor over rocky ground; everything else is infertile sand.</summary>
        public static GroundKind Classify(Sector sector)
        {
            if (sector.HasResource(ResourceNodeType.FertileLand)) return GroundKind.Fertile;
            if (sector.HasResource(ResourceNodeType.Forest)) return GroundKind.Forest;
            if (sector.HasResource(ResourceNodeType.Stone) ||
                sector.HasResource(ResourceNodeType.Coal) ||
                sector.HasResource(ResourceNodeType.Iron) ||
                sector.HasResource(ResourceNodeType.Gold)) return GroundKind.Rocky;
            return GroundKind.Sand;
        }

        /// <summary>Mid-tone base color for a ground kind (used for owner tinting).</summary>
        public static Color BaseColor(GroundKind kind)
        {
            var (a, b) = Palette(kind);
            return Color.Lerp(a, b, 0.5f);
        }

        /// <summary>Cached procedural noise texture for a ground kind.</summary>
        public static Texture2D GetGroundTexture(GroundKind kind)
        {
            if (_textureCache.TryGetValue(kind, out var cached)) return cached;
            var (a, b) = Palette(kind);
            var tex = GenerateNoiseTexture(a, b, (int)kind * 977 + 31);
            tex.name = $"Ground_{kind}";
            _textureCache[kind] = tex;
            return tex;
        }

        /// <summary>Per-sector UV tiling+offset so neighboring sectors don't
        /// repeat the same pattern. Deterministic per sector ID.</summary>
        public static Vector4 UvTilingOffset(int sectorId)
        {
            var rng = new System.Random(sectorId * 7919 + 13);
            return new Vector4(UV_TILING, UV_TILING,
                (float)rng.NextDouble(), (float)rng.NextDouble());
        }

        private static (Color a, Color b) Palette(GroundKind kind) => kind switch
        {
            GroundKind.Fertile => (FERTILE_A, FERTILE_B),
            GroundKind.Forest => (FOREST_A, FOREST_B),
            GroundKind.Rocky => (ROCKY_A, ROCKY_B),
            _ => (SAND_A, SAND_B),
        };

        private static Texture2D GenerateNoiseTexture(Color a, Color b, int seed)
        {
            var tex = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGB24, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };
            var rng = new System.Random(seed);
            float offX = seed * 0.731f, offY = seed * 1.137f;
            var pixels = new Color[TEXTURE_SIZE * TEXTURE_SIZE];
            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    // Two tileable Perlin octaves for soft patches, plus a fine speckle
                    float u = (float)x / TEXTURE_SIZE, v = (float)y / TEXTURE_SIZE;
                    float n = TileableNoise(u, v, NOISE_SCALE, offX, offY) * 0.7f
                            + TileableNoise(u, v, NOISE_SCALE * 3f, offX, offY) * 0.3f;
                    float speckle = ((float)rng.NextDouble() - 0.5f) * 2f * SPECKLE_STRENGTH;
                    var c = Color.Lerp(a, b, Mathf.Clamp01(n + speckle));
                    pixels[y * TEXTURE_SIZE + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(updateMipmaps: true, makeNoLongerReadable: true);
            return tex;
        }

        /// <summary>Perlin sample blended across opposite edges so the
        /// texture wraps seamlessly when tiled.</summary>
        private static float TileableNoise(float u, float v, float scale, float ox, float oy)
        {
            float n00 = Mathf.PerlinNoise(ox + u * scale, oy + v * scale);
            float n10 = Mathf.PerlinNoise(ox + (u - 1f) * scale, oy + v * scale);
            float n01 = Mathf.PerlinNoise(ox + u * scale, oy + (v - 1f) * scale);
            float n11 = Mathf.PerlinNoise(ox + (u - 1f) * scale, oy + (v - 1f) * scale);
            return Mathf.Lerp(Mathf.Lerp(n00, n10, u), Mathf.Lerp(n01, n11, u), v);
        }
    }
}
