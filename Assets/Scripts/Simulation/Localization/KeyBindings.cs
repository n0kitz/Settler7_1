using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Action-to-key mapping for rebindable controls.
    /// Stored as string key-code names (e.g., "Q", "Escape", "F1").
    /// Pure C# — presentation layer reads this to configure Input.
    /// </summary>
    public sealed class KeyBindings
    {
        public static readonly KeyBindings Default = new KeyBindings();

        private readonly Dictionary<string, string> _bindings =
            new Dictionary<string, string>
            {
                { "toggle_quest",      "Q" },
                { "toggle_tech",       "T" },
                { "toggle_trade",      "R" },
                { "toggle_army",       "M" },
                { "toggle_tavern",     "V" },
                { "toggle_prestige",   "P" },
                { "toggle_diplomacy",  "J" },
                { "toggle_achievements","K" },
                { "toggle_pause",      "Escape" },
                { "camera_pan_up",     "W" },
                { "camera_pan_down",   "S" },
                { "camera_pan_left",   "A" },
                { "camera_pan_right",  "D" },
                { "camera_zoom_in",    "E" },
                { "camera_zoom_out",   "Q" },
            };

        /// <summary>Returns the key name for an action (e.g., "Q"), or null if unbound.</summary>
        public string Get(string action) =>
            _bindings.TryGetValue(action, out var key) ? key : null;

        /// <summary>Rebind an action to a new key name. Empty string = unbound.</summary>
        public void Set(string action, string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                _bindings.Remove(action);
            else
                _bindings[action] = keyName;
        }

        /// <summary>Reset a single action to its default binding.</summary>
        public void ResetAction(string action)
        {
            if (Default._bindings.TryGetValue(action, out var def))
                _bindings[action] = def;
            else
                _bindings.Remove(action);
        }

        /// <summary>Reset all bindings to defaults.</summary>
        public void ResetAll()
        {
            _bindings.Clear();
            foreach (var kv in Default._bindings)
                _bindings[kv.Key] = kv.Value;
        }

        /// <summary>Returns a snapshot of all bindings.</summary>
        public IReadOnlyDictionary<string, string> All => _bindings;
    }
}
