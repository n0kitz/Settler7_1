using UnityEngine;

namespace Settlers.UI
{
    public static class UIColors
    {
        // Panel backgrounds
        public static readonly Color PANEL_DARK = new(0.08f, 0.08f, 0.08f, 0.85f);
        public static readonly Color PANEL_BLUE_DARK = new(0.08f, 0.08f, 0.1f, 0.95f);
        public static readonly Color PANEL_GRAY_MEDIUM = new(0.12f, 0.12f, 0.15f, 0.8f);

        // Title / header text
        public static readonly Color TEXT_HEADER_GOLD = new(0.9f, 0.82f, 0.55f);
        public static readonly Color TEXT_GOLD = new(0.9f, 0.85f, 0.6f);
        public static readonly Color HIGHLIGHT_GOLD = new(1f, 0.85f, 0.3f);

        // Semantic text
        public static readonly Color TEXT_RED_BRIGHT = new(1f, 0.3f, 0.3f);
        public static readonly Color TEXT_GRAY_DIM = new(0.6f, 0.6f, 0.6f);
        public static readonly Color TEXT_GREEN_LIGHT = new(0.6f, 0.9f, 0.7f);

        // Button colors
        public static readonly Color BUTTON_GREEN = new(0.2f, 0.5f, 0.25f, 0.9f);
        public static readonly Color BUTTON_BLUE = new(0.25f, 0.35f, 0.5f, 0.9f);
        public static readonly Color BUTTON_RED = new(0.5f, 0.2f, 0.2f, 0.9f);

        // Column header accent
        public static readonly Color ACCENT_ORANGE = new(0.9f, 0.8f, 0.4f);
    }
}
