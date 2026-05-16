using System;
using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Key-based string lookup with English fallback.
    /// Populated by StringTablePersistence at startup.
    /// Usage: L.Get("ui.main_menu.new_game")
    /// </summary>
    public static class L
    {
        private static Dictionary<string, string> _strings = new Dictionary<string, string>();
        private static string _currentLocale = "en";

        public static string CurrentLocale => _currentLocale;

        /// <summary>Returns the localized string for key, or the key itself if missing.</summary>
        public static string Get(string key)
        {
            return _strings.TryGetValue(key, out var val) ? val : key;
        }

        /// <summary>Returns true if the key exists in the current locale.</summary>
        public static bool Has(string key) => _strings.ContainsKey(key);

        /// <summary>Load strings for the given locale. Falls back to en if locale missing.</summary>
        public static void SetLocale(string locale)
        {
            _currentLocale = locale ?? "en";
            _strings = StringTablePersistence.Load(_currentLocale);

            // Fall back to English if locale loaded nothing
            if (_strings.Count == 0 && _currentLocale != "en")
                _strings = StringTablePersistence.Load("en");
        }

        /// <summary>Seed strings directly (used in tests or for bootstrapping).</summary>
        internal static void Seed(Dictionary<string, string> strings)
        {
            _strings = strings ?? new Dictionary<string, string>();
        }

        /// <summary>Clear all strings (test cleanup).</summary>
        internal static void Clear() => _strings.Clear();
    }
}
