using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Procedurally drawn pixel-art sprites for UI icons (no asset files).
    /// 24×24, point-filtered. All original art — approximates the S7 icon
    /// style (small building tiles, house/shield/crown tabs).
    /// </summary>
    public static class IconFactory
    {
        private static readonly Dictionary<string, Sprite> _cache = new();

        // --- Tab icons ---

        public static Sprite HouseTab()  => Cached("tab_house", () =>
            DrawHouse(new Color(0.78f, 0.68f, 0.50f), new Color(0.62f, 0.24f, 0.18f)));

        public static Sprite ShieldTab() => Cached("tab_shield", DrawShield);

        public static Sprite CrownTab()  => Cached("tab_crown", DrawCrown);

        /// <summary>Building tile icon — house silhouette in per-type colors.</summary>
        public static Sprite Building(BaseBuildingType type) =>
            Cached("bld_" + type, () => type switch
            {
                BaseBuildingType.Lodge => DrawHouse(
                    new Color(0.45f, 0.32f, 0.20f), new Color(0.30f, 0.42f, 0.22f)),
                BaseBuildingType.Farm => DrawHouse(
                    new Color(0.72f, 0.62f, 0.42f), new Color(0.75f, 0.58f, 0.25f)),
                BaseBuildingType.MountainShelter => DrawHouse(
                    new Color(0.52f, 0.52f, 0.52f), new Color(0.38f, 0.38f, 0.42f)),
                BaseBuildingType.Residence => DrawHouse(
                    new Color(0.82f, 0.76f, 0.62f), new Color(0.62f, 0.24f, 0.18f)),
                BaseBuildingType.NobleResidence => DrawHouse(
                    new Color(0.90f, 0.88f, 0.80f), new Color(0.80f, 0.62f, 0.20f)),
                _ => DrawHouse(Color.gray, Color.gray)
            });

        /// <summary>Castle icon for the trade map capital node (§14.7).</summary>
        public static Sprite Castle() => Cached("map_castle", DrawCastle);

        /// <summary>Treasure chest icon for special trade outposts (§14.7).</summary>
        public static Sprite Chest() => Cached("map_chest", DrawChest);

        // --- Drawing ---

        private static Sprite Cached(string key, System.Func<Sprite> draw)
        {
            if (_cache.TryGetValue(key, out var s) && s != null) return s;
            var sprite = draw();
            _cache[key] = sprite;
            return sprite;
        }

        private static Texture2D NewTex()
        {
            var tex = new Texture2D(24, 24, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < 24; y++)
                for (int x = 0; x < 24; x++)
                    tex.SetPixel(x, y, clear);
            return tex;
        }

        private static Sprite Finish(Texture2D tex)
        {
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 24, 24),
                new Vector2(0.5f, 0.5f), 24f);
        }

        /// <summary>Simple house: wall block + triangular roof + door.</summary>
        private static Sprite DrawHouse(Color wall, Color roof)
        {
            var tex = NewTex();
            for (int y = 3; y <= 11; y++)             // walls
                for (int x = 5; x <= 18; x++)
                    tex.SetPixel(x, y, wall);
            for (int y = 12; y <= 19; y++)            // roof (narrowing)
            {
                int inset = y - 12;
                for (int x = 4 + inset; x <= 19 - inset; x++)
                    tex.SetPixel(x, y, roof);
            }
            var door = new Color(0.25f, 0.17f, 0.10f);
            for (int y = 3; y <= 8; y++)              // door
                for (int x = 10; x <= 13; x++)
                    tex.SetPixel(x, y, door);
            return Finish(tex);
        }

        /// <summary>Heater shield silhouette.</summary>
        private static Sprite DrawShield()
        {
            var tex = NewTex();
            var steel = new Color(0.62f, 0.64f, 0.68f);
            var trim  = new Color(0.45f, 0.38f, 0.22f);
            for (int y = 2; y <= 20; y++)
            {
                // Width shrinks toward the bottom point
                int half = y >= 12 ? 8 : 2 + (y - 2);
                for (int x = 12 - half; x <= 11 + half; x++)
                {
                    bool edge = x == 12 - half || x == 11 + half || y == 20;
                    tex.SetPixel(x, y, edge ? trim : steel);
                }
            }
            return Finish(tex);
        }

        /// <summary>Keep with two flanking towers and battlements.</summary>
        private static Sprite DrawCastle()
        {
            var tex = NewTex();
            var stone = new Color(0.55f, 0.52f, 0.48f);
            var dark  = new Color(0.35f, 0.32f, 0.30f);
            for (int y = 2; y <= 14; y++)              // towers
                for (int x = 3; x <= 7; x++)
                { tex.SetPixel(x, y, stone); tex.SetPixel(x + 13, y, stone); }
            for (int y = 2; y <= 10; y++)              // keep between towers
                for (int x = 8; x <= 15; x++)
                    tex.SetPixel(x, y, stone);
            for (int x = 3; x <= 20; x += 2)           // battlement merlons
            { tex.SetPixel(x, 15, stone); tex.SetPixel(x, 16, stone); }
            for (int y = 2; y <= 6; y++)               // gate
                for (int x = 10; x <= 13; x++)
                    tex.SetPixel(x, y, dark);
            var flag = new Color(0.75f, 0.20f, 0.18f); // red flag on left tower
            tex.SetPixel(5, 17, stone); tex.SetPixel(5, 18, stone);
            for (int y = 18; y <= 20; y++)
                for (int x = 6; x <= 9; x++)
                    tex.SetPixel(x, y, flag);
            return Finish(tex);
        }

        /// <summary>Chest with gold coins spilling over (§14.7 treasure node).</summary>
        private static Sprite DrawChest()
        {
            var tex = NewTex();
            var wood = new Color(0.48f, 0.32f, 0.16f);
            var band = new Color(0.30f, 0.28f, 0.26f);
            var gold = new Color(0.92f, 0.76f, 0.25f);
            for (int y = 3; y <= 10; y++)              // body
                for (int x = 4; x <= 19; x++)
                    tex.SetPixel(x, y, x == 11 || x == 12 ? band : wood);
            for (int y = 11; y <= 13; y++)             // curved lid
                for (int x = 5; x <= 18; x++)
                    tex.SetPixel(x, y, wood);
            int[,] coins = { { 8, 15 }, { 12, 16 }, { 16, 15 }, { 10, 18 }, { 14, 18 } };
            for (int i = 0; i < coins.GetLength(0); i++)
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) <= 1)
                            tex.SetPixel(coins[i, 0] + dx, coins[i, 1] + dy, gold);
            return Finish(tex);
        }

        /// <summary>Three-point crown with band.</summary>
        private static Sprite DrawCrown()
        {
            var tex = NewTex();
            var gold = new Color(0.92f, 0.76f, 0.25f);
            for (int y = 5; y <= 9; y++)              // band
                for (int x = 4; x <= 19; x++)
                    tex.SetPixel(x, y, gold);
            int[] peaks = { 6, 12, 18 };              // spikes
            foreach (int px in peaks)
            {
                for (int y = 10; y <= 17; y++)
                {
                    int half = (17 - y) / 3;
                    for (int x = px - half; x <= px + half; x++)
                        if (x >= 4 && x <= 19) tex.SetPixel(x, y, gold);
                }
            }
            return Finish(tex);
        }
    }
}
