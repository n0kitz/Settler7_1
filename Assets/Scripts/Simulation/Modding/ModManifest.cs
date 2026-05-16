namespace Settlers.Simulation
{
    /// <summary>
    /// Metadata parsed from a mod's manifest.ini file.
    /// A mod lives in ~/AppData/Roaming/Settlers7/Mods/<ModId>/manifest.ini
    /// </summary>
    public sealed class ModManifest
    {
        public string ModId      { get; set; } = "";
        public string Name       { get; set; } = "";
        public string Author     { get; set; } = "";
        public string Version    { get; set; } = "1.0";
        public string Description { get; set; } = "";
        public bool   Enabled    { get; set; } = true;

        /// <summary>Root folder on disk where this mod's files live.</summary>
        public string RootPath   { get; set; } = "";

        public override string ToString() => $"{Name} v{Version} by {Author}";

        /// <summary>Parse a manifest.ini from key=value lines.</summary>
        public static ModManifest Parse(string rootPath, string[] lines)
        {
            var m = new ModManifest { RootPath = rootPath };
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                int sep = line.IndexOf('=');
                if (sep < 0) continue;
                string key = line.Substring(0, sep).Trim();
                string val = line.Substring(sep + 1).Trim();
                switch (key)
                {
                    case "ModId":       m.ModId       = val; break;
                    case "Name":        m.Name        = val; break;
                    case "Author":      m.Author      = val; break;
                    case "Version":     m.Version     = val; break;
                    case "Description": m.Description = val; break;
                    case "Enabled":
                        m.Enabled = !string.Equals(val, "false",
                            System.StringComparison.OrdinalIgnoreCase);
                        break;
                }
            }
            if (string.IsNullOrEmpty(m.ModId))
                m.ModId = System.IO.Path.GetFileName(rootPath);
            return m;
        }
    }
}
