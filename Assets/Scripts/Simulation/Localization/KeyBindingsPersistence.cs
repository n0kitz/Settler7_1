using System;
using System.IO;

namespace Settlers.Simulation
{
    /// <summary>
    /// Saves and loads KeyBindings as a simple key=value text file.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class KeyBindingsPersistence
    {
        private static readonly string FILE_NAME = "keybinds.ini";

        public static void Save(KeyBindings bindings)
        {
            try
            {
                var lines = new System.Collections.Generic.List<string>();
                foreach (var kv in bindings.All)
                    lines.Add($"{kv.Key}={kv.Value}");
                File.WriteAllLines(GetPath(), lines);
            }
            catch (Exception) { }
        }

        public static KeyBindings Load()
        {
            var kb = new KeyBindings();
            try
            {
                string path = GetPath();
                if (!File.Exists(path)) return kb;

                foreach (var raw in File.ReadAllLines(path))
                {
                    var line = raw.Trim();
                    int sep = line.IndexOf('=');
                    if (sep < 0) continue;
                    string action = line.Substring(0, sep).Trim();
                    string key    = line.Substring(sep + 1).Trim();
                    if (!string.IsNullOrEmpty(action))
                        kb.Set(action, key);
                }
            }
            catch (Exception) { }
            return kb;
        }

        private static string GetPath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Settlers7");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, FILE_NAME);
        }
    }
}
