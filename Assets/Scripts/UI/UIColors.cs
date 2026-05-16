using UnityEngine;

namespace Settlers.UI
{
    public static class UIColors
    {
        // Panel backgrounds
        public static Color PANEL_DARK        = new(0.08f, 0.08f, 0.08f, 0.85f);
        public static Color PANEL_BLUE_DARK   = new(0.08f, 0.08f, 0.1f,  0.95f);
        public static Color PANEL_GRAY_MEDIUM = new(0.12f, 0.12f, 0.15f, 0.8f);

        // Title / header text
        public static Color TEXT_HEADER_GOLD  = new(0.9f,  0.82f, 0.55f);
        public static Color TEXT_GOLD         = new(0.9f,  0.85f, 0.6f);
        public static Color HIGHLIGHT_GOLD    = new(1f,    0.85f, 0.3f);
        public static Color TEXT_LIGHT        = new(0.85f, 0.85f, 0.85f);

        // Semantic text
        public static Color TEXT_RED_BRIGHT   = new(1f,    0.3f,  0.3f);
        public static Color TEXT_GRAY_DIM     = new(0.6f,  0.6f,  0.6f);
        public static Color TEXT_GREEN_LIGHT  = new(0.6f,  0.9f,  0.7f);

        // Button colors
        public static Color BUTTON_GREEN      = new(0.2f,  0.5f,  0.25f, 0.9f);
        public static Color BUTTON_BLUE       = new(0.25f, 0.35f, 0.5f,  0.9f);
        public static Color BUTTON_RED        = new(0.5f,  0.2f,  0.2f,  0.9f);

        // Column header accent
        public static Color ACCENT_ORANGE     = new(0.9f,  0.8f,  0.4f);

        /// <summary>
        /// Swap to color-blind-safe palette (high-contrast, no red/green reliance).
        /// Call on Settings Apply when ColorBlindMode changes.
        /// </summary>
        public static void SetColorBlindMode(bool enabled)
        {
            if (enabled)
            {
                // Blue/orange palette accessible to deuteranopia/protanopia
                BUTTON_GREEN     = new Color(0.0f, 0.45f, 0.7f, 0.9f);  // blue
                BUTTON_RED       = new Color(0.9f, 0.6f,  0.0f, 0.9f);  // orange
                TEXT_GREEN_LIGHT = new Color(0.35f, 0.7f, 1.0f);         // sky blue
                TEXT_RED_BRIGHT  = new Color(1.0f, 0.65f, 0.0f);         // amber
            }
            else
            {
                BUTTON_GREEN     = new Color(0.2f,  0.5f,  0.25f, 0.9f);
                BUTTON_RED       = new Color(0.5f,  0.2f,  0.2f,  0.9f);
                TEXT_GREEN_LIGHT = new Color(0.6f,  0.9f,  0.7f);
                TEXT_RED_BRIGHT  = new Color(1f,    0.3f,  0.3f);
            }
        }
    }
}
