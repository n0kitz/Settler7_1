namespace Settlers.Simulation
{
    /// <summary>
    /// Pure-data model for all player-configurable settings.
    /// Applied to AudioManager and Unity QualitySettings at load and on Apply.
    /// </summary>
    public sealed class SettingsState
    {
        public float MusicVolume  = 0.3f;   // 0-1
        public float SfxVolume   = 0.6f;   // 0-1
        public bool  MasterMute  = false;
        public int   GraphicsQuality = 2;  // 0=Low 1=Medium 2=High 3=Ultra
        public bool  Fullscreen  = false;

        /// <summary>Returns a copy of this state.</summary>
        public SettingsState Clone()
        {
            return new SettingsState
            {
                MusicVolume    = MusicVolume,
                SfxVolume      = SfxVolume,
                MasterMute     = MasterMute,
                GraphicsQuality = GraphicsQuality,
                Fullscreen     = Fullscreen,
            };
        }

        public static SettingsState Default => new SettingsState();
    }
}
